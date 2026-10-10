using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;

namespace LumiAdventure
{
    /// <summary>Builds and presents the mobile-only joystick and action controls.</summary>
    public sealed class LumiMobileControlsView : MonoBehaviour
    {
        private LumiGame game;
        private GameObject root;
        private GameObject interactButton;

        public GameObject Root => root;

        public void Initialize(LumiGame owner, Transform canvasRoot)
        {
            game = owner;
            Build(canvasRoot);
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (root != null) root.SetActive(visible);
            if (!visible) SetInteractionVisible(false);
        }

        public void SetInteractionVisible(bool visible)
        {
            if (interactButton != null) interactButton.SetActive(visible && game.IsPlaying && game.Controls.UsesMobileControls);
        }

        private void Build(Transform canvasRoot)
        {
            root = new GameObject("Mobile Controls", typeof(RectTransform));
            root.transform.SetParent(canvasRoot, false);
            LumiFactory.Stretch(root.GetComponent<RectTransform>());
            root.AddComponent<LumiMobileSafeArea>();

            Image lookZone = LumiFactory.Image(root.transform, new Color(1f, 1f, 1f, .001f));
            lookZone.rectTransform.anchorMin = new Vector2(.45f, 0f);
            lookZone.rectTransform.anchorMax = Vector2.one;
            lookZone.rectTransform.offsetMin = Vector2.zero;
            lookZone.rectTransform.offsetMax = Vector2.zero;
            lookZone.gameObject.AddComponent<LumiMobileLookZone>();

            Image joystick = LumiFactory.Image(root.transform, new Color(.08f, .12f, .18f, .34f));
            joystick.sprite = LumiAbilityHud.Circle;
            LumiFactory.Rect(joystick.rectTransform, new Vector2(0f, 0f), new Vector2(260f, 260f), new Vector2(285f, 180f));
            Image knob = LumiFactory.Image(joystick.transform, new Color(.35f, .9f, .88f, .88f));
            knob.sprite = LumiAbilityHud.Circle;
            LumiFactory.Rect(knob.rectTransform, new Vector2(.5f, .5f), new Vector2(105f, 105f), Vector2.zero);
            LumiVirtualJoystick joystickInput = joystick.gameObject.AddComponent<LumiVirtualJoystick>();
            joystickInput.Knob = knob.rectTransform;

            interactButton = CreateActionButton("NÓI", new Vector2(1f, 0f), new Vector2(-520f, 280f), new Color(.55f, .35f, .82f, .86f), LumiMobileAction.Interact);
            interactButton.SetActive(false);
            CreateActionButton("MENU", new Vector2(1f, 1f), new Vector2(-68f, -78f), new Color(.11f, .18f, .28f, .9f), LumiMobileAction.Menu, 112f);
            CreateActionButton("CAM", new Vector2(0f, 1f), new Vector2(62f, -285f), new Color(.08f, .42f, .72f, .88f), LumiMobileAction.Camera, 112f);
        }

        private GameObject CreateActionButton(string label, Vector2 anchor, Vector2 position, Color color, LumiMobileAction action, float size = 135f)
        {
            Image image = LumiFactory.Image(root.transform, color);
            image.sprite = LumiAbilityHud.Circle;
            image.gameObject.name = action == LumiMobileAction.Camera ? "Đổi góc nhìn" : label;
            LumiFactory.Rect(image.rectTransform, anchor, new Vector2(size, size), position);
            LumiMobileActionButton actionButton = image.gameObject.AddComponent<LumiMobileActionButton>();
            actionButton.Action = action;
            Text text = LumiFactory.Text(image.transform, label, 22, TextAnchor.MiddleCenter, Color.white);
            LumiFactory.Stretch(text.rectTransform);
            text.raycastTarget = false;
            return image.gameObject;
        }
    }
}
