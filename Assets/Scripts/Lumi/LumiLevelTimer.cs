using UnityEngine;
using Text = TMPro.TextMeshProUGUI;

namespace LumiAdventure
{
    /// <summary>Owns the per-level countdown and its HUD presentation.</summary>
    public sealed class LumiLevelTimer : MonoBehaviour
    {
        public const float DefaultDurationSeconds = 15f * 60f;

        private LumiGame game;
        private Text display;

        public float RemainingSeconds { get; private set; }
        public bool Expired { get; private set; }

        public void Initialize(LumiGame owner) => game = owner;

        public void BindDisplay(Text timerDisplay)
        {
            display = timerDisplay;
            RefreshDisplay();
        }

        public void BeginLevel(float durationSeconds = DefaultDurationSeconds)
        {
            RemainingSeconds = Mathf.Max(0f, durationSeconds);
            Expired = false;
            RefreshDisplay();
        }

        private void Update()
        {
            if (game == null || !game.IsPlaying || Expired) return;
            RemainingSeconds = Mathf.Max(0f, RemainingSeconds - Time.deltaTime);
            RefreshDisplay();
            if (RemainingSeconds > 0f) return;

            Expired = true;
            game.LoseLevel();
        }

        private void RefreshDisplay()
        {
            if (display == null) return;
            int totalSeconds = Mathf.CeilToInt(RemainingSeconds);
            display.text = "THỜI GIAN  " + (totalSeconds / 60).ToString("00") + ":" + (totalSeconds % 60).ToString("00");
            display.color = RemainingSeconds <= 60f
                ? new Color(1f, .35f, .2f)
                : new Color(1f, .9f, .45f);
        }
    }
}
