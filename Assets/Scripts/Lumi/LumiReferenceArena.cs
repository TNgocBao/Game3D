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

        private void ImportedTree(Vector3 position,float height)
        {
            LumiEnvironmentAssets.Tree(worldRoot,position,height);
        }
    }
}
