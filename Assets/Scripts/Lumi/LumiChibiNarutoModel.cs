using System.Collections.Generic;
using UnityEngine;

namespace LumiAdventure
{
    // Smooth, individually sculpted surfaces are combined into one rigged character mesh.
    public static class LumiChibiNarutoModel
    {
        private static Mesh sphere;
        private static readonly Color[] palette={new Color(1,.77f,.61f),new Color(1,.87f,.48f),new Color(1,.46f,.16f),new Color(.085f,.16f,.29f),new Color(.76f,.79f,.8f),new Color(.045f,.06f,.075f),new Color(.97f,.97f,.94f),new Color(.08f,.52f,.78f),new Color(.3f,.17f,.11f),new Color(.72f,.62f,.47f),new Color(.76f,.2f,.18f),new Color(.95f,.54f,.39f)};
        private sealed class Sculpt
        {
            public GameObject root;public List<Transform> bones=new List<Transform>();public List<Vector3> vertices=new List<Vector3>(),normals=new List<Vector3>();public List<Vector2> uv=new List<Vector2>();public List<int> triangles=new List<int>();public List<BoneWeight> weights=new List<BoneWeight>();
            public Sculpt(Transform parent){root=new GameObject("Naruto chibi • smooth sculpt");root.transform.SetParent(parent,false);}
            public Transform Bone(string name,Transform parent,Vector3 position){var b=new GameObject(name).transform;b.SetParent(parent,false);b.localPosition=position;bones.Add(b);return b;}
            public void Surface(Mesh mesh,Transform bone,Vector3 position,Vector3 size,int color,Quaternion rotation)
            {
                Matrix4x4 matrix=root.transform.worldToLocalMatrix*bone.localToWorldMatrix*Matrix4x4.TRS(position,rotation,size);int start=vertices.Count,index=bones.IndexOf(bone);
                Vector3[] v=mesh.vertices,n=mesh.normals;for(int i=0;i<v.Length;i++){vertices.Add(matrix.MultiplyPoint3x4(v[i]));normals.Add(matrix.inverse.transpose.MultiplyVector(n[i]).normalized);uv.Add(new Vector2((color+.5f)/palette.Length,.5f));weights.Add(new BoneWeight{boneIndex0=index,weight0=1});}
                foreach(int triangle in mesh.triangles)triangles.Add(start+triangle);
            }
            public void Oval(string name,Transform bone,Vector3 p,Vector3 size,int color){Surface(Sphere,bone,p,size,color,Quaternion.identity);}
            public void Curve(Transform bone,Vector3[] points,float width,int color,float taper=1){Surface(Tube(points,width,taper),bone,Vector3.zero,Vector3.one,color,Quaternion.identity);}
            public void Tuft(Transform head,Vector3 start,Vector3 bend,Vector3 end,float width)
            {
                Vector3[] centers=new Vector3[19];for(int i=0;i<centers.Length;i++){float t=i/(float)(centers.Length-1);centers[i]=(1-t)*(1-t)*start+2*(1-t)*t*bend+t*t*end;}
                Surface(Tube(centers,width,.035f,true),head,Vector3.zero,Vector3.one,1,Quaternion.identity);
            }
            public GameObject Finish()
            {
                Texture2D colors=new Texture2D(palette.Length,1,TextureFormat.RGBA32,false);colors.name="Naruto hand painted color palette";colors.filterMode=FilterMode.Point;colors.wrapMode=TextureWrapMode.Clamp;colors.SetPixels(palette);colors.Apply();
                var mesh=new Mesh{name="Naruto smooth chibi rigged sculpt"};mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.boneWeights=weights.ToArray();
                Matrix4x4[] bind=new Matrix4x4[bones.Count];for(int i=0;i<bind.Length;i++)bind[i]=bones[i].worldToLocalMatrix*root.transform.localToWorldMatrix;mesh.bindposes=bind;mesh.RecalculateBounds();
                root.layer=30;var renderer=root.AddComponent<SkinnedMeshRenderer>();renderer.sharedMesh=mesh;renderer.bones=bones.ToArray();renderer.rootBone=bones[0];renderer.localBounds=mesh.bounds;renderer.updateWhenOffscreen=true;
                Shader shader=Resources.Load<Shader>("LumiSoftChibi");renderer.sharedMaterial=new Material(shader!=null?shader:Shader.Find("Standard")){name="Naruto soft painted finish",mainTexture=colors};root.AddComponent<LumiInfantryMotion>();return root;
            }
        }
        private static Mesh Sphere
        {
            get
            {
                if(sphere!=null)return sphere;int rings=24,segments=40;var v=new List<Vector3>();var n=new List<Vector3>();var triangles=new List<int>();
                for(int y=0;y<=rings;y++)for(int x=0;x<=segments;x++){float latitude=y*Mathf.PI/rings,longitude=x*2*Mathf.PI/segments;Vector3 p=new Vector3(Mathf.Sin(latitude)*Mathf.Cos(longitude),Mathf.Cos(latitude),Mathf.Sin(latitude)*Mathf.Sin(longitude));v.Add(p*.5f);n.Add(p);}
                for(int y=0;y<rings;y++)for(int x=0;x<segments;x++){int a=y*(segments+1)+x,b=a+segments+1;triangles.AddRange(new[]{a,a+1,b,a+1,b+1,b});}
                sphere=new Mesh{name="Smooth sculpt surface 48x32"};sphere.SetVertices(v);sphere.SetNormals(n);sphere.SetTriangles(triangles,0);return sphere;
            }
        }
        private static Mesh Tube(Vector3[] points,float radius,float tip,bool tuft=false)
        {
            int rings=points.Length,sides=24;var v=new List<Vector3>();var triangles=new List<int>();
            Vector3 previous=Vector3.right;
            for(int j=0;j<rings;j++)
            {
                Vector3 tangent=(points[Mathf.Min(j+1,rings-1)]-points[Mathf.Max(0,j-1)]).normalized;
                Vector3 side=Vector3.Cross(tangent,Vector3.forward);if(side.sqrMagnitude<.01f)side=Vector3.Cross(tangent,Vector3.up);side.Normalize();if(Vector3.Dot(side,previous)<0)side=-side;previous=side;Vector3 up=Vector3.Cross(tangent,side).normalized;
                float t=j/(float)(rings-1);float r=radius*(tuft?Mathf.Max(.035f,Mathf.Sin((.13f+t*.86f)*Mathf.PI)):Mathf.Lerp(1,tip,t));
                for(int i=0;i<sides;i++){float a=i*2*Mathf.PI/sides;v.Add(points[j]+side*Mathf.Cos(a)*r+up*Mathf.Sin(a)*r*(tuft?.75f:1));}
            }
            for(int j=0;j<rings-1;j++)for(int i=0;i<sides;i++){int a=j*sides+i,b=j*sides+(i+1)%sides,c=a+sides,d=b+sides;triangles.AddRange(new[]{a,b,c,b,d,c});}
            for(int i=1;i<sides-1;i++){triangles.AddRange(new[]{0,i+1,i,(rings-1)*sides,(rings-1)*sides+i,(rings-1)*sides+i+1});}
            var mesh=new Mesh{name="Rounded sculpted strand"};mesh.SetVertices(v);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();return mesh;
        }
        private static Vector3[] Arc(Vector3 center,float radius,float start,float end,int count=30,float yScale=1)
        {Vector3[] points=new Vector3[count];for(int i=0;i<count;i++){float a=Mathf.Lerp(start,end,i/(float)(count-1));points[i]=center+new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*yScale,0);}return points;}
        public static GameObject Create(Transform parent)
        {
            var s=new Sculpt(parent);Transform root=s.Bone("Rig root",s.root.transform,Vector3.zero);
            Transform spine=s.Bone("Body pivot",root,new Vector3(0,.94f,0));Transform head=s.Bone("Head pivot",spine,new Vector3(0,.69f,0));
            s.Oval("Round face",head,Vector3.zero,new Vector3(1.02f,.86f,.88f),0);
            // The face is a continuous surface: separate cheek spheres hid the nose and smile.
            s.Oval("Blond cap",head,new Vector3(0,.27f,-.09f),new Vector3(1.07f,.58f,.94f),1);
            for(int side=-1;side<=1;side+=2)
            {
                s.Oval("Ear",head,new Vector3(side*.5f,-.005f,0),new Vector3(.16f,.26f,.18f),0);s.Oval("Inner ear",head,new Vector3(side*.525f,.005f,.055f),new Vector3(.074f,.16f,.04f),11);
                float x=side*.185f;Vector3 eye=new Vector3(x,.03f,.415f);
                s.Oval("Eye outline",head,eye,new Vector3(.213f,.231f,.025f),8);s.Oval("Eye white",head,eye+Vector3.forward*.008f,new Vector3(.205f,.222f,.027f),6);
                s.Oval("Blue iris",head,eye+new Vector3(-side*.005f,0,.022f),new Vector3(.127f,.163f,.012f),7);s.Oval("Dark pupil",head,eye+new Vector3(-side*.005f,.005f,.030f),new Vector3(.063f,.102f,.009f),5);
                s.Oval("Eye sparkle",head,eye+new Vector3(-.025f,.046f,.035f),new Vector3(.032f,.035f,.006f),6);s.Oval("Eye tiny sparkle",head,eye+new Vector3(.021f,-.035f,.034f),new Vector3(.014f,.018f,.004f),6);
                s.Curve(head,new[]{FacePoint(x-side*.07f,.171f,.009f),FacePoint(x,.189f,.013f),FacePoint(x+side*.075f,.166f,.009f)},.018f,1,.55f);
                for(int i=0;i<3;i++){float y=-.11f-i*.065f;s.Curve(head,new[]{FacePoint(side*.29f,y,.004f),FacePoint(side*.36f,y+(i-1)*.013f,.004f),FacePoint(side*.397f,y+(i-1)*.022f,.004f)},.0035f,8,.6f);}
                s.Tuft(head,new Vector3(side*.4f,.15f,0),new Vector3(side*.47f,.08f,.11f),new Vector3(side*.44f,-.15f,.12f),.078f);
            }
            s.Oval("Button nose",head,new Vector3(0,-.075f,.454f),new Vector3(.075f,.063f,.05f),0);
            s.Curve(head,new[]{FacePoint(-.095f,-.216f,.004f),FacePoint(-.05f,-.233f,.004f),FacePoint(0,-.238f,.004f),FacePoint(.05f,-.233f,.004f),FacePoint(.095f,-.216f,.004f)},.005f,8,.7f);
            // Soft overlapping hair petals, laid down rather than spiking like a crown.
            for(int i=0;i<7;i++)
            {
                float x=(i-3)*.145f;float y=.35f+(.45f-Mathf.Abs(x))*.18f;
                s.Tuft(head,new Vector3(x*.55f,y,-.015f),new Vector3(x*1.03f,y+.17f,.22f),new Vector3(x*1.04f,.305f-Mathf.Abs(x)*.30f,.49f),.125f);
            }
            for(int i=0;i<9;i++)
            {
                float angle=i*Mathf.PI*2/9;Vector3 direction=new Vector3(Mathf.Cos(angle),.8f,Mathf.Sin(angle)).normalized;
                s.Tuft(head,new Vector3(direction.x*.19f,.3f,direction.z*.2f-.1f),new Vector3(direction.x*.5f,.51f,direction.z*.46f-.1f),new Vector3(direction.x*.58f,.37f+(.5f+direction.y)*.19f,direction.z*.55f-.1f),.145f);
            }
            for(int i=0;i<7;i++){float a=(i-3)*.35f;s.Tuft(head,new Vector3(Mathf.Sin(a)*.35f,.13f,-.37f),new Vector3(Mathf.Sin(a)*.47f,-.04f,-.49f),new Vector3(Mathf.Sin(a)*.4f,-.28f,-.34f),.095f);}
            s.Oval("Headband cloth",head,new Vector3(0,.265f,.015f),new Vector3(1.06f,.17f,.91f),3);
            s.Surface(BevelPlate(.59f,.17f,.026f,.035f),head,new Vector3(0,.28f,.455f),Vector3.one,4,Quaternion.identity);
            for(int side=-1;side<=1;side+=2)for(int i=0;i<3;i++)s.Oval("Plate rivet",head,new Vector3(side*.255f,.228f+i*.05f,.476f),Vector3.one*.013f,6);
            Vector3[] spiral=new Vector3[42];for(int i=0;i<spiral.Length;i++){float t=i/(float)(spiral.Length-1),a=t*Mathf.PI*3;float r=.006f+t*.047f;spiral[i]=new Vector3(Mathf.Cos(a)*r,.28f+Mathf.Sin(a)*r,.479f);}s.Curve(head,spiral,.006f,5);
            s.Curve(head,new[]{new Vector3(-.053f,.273f,.480f),new Vector3(-.081f,.23f,.480f),new Vector3(-.018f,.23f,.480f)},.006f,5);
            s.Oval("Headband knot",head,new Vector3(0,.17f,-.471f),new Vector3(.19f,.12f,.09f),3);
            for(int side=-1;side<=1;side+=2)s.Curve(head,new[]{new Vector3(side*.04f,.15f,-.49f),new Vector3(side*.08f,-.01f,-.55f),new Vector3(side*.1f,-.18f,-.5f)},.045f,3,.7f);
            s.Surface(TailoredBody(),spine,Vector3.zero,Vector3.one,2,Quaternion.identity);
            s.Oval("Navy shoulder yoke",spine,new Vector3(0,.19f,-.025f),new Vector3(.66f,.18f,.43f),3);
            s.Oval("Navy waistband",spine,new Vector3(0,-.285f,0),new Vector3(.60f,.065f,.40f),3);
            s.Oval("Ribbed high collar",spine,new Vector3(0,.285f,-.025f),new Vector3(.40f,.14f,.35f),3);
            for(int i=0;i<18;i++){float a=i*Mathf.PI*2/18;s.Curve(spine,new[]{new Vector3(Mathf.Sin(a)*.195f,.23f,Mathf.Cos(a)*.175f-.025f),new Vector3(Mathf.Sin(a)*.20f,.32f,Mathf.Cos(a)*.175f-.025f)},.004f,3);}
            s.Curve(spine,new[]{new Vector3(0,.27f,.177f),new Vector3(0,.18f,.225f),new Vector3(0,-.04f,.25f),new Vector3(0,-.26f,.21f)},.014f,4);
            for(int i=0;i<24;i++){float y=-.24f+i*.021f;s.Oval("Zipper tooth",spine,new Vector3((i%2==0?-.011f:.011f),y,.245f-Mathf.Abs(y)*.1f),new Vector3(.023f,.014f,.014f),4);}
            s.Oval("Zipper tab",spine,new Vector3(0,.192f,.248f),new Vector3(.047f,.071f,.019f),4);
            s.Oval("Back pouch",spine,new Vector3(0,.07f,-.265f),new Vector3(.33f,.36f,.16f),9);s.Oval("Red back spiral",spine,new Vector3(0,.16f,-.345f),new Vector3(.22f,.22f,.045f),10);
            Vector3[] redSpiral=new Vector3[30];for(int i=0;i<30;i++){float t=i/29f,a=t*Mathf.PI*3;redSpiral[i]=new Vector3(Mathf.Cos(a)*t*.065f,.16f+Mathf.Sin(a)*t*.065f,-.374f);}s.Curve(spine,redSpiral,.005f,8);
            for(int side=-1;side<=1;side+=2)
            {
                Transform arm=s.Bone(side<0?"Left arm":"Right arm",spine,new Vector3(side*.32f,.17f,0));Transform elbow=s.Bone(side<0?"Left elbow":"Right elbow",arm,new Vector3(side*.105f,-.185f,0));
                int first=s.vertices.Count;
                Vector3[] sleeve=new Vector3[15];for(int j=0;j<sleeve.Length;j++){float t=j/(float)(sleeve.Length-1);sleeve[j]=new Vector3(side*(.005f+.152f*t),.012f-.352f*t,0);}
                s.Surface(Tube(sleeve,.12f,.87f),arm,Vector3.zero,Vector3.one,2,Quaternion.identity);
                for(int j=first;j<s.vertices.Count;j++){int ring=(j-first)/24;float t=ring/14f,blend=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.35f,.82f,t));s.weights[j]=new BoneWeight{boneIndex0=s.bones.IndexOf(arm),weight0=1-blend,boneIndex1=s.bones.IndexOf(elbow),weight1=blend};}
                s.Curve(elbow,new[]{new Vector3(side*.046f,-.128f,0),new Vector3(side*.056f,-.169f,0)},.112f,2,.97f);
                s.Bone(side<0?"Left hand":"Right hand",elbow,new Vector3(side*.062f,-.232f,.015f));
                s.Surface(BevelPlate(.14f,.105f,.012f,.02f),arm,new Vector3(side*.075f,-.035f,.125f),Vector3.one,4,Quaternion.Euler(0,side*18,0));
                Vector3[] shoulderSpiral=new Vector3[25];for(int j=0;j<25;j++){float t=j/24f,a=t*Mathf.PI*3;shoulderSpiral[j]=new Vector3(side*.075f+Mathf.Cos(a)*t*.032f,-.035f+Mathf.Sin(a)*t*.032f,.148f);}s.Curve(arm,shoulderSpiral,.003f,9);
                s.Oval("Small hand",elbow,new Vector3(side*.062f,-.232f,.015f),new Vector3(.16f,.18f,.125f),0);
                s.Oval("Thumb",elbow,new Vector3(side*.004f,-.233f,.067f),new Vector3(.07f,.109f,.064f),0);
                for(int finger=0;finger<3;finger++)s.Oval("Rounded finger",elbow,new Vector3(side*.053f+(finger-1)*.036f,-.29f,.025f),new Vector3(.044f,.075f,.083f),0);
                Transform leg=s.Bone(side<0?"Left leg":"Right leg",root,new Vector3(side*.175f,.62f,0));
                s.Surface(TailoredTrousers(),leg,Vector3.zero,Vector3.one,3,Quaternion.identity);
                s.Oval("Rolled trouser hem",leg,new Vector3(0,-.28f,.005f),new Vector3(.29f,.04f,.31f),3);
                s.Oval("Ankle",leg,new Vector3(0,-.35f,0),new Vector3(.18f,.135f,.19f),3);
                s.Oval("Sandal cuff",leg,new Vector3(0,-.407f,-.004f),new Vector3(.22f,.102f,.24f),3);
                s.Oval("Ninja sandal",leg,new Vector3(0,-.495f,.055f),new Vector3(.28f,.16f,.38f),3);
                s.Oval("Sandal sole",leg,new Vector3(0,-.558f,.055f),new Vector3(.29f,.035f,.38f),3);
                s.Oval("Open toe skin",leg,new Vector3(0,-.475f,.224f),new Vector3(.228f,.061f,.1f),0);
                for(int toe=0;toe<4;toe++)s.Oval("Toe",leg,new Vector3((toe-1.5f)*.047f,-.472f,.25f),new Vector3(.05f,.06f,.068f),0);
                if(side==1){for(int band=0;band<2;band++)s.Oval("Leg wraps",leg,new Vector3(0,-.12f-band*.085f,0),new Vector3(.305f,.044f,.329f),4);s.Oval("Kunai holster",leg,new Vector3(.166f,-.16f,-.012f),new Vector3(.083f,.215f,.14f),3);}
            }
            return s.Finish();
        }
        private static Vector3 FacePoint(float x,float y,float offset)
        {return new Vector3(x,y,.44f*Mathf.Sqrt(Mathf.Max(.01f,1-x*x/(.51f*.51f)-y*y/(.43f*.43f)))+offset);}
        private static Mesh TailoredTrousers()
        {
            Vector3[] profile={new Vector3(.01f,.045f,.01f),new Vector3(.12f,.03f,.13f),new Vector3(.15f,-.02f,.155f),new Vector3(.145f,-.13f,.16f),new Vector3(.14f,-.23f,.155f),new Vector3(.125f,-.285f,.135f),new Vector3(.01f,-.30f,.01f)};
            return ProfileSurface(profile,"Tailored ninja trousers");
        }
        private static Mesh ProfileSurface(Vector3[] profile,string name)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();const int sides=48;
            for(int j=0;j<profile.Length;j++)for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides;vertices.Add(new Vector3(Mathf.Cos(a)*profile[j].x,profile[j].y,Mathf.Sin(a)*profile[j].z));}
            for(int j=0;j<profile.Length-1;j++)for(int i=0;i<sides;i++){int a=j*sides+i,b=j*sides+(i+1)%sides,c=a+sides,d=b+sides;triangles.AddRange(profile[1].y>profile[0].y?new[]{a,c,b,b,c,d}:new[]{a,b,c,b,d,c});}
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();return mesh;
        }
        private static Mesh TailoredBody()
        {
            Vector3[] profile={new Vector3(.01f,-.30f,.01f),new Vector3(.27f,-.29f,.18f),new Vector3(.31f,-.22f,.215f),new Vector3(.325f,-.08f,.235f),new Vector3(.31f,.08f,.225f),new Vector3(.275f,.18f,.195f),new Vector3(.18f,.235f,.15f),new Vector3(.01f,.25f,.01f)};
            var vertices=new List<Vector3>();var triangles=new List<int>();const int sides=48;
            for(int j=0;j<profile.Length;j++)for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides;vertices.Add(new Vector3(Mathf.Cos(a)*profile[j].x,profile[j].y,Mathf.Sin(a)*profile[j].z));}
            for(int j=0;j<profile.Length-1;j++)for(int i=0;i<sides;i++){int a=j*sides+i,b=j*sides+(i+1)%sides,c=a+sides,d=b+sides;triangles.AddRange(new[]{a,c,b,b,c,d});}
            var mesh=new Mesh{name="Tailored chibi jacket"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();return mesh;
        }
        private static Mesh BevelPlate(float width,float height,float depth,float radius)
        {
            // Rounded outline, softly domed front and a flat back; avoids a sharp rectangular plaque.
            var v=new List<Vector3>();var t=new List<int>();v.Add(new Vector3(0,0,depth));int steps=12;
            for(int corner=0;corner<4;corner++)for(int i=0;i<=steps;i++){float a=(corner*90+i*90f/steps)*Mathf.Deg2Rad;Vector3 c=new Vector3(Mathf.Cos((corner*90+45)*Mathf.Deg2Rad)>0?width*.5f-radius:-width*.5f+radius,Mathf.Sin((corner*90+45)*Mathf.Deg2Rad)>0?height*.5f-radius:-height*.5f+radius,0);v.Add(c+new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,depth*.6f));}
            for(int i=1;i<v.Count;i++)t.AddRange(new[]{0,i,i==v.Count-1?1:i+1});Mesh mesh=new Mesh{name="Rounded forehead plate"};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();return mesh;
        }
    }
}
