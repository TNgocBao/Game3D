using UnityEngine;
using UnityEngine.EventSystems;

namespace LumiAdventure
{
    public enum LumiMobileAction { Jump, Interact, Camera, Menu }

    /// <summary>Routes one mobile HUD button to exactly one gameplay action.</summary>
    public sealed class LumiMobileActionButton : MonoBehaviour, IPointerDownHandler
    {
        public LumiMobileAction Action;

        public void OnPointerDown(PointerEventData eventData)
        {
            switch (Action)
            {
                case LumiMobileAction.Jump:
                    LumiMobileInput.QueueJump();
                    break;
                case LumiMobileAction.Interact:
                    LumiMobileInput.QueueInteract();
                    break;
                case LumiMobileAction.Camera:
                    LumiCameraRig camera = Object.FindObjectOfType<LumiCameraRig>();
                    if (camera != null) camera.CycleMode();
                    break;
                case LumiMobileAction.Menu:
                    LumiGame game = Object.FindObjectOfType<LumiGame>();
                    if (game != null) game.PauseGame();
                    break;
            }
        }
    }
}
