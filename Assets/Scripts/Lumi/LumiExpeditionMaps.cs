using UnityEngine;

namespace LumiAdventure
{
    public partial class LumiGame
    {
        private static Texture2D floorGrid;
        private static Vector3 P(float x,float z)=>new Vector3(x,0,z);
        private static Vector3[] ExpeditionRoute(int level)
        {
            switch(level)
            {
                case 1:return new[]{P(0,-62),P(-18,-28),P(-10,-8),P(15,14),P(6,32),P(0,62)};
                case 2:return new[]{P(0,-67),P(-20,-36),P(-20,-28),P(20,-6),P(20,2),P(-20,24),P(-20,32),P(18,67)};
                case 3:return new[]{P(0,-70),P(-25,-28),P(-28,-4),P(-8,22),P(16,32),P(22,70)};
                case 4:return new[]{P(0,-72),P(-18,-42),P(-18,-34),P(18,-14),P(18,-6),P(-18,14),P(-18,22),P(0,42),P(0,50),P(0,72)};
                default:return new[]{P(0,-67),P(0,-36),P(-18,-12),P(-18,18),P(0,36),P(18,67)};
            }
        }
        private static Vector3 RoutePoint(Vector3[] route,float progress)
        {
            float total=0;for(int i=1;i<route.Length;i++)total+=Vector3.Distance(route[i-1],route[i]);
            float remaining=total*Mathf.Clamp01(progress);
            for(int i=1;i<route.Length;i++)
            {
                float length=Vector3.Distance(route[i-1],route[i]);
                if(remaining<=length)return Vector3.Lerp(route[i-1],route[i],remaining/length);
                remaining-=length;
            }
            return route[route.Length-1];
        }
        private static float RouteDistance(Vector3 point,Vector3[] route)
        {
            float best=float.PositiveInfinity;
            for(int i=1;i<route.Length;i++)
            {
                Vector3 d=route[i]-route[i-1];
                Vector3 nearest=route[i-1]+d*Mathf.Clamp01(Vector3.Dot(point-route[i-1],d)/d.sqrMagnitude);
                best=Mathf.Min(best,Vector3.Distance(point,nearest));
            }
            return best;
        }
        private GameObject Scenery(string name,Vector3 position,Vector3 size,Material material,Quaternion rotation=default)
        {
            GameObject obj=LumiFactory.Primitive(name,PrimitiveType.Cube,worldRoot,position,size,material,false);
            if(rotation!=default)obj.transform.rotation=rotation;
            return obj;
        }
        private void Trail(Vector3[] route,Material material,float width)
        {
            for(int i=1;i<route.Length;i++)
            {
                Vector3 a=route[i-1],b=route[i];
                Scenery("Expedition route",(a+b)*.5f+Vector3.up*.025f,new Vector3(width,.025f,Vector3.Distance(a,b)+width*.4f),material,Quaternion.LookRotation(b-a));
            }
        }
        private void BuildExpeditionLayout(LevelConfig config,int level)
        {
            Random.State previous=Random.state;Random.InitState(1709+level);
            Material wood=LumiFactory.Material("Expedition timber",new Color(.32f,.22f,.13f));
            Material rock=LumiFactory.Material("Expedition rock"+level,level==2?new Color(.68f,.45f,.25f):new Color(.37f,.43f,.38f));
            Material snow=LumiFactory.Material("Snow caps",new Color(.87f,.94f,.98f));
            Material leaves=LumiFactory.Material("Forest canopy",new Color(.14f,.32f,.2f));
            Material route=LumiFactory.Material("Expedition trail"+level,level==1?new Color(.46f,.38f,.22f):level==2?new Color(.82f,.62f,.34f):level==3?new Color(.29f,.58f,.72f):new Color(.22f,.27f,.29f));
            Trail(config.Route,route,level>=4?7:8);
            if(level<=3)
            {
                for(int side=-1;side<=1;side+=2)
                {
                    for(float z=-config.Depth*.5f+2;z<config.Depth*.5f;z+=3.4f)
                        Edge(P(side*(config.Width*.5f-1),z),level,rock,config.Route);
                    for(float x=-config.Width*.5f+2;x<config.Width*.5f;x+=3.4f)
                        Edge(P(x,side*(config.Depth*.5f-1)),level,rock,config.Route);
                }
            }
            switch(level)
            {
                case 1:
                    for(int i=0;i<90;i++)
                    {
                        Vector3 p=P(Random.Range(-35f,35f),Random.Range(-44f,44f));
                        if(RouteDistance(p,config.Route)<7)continue;
                        ImportedTree(p,Random.Range(3.3f,6f));
                        if(i%3==0)ArenaRock(p+Vector3.right*2,rock,1.7f);
                    }
                    Material stream=LumiFactory.Material("Forest river",new Color(.23f,.56f,.65f));
                    for(int i=0;i<30;i++)Scenery("Winding river",P(-40+i*2.8f,10+Mathf.Sin(i*.42f)*2)+Vector3.up*.04f,new Vector3(3.2f,.03f,3.3f),stream);
                    CreateBlock("Forest bridge",P(11,10)+Vector3.up*.13f,new Vector3(8,.24f,6),wood);
                    for(int j=-3;j<=3;j++)Scenery("Bridge planks",P(11+j,10)+Vector3.up*.27f,new Vector3(.85f,.035f,6),wood);
                    for(int i=0;i<5;i++)Scenery("Forest camp log",P(-26+i*1.5f,-12)+Vector3.up*.35f,new Vector3(1.2f,.65f,3),wood);
                    break;
                case 2:
                    float[] canyonRows={-32,-2,28};float[] canyonGaps={-20,20,-20};
                    for(int i=0;i<canyonRows.Length;i++)BarrierWithDoor(config.Width,canyonRows[i],canyonGaps[i],11,rock,3.5f,"Canyon escarpment");
                    Material cactus=LumiFactory.Material("Cactus",new Color(.23f,.43f,.26f));
                    for(int i=0;i<36;i++)
                    {
                        Vector3 p=P(Random.Range(-31f,31f),Random.Range(-50f,50f));
                        if(RouteDistance(p,config.Route)<6)continue;
                        if(i%3==0)
                        {
                            LumiFactory.Primitive("Cactus stem",PrimitiveType.Capsule,worldRoot,p+Vector3.up*1.2f,new Vector3(.6f,1.2f,.6f),cactus,true);
                            Scenery("Cactus arm",p+new Vector3(.5f,1.1f,0),new Vector3(1.1f,.35f,.35f),cactus);
                            Scenery("Cactus tip",p+new Vector3(.9f,1.5f,0),new Vector3(.35f,.9f,.35f),cactus);
                        }
                        else ArenaRock(p,rock,Random.Range(2f,4.7f));
                    }
                    Material sandstone=LumiFactory.Material("Desert ruins",new Color(.87f,.69f,.44f));
                    for(int i=0;i<5;i++)CreatePillar(P(28,-45+i*22),sandstone,sandstone,3.2f);
                    break;
                case 3:
                    Material ice=LumiFactory.Material("Frozen lake",new Color(.24f,.51f,.69f));
                    Scenery("Great frozen lake",P(3,0)+Vector3.up*.035f,new Vector3(63,.03f,72),ice);
                    Trail(config.Route,route,8);
                    Vector3[] islands={P(6,-22),P(20,4),P(-4,3),P(-26,29)};
                    foreach(Vector3 island in islands)
                    {
                        LumiFactory.Primitive("Snow island",PrimitiveType.Sphere,worldRoot,island+Vector3.up*.09f,new Vector3(15,.18f,12),snow,false);
                        ImportedTree(island,5);
                        ArenaRock(island+P(3,-2),snow,3);
                    }
                    Material crystal=LumiFactory.Material("Ice spires",new Color(.45f,.8f,.95f));
                    for(int i=0;i<34;i++)
                    {
                        Vector3 p=P(Random.Range(-40f,40f),Random.Range(-45f,45f));
                        if(RouteDistance(p,config.Route)<7)continue;
                        ArenaRock(p,snow,Random.Range(2,4));CreateCrystal(p+P(1,2),crystal,Random.Range(2,5));
                    }
                    Material crack=LumiFactory.Material("Ice fissure",new Color(.1f,.32f,.48f));
                    for(int i=0;i<20;i++)Scenery("Ice crack",P(Random.Range(-25,25),Random.Range(-30,30))+Vector3.up*.07f,new Vector3(.13f,.02f,Random.Range(2,5)),crack,Quaternion.Euler(0,Random.Range(0,180),0));
                    break;
                case 4:BuildDungeonRooms(config);break;
                case 5:BuildFacilityDistricts(config);break;
            }
            if(level<=3)
            {
                for(int i=0;i<160;i++)
                {
                    Vector3 p=P(Random.Range(-config.Width*.43f,config.Width*.43f),Random.Range(-config.Depth*.43f,config.Depth*.43f));
                    if(RouteDistance(p,config.Route)<5)continue;
                    if(level==1)Scenery("Grass tuft",p+Vector3.up*.2f,new Vector3(.16f,.4f,.16f),leaves,Quaternion.Euler(0,Random.Range(0,180),12));
                    else Scenery("Ground pebbles",p+Vector3.up*.1f,new Vector3(.35f,.2f,.4f),level==3?snow:rock);
                }
            }
            for(int i=0;i<7;i++)
            {
                Vector3 p=RoutePoint(config.Route,(i+.5f)/8)+Vector3.right*5;
                if(RouteDistance(p,config.Route)<4.5f)continue;
                CreateBlock("Supply crate",p+Vector3.up*.6f,new Vector3(1.3f,1.2f,1.3f),wood);
            }
            GameObject weather=new GameObject("Biome atmosphere");weather.transform.SetParent(worldRoot,false);
            weather.AddComponent<LumiBiomeAtmosphere>().Initialize(level);
            Random.state=previous;
        }
        private void Edge(Vector3 point,int level,Material rock,Vector3[] route)
        {
            if(Vector3.Distance(point,route[0])<10 || Vector3.Distance(point,route[route.Length-1])<9)return;
            if(level==2)ArenaRock(point,rock,Random.Range(4,6));else ImportedTree(point,Random.Range(4,7));
        }
        private void BarrierWithDoor(float width,float z,float gap,float opening,Material wall,float height,string name)
        {
            float left=-width*.5f+1,right=width*.5f-1;
            float end=gap-opening*.5f,start=gap+opening*.5f;
            CreateBlock(name,P((left+end)*.5f,z)+Vector3.up*height*.5f,new Vector3(end-left,height,2.5f),wall);
            CreateBlock(name,P((start+right)*.5f,z)+Vector3.up*height*.5f,new Vector3(right-start,height,2.5f),wall);
            if(currentLevel==2)
                for(float x=left;x<right;x+=5)if(Mathf.Abs(x-gap)>opening*.5f+1)ArenaRock(P(x,z),wall,Random.Range(3.2f,4.2f));
        }
        private void BuildDungeonRooms(LevelConfig config)
        {
            Material stone=LumiFactory.Material("Dungeon masonry",new Color(.24f,.27f,.25f));
            Material fire=LumiFactory.Material("Dungeon torches",new Color(1,.45f,.08f),true);
            Material poison=LumiFactory.Material("Poison pools",new Color(.25f,.72f,.06f),true);
            Material purple=LumiFactory.Material("Dungeon amethyst",new Color(.58f,.26f,.8f),true);
            float[] rows={-38,-10,18,46},gaps={-18,18,-18,0};
            for(int i=0;i<rows.Length;i++)
            {
                BarrierWithDoor(config.Width,rows[i],gaps[i],10,stone,2.8f,"Dungeon chamber wall");
                for(int side=-1;side<=1;side+=2)
                {
                    CreatePillar(P(gaps[i]+side*6,rows[i]),stone,fire,2.4f);
                    CreatePillar(P(side*35,rows[i]-8),stone,fire,3);
                }
            }
            for(int i=0;i<8;i++)
            {
                Vector3 p=P(i%2==0?-31:31,-48+i*13);
                CreateHazard(p+Vector3.up*.08f,new Vector3(8,.12f,6),poison);
                CreateCrystal(p+P(5,3),purple,2.6f);
                CreateBlock("Stone sarcophagus",p+P(-4,-5)+Vector3.up*.6f,new Vector3(2.4f,1.2f,4),stone);
            }
            GroundGrid(config,stone);
        }
        private void BuildFacilityDistricts(LevelConfig config)
        {
            Material steel=LumiFactory.Material("Facility steel",new Color(.3f,.39f,.45f));
            Material dark=LumiFactory.Material("Facility chassis",new Color(.075f,.12f,.17f));
            Material cyan=LumiFactory.Material("Facility cyan",new Color(.08f,.75f,1),true);
            Material red=LumiFactory.Material("Facility red",new Color(1,.15f,.24f),true);
            foreach(float z in new[]{-43f,35f})
            {
                BarrierWithDoor(config.Width,z,0,19,steel,2.5f,"Facility airlock wall");
                for(int s=-1;s<=1;s+=2)Scenery("Airlock light",P(s*10,z)+Vector3.up*1.7f,new Vector3(.2f,2.1f,.18f),cyan);
            }
            CreateBlock("Reactor base",Vector3.up*.6f,new Vector3(13,1.2f,18),dark);
            LumiFactory.Primitive("Reactor core",PrimitiveType.Cylinder,worldRoot,Vector3.up*2.8f,new Vector3(5,2.2f,5),cyan,true);
            for(int i=0;i<4;i++)CreatePillar(P(i%2==0?-8:8,i<2?-10:10),steel,red,3.6f);
            for(int side=-1;side<=1;side+=2)
                for(int row=0;row<6;row++)
                {
                    Vector3 p=P(side*36,-55+row*21);
                    CreateBlock("Server rack",p+Vector3.up*1.6f,new Vector3(5,3.2f,8),dark);
                    for(int led=0;led<4;led++)Scenery("Rack status strip",p+new Vector3(-side*2.55f,.7f+led*.55f,0),new Vector3(.08f,.11f,5),row%3==0?red:cyan);
                    Scenery("Service lane",p+P(-side*6,0)+Vector3.up*.05f,new Vector3(.15f,.07f,14),cyan);
                    Scenery("Hazard markings",p+P(-side*9,0)+Vector3.up*.06f,new Vector3(2,.03f,1),red);
                }
            for(int i=0;i<16;i++)
            {
                Vector3 p=RoutePoint(config.Route,i/15f);
                Scenery("Route beacon",p+P(4,0)+Vector3.up*.09f,new Vector3(.16f,.1f,2),cyan);
            }
            GroundGrid(config,steel);
        }
        private void GroundGrid(LevelConfig config,Material tint)
        {
            if(floorGrid==null)
            {
                floorGrid=new Texture2D(64,64,TextureFormat.RGB24,false){name="Expedition floor grid",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};
                Color[] pixels=new Color[4096];
                for(int y=0;y<64;y++)for(int x=0;x<64;x++)pixels[y*64+x]=(x<2 || y<2)?new Color(.5f,.55f,.6f):Color.white;
                floorGrid.SetPixels(pixels);floorGrid.Apply(false,true);
            }
            Material floor=LumiFactory.Material("Expedition floor"+currentLevel,currentLevel==4?new Color(.18f,.22f,.2f):new Color(.12f,.19f,.24f));
            floor.mainTexture=floorGrid;floor.mainTextureScale=new Vector2(config.Width/4,config.Depth/4);
            Scenery("Tiled expedition floor",Vector3.up*.01f,new Vector3(config.Width,.02f,config.Depth),floor);
        }
    }
    public sealed class LumiBiomeAtmosphere:MonoBehaviour
    {
        private ParticleSystem particles;
        private LumiGame game;
        private static Material atmosphereMaterial;
        public void Initialize(int level)
        {
            game=GetComponentInParent<LumiGame>();
            particles=gameObject.AddComponent<ParticleSystem>();particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=particles.main;main.loop=true;main.startLifetime=5;main.startSpeed=level==3?1.4f:.35f;main.startSize=level==3?.085f:.045f;main.maxParticles=80;main.simulationSpace=ParticleSystemSimulationSpace.World;
            main.startColor=level==3?new Color(.92f,.98f,1,.7f):level==2?new Color(.9f,.73f,.45f,.25f):new Color(.9f,.95f,.7f,.25f);
            var shape=particles.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(28,.2f,28);
            var emission=particles.emission;emission.rateOverTime=level==3?12:5;
            if(atmosphereMaterial==null)atmosphereMaterial=new Material(Shader.Find("Sprites/Default")){name="Biome motes"};
            particles.GetComponent<ParticleSystemRenderer>().sharedMaterial=atmosphereMaterial;
            transform.rotation=Quaternion.Euler(90,0,0);particles.Play();
        }
        private void LateUpdate(){if(game!=null && game.Player!=null)transform.position=game.Player.transform.position+Vector3.up*7;}
    }
}
