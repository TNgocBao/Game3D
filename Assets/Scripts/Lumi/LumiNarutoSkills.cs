using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LumiAdventure
{
    [DefaultExecutionOrder(80)]
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
        [SerializeField,Min(.5f)] private float rasenshurikenBlastRadius=3f;
        [SerializeField,Min(1)] private int rasenshurikenDamage=18;
        [SerializeField,Min(.5f)] private float rasenshurikenBlastDuration=1.4f;
        private bool casting,releasePending;
        private int charging=-1;
        private float chargeTime;
        private GameObject chargeOrb;
        private GameObject handOrb;
        private int handAbility;
        private float handPower;
        public bool TryMeleeThreat(out Vector3 center,out float radius){radius=1.35f*(charging==1?ChargeMultiplier:handPower);center=chargeOrb!=null?chargeOrb.transform.position:handOrb!=null?handOrb.transform.position:transform.position;return charging==1 || (handOrb!=null && handAbility==1);}
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
            if(charging>=0 && motion.ChargePoseReady && !releasePending)chargeTime=Mathf.Min(fullChargeSeconds,chargeTime+Time.deltaTime);
        }
        private void LateUpdate()
        {
            // Follow the palm after the skeleton has posed, avoiding a frame of lag.
            if(game==null || !game.IsPlaying)return;
            if(charging>=0 && chargeOrb!=null)
            {
                chargeOrb.SetActive(!motion.IsPreparingSeal);
                chargeOrb.transform.localScale=Vector3.one*ChargeMultiplier;
                chargeOrb.transform.position=OrbHandPosition(charging,ChargeMultiplier);
            }
            if(handOrb!=null)handOrb.transform.position=OrbHandPosition(handAbility,handPower);
        }
        private Vector3 OrbHandPosition(int ability,float chargeScale=1)
        {
            Transform hand=player.ChakraHand;
            // Move larger chakra farther forward so its growing mesh remains visible and does not
            // intersect Naruto's chest/head. The palm stays close to the rear edge of the orb.
            float baseRadius=ability==2?.45f:.35f;
            float clearance=.1f+baseRadius*Mathf.Clamp(chargeScale,1,3)*.72f;
            Vector3 offset=ability==2?Vector3.up*clearance:transform.TransformDirection(new Vector3(-.65f,.1f,.75f)).normalized*clearance;
            if(charging<0 && ability==1)offset=transform.forward*clearance;
            return (hand!=null?hand.position:transform.position+Vector3.up*1.25f)
                +offset;
        }
        private bool CanCast(int ability)=>ability>=0 && ability<4 && game!=null && game.IsPlaying && player!=null && player.IsAlive && !casting && charging<0 && Remaining(ability)<=0;
        private bool Consume(int ability){if(!CanCast(ability))return false;ready[ability]=Time.time+cooldown[ability];return true;}
        public bool BeginCharge(int ability)
        {
            if((ability!=1 && ability!=2) || !CanCast(ability))return false;
            charging=ability;chargeTime=0;releasePending=false;motion.HoldTechnique(ability==1?LumiTechnique.Rasengan:LumiTechnique.Rasenshuriken);
            chargeOrb=LumiChakraVisual.Orb(transform.parent,ability==2,ability==1?.35f:.45f);chargeOrb.transform.position=OrbHandPosition(ability,1);chargeOrb.SetActive(false);
            game.Audio.Play(ability==1?"chakra":"wind");return true;
        }
        public void ReleaseCharge(int ability)
        {
            if(charging!=ability)return;
            if(game==null || !game.IsPlaying || !player.IsAlive){CancelCharge();return;}
            if(!motion.ChargePoseReady){if(!releasePending){releasePending=true;StartCoroutine(ReleaseAfterSeal(ability));}return;}
            float power=ChargeMultiplier;GameObject orb=chargeOrb;chargeOrb=null;charging=-1;
            if(orb!=null)orb.SetActive(true);releasePending=false;handOrb=orb;handAbility=ability;handPower=power;
            ready[ability]=Time.time+cooldown[ability];casting=true;
            motion.PlayTechnique(ability==1?LumiTechnique.Rasengan:LumiTechnique.Rasenshuriken,ability==1?.85f:.75f);
            if(ability==1)StartCoroutine(Rasengan(orb,power));else StartCoroutine(Rasenshuriken(orb,power));
        }
        private IEnumerator ReleaseAfterSeal(int ability){while(charging==ability && player.IsAlive && (!game.IsPlaying || !motion.ChargePoseReady))yield return null;releasePending=false;if(charging==ability)ReleaseCharge(ability);}
        private void CancelCharge(){if(chargeOrb!=null)Destroy(chargeOrb);chargeOrb=null;charging=-1;releasePending=false;if(motion!=null)motion.CancelTechnique();}
        private void OnDisable(){StopAllCoroutines();CancelCharge();if(handOrb!=null)Destroy(handOrb);handOrb=null;casting=false;}
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
            GameObject star=LumiShurikenArt.Create(transform.parent);
            Transform hand=player.WeaponHand;
            star.transform.position=(hand!=null?hand.position:transform.position+Vector3.up*1.05f)+transform.forward*.3f;
            // The mesh lies in XY; map that plane onto the horizontal XZ plane.
            star.transform.rotation=Quaternion.LookRotation(Vector3.up,transform.forward);
            LumiProjectile projectile=star.AddComponent<LumiProjectile>();Vector3 aim=player.CurrentAimPoint()-star.transform.position;if(aim.sqrMagnitude<.001f)aim=game.CameraRig.ViewCamera.transform.forward;projectile.Initialize(game,aim.normalized,22,4,true,transform);projectile.SetSpin(new Vector3(0,0,1800));game.Audio.Play("wind",.3f);
        }
        private IEnumerator Rasengan(GameObject orb,float power)
        {
            float prep=0;while(prep<.25f && player.IsAlive){if(game.IsPlaying){prep+=Time.deltaTime;if(orb!=null)orb.transform.position=OrbHandPosition(1,power);}yield return null;}
            Vector3 direction=transform.forward;damaged.Clear();bossVictims.Clear();float elapsed=0;
            while(elapsed<.42f && player.IsAlive && orb!=null)
            {
                if(game.IsPlaying){elapsed+=Time.deltaTime;player.Controller.Move(direction*8*Time.deltaTime);orb.transform.position=OrbHandPosition(1,power);DamageArea(orb.transform.position,1.35f*power,Mathf.RoundToInt(12*power),damaged,false);}
                yield return null;
            }
            if(orb!=null){LumiChakraVisual.Burst(transform.parent,orb.transform.position,false,power);Destroy(orb);}handOrb=null;casting=false;
        }
        private IEnumerator Rasenshuriken(GameObject orb,float power)
        {
            float prep=0;while(prep<.35f && player.IsAlive){if(game.IsPlaying){prep+=Time.deltaTime;if(orb!=null)orb.transform.position=OrbHandPosition(2,power);}yield return null;}
            if(!player.IsAlive || orb==null){if(orb!=null)Destroy(orb);handOrb=null;casting=false;yield break;}
            orb.transform.position=OrbHandPosition(2,power);
            Vector3 target=player.CurrentAimPoint();
            Vector3 aim=target-orb.transform.position;
            if(aim.sqrMagnitude<.001f)aim=game.CameraRig.ViewCamera.transform.forward;
            // Aim from the raised hand toward the actual mouse-ray target, including vertical pitch.
            orb.AddComponent<LumiChakraProjectile>().Initialize(game,this,transform,aim.normalized,power);handOrb=null;casting=false;
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
        public void DamageArea(Vector3 point,float radius,int damage,HashSet<LumiEnemy> victims,bool frontal,bool newAttack=false,HashSet<LumiVillageBoss> attackBosses=null)
        {
            HashSet<LumiVillageBoss> bosses=attackBosses??bossVictims;
            if(newAttack)bosses.Clear();
            int count=Physics.OverlapSphereNonAlloc(point,radius,hits,~0,QueryTriggerInteraction.Ignore);
            for(int i=0;i<count;i++)
            {
                LumiVillageBoss boss=hits[i].GetComponentInParent<LumiVillageBoss>();
                if(boss!=null && boss.IsAlive && !bosses.Contains(boss))
                {
                    Vector3 toBoss=boss.transform.position-transform.position;toBoss.y=0;
                    if(!frontal || toBoss.sqrMagnitude<.05f || Vector3.Dot(transform.forward,toBoss.normalized)>.15f)
                        if(!Physics.Linecast(point,boss.AimPoint,out RaycastHit blocker,~0,QueryTriggerInteraction.Ignore) || blocker.collider.GetComponentInParent<LumiVillageBoss>()==boss || blocker.collider.transform.IsChildOf(transform))
                        {bosses.Add(boss);boss.TakeDamage(damage,boss.AimPoint);}
                }
                LumiEnemy enemy=hits[i].GetComponentInParent<LumiEnemy>();
                if(enemy==null || !enemy.IsAlive || victims.Contains(enemy))continue;
                Vector3 delta=enemy.transform.position-transform.position;delta.y=0;
                if(frontal && delta.sqrMagnitude>.05f && Vector3.Dot(transform.forward,delta.normalized)<.15f)continue;
                if(Physics.Linecast(point,enemy.AimPoint,out RaycastHit wall,~0,QueryTriggerInteraction.Ignore) && wall.collider.GetComponentInParent<LumiEnemy>()!=enemy && !wall.collider.transform.IsChildOf(transform))continue;
                victims.Add(enemy);enemy.TakeDamage(damage,enemy.AimPoint);
            }
        }
        public void DetonateRasenshuriken(Vector3 point,float power)
        {
            GameObject blast=new GameObject("Rasenshuriken chakra vortex");blast.transform.SetParent(transform.parent,false);blast.transform.position=point;
            blast.AddComponent<LumiRasenshurikenBlast>().Initialize(game,this,rasenshurikenBlastRadius*power,Mathf.RoundToInt(rasenshurikenDamage*power),rasenshurikenBlastDuration);
            game.Audio.Play("chakraExplosion",.85f);
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
        private bool detonated;
        private float hitRadius;
        private Transform shooter;
        private readonly Collider[] overlaps=new Collider[32];
        public static readonly List<LumiChakraProjectile> Active=new List<LumiChakraProjectile>();
        private void OnEnable(){Active.Add(this);}
        private void OnDisable(){Active.Remove(this);}
        public float CollisionRadius=>hitRadius;
        public Vector3 Direction=>direction;
        public void Initialize(LumiGame owner,LumiNarutoSkills caster,Transform source,Vector3 aim,float chargePower=1)
        {
            game=owner;skills=caster;direction=aim.normalized;power=Mathf.Clamp(chargePower,1,3);hitRadius=.45f*power;shooter=source;
            transform.rotation=Quaternion.LookRotation(direction,Vector3.up);
            trace=gameObject.AddComponent<LumiProjectile>();trace.Initialize(owner,direction,16,18,true,source);trace.enabled=false;
        }
        private void Update()
        {
            if(game==null || skills==null){Destroy(gameObject);return;}
            if(game==null || !game.IsPlaying)return;
            float distance=16*Time.deltaTime;
            // SphereCast misses objects already overlapping the sphere at spawn. Check those too.
            int count=Physics.OverlapSphereNonAlloc(transform.position,hitRadius,overlaps,~0,QueryTriggerInteraction.Ignore);
            for(int i=0;i<count;i++)
            {
                if(overlaps[i]==null || (shooter!=null && overlaps[i].transform.IsChildOf(shooter)))continue;
                Detonate(transform.position);return;
            }
            if(trace.Trace(transform.position,direction,distance,out RaycastHit hit,hitRadius)){Detonate(hit.point);return;}
            transform.position+=direction*distance;life-=Time.deltaTime;
            if(life<=0)Detonate(transform.position);
        }
        private void Detonate(Vector3 point)
        {
            if(detonated)return;detonated=true;
            // A small offset keeps the blast origin outside the wall/target surface for LOS checks.
            skills.DetonateRasenshuriken(point-direction*.08f,power);Destroy(gameObject);
        }
    }
}
