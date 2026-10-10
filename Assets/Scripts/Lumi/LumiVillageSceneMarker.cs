using UnityEngine;

namespace LumiAdventure
{
    public sealed class LumiVillageSceneMarker : MonoBehaviour
    {
        [Range(1,5)] public int level=1;

        public static string SceneName(int level)
        {
            switch(Mathf.Clamp(level,1,5))
            {
                case 1:return "Village_Leaf";
                case 2:return "Village_Sand";
                case 3:return "Village_Stone";
                case 4:return "Village_Cloud";
                default:return "Village_Mist";
            }
        }
    }
}
