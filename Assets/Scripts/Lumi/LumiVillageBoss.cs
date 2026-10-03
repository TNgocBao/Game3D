using System.Collections;
using UnityEngine;

namespace LumiAdventure
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class LumiVillageBoss:MonoBehaviour,ILumiDamageable
    {
        public static readonly string[] Names={"Hashirama Senju","Gaara","Ōnoki","Raikage Đệ Tứ","Mei Terumī"};
        public static readonly string[] Elements={"MỘC ĐỘN","CÁT","TRẦN ĐỘN","LÔI ĐỘN","DUNG ĐỘN"};
        private LumiGame game;
        private CharacterController controller;
        private LumiHitFlash flash;
        private LumiInfantryMotion motion;
        private Vector3 home;
        private int village,health,maxHealth;
        private float nextSkill,nextMelee,gravity;
        private bool casting,engaged;
        public bool IsAlive=>health>0;
        public float HealthFraction=>health/(float)maxHealth;
        public string BossName=>Names[village-1];
        public string Element=>Elements[village-1];
        public Vector3 AimPoint=>transform.position+Vector3.up*1.4f;
        public bool Engaged=>engaged && IsAlive;
        public void Initialize(LumiGame owner,int level)
        {
            game=owner;village=level;home=transform.position;maxHealth=72+level*12;health=maxHealth;
            controller=GetComponent<CharacterController>();controller.height=2.4f;controller.radius=.55f;controller.center=Vector3.up*1.2f;
            GameObject model=LumiBossArt.Create(transform,village);motion=model.GetComponent<LumiInfantryMotion>();motion.Configure(game,true);
            flash=gameObject.AddComponent<LumiHitFlash>();nextSkill=Time.time+2;
        }
        private void Update()
        {
            if(!IsAlive || game==null || !game.IsPlaying || !game.Player.IsAlive)return;
            LumiPlayer player=game.Player;Vector3 delta=player.transform.position-transform.position;delta.y=0;
            if(!engaged && delta.magnitude<22){engaged=true;game.ShowToast(BossName+" — "+Element,new Color(1,.7f,.25f));nextSkill=Time.time+1.5f;}
            if(!engaged)return;
            if(!casting)
            {
                if(delta.sqrMagnitude>.1f)transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(delta),360*Time.deltaTime);
                if(Vector3.Distance(player.transform.position,home)<25 && delta.magnitude>3 && !player.IsInvisible)controller.Move(delta.normalized*(village==4?3.2f:2.1f)*Time.deltaTime);
                if(delta.magnitude<2.1f && Time.time>nextMelee && !player.IsInvisible)
                {nextMelee=Time.time+1.8f;motion.PlayTechnique(LumiTechnique.BasicAttack,.45f);player.TakeDamage(2,player.transform.position+Vector3.up);}
                if(Time.time>=nextSkill && delta.magnitude<25 && !player.IsInvisible)
                {nextSkill=Time.time+(HealthFraction<.5f?4.2f:5.8f);StartCoroutine(ExclusiveSkill());}
            }
            if(controller.isGrounded && gravity<0)gravity=-2;gravity-=22*Time.deltaTime;controller.Move(Vector3.up*gravity*Time.deltaTime);motion.SetGrounded(controller.isGrounded);
        }
        private IEnumerator ExclusiveSkill()
        {
            casting=true;motion.PlayTechnique((LumiTechnique)((int)LumiTechnique.WoodRelease+village-1),village==4?1.9f:1.65f);
            Vector3 target=game.Player.transform.position;target.y=.08f;
            Color color=village==1?new Color(.3f,.9f,.36f):village==2?new Color(1,.73f,.25f):village==3?new Color(.7f,.87f,1):village==4?new Color(.3f,.75f,1):new Color(1,.3f,.07f);
            GameObject warning=Circle(target,village==3?3.2f:3.7f,color,"Boss attack warning");
            warning.transform.SetParent(transform.parent,true);Destroy(warning,2);
            game.Audio.Play(village==4?"chakra":"wind",.7f);
            yield return new WaitForSeconds(village==4?.85f:1.2f);
            if(!IsAlive || !game.Player.IsAlive){Destroy(warning);casting=false;yield break;}
            Destroy(warning);
            if(village==1)
            {
                Transform forest=LumiFactory.WorldObject("Wood release • root eruption",transform.parent,target).transform;
                Material bark=LumiFactory.Material("Hashirama living wood",new Color(.36f,.22f,.1f));
                for(int i=0;i<12;i++)
                {
                    float angle=i*Mathf.PI/6;
                    GameObject root=LumiFactory.Primitive("Emerging wood root",PrimitiveType.Capsule,forest,new Vector3(Mathf.Cos(angle)*2.5f,1.1f,Mathf.Sin(angle)*2.5f),new Vector3(.65f,1.6f,.65f),bark,false);
                    root.transform.localRotation=Quaternion.Euler(Mathf.Sin(angle)*30,0,Mathf.Cos(angle)*30);
                }
                // A segmented wood dragon rises above the roots, with a readable head and horns.
                for(int i=0;i<9;i++)LumiFactory.Primitive("Wood dragon segment",PrimitiveType.Sphere,forest,new Vector3(Mathf.Sin(i*.45f)*1.6f,1+i*.42f,Mathf.Cos(i*.45f)*1.6f),Vector3.one*(.85f-i*.025f),bark,false);
                Vector3 head=new Vector3(Mathf.Sin(3.6f)*1.6f,4.55f,Mathf.Cos(3.6f)*1.6f);
                LumiFactory.Primitive("Wood dragon snout",PrimitiveType.Cube,forest,head+Vector3.forward*.45f,new Vector3(.65f,.5f,1.25f),bark,false);
                for(int side=-1;side<=1;side+=2)LumiFactory.Primitive("Dragon horn",PrimitiveType.Capsule,forest,head+new Vector3(side*.4f,.5f,0),new Vector3(.14f,.55f,.14f),bark,false);
                forest.gameObject.AddComponent<LumiBossHazard>().Initialize(game,3.7f,3,2.4f,false);
            }
            else if(village==2)
            {
                Transform wave=LumiFactory.WorldObject("Sand release • sand burial",transform.parent,target).transform;
                Material sand=LumiFactory.Material("Gaara sand",new Color(.82f,.6f,.28f));
                for(int i=0;i<16;i++)
                {
                    float angle=i*Mathf.PI/8;
                    LumiFactory.Primitive("Curling sand wave",PrimitiveType.Sphere,wave,new Vector3(Mathf.Cos(angle)*2.7f,.7f,Mathf.Sin(angle)*2.7f),new Vector3(1.6f,2.4f,1.2f),sand,false);
                }
                wave.gameObject.AddComponent<LumiBossHazard>().Initialize(game,3.7f,3,2.2f,false);
                LumiChakraVisual.Burst(transform.parent,target+Vector3.up,false,1.2f);
            }
            else if(village==3)
            {
                Transform cube=LumiFactory.WorldObject("Particle release • primitive world cube",transform.parent,target+Vector3.up*2).transform;
                CubeEdges(cube,6.4f,color);
                LumiFactory.Primitive("Particle release core",PrimitiveType.Sphere,cube,Vector3.zero,Vector3.one*1.4f,LumiFactory.Material("Dust release core",Color.white,true),false);
                cube.gameObject.AddComponent<LumiBossHazard>().Initialize(game,3.2f,4,1.3f,true);
            }
            else if(village==4)
            {
                Transform aura=LumiFactory.WorldObject("Lightning chakra armour",transform,Vector3.zero).transform;
                for(int i=0;i<8;i++)
                {
                    Vector3 a=new Vector3(Mathf.Cos(i*.785f)*.8f,.35f,Mathf.Sin(i*.785f)*.8f);
                    Line(aura,new[]{a,a+new Vector3(.2f,.5f,-.18f),a+new Vector3(-.14f,1,.16f),a+Vector3.up*1.8f},color,.08f);
                }
                float elapsed=0;Vector3 direction=target-transform.position;direction.y=0;direction.Normalize();bool struck=false;
                while(elapsed<.65f && IsAlive && game.Player.IsAlive)
                {
                    elapsed+=Time.deltaTime;controller.Move(direction*13*Time.deltaTime);
                    if(!struck && Vector3.Distance(game.Player.transform.position,transform.position)<2){struck=true;game.Player.TakeDamage(4,game.Player.transform.position+Vector3.up);}
                    yield return null;
                }
                Destroy(aura);
            }
            else
            {
                Transform lava=LumiFactory.WorldObject("Lava release • molten stream",transform.parent,target).transform;
                Material molten=LumiFactory.Material("Mei molten lava",new Color(1,.2f,.015f),true);
                LumiFactory.Primitive("Molten pool",PrimitiveType.Cylinder,lava,Vector3.up*.035f,new Vector3(7,.045f,7),molten,false);
                for(int i=0;i<9;i++)
                {
                    float t=i/8f;Vector3 p=Vector3.Lerp(AimPoint,target+Vector3.up*.3f,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*1.5f;
                    LumiFactory.Primitive("Lava jet",PrimitiveType.Sphere,lava,p-target,Vector3.one*(.3f+t*.55f),molten,false);
                }
                lava.gameObject.AddComponent<LumiBossHazard>().Initialize(game,3.5f,2,4.5f,false,true);
            }
            yield return new WaitForSeconds(.4f);casting=false;
        }
        public static GameObject Circle(Vector3 point,float radius,Color color,string name)
        {
            Transform root=new GameObject(name).transform;root.position=point;
            Vector3[] points=new Vector3[65];for(int i=0;i<65;i++){float a=i*Mathf.PI/32;points[i]=new Vector3(Mathf.Cos(a)*radius,.035f,Mathf.Sin(a)*radius);}
            Line(root,points,color,.12f);return root.gameObject;
        }
        private static void CubeEdges(Transform root,float size,Color color)
        {
            for(int axis=0;axis<3;axis++)for(int a=-1;a<=1;a+=2)for(int b=-1;b<=1;b+=2)
            {
                Vector3 start=new Vector3(a,b,-1)*size*.5f,end=new Vector3(a,b,1)*size*.5f;
                if(axis==1){start=new Vector3(start.z,start.x,start.y);end=new Vector3(end.z,end.x,end.y);}
                if(axis==2){start=new Vector3(start.y,start.z,start.x);end=new Vector3(end.y,end.z,end.x);}
                Line(root,new[]{start,end},color,.1f);
            }
        }
        private static void Line(Transform parent,Vector3[] points,Color color,float width)
        {
            GameObject obj=LumiFactory.WorldObject("Chakra outline",parent,Vector3.zero);LineRenderer line=obj.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=points.Length;line.SetPositions(points);line.widthMultiplier=width;
            line.sharedMaterial=LumiFactory.Material("Kage chakra line"+color,color,true);line.startColor=line.endColor=color;
        }
        public void TakeDamage(int amount,Vector3 point)
        {
            if(!IsAlive || !game.IsPlaying)return;health=Mathf.Max(0,health-amount);engaged=true;flash.Flash(Color.red);game.Audio.Play("hit",.6f);game.SpawnImpact(point,new Color(1,.7f,.25f));
            if(health==0){StopAllCoroutines();controller.enabled=false;game.BossDefeated(this);LumiChakraVisual.Burst(transform.parent,AimPoint,true,2);Destroy(gameObject,.5f);}
        }
    }
    public sealed class LumiBossHazard:MonoBehaviour
    {
        private LumiGame game;private float radius,life,nextHit;private int damage;private bool cube,repeat;private Vector3 scale;
        public void Initialize(LumiGame owner,float range,int hitDamage,float duration,bool box,bool repeated=false){game=owner;radius=range;damage=hitDamage;life=duration;cube=box;repeat=repeated;scale=transform.localScale;transform.localScale=scale*.1f;}
        private void Update()
        {
            if(game==null){Destroy(gameObject);return;}if(!game.IsPlaying)return;
            transform.localScale=Vector3.Lerp(transform.localScale,scale,Time.deltaTime*12);life-=Time.deltaTime;
            if(life<=0){Destroy(gameObject);return;}
            Vector3 d=game.Player.transform.position-transform.position;bool inside=cube?Mathf.Abs(d.x)<radius && Mathf.Abs(d.z)<radius:new Vector2(d.x,d.z).magnitude<radius;
            if(inside && Time.time>=nextHit){game.Player.TakeDamage(damage,game.Player.transform.position+Vector3.up);nextHit=Time.time+(repeat?1:100);}
            foreach(LumiShadowClone clone in LumiShadowClone.Active.ToArray())if(clone!=null && Vector3.Distance(clone.transform.position,transform.position)<radius+2)clone.TakeDamage(damage,clone.transform.position);
        }
    }
}
