using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using LumiAdventure;

[InitializeOnLoad]
public static class LumiNarutoModelReview
{
    private const string Request="QA/NarutoModelReview.request";
    private static int stage;private static double next,chargeDeadline;private static LumiGame game;private static LumiNarutoSkills skills;
    private static readonly StringBuilder report=new StringBuilder();
    static LumiNarutoModelReview(){EditorApplication.update+=Tick;}
    private static void Tick()
    {
        try
        {
            if(EditorApplication.isCompiling || EditorApplication.timeSinceStartup<next)return;
            if(stage==0)
            {
                if(!File.Exists(Request))return;File.Delete(Request);report.Clear();Directory.CreateDirectory("Logs/NarutoQA");
                SessionState.SetInt("NarutoModelReview.Stage",1);EditorApplication.isPlaying=false;stage=1;next=EditorApplication.timeSinceStartup+1;return;
            }
            if(stage==1)
            {
                if(EditorApplication.isPlayingOrWillChangePlaymode)return;
                // SessionState survives the domain reload caused by entering Play.
                SessionState.SetInt("NarutoModelReview.Stage",2);EditorApplication.isPlaying=true;stage=2;return;
            }
            if(stage==2)
            {
                if(!EditorApplication.isPlaying)return;game=UnityEngine.Object.FindObjectOfType<LumiGame>();if(game==null)return;
                game.StartLevel(1);skills=game.Player.GetComponent<LumiNarutoSkills>();
                var skin=game.Player.GetComponentsInChildren<SkinnedMeshRenderer>();
                if(skin.Length==0)throw new Exception("Naruto is still a primitive fallback: no skinned meshes imported.");
                report.AppendLine("Textured Naruto chibi: "+skin.Length+" skinned mesh parts.");
                int vertices=0;foreach(var s in skin){vertices+=s.sharedMesh.vertexCount;if(s.bones.Length==0)throw new Exception("Mesh has no animation bones.");foreach(var m in s.sharedMaterials)if(m.mainTexture==null)throw new Exception("Untextured Naruto material.");}
                report.AppendLine("Vertices: "+vertices+". Rig and texture checks passed.");
                game.PauseGame();Capture(game.Player.transform,1.16f,4.5f,"Naruto-Front.png");Capture(game.Player.transform,1.16f,-4.5f,"Naruto-Back.png");Capture(game.Player.transform,1.16f,4.5f,"Naruto-Side.png",90);Capture(game.Player.transform,1.16f,4.5f,"Naruto-ThreeQuarter.png",28);
                game.ResumeGame();if(!skills.BeginCharge(1))throw new Exception("Could not begin Rasengan charge.");stage=3;chargeDeadline=EditorApplication.timeSinceStartup+10;next=EditorApplication.timeSinceStartup+.1;return;
            }
            if(stage==3)
            {
                if(skills.ChargeMultiplier<3 && EditorApplication.timeSinceStartup<chargeDeadline)return;
                if(Mathf.Abs(skills.ChargeMultiplier-3)>.01f)throw new Exception("Charge did not reach 3x: "+skills.ChargeMultiplier);
                report.AppendLine("Rasengan hold: maximum 3x scale and damage multiplier reached.");game.PauseGame();Capture(game.Player.transform,1.05f,4.4f,"Naruto-Rasengan-Charge.png");game.ResumeGame();skills.ReleaseCharge(1);
                stage=4;next=EditorApplication.timeSinceStartup+1;return;
            }
            if(stage==4)
            {
                if(!skills.BeginCharge(2))throw new Exception("Could not begin Rasenshuriken charge.");stage=5;chargeDeadline=EditorApplication.timeSinceStartup+10;next=EditorApplication.timeSinceStartup+.1;return;
            }
            if(stage==5)
            {
                if(skills.ChargeMultiplier<3 && EditorApplication.timeSinceStartup<chargeDeadline)return;
                if(Mathf.Abs(skills.ChargeMultiplier-3)>.01f)throw new Exception("Rasenshuriken charge did not reach 3x.");
                report.AppendLine("Rasenshuriken hold: maximum 3x reached.");game.PauseGame();Capture(game.Player.transform,1.05f,4.4f,"Naruto-Rasenshuriken-Charge.png");game.ResumeGame();skills.ReleaseCharge(2);
                stage=6;next=EditorApplication.timeSinceStartup+1;return;
            }
            if(stage==6)
            {
                if(!skills.TryBasicAttack())throw new Exception("Basic throwing attack refused.");stage=7;chargeDeadline=EditorApplication.timeSinceStartup+3;next=EditorApplication.timeSinceStartup+.12;return;
            }
            if(stage==7)
            {
                bool star=false;foreach(var projectile in UnityEngine.Object.FindObjectsOfType<LumiProjectile>())if(projectile.name.Contains("shuriken"))star=true;
                if(!star && EditorApplication.timeSinceStartup<chargeDeadline)return;
                if(!star)throw new Exception("Basic attack did not spawn a steel shuriken.");report.AppendLine("Basic attack: steel shuriken projectile spawned.");
                game.PauseGame();report.AppendLine("PASS — live Play model / charge / throwing checks.");File.WriteAllText("Logs/NarutoQA/ModelReview.txt",report.ToString());Debug.Log(report.ToString());stage=0;SessionState.SetInt("NarutoModelReview.Stage",0);
            }
        }
        catch(Exception error){File.WriteAllText("Logs/NarutoQA/ModelReview.txt",report+"FAIL: "+error);Debug.LogException(error);stage=0;SessionState.SetInt("NarutoModelReview.Stage",0);}
    }
    [InitializeOnLoadMethod] private static void Restore(){stage=SessionState.GetInt("NarutoModelReview.Stage",0);}
    private static void Capture(Transform target,float height,float forward,string file,float yaw=0)
    {
        Camera camera=game.CameraRig.ViewCamera;Vector3 position=camera.transform.position;Quaternion rotation=camera.transform.rotation;float fov=camera.fieldOfView;RenderTexture old=camera.targetTexture;
        RenderTexture texture=new RenderTexture(768,960,24);camera.targetTexture=texture;camera.fieldOfView=35;
        camera.transform.position=target.position+(Quaternion.AngleAxis(yaw,Vector3.up)*target.forward)*forward+Vector3.up*height;camera.transform.LookAt(target.position+Vector3.up*height);
        camera.Render();RenderTexture active=RenderTexture.active;RenderTexture.active=texture;Texture2D image=new Texture2D(768,960,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,768,960),0,0);image.Apply();File.WriteAllBytes("Logs/NarutoQA/"+file,image.EncodeToPNG());
        camera.targetTexture=old;camera.fieldOfView=fov;camera.transform.SetPositionAndRotation(position,rotation);RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(image);texture.Release();UnityEngine.Object.DestroyImmediate(texture);
    }
}
