using UnityEngine;

namespace LumiAdventure
{
    /// <summary>Routes the desktop Escape key to the current game screen.</summary>
    public sealed class LumiPauseInputRouter : MonoBehaviour
    {
        private LumiGame game;

        public void Initialize(LumiGame owner) => game = owner;

        private void Update()
        {
            if (game != null && Input.GetKeyDown(KeyCode.Escape)) game.HandleEscapeCommand();
        }
    }
}
