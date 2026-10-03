using UnityEngine;

namespace LumiAdventure
{
    public static class LumiNarutoArt
    {
        public static GameObject Create(Transform parent)
        {
            GameObject prefab=Resources.Load<GameObject>("NarutoChibi/Naruto");
            return prefab!=null?Object.Instantiate(prefab,parent,false):LumiChibiNarutoModel.Create(parent);
        }
    }
}
