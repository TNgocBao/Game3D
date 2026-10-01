using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using TMPro;

public static class LumiTypography
{
    [MenuItem("Vanguard/Prepare crisp typography")]
    public static void Prepare()
    {
        var chars=new StringBuilder();
        for(int i=32;i<383;i++)chars.Append((char)i);
        for(int i=0x1ea0;i<=0x1ef9;i++)chars.Append((char)i);
        chars.Append("★☆›−×→•…–—");
        foreach(string name in new[]{"NotoSansSymbols2-Regular","Inter-Medium","Inter-SemiBold","Inter-Bold"})
        {
            string path="Assets/Resources/LumiFonts/"+name+"-SDF.asset";
            if(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path)!=null)continue;
            Font source=Resources.Load<Font>("LumiFonts/"+name);
            TMP_FontAsset font=TMP_FontAsset.CreateFontAsset(source,90,10,GlyphRenderMode.SDFAA,2048,2048,AtlasPopulationMode.Dynamic);
            font.name=name+"-SDF";
            font.fallbackFontAssetTable=new System.Collections.Generic.List<TMP_FontAsset>();
            font.TryAddCharacters(name.StartsWith("Noto")?"★☆→":chars.ToString(),out string missing);
            TMP_FontAsset symbols=Resources.Load<TMP_FontAsset>("LumiFonts/NotoSansSymbols2-Regular-SDF");
            if(!name.StartsWith("Noto") && symbols!=null)font.fallbackFontAssetTable.Add(symbols);
            TMP_FontAsset fallback=Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if(fallback!=null)font.fallbackFontAssetTable.Add(fallback);
            AssetDatabase.CreateAsset(font,path);
            font.material.name=font.name+" Material";AssetDatabase.AddObjectToAsset(font.material,font);
            foreach(Texture2D atlas in font.atlasTextures){atlas.name=font.name+" Atlas";atlas.filterMode=FilterMode.Bilinear;AssetDatabase.AddObjectToAsset(atlas,font);}
            EditorUtility.SetDirty(font);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Vanguard: signed-distance fonts ready with Vietnamese glyphs.");
    }
}
