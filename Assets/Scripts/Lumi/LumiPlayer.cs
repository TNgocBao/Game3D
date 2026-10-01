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
        private readonly LumiShotGate shotGate=new LumiShotGate();
        private float speedMultiplier = 1f;
        private float speedUntil;
        private float invisibleUntil;
        private Vector3 lastPosition;
        private float idleTime;
        private bool wasGrounded;
        private LumiInfantryMotion infantryMotion;
        private LumiWeaponVisual pistol;
        private Vector3 safeRespawnPoint;
        private float nextFallRecovery;
        private Coroutine invisibilityRoutine;
        private Renderer[] ghostRenderers;
        private Material[][] originalMaterials;
        [SerializeField, Min(.01f)] private float shotCooldown = 1f;
        [SerializeField, Min(1)] private int shotDamage = 4;
        [SerializeField, Min(.1f)] private float movementSpeed = 5.2f;

        public int Health { get; private set; } = MaxHealth;
        public int Armor { get; private set; }
        public bool IsAlive => Health > 0;
        public bool IsInvisible => Time.time < invisibleUntil;
        public bool IsSpeedBoosted => Time.time < speedUntil;
        public float IdleTime => idleTime;
        public Transform Muzzle => muzzle;

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
            lastPosition = transform.position;
            safeRespawnPoint = transform.position;
        }

        private void BuildVisual()
        {
            Transform visual = LumiFactory.WorldObject("Nova Visual", transform, Vector3.zero).transform;
            GameObject character = LumiArt.CreatePlayer(visual);
            if (character != null)
            {
                infantryMotion=character.GetComponent<LumiInfantryMotion>();infantryMotion.Configure(game,false);
                GameObject gun = LumiArt.CreateBlaster(infantryMotion.WeaponHand);
                if (gun != null)
                {
                    gun.transform.localPosition = new Vector3(0f,-.14f,.36f);
                    pistol=gun.GetComponent<LumiWeaponVisual>();
                }
                muzzle = pistol.Muzzle;
                BuildShield(visual);
                return;
            }

            Material body = LumiFactory.Material("LumiBody", new Color(0.15f, 0.75f, 0.9f));
            Material cream = LumiFactory.Material("LumiCream", new Color(0.95f, 0.95f, 0.82f));
            Material dark = LumiFactory.Material("LumiDark", new Color(0.06f, 0.12f, 0.18f));
            Material glow = LumiFactory.Material("LumiGlow", new Color(0.25f, 1f, 0.9f), true);

            LumiFactory.Primitive("Body", PrimitiveType.Capsule, visual, new Vector3(0f, 0.95f, 0f), new Vector3(0.72f, 0.62f, 0.62f), body, false);
            LumiFactory.Primitive("Head", PrimitiveType.Sphere, visual, new Vector3(0f, 1.68f, 0f), new Vector3(0.82f, 0.68f, 0.72f), cream, false);
            LumiFactory.Primitive("Face", PrimitiveType.Cube, visual, new Vector3(0f, 1.67f, 0.34f), new Vector3(0.56f, 0.28f, 0.08f), dark, false);
            LumiFactory.Primitive("Eye L", PrimitiveType.Sphere, visual, new Vector3(-0.16f, 1.7f, 0.405f), Vector3.one * 0.09f, glow, false);
            LumiFactory.Primitive("Eye R", PrimitiveType.Sphere, visual, new Vector3(0.16f, 1.7f, 0.405f), Vector3.one * 0.09f, glow, false);
            LumiFactory.Primitive("Antenna", PrimitiveType.Cylinder, visual, new Vector3(0f, 2.12f, 0f), new Vector3(0.045f, 0.18f, 0.045f), dark, false);
            LumiFactory.Primitive("Antenna Light", PrimitiveType.Sphere, visual, new Vector3(0f, 2.34f, 0f), Vector3.one * 0.12f, glow, false);
            LumiFactory.Primitive("Leg L", PrimitiveType.Capsule, visual, new Vector3(-0.23f, 0.35f, 0f), new Vector3(0.24f, 0.34f, 0.24f), dark, false);
            LumiFactory.Primitive("Leg R", PrimitiveType.Capsule, visual, new Vector3(0.23f, 0.35f, 0f), new Vector3(0.24f, 0.34f, 0.24f), dark, false);
            LumiFactory.Primitive("Arm L", PrimitiveType.Capsule, visual, new Vector3(-0.52f, 1.05f, 0.08f), new Vector3(0.18f, 0.42f, 0.18f), body, false).transform.localRotation = Quaternion.Euler(12f, 0f, -18f);
            LumiFactory.Primitive("Arm R", PrimitiveType.Capsule, visual, new Vector3(0.52f, 1.05f, 0.12f), new Vector3(0.18f, 0.42f, 0.18f), body, false).transform.localRotation = Quaternion.Euler(70f, 0f, 8f);
            LumiFactory.Primitive("Blaster", PrimitiveType.Cube, visual, new Vector3(0.48f, 1.22f, 0.48f), new Vector3(0.2f, 0.2f, 0.62f), dark, false);
            muzzle = LumiFactory.WorldObject("Muzzle", visual, new Vector3(0.48f, 1.22f, 0.86f)).transform;

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
            Vector3 pointer=Input.mousePosition;
            if(!Application.isMobilePlatform && (pointer.x<0 || pointer.x>Screen.width || pointer.y<0 || pointer.y>Screen.height))return;

            Ray ray = game.CameraRig.ViewCamera.ScreenPointToRay(Input.mousePosition);
            Vector3 aimOrigin=transform.position+Vector3.up*1.1f;
            Vector3 target = aimOrigin + game.CameraRig.FlatForward * 30f;
            Plane aimPlane = new Plane(Vector3.up, aimOrigin);
            if (aimPlane.Raycast(ray, out float aimDistance)) target = ray.GetPoint(aimDistance);
            if(Physics.Raycast(ray,out RaycastHit hit,80f,~0,QueryTriggerInteraction.Ignore))
            {
                LumiEnemy pointedEnemy=hit.collider.GetComponentInParent<LumiEnemy>();
                if(pointedEnemy!=null && pointedEnemy.IsAlive)target=pointedEnemy.AimPoint;
            }
            if(Application.isMobilePlatform)
            {
                LumiEnemy assisted = FindAimAssistTarget();
                target = assisted != null ? assisted.AimPoint : muzzle.position + transform.forward * 30f;
            }
            Vector3 flatAim=target-aimOrigin;flatAim.y=0;
            if(flatAim.sqrMagnitude<.36f)target=aimOrigin+transform.forward*30;
            else transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(flatAim),720f*Time.deltaTime);
            Vector3 direction=(target-muzzle.position).normalized;
            if(direction.sqrMagnitude<.001f)direction=transform.forward;
            if(!wantsShoot)return;
            if(!shotGate.TryConsume(Time.time,shotCooldown))return;
            game.SpawnProjectile(muzzle.position, direction, shotDamage, true,transform);
            game.SpawnImpact(muzzle.position, new Color(0.2f, 0.95f, 1f));
            if(pistol!=null)pistol.Fire();
            game.Audio.Play("shoot");
        }

        private LumiEnemy FindAimAssistTarget()
        {
            Camera camera = game.CameraRig.ViewCamera;
            LumiEnemy best = null;
            float bestScore = 0.16f;
            foreach (LumiEnemy enemy in FindObjectsOfType<LumiEnemy>())
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

