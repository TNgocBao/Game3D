using UnityEngine;
namespace LumiAdventure
{
    public class LumiInfantryMotion:MonoBehaviour
    {
        [SerializeField] private Transform leftLeg,rightLeg,leftArm,rightArm,leftElbow,rightElbow,spine,head;
        private Vector3 last,rest,leftRest,rightRest;
        private Quaternion leftRotation,rightRotation,leftArmRotation,rightArmRotation,leftElbowRotation,rightElbowRotation,spineRotation,headRotation;
        private float phase,blend,castStart,castDuration;
        private bool grounded=true,hostile,configured;
        private LumiGame game;
        private LumiTechnique technique;
        public Transform WeaponHand=>rightArm;
        public Transform ChakraHand=>FindBone("Left hand","LeftHand") ?? leftArm;
        private bool holdingTechnique;
        public float StridePhase=>phase;
        public bool IsAnimatingTechnique=>holdingTechnique || Time.time<castStart+castDuration;
        public void Configure(LumiGame owner,bool enemy)
        {
            game=owner;hostile=enemy;if(configured)return;
            rest=transform.localPosition;last=transform.parent.position;
            leftLeg=FindBone("Left leg","LeftUpLeg");rightLeg=FindBone("Right leg","RightUpLeg");
            leftArm=FindBone("Left arm","LeftArm");rightArm=FindBone("Right arm","RightArm");
            leftElbow=FindBone("Left elbow","LeftForeArm");rightElbow=FindBone("Right elbow","RightForeArm");
            spine=FindBone("Body pivot","Spine");head=FindBone("Head pivot","Head");
            if(leftLeg==null || rightLeg==null || leftArm==null || rightArm==null)return;
            leftRest=leftLeg.localPosition;rightRest=rightLeg.localPosition;leftRotation=leftLeg.localRotation;rightRotation=rightLeg.localRotation;
            leftArmRotation=leftArm.localRotation;rightArmRotation=rightArm.localRotation;
            if(leftElbow!=null)leftElbowRotation=leftElbow.localRotation;if(rightElbow!=null)rightElbowRotation=rightElbow.localRotation;
            if(spine!=null)spineRotation=spine.localRotation;if(head!=null)headRotation=head.localRotation;configured=true;
        }
        private Transform FindBone(string direct,string suffix)
        {foreach(Transform bone in GetComponentsInChildren<Transform>())if(bone.name==direct || bone.name.EndsWith("_"+suffix))return bone;return null;}
        private void Start(){if(!configured)Configure(null,false);}
        public void SetGrounded(bool value){grounded=value;}
        public void CastPose(float duration){PlayTechnique(LumiTechnique.BasicAttack,duration);}
        public void PlayTechnique(LumiTechnique move,float duration){holdingTechnique=false;technique=move;castStart=Time.time;castDuration=Mathf.Max(.1f,duration);}
        public void HoldTechnique(LumiTechnique move){technique=move;holdingTechnique=true;}
        public void CancelTechnique(){holdingTechnique=false;castDuration=0;}
        private void LateUpdate()
        {
            if(!configured || transform.parent==null || Time.deltaTime<=0 || (game!=null && !game.IsPlaying))return;
            Vector3 movement=transform.parent.position-last;movement.y=0;
            if(movement.magnitude>4){last=transform.parent.position;return;}
            float distance=movement.magnitude;blend=Mathf.MoveTowards(blend,grounded && distance>.001f?1:0,Time.deltaTime*12);
            float previous=phase;phase+=distance/1.35f*Mathf.PI*2;Vector3 local=transform.InverseTransformDirection(movement.normalized);float swing=Mathf.Sin(phase)*blend;
            PoseLeg(leftLeg,leftRest,leftRotation,local,swing,phase);PoseLeg(rightLeg,rightRest,rightRotation,local,-swing,phase+Mathf.PI);
            leftArm.localRotation=leftArmRotation*Quaternion.Euler(-swing*18,0,-blend*8);rightArm.localRotation=rightArmRotation*Quaternion.Euler(swing*18,0,blend*8);
            if(leftElbow!=null)leftElbow.localRotation=leftElbowRotation*Quaternion.Euler(-blend*18,0,0);if(rightElbow!=null)rightElbow.localRotation=rightElbowRotation*Quaternion.Euler(-blend*18,0,0);
            if(spine!=null)spine.localRotation=spineRotation*Quaternion.Euler(blend*8,0,-swing*2);if(head!=null)head.localRotation=headRotation;
            transform.localRotation=Quaternion.identity;
            float bob=Mathf.Abs(Mathf.Cos(phase))*.035f*blend+Mathf.Sin(Time.time*2.5f)*.008f*(1-blend);
            if(IsAnimatingTechnique)
            {
                var pose=LumiTechniquePoses.Sample(technique,holdingTechnique?.38f:(Time.time-castStart)/castDuration);
                leftArm.localRotation=leftArmRotation*Quaternion.Euler(pose.leftArm);rightArm.localRotation=rightArmRotation*Quaternion.Euler(pose.rightArm);
                if(leftElbow!=null)leftElbow.localRotation=leftElbowRotation*Quaternion.Euler(pose.leftElbow);if(rightElbow!=null)rightElbow.localRotation=rightElbowRotation*Quaternion.Euler(pose.rightElbow);
                if(spine!=null)spine.localRotation=spineRotation*Quaternion.Euler(pose.body);else transform.localRotation=Quaternion.Euler(pose.body);
                if(head!=null)head.localRotation=headRotation*Quaternion.Euler(pose.head);
                leftLeg.localRotation*=Quaternion.Euler(pose.leftLeg);rightLeg.localRotation*=Quaternion.Euler(pose.rightLeg);bob+=pose.lift;
            }
            transform.localPosition=rest+Vector3.up*bob;
            if(grounded && game!=null && game.IsPlaying && Mathf.FloorToInt(phase/Mathf.PI)!=Mathf.FloorToInt(previous/Mathf.PI))
                if(!hostile || (game.Player!=null && Vector3.Distance(game.Player.transform.position,transform.position)<8))game.Audio.Play("step",hostile?.065f:.25f);
            last=transform.parent.position;
        }
        private void PoseLeg(Transform leg,Vector3 origin,Quaternion rotation,Vector3 direction,float swing,float timing)
        {
            leg.localPosition=origin;
            if(leg.name.Contains(" "))leg.localPosition+=direction*swing*.12f+Vector3.up*Mathf.Max(0,Mathf.Sin(timing))*.075f*blend;
            leg.localRotation=rotation*Quaternion.Euler(-swing*direction.z*28,0,swing*direction.x*28);
        }
    }
}
