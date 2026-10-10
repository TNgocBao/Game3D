using UnityEngine;

namespace LumiAdventure
{
    /// <summary>Selects one equally likely NPC blessing per level and grants it once.</summary>
    public sealed class LumiNpcBuffService : MonoBehaviour
    {
        public const float DurationSeconds = 90f;
        private LumiGame game;

        public LumiBuffType CurrentBuff { get; private set; }
        public bool Granted { get; private set; }

        public void Initialize(LumiGame owner) => game = owner;

        public void BeginLevel()
        {
            CurrentBuff = (LumiBuffType)Random.Range(0, 3);
            Granted = false;
            if (game.BuffStatusView != null) game.BuffStatusView.SetNpcOffer(CurrentBuff, false);
            game.ShowToast("NPC đang chờ bạn: " + DisplayName(CurrentBuff), Accent(CurrentBuff));
        }

        public string TalkTo(LumiPlayer player)
        {
            string encouragement;
            switch (CurrentBuff)
            {
                case LumiBuffType.Attack: encouragement = "Hãy tiến lên! Ta truyền cho con sức mạnh, sát thương tăng ×1.5 trong 90 giây."; break;
                case LumiBuffType.Speed: encouragement = "Đừng chùn bước! Tốc độ của con tăng ×1.5 trong 90 giây."; break;
                default: encouragement = "Vững lòng nhé! Con sẽ giảm 50% sát thương nhận vào trong 90 giây."; break;
            }

            if (!Granted)
            {
                Granted = true;
                player.ApplyNpcBuff(CurrentBuff, DurationSeconds);
                game.BuffStatusView.SetNpcOffer(CurrentBuff, false);
                game.ShowToast("ĐÃ NHẬN " + DisplayName(CurrentBuff).ToUpperInvariant(), Accent(CurrentBuff));
            }
            return encouragement;
        }

        public static string DisplayName(LumiBuffType type)
        {
            switch (type)
            {
                case LumiBuffType.Attack: return "Công ×1.5";
                case LumiBuffType.Speed: return "Tốc độ ×1.5";
                case LumiBuffType.Defense: return "Giảm sát thương 50%";
                default: return "Tàng hình";
            }
        }

        public static Color Accent(LumiBuffType type)
        {
            switch (type)
            {
                case LumiBuffType.Attack: return new Color(1f, .38f, .16f);
                case LumiBuffType.Speed: return new Color(.2f, .9f, 1f);
                case LumiBuffType.Defense: return new Color(.3f, .65f, 1f);
                default: return new Color(.72f, .55f, 1f);
            }
        }
    }
}
