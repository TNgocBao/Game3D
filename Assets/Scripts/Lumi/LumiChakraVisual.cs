using UnityEngine;

namespace LumiAdventure
{
    public static class LumiChakraVisual
    {
        private static Material chakra,lineMaterial,particleMaterial;
        private static Mesh blade;
        public static GameObject Orb(Transform parent,bool shuriken,float radius)
        {
            if(chakra==null){Shader shader=Resources.Load<Shader>("LumiChakra");chakra=new Material(shader!=null?shader:Shader.Find("Standard")){name="Swirling blue chakra"};chakra.SetColor("_Color",new Color(.1f,.6f,1,1));}
            if(lineMaterial==null)lineMaterial=new Material(Shader.Find("Sprites/Default")){name="Chakra wind filaments"};
            GameObject root=LumiFactory.WorldObject(shuriken?"Wind Rasenshuriken":"Rasengan vortex",parent,Vector3.zero);
            LumiFactory.Primitive("Chakra sphere",PrimitiveType.Sphere,root.transform,Vector3.zero,Vector3.one*radius*2,chakra,false);
            LumiFactory.Primitive("White chakra core",PrimitiveType.Sphere,root.transform,Vector3.zero,Vector3.one*radius*.65f,LumiFactory.Material("Chakra core",new Color(.7f,.95f,1),true),false);
            for(int j=0;j<7;j++)
            {
                GameObject ribbon=new GameObject("Spiral chakra ribbon");ribbon.transform.SetParent(root.transform,false);
                ribbon.transform.localRotation=Quaternion.Euler(j*43,j*57,j*31);
                LineRenderer line=ribbon.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=false;line.positionCount=65;line.sharedMaterial=lineMaterial;
                line.startWidth=line.endWidth=radius*.045f;line.startColor=new Color(.3f,.8f,1,.35f);line.endColor=new Color(.85f,1,1,.85f);
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
            root.AddComponent<LumiChakraSpin>().IsShuriken=shuriken;return root;
        }
        public static GameObject Vortex(Transform parent,float radius)
        {
            GameObject root=Orb(parent,false,1);root.name="Dense expanding chakra sphere";
            // Retain the hollow, swirling shell so targets remain readable inside the blast.
            Transform core=root.transform.Find("White chakra core");if(core!=null)core.localScale=Vector3.one*.22f;
            Renderer shell=root.transform.Find("Chakra sphere").GetComponent<Renderer>();var tint=new MaterialPropertyBlock();
            tint.SetFloat("_Density",.7f);tint.SetFloat("_Energy",1.15f);shell.SetPropertyBlock(tint);
            for(int ring=0;ring<5;ring++)
            {
                var obj=new GameObject("Wind shockwave ring "+ring);obj.transform.SetParent(root.transform,false);obj.transform.localRotation=Quaternion.Euler(ring*31,ring*43,ring*17);
                LineRenderer line=obj.AddComponent<LineRenderer>();line.sharedMaterial=lineMaterial;line.useWorldSpace=false;line.loop=true;line.positionCount=96;
                line.widthMultiplier=.026f;line.startColor=line.endColor=new Color(.65f,.94f,1,.62f);
                for(int i=0;i<96;i++){float angle=i*Mathf.PI*2/96;line.SetPosition(i,new Vector3(Mathf.Cos(angle),.06f*Mathf.Sin(angle*8),Mathf.Sin(angle))*1.035f);}
                line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            root.transform.localScale=Vector3.one*radius;return root;
        }
        public static ParticleSystem WindParticles(Transform parent,float radius)
        {
            if(particleMaterial==null)
            {
                particleMaterial=new Material(Shader.Find("Sprites/Default")){name="Soft chakra motes"};
                var texture=new Texture2D(32,32,TextureFormat.RGBA32,false);var pixels=new Color[1024];
                for(int y=0;y<32;y++)for(int x=0;x<32;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f))/15.5f;pixels[y*32+x]=new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-d),2));}
                texture.SetPixels(pixels);texture.Apply();particleMaterial.mainTexture=texture;
            }
            GameObject obj=new GameObject("Chakra wind debris");obj.transform.SetParent(parent,false);ParticleSystem ps=obj.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.loop=false;main.duration=1.4f;main.startLifetime=new ParticleSystem.MinMaxCurve(.45f,.85f);main.startSpeed=radius*2.6f;main.startSize=new ParticleSystem.MinMaxCurve(.08f,.18f);main.maxParticles=180;main.simulationSpace=ParticleSystemSimulationSpace.Local;
            main.startColor=new Color(.55f,.9f,1,.85f);var emission=ps.emission;emission.enabled=false;
            var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=radius*.12f;
            var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.orbitalY=5;velocity.orbitalX=1.5f;
            var fade=ps.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(new Color(.15f,.6f,1),1)},new[]{new GradientAlphaKey(.8f,0),new GradientAlphaKey(0,1)});fade.color=gradient;
            ParticleSystemRenderer renderer=obj.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=particleMaterial;renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.lengthScale=2;renderer.velocityScale=.08f;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            ps.Play();ps.Emit(100);return ps;
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
        public bool IsShuriken {get;set;}
        private void Update(){transform.Rotate(IsShuriken?0:Time.deltaTime*90,Time.deltaTime*1080,IsShuriken?0:Time.deltaTime*150,Space.Self);}
    }
}
