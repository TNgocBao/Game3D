using UnityEngine;

namespace LumiAdventure
{
    /// <summary>Reads player attack intent without owning movement or UI behavior.</summary>
    public sealed class LumiPlayerCombatInput : MonoBehaviour
    {
        private LumiGame game;

        public bool UsesTouchAim => game.Controls.UsesMobileControls;
        public bool BasicAttackPressed => game.Controls.UsesPcControls && Input.GetMouseButtonDown(0);

        public void Initialize(LumiGame owner) => game = owner;
    }
}
