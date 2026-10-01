using UnityEngine;
using UnityEngine.EventSystems;

namespace LumiAdventure
{
    public static class LumiMobileInput
    {
        public static Vector2 Move;
        public static Vector2 Look;
        private static bool jump;
        private static bool shoot;
        private static bool interact;

        public static void QueueJump() => jump = true;
        public static void QueueShoot() => shoot = true;
        public static void QueueInteract() => interact = true;

        public static bool ConsumeJump() { bool value = jump; jump = false; return value; }
        public static bool ConsumeShoot() { bool value = shoot; shoot = false; return value; }
        public static bool ConsumeInteract() { bool value = interact; interact = false; return value; }

        public static void Reset()
        {
            Move = Vector2.zero;
            Look = Vector2.zero;
            jump = shoot = interact = false;
        }
    }

    public class LumiVirtualStick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform Knob;
        private RectTransform rect;

        private void Awake() => rect = transform as RectTransform;
        public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

        public void OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out var local)) return;
            float radius = rect.rect.width * 0.36f;
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

    public class LumiLookZone : MonoBehaviour, IDragHandler, IPointerUpHandler
    {
        private Vector2 last;
        public void OnDrag(PointerEventData eventData)
        {
            Vector2 delta = eventData.delta;
            LumiMobileInput.Look = delta * 0.16f;
            last = delta;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            LumiMobileInput.Look = Vector2.zero;
            last = Vector2.zero;
        }

        private void LateUpdate()
        {
            if (last.sqrMagnitude > 0f)
            {
                LumiMobileInput.Look = last * 0.16f;
                last = Vector2.zero;
            }
            else LumiMobileInput.Look = Vector2.zero;
        }
    }

    public enum LumiMobileAction { Shoot, Jump, Interact }

    public class LumiMobileActionButton : MonoBehaviour, IPointerDownHandler
    {
        public LumiMobileAction Action;
        public void OnPointerDown(PointerEventData eventData)
        {
            if (Action == LumiMobileAction.Shoot) LumiMobileInput.QueueShoot();
            else if (Action == LumiMobileAction.Jump) LumiMobileInput.QueueJump();
            else LumiMobileInput.QueueInteract();
        }
    }
}
