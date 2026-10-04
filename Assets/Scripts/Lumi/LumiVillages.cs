using UnityEngine;

namespace LumiAdventure
{
    public partial class LumiGame
    {
        private void BuildVillage(LevelConfig config,int level)
        {
            Random.State saved=Random.state;Random.InitState(7100+level);
            Material plaster=LumiFactory.Material("Village plaster"+level,level==2?new Color(.82f,.65f,.4f):level==3?new Color(.52f,.49f,.43f):level==5?new Color(.39f,.49f,.52f):new Color(.88f,.83f,.69f));
            Material roof=LumiFactory.Material("Village roof"+level,level==5?new Color(.16f,.26f,.3f):level==2?new Color(.73f,.48f,.23f):new Color(.65f,.23f,.14f));
            Material timber=LumiFactory.Material("Village timber",new Color(.28f,.19f,.12f));
            Material stone=LumiFactory.Material("Village cliffs"+level,Color.Lerp(config.Wall,Color.gray,.35f));
            Material path=LumiFactory.Material("Village avenue"+level,level==2?new Color(.85f,.69f,.46f):new Color(.59f,.55f,.44f));
            Trail(config.Route,path,9);
            for(int row=0;row<8;row++)for(int side=-1;side<=1;side+=2)
            {
                Vector3 center=RoutePoint(config.Route,.08f+row*.105f);
                for(int column=0;column<2;column++)
                {
                    Vector3 p=center+new Vector3(side*(15+column*15),0,Random.Range(-3f,3f));
                    if(Mathf.Abs(p.x)>config.Width*.5f-8 || RouteDistance(p,config.Route)<10 || Vector3.Distance(p,config.Route[config.Route.Length-1])<15)continue;
                    VillageHouse(p,level,Random.Range(4.8f,7f),Random.Range(4f,7f));
                }
                Vector3 lantern=center+Vector3.right*side*6.3f;
                Scenery("Stone lantern base",lantern+Vector3.up*.3f,new Vector3(.85f,.6f,.85f),stone);
                Scenery("Village lantern pole",lantern+Vector3.up*1.65f,new Vector3(.2f,2.2f,.2f),timber);
                Scenery("Paper lantern",lantern+Vector3.up*2.65f,new Vector3(.7f,.9f,.7f),LumiFactory.Material("Paper lantern",new Color(1,.73f,.38f),true));
            }
            for(int side=-1;side<=1;side+=2)for(float z=-config.Depth*.5f+5;z<config.Depth*.5f;z+=7)
            {
                Vector3 p=P(side*(config.Width*.5f-3),z);
                if(level==1)ImportedTree(p,Random.Range(5f,8f));
                else ArenaRock(p,stone,Random.Range(4f,7f));
            }
            Vector3 end=config.Route[config.Route.Length-1];
            Scenery("Boss arena",end+new Vector3(0,.035f,-8),new Vector3(25,.05f,25),path);
            VillageGate(config.Route[0]+Vector3.back*3,roof,timber);
            VillageGate(end+Vector3.forward*2,roof,timber);
            if(level==1)
            {
                for(int i=0;i<5;i++)
                {
                    Vector3 p=P(-32+i*16,config.Depth*.5f+5);
                    ArenaRock(p,stone,10);
                    Transform face=LumiFactory.WorldObject("Hokage monument",worldRoot,p+Vector3.up*8).transform;
                    LumiFactory.Primitive("Carved face",PrimitiveType.Sphere,face,Vector3.zero,new Vector3(5,6,2.5f),plaster,false);
                    Scenery("Hokage brow",p+new Vector3(0,9,-1.4f),new Vector3(3,.45f,.4f),stone);
                    Scenery("Hokage nose",p+new Vector3(0,7.7f,-1.7f),new Vector3(.6f,1.3f,.8f),plaster);
                }
                for(int i=0;i<40;i++){Vector3 p=P(Random.Range(-40,40),Random.Range(-60,60));if(RouteDistance(p,config.Route)>12)ImportedTree(p,Random.Range(3f,5f));}
            }
            if(level==2)
            {
                VillageHouse(P(-33,38),level,14,18);
                for(int i=0;i<15;i++)ArenaRock(P(Random.Range(-45,45),config.Depth*.5f+7),stone,Random.Range(8f,14f));
            }
            if(level==3)
            {
                for(int i=0;i<14;i++)ArenaRock(P((i%2==0?-1:1)*42,-60+i*9),stone,Random.Range(7f,12f));
                VillageHouse(P(-33,40),level,12,15);
            }
            if(level==4)
            {
                for(int i=0;i<8;i++)
                {
                    Vector3 p=P((i%2==0?-1:1)*37,-58+i*18);
                    LumiFactory.Primitive("Cloud mountain pillar",PrimitiveType.Cylinder,worldRoot,p+Vector3.up*7,new Vector3(12,7,12),stone,true);
                    VillageHouse(p+Vector3.up*14,level,9,7);
                }
                Material cloud=LumiFactory.Material("Distant cloud banks",new Color(.92f,.97f,1,.65f),false,true);
                for(int i=0;i<18;i++)LumiFactory.Primitive("Mountain cloud",PrimitiveType.Sphere,worldRoot,P((i%2==0?-1:1)*58,-75+i*9)+Vector3.up*8,new Vector3(20,3,9),cloud,false);
            }
            if(level==5)
            {
                Material water=LumiFactory.Material("Mist village canals",new Color(.2f,.46f,.52f,.85f),false,true);
                for(int side=-1;side<=1;side+=2)Scenery("Canal",P(side*32,0)+Vector3.up*.055f,new Vector3(8,.025f,config.Depth-12),water);
                for(int i=0;i<4;i++)
                {
                    Vector3 p=P(32,-50+i*32);
                    CreateBlock("Canal bridge",p+Vector3.up*.14f,new Vector3(12,.26f,5),timber);
                    for(int j=-5;j<=5;j++)Scenery("Bridge boards",p+new Vector3(j,.285f,0),new Vector3(.8f,.04f,5),roof);
                }
            }
            Random.state=saved;
        }
        private void VillageHouse(Vector3 p,int level,float width,float height)
        {
            LumiEnvironmentAssets.House(worldRoot,p,level,width,height);
        }
        private void VillageGate(Vector3 p,Material roof,Material wood)
        {
            for(int side=-1;side<=1;side+=2)CreateBlock("Village gate post",p+new Vector3(side*5,3,0),new Vector3(.7f,6,.7f),wood);
            Scenery("Village gate lintel",p+Vector3.up*5.7f,new Vector3(13,.8f,1.4f),roof);
            Scenery("Village gate crossbar",p+Vector3.up*4.8f,new Vector3(11,.35f,.6f),wood);
        }
    }
}
