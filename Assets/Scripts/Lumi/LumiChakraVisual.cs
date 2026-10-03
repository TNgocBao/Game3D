using UnityEngine;

namespace LumiAdventure
{
    public static class LumiChakraVisual
    {
        private static Material chakra,lineMaterial;
        private static Mesh blade;
        public static GameObject Orb(Transform parent,bool shuriken,float radius)
        {
            if(chakra==null){Shader shader=Resources.Load<Shader>("LumiChakra");chakra=new Material(shader!=null?shader:Shader.Find("Standard")){name="Swirling blue chakra"};chakra.SetColor("_Color",new Color(.1f,.6f,1,1));}
            if(lineMaterial==null)lineMaterial=new Material(Shader.Find("Sprites/Default")){name="Chakra wind filaments"};
            GameObject root=LumiFactory.WorldObject(shuriken?"Wind Rasenshuriken":"Rasengan vortex",parent,Vector3.zero);
            LumiFactory.Primitive("Chakra sphere",PrimitiveType.Sphere,root.transform,Vector3.zero,Vector3.one*radius*2,chakra,false);
            LumiFactory.Primitive("White chakra core",PrimitiveType.Sphere,root.transform,Vector3.zero,Vector3.one*radius*.65f,LumiFactory.Material("Chakra core",new Color(.7f,.95f,1),true),false);
            for(int j=0;j<4;j++)
            {
                GameObject ribbon=new GameObject("Spiral chakra ribbon");ribbon.transform.SetParent(root.transform,false);
                ribbon.transform.localRotation=Quaternion.Euler(j*43,j*57,j*31);
                LineRenderer line=ribbon.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=false;line.positionCount=65;line.sharedMaterial=lineMaterial;
                line.startWidth=line.endWidth=radius*.038f;line.startColor=new Color(.3f,.8f,1,.2f);line.endColor=new Color(.85f,1,1,.85f);
                for(int i=0;i<65;i++){float t=i/64f,angle=t*Mathf.PI*4.5f+j;float y=(t-.5f)*1.75f;float r=Mathf.Sqrt(Mathf.Max(.05f,1-y*y));line.SetPosition(i,new Vector3(Mathf.Cos(angle)*r,y,Mathf.Sin(angle)*r)*radius*1.08f);}
                line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            if(shuriken)
            {
                if(blade==null)
                {
                    blade=new Mesh{name="Four-point wind blade"};
                    blade.vertices=new[]{new Vector3(-.25f,0,.15f),new Vector3(.28f,0,.45f),new Vector3(.05f,0,2.5f),new Vector3(-.35f,0,1.15f),new Vector3(0,.08f,.6f)};
                    blade.triangles=new[]{0,4,1,1,4,2,2,4,3,3,4,0,1,4,0,2,4,1,3,4,2,0,4,3};blade.RecalculateNormals();
                }
                for(int i=0;i<4;i++)
                {
                    GameObject wing=new GameObject("Wind blade "+i,typeof(MeshFilter),typeof(MeshRenderer));wing.transform.SetParent(root.transform,false);
                    wing.transform.localRotation=Quaternion.Euler(0,i*90,0);wing.transform.localScale=Vector3.one*radius;
                    wing.GetComponent<MeshFilter>().sharedMesh=blade;wing.GetComponent<MeshRenderer>().sharedMaterial=LumiFactory.Material("Wind blades",new Color(.5f,.9f,1,.65f),true,true);
                }
            }
            root.AddComponent<LumiChakraSpin>();return root;
        }
        public static void Burst(Transform parent,Vector3 position,bool smoke,float size=1)
        {
            GameObject root=new GameObject(smoke?"Shadow clone smoke":"Chakra impact");root.transform.SetParent(parent,false);root.transform.position=position;
            ParticleSystem particles=root.AddComponent<ParticleSystem>();particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=particles.main;main.loop=false;main.duration=.65f;main.startLifetime=.65f;main.startSize=smoke?.35f:.16f;main.startSpeed=(smoke?2:5)*size;main.maxParticles=42;
            main.startColor=smoke?new Color(.86f,.93f,1,.8f):new Color(.35f,.85f,1,.9f);
            var emission=particles.emission;emission.enabled=false;var shape=particles.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.35f*size;
            if(lineMaterial==null)lineMaterial=new Material(Shader.Find("Sprites/Default"));particles.GetComponent<ParticleSystemRenderer>().sharedMaterial=lineMaterial;
            var fade=particles.colorOverLifetime;fade.enabled=true;Gradient gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(smoke?new Color(.75f,.83f,.9f):new Color(.1f,.65f,1),1)},new[]{new GradientAlphaKey(.8f,0),new GradientAlphaKey(0,1)});fade.color=gradient;
            particles.Play();particles.Emit(smoke?30:40);Object.Destroy(root,1.1f);
        }
    }
    public sealed class LumiChakraSpin:MonoBehaviour
    {
        private void Update(){transform.Rotate(0,Time.deltaTime*720,Time.deltaTime*120,Space.Self);}
    }
}
