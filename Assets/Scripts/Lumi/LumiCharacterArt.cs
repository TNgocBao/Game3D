using UnityEngine;

namespace LumiAdventure
{
    public static class LumiCharacterArt
    {
        private static Mesh cone;
        public static Mesh Cone
        {
            get
            {
                if(cone!=null)return cone;
                var vertices=new Vector3[24]; var triangles=new int[24];
                for(int i=0;i<8;i++)
                {
                    float a=i*Mathf.PI/4,b=(i+1)*Mathf.PI/4;
                    vertices[i*3]=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                    vertices[i*3+1]=Vector3.up;
                    vertices[i*3+2]=new Vector3(Mathf.Cos(b),0,Mathf.Sin(b));
                    triangles[i*3]=i*3;triangles[i*3+1]=i*3+1;triangles[i*3+2]=i*3+2;
                }
                cone=new Mesh {name="Faceted pine",vertices=vertices,triangles=triangles};cone.RecalculateNormals();return cone;
            }
        }

        public static GameObject Soldier(Transform parent, Color uniform, bool hostile=false, float scale=1f)
        {
            var root=LumiFactory.WorldObject(hostile?"Enemy infantry":"Vanguard infantry",parent,Vector3.zero);
            Transform t=root.transform;
            Material armor=LumiFactory.Material("Infantry armor",uniform);
            Material dark=LumiFactory.Material("Infantry boots",new Color(.13f,.16f,.18f));
            Material skin=LumiFactory.Material("Infantry face",new Color(.88f,.69f,.48f));
            Material trim=LumiFactory.Material("Infantry webbing",new Color(.36f,.32f,.23f));
            Part("Body",PrimitiveType.Capsule,t,new Vector3(0,.85f,0),new Vector3(.7f,.45f,.52f),armor);
            Part("Vest",PrimitiveType.Cube,t,new Vector3(0,.94f,.23f),new Vector3(.62f,.52f,.15f),trim);
            Part("Face",PrimitiveType.Sphere,t,new Vector3(0,1.48f,.07f),new Vector3(.63f,.63f,.6f),skin);
            Part("Helmet",PrimitiveType.Sphere,t,new Vector3(0,1.68f,-.03f),new Vector3(.76f,.51f,.73f),armor);
            Part("Helmet rim",PrimitiveType.Cube,t,new Vector3(0,1.57f,.3f),new Vector3(.76f,.12f,.2f),armor);
            for(int s=-1;s<=1;s+=2)
            {
                Transform leg=LumiFactory.WorldObject(s<0?"Left leg":"Right leg",t,new Vector3(s*.22f,.58f,0)).transform;
                Part("Knee",PrimitiveType.Sphere,leg,new Vector3(0,-.18f,0),new Vector3(.3f,.34f,.3f),armor);
                Part("Boot",PrimitiveType.Cube,leg,new Vector3(0,-.42f,.06f),new Vector3(.3f,.32f,.43f),dark);
                Transform arm=LumiFactory.WorldObject(s<0?"Left arm":"Right arm",t,new Vector3(s*.43f,1.08f,.03f)).transform;
                Part("Shoulder",PrimitiveType.Sphere,arm,Vector3.zero,new Vector3(.34f,.37f,.36f),armor);
                Part("Forearm",PrimitiveType.Capsule,arm,new Vector3(0,-.12f,.2f),new Vector3(.2f,.21f,.2f),armor);
                Part("Hand",PrimitiveType.Sphere,arm,new Vector3(0,-.14f,.39f),Vector3.one*.23f,skin);
                Part("Eye",PrimitiveType.Sphere,t,new Vector3(s*.12f,1.48f,.352f),Vector3.one*.065f,hostile?LumiFactory.Material("Hostile eyes",Color.red,true):dark);
            }
            Part("Backpack",PrimitiveType.Cube,t,new Vector3(0,.95f,-.35f),new Vector3(.48f,.55f,.25f),trim);
            t.localScale=Vector3.one*scale;
            root.AddComponent<LumiInfantryMotion>();
            return root;
        }
        private static void Part(string n,PrimitiveType p,Transform t,Vector3 v,Vector3 s,Material m)=>LumiFactory.Primitive(n,p,t,v,s,m,false);
    }

}
