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
        var ui=new GameObject("Vanguard Startup Preview",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
        ui.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler=ui.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1920,1080);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
        var art=new GameObject("Camp artwork",typeof(RectTransform),typeof(CanvasRenderer),typeof(LumiCoverImage));
        art.transform.SetParent(ui.transform,false);var image=art.GetComponent<LumiCoverImage>();
        image.texture=Resources.Load<Texture2D>("LumiReference/Camp");image.raycastTarget=false;LumiFactory.Stretch(image.rectTransform);image.UpdateCrop();
        Image veil=LumiFactory.Image(ui.transform,new Color(.02f,.04f,.06f,.58f));LumiFactory.Stretch(veil.rectTransform);veil.raycastTarget=false;
        Text title=LumiFactory.Text(ui.transform,"V A N G U A R D",80,TextAnchor.MiddleCenter,Color.white);
        LumiFactory.Rect(title.rectTransform,new Vector2(.5f,.58f),new Vector2(1600,140),Vector2.zero);
        Text subtitle=LumiFactory.Text(ui.transform,"BIÊN NIÊN SỬ NĂM VÙNG ĐẤT",32,TextAnchor.MiddleCenter,new Color(.96f,.83f,.52f));
        LumiFactory.Rect(subtitle.rectTransform,new Vector2(.5f,.45f),new Vector2(1600,70),Vector2.zero);
        Text hint=LumiFactory.Text(ui.transform,"Nhấn Play trong Unity để mở chiến dịch",28,TextAnchor.MiddleCenter,Color.white);
        LumiFactory.Rect(hint.rectTransform,new Vector2(.5f,.15f),new Vector2(1600,70),Vector2.zero);
        EditorSceneManager.SaveScene(scene,"Assets/3D.unity");
        EditorSceneManager.SaveScene(scene,oldPath,true);
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Start.unity",true);
        Debug.Log("Vanguard startup preview saved. Original Mario scene preserved under Assets/Legacy.");
    }
}
