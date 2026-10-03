using System.IO;
using UnityEditor;
using UnityEngine;
using LumiAdventure;

[InitializeOnLoad]
public static class LumiChibiSculptBuild
{
    private const string Flag="QA/NarutoChibiSculpt.request";
    static LumiChibiSculptBuild(){EditorApplication.update+=Poll;}
    private static void Poll()
    {
        if(!File.Exists(Flag) || EditorApplication.isCompiling || EditorApplication.isUpdating)return;
        if(EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.isPlaying=false;return;}
        File.Delete(Flag);
        BuildNow();
    }
    public static void BuildNow(bool review=true)
    {
        GameObject character=null;
        try
        {
            const string dir="Assets/Resources/NarutoChibi";Directory.CreateDirectory(dir);
            character=LumiChibiNarutoModel.Create(null);var renderer=character.GetComponent<SkinnedMeshRenderer>();
            Save(renderer.sharedMesh,dir+"/Naruto-Sculpt-v3.asset");renderer.sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(dir+"/Naruto-Sculpt-v3.asset");
            Material material=renderer.sharedMaterial;Save(material.mainTexture,dir+"/Naruto-Palette-v3.asset");material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(dir+"/Naruto-Palette-v3.asset");Save(material,dir+"/Naruto-Soft-v3.mat");renderer.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(dir+"/Naruto-Soft-v3.mat");
            PrefabUtility.SaveAsPrefabAsset(character,dir+"/Naruto.prefab");
            int vertexCount=renderer.sharedMesh.vertexCount,boneCount=renderer.bones.Length;
            Object.DestroyImmediate(character);character=null;
            if(review && !Application.isBatchMode){LumiProductionBuild.PrepareAssets();LumiStartupScene.CreatePreview();}
            AssetDatabase.SaveAssets();
            Debug.Log("Naruto reference sculpt generated: "+vertexCount+" smooth vertices, "+boneCount+" animation bones.");
            if(review && !Application.isBatchMode)File.WriteAllText("QA/NarutoModelReview.request","review updated sculpt");
        }
        catch(System.Exception e){Debug.LogException(e);File.WriteAllText("Logs/NarutoQA/SculptBuild.txt",e.ToString());}
        finally{if(character!=null)Object.DestroyImmediate(character);}
    }
    private static void Save(Object asset,string path)
    {
        Object existing=AssetDatabase.LoadMainAssetAtPath(path);
        if(existing==null)AssetDatabase.CreateAsset(asset,path);
        else if(asset is Mesh source && existing is Mesh destination)
        {
            destination.Clear();destination.indexFormat=source.indexFormat;
            destination.vertices=source.vertices;destination.normals=source.normals;destination.uv=source.uv;
            destination.triangles=source.triangles;destination.boneWeights=source.boneWeights;destination.bindposes=source.bindposes;
            destination.bounds=source.bounds;EditorUtility.SetDirty(destination);
        }
        else{EditorUtility.CopySerialized(asset,existing);EditorUtility.SetDirty(existing);}
    }
}
