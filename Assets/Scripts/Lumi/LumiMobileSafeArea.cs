using UnityEngine;

namespace LumiAdventure
{
    /// <summary>Keeps mobile controls inside the device safe area.</summary>
    public sealed class LumiMobileSafeArea : MonoBehaviour
    {
        private RectTransform rect;
        private Rect lastArea;
        private Vector2Int lastScreen;

        private void Awake() { rect = transform as RectTransform; Apply(); }
        private void OnEnable() => Apply();

        private void Update()
        {
            if (lastArea != Screen.safeArea || lastScreen.x != Screen.width || lastScreen.y != Screen.height) Apply();
        }

        private void Apply()
        {
            if (rect == null || Screen.width <= 0 || Screen.height <= 0) return;
            Rect area = Screen.safeArea;
            lastArea = area;
            lastScreen = new Vector2Int(Screen.width, Screen.height);
            rect.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            rect.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
