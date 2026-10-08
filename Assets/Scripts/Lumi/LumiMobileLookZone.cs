using UnityEngine;
using UnityEngine.EventSystems;

namespace LumiAdventure
{
    /// <summary>Converts right-side touch drags into camera look and optional aim input.</summary>
    public sealed class LumiMobileLookZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private Vector2 lastDelta;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!LumiMobileInput.PointerAimEnabled) return;
            LumiMobileInput.AimScreenPoint = eventData.position;
            LumiMobileInput.HasAim = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (LumiMobileInput.PointerAimEnabled)
            {
                LumiMobileInput.AimScreenPoint = eventData.position;
                LumiMobileInput.HasAim = true;
            }
            LumiMobileInput.Look = eventData.delta * .16f;
            lastDelta = eventData.delta;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            LumiMobileInput.Look = Vector2.zero;
            lastDelta = Vector2.zero;
        }

        private void LateUpdate()
        {
            if (lastDelta.sqrMagnitude > 0f)
            {
                LumiMobileInput.Look = lastDelta * .16f;
                lastDelta = Vector2.zero;
            }
            else LumiMobileInput.Look = Vector2.zero;
        }
    }
}
