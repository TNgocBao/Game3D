using UnityEngine;

namespace LumiAdventure
{
    public enum LumiTechnique { BasicAttack, Rasengan, Rasenshuriken, ShadowClone, WoodRelease, SandRelease, ParticleRelease, LightningCharge, LavaRelease, WoodGolem, SandTsunami, ParticleBeam, LightningLariat, BoilMist }

    // Authored skeletal poses are interpolated through anticipation, contact and recovery.
    public static class LumiTechniquePoses
    {
        public struct Pose
        {
            public float time,lift;public Vector3 leftArm,rightArm,leftElbow,rightElbow,leftLeg,rightLeg,body,head;
            public Pose(float t,Vector3 l,Vector3 r,Vector3 b,float y=0){time=t;leftArm=l;rightArm=r;body=b;lift=y;leftElbow=rightElbow=leftLeg=rightLeg=head=Vector3.zero;}
        }
        private static readonly Pose[][] poses={
            new[]{new Pose(0,Vector3.zero,Vector3.zero,Vector3.zero),new Pose(.25f,new Vector3(-20,0,-18),new Vector3(25,-35,15),new Vector3(0,-22,0)),new Pose(.5f,new Vector3(10,0,-12),new Vector3(-78,0,0),new Vector3(12,16,0)),new Pose(.7f,new Vector3(0,0,-10),new Vector3(-65,0,0),new Vector3(8,8,0)),new Pose(1,Vector3.zero,Vector3.zero,Vector3.zero)},
            // Rasengan: bring the left palm forward; leave the right arm relaxed by the side.
            new[]{new Pose(0,Vector3.zero,Vector3.zero,Vector3.zero),new Pose(.22f,new Vector3(-34,0,10),Vector3.zero,new Vector3(0,-5,0)),new Pose(.38f,new Vector3(-58,0,14),Vector3.zero,new Vector3(5,-9,0)),new Pose(.58f,new Vector3(-68,0,8),Vector3.zero,new Vector3(10,6,0)),new Pose(.84f,new Vector3(-58,0,8),Vector3.zero,new Vector3(6,4,0)),new Pose(1,Vector3.zero,Vector3.zero,Vector3.zero)},
            new[]{new Pose(0,Vector3.zero,Vector3.zero,Vector3.zero),new Pose(.28f,new Vector3(-145,0,-15),new Vector3(-105,0,25),new Vector3(-12,-18,0)),new Pose(.48f,new Vector3(-155,0,-12),new Vector3(-75,0,30),new Vector3(-17,-25,0)),new Pose(.68f,new Vector3(-82,0,0),new Vector3(15,0,20),new Vector3(22,22,0)),new Pose(.86f,new Vector3(-45,0,-10),new Vector3(10,0,15),new Vector3(10,12,0)),new Pose(1,Vector3.zero,Vector3.zero,Vector3.zero)},
            new[]{new Pose(0,Vector3.zero,Vector3.zero,Vector3.zero),new Pose(.18f,new Vector3(-54,0,-42),new Vector3(-54,0,42),new Vector3(4,0,0)),new Pose(.42f,new Vector3(-65,12,-32),new Vector3(-65,-12,32),new Vector3(9,0,0)),new Pose(.62f,new Vector3(-56,-10,-38),new Vector3(-56,10,38),new Vector3(5,0,0)),new Pose(.8f,new Vector3(-34,0,-62),new Vector3(-34,0,62),Vector3.zero),new Pose(1,Vector3.zero,Vector3.zero,Vector3.zero)},
            new[]{new Pose(0,Vector3.zero,Vector3.zero,Vector3.zero),new Pose(.2f,new Vector3(-65,0,-34),new Vector3(-65,0,34),new Vector3(7,0,0)),new Pose(.6f,new Vector3(-72,10,-28),new Vector3(-72,-10,28),new Vector3(10,0,0)),new Pose(.76f,new Vector3(-50,0,-48),new Vector3(-50,0,48),new Vector3(25,0,0)),new Pose(.88f,new Vector3(-35,0,-58),new Vector3(-35,0,58),new Vector3(18,0,0)),new Pose(1,Vector3.zero,Vector3.zero,Vector3.zero)},
            new[]{new Pose(0,Vector3.zero,Vector3.zero,Vector3.zero),new Pose(.24f,new Vector3(-35,0,-18),new Vector3(-62,-30,40),new Vector3(0,-18,0)),new Pose(.58f,new Vector3(-40,0,-15),new Vector3(-74,0,30),new Vector3(3,-12,0)),new Pose(.76f,new Vector3(-30,0,-18),new Vector3(-86,0,5),new Vector3(12,8,0)),new Pose(.9f,new Vector3(-15,0,-10),new Vector3(-65,0,10),new Vector3(6,4,0)),new Pose(1,Vector3.zero,Vector3.zero,Vector3.zero)},
            new[]{new Pose(0,Vector3.zero,Vector3.zero,Vector3.zero),new Pose(.24f,new Vector3(-62,0,-36),new Vector3(-62,0,36),new Vector3(-8,0,0),.16f),new Pose(.55f,new Vector3(-73,0,-30),new Vector3(-73,0,30),new Vector3(-6,0,0),.3f),new Pose(.76f,new Vector3(-86,0,-15),new Vector3(-86,0,15),new Vector3(10,0,0),.32f),new Pose(.9f,new Vector3(-65,0,-22),new Vector3(-65,0,22),new Vector3(3,0,0),.16f),new Pose(1,Vector3.zero,Vector3.zero,Vector3.zero)},
            new[]{new Pose(0,Vector3.zero,Vector3.zero,Vector3.zero),new Pose(.2f,new Vector3(28,0,-25),new Vector3(-30,0,25),new Vector3(22,-12,0),-.09f),new Pose(.45f,new Vector3(42,0,-20),new Vector3(-58,0,25),new Vector3(32,-18,0),-.12f),new Pose(.68f,new Vector3(35,0,-15),new Vector3(-88,0,5),new Vector3(35,16,0)),new Pose(.9f,new Vector3(15,0,-15),new Vector3(-50,0,15),new Vector3(20,8,0)),new Pose(1,Vector3.zero,Vector3.zero,Vector3.zero)},
            new[]{new Pose(0,Vector3.zero,Vector3.zero,Vector3.zero),new Pose(.22f,new Vector3(-65,0,-35),new Vector3(-65,0,35),new Vector3(-8,0,0)),new Pose(.55f,new Vector3(-62,0,-30),new Vector3(-62,0,30),new Vector3(-14,0,0)),new Pose(.74f,new Vector3(-20,0,-28),new Vector3(-20,0,28),new Vector3(28,0,0)),new Pose(.9f,new Vector3(-12,0,-20),new Vector3(-12,0,20),new Vector3(20,0,0)),new Pose(1,Vector3.zero,Vector3.zero,Vector3.zero)}
        };
        public static Pose Sample(LumiTechnique technique,float time)
        {
            int index=(int)technique;bool second=index>=9;if(second)index-=5;Pose[] frames=poses[index];int next=1;while(next<frames.Length-1 && frames[next].time<time)next++;
            Pose a=frames[next-1],b=frames[next];float blend=Mathf.SmoothStep(0,1,Mathf.InverseLerp(a.time,b.time,time));
            Pose pose=new Pose(time,Vector3.Lerp(a.leftArm,b.leftArm,blend),Vector3.Lerp(a.rightArm,b.rightArm,blend),Vector3.Lerp(a.body,b.body,blend),Mathf.Lerp(a.lift,b.lift,blend));
            // Bent elbows bring the hands together for seals; release straightens them.
            float seal=(technique==LumiTechnique.ShadowClone || technique==LumiTechnique.WoodRelease || technique==LumiTechnique.LavaRelease || technique==LumiTechnique.ParticleRelease)?Mathf.Sin(Mathf.Clamp01(time/.74f)*Mathf.PI)*72:20;
            pose.leftElbow=new Vector3(-seal,0,0);pose.rightElbow=new Vector3(-seal,0,0);pose.head=new Vector3(-pose.body.x*.35f,0,0);
            float crouch=Mathf.Max(0,pose.body.x);pose.leftLeg=new Vector3(-crouch*.45f,0,0);pose.rightLeg=new Vector3(crouch*.3f,0,0);
            if(second){float charge=Mathf.Sin(Mathf.Clamp01(time/.60f)*Mathf.PI);float release=Mathf.Sin(Mathf.Clamp01((time-.48f)/.48f)*Mathf.PI);
                switch(technique){
                case LumiTechnique.WoodGolem:pose.leftArm=new Vector3(-145*charge+45*release,0,-18*charge);pose.rightArm=new Vector3(-145*charge+45*release,0,18*charge);pose.body=new Vector3(30*release,0,0);break;
                case LumiTechnique.SandTsunami:pose.leftArm=new Vector3(-110*charge,0,-40*charge);pose.rightArm=new Vector3(-110*charge,0,40*charge);pose.body=new Vector3(20*release,0,0);break;
                case LumiTechnique.ParticleBeam:pose.leftArm=new Vector3(-85*charge-80*release,0,-20*charge);pose.rightArm=new Vector3(-85*charge-80*release,0,20*charge);pose.body=new Vector3(-10*charge+15*release,0,0);break;
                case LumiTechnique.LightningLariat:pose.leftArm=new Vector3(35*charge,0,-10);pose.rightArm=new Vector3(-30*charge,0,85*release);pose.body=new Vector3(25*charge,30*release,0);break;
                case LumiTechnique.BoilMist:pose.leftArm=new Vector3(-60*charge,0,-30*charge);pose.rightArm=new Vector3(-60*charge,0,30*charge);pose.body=new Vector3(-15*charge+25*release,0,0);break;
                }pose.head=new Vector3(-pose.body.x*.3f,0,0);
            }
            return pose;
        }
    }
}
