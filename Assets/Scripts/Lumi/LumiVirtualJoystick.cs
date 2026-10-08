using UnityEngine;
using UnityEngine.EventSystems;

namespace LumiAdventure
{
    /// <summary>Converts joystick pointer movement into normalized movement input.</summary>
    public sealed class LumiVirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform Knob;
        private RectTransform rect;

        private void Awake() => rect = transform as RectTransform;
        public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out local)) return;
            float radius = rect.rect.width * .36f;
            Vector2 value = Vector2.ClampMagnitude(local / radius, 1f);
            LumiMobileInput.Move = value;
            if (Knob != null) Knob.anchoredPosition = value * radius;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            LumiMobileInput.Move = Vector2.zero;
            if (Knob != null) Knob.anchoredPosition = Vector2.zero;
        }
    }
}
