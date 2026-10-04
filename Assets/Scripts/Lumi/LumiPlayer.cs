using System.Collections;
using UnityEngine;

namespace LumiAdventure
{
    [RequireComponent(typeof(CharacterController))]
    public class LumiPlayer : MonoBehaviour, ILumiDamageable
    {
        public const int MaxHealth = 15;
        public const int MaxArmor = 15;

        private LumiGame game;
        private CharacterController controller;
        private LumiHitFlash hitFlash;
        private Transform muzzle;
        private GameObject shieldVisual;
        private float verticalVelocity;
        private float speedMultiplier = 1f;
        private float speedUntil;
        private float invisibleUntil;
        private Vector3 lastPosition;
        private float idleTime;
        private bool wasGrounded;
        private LumiInfantryMotion infantryMotion;
        private LumiNarutoSkills skills;
        private Vector3 safeRespawnPoint;
        private float nextFallRecovery;
        private Coroutine invisibilityRoutine;
        private Renderer[] ghostRenderers;
        private Material[][] originalMaterials;
        private readonly RaycastHit[] aimHits=new RaycastHit[64];
        [SerializeField, Min(.1f)] private float movementSpeed = 5.2f;

        public float MovementSpeed=>movementSpeed*speedMultiplier;
        public int BasicAttackDamage=>4;
        public int Health { get; private set; } = MaxHealth;
        public int Armor { get; private set; }
        public bool IsAlive => Health > 0;
        public bool IsInvisible => Time.time < invisibleUntil;
        public bool IsSpeedBoosted => Time.time < speedUntil;
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
            Ray ray=Cursor.lockState==CursorLockMode.Locked || Application.isMobilePlatform
                ?camera.ViewportPointToRay(new Vector3(.5f,.5f,0))
                :camera.ScreenPointToRay(Input.mousePosition);
            return ResolveAimPoint(ray);
        }

        public void Initialize(LumiGame owner)
        {
            game = owner;
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
            Material shield = LumiFactory.Material("Shield", new Color(0.85f, 0.95f, 1f, 0.55f), true, true);
            for (int i = 0; i < 18; i++)
            {
                float angle = i * Mathf.PI * 2f / 18f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 0.72f, 0f, Mathf.Sin(angle) * 0.72f);
                LumiFactory.Primitive("Shield Spark", PrimitiveType.Sphere, shieldVisual.transform, pos, Vector3.one * 0.09f, shield, false);
            }
            shieldVisual.SetActive(false);
        }

        private void Update()
        {
            if (game == null || !game.IsPlaying || !IsAlive) return;
            Move();
            Shoot();
            UpdateBuffs();
            TrackIdle();
            RecoverFromFall();
        }

        private void Move()
        {
            Vector2 input = LumiMobileInput.Move;
            if (!Application.isMobilePlatform)
            {
                input += new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            }
            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 forward = game.CameraRig.FlatForward;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector3 direction = forward * input.y + right * input.x;
            float speed = movementSpeed * speedMultiplier;
            controller.Move(direction * speed * Time.deltaTime);

            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            bool jump = Input.GetKeyDown(KeyCode.Space) || LumiMobileInput.ConsumeJump();
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

        private void Shoot()
        {
            bool wantsShoot = Input.GetMouseButton(0) || LumiMobileInput.ConsumeShoot();
            if(game.IsPointerOverUi())return;
            if(!Application.isMobilePlatform && (!game.CameraRig.MouseLookActive || game.CameraRig.ReacquiredThisFrame))return;
            Vector3 pointer=Input.mousePosition;
            if(!Application.isMobilePlatform && (pointer.x<0 || pointer.x>Screen.width || pointer.y<0 || pointer.y>Screen.height))return;

            Vector3 aimOrigin=transform.position+Vector3.up*1.1f;
            Vector3 target=CurrentAimPoint();
            if(Application.isMobilePlatform)
            {
                LumiEnemy assisted = FindAimAssistTarget();
                target = assisted != null ? assisted.AimPoint : muzzle.position + transform.forward * 30f;
            }
            Vector3 flatAim=target-aimOrigin;flatAim.y=0;
            if(flatAim.sqrMagnitude<.36f)target=aimOrigin+transform.forward*30;
            else transform.rotation=Application.isMobilePlatform?Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(flatAim),720f*Time.deltaTime):Quaternion.LookRotation(flatAim);
            Vector3 direction=(target-muzzle.position).normalized;
            if(direction.sqrMagnitude<.001f)direction=transform.forward;
            if(!wantsShoot)return;
            if(skills!=null)skills.TryBasicAttack();
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
            speedMultiplier = 1.5f;
            speedUntil = Mathf.Max(speedUntil, Time.time + duration);
            game.RefreshHud();
        }

        public void BecomeInvisible(float duration)
        {
            invisibleUntil = Mathf.Max(invisibleUntil, Time.time + duration);
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

        private void UpdateBuffs()
        {
            if (speedMultiplier > 1f && Time.time >= speedUntil)
            {
                speedMultiplier = 1f;
                game.RefreshHud();
            }
        }

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

