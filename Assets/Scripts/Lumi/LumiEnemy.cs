using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace LumiAdventure
{
    public enum LumiEnemyType { Sprout = 1, Ranger = 2, Golem = 3 }

    [RequireComponent(typeof(CharacterController))]
    public class LumiEnemy : MonoBehaviour, ILumiDamageable
    {
        public static readonly System.Collections.Generic.List<LumiEnemy> Active=new System.Collections.Generic.List<LumiEnemy>();
        private void OnEnable(){Active.Add(this);}
        private void OnDisable(){Active.Remove(this);}
        private LumiGame game;
        private LumiPlayer player;
        private CharacterController controller;
        private LumiHitFlash hitFlash;
        private LumiEnemyType enemyType;
        private LumiKuramaMotion kurama;
        private LumiImportedBijuuMotion bijuu;
        private int health,maxHealth;
        private float healing;
        private LumiCombatTactics tactics;
        private Component combatTarget;
        private Vector3 targetPoint=>combatTarget is LumiShadowClone c?c.AimPoint:player.transform.position+Vector3.up*.9f;
        public int MaxHealth=>maxHealth;
        public int AttackDamage=>damage;
        public float HealthFraction=>maxHealth>0?health/(float)maxHealth:0;
        public void Heal(float amount){if(!IsAlive)return;healing+=amount;int whole=Mathf.FloorToInt(healing);healing-=whole;health=Mathf.Min(maxHealth,health+whole);}
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
        private readonly RaycastHit[] obstacleHits=new RaycastHit[32];
        private int cornerCount;
        private int pathCorner;
        private float nextPath;
        private Vector3 spawnPoint;
        private Vector3 lastGravityPosition;

        public bool IsAlive => health > 0;
        public LumiEnemyType EnemyType => enemyType;
        public Vector3 AimPoint => bijuu!=null&&bijuu.BodyHitbox!=null?bijuu.BodyHitbox.bounds.center:kurama!=null&&kurama.BodyHitbox!=null?kurama.BodyHitbox.bounds.center:transform.position + Vector3.up * (enemyType == LumiEnemyType.Golem ? 1.45f : 0.95f);

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
                    health = 15; damage = 1; speed = 2.1f + difficulty * 0.25f;
                    detectRange = 11f + difficulty * 2f; attackRange = 1.35f; attackDelay = 1.25f;
                    break;
                case LumiEnemyType.Ranger:
                    health = 30; damage = 3; speed = 2.4f + difficulty * 0.28f;
                    detectRange = 15f + difficulty * 2.5f; attackRange = 10f; attackDelay = Mathf.Max(1.15f, 2.4f - difficulty * 0.15f);
                    break;
                default:
                    health = 60; damage = 5; speed = 1.7f + difficulty * 0.22f;
                    detectRange = 18f + difficulty * 2f; attackRange = 2.2f; attackDelay = Mathf.Max(1.2f, 2.1f - difficulty * 0.12f);
                    break;
            }
            float levelScale=1+.15f*(Mathf.Clamp(game.CurrentLevel,1,5)-1);health=Mathf.RoundToInt(health*levelScale);damage=Mathf.Max(1,Mathf.RoundToInt(damage*levelScale));
            maxHealth=health;tactics=gameObject.AddComponent<LumiCombatTactics>();tactics.Initialize(game,controller);
            strafeSign = Random.value > 0.5f ? 1f : -1f;
            BuildVisual();
            hitFlash = gameObject.AddComponent<LumiHitFlash>();
        }

        private void BuildVisual()
        {
            var visual=LumiBeastArt.Create(transform,enemyType);kurama=visual.GetComponent<LumiKuramaMotion>();if(kurama!=null){kurama.Initialize(game,this);controller.height=2.1f;controller.radius=.75f;controller.center=Vector3.up*1.05f;}
            bijuu=visual.GetComponent<LumiImportedBijuuMotion>();if(bijuu!=null){bijuu.Initialize(game,this);controller.height=1.8f;controller.radius=.65f;controller.center=Vector3.up*.9f;if(!bijuu.Ranged)attackRange=1.9f;}
        }

        private void Update()
        {
            if (!IsAlive || player == null || !player.IsAlive || game==null || !game.IsPlaying) return;
            if(tactics==null){maxHealth=enemyType==LumiEnemyType.Sprout?15:enemyType==LumiEnemyType.Ranger?30:60;tactics=GetComponent<LumiCombatTactics>()??gameObject.AddComponent<LumiCombatTactics>();tactics.Initialize(game,controller);}
            combatTarget=LumiCombatTactics.IsContested(player,transform.position)?player:LumiCombatTactics.SelectTarget(player,transform.position,detectRange,false);
            if(tactics.Tick(HealthFraction,speed,amount=>Heal(maxHealth*amount))){if(kurama!=null&&kurama.Busy)kurama.CancelAttack();if(bijuu!=null&&bijuu.Busy)bijuu.CancelAttack();ApplyGravity();return;}
            if (combatTarget==null)
            {
                ApplyGravity();
                return;
            }

            Vector3 toPlayer = combatTarget.transform.position - transform.position;
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

            if(kurama!=null){if(kurama.Busy){ApplyGravity();return;}if(distance>attackRange+1 && distance<=16 && Time.time>=nextAttack && HasLineOfSight()){nextAttack=Time.time+4.5f;kurama.Fire(combatTarget,damage);ApplyGravity();return;}}
            if(bijuu!=null&&bijuu.Busy){ApplyGravity();return;}
            if (enemyType == LumiEnemyType.Ranger) UpdateRanger(flat, distance);
            else UpdateMelee(flat, distance);
            ApplyGravity();
        }

        private void UpdateMelee(Vector3 direction, float distance)
        {
            if(bijuu!=null&&!bijuu.Ranged&&bijuu.WithinMeleeReach(combatTarget,.8f))distance=Mathf.Min(distance,attackRange);
            if (distance > attackRange)
            {
                float charge = enemyType == LumiEnemyType.Golem && distance < 7f && Time.time > nextThink ? 1.65f : 1f;
                if (charge > 1f) nextThink = Time.time + 4.5f;
                controller.Move(AvoidObstacles(direction) * speed * charge * Time.deltaTime);
            }
            else if (Time.time >= nextAttack && HasLineOfSight())
            {
                nextAttack = Time.time + attackDelay;
                if(bijuu!=null)bijuu.Attack(combatTarget,damage);
                else{((ILumiDamageable)combatTarget).TakeDamage(damage,targetPoint);game.SpawnImpact(targetPoint, Color.red);}
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
                if(bijuu!=null){bijuu.Attack(combatTarget,damage);return;}
                Vector3 origin = transform.position + Vector3.up * 1.15f + transform.forward * 0.6f;
                Vector3 target = targetPoint;
                game.SpawnProjectile(origin, (target - origin).normalized, damage, false,transform);
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
                    && NavMesh.SamplePosition(combatTarget.transform.position, out NavMeshHit end, 2f, NavMesh.AllAreas)
                    && NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,navigationPath))
                {
                    cornerCount = navigationPath.GetCornersNonAlloc(pathCorners);
                    pathCorner = 1;
                }
            }
            if (cornerCount > 1 && pathCorner < cornerCount && (enemyType != LumiEnemyType.Ranger || Vector3.Distance(transform.position,combatTarget.transform.position)>8.5f))
            {
                Vector3 waypoint = pathCorners[pathCorner] - transform.position; waypoint.y = 0;
                if (waypoint.sqrMagnitude < .65f && pathCorner < cornerCount-1) pathCorner++;
                else desired = waypoint.normalized;
            }
            Vector3 origin = transform.position + Vector3.up * 0.7f;
            if (Blocked(origin,.42f,desired,1.2f))
            {
                Vector3 side = Vector3.Cross(Vector3.up, desired) * strafeSign;
                if (!Blocked(origin,.4f,side,1f)) return side;
                return -side;
            }
            return desired;
        }

        private bool HasLineOfSight()
        {
            Vector3 origin = transform.position + Vector3.up * 1.2f;
            Vector3 target = targetPoint;
            Vector3 delta=target-origin;int count=Physics.RaycastNonAlloc(origin,delta.normalized,obstacleHits,delta.magnitude,~(1<<31),QueryTriggerInteraction.Ignore);var hits=obstacleHits;if(count==hits.Length){hits=Physics.RaycastAll(origin,delta.normalized,delta.magnitude,~(1<<31),QueryTriggerInteraction.Ignore);count=hits.Length;}float nearest=float.PositiveInfinity;Collider closest=null;for(int i=0;i<count;i++){var hit=hits[i];if(hit.collider.transform.IsChildOf(transform)||hit.distance>=nearest)continue;nearest=hit.distance;closest=hit.collider;}return closest==null||(combatTarget!=null&&closest.transform.IsChildOf(combatTarget.transform));
        }

        private bool Blocked(Vector3 origin,float radius,Vector3 direction,float distance){int count=Physics.SphereCastNonAlloc(origin,radius,direction,obstacleHits,distance,~(1<<31),QueryTriggerInteraction.Ignore);var hits=obstacleHits;if(count==hits.Length){hits=Physics.SphereCastAll(origin,radius,direction,distance,~(1<<31),QueryTriggerInteraction.Ignore);count=hits.Length;}for(int i=0;i<count;i++)if(!hits[i].collider.transform.IsChildOf(transform))return true;return false;}

        private void ApplyGravity()
        {
            if(controller.isGrounded&&verticalVelocity<0&&(transform.position-lastGravityPosition).sqrMagnitude<.000001f)return;
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
            lastGravityPosition=transform.position;
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
            game.TryDropEnemySupportItem(transform.position);
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

