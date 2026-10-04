using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LumiAdventure
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class LumiShadowClone:MonoBehaviour,ILumiDamageable
    {
        public static readonly List<LumiShadowClone> Active=new List<LumiShadowClone>();
        private LumiGame game;
        private LumiPlayer owner;
        private CharacterController controller;
        private LumiInfantryMotion motion;
        private Component target;
        public float Health {get;private set;}
        public float MaxHealth {get;private set;}
        public float Armor {get;private set;}
        public float AttackDamage {get;private set;}
        public float MovementSpeed {get;private set;}
        private ILumiDamageable Victim=>target as ILumiDamageable;
        private Vector3 TargetPoint=>target is LumiEnemy e?e.AimPoint:target is LumiVillageBoss b?b.AimPoint:owner.transform.position;
        private NavMeshPath path;
        private readonly Vector3[] corners=new Vector3[24];
        private int cornerCount,corner;
        private float damageRemainder;
        private float ends,nextThink,nextAttack,vertical;
        public bool IsAlive{get;private set;}
        public Vector3 AimPoint=>transform.position+Vector3.up;
        public void Initialize(LumiGame session,LumiPlayer player,float duration)
        {
            game=session;owner=player;MaxHealth=LumiPlayer.MaxHealth*.3f;Health=Mathf.Min(MaxHealth,player.Health*.3f);Armor=player.Armor*.3f;AttackDamage=player.BasicAttackDamage*.3f;MovementSpeed=player.MovementSpeed*.3f;IsAlive=true;ends=Time.time+duration;Active.Add(this);
            controller=GetComponent<CharacterController>();controller.height=1.8f;controller.radius=.38f;controller.center=Vector3.up*.9f;controller.stepOffset=.35f;
            GameObject model=LumiNarutoArt.Create(transform);motion=model.GetComponent<LumiInfantryMotion>();motion.Configure(game,true);
            path=new NavMeshPath();LumiChakraVisual.Burst(transform.parent,AimPoint,true);
        }
        private void OnEnable(){if(IsAlive && !Active.Contains(this))Active.Add(this);}
        private void OnDisable(){Active.Remove(this);}
        private void Update()
        {
            if(!IsAlive || game==null || !game.IsPlaying)return;
            if(Time.time>=ends || owner==null || !owner.IsAlive){Disperse();return;}
            if(Time.time>=nextThink)
            {
                nextThink=Time.time+.45f;target=null;float nearest=18;
                foreach(LumiEnemy enemy in LumiEnemy.Active)
                {
                    if(enemy==null || !enemy.IsAlive)continue;float distance=Vector3.Distance(transform.position,enemy.transform.position);
                    if(distance<nearest){nearest=distance;target=enemy;}
                }
                if(game.VillageBoss!=null && game.VillageBoss.IsAlive && Vector3.Distance(transform.position,game.VillageBoss.transform.position)<nearest)target=game.VillageBoss;
                Vector3 destination=target!=null?target.transform.position:owner.transform.position-transform.forward*2;
                cornerCount=0;
                if(NavMesh.SamplePosition(transform.position,out NavMeshHit start,2,NavMesh.AllAreas) && NavMesh.SamplePosition(destination,out NavMeshHit end,2,NavMesh.AllAreas) && NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path))
                {cornerCount=path.GetCornersNonAlloc(corners);corner=1;}
            }
            if(target!=null && (Victim==null || !Victim.IsAlive)){target=null;nextThink=0;}
            Vector3 to=target!=null?target.transform.position-transform.position:owner.transform.position-transform.position;to.y=0;
            if(to.magnitude>(target!=null?1.4f:3))
            {
                Vector3 step=to.normalized;
                if(cornerCount>1 && corner<cornerCount)
                {
                    Vector3 toward=corners[corner]-transform.position;toward.y=0;
                    if(toward.magnitude<.6f && corner<cornerCount-1)corner++;else step=toward.normalized;
                }
                controller.Move(step*MovementSpeed*Time.deltaTime);
                if(step.sqrMagnitude>.001f)transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(step),540*Time.deltaTime);
            }
            else if(target!=null && Time.time>=nextAttack)
            {
                if(Physics.Linecast(AimPoint,TargetPoint,out RaycastHit hit,~0,QueryTriggerInteraction.Ignore) && !hit.collider.transform.IsChildOf(target.transform))return;
                nextAttack=Time.time+1.1f;motion.CastPose(.22f);damageRemainder+=AttackDamage;int hitDamage=Mathf.FloorToInt(damageRemainder);damageRemainder-=hitDamage;if(hitDamage>0)Victim.TakeDamage(hitDamage,TargetPoint);
            }
            if(controller.isGrounded && vertical<0)vertical=-2;vertical-=22*Time.deltaTime;controller.Move(Vector3.up*vertical*Time.deltaTime);motion.SetGrounded(controller.isGrounded);
        }
        public void TakeDamage(int damage,Vector3 point){if(!IsAlive || damage<=0)return;float absorbed=Mathf.Min(Armor,damage);Armor-=absorbed;Health=Mathf.Max(0,Health-(damage-absorbed));motion.ReactToHit();if(Health<=0)Disperse();}
        public void Disperse(){if(!IsAlive)return;IsAlive=false;Active.Remove(this);LumiChakraVisual.Burst(transform.parent,AimPoint,true);Destroy(gameObject);}
    }
}
