using UnityEngine;
namespace LumiAdventure
{
    public class LumiInfantryMotion:MonoBehaviour
    {
        [SerializeField] private Transform leftLeg,rightLeg,leftKnee,rightKnee,leftFoot,rightFoot,leftToe,rightToe,leftArm,rightArm,leftElbow,rightElbow,leftHand,rightHand,spine,head;
        private Vector3 last,rest;
        private Vector3 leftLegAxis,rightLegAxis,leftShinAxis,rightShinAxis,leftArmAxis,rightArmAxis,leftForearmAxis,rightForearmAxis;
        private Quaternion leftArmRotation,rightArmRotation,leftElbowRotation,rightElbowRotation,spineRotation,headRotation;
        private Quaternion leftLegRootRotation,rightLegRootRotation,leftKneeRootRotation,rightKneeRootRotation;
        private Quaternion leftArmRootRotation,rightArmRootRotation,leftElbowRootRotation,rightElbowRootRotation;
        [SerializeField,Range(.1f,.55f)] private float strideReach=.32f;
        [SerializeField,Range(.04f,.25f)] private float footLift=.13f;
        [SerializeField,Range(4,25)] private float poseResponse=12;
        [SerializeField,Range(.08f,.22f)] private float idleFootSeparation=.19f;
        private float phase,blend,castStart,castDuration,landStart=-10,hitUntil,holdStart;
        private float leftThighLength,leftCalfLength,rightThighLength,rightCalfLength;
        private Vector3 leftFootRest,rightFootRest,moveDirection=Vector3.forward;
        private Quaternion leftFootRotation,rightFootRotation,leftToeRotation,rightToeRotation;
        private Vector3 leftUpperTarget,rightUpperTarget,leftLowerTarget,rightLowerTarget;
        private bool grounded=true,hostile,configured;
        private LumiGame game;
        private LumiTechnique technique;
        private LumiFingerMotion fingers;
        public float SealWeight {get;private set;}
        public bool IsPreparingSeal=>holdingTechnique && Time.time-holdStart<.36f;
        public bool ChargePoseReady=>holdingTechnique && Time.time-holdStart>=.65f;
        public Transform WeaponHand=>rightHand ?? rightArm;
        public Transform ChakraHand=>leftHand ?? leftArm;
        private bool holdingTechnique;
        public float StridePhase=>phase;
        public bool RigConfigured=>configured;
        public bool IsAnimatingTechnique=>holdingTechnique || Time.time<castStart+castDuration;
        public void Configure(LumiGame owner,bool enemy)
        {
            game=owner;hostile=enemy;if(configured)return;
            rest=transform.localPosition;last=transform.parent.position;
            leftLeg=FindBone("Left leg","LeftUpLeg");rightLeg=FindBone("Right leg","RightUpLeg");
            leftKnee=FindBone("Left knee","LeftLeg");rightKnee=FindBone("Right knee","RightLeg");
            leftFoot=FindBone("Left foot","LeftFoot");rightFoot=FindBone("Right foot","RightFoot");
            leftToe=FindBone("Left toes","LeftToes");rightToe=FindBone("Right toes","RightToes");
            leftArm=FindBone("Left arm","LeftArm");rightArm=FindBone("Right arm","RightArm");
            leftElbow=FindBone("Left elbow","LeftForeArm");rightElbow=FindBone("Right elbow","RightForeArm");
            leftHand=FindBone("Left hand","LeftHand");rightHand=FindBone("Right hand","RightHand");
            spine=FindBone("Body pivot","Spine");head=FindBone("Head pivot","Head");
            if(leftLeg==null || rightLeg==null || leftArm==null || rightArm==null)return;
            leftLegRootRotation=Quaternion.Inverse(transform.rotation)*leftLeg.rotation;rightLegRootRotation=Quaternion.Inverse(transform.rotation)*rightLeg.rotation;
            if(leftKnee!=null)leftKneeRootRotation=Quaternion.Inverse(transform.rotation)*leftKnee.rotation;
            if(rightKnee!=null)rightKneeRootRotation=Quaternion.Inverse(transform.rotation)*rightKnee.rotation;
            leftArmRotation=leftArm.localRotation;rightArmRotation=rightArm.localRotation;
            if(leftElbow!=null)leftElbowRotation=leftElbow.localRotation;if(rightElbow!=null)rightElbowRotation=rightElbow.localRotation;
            leftArmRootRotation=Quaternion.Inverse(transform.rotation)*leftArm.rotation;rightArmRootRotation=Quaternion.Inverse(transform.rotation)*rightArm.rotation;
            leftLegAxis=SegmentAxis(leftLeg,leftKnee,Vector3.down);rightLegAxis=SegmentAxis(rightLeg,rightKnee,Vector3.down);
            leftShinAxis=SegmentAxis(leftKnee,leftFoot,Vector3.down);rightShinAxis=SegmentAxis(rightKnee,rightFoot,Vector3.down);
            leftArmAxis=SegmentAxis(leftArm,leftElbow,Vector3.left);rightArmAxis=SegmentAxis(rightArm,rightElbow,Vector3.right);
            leftForearmAxis=SegmentAxis(leftElbow,leftHand,Vector3.down);rightForearmAxis=SegmentAxis(rightElbow,rightHand,Vector3.down);
            if(leftElbow!=null)leftElbowRootRotation=Quaternion.Inverse(transform.rotation)*leftElbow.rotation;
            if(rightElbow!=null)rightElbowRootRotation=Quaternion.Inverse(transform.rotation)*rightElbow.rotation;
            if(spine!=null)spineRotation=spine.localRotation;if(head!=null)headRotation=head.localRotation;configured=true;
            if(leftFoot!=null)leftFootRotation=Quaternion.Inverse(transform.rotation)*leftFoot.rotation;
            if(rightFoot!=null)rightFootRotation=Quaternion.Inverse(transform.rotation)*rightFoot.rotation;
            if(leftToe!=null)leftToeRotation=leftToe.localRotation;if(rightToe!=null)rightToeRotation=rightToe.localRotation;
            if(leftFoot!=null && leftKnee!=null)
            {
                leftFootRest=transform.InverseTransformPoint(leftFoot.position);
                leftThighLength=Vector3.Distance(transform.InverseTransformPoint(leftLeg.position),transform.InverseTransformPoint(leftKnee.position));
                leftCalfLength=Vector3.Distance(transform.InverseTransformPoint(leftKnee.position),leftFootRest);
                leftFootRest.x=-idleFootSeparation*.5f;
            }
            if(rightFoot!=null && rightKnee!=null)
            {
                rightFootRest=transform.InverseTransformPoint(rightFoot.position);
                rightThighLength=Vector3.Distance(transform.InverseTransformPoint(rightLeg.position),transform.InverseTransformPoint(rightKnee.position));
                rightCalfLength=Vector3.Distance(transform.InverseTransformPoint(rightKnee.position),rightFootRest);
                rightFootRest.x=idleFootSeparation*.5f;
            }
            fingers=GetComponent<LumiFingerMotion>()??gameObject.AddComponent<LumiFingerMotion>();fingers.Initialize();
            leftUpperTarget=new Vector3(-.18f,-1,.02f);rightUpperTarget=new Vector3(.18f,-1,.02f);
            leftLowerTarget=new Vector3(-.09f,-1,.10f);rightLowerTarget=new Vector3(.09f,-1,.10f);
            SetSegment(leftArm,leftArmAxis,leftArmRootRotation,leftUpperTarget);SetSegment(rightArm,rightArmAxis,rightArmRootRotation,rightUpperTarget);
            SetSegment(leftElbow,leftForearmAxis,leftElbowRootRotation,leftLowerTarget);SetSegment(rightElbow,rightForearmAxis,rightElbowRootRotation,rightLowerTarget);
        }
        private Transform FindBone(string direct,string suffix)
        {
            string shippuden=suffix switch
            {
                "LeftUpLeg"=>"leg left leg1","RightUpLeg"=>"leg right leg1",
                "LeftLeg"=>"leg left leg2","RightLeg"=>"leg right leg2",
                "LeftFoot"=>"leg left foot","RightFoot"=>"leg right foot",
                "LeftToes"=>"leg left toes","RightToes"=>"leg right toes",
                "LeftArm"=>"arm left arm1","RightArm"=>"arm right arm1",
                "LeftForeArm"=>"arm left arm2","RightForeArm"=>"arm right arm2",
                "LeftHand"=>"arm left hand","RightHand"=>"arm right hand",
                "Spine"=>"body spine1","Head"=>"head head",_=>null
            };
            foreach(Transform bone in GetComponentsInChildren<Transform>(true))
                if(string.Equals(bone.name,direct,System.StringComparison.OrdinalIgnoreCase) ||
                   bone.name.EndsWith("_"+suffix,System.StringComparison.OrdinalIgnoreCase) ||
                   (shippuden!=null && string.Equals(bone.name,shippuden,System.StringComparison.OrdinalIgnoreCase)))return bone;
            return null;
        }
        private void Start(){if(!configured)Configure(null,false);}
        public void AlignFeetToGround(float groundY)
        {
            if((leftFoot==null && leftToe==null) && (rightFoot==null && rightToe==null))return;
            float leftY=leftToe!=null?leftToe.position.y-.025f:leftFoot!=null?leftFoot.position.y-.085f:float.PositiveInfinity;
            float rightY=rightToe!=null?rightToe.position.y-.025f:rightFoot!=null?rightFoot.position.y-.085f:float.PositiveInfinity;
            float estimatedSoleY=Mathf.Min(leftY,rightY);
            float correction=Mathf.Clamp(groundY-estimatedSoleY,-.25f,.25f);
            transform.position+=Vector3.up*correction;
            rest=transform.localPosition;last=transform.parent.position;
        }
        public void SetGrounded(bool value){if(value && !grounded)landStart=Time.time;grounded=value;}
        public void ReactToHit(){hitUntil=Time.time+.32f;}
        public void CastPose(float duration){PlayTechnique(LumiTechnique.BasicAttack,duration);}
        public void PlayTechnique(LumiTechnique move,float duration){holdingTechnique=false;technique=move;castStart=Time.time;castDuration=Mathf.Max(.1f,duration);}
        public void HoldTechnique(LumiTechnique move){technique=move;holdingTechnique=true;holdStart=Time.time;castStart=Time.time;}
        public void CancelTechnique(){holdingTechnique=false;castDuration=0;}
        private void LateUpdate()
        {
            if(!configured || transform.parent==null || Time.deltaTime<=0 || (game!=null && !game.IsPlaying))return;
            Vector3 movement=transform.parent.position-last;movement.y=0;
            if(movement.magnitude>4){last=transform.parent.position;return;}
            float distance=movement.magnitude,speed=distance/Mathf.Max(Time.deltaTime,.0001f);
            float run=Mathf.InverseLerp(1.8f,5.2f,speed);
            blend=Mathf.MoveTowards(blend,grounded && speed>.08f?Mathf.Clamp01(speed/2):0,Time.deltaTime*7);
            if(distance>.0001f)moveDirection=Vector3.Lerp(moveDirection,transform.InverseTransformDirection(movement.normalized),1-Mathf.Exp(-12*Time.deltaTime));
            float previous=phase;phase+=distance/Mathf.Lerp(1.5f,2.6f,run)*Mathf.PI*2;
            float swing=Mathf.Sin(phase)*blend;
            float breath=Mathf.Sin(Time.time*2.1f)*.5f*(1-blend);
            float landing=Mathf.Sin(Mathf.Clamp01((Time.time-landStart)/.22f)*Mathf.PI)*4;
            Vector3 body=new Vector3(blend*7+landing, -swing*4, -swing*2+breath);
            Vector3 leftUpper=new Vector3(-.18f,-1,.02f-swing*.58f);
            Vector3 rightUpper=new Vector3(.18f,-1,.02f+swing*.58f);
            Vector3 leftLower=new Vector3(-.09f,-1,.10f-swing*.40f+blend*.24f);
            Vector3 rightLower=new Vector3(.09f,-1,.10f+swing*.40f+blend*.24f);
            if(!grounded)
            {
                leftUpper=new Vector3(-.35f,-.65f,.4f);rightUpper=new Vector3(.35f,-.65f,.4f);
                leftLower=new Vector3(-.1f,-.25f,.95f);rightLower=new Vector3(.1f,-.25f,.95f);body.x=8;
            }
            transform.localRotation=Quaternion.identity;
            if(IsAnimatingTechnique)
            {
                float progress=holdingTechnique?.38f:(Time.time-castStart)/castDuration;
                var pose=LumiTechniquePoses.Sample(technique,progress);
                float elapsed=Time.time-castStart;
                float hold=technique==LumiTechnique.Rasengan?.62f:technique==LumiTechnique.BasicAttack?0f:.36f;
                float action=holdingTechnique?Mathf.SmoothStep(0,1,(Time.time-holdStart)/.24f):1f-Mathf.SmoothStep(0,1,Mathf.InverseLerp(hold,castDuration,elapsed));
                if(technique==LumiTechnique.BasicAttack)
                {
                    float throwWeight=Mathf.Sin(Mathf.Clamp01(progress/.7f)*Mathf.PI);
                    rightUpper=Vector3.Lerp(rightUpper,new Vector3(.20f,-.1f,.98f),throwWeight);
                    rightLower=Vector3.Lerp(rightLower,new Vector3(.02f,-.1f,1),throwWeight);
                }
                else if(technique==LumiTechnique.Rasengan)
                {
                    // Side charge, then a forward palm thrust and a smooth return to relaxed idle.
                    float thrust=holdingTechnique?0:Mathf.SmoothStep(0,1,Mathf.InverseLerp(.12f,.38f,progress));
                    leftUpper=Vector3.Lerp(leftUpper,Vector3.Lerp(new Vector3(-.95f,-.12f,.28f),new Vector3(-.12f,-.12f,.98f),thrust),action);
                    leftLower=Vector3.Lerp(leftLower,Vector3.Lerp(new Vector3(-.85f,.12f,.52f),new Vector3(-.04f,.04f,1),thrust),action);
                    rightUpper=Vector3.Lerp(rightUpper,new Vector3(.08f,-.88f,.42f),action*.5f);
                    rightLower=Vector3.Lerp(rightLower,new Vector3(-.28f,-.2f,.95f),action*.5f);
                    body=Vector3.Lerp(body,new Vector3(thrust*15,-12+thrust*24,0),action);
                }
                else if(technique==LumiTechnique.Rasenshuriken)
                {
                    // Palm over the head while held; release swings the same hand forward to throw.
                    float throwBlend=holdingTechnique?0:Mathf.SmoothStep(0,1,Mathf.InverseLerp(.18f,.5f,progress));
                    leftUpper=Vector3.Lerp(leftUpper,Vector3.Lerp(new Vector3(-.22f,.96f,.12f),new Vector3(-.12f,-.08f,.98f),throwBlend),action);
                    leftLower=Vector3.Lerp(leftLower,Vector3.Lerp(new Vector3(.08f,.99f,.06f),new Vector3(-.02f,-.24f,.97f),throwBlend),action);
                    rightUpper=Vector3.Lerp(rightUpper,new Vector3(.16f,-.72f,.68f),action*.6f);
                    rightLower=Vector3.Lerp(rightLower,new Vector3(-.28f,.2f,.94f),action*.6f);
                    body=Vector3.Lerp(body,new Vector3(-7+throwBlend*26,-14+throwBlend*35,0),action);
                }
                else if(hostile && (int)technique>=4)
                {
                    leftUpper=Vector3.Lerp(leftUpper,Quaternion.Euler(pose.leftArm)*Vector3.down,action);
                    rightUpper=Vector3.Lerp(rightUpper,Quaternion.Euler(pose.rightArm)*Vector3.down,action);
                    leftLower=Vector3.Lerp(leftLower,Quaternion.Euler(pose.leftArm+pose.leftElbow)*Vector3.down,action);
                    rightLower=Vector3.Lerp(rightLower,Quaternion.Euler(pose.rightArm+pose.rightElbow)*Vector3.down,action);
                    body=Vector3.Lerp(body,pose.body,action);
                }
                else
                {
                    // Hand seals use bone-axis directions instead of rig-dependent Euler offsets.
                    float seal=Mathf.Sin(Mathf.Clamp01(progress/.85f)*Mathf.PI);
                    leftUpper=Vector3.Lerp(leftUpper,new Vector3(-.32f,-.78f,.54f),seal);
                    rightUpper=Vector3.Lerp(rightUpper,new Vector3(.32f,-.78f,.54f),seal);
                    leftLower=Vector3.Lerp(leftLower,new Vector3(.58f,.42f,.70f),seal);
                    rightLower=Vector3.Lerp(rightLower,new Vector3(-.58f,.42f,.70f),seal);
                    body=Vector3.Lerp(body,pose.body,seal);
                }
            }
            if(Time.time<hitUntil)body+=new Vector3(-12,0,5)*Mathf.Clamp01((hitUntil-Time.time)/.32f);
            SealWeight=0;
            if(!hostile && IsAnimatingTechnique){float elapsed=holdingTechnique?Time.time-holdStart:Time.time-castStart;
                bool chakra=technique==LumiTechnique.Rasengan||technique==LumiTechnique.Rasenshuriken;
                if(holdingTechnique)SealWeight=Mathf.SmoothStep(0,1,elapsed/.10f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.32f,.58f,elapsed)));
                else if(technique==LumiTechnique.ShadowClone)SealWeight=Mathf.Sin(Mathf.Clamp01(elapsed/castDuration)*Mathf.PI);
                if(SealWeight>0){Vector3 center=(transform.InverseTransformPoint(leftArm.position)+transform.InverseTransformPoint(rightArm.position))*.5f+new Vector3(0,-.14f,.26f);SealArm(leftArm,leftElbow,leftHand,center+Vector3.left*.018f,-1,ref leftUpper,ref leftLower);SealArm(rightArm,rightElbow,rightHand,center+Vector3.right*.018f,1,ref rightUpper,ref rightLower);body=Vector3.Lerp(body,new Vector3(3,0,0),SealWeight);}
            }
            float response=1-Mathf.Exp(-poseResponse*Time.deltaTime);
            if(spine!=null)spine.localRotation=Quaternion.Slerp(spine.localRotation,spineRotation*Quaternion.Euler(body),response);
            if(head!=null)head.localRotation=Quaternion.Slerp(head.localRotation,headRotation*Quaternion.Euler(-body.x*.35f,body.y*-.25f,0),response);
            leftUpperTarget=Vector3.Lerp(leftUpperTarget,leftUpper,response);rightUpperTarget=Vector3.Lerp(rightUpperTarget,rightUpper,response);
            leftLowerTarget=Vector3.Lerp(leftLowerTarget,leftLower,response);rightLowerTarget=Vector3.Lerp(rightLowerTarget,rightLower,response);
            SetSegment(leftArm,leftArmAxis,leftArmRootRotation,leftUpperTarget);SetSegment(rightArm,rightArmAxis,rightArmRootRotation,rightUpperTarget);
            SetSegment(leftElbow,leftForearmAxis,leftElbowRootRotation,leftLowerTarget);SetSegment(rightElbow,rightForearmAxis,rightElbowRootRotation,rightLowerTarget);
            AnimateLeg(leftLeg,leftKnee,leftFoot,leftToe,leftLegAxis,leftShinAxis,leftLegRootRotation,leftKneeRootRotation,leftFootRotation,leftToeRotation,leftFootRest,leftThighLength,leftCalfLength,phase,run);
            AnimateLeg(rightLeg,rightKnee,rightFoot,rightToe,rightLegAxis,rightShinAxis,rightLegRootRotation,rightKneeRootRotation,rightFootRotation,rightToeRotation,rightFootRest,rightThighLength,rightCalfLength,phase+Mathf.PI,run);
            // Never lift the visual root: the CharacterController stays planted and the feet
            // retain their imported height during locomotion and every jutsu pose.
            transform.localPosition=rest;
            if(grounded && game!=null && game.IsPlaying && Mathf.FloorToInt(phase/Mathf.PI)!=Mathf.FloorToInt(previous/Mathf.PI))
                if(!hostile || (game.Player!=null && Vector3.Distance(game.Player.transform.position,transform.position)<8))game.Audio.Play("step",hostile?.065f:.25f);
            if(fingers!=null){fingers.PosePalms(transform,SealWeight,holdingTechnique,technique==LumiTechnique.Rasenshuriken,response);fingers.Apply(holdingTechnique?.42f:1f,SealWeight,response);}
            last=transform.parent.position;
        }
        private void SealArm(Transform upper,Transform elbow,Transform hand,Vector3 target,float side,ref Vector3 upperDirection,ref Vector3 lowerDirection){
            if(upper==null||elbow==null||hand==null)return;Vector3 start=transform.InverseTransformPoint(upper.position),mid=transform.InverseTransformPoint(elbow.position),end=transform.InverseTransformPoint(hand.position);float a=Vector3.Distance(start,mid),b=Vector3.Distance(mid,end);Vector3 delta=target-start;float distance=Mathf.Clamp(delta.magnitude,Mathf.Abs(a-b)+.001f,a+b-.001f);Vector3 direction=delta.normalized;float along=(a*a-b*b+distance*distance)/(2*distance);float bend=Mathf.Sqrt(Mathf.Max(0,a*a-along*along));Vector3 pole=Vector3.ProjectOnPlane(new Vector3(side,-.6f,0),direction).normalized;Vector3 joint=start+direction*along+pole*bend;
            upperDirection=Vector3.Lerp(upperDirection,(joint-start).normalized,SealWeight);lowerDirection=Vector3.Lerp(lowerDirection,(target-joint).normalized,SealWeight);
        }
        private Vector3 SegmentAxis(Transform start,Transform end,Vector3 fallback)
        {
            if(start==null || end==null)return fallback;
            Vector3 axis=transform.InverseTransformDirection(end.position-start.position);
            return axis.sqrMagnitude>.0001f?axis.normalized:fallback;
        }
        private void SetSegment(Transform bone,Vector3 bindAxis,Quaternion bindRotationRoot,Vector3 targetAxis)
        {
            if(bone==null || targetAxis.sqrMagnitude<.0001f)return;
            Quaternion delta=Quaternion.FromToRotation(transform.TransformDirection(bindAxis),transform.TransformDirection(targetAxis.normalized));
            bone.rotation=delta*(transform.rotation*bindRotationRoot);
        }
        private void AnimateLeg(Transform thigh,Transform knee,Transform foot,Transform toe,Vector3 thighAxis,Vector3 calfAxis,Quaternion thighRest,Quaternion calfRest,Quaternion footRestRotation,Quaternion toeRest,Vector3 footAnchor,float upperLength,float lowerLength,float cycle,float run)
        {
            float step=Mathf.Sin(cycle)*blend,lift=Mathf.Max(0,Mathf.Cos(cycle))*blend;
            if(knee==null || foot==null || upperLength<=0 || lowerLength<=0)
            {SetSegment(thigh,thighAxis,thighRest,thighAxis+moveDirection*step*.55f);return;}
            Vector3 target=footAnchor+moveDirection*step*strideReach*Mathf.Lerp(.65f,1.1f,run)+Vector3.up*lift*footLift;
            if(!grounded)target+=Vector3.up*.16f+Vector3.back*.12f;
            Vector3 hip=transform.InverseTransformPoint(thigh.position),delta=target-hip;
            float length=Mathf.Clamp(delta.magnitude,Mathf.Abs(upperLength-lowerLength)+.001f,upperLength+lowerLength-.001f);
            Vector3 direction=delta.normalized;
            float along=(upperLength*upperLength-lowerLength*lowerLength+length*length)/(2*length);
            float outwards=Mathf.Sqrt(Mathf.Max(0,upperLength*upperLength-along*along));
            Vector3 pole=Vector3.ProjectOnPlane(Vector3.forward,direction).normalized;
            Vector3 joint=hip+direction*along+pole*outwards;
            SetSegment(thigh,thighAxis,thighRest,joint-hip);
            SetSegment(knee,calfAxis,calfRest,target-transform.InverseTransformPoint(knee.position));
            foot.rotation=transform.rotation*Quaternion.Euler(lift*-14,0,0)*footRestRotation;
            if(toe!=null)toe.localRotation=toeRest*Quaternion.Euler(lift*8,0,0);
        }
    }
}
