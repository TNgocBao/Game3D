using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;

namespace LumiAdventure
{
    /// <summary>Presents timed buff icons beside the mobile camera control.</summary>
    public sealed class LumiBuffStatusView : MonoBehaviour
    {
        private sealed class Slot
        {
            public GameObject Root;
            public Image Icon;
            public Text Time;
        }

        private readonly List<LumiActiveBuff> active = new List<LumiActiveBuff>(4);
        private readonly Slot[] slots = new Slot[4];
        private LumiGame game;
        private GameObject offerRoot;
        private Image offerIcon;
        private Text offerLabel;

        public void Initialize(LumiGame owner, Transform hudRoot)
        {
            game = owner;
            BuildOffer(hudRoot);
            BuildActiveSlots(hudRoot);
        }

        public void SetNpcOffer(LumiBuffType type, bool available)
        {
            if (offerRoot == null) return;
            offerRoot.SetActive(available);
            offerIcon.sprite = LumiIconLibrary.Get(LumiIconLibrary.FromBuff(type));
            offerLabel.text = "NPC\n" + ShortName(type);
        }

        private void LateUpdate()
        {
            if (game == null || game.Player == null || game.Player.Buffs == null)
            {
                HideSlots(); return;
            }
            game.Player.Buffs.CollectActive(active);
            for (int i = 0; i < slots.Length; i++)
            {
                bool visible = i < active.Count;
                if (slots[i] == null || slots[i].Root == null) continue;
                slots[i].Root.SetActive(visible);
                if (!visible) continue;
                LumiActiveBuff buff = active[i];
                slots[i].Icon.sprite = LumiIconLibrary.Get(LumiIconLibrary.FromBuff(buff.Type));
                slots[i].Time.text = Mathf.CeilToInt(buff.RemainingSeconds) + "s";
            }
        }

        private void BuildOffer(Transform hudRoot)
        {
            Image panel = LumiFactory.Image(hudRoot, new Color(.025f, .06f, .1f, .9f));
            panel.gameObject.name = "NPC buff offer";
            panel.sprite = LumiAbilityHud.Circle; panel.type = Image.Type.Simple;
            LumiFactory.Rect(panel.rectTransform, new Vector2(0f, 1f), new Vector2(118f, 118f), new Vector2(505f, -115f));
            panel.raycastTarget = false;
            offerRoot = panel.gameObject;
            offerIcon = LumiFactory.Image(panel.transform, Color.white);
            LumiFactory.Rect(offerIcon.rectTransform, new Vector2(.5f, .62f), new Vector2(62f, 62f), Vector2.zero);
            offerIcon.raycastTarget = false;
            offerLabel = LumiFactory.Text(panel.transform, "NPC", 15, TextAnchor.MiddleCenter, Color.white);
            LumiFactory.Rect(offerLabel.rectTransform, new Vector2(.5f, .18f), new Vector2(105f, 40f), Vector2.zero);
            offerLabel.raycastTarget = false;
            offerRoot.SetActive(false);
        }

        private void BuildActiveSlots(Transform hudRoot)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                Image panel = LumiFactory.Image(hudRoot, new Color(.025f, .06f, .1f, .92f));
                panel.gameObject.name = "Active buff " + i;
                panel.sprite = LumiAbilityHud.Circle; panel.type = Image.Type.Simple;
            LumiFactory.Rect(panel.rectTransform, new Vector2(0f, 1f), new Vector2(82f, 82f), new Vector2(245f + i * 92f, -285f));
                panel.raycastTarget = false;
                Image icon = LumiFactory.Image(panel.transform, Color.white);
                LumiFactory.Rect(icon.rectTransform, new Vector2(.5f, .58f), new Vector2(57f, 57f), Vector2.zero);
                icon.raycastTarget = false;
                Text time = LumiFactory.Text(panel.transform, "", 17, TextAnchor.MiddleCenter, Color.white);
                LumiFactory.Rect(time.rectTransform, new Vector2(.5f, .13f), new Vector2(75f, 24f), Vector2.zero);
                time.raycastTarget = false;
                slots[i] = new Slot { Root = panel.gameObject, Icon = icon, Time = time };
                panel.gameObject.SetActive(false);
            }
        }

        private void HideSlots()
        {
            for (int i = 0; i < slots.Length; i++) if (slots[i] != null) slots[i].Root.SetActive(false);
        }

        private static string ShortName(LumiBuffType type)
        {
            switch (type)
            {
                case LumiBuffType.Attack: return "CÔNG";
                case LumiBuffType.Speed: return "TỐC";
                case LumiBuffType.Defense: return "THỦ";
                default: return "ẨN";
            }
        }
    }
}
