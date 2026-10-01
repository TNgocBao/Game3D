using UnityEngine;

namespace LumiAdventure
{
    public static class LumiBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartGame()
        {
            if (Object.FindObjectOfType<LumiGame>() != null) return;

            var root = new GameObject("Lumi Adventure");
            Object.DontDestroyOnLoad(root);
            root.AddComponent<LumiGame>();
        }
    }
}
