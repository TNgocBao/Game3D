using UnityEngine;

namespace LumiAdventure
{
    public partial class LumiGame
    {
        private void BuildReferenceArena(LevelConfig config, int level)
        {
            BuildExpeditionLayout(config,level);
        }

        private void ArenaRock(Vector3 position, Material material, float size)
        {
            GameObject rock=LumiFactory.Primitive("Layered boulder",PrimitiveType.Sphere,worldRoot,position+Vector3.up*size*.35f,new Vector3(size,size*.75f,size*.85f),material,true);
            rock.transform.localRotation=Quaternion.Euler(12,Random.Range(0,180),18);
            LumiFactory.Primitive("Rock cap",PrimitiveType.Cube,worldRoot,position+Vector3.up*size*.65f,new Vector3(size*.7f,size*.25f,size*.65f),material,false).transform.localRotation=Quaternion.Euler(0,28,8);
        }

        private void ArenaPine(Vector3 position, Material trunk, Material leaves, float height)
        {
            LumiFactory.Primitive("Pine trunk",PrimitiveType.Cylinder,worldRoot,position+Vector3.up*height*.28f,new Vector3(.3f,height*.28f,.3f),trunk,true);
            for(int tier=0;tier<4;tier++)
            {
                GameObject crown=new GameObject("Pine crown",typeof(MeshFilter),typeof(MeshRenderer));
                crown.transform.SetParent(worldRoot,false);
                crown.transform.position=position+Vector3.up*(height*.22f+tier*height*.17f);
                crown.GetComponent<MeshFilter>().sharedMesh=LumiCharacterArt.Cone;
                crown.GetComponent<MeshRenderer>().sharedMaterial=leaves;
                float radius=height*(.32f-tier*.055f);
                crown.transform.localScale=new Vector3(radius,height*.43f,radius);
                if(tier==0)
                {
                    BoxCollider canopy=crown.AddComponent<BoxCollider>();
                    canopy.center=new Vector3(0,.38f,0);canopy.size=new Vector3(1.5f,.65f,1.5f);
                }
            }
        }
    }
}
