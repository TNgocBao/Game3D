using System.Collections;
using System.Collections.Generic;
using System.IO;
using LumiAdventure;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[InitializeOnLoad]
public static class LumiEnvironmentValidation
{
    private const string Request="Temp/EnvironmentValidation.request";
    static LumiEnvironmentValidation(){EditorApplication.update+=Poll;EditorApplication.playModeStateChanged+=OnPlay;}
    private static void Poll()
    {
        if(!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)return;
        File.Delete(Request);SessionState.SetBool("Lumi.EnvironmentValidation",true);LumiEnvironmentBuild.Prepare();EditorApplication.isPlaying=true;
    }
    private static void OnPlay(PlayModeStateChange state)
    {
        if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("Lumi.EnvironmentValidation",false))
        {SessionState.SetBool("Lumi.EnvironmentValidation",false);Run();}
    }
    [MenuItem("Naruto/Validate environment in Play Mode")]
    public static void Run()
    {
        LumiGame game=Object.FindObjectOfType<LumiGame>();
        if(!Application.isPlaying || game==null){Debug.LogWarning("Enter Play Mode to validate environments.");return;}
        game.StartCoroutine(Check(game));
    }
    public static void RunBatch()
    {
        LumiEnvironmentBuild.Prepare();SessionState.SetBool("Lumi.EnvironmentValidation",true);EditorApplication.isPlaying=true;
    }
    private static IEnumerator Check(LumiGame game)
    {
        Directory.CreateDirectory("Logs/EnvironmentQA");var errors=new List<string>();var results=new List<string>();
        int unlocked=PlayerPrefs.GetInt("Lumi.Unlocked",1);int windChanges=0;
        for(int level=1;level<=5;level++)
        {
            game.StartLevelForValidation(level);game.PauseGame();yield return null;
            var houses=new List<GameObject>();var trees=new List<GameObject>();
            foreach(Transform t in Object.FindObjectsOfType<Transform>())
            {
                if(t.name.StartsWith("Village house "))houses.Add(t.gameObject);
                if(t.name=="Styloo wind tree")trees.Add(t.gameObject);
                if(t.name=="House body" || t.name=="Pine crown" || t.name=="Pine trunk" || t.name=="Thân cây" || t.name=="Tán cây")errors.Add("Level "+level+": old procedural scenery remains.");
            }
            if(houses.Count<10)errors.Add("Level "+level+": imported houses missing.");
            if(level==1 && trees.Count<10)errors.Add("Level 1: imported trees missing.");
            foreach(GameObject house in houses)if(house.GetComponent<BoxCollider>()==null)errors.Add("House collider missing.");
            foreach(GameObject obj in houses)
                foreach(Renderer r in obj.GetComponentsInChildren<Renderer>())
                    foreach(Material m in r.sharedMaterials)if(m==null || m.shader==null || !m.shader.isSupported)errors.Add("Broken house material: "+obj.name);
            foreach(GameObject tree in trees)
                foreach(Renderer r in tree.GetComponentsInChildren<Renderer>())
                    foreach(Material m in r.sharedMaterials)if(m==null || m.shader.name!="Lumi/Environment Tree Wind" || !m.shader.isSupported)errors.Add("Tree wind material unsupported.");
            LumiGoal goal=Object.FindObjectOfType<LumiGoal>();var path=new NavMeshPath();
            if(goal==null || !NavMesh.CalculatePath(game.Player.transform.position,goal.transform.position,NavMesh.AllAreas,path) || path.status!=NavMeshPathStatus.PathComplete)errors.Add("Level "+level+": route to goal blocked.");
            Capture(game.CameraRig.ViewCamera,new Vector3(24,19,-48),new Vector3(0,2,-23),"Logs/EnvironmentQA/Village"+level+".png");
            if(level==1 && trees.Count>0)
            {
                GameObject tree=trees[trees.Count/2];Vector3 p=tree.transform.position;float h=tree.transform.localScale.y;
                Renderer[] renderers=tree.GetComponentsInChildren<Renderer>();var block=new MaterialPropertyBlock();
                block.SetFloat("_WindTime",2);foreach(Renderer r in renderers)r.SetPropertyBlock(block);
                Texture2D first=Capture(game.CameraRig.ViewCamera,p+new Vector3(h*.85f,h*.65f,h*1.2f),p+Vector3.up*h*.5f,"Logs/EnvironmentQA/WindA.png",true);
                block.SetFloat("_WindTime",6);foreach(Renderer r in renderers)r.SetPropertyBlock(block);
                Texture2D second=Capture(game.CameraRig.ViewCamera,p+new Vector3(h*.85f,h*.65f,h*1.2f),p+Vector3.up*h*.5f,"Logs/EnvironmentQA/WindB.png",true);
                Color32[] a=first.GetPixels32(),b=second.GetPixels32();for(int i=0;i<a.Length;i++)if(a[i].r!=b[i].r || a[i].g!=b[i].g || a[i].b!=b[i].b)windChanges++;
                if(windChanges<30)errors.Add("Wind shader did not change rendered pixels.");
                foreach(Renderer r in renderers)r.SetPropertyBlock(null);Object.Destroy(first);Object.Destroy(second);
            }
            results.Add("Village "+level+": "+houses.Count+" imported houses, "+trees.Count+" imported trees; route checked.");
            game.ResumeGame();yield return null;
        }
        if(PlayerPrefs.GetInt("Lumi.Unlocked",1)!=unlocked)errors.Add("Progression changed during validation.");
        game.ShowLevelMenu();results.Add("Wind rendered pixel changes: "+windChanges);
        string report=(errors.Count==0?"PASS":"FAIL")+"\n"+string.Join("\n",results)+"\n"+string.Join("\n",errors);
        File.WriteAllText("Logs/EnvironmentQA/Validation.txt",report);Debug.Log(report);EditorApplication.isPlaying=false;
        if(Application.isBatchMode)EditorApplication.delayCall+=()=>EditorApplication.Exit(errors.Count==0?0:1);
    }
    private static Texture2D Capture(Camera camera,Vector3 position,Vector3 look,string file,bool keep=false)
    {
        Vector3 p=camera.transform.position;Quaternion rotation=camera.transform.rotation;float fov=camera.fieldOfView;RenderTexture old=camera.targetTexture,active=RenderTexture.active;
        RenderTexture rt=RenderTexture.GetTemporary(1280,720,24);var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
        try{camera.transform.position=position;camera.transform.LookAt(look);camera.fieldOfView=50;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(file,image.EncodeToPNG());}
        finally{camera.transform.position=p;camera.transform.rotation=rotation;camera.fieldOfView=fov;camera.targetTexture=old;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);}
        if(keep)return image;Object.Destroy(image);return null;
    }
}
