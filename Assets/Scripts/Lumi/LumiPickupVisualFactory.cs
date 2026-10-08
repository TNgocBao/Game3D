using UnityEngine;

namespace LumiAdventure
{
    /// <summary>Builds readable pickup badges from the shared icon set.</summary>
    public static class LumiPickupVisualFactory
    {
        public static void Build(Transform parent, LumiPickupType type)
        {
            GameObject badge = new GameObject(type + " pickup icon", typeof(SpriteRenderer));
            badge.transform.SetParent(parent, false);
            SpriteRenderer renderer = badge.GetComponent<SpriteRenderer>();
            renderer.sprite = LumiIconLibrary.Get(LumiIconLibrary.FromPickup(type));
            renderer.sortingOrder = 12;
            badge.transform.localScale = Vector3.one * 1.08f;
            badge.AddComponent<LumiWorldIconBillboard>();

            GameObject glow = new GameObject("Pickup glow", typeof(Light));
            glow.transform.SetParent(parent, false);
            Light light = glow.GetComponent<Light>();
            light.color = type == LumiPickupType.Health ? new Color(1f, .18f, .24f) :
                type == LumiPickupType.Armor ? new Color(.2f, .7f, 1f) :
                type == LumiPickupType.Speed ? new Color(.2f, .9f, 1f) :
                type == LumiPickupType.Invisibility ? new Color(.7f, .45f, 1f) : new Color(1f, .8f, .1f);
            light.range = 2.3f; light.intensity = .65f;
        }
    }
}
