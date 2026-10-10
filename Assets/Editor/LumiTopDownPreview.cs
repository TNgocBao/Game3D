using System.Collections;
using System.IO;
using LumiAdventure;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class LumiTopDownPreview
{
    private const string Request="Temp/TopDownPreview.request";
    static LumiTopDownPreview(){EditorApplication.update+=Poll;EditorApplication.playModeStateChanged+=OnPlay;}

    private static void Poll()
    {
        if(!File.Exists(Request)||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
        File.Delete(Request);SessionState.SetBool("Lumi.TopDownPreview",true);EditorApplication.isPlaying=true;
    }

    private static void OnPlay(PlayModeStateChange state)
    {
        if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("Lumi.TopDownPreview",false))
        {
            SessionState.SetBool("Lumi.TopDownPreview",false);
            LumiGame game=Object.FindObjectOfType<LumiGame>();
            if(game!=null)game.StartCoroutine(CaptureAll(game));
        }
    }

    [MenuItem("Naruto/Preview five villages top-down")]
    public static void Run()
    {
        if(!Application.isPlaying){SessionState.SetBool("Lumi.TopDownPreview",true);EditorApplication.isPlaying=true;return;}
        LumiGame game=Object.FindObjectOfType<LumiGame>();if(game!=null)game.StartCoroutine(CaptureAll(game));
    }

    private static IEnumerator CaptureAll(LumiGame game)
    {
        Directory.CreateDirectory("Logs/TopDownQA");
        for(int level=1;level<=5;level++)
        {
            game.StartLevelForValidation(level);yield return null;yield return null;
            Camera camera=game.CameraRig.ViewCamera;
            bool wasOrtho=camera.orthographic;float oldSize=camera.orthographicSize;RenderTexture oldTarget=camera.targetTexture;
            camera.orthographic=true;camera.orthographicSize=96f;camera.fieldOfView=60f;
            camera.transform.position=new Vector3(0,128,0);camera.transform.rotation=Quaternion.Euler(90,0,0);
            Capture(camera,"Logs/TopDownQA/Village"+level+"_topdown.png");
            camera.orthographic=wasOrtho;camera.orthographicSize=oldSize;camera.targetTexture=oldTarget;
        }
        game.ShowLevelMenu();EditorApplication.isPlaying=false;
    }

    private static void Capture(Camera camera,string file)
    {
        RenderTexture rt=RenderTexture.GetTemporary(1280,720,24);Texture2D image=new Texture2D(1280,720,TextureFormat.RGB24,false);
        RenderTexture oldTarget=camera.targetTexture;RenderTexture oldActive=RenderTexture.active;
        try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(file,image.EncodeToPNG());}
        finally{camera.targetTexture=oldTarget;RenderTexture.active=oldActive;RenderTexture.ReleaseTemporary(rt);Object.Destroy(image);}
    }
}
