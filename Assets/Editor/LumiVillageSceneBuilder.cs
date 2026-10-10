using System.Collections.Generic;
using System.IO;
using LumiAdventure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class LumiVillageSceneBuilder
{
    private const string Request="Temp/BuildVillageScenes.request";
    static LumiVillageSceneBuilder(){EditorApplication.update+=Poll;}

    private static void Poll()
    {
        if(!File.Exists(Request)||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
        File.Delete(Request);Build();
    }

    [MenuItem("Naruto/Build five village scenes")]
    public static void Build()
    {
        Directory.CreateDirectory("Assets/Scenes");
        var scenes=new List<EditorBuildSettingsScene>();
        scenes.Add(new EditorBuildSettingsScene("Assets/3D.unity",true));
        for(int level=1;level<=5;level++)
        {
            string path="Assets/Scenes/"+LumiVillageSceneMarker.SceneName(level)+".unity";
            Scene scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            GameObject marker=new GameObject("Village scene layout "+level,typeof(LumiVillageSceneMarker));
            marker.GetComponent<LumiVillageSceneMarker>().level=level;
            EditorSceneManager.SaveScene(scene,path);
            scenes.Add(new EditorBuildSettingsScene(path,true));
        }
        EditorBuildSettings.scenes=scenes.ToArray();
        EditorSceneManager.OpenScene("Assets/3D.unity");
        AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        Debug.Log("Created five separate village scenes and added them to Build Settings.");
    }
}
