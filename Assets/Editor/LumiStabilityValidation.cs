using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LumiAdventure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

[InitializeOnLoad]
public static class LumiStabilityValidation
{
    const string Request="Temp/StabilityValidation.request";
    const string Running="Lumi.StabilityValidation";

    static LumiStabilityValidation(){EditorApplication.update+=Poll;}

    static void Poll()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        if(File.Exists(Request))
        {
            if(EditorApplication.isPlaying){EditorApplication.isPlaying=false;return;}
            File.Delete(Request);
            PlayerPrefs.SetInt("Lumi.Unlocked",5);
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/3D.unity");
            SessionState.SetBool(Running,true);
            EditorApplication.isPlaying=true;
        }
        if(!SessionState.GetBool(Running,false)||!EditorApplication.isPlaying)return;
        LumiGame game=UnityEngine.Object.FindObjectOfType<LumiGame>();
        if(game==null)return;
        SessionState.SetBool(Running,false);
        game.StartCoroutine(Check(game));
    }

    static IEnumerator Check(LumiGame game)
    {
        Directory.CreateDirectory("Logs/StabilityQA");
        var errors=new List<string>();
        var results=new List<string>();
        Application.LogCallback callback=(message,stack,type)=>
        {
            if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)
                errors.Add(type+": "+message);
        };
        Application.logMessageReceived+=callback;

        game.StartLevelForValidation(1);
        yield return null;
        ValidateShield(game,errors,results);
        Capture(game,"Logs/StabilityQA/ShieldMesh.png");

        MethodInfo transition=typeof(LumiGame).GetMethod("TransitionToLevel",BindingFlags.Instance|BindingFlags.NonPublic);
        for(int level=1;level<=5;level++)
        {
            if(level>1)
            {
                bool transitionRunning=true;int missingCameraFrames=0;
                game.StartCoroutine(MonitorCameras(()=>transitionRunning,()=>missingCameraFrames++));
                IEnumerator routine=(IEnumerator)transition.Invoke(game,new object[]{level});
                yield return game.StartCoroutine(routine);
                transitionRunning=false;
                yield return null;
                if(missingCameraFrames>0)errors.Add("Map "+level+" transition had "+missingCameraFrames+" frame(s) without a rendering camera");
                foreach(Camera camera in UnityEngine.Object.FindObjectsOfType<Camera>())if(camera.name=="Village transition camera")errors.Add("Map "+level+" transition camera was not cleaned up");
            }
            yield return null;
            // Let ranged/melee AI execute obstacle avoidance before freezing the scene for stable metrics.
            // This catches the non-normalized SphereCast assertion that previously flooded maps 2-5.
            for(int activeFrame=0;activeFrame<30;activeFrame++)yield return null;
            if(game.Player!=null)game.Player.enabled=false;
            if(game.VillageBoss!=null)game.VillageBoss.enabled=false;
            foreach(LumiEnemy enemy in UnityEngine.Object.FindObjectsOfType<LumiEnemy>())enemy.enabled=false;
            yield return Resources.UnloadUnusedAssets();
            yield return new WaitForSecondsRealtime(.5f);

            int worlds=0;foreach(Transform child in game.transform)if(child.name=="Lumi World")worlds++;
            if(worlds!=1)errors.Add("Map "+level+" has "+worlds+" Lumi World roots");
            int enemyCount=UnityEngine.Object.FindObjectsOfType<LumiEnemy>().Length;
            int renderers=UnityEngine.Object.FindObjectsOfType<Renderer>().Length;
            int particles=UnityEngine.Object.FindObjectsOfType<ParticleSystem>().Length;
            int colliders=UnityEngine.Object.FindObjectsOfType<Collider>().Length;
            long memory=Profiler.GetTotalAllocatedMemoryLong();
            var frames=new List<float>();
            for(int i=0;i<45;i++){frames.Add(Time.unscaledDeltaTime*1000f);yield return null;}
            frames.Sort();float sum=0;foreach(float frame in frames)sum+=frame;
            results.Add("Map "+level+": enemies="+enemyCount+", renderers="+renderers+", particles="+particles+", colliders="+colliders+", allocated="+(memory/1048576f).ToString("F1")+" MB, avg="+(sum/frames.Count).ToString("F2")+" ms, p95="+frames[Mathf.Min(frames.Count-1,Mathf.FloorToInt(frames.Count*.95f))].ToString("F2")+" ms");
        }

        Application.logMessageReceived-=callback;
        File.WriteAllText("Logs/StabilityQA/Validation.txt",(errors.Count==0?"PASS":"FAIL")+"\n"+string.Join("\n",results)+"\n"+string.Join("\n",errors));
        game.ShowLevelMenu();
        EditorApplication.isPlaying=false;
    }

    static IEnumerator MonitorCameras(Func<bool> active,Action missing)
    {
        while(active())
        {
            if(Camera.allCamerasCount==0)missing();
            yield return null;
        }
    }

    static void ValidateShield(LumiGame game,List<string> errors,List<string> results)
    {
        LumiShieldFormation formation=game.Player.GetComponentInChildren<LumiShieldFormation>(true);
        if(formation==null){errors.Add("Shield formation missing");return;}
        MeshRenderer[] meshes=formation.GetComponentsInChildren<MeshRenderer>(true);
        SpriteRenderer[] sprites=formation.GetComponentsInChildren<SpriteRenderer>(true);
        if(meshes.Length!=4)errors.Add("Shield mesh count="+meshes.Length+", expected 4");
        if(sprites.Length!=0)errors.Add("Shield still depends on "+sprites.Length+" SpriteRenderer(s)");
        foreach(MeshRenderer renderer in meshes)
        {
            MeshFilter filter=renderer.GetComponent<MeshFilter>();
            if(filter==null||filter.sharedMesh==null||filter.sharedMesh.vertexCount!=6)errors.Add("Invalid procedural shield mesh");
            if(renderer.sharedMaterial==null||renderer.sharedMaterial.mainTexture!=null)errors.Add("Shield still has a texture dependency");
        }
        results.Add("Shield: "+meshes.Length+" procedural mesh silhouettes, "+sprites.Length+" sprites, texture-independent");
    }

    static void Capture(LumiGame game,string path)
    {
        Camera camera=game.CameraRig.ViewCamera;
        Vector3 center=game.Player.transform.position+Vector3.up*.9f;
        camera.transform.position=center+new Vector3(2.2f,1.2f,3.4f);
        camera.transform.LookAt(center);
        RenderTexture rt=RenderTexture.GetTemporary(1280,720,24);
        RenderTexture active=RenderTexture.active;RenderTexture old=camera.targetTexture;
        Texture2D image=new Texture2D(1280,720,TextureFormat.RGB24,false);
        try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());}
        finally{camera.targetTexture=old;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.Destroy(image);}
    }
}
