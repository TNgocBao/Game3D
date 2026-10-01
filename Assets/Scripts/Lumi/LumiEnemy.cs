using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace LumiAdventure
{
    public enum LumiEnemyType { Sprout = 1, Ranger = 2, Golem = 3 }

    [RequireComponent(typeof(CharacterController))]
    public class LumiEnemy : MonoBehaviour, ILumiDamageable
    {
        private LumiGame game;
        private LumiPlayer player;
        private CharacterController controller;
        private LumiHitFlash hitFlash;
        private LumiEnemyType enemyType;
        private int health;
        private int damage;
        private float speed;
        private float detectRange;
        private float attackRange;
        private float attackDelay;
        private float nextAttack;
        private float verticalVelocity;
        private float strafeSign;
        private float nextThink;
        private float intelligence;
        private NavMeshPath navigationPath;
        private readonly Vector3[] pathCorners = new Vector3[32];
        private int cornerCount;
        private int pathCorner;
        private float nextPath;
        private Vector3 spawnPoint;
        private LumiWeaponVisual pistol;
        private LumiInfantryMotion infantryMotion;

        public bool IsAlive => health > 0;
        public LumiEnemyType EnemyType => enemyType;
        public Vector3 AimPoint => transform.position + Vector3.up * (enemyType == LumiEnemyType.Golem ? 1.45f : 0.95f);

        public void Initialize(LumiGame owner, LumiPlayer target, LumiEnemyType type, float difficulty)
        {
            game = owner;
            player = target;
            enemyType = type;
            intelligence = difficulty;
            spawnPoint = transform.position;
            navigationPath = new NavMeshPath();
            nextPath = Time.time + Random.Range(0f,.6f);
            controller = GetComponent<CharacterController>();
            controller.radius = type == LumiEnemyType.Golem ? 0.72f : 0.46f;
            controller.height = type == LumiEnemyType.Golem ? 2.6f : 1.7f;
            controller.center = Vector3.up * controller.height * 0.5f;
            controller.stepOffset = 0.3f;

            switch (type)
            {
                case LumiEnemyType.Sprout:
                    health = 10; damage = 1; speed = 2.1f + difficulty * 0.25f;
                    detectRange = 11f + difficulty * 2f; attackRange = 1.35f; attackDelay = 1.25f;
                    break;
                case LumiEnemyType.Ranger:
                    health = 20; damage = 3; speed = 2.4f + difficulty * 0.28f;
                    detectRange = 15f + difficulty * 2.5f; attackRange = 10f; attackDelay = Mathf.Max(1.15f, 2.4f - difficulty * 0.15f);
                    break;
                default:
                    health = 40; damage = 5; speed = 1.7f + difficulty * 0.22f;
                    detectRange = 18f + difficulty * 2f; attackRange = 2.2f; attackDelay = Mathf.Max(1.2f, 2.1f - difficulty * 0.12f);
                    break;
            }
            strafeSign = Random.value > 0.5f ? 1f : -1f;
            BuildVisual();
            hitFlash = gameObject.AddComponent<LumiHitFlash>();
        }

        private void BuildVisual()
        {
            Transform visual = LumiFactory.WorldObject(enemyType + " Visual", transform, Vector3.zero).transform;
            GameObject model = LumiArt.CreateEnemyModel(enemyType, visual);
            if (model != null)
            {
                infantryMotion=model.GetComponent<LumiInfantryMotion>();infantryMotion.Configure(game,true);
                if (enemyType == LumiEnemyType.Ranger)
                {
                    GameObject gun = LumiArt.CreateBlaster(infantryMotion.WeaponHand,true);
                    if (gun != null)
                    {
                        gun.transform.localPosition = new Vector3(0,-.14f,.36f);
                        pistol=gun.GetComponent<LumiWeaponVisual>();
                    }
                }
                else LumiChibiWeapon.Melee(infantryMotion.WeaponHand,enemyType==LumiEnemyType.Golem);
                return;
            }
            Material eye = LumiFactory.Material("EnemyEye", new Color(1f, 0.3f, 0.12f), true);
            if (enemyType == LumiEnemyType.Sprout)
            {
                Material green = LumiFactory.Material("Sprout", new Color(0.3f, 0.78f, 0.28f));
                LumiFactory.Primitive("Body", PrimitiveType.Sphere, visual, new Vector3(0f, 0.65f, 0f), new Vector3(0.9f, 0.75f, 0.85f), green, false);
                LumiFactory.Primitive("Leaf L", PrimitiveType.Sphere, visual, new Vector3(-0.2f, 1.28f, 0f), new Vector3(0.22f, 0.42f, 0.12f), green, false).transform.localRotation = Quaternion.Euler(0f, 0f, 35f);
                LumiFactory.Primitive("Leaf R", PrimitiveType.Sphere, visual, new Vector3(0.2f, 1.28f, 0f), new Vector3(0.22f, 0.42f, 0.12f), green, false).transform.localRotation = Quaternion.Euler(0f, 0f, -35f);
                AddEyes(visual, eye, 0.66f, 0.25f);
            }
            else if (enemyType == LumiEnemyType.Ranger)
            {
                Material purple = LumiFactory.Material("Ranger", new Color(0.58f, 0.25f, 0.82f));
                Material dark = LumiFactory.Material("RangerDark", new Color(0.14f, 0.08f, 0.22f));
                LumiFactory.Primitive("Body", PrimitiveType.Capsule, visual, new Vector3(0f, 0.78f, 0f), new Vector3(0.68f, 0.72f, 0.68f), purple, false);
                LumiFactory.Primitive("Hood", PrimitiveType.Sphere, visual, new Vector3(0f, 1.45f, 0f), new Vector3(0.76f, 0.62f, 0.68f), dark, false);
                LumiFactory.Primitive("Blaster", PrimitiveType.Cube, visual, new Vector3(0.45f, 0.95f, 0.35f), new Vector3(0.18f, 0.18f, 0.55f), purple, false);
                AddEyes(visual, eye, 1.45f, 0.29f);
            }
            else
            {
                Material stone = LumiFactory.Material("Golem", new Color(0.38f, 0.42f, 0.48f));
                Material core = LumiFactory.Material("GolemCore", new Color(1f, 0.34f, 0.08f), true);
                LumiFactory.Primitive("Torso", PrimitiveType.Cube, visual, new Vector3(0f, 1.25f, 0f), new Vector3(1.2f, 1.5f, 0.8f), stone, false);
                LumiFactory.Primitive("Head", PrimitiveType.Cube, visual, new Vector3(0f, 2.15f, 0f), new Vector3(0.82f, 0.62f, 0.72f), stone, false);
                LumiFactory.Primitive("Arm L", PrimitiveType.Cube, visual, new Vector3(-0.8f, 1.25f, 0f), new Vector3(0.42f, 1.4f, 0.52f), stone, false);
                LumiFactory.Primitive("Arm R", PrimitiveType.Cube, visual, new Vector3(0.8f, 1.25f, 0f), new Vector3(0.42f, 1.4f, 0.52f), stone, false);
                LumiFactory.Primitive("Core", PrimitiveType.Sphere, visual, new Vector3(0f, 1.35f, 0.43f), Vector3.one * 0.3f, core, false);
                AddEyes(visual, eye, 2.18f, 0.38f);
            }
        }

        private static void AddEyes(Transform visual, Material material, float height, float forward)
        {
            LumiFactory.Primitive("Eye L", PrimitiveType.Sphere, visual, new Vector3(-0.16f, height, forward), Vector3.one * 0.1f, material, false);
            LumiFactory.Primitive("Eye R", PrimitiveType.Sphere, visual, new Vector3(0.16f, height, forward), Vector3.one * 0.1f, material, false);
        }

        private void Update()
        {
            if (!IsAlive || player == null || !player.IsAlive || !game.IsPlaying) return;
            if (player.IsInvisible)
            {
                ApplyGravity();
                return;
            }

            Vector3 toPlayer = player.transform.position - transform.position;
            float distance = new Vector2(toPlayer.x, toPlayer.z).magnitude;
            if (distance > detectRange)
            {
                ApplyGravity();
                return;
            }

            Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z).normalized;
            if (flat.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat), (5f + intelligence) * Time.deltaTime);
            }

            if (enemyType == LumiEnemyType.Ranger) UpdateRanger(flat, distance);
            else UpdateMelee(flat, distance);
            ApplyGravity();
        }

        private void UpdateMelee(Vector3 direction, float distance)
        {
            if (distance > attackRange)
            {
                float charge = enemyType == LumiEnemyType.Golem && distance < 7f && Time.time > nextThink ? 1.65f : 1f;
                if (charge > 1f) nextThink = Time.time + 4.5f;
                controller.Move(AvoidObstacles(direction) * speed * charge * Time.deltaTime);
            }
            else if (Time.time >= nextAttack && HasLineOfSight())
            {
                nextAttack = Time.time + attackDelay;
                player.TakeDamage(damage, player.transform.position + Vector3.up);
                game.SpawnImpact(player.transform.position + Vector3.up, Color.red);
            }
        }

        private void UpdateRanger(Vector3 direction, float distance)
        {
            Vector3 move = Vector3.zero;
            if (distance > 8.5f) move = direction;
            else if (distance < 5.5f) move = -direction;
            else move = Vector3.Cross(Vector3.up, direction) * strafeSign * Mathf.Clamp01(intelligence / 2f);

            if (Time.time >= nextThink)
            {
                nextThink = Time.time + Random.Range(1.2f, 2.4f);
                if (Random.value < 0.35f + intelligence * 0.07f) strafeSign *= -1f;
            }
            controller.Move(AvoidObstacles(move) * speed * Time.deltaTime);

            if (distance <= attackRange && Time.time >= nextAttack && HasLineOfSight())
            {
                nextAttack = Time.time + attackDelay;
                Vector3 origin = transform.position + Vector3.up * 1.15f + transform.forward * 0.6f;
                Vector3 target = player.transform.position + Vector3.up * 0.9f;
                if(pistol!=null)origin=pistol.Muzzle.position;
                game.SpawnProjectile(origin, (target - origin).normalized, damage, false,transform);
                if(pistol!=null)pistol.Fire();
            }
        }

        private Vector3 AvoidObstacles(Vector3 desired)
        {
            if (desired.sqrMagnitude < 0.01f) return desired;
            if (Time.time >= nextPath)
            {
                nextPath = Time.time + Mathf.Max(.35f, 1.1f - intelligence * .15f);
                cornerCount = 0;
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit start, 2f, NavMesh.AllAreas)
                    && NavMesh.SamplePosition(player.transform.position, out NavMeshHit end, 2f, NavMesh.AllAreas)
                    && NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,navigationPath))
                {
                    cornerCount = navigationPath.GetCornersNonAlloc(pathCorners);
                    pathCorner = 1;
                }
            }
            if (cornerCount > 1 && pathCorner < cornerCount && (enemyType != LumiEnemyType.Ranger || Vector3.Distance(transform.position,player.transform.position)>8.5f))
            {
                Vector3 waypoint = pathCorners[pathCorner] - transform.position; waypoint.y = 0;
                if (waypoint.sqrMagnitude < .65f && pathCorner < cornerCount-1) pathCorner++;
                else desired = waypoint.normalized;
            }
            Vector3 origin = transform.position + Vector3.up * 0.7f;
            if (Physics.SphereCast(origin, 0.42f, desired, out _, 1.2f, ~0, QueryTriggerInteraction.Ignore))
            {
                Vector3 side = Vector3.Cross(Vector3.up, desired) * strafeSign;
                if (!Physics.SphereCast(origin, 0.4f, side, out _, 1f, ~0, QueryTriggerInteraction.Ignore)) return side;
                return -side;
            }
            return desired;
        }

        private bool HasLineOfSight()
        {
            Vector3 origin = transform.position + Vector3.up * 1.2f;
            Vector3 target = player.transform.position + Vector3.up * 0.9f;
            if (!Physics.Linecast(origin, target, out var hit, ~0, QueryTriggerInteraction.Ignore)) return true;
            return hit.collider.GetComponentInParent<LumiPlayer>() != null;
        }

        private void ApplyGravity()
        {
            if (transform.position.y < -6f)
            {
                controller.enabled = false;
                transform.position = spawnPoint;
                controller.enabled = true;
                verticalVelocity = 0;
            }
            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            verticalVelocity += -22f * Time.deltaTime;
            controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
            if(infantryMotion!=null)infantryMotion.SetGrounded(controller.isGrounded);
        }

        public void TakeDamage(int amount, Vector3 hitPoint)
        {
            if (!IsAlive) return;
            health -= amount;
            hitFlash.Flash(new Color(1f, 0.08f, 0.05f));
            game.Audio.Play("hit", 0.7f);
            game.SpawnImpact(hitPoint, Color.red);
            if (health <= 0) StartCoroutine(Die());
        }

        private IEnumerator Die()
        {
            controller.enabled = false;
            game.RegisterKill(enemyType);
            game.SpawnImpact(transform.position + Vector3.up, enemyType == LumiEnemyType.Golem
                ? new Color(1f, 0.35f, 0.08f) : new Color(0.8f, 0.2f, 0.35f));
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            while (elapsed < 0.35f)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / 0.35f);
                transform.Rotate(0f, 420f * Time.deltaTime, 0f);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
