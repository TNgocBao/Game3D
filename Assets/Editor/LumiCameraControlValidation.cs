using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using LumiAdventure;

[InitializeOnLoad]
public static class LumiCameraControlValidation
{
    const string Request="Temp/CameraControl.request";
    static LumiCameraControlValidation(){EditorApplication.update+=Poll;}
    static void Poll()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        if(File.Exists(Request))
        {
            if(EditorApplication.isPlaying){EditorApplication.isPlaying=false;return;}
            File.Delete(Request);UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/3D.unity");
            SessionState.SetBool("Lumi.CameraControl",true);EditorApplication.isPlaying=true;
        }
        if(SessionState.GetBool("Lumi.CameraControl",false)&&EditorApplication.isPlaying)
        {
            LumiGame game=UnityEngine.Object.FindObjectOfType<LumiGame>();
            if(game!=null){SessionState.SetBool("Lumi.CameraControl",false);game.StartCoroutine(Check(game));}
        }
    }

    static IEnumerator Check(LumiGame game)
    {
        Directory.CreateDirectory("Logs/CameraControlQA");var errors=new List<string>();
        LumiControlMode restoreControlMode=game.Controls.Mode;game.Controls.SetMode(LumiControlMode.PC);
        Application.LogCallback log=(m,s,t)=>{if(t==LogType.Error||t==LogType.Exception)errors.Add(m);};
        Application.logMessageReceived+=log;game.StartLevelForValidation(1);yield return null;
        foreach(var enemy in UnityEngine.Object.FindObjectsOfType<LumiEnemy>())enemy.enabled=false;
        if(game.VillageBoss!=null)game.VillageBoss.enabled=false;
        LumiPlayer player=game.Player;LumiCameraRig camera=game.CameraRig;
        MethodInfo resolve=typeof(LumiPlayer).GetMethod("ResolveMovementDirection",BindingFlags.Instance|BindingFlags.NonPublic);
        MethodInfo face=typeof(LumiPlayer).GetMethod("FaceAim",BindingFlags.Instance|BindingFlags.NonPublic);
        MethodInfo faceMovement=typeof(LumiPlayer).GetMethod("FaceMovement",BindingFlags.Instance|BindingFlags.NonPublic);
        camera.SetMode(LumiCameraMode.MovementFollow,false);player.transform.rotation=Quaternion.Euler(0,30,0);
        resolve.Invoke(player,new object[]{Vector2.zero});
        Vector3 forward=(Vector3)resolve.Invoke(player,new object[]{Vector2.up});
        Vector3 right=(Vector3)resolve.Invoke(player,new object[]{Vector2.right});
        if(Vector3.Dot(forward,Quaternion.Euler(0,30,0)*Vector3.forward)<.999f)errors.Add("W direction did not use movement gesture heading");
        if(Vector3.Dot(right,Quaternion.Euler(0,30,0)*Vector3.right)<.999f)errors.Add("D direction changed with camera during same gesture");
        faceMovement.Invoke(player,new object[]{right});
        if(Vector3.Dot(player.transform.forward,right.normalized)<.999f)errors.Add("Whole body did not turn 90 degrees for sideways movement");
        player.transform.rotation=Quaternion.Euler(0,30,0);resolve.Invoke(player,new object[]{Vector2.zero});
        Vector3 backward=(Vector3)resolve.Invoke(player,new object[]{Vector2.down});faceMovement.Invoke(player,new object[]{backward});
        if(Vector3.Dot(player.transform.forward,backward.normalized)<.999f)errors.Add("Whole body did not turn 180 degrees for backward movement");
        player.transform.rotation=Quaternion.Euler(0,30,0);resolve.Invoke(player,new object[]{Vector2.zero});resolve.Invoke(player,new object[]{Vector2.up});
        player.transform.rotation=Quaternion.Euler(0,120,0);
        Vector3 held=(Vector3)resolve.Invoke(player,new object[]{Vector2.up});
        if(Vector3.Dot(held,Quaternion.Euler(0,30,0)*Vector3.forward)<.999f)errors.Add("Held input basis drifted with player/camera");
        resolve.Invoke(player,new object[]{Vector2.zero});
        Vector3 restarted=(Vector3)resolve.Invoke(player,new object[]{Vector2.up});
        if(Vector3.Dot(restarted,Quaternion.Euler(0,120,0)*Vector3.forward)<.999f)errors.Add("New movement gesture did not adopt current heading");
        Quaternion before=player.transform.rotation;face.Invoke(player,new object[]{Vector3.left});
        if(Quaternion.Angle(before,player.transform.rotation)>.01f)errors.Add("Mouse aim rotated character in movement-follow mode");
        camera.SetMode(LumiCameraMode.FreeThirdPerson,false);face.Invoke(player,new object[]{Vector3.left});
        if(Vector3.Dot(player.transform.forward,Vector3.left)<.999f)errors.Add("Legacy free-camera aim facing broke");
        camera.SetMode(LumiCameraMode.MovementFollow,false);player.transform.rotation=Quaternion.Euler(0,65,0);
        yield return new WaitForSecondsRealtime(.5f);
        Vector3 horizontal=camera.transform.position-player.transform.position;horizontal.y=0;
        if(Vector3.Dot(horizontal.normalized,-player.transform.forward)<.96f)errors.Add("Camera did not settle behind movement-facing character");
        int cameraButtons=0;foreach(var button in game.GetComponentsInChildren<LumiMobileActionButton>(true))if(button.Action==LumiMobileAction.Camera)cameraButtons++;
        if(cameraButtons!=1)errors.Add("Expected one round mobile camera button, found "+cameraButtons);
        LumiCameraMode start=camera.Mode;camera.CycleMode();camera.CycleMode();camera.CycleMode();if(camera.Mode!=start)errors.Add("Three-mode camera cycle did not wrap");
        camera.SetMode(LumiCameraMode.MovementFollow,false);Capture(camera.ViewCamera,"Logs/CameraControlQA/MovementFollow.png");
        File.WriteAllText("Logs/CameraControlQA/Validation.txt",(errors.Count==0?"PASS":"FAIL")+"\nThree camera modes; stable keyboard/joystick movement basis; whole-body 90/180 degree turns; mouse aim independence; behind-player camera; round mobile CAM button.\n"+string.Join("\n",errors));
        Application.logMessageReceived-=log;game.Controls.SetMode(restoreControlMode);game.ShowLevelMenu();EditorApplication.isPlaying=false;
    }
    static void Capture(Camera camera,string path)
    {
        var rt=RenderTexture.GetTemporary(1280,720,24);var old=camera.targetTexture;var active=RenderTexture.active;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);
        try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());}
        finally{camera.targetTexture=old;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.Destroy(tex);}
    }
}
