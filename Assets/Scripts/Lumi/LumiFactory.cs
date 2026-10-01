using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TextCore.LowLevel;
using Text = TMPro.TextMeshProUGUI;

namespace LumiAdventure
{
    public static class LumiFactory
    {
        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();
        private static TMP_FontAsset cachedFont;
        private static TMP_FontAsset headingFont;
        private static TMP_FontAsset buttonFont;

        public static TMP_FontAsset Font
        {
            get
            {
                if (cachedFont == null) cachedFont = LoadFont("Inter-Medium");
                return cachedFont;
            }
        }

        private static TMP_FontAsset LoadFont(string name)
        {
            TMP_FontAsset font=Resources.Load<TMP_FontAsset>("LumiFonts/"+name+"-SDF");
            if(font!=null)return font;
            Font source=Resources.Load<UnityEngine.Font>("LumiFonts/"+name) ?? Resources.GetBuiltinResource<UnityEngine.Font>("Arial.ttf");
            return TMP_FontAsset.CreateFontAsset(source,90,10,GlyphRenderMode.SDFAA,2048,2048,AtlasPopulationMode.Dynamic);
        }

        public static Material Material(string key, Color color, bool emission = false, bool transparent = false)
        {
            string cacheKey = key + color + emission + transparent;
            if (Materials.TryGetValue(cacheKey, out var existing) && existing != null) return existing;

            Shader shader = Shader.Find("Standard");
            var material = new Material(shader) { name = "Lumi_" + key, color = color };
            material.SetFloat("_Glossiness", key.Contains("metal") ? .3f : .08f);
            if (emission)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 1.4f);
            }

            if (transparent)
            {
                material.SetFloat("_Mode", 3f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }

            Materials[cacheKey] = material;
            return material;
        }

        public static GameObject Primitive(string name, PrimitiveType type, Transform parent, Vector3 position,
            Vector3 scale, Material material, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            if (!collider)
            {
                var primitiveCollider = go.GetComponent<Collider>();
                if (primitiveCollider != null)
                {
                    primitiveCollider.enabled = false;
                    Object.Destroy(primitiveCollider);
                }
            }
            return go;
        }

        public static GameObject WorldObject(string name, Transform parent, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            return go;
        }

        public static Text Text(Transform parent, string value, int size, TextAnchor alignment, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            if(headingFont==null)headingFont=LoadFont("Inter-Bold");
            text.font = size >= 40 ? headingFont : Font;
            text.raycastTarget = false;
            text.text = value;
            text.fontSize = Mathf.Max(32,size);
            TextAlignmentOptions[] alignments={TextAlignmentOptions.TopLeft,TextAlignmentOptions.Top,TextAlignmentOptions.TopRight,
                TextAlignmentOptions.Left,TextAlignmentOptions.Center,TextAlignmentOptions.Right,
                TextAlignmentOptions.BottomLeft,TextAlignmentOptions.Bottom,TextAlignmentOptions.BottomRight};
            text.alignment = alignments[(int)alignment];
            text.color = color;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow;
            text.enableAutoSizing = false;
            text.extraPadding = true;
            return text;
        }

        public static Image Image(Transform parent, Color color)
        {
            var go = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        public static Button Button(Transform parent, string label, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var image = Image(parent, color);
            image.gameObject.name = label;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.2f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
            colors.disabledColor = new Color(color.r, color.g, color.b, 0.3f);
            button.colors = colors;
            button.onClick.AddListener(onClick);
            image.gameObject.AddComponent<LumiUiSound>();
            var text = Text(image.transform, label, 25, TextAnchor.MiddleCenter, Color.white);
            if(buttonFont==null)buttonFont=LoadFont("Inter-SemiBold");
            text.font=buttonFont;
            Stretch(text.rectTransform);
            return button;
        }

        public static void Stretch(RectTransform rect, float margin = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(margin, margin);
            rect.offsetMax = new Vector2(-margin, -margin);
        }

        public static void Rect(RectTransform rect, Vector2 anchor, Vector2 size, Vector2 position)
        {
            Text label=rect.GetComponent<Text>();
            if(label!=null)size.y=Mathf.Max(size.y,label.fontSize*1.55f);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        public static void DestroyChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i);
                child.gameObject.SetActive(false);
                Object.Destroy(child.gameObject);
            }
        }
    }
}

