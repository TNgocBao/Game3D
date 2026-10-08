using System.Collections;
using UnityEngine;

namespace LumiAdventure
{
    [RequireComponent(typeof(CharacterController))]
    public class LumiPlayer : MonoBehaviour, ILumiDamageable
    {
        public const int MaxHealth = 20;
        public const int MaxArmor = 20;

        private LumiGame game;
        private CharacterController controller;
        private LumiHitFlash hitFlash;
        private Transform muzzle;
        private GameObject shieldVisual;
        private float verticalVelocity;
        private Vector3 lastPosition;
        private float idleTime;
        private bool wasGrounded;
        private LumiInfantryMotion infantryMotion;
        private LumiNarutoSkills skills;
        private LumiPlayerMovementInput movementInput;
        private LumiPlayerCombatInput combatInput;
        private LumiPlayerBuffController buffs;
        private Vector3 safeRespawnPoint;
        private float nextFallRecovery;
        private Coroutine invisibilityRoutine;
        private Renderer[] ghostRenderers;
        private Material[][] originalMaterials;
        private readonly RaycastHit[] aimHits=new RaycastHit[64];
        private bool movementGesture;
        private float movementBasisYaw;
        public Vector3 LastMovementDirection{get;private set;}
        [SerializeField, Min(.1f)] private float movementSpeed = 5.2f;

        public float MovementSpeed=>movementSpeed*buffs.SpeedMultiplier;
        public int BasicAttackDamage=>ScaleOutgoingDamage(4);
        public int Health { get; private set; } = MaxHealth;
        public int Armor { get; private set; } = MaxArmor;
        public bool IsAlive => Health > 0;
        public bool IsInvisible => buffs!=null&&buffs.IsActive(LumiBuffType.Invisibility);
        public bool IsSpeedBoosted => buffs!=null&&buffs.IsActive(LumiBuffType.Speed);
        public LumiPlayerBuffController Buffs=>buffs;
        public float IdleTime => idleTime;
        public Transform Muzzle => muzzle;
        public CharacterController Controller=>controller;
        public Transform ChakraHand=>infantryMotion.ChakraHand;
        public Transform WeaponHand=>infantryMotion!=null?infantryMotion.WeaponHand:null;
        public Vector3 ResolveAimPoint(Ray ray,float range=60)
        {
            int count=Physics.RaycastNonAlloc(ray,aimHits,range,~0,QueryTriggerInteraction.Ignore);
            RaycastHit[] results=aimHits;
            if(count==aimHits.Length){results=Physics.RaycastAll(ray,range,~0,QueryTriggerInteraction.Ignore);count=results.Length;}
            float nearest=float.PositiveInfinity;Collider target=null;Vector3 point=ray.GetPoint(range);
            for(int i=0;i<count;i++)
            {
                if(results[i].collider==null || results[i].collider.transform.IsChildOf(transform) || results[i].distance>=nearest)continue;
                nearest=results[i].distance;target=results[i].collider;point=results[i].point;
            }
            if(target!=null)
            {
                LumiEnemy enemy=target.GetComponentInParent<LumiEnemy>();if(enemy!=null && enemy.IsAlive)return enemy.AimPoint;
                LumiVillageBoss boss=target.GetComponentInParent<LumiVillageBoss>();if(boss!=null && boss.IsAlive)return boss.AimPoint;
            }
            return point;
        }
        public Vector3 CurrentAimPoint()
        {
            Camera camera=game.CameraRig.ViewCamera;
            Ray ray;
            if(combatInput.UsesTouchAim&&game.CameraRig.MovementFacing&&LumiMobileInput.HasAim)
                ray=camera.ScreenPointToRay(LumiMobileInput.AimScreenPoint);
            else if(Cursor.lockState==CursorLockMode.Locked||combatInput.UsesTouchAim)
                ray=camera.ViewportPointToRay(new Vector3(.5f,.5f,0));
            else ray=camera.ScreenPointToRay(Input.mousePosition);
            return ResolveAimPoint(ray);
        }

        public void Initialize(LumiGame owner)
        {
            game = owner;
            buffs=gameObject.AddComponent<LumiPlayerBuffController>();
            controller = GetComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.42f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.stepOffset = 0.35f;
            BuildVisual();
            ghostRenderers = GetComponentsInChildren<Renderer>(true);
            originalMaterials = new Material[ghostRenderers.Length][];
            for (int i=0;i<ghostRenderers.Length;i++) originalMaterials[i] = ghostRenderers[i].sharedMaterials;
            hitFlash = gameObject.AddComponent<LumiHitFlash>();
            gameObject.AddComponent<LumiSpeedWind>().Initialize(this,game);
            skills=gameObject.AddComponent<LumiNarutoSkills>();skills.Initialize(this,game);
            movementInput=gameObject.AddComponent<LumiPlayerMovementInput>();movementInput.Initialize(game);
            combatInput=gameObject.AddComponent<LumiPlayerCombatInput>();combatInput.Initialize(game);
            lastPosition = transform.position;
            safeRespawnPoint = transform.position;
        }

        private void BuildVisual()
        {
            Transform visual=LumiFactory.WorldObject("Naruto Visual",transform,Vector3.zero).transform;
            GameObject character=LumiArt.CreatePlayer(visual);
            infantryMotion=character.GetComponent<LumiInfantryMotion>();infantryMotion.Configure(game,false);infantryMotion.AlignFeetToGround(controller.bounds.min.y);
            muzzle=LumiFactory.WorldObject("Strike aim",visual,new Vector3(0,1.1f,.55f)).transform;
            BuildShield(visual);
        }

        private void BuildShield(Transform visual)
        {
            shieldVisual = LumiFactory.WorldObject("Armor Shield", visual, new Vector3(0f, 1f, 0f));
            shieldVisual.AddComponent<LumiShieldFormation>().Build();
            shieldVisual.SetActive(Armor>0);
        }

        private void Update()
        {
            if (game == null || !game.IsPlaying || !IsAlive) return;
            Move();
            Shoot();
            TrackIdle();
            RecoverFromFall();
        }

        private void Move()
        {
            Vector2 input=movementInput.ReadMovement();

            Vector3 direction=ResolveMovementDirection(input);
            FaceMovement(direction);
            float speed = MovementSpeed;
            controller.Move(direction * speed * Time.deltaTime);

            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            bool jump=movementInput.ConsumeJump();
            if (jump && controller.isGrounded)
            {
                verticalVelocity = 7.2f;
                game.Audio.Play("jump");
            }
            verticalVelocity += -22f * Time.deltaTime;
            controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);

            if (controller.isGrounded && !wasGrounded) game.Audio.Play("land", 0.55f);
            wasGrounded = controller.isGrounded;
            if (infantryMotion != null) infantryMotion.SetGrounded(controller.isGrounded);
        }

        private Vector3 ResolveMovementDirection(Vector2 input)
        {
            if(input.sqrMagnitude<.0025f){movementGesture=false;LastMovementDirection=Vector3.zero;return Vector3.zero;}
            Vector3 direction;
            if(game.CameraRig.MovementFacing)
            {
                if(!movementGesture){movementBasisYaw=transform.eulerAngles.y;movementGesture=true;}
                Quaternion basis=Quaternion.Euler(0,movementBasisYaw,0);
                direction=basis*(Vector3.forward*input.y+Vector3.right*input.x);
            }
            else
            {
                movementGesture=false;
                Vector3 forward=game.CameraRig.FlatForward;
                direction=forward*input.y+Vector3.Cross(Vector3.up,forward)*input.x;
            }
            LastMovementDirection=direction.normalized;
            return Vector3.ClampMagnitude(direction,1);
        }

        private void FaceMovement(Vector3 direction)
        {
            if(!game.CameraRig.MovementFacing||direction.sqrMagnitude<.001f)return;
            direction.y=0f;
            if(direction.sqrMagnitude>.001f)transform.rotation=Quaternion.LookRotation(direction.normalized,Vector3.up);
        }

        private void Shoot()
        {
            // Phones report every touch as mouse button 0. Mobile basic attacks are
            // dispatched directly by the dedicated HUD button instead.
            bool wantsShoot=combatInput.BasicAttackPressed;
            if(game.IsPointerOverUi())return;
            if(game.Controls.UsesPcControls&&!game.CameraRig.MovementFacing&&(!game.CameraRig.MouseLookActive||game.CameraRig.ReacquiredThisFrame))return;
            Vector3 pointer=Input.mousePosition;
            if(game.Controls.UsesPcControls&&(pointer.x<0||pointer.x>Screen.width||pointer.y<0||pointer.y>Screen.height))return;

            Vector3 aimOrigin=transform.position+Vector3.up*1.1f;
            Vector3 target=CurrentAimPoint();
            if(combatInput.UsesTouchAim&&(!game.CameraRig.MovementFacing||!LumiMobileInput.HasAim))
            {
                LumiEnemy assisted = FindAimAssistTarget();
                target = assisted != null ? assisted.AimPoint : muzzle.position + transform.forward * 30f;
            }
            Vector3 flatAim=target-aimOrigin;flatAim.y=0;
            if(flatAim.sqrMagnitude<.36f)target=aimOrigin+transform.forward*30;
            else FaceAim(flatAim);
            Vector3 direction=(target-muzzle.position).normalized;
            if(direction.sqrMagnitude<.001f)direction=transform.forward;
            if(!wantsShoot)return;
            if(skills!=null)skills.TryBasicAttack();
        }

        private void FaceAim(Vector3 flatAim)
        {
            if(game.CameraRig.MovementFacing)return;
            Quaternion facing=Quaternion.LookRotation(flatAim);
            transform.rotation=game.Controls.UsesMobileControls?Quaternion.RotateTowards(transform.rotation,facing,720f*Time.deltaTime):facing;
        }

        private LumiEnemy FindAimAssistTarget()
        {
            Camera camera = game.CameraRig.ViewCamera;
            LumiEnemy best = null;
            float bestScore = 0.16f;
            foreach (LumiEnemy enemy in LumiEnemy.Active)
            {
                if (!enemy.IsAlive) continue;
                Vector3 viewport = camera.WorldToViewportPoint(enemy.AimPoint);
                if (viewport.z <= 0f || viewport.z > 55f) continue;
                float score = Vector2.Distance(new Vector2(viewport.x, viewport.y), Vector2.one * 0.5f);
                score += viewport.z * 0.0008f;
                if (score >= bestScore) continue;
                Vector3 origin = camera.transform.position;
                Vector3 direction = enemy.AimPoint - origin;
                if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, direction.magnitude, ~0, QueryTriggerInteraction.Ignore)
                    && hit.collider.GetComponentInParent<LumiEnemy>() != enemy) continue;
                bestScore = score;
                best = enemy;
            }
            return best;
        }

        public void TakeDamage(int amount, Vector3 hitPoint)
        {
            if (!IsAlive) return;
            amount=Mathf.Max(1,Mathf.CeilToInt(amount*(buffs==null?1f:buffs.DamageTakenMultiplier)));
            int absorbed = Mathf.Min(Armor, amount);
            Armor -= absorbed;
            Health = Mathf.Max(0, Health - (amount - absorbed));
            shieldVisual.SetActive(Armor > 0);
            hitFlash.Flash(new Color(1f, 0.15f, 0.12f));
            if(infantryMotion!=null)infantryMotion.ReactToHit();
            game.Audio.Play("hurt");
            game.SpawnImpact(hitPoint, Color.red);
            game.RefreshHud();
            if (Health <= 0) game.LoseLevel();
        }

        public void Heal(int amount)
        {
            Health = Mathf.Min(MaxHealth, Health + amount);
            game.RefreshHud();
        }

        public void AddArmor(int amount)
        {
            Armor = Mathf.Min(MaxArmor, Armor + amount);
            shieldVisual.SetActive(Armor > 0);
            game.RefreshHud();
        }

        public void BoostSpeed(float duration)
        {
            buffs.Apply(LumiBuffType.Speed,duration,1.5f);
            game.RefreshHud();
        }

        public void BecomeInvisible(float duration)
        {
            buffs.Apply(LumiBuffType.Invisibility,duration,1f);
            if (invisibilityRoutine == null) invisibilityRoutine = StartCoroutine(InvisibilityVisual(duration));
            game.RefreshHud();
        }

        private IEnumerator InvisibilityVisual(float duration)
        {
            Material ghost = LumiFactory.Material("Ghost silhouette", new Color(.55f,.8f,.95f,.4f), false, true);
            for(int i=0;i<ghostRenderers.Length;i++)
                if(!ghostRenderers[i].gameObject.name.Contains("Shield"))ghostRenderers[i].sharedMaterial=ghost;
            while(IsInvisible) yield return null;
            for(int i=0;i<ghostRenderers.Length;i++)
                if(ghostRenderers[i]!=null)ghostRenderers[i].sharedMaterials=originalMaterials[i];
            shieldVisual.SetActive(Armor > 0);
            invisibilityRoutine = null;
            game.RefreshHud();
        }

        public void ApplyNpcBuff(LumiBuffType type,float duration)
        {
            buffs.Apply(type,duration,1.5f);
            game.RefreshHud();
        }

        public int ScaleOutgoingDamage(int baseDamage)=>Mathf.Max(1,Mathf.RoundToInt(baseDamage*(buffs==null?1f:buffs.AttackMultiplier)));

        private void TrackIdle()
        {
            float moved = Vector3.Distance(lastPosition, transform.position);
            idleTime = moved < 0.015f ? idleTime + Time.deltaTime : 0f;
            lastPosition = transform.position;
            game.SetDirectionArrow(idleTime >= 7f);
        }

        private void RecoverFromFall()
        {
            if (transform.position.y >= -6f || Time.time < nextFallRecovery) return;
            nextFallRecovery = Time.time + 1f;
            controller.enabled = false;
            transform.position = safeRespawnPoint;
            controller.enabled = true;
            verticalVelocity = 0f;
            TakeDamage(2, transform.position + Vector3.up);
            game.ShowToast("ĐÃ ĐƯA BẠN VỀ KHU VỰC AN TOÀN  -2 MÁU", new Color(1f, 0.68f, 0.2f));
        }
    }
}

