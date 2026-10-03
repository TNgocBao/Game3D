using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LumiAdventure
{
    public sealed class LumiNarutoSkills:MonoBehaviour
    {
        [SerializeField] private LumiPlayer player;
        [SerializeField] private LumiGame game;
        [SerializeField] private LumiInfantryMotion motion;
        [SerializeField] private float[] ready=new float[4];
        [SerializeField,Min(.1f)] private float fullChargeSeconds=2;
        private readonly float[] cooldown={1,5,8,15};
        private readonly Collider[] hits=new Collider[64];
        private readonly HashSet<LumiEnemy> damaged=new HashSet<LumiEnemy>();
        private readonly HashSet<LumiVillageBoss> bossVictims=new HashSet<LumiVillageBoss>();
        private bool casting;
        private int charging=-1;
        private float chargeTime;
        private GameObject chargeOrb;
        public int ChargingAbility=>charging;
        public float ChargeMultiplier=>1+2*Mathf.Clamp01(chargeTime/fullChargeSeconds);
        public void Initialize(LumiPlayer owner,LumiGame session){player=owner;game=session;motion=owner.GetComponentInChildren<LumiInfantryMotion>();}
        public float Remaining(int ability)=>Mathf.Max(0,ready[ability]-Time.time);
        public float Cooldown(int ability)=>cooldown[ability];
        private void Update()
        {
            if(game==null || player==null)return;
            if(!player.IsAlive){CancelCharge();return;}
            if(!game.IsPlaying)return;
            if(Input.GetKeyDown(KeyCode.Q))BeginCharge(1);
            if(Input.GetKeyDown(KeyCode.R))BeginCharge(2);
            if(Input.GetKeyUp(KeyCode.Q))ReleaseCharge(1);
            if(Input.GetKeyUp(KeyCode.R))ReleaseCharge(2);
            if(Input.GetKeyDown(KeyCode.F))Cast(3);
            if(charging>=0 && chargeOrb!=null)
            {
                chargeTime=Mathf.Min(fullChargeSeconds,chargeTime+Time.deltaTime);
                chargeOrb.transform.localScale=Vector3.one*ChargeMultiplier;
                chargeOrb.transform.position=OrbHandPosition(charging);
            }
        }
        private Vector3 OrbHandPosition(int ability)
        {
            Transform hand=player.ChakraHand;
            return (hand!=null?hand.position:transform.position+Vector3.up*1.3f)+transform.forward*.3f+Vector3.up*(ability==2?.22f:.05f);
        }
        private bool CanCast(int ability)=>ability>=0 && ability<4 && game!=null && game.IsPlaying && player!=null && player.IsAlive && !casting && charging<0 && Remaining(ability)<=0;
        private bool Consume(int ability){if(!CanCast(ability))return false;ready[ability]=Time.time+cooldown[ability];return true;}
        public bool BeginCharge(int ability)
        {
            if((ability!=1 && ability!=2) || !CanCast(ability))return false;
            charging=ability;chargeTime=0;motion.HoldTechnique(ability==1?LumiTechnique.Rasengan:LumiTechnique.Rasenshuriken);
            chargeOrb=LumiChakraVisual.Orb(transform.parent,ability==2,ability==1?.35f:.45f);chargeOrb.transform.position=OrbHandPosition(ability);
            game.Audio.Play(ability==1?"chakra":"wind");return true;
        }
        public void ReleaseCharge(int ability)
        {
            if(charging!=ability)return;
            if(game==null || !game.IsPlaying || !player.IsAlive){CancelCharge();return;}
            float power=ChargeMultiplier;GameObject orb=chargeOrb;chargeOrb=null;charging=-1;
            ready[ability]=Time.time+cooldown[ability];casting=true;
            motion.PlayTechnique(ability==1?LumiTechnique.Rasengan:LumiTechnique.Rasenshuriken,ability==1?.85f:.75f);
            if(ability==1)StartCoroutine(Rasengan(orb,power));else StartCoroutine(Rasenshuriken(orb,power));
        }
        private void CancelCharge(){if(chargeOrb!=null)Destroy(chargeOrb);chargeOrb=null;charging=-1;if(motion!=null)motion.CancelTechnique();}
        private void OnDisable(){CancelCharge();}
        public void Cast(int ability)
        {
            if(ability==0){TryBasicAttack();return;}
            if(ability==1 || ability==2){if(BeginCharge(ability))ReleaseCharge(ability);return;}
            if(ability==3 && Consume(3))StartCoroutine(ShadowClones());
        }
        public bool TryBasicAttack()
        {
            if(!Consume(0))return false;
            motion.PlayTechnique(LumiTechnique.BasicAttack,.4f);StartCoroutine(ThrowShuriken());return true;
        }
        private IEnumerator ThrowShuriken()
        {
            yield return new WaitForSeconds(.12f);
            if(game==null || !game.IsPlaying || !player.IsAlive)yield break;
            GameObject star=LumiShurikenArt.Create(transform.parent);star.transform.position=transform.position+Vector3.up*1.15f+transform.forward*.9f;star.transform.rotation=Quaternion.LookRotation(transform.forward);
            LumiProjectile projectile=star.AddComponent<LumiProjectile>();projectile.Initialize(game,transform.forward,22,4,true,transform);projectile.SetSpin(new Vector3(0,0,1800));game.Audio.Play("wind",.3f);
        }
        private IEnumerator Rasengan(GameObject orb,float power)
        {
            float prep=0;while(prep<.25f && player.IsAlive){if(game.IsPlaying){prep+=Time.deltaTime;if(orb!=null)orb.transform.position=OrbHandPosition(1);}yield return null;}
            Vector3 direction=transform.forward;damaged.Clear();bossVictims.Clear();float elapsed=0;
            while(elapsed<.42f && player.IsAlive && orb!=null)
            {
                if(game.IsPlaying){elapsed+=Time.deltaTime;player.Controller.Move(direction*8*Time.deltaTime);orb.transform.position=OrbHandPosition(1);DamageArea(orb.transform.position,1.5f+.35f*(power-1),Mathf.RoundToInt(12*power),damaged,false);}
                yield return null;
            }
            if(orb!=null){LumiChakraVisual.Burst(transform.parent,orb.transform.position,false,power);Destroy(orb);}casting=false;
        }
        private IEnumerator Rasenshuriken(GameObject orb,float power)
        {
            float prep=0;while(prep<.35f && player.IsAlive){if(game.IsPlaying){prep+=Time.deltaTime;if(orb!=null)orb.transform.position=OrbHandPosition(2);}yield return null;}
            if(!player.IsAlive || orb==null){if(orb!=null)Destroy(orb);casting=false;yield break;}
            orb.transform.position=transform.position+Vector3.up*1.4f+transform.forward*(.9f+.25f*(power-1));
            orb.AddComponent<LumiChakraProjectile>().Initialize(game,this,transform,transform.forward,power);casting=false;
        }
        private IEnumerator ShadowClones()
        {
            casting=true;motion.PlayTechnique(LumiTechnique.ShadowClone,.7f);game.Audio.Play("clone");
            float prep=0;while(prep<.4f && player.IsAlive){if(game.IsPlaying)prep+=Time.deltaTime;yield return null;}
            if(player.IsAlive && game.IsPlaying)
            {
                foreach(LumiShadowClone old in new List<LumiShadowClone>(LumiShadowClone.Active))old.Disperse();
                for(int i=0;i<3;i++)for(int attempt=0;attempt<8;attempt++)
                {
                    float angle=(i*120+attempt*25)*Mathf.Deg2Rad;
                    Vector3 candidate=transform.position+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*(2.6f+attempt*.15f);
                    if(!NavMesh.SamplePosition(candidate,out NavMeshHit nav,1.5f,NavMesh.AllAreas) || Physics.CheckSphere(nav.position+Vector3.up*.9f,.5f,~0,QueryTriggerInteraction.Ignore))continue;
                    GameObject clone=new GameObject("Naruto shadow clone",typeof(CharacterController));clone.transform.SetParent(transform.parent,false);clone.transform.position=nav.position;
                    clone.AddComponent<LumiShadowClone>().Initialize(game,player,10);break;
                }
            }
            casting=false;
        }
        public void DamageArea(Vector3 point,float radius,int damage,HashSet<LumiEnemy> victims,bool frontal,bool newAttack=false)
        {
            if(newAttack)bossVictims.Clear();
            int count=Physics.OverlapSphereNonAlloc(point,radius,hits,~0,QueryTriggerInteraction.Ignore);
            for(int i=0;i<count;i++)
            {
                LumiVillageBoss boss=hits[i].GetComponentInParent<LumiVillageBoss>();
                if(boss!=null && boss.IsAlive && !bossVictims.Contains(boss))
                {
                    Vector3 toBoss=boss.transform.position-transform.position;toBoss.y=0;
                    if(!frontal || toBoss.sqrMagnitude<.05f || Vector3.Dot(transform.forward,toBoss.normalized)>.15f)
                        if(!Physics.Linecast(point,boss.AimPoint,out RaycastHit blocker,~0,QueryTriggerInteraction.Ignore) || blocker.collider.GetComponentInParent<LumiVillageBoss>()==boss || blocker.collider.transform.IsChildOf(transform))
                        {bossVictims.Add(boss);boss.TakeDamage(damage,boss.AimPoint);}
                }
                LumiEnemy enemy=hits[i].GetComponentInParent<LumiEnemy>();
                if(enemy==null || !enemy.IsAlive || victims.Contains(enemy))continue;
                Vector3 delta=enemy.transform.position-transform.position;delta.y=0;
                if(frontal && delta.sqrMagnitude>.05f && Vector3.Dot(transform.forward,delta.normalized)<.15f)continue;
                if(Physics.Linecast(point,enemy.AimPoint,out RaycastHit wall,~0,QueryTriggerInteraction.Ignore) && wall.collider.GetComponentInParent<LumiEnemy>()!=enemy && !wall.collider.transform.IsChildOf(transform))continue;
                victims.Add(enemy);enemy.TakeDamage(damage,enemy.AimPoint);
            }
        }
    }
    public sealed class LumiChakraProjectile:MonoBehaviour
    {
        [SerializeField] private LumiGame game;
        private LumiNarutoSkills skills;
        private Vector3 direction;
        private float life=3;
        private float power=1;
        private LumiProjectile trace;
        private readonly HashSet<LumiEnemy> victims=new HashSet<LumiEnemy>();
        public void Initialize(LumiGame owner,LumiNarutoSkills caster,Transform source,Vector3 aim,float chargePower=1)
        {
            game=owner;skills=caster;direction=aim.normalized;power=Mathf.Clamp(chargePower,1,3);
            trace=gameObject.AddComponent<LumiProjectile>();trace.Initialize(owner,direction,16,18,true,source);trace.enabled=false;
        }
        private void Update()
        {
            if(game==null || skills==null){Destroy(gameObject);return;}
            if(game==null || !game.IsPlaying)return;
            float distance=16*Time.deltaTime;
            if(trace.Trace(transform.position,direction,distance,out RaycastHit hit)){Detonate(hit.point);return;}
            transform.position+=direction*distance;life-=Time.deltaTime;
            if(life<=0)Detonate(transform.position);
        }
        private void Detonate(Vector3 point)
        {
            skills.DamageArea(point,3.3f+.55f*(power-1),Mathf.RoundToInt(18*power),victims,false,true);LumiChakraVisual.Burst(transform.parent,point,false,1.8f*power);
            game.Audio.Play("star",.55f);Destroy(gameObject);
        }
    }
}
