using UnityEngine;

namespace LumiAdventure
{
    /// <summary>Owns visibility of desktop-only HUD controls so mobile has one menu entry point.</summary>
    public sealed class LumiPcOnlyUi : MonoBehaviour
    {
        private LumiGame game;
        private CanvasGroup group;
        public void Initialize(LumiGame owner) => game = owner;
        private void Awake() { group = gameObject.AddComponent<CanvasGroup>(); }
        private void LateUpdate()
        {
            if (game == null || group == null) return;
            bool visible = game.Controls.UsesPcControls;
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
