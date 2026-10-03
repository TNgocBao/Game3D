using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;
using LumiAdventure;

public static class LumiStartupScene
{
    [MenuItem("Vanguard/Create Vanguard startup preview")]
    public static void CreatePreview()
    {
        if(EditorApplication.isPlaying){Debug.LogWarning("Stop Play before preparing startup preview.");return;}
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        LumiTypography.Prepare();
        Directory.CreateDirectory("Assets/Legacy");
        const string oldPath="Assets/Scenes/SampleScene.unity";
        const string backup="Assets/Legacy/MarioSampleScene.unity";
        if(File.Exists(oldPath) && !File.Exists(backup))AssetDatabase.CopyAsset(oldPath,backup);
        Scene scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
        var ui=new GameObject("Naruto Startup Preview",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
        ui.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler=ui.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1920,1080);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
        var art=new GameObject("Naruto chibi artwork",typeof(RectTransform),typeof(CanvasRenderer),typeof(LumiCoverImage));
        art.transform.SetParent(ui.transform,false);var image=art.GetComponent<LumiCoverImage>();
        image.texture=Resources.Load<Texture2D>("LumiReference/NarutoMenu");image.raycastTarget=false;LumiFactory.Stretch(image.rectTransform);image.UpdateCrop();
        Image veil=LumiFactory.Image(ui.transform,new Color(.015f,.025f,.045f,.25f));LumiFactory.Stretch(veil.rectTransform);veil.raycastTarget=false;
        Image titlePanel=LumiFactory.Image(ui.transform,new Color(.02f,.035f,.065f,.78f));
        LumiFactory.Rect(titlePanel.rectTransform,new Vector2(.27f,.57f),new Vector2(820,330),Vector2.zero);titlePanel.raycastTarget=false;
        Text title=LumiFactory.Text(titlePanel.transform,"NARUTO",100,TextAnchor.MiddleCenter,Color.white);
        LumiFactory.Rect(title.rectTransform,new Vector2(.5f,.67f),new Vector2(780,150),Vector2.zero);
        Text subtitle=LumiFactory.Text(titlePanel.transform,"HÀNH TRÌNH NGŨ ĐẠI NHẪN THÔN",32,TextAnchor.MiddleCenter,new Color(1f,.78f,.36f));
        LumiFactory.Rect(subtitle.rectTransform,new Vector2(.5f,.27f),new Vector2(780,90),Vector2.zero);
        Text hint=LumiFactory.Text(ui.transform,"Nhấn Play trong Unity để mở chiến dịch",28,TextAnchor.MiddleCenter,Color.white);
        LumiFactory.Rect(hint.rectTransform,new Vector2(.5f,.15f),new Vector2(1600,70),Vector2.zero);
        EditorSceneManager.SaveScene(scene,"Assets/3D.unity");
        EditorSceneManager.SaveScene(scene,oldPath,true);
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Start.unity",true);
        Debug.Log("Vanguard startup preview saved. Original Mario scene preserved under Assets/Legacy.");
    }
}
