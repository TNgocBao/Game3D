using System.Collections.Generic;
using UnityEngine;

namespace LumiAdventure
{
    public enum LumiIconKind { Health, Armor, Speed, Invisibility, Attack, Defense, Star }

    /// <summary>Creates one cached, resolution-independent-style icon set for world items and HUD widgets.</summary>
    public static class LumiIconLibrary
    {
        private const int Size = 96;
        private static readonly Dictionary<LumiIconKind, Sprite> Cache = new Dictionary<LumiIconKind, Sprite>();
        private static Sprite shieldEmblem;

        public static Sprite Get(LumiIconKind kind)
        {
            Sprite sprite;
            if (Cache.TryGetValue(kind, out sprite) && sprite != null) return sprite;
            Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            texture.name = "Lumi " + kind + " icon";
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            Color accent = Accent(kind);
            Color[] pixels = new Color[Size * Size];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    Vector2 p = new Vector2((x + .5f) / Size * 2f - 1f, (y + .5f) / Size * 2f - 1f);
                    float radius = p.magnitude;
                    Color color = radius < .92f ? new Color(.025f, .06f, .1f, Mathf.SmoothStep(0f, .94f, 1.02f - radius)) : Color.clear;
                    if (radius > .78f && radius < .9f) color = accent;
                    if (Inside(kind, p)) color = Color.Lerp(Color.white, accent, .28f);
                    pixels[y * Size + x] = color;
                }
            texture.SetPixels(pixels); texture.Apply(false, true);
            sprite = Sprite.Create(texture, new Rect(0, 0, Size, Size), Vector2.one * .5f, 100f);
            sprite.name = kind + " Icon";
            Cache[kind] = sprite;
            return sprite;
        }

        public static Sprite GetShieldEmblem()
        {
            if(shieldEmblem!=null)return shieldEmblem;
            Texture2D texture=new Texture2D(Size,Size,TextureFormat.RGBA32,false);
            texture.name="Lumi shield emblem";texture.filterMode=FilterMode.Bilinear;texture.wrapMode=TextureWrapMode.Clamp;
            Color[] pixels=new Color[Size*Size];Color edge=new Color(.25f,.78f,1f,.92f),fill=new Color(.78f,.96f,1f,.72f);
            for(int y=0;y<Size;y++)for(int x=0;x<Size;x++)
            {
                Vector2 p=new Vector2((x+.5f)/Size*2-1,(y+.5f)/Size*2-1);
                bool outer=ShieldShape(p,.64f,.66f),inner=ShieldShape(p,.48f,.52f);
                pixels[y*Size+x]=inner?fill:outer?edge:Color.clear;
            }
            texture.SetPixels(pixels);texture.Apply(false,true);
            shieldEmblem=Sprite.Create(texture,new Rect(0,0,Size,Size),Vector2.one*.5f,100);shieldEmblem.name="Shield Emblem";
            return shieldEmblem;
        }

        public static LumiIconKind FromPickup(LumiPickupType type)
        {
            switch (type)
            {
                case LumiPickupType.Health: return LumiIconKind.Health;
                case LumiPickupType.Armor: return LumiIconKind.Armor;
                case LumiPickupType.Speed: return LumiIconKind.Speed;
                case LumiPickupType.Invisibility: return LumiIconKind.Invisibility;
                default: return LumiIconKind.Star;
            }
        }

        public static LumiIconKind FromBuff(LumiBuffType type)
        {
            switch (type)
            {
                case LumiBuffType.Attack: return LumiIconKind.Attack;
                case LumiBuffType.Speed: return LumiIconKind.Speed;
                case LumiBuffType.Defense: return LumiIconKind.Defense;
                default: return LumiIconKind.Invisibility;
            }
        }

        private static Color Accent(LumiIconKind kind)
        {
            switch (kind)
            {
                case LumiIconKind.Health: return new Color(1f, .18f, .26f, 1f);
                case LumiIconKind.Armor:
                case LumiIconKind.Defense: return new Color(.2f, .68f, 1f, 1f);
                case LumiIconKind.Speed: return new Color(.16f, .92f, 1f, 1f);
                case LumiIconKind.Invisibility: return new Color(.72f, .5f, 1f, 1f);
                case LumiIconKind.Attack: return new Color(1f, .38f, .12f, 1f);
                default: return new Color(1f, .8f, .08f, 1f);
            }
        }

        private static bool Inside(LumiIconKind kind, Vector2 p)
        {
            switch (kind)
            {
                case LumiIconKind.Health:
                    bool lobes = (p - new Vector2(-.22f, .18f)).sqrMagnitude < .22f || (p - new Vector2(.22f, .18f)).sqrMagnitude < .22f;
                    bool heartBody = Mathf.Abs(p.x) + Mathf.Abs(p.y + .08f) < .68f && p.y < .38f;
                    return lobes || heartBody;
                case LumiIconKind.Armor:
                case LumiIconKind.Defense:
                    bool shield = p.y < .58f && p.y > -.62f && Mathf.Abs(p.x) < Mathf.Lerp(.16f, .54f, Mathf.InverseLerp(-.62f, .48f, p.y));
                    bool cutout = p.y > -.08f && p.y < .08f && Mathf.Abs(p.x) < .36f;
                    return shield && !cutout;
                case LumiIconKind.Speed:
                    bool shoe = p.x > -.52f && p.x < .56f && p.y > -.42f && p.y < -.08f && p.x + p.y > -.68f;
                    bool ankle = p.x < -.16f && p.x > -.5f && p.y >= -.12f && p.y < .48f;
                    bool wing = p.x > -.05f && p.x < .62f && Mathf.Abs(p.y - (.38f - p.x * .48f)) < .09f;
                    return shoe || ankle || wing;
                case LumiIconKind.Invisibility:
                    float eye = p.x * p.x / .42f + p.y * p.y / .1f;
                    bool eyeRing = eye < 1f && eye > .48f;
                    bool pupil = p.sqrMagnitude < .055f;
                    bool slash = Mathf.Abs(p.y - p.x) < .075f && Mathf.Abs(p.x) < .62f;
                    return eyeRing || pupil || slash;
                case LumiIconKind.Attack:
                    bool boltA = p.x > -.18f && p.x < .24f && p.y > -.62f && p.y < .65f && p.y > -2.1f * p.x - .14f;
                    bool boltB = p.x > -.36f && p.x < .14f && p.y > -.5f && p.y < .22f && p.y < -2.1f * p.x + .08f;
                    return boltA || boltB;
                default:
                    float angle = Mathf.Atan2(p.y, p.x);
                    float starRadius = Mathf.Lerp(.34f, .7f, .5f + .5f * Mathf.Cos(angle * 5f));
                    return p.magnitude < starRadius;
            }
        }

        private static bool ShieldShape(Vector2 p,float halfWidth,float height)
        {
            if(p.y>.62f||p.y<-.72f)return false;
            float normalized=Mathf.InverseLerp(-.72f,.52f,p.y);
            float width=Mathf.Lerp(.05f,halfWidth,Mathf.Pow(normalized,.56f));
            return Mathf.Abs(p.x)<width&&p.y<height;
        }
    }

    public sealed class LumiWorldIconBillboard : MonoBehaviour
    {
        private Camera targetCamera;
        private void LateUpdate()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera != null) transform.rotation = targetCamera.transform.rotation;
        }
    }
}
