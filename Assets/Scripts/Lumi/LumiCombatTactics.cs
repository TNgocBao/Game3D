using System;
using UnityEngine;
using UnityEngine.AI;
namespace LumiAdventure
{
    // Shared decisions; CharacterControllers retain ownership of movement and gravity.
    public sealed class LumiCombatTactics:MonoBehaviour
    {
        private LumiGame game;private CharacterController controller;private bool retreating;
        private float healClock,nextPath;private Vector3 pathDestination;private int count,corner;private NavMeshPath path;
        private readonly Vector3[] corners=new Vector3[24];
        public bool Retreating=>retreating;
        public static bool IsContested(LumiPlayer player,Vector3 point){var altar=LumiHealingAltar.FindNearest(point);return altar!=null && player!=null && player.IsAlive && altar.Contains(player.transform.position,1.5f);}
        public void Initialize(LumiGame owner,CharacterController body){game=owner;controller=body;path=new NavMeshPath();}
        public static Component SelectTarget(LumiPlayer player,Vector3 origin,float range,bool preferClones)
        {
            Component best=player!=null && player.IsAlive && !player.IsInvisible?player:null;
            float nearest=best!=null?Vector3.Distance(origin,player.transform.position):range;
            if(preferClones)nearest=range;
            foreach(var clone in LumiShadowClone.Active)
            {
                if(clone==null || !clone.IsAlive)continue;float distance=Vector3.Distance(origin,clone.transform.position);
                if(distance<nearest){nearest=distance;best=clone;}
            }
            return best;
        }
        public bool Tick(float health,float speed,Action<float> heal)
        {
            LumiHealingAltar altar=LumiHealingAltar.FindNearest(transform.position);
            bool contested=altar!=null && game.Player!=null && game.Player.IsAlive && altar.Contains(game.Player.transform.position,1.5f);
            if(health<.3f && altar!=null)retreating=true;
            if(health>=.8f)retreating=false;
            Vector3 escape;bool meleeDanger=false;
            var skills=game.Player!=null?game.Player.GetComponent<LumiNarutoSkills>():null;
            escape=transform.position;
            if(skills!=null && skills.TryMeleeThreat(out Vector3 center,out float radius)){Vector3 away=transform.position-center;away.y=0;float safe=radius+controller.radius+1;if(away.magnitude<safe){meleeDanger=true;escape=center+(away.sqrMagnitude>.01f?away.normalized:Vector3.right)*safe;escape.y=transform.position.y;}}
            if(meleeDanger || TryDangerEscape(transform.position,controller.radius,out escape))
            {healClock=0;MoveTo(escape,speed*1.5f);return true;}
            if(retreating && altar!=null && !contested)
            {
                if(!altar.Contains(transform.position,-1)){healClock=0;MoveTo(altar.transform.position,speed*1.5f);}
                else{healClock+=Time.deltaTime;while(healClock>=1){healClock-=1;heal(.1f);}}
                return true;
            }
            healClock=0;return false;
        }
        public static bool TryDangerEscape(Vector3 point,float bodyRadius,out Vector3 destination)
        {
            destination=point;float biggest=0;
            foreach(var blast in LumiRasenshurikenBlast.Active)
            {
                if(blast==null)continue;Vector3 away=point-blast.transform.position;away.y=0;
                float safe=blast.DamageRadius+bodyRadius+1f;
                if(away.magnitude<safe && safe-away.magnitude>biggest){biggest=safe-away.magnitude;destination=blast.transform.position+(away.sqrMagnitude>.01f?away.normalized:Vector3.right)*safe;destination.y=point.y;}
            }
            foreach(var shot in LumiChakraProjectile.Active)
            {
                if(shot==null)continue;Vector3 to=point+Vector3.up-shot.transform.position;
                float along=Vector3.Dot(to,shot.Direction);if(along<0 || along>16*.65f)continue;
                Vector3 closest=shot.transform.position+shot.Direction*along;Vector3 away=point+Vector3.up-closest;
                float safe=shot.CollisionRadius+bodyRadius+.8f;
                if(away.magnitude>=safe)continue;
                Vector3 side=Vector3.Cross(Vector3.up,shot.Direction).normalized;if(side.sqrMagnitude<.01f)side=Vector3.right;
                if(Vector3.Dot(away,side)<0)side=-side;
                destination=point+side*(safe+1);biggest=Mathf.Max(biggest,.1f);
            }
            return biggest>0;
        }
        private void MoveTo(Vector3 destination,float speed)
        {
            Vector3 step=destination-transform.position;step.y=0;
            if(Time.time>=nextPath || (destination-pathDestination).sqrMagnitude>4)
            {nextPath=Time.time+.3f;pathDestination=destination;count=0;corner=1;
            if(NavMesh.SamplePosition(transform.position,out NavMeshHit start,2,NavMesh.AllAreas) && NavMesh.SamplePosition(destination,out NavMeshHit end,3,NavMesh.AllAreas) && NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path) && path.status==NavMeshPathStatus.PathComplete)
            {
                count=path.GetCornersNonAlloc(corners);
            }
            }
            while(corner<count){Vector3 next=corners[corner]-transform.position;next.y=0;if(next.magnitude>.5f){step=next;break;}corner++;
            }
            if(step.sqrMagnitude<.01f)return;
            controller.Move(step.normalized*speed*Time.deltaTime);
            transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(step),480*Time.deltaTime);
        }
    }
    public sealed class LumiHealingAltar:MonoBehaviour
    {
        public static readonly System.Collections.Generic.List<LumiHealingAltar> Active=new System.Collections.Generic.List<LumiHealingAltar>();
        public const float Radius=6;
        private void OnEnable(){Active.Add(this);}private void OnDisable(){Active.Remove(this);}
        public bool Contains(Vector3 point,float margin=0){Vector3 d=point-transform.position;return new Vector2(d.x,d.z).magnitude<Radius+margin && Mathf.Abs(d.y)<4;}
        public static LumiHealingAltar FindNearest(Vector3 point){LumiHealingAltar best=null;float distance=float.PositiveInfinity;foreach(var altar in Active){if(altar==null)continue;float d=(altar.transform.position-point).sqrMagnitude;if(d<distance){distance=d;best=altar;}}return best;}
        public static void Create(LumiGame game,Transform parent,Vector3 bossHome)
        {
            Vector3 point=bossHome+Vector3.right*7;
            if(NavMesh.SamplePosition(point,out NavMeshHit hit,5,NavMesh.AllAreas))point=hit.position;
            else point=bossHome;
            Transform root=LumiFactory.WorldObject("Tế đàn hồi phục • 10% HP/s",parent,point).transform;root.gameObject.AddComponent<LumiHealingAltar>();
            Material stone=LumiFactory.Material("Altar stone",new Color(.34f,.40f,.43f));Material glow=LumiFactory.Material("Altar healing chakra",new Color(.15f,.95f,.65f),true);
            LumiFactory.Primitive("Altar platform",PrimitiveType.Cylinder,root,new Vector3(0,.09f,0),new Vector3(3,.09f,3),stone,false);
            LumiFactory.Primitive("Altar pedestal",PrimitiveType.Cube,root,new Vector3(0,.5f,0),new Vector3(.9f,.8f,.9f),stone,false);
            LumiFactory.Primitive("Healing crystal",PrimitiveType.Sphere,root,new Vector3(0,1.15f,0),new Vector3(.55f,.8f,.55f),glow,false);
            GameObject circle=LumiVillageBoss.Circle(point,Radius,new Color(.15f,.95f,.65f),"Healing area • 12m diameter");circle.transform.SetParent(root,true);
        }
    }
}
