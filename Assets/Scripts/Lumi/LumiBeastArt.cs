using UnityEngine;

namespace LumiAdventure
{
    public static class LumiBeastArt
    {
        public static string NameFor(LumiEnemyType tier)=>tier==LumiEnemyType.Sprout?"Matatabi":tier==LumiEnemyType.Ranger?"Gyuki":"Kurama";
        public static GameObject Create(Transform parent,LumiEnemyType tier)
        {
            string name=NameFor(tier);
            var prefab=Resources.Load<GameObject>("LumiEnemies/"+name);
            if(prefab==null)throw new System.InvalidOperationException("Missing approved enemy prefab: "+name);
            return Object.Instantiate(prefab,parent,false);
        }
    }
}
