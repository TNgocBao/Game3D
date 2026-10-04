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
        private GameObject castWarning,castAura;
        private int skillSequence;
        private readonly System.Collections.Generic.List<GameObject> releasedEffects=new System.Collections.Generic.List<GameObject>();
        private Vector3 home;
        private int village,health,maxHealth;
        private float nextSkill,nextMelee,gravity;
        private bool casting,engaged;
        private LumiCombatTactics tactics;
        private Component combatTarget;
        private float healing;
        public void Heal(float amount){if(!IsAlive)return;healing+=amount;int whole=Mathf.FloorToInt(healing);healing-=whole;health=Mathf.Min(maxHealth,health+whole);}
        public int MaxHealth=>maxHealth;
        public bool SecondPhaseUnlocked {get;private set;}
        private float MoveSpeed=>2.1f+(village-1)*.3f;
        private int ScaleDamage(int value)=>Mathf.Max(1,Mathf.RoundToInt(value*(1+.15f*(village-1))));
        public bool IsAlive=>health>0;
        public float HealthFraction=>health/(float)maxHealth;
        public string BossName=>Names[village-1];
        public string Element=>Elements[village-1];
        public Vector3 AimPoint=>transform.position+Vector3.up*1.4f;
        public bool Engaged=>engaged && IsAlive;
        public void Initialize(LumiGame owner,int level)
        {
            game=owner;village=level;home=transform.position;maxHealth=(72+level*12)*2;health=maxHealth;
            controller=GetComponent<CharacterController>();controller.height=2.4f;controller.radius=.55f;controller.center=Vector3.up*1.2f;
            GameObject model=LumiBossArt.Create(transform,village);motion=model.GetComponent<LumiInfantryMotion>();if(motion!=null)motion.Configure(game,true);
            LumiHealingAltar.Create(game,transform.parent,home);tactics=gameObject.AddComponent<LumiCombatTactics>();tactics.Initialize(game,controller);
            flash=gameObject.AddComponent<LumiHitFlash>();nextSkill=Time.time+2;
        }
        private void Update()
        {
            if(!IsAlive || game==null || !game.IsPlaying || !game.Player.IsAlive)return;
            if(tactics==null){tactics=GetComponent<LumiCombatTactics>()??gameObject.AddComponent<LumiCombatTactics>();tactics.Initialize(game,controller);}
            LumiPlayer player=game.Player;combatTarget=LumiCombatTactics.IsContested(player,transform.position)?player:LumiCombatTactics.SelectTarget(player,transform.position,25,true);
            Vector3 delta=(combatTarget!=null?combatTarget.transform.position:player.transform.position)-transform.position;delta.y=0;
            if(!engaged && delta.magnitude<22){engaged=true;game.ShowToast(BossName+" — "+Element,new Color(1,.7f,.25f));nextSkill=Time.time+1.5f;}
            if(!engaged)return;
            if(tactics.Tick(HealthFraction,MoveSpeed,amount=>Heal(maxHealth*amount))){if(casting){StopAllCoroutines();casting=false;motion?.CancelTechnique();ClearCast();}ApplyGravity();return;}
            if(!casting && combatTarget!=null)
            {
                if(delta.sqrMagnitude>.1f)transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(delta),360*Time.deltaTime);
                if(Vector3.Distance(player.transform.position,home)<25 && delta.magnitude>3 && combatTarget!=null)controller.Move(delta.normalized*(MoveSpeed)*Time.deltaTime);
                if(delta.magnitude<2.1f && Time.time>nextMelee && combatTarget!=null)
                {nextMelee=Time.time+1.8f;motion?.PlayTechnique(LumiTechnique.BasicAttack,.45f);((ILumiDamageable)combatTarget).TakeDamage(ScaleDamage(2),combatTarget.transform.position+Vector3.up);}
                if(Time.time>=nextSkill && delta.magnitude<25 && combatTarget!=null)
                {nextSkill=Time.time+(SecondPhaseUnlocked?4.2f-(village-1)*.2f:5.8f-(village-1)*.25f);StartCoroutine(ExclusiveSkill());}
            }
            ApplyGravity();
        }
        private void ApplyGravity()
        {
            if(controller.isGrounded && gravity<0)gravity=-2;gravity-=22*Time.deltaTime;controller.Move(Vector3.up*gravity*Time.deltaTime);motion?.SetGrounded(controller.isGrounded);
        }
        private void ClearCast(){if(castWarning!=null)Destroy(castWarning);if(castAura!=null)Destroy(castAura);}
        private IEnumerator ExclusiveSkill()
        {
            bool second=SecondPhaseUnlocked && (++skillSequence%2==1);
            casting=true;motion?.PlayTechnique(second?(LumiTechnique)((int)LumiTechnique.WoodGolem+village-1):(LumiTechnique)((int)LumiTechnique.WoodRelease+village-1),2.3f);
            Component victim=combatTarget;Vector3 target=victim!=null?victim.transform.position:game.Player.transform.position;target.y=.08f;
            Color color=village==1?new Color(.3f,.9f,.36f):village==2?new Color(1,.73f,.25f):village==3?new Color(.7f,.87f,1):village==4?new Color(.3f,.75f,1):second?new Color(.5f,.85f,.65f):new Color(1,.3f,.07f);
            float range=village==3?(second?6.8f:1.8f):second?5.2f:3.5f;
            castWarning=Circle(target,range,color,"Boss attack warning");castWarning.transform.SetParent(transform.parent,true);
            game.Audio.Play(village==4?"chakra":"wind",.7f);yield return new WaitForSeconds(1.2f);
            if(!IsAlive || !game.Player.IsAlive){ClearCast();casting=false;yield break;}Destroy(castWarning);
            string key=new[]{"Hashirama","Gaara","Onoki","Raikage","Mei"}[village-1]+(second?"_Skill2":"_Skill1");
            GameObject prefab=Resources.Load<GameObject>("LumiBosses/"+key);
            if(prefab==null){Debug.LogError("Missing boss skill prefab "+key);casting=false;yield break;}
            GameObject effect=Instantiate(prefab,transform.parent);effect.name=key+" active";releasedEffects.RemoveAll(item=>item==null);releasedEffects.Add(effect);effect.transform.position=target;
            float duration=village==5?5:2.6f;effect.transform.localScale=Vector3.one*(second?2.4f:1.8f);
            if(village==3 && second){effect.transform.localScale=new Vector3(1.1f,1.4f,4);Vector3 aim=target-transform.position;aim.y=0;if(aim.sqrMagnitude>.01f)effect.transform.rotation=Quaternion.LookRotation(aim);}
            if(village==2 && second){Vector3 aim=target-transform.position;aim.y=0;if(aim.sqrMagnitude>.01f)effect.transform.rotation=Quaternion.LookRotation(aim);}
            effect.AddComponent<LumiBossSkillVisual>().Initialize(game,village,second,duration);
            if(village==4){castAura=effect;effect.transform.position=transform.position;float elapsed=0;Vector3 direction=(target-transform.position);direction.y=0;direction.Normalize();
                effect.AddComponent<LumiBossHazard>().Initialize(game,second?2.5f:1.8f,ScaleDamage(second?6:4),1.0f,false);
                while(elapsed<.8f && IsAlive){elapsed+=Time.deltaTime;controller.Move(direction*(second?17:13)*Time.deltaTime);effect.transform.position=transform.position;yield return null;}Destroy(effect);
            }else{
                var hazard=effect.AddComponent<LumiBossHazard>();hazard.Initialize(game,range,ScaleDamage(second?5:3),duration,village==3,village==5);
                if(village==3 && second)hazard.BoxHalfExtents=new Vector2(1.87f,6.8f);
                if(village==1)LumiChakraVisual.Burst(transform.parent,target+Vector3.up,true,second?1.8f:1);
            }
            yield return new WaitForSeconds(village==4?.4f:1.1f);casting=false;
        }
        public static GameObject Circle(Vector3 point,float radius,Color color,string name)
        {
            Transform root=new GameObject(name).transform;root.position=point;
            Vector3[] points=new Vector3[65];for(int i=0;i<65;i++){float a=i*Mathf.PI/32;points[i]=new Vector3(Mathf.Cos(a)*radius,.035f,Mathf.Sin(a)*radius);}
            Line(root,points,color,.12f);return root.gameObject;
        }
        private static void Line(Transform parent,Vector3[] points,Color color,float width)
        {
            GameObject obj=LumiFactory.WorldObject("Chakra outline",parent,Vector3.zero);LineRenderer line=obj.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=points.Length;line.SetPositions(points);line.widthMultiplier=width;
            line.sharedMaterial=LumiFactory.Material("Kage chakra line"+color,color,true);line.startColor=line.endColor=color;
        }
        public void TakeDamage(int amount,Vector3 point)
        {
            if(!IsAlive || !game.IsPlaying)return;health=Mathf.Max(0,health-amount);engaged=true;
            if(health>0 && HealthFraction<.5f && !SecondPhaseUnlocked){SecondPhaseUnlocked=true;game.ShowToast(BossName+" — GIAI ĐOẠN 2",new Color(1,.35f,.15f));nextSkill=Mathf.Min(nextSkill,Time.time+1);}
            flash.Flash(Color.red);game.Audio.Play("hit",.6f);game.SpawnImpact(point,new Color(1,.7f,.25f));
            if(health==0){StopAllCoroutines();ClearCast();foreach(var effect in releasedEffects)if(effect!=null)Destroy(effect);controller.enabled=false;game.BossDefeated(this);LumiChakraVisual.Burst(transform.parent,AimPoint,true,2);Destroy(gameObject,.5f);}
        }
    }
    public sealed class LumiBossHazard:MonoBehaviour
    {
        private readonly System.Collections.Generic.Dictionary<LumiShadowClone,float> cloneHits=new System.Collections.Generic.Dictionary<LumiShadowClone,float>();
        public Vector2 BoxHalfExtents;
        private LumiGame game;private float radius,life,nextHit,activeAt;private int damage;private bool cube,repeat;private Vector3 scale;
        public void Initialize(LumiGame owner,float range,int hitDamage,float duration,bool box,bool repeated=false){game=owner;radius=range;damage=hitDamage;life=duration;activeAt=Time.time+.15f;cube=box;BoxHalfExtents=new Vector2(range,range);repeat=repeated;scale=transform.localScale;transform.localScale=scale*.1f;}
        private void Update()
        {
            if(game==null){Destroy(gameObject);return;}if(!game.IsPlaying)return;
            transform.localScale=Vector3.Lerp(transform.localScale,scale,Time.deltaTime*12);life-=Time.deltaTime;
            if(life<=0){Destroy(gameObject);return;}
            if(Time.time<activeAt)return;
            Vector3 d=Quaternion.Inverse(transform.rotation)*(game.Player.transform.position-transform.position);bool inside=cube?Mathf.Abs(d.x)<BoxHalfExtents.x && Mathf.Abs(d.z)<BoxHalfExtents.y:new Vector2(d.x,d.z).magnitude<radius;
            if(inside && Time.time>=nextHit){game.Player.TakeDamage(damage,game.Player.transform.position+Vector3.up);nextHit=Time.time+(repeat?1:100);}
            foreach(LumiShadowClone clone in LumiShadowClone.Active.ToArray()){if(clone==null || !clone.IsAlive)continue;Vector3 c=Quaternion.Inverse(transform.rotation)*(clone.transform.position-transform.position);bool inZone=cube?Mathf.Abs(c.x)<BoxHalfExtents.x && Mathf.Abs(c.z)<BoxHalfExtents.y:new Vector2(c.x,c.z).magnitude<radius;if(inZone && (!cloneHits.TryGetValue(clone,out float next) || Time.time>=next)){clone.TakeDamage(damage,clone.AimPoint);cloneHits[clone]=Time.time+(repeat?1:100);}}
        }
    }
}
