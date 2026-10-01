using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using LumiAdventure;

public sealed class LumiProductionBuild : IPreprocessBuildWithReport
{
    public int callbackOrder=>0;
    public void OnPreprocessBuild(BuildReport report)=>PrepareAssets();

    [MenuItem("Vanguard/Prepare production assets")]
    public static void PrepareAssets()
    {
        LumiTypography.Prepare();
        const string catalogPath="Assets/Resources/LumiAssetCatalog.asset";
        LumiAssetCatalog catalog=AssetDatabase.LoadAssetAtPath<LumiAssetCatalog>(catalogPath);
        if(catalog==null){catalog=ScriptableObject.CreateInstance<LumiAssetCatalog>();AssetDatabase.CreateAsset(catalog,catalogPath);}
        var entries=new List<LumiAssetCatalog.Entry>();
        foreach(string guid in AssetDatabase.FindAssets("",new[]{"Assets/ThirdParty/Kenney"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);
            if(!path.EndsWith(".fbx") && !path.EndsWith(".png"))continue;
            // Only package assets used by the game, excluding previews and unused packs.
            if(!path.EndsWith("Blasters/Models/FBX format/blaster-l.fbx") && !path.EndsWith("Blasters/Models/FBX format/blaster-r.fbx"))continue;
            Object asset=AssetDatabase.LoadMainAssetAtPath(path);
            if(asset!=null)entries.Add(new LumiAssetCatalog.Entry{path=path,asset=asset});
        }
        catalog.entries=entries.ToArray();EditorUtility.SetDirty(catalog);
        foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Resources/LumiReference"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null)continue;
            importer.maxTextureSize=2048;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Vanguard: production assets prepared, references included in standalone builds.");
    }
}
