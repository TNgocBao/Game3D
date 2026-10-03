using UnityEngine;

namespace LumiAdventure
{
    public static class LumiNarutoArt
    {
        public static GameObject Create(Transform parent)
        {
            GameObject prefab=Resources.Load<GameObject>("NarutoChibi/Naruto");
            if(prefab==null)throw new System.InvalidOperationException("Naruto Starter prefab is missing. Rebuild it with LumiStarterNarutoBuild in the Unity Editor.");
            return Object.Instantiate(prefab,parent,false);
        }
    }
}
