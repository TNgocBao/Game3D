using UnityEngine;

namespace LumiAdventure
{
    /// <summary>Reads movement and jump input for the active control scheme.</summary>
    public sealed class LumiPlayerMovementInput : MonoBehaviour
    {
        private LumiGame game;

        public void Initialize(LumiGame owner) => game = owner;

        public Vector2 ReadMovement()
        {
            if (game.Controls.UsesMobileControls) return LumiMobileInput.Move;
            return Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);
        }

        public bool ConsumeJump()
        {
            return game.Controls.UsesMobileControls ? LumiMobileInput.ConsumeJump() : Input.GetKeyDown(KeyCode.Space);
        }
    }
}
