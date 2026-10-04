using UnityEngine;

namespace LumiAdventure
{
    public static class LumiNarutoArt
    {
        public static GameObject Create(Transform parent)
        {
            GameObject prefab=Resources.Load<GameObject>("NarutoChibi/Naruto");
            if(prefab==null)throw new System.InvalidOperationException("Shippuden Naruto prefab is missing. In Unity choose Naruto > Build Shippuden character rig.");
            return Object.Instantiate(prefab,parent,false);
        }
    }
}
