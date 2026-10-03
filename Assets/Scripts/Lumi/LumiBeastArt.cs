using UnityEngine;

namespace LumiAdventure
{
    public static class LumiBeastArt
    {
        private static readonly Color[] Palette={new Color(.78f,.61f,.32f),new Color(.1f,.55f,.93f),new Color(.43f,.55f,.53f),new Color(.73f,.16f,.1f),new Color(.87f,.85f,.76f),new Color(.74f,.87f,.71f),new Color(.21f,.59f,.56f),new Color(.62f,.38f,.47f),new Color(.94f,.43f,.1f)};
        public static GameObject Create(Transform parent,LumiEnemyType tier)
        {
            int kind=((int)tier-1)*3+Random.Range(0,3);
            Transform t=LumiFactory.WorldObject("Bijuu "+(kind+1)+" tails",parent,Vector3.zero).transform;
            Material body=LumiFactory.Material("Bijuu skin "+kind,Palette[kind],kind==1);
            Material dark=LumiFactory.Material("Bijuu markings",new Color(.085f,.09f,.14f));
            Material eyes=LumiFactory.Material("Bijuu eyes",new Color(1,.84f,.15f),true);
            Part("Body",PrimitiveType.Sphere,t,new Vector3(0,.91f,-.16f),new Vector3(1.23f,1.08f,1.55f),body);
            Part("Head",PrimitiveType.Sphere,t,new Vector3(0,1.29f,.61f),new Vector3(.95f,.9f,.82f),body);
            if(kind==2)
            {
                Part("Armoured turtle shell",PrimitiveType.Sphere,t,new Vector3(0,1.05f,-.2f),new Vector3(1.48f,1.18f,1.7f),LumiFactory.Material("Isobu shell",new Color(.34f,.4f,.38f)));
                for(int i=0;i<8;i++)Part("Shell spike",PrimitiveType.Cube,t,new Vector3(Mathf.Cos(i*.785f)*.57f,1.55f,Mathf.Sin(i*.785f)*.65f-.2f),new Vector3(.16f,.43f,.16f),dark).transform.localRotation=Quaternion.Euler(0,0,30);
            }
            if(kind==3)Part("Gorilla muzzle",PrimitiveType.Sphere,t,new Vector3(0,1.12f,1),new Vector3(.8f,.47f,.4f),LumiFactory.Material("Son Goku muzzle",new Color(.85f,.66f,.4f)));
            if(kind==4 || kind==8)Part("Long muzzle",PrimitiveType.Sphere,t,new Vector3(0,1.17f,1.05f),new Vector3(.53f,.47f,.82f),body);
            if(kind==5)
            {
                Part("Slug neck",PrimitiveType.Capsule,t,new Vector3(0,1.18f,.35f),new Vector3(.87f,.7f,.84f),body);
                for(int side=-1;side<=1;side+=2)Part("Slug antenna",PrimitiveType.Capsule,t,new Vector3(side*.32f,1.98f,.7f),new Vector3(.15f,.43f,.15f),body).transform.localRotation=Quaternion.Euler(0,0,side*-20);
            }
            if(kind==6)
            {
                Material wing=LumiFactory.Material("Chomei wings",new Color(.59f,.87f,.65f,.7f),false,true);
                for(int side=-1;side<=1;side+=2)for(int j=0;j<3;j++)Part("Insect wing",PrimitiveType.Sphere,t,new Vector3(side*(.8f+j*.1f),1.4f,-.4f+j*.35f),new Vector3(1.8f,.12f,.65f),wing).transform.localRotation=Quaternion.Euler(0,side*(j-1)*25,side*20);
            }
            for(int side=-1;side<=1;side+=2)
            {
                if(kind!=5 && kind!=6)Part("Ear",PrimitiveType.Capsule,t,new Vector3(side*.37f,1.85f,.52f),new Vector3(.23f,.35f,.2f),body).transform.localRotation=Quaternion.Euler(0,0,side*-25);
                if(kind==4 || kind==7)Part("Horn",PrimitiveType.Capsule,t,new Vector3(side*.47f,1.7f,.6f),new Vector3(.17f,.47f,.17f),LumiFactory.Material("Bijuu ivory",new Color(.94f,.87f,.7f))).transform.localRotation=Quaternion.Euler(0,0,side*-40);
                Part("Eye marking",PrimitiveType.Sphere,t,new Vector3(side*.24f,1.4f,.987f),new Vector3(.25f,.23f,.075f),dark);
                Part("Eye",PrimitiveType.Sphere,t,new Vector3(side*.24f,1.43f,1.025f),new Vector3(.11f,.11f,.04f),eyes);
                for(int row=0;row<2;row++)
                {
                    Transform leg=LumiFactory.WorldObject("Beast leg "+side+" "+row,t,new Vector3(side*.43f,.65f,row==0?.44f:-.65f)).transform;
                    Part("Leg",PrimitiveType.Capsule,leg,new Vector3(0,-.24f,0),new Vector3(kind==3?.48f:.29f,.3f,.33f),body);
                    Part("Paw",PrimitiveType.Sphere,leg,new Vector3(0,-.5f,.09f),new Vector3(.38f,.23f,.49f),body);
                }
            }
            for(int i=0;i<kind+1;i++)
            {
                Transform tail=LumiFactory.WorldObject("Tail "+i,t,new Vector3(0,.85f,-.86f)).transform;
                float spread=(i-kind*.5f)*Mathf.Min(24,110f/(kind+1));tail.localRotation=Quaternion.Euler(-25,spread,0);
                for(int j=0;j<4;j++)
                    Part("Tail curve",PrimitiveType.Sphere,tail,new Vector3(0,j*j*.065f,-.22f-j*.32f),new Vector3(.35f-j*.045f,.36f-j*.04f,.62f),body);
            }
            if(tier==LumiEnemyType.Golem)t.localScale=Vector3.one*1.35f;
            t.gameObject.AddComponent<LumiBeastMotion>();return t.gameObject;
        }
        private static GameObject Part(string name,PrimitiveType type,Transform parent,Vector3 position,Vector3 size,Material material)=>LumiFactory.Primitive(name,type,parent,position,size,material,false);
    }
    public sealed class LumiBeastMotion:MonoBehaviour
    {
        private Transform[] legs,tails;private Quaternion[] rotations;private Vector3 previous;private float phase;
        private void Start()
        {
            var l=new System.Collections.Generic.List<Transform>();var t=new System.Collections.Generic.List<Transform>();
            foreach(Transform child in transform){if(child.name.StartsWith("Beast leg"))l.Add(child);if(child.name.StartsWith("Tail "))t.Add(child);}
            legs=l.ToArray();tails=t.ToArray();rotations=new Quaternion[tails.Length];for(int i=0;i<tails.Length;i++)rotations[i]=tails[i].localRotation;previous=transform.parent.position;
        }
        private void LateUpdate()
        {
            if(Time.deltaTime<=0 || legs==null)return;Vector3 delta=transform.parent.position-previous;previous=transform.parent.position;delta.y=0;phase+=delta.magnitude*7;
            float strength=Mathf.Clamp01(delta.magnitude/Mathf.Max(Time.deltaTime,.001f));
            for(int i=0;i<legs.Length;i++)legs[i].localRotation=Quaternion.Euler(Mathf.Sin(phase+(i==0 || i==3?0:Mathf.PI))*28*strength,0,0);
            for(int i=0;i<tails.Length;i++)tails[i].localRotation=rotations[i]*Quaternion.Euler(Mathf.Sin(Time.time*2+i)*8,Mathf.Sin(Time.time*2.5f+i)*7,0);
        }
    }
}
