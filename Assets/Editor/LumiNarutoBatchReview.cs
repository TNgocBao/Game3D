using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using LumiAdventure;
public static class LumiNarutoBatchReview
{
    public static void Run()
    {
        try
        {
            Directory.CreateDirectory("Logs/NarutoQA");
            LumiChibiSculptBuild.BuildNow(false);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var prefab=Resources.Load<GameObject>("NarutoChibi/Naruto");
            var model=UnityEngine.Object.Instantiate(prefab);var skin=model.GetComponent<SkinnedMeshRenderer>();
            if(skin==null || skin.bones.Length!=11 || skin.sharedMesh.vertexCount<50000)throw new Exception("Missing reference sculpt or rig.");
            if(skin.sharedMesh.bindposes.Length!=skin.bones.Length)throw new Exception("Mesh bindposes and animation rig differ.");
            foreach(var weight in skin.sharedMesh.boneWeights)if(weight.boneIndex0>=skin.bones.Length || weight.boneIndex1>=skin.bones.Length)throw new Exception("Invalid skinning index.");
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.52f,.55f,.6f);RenderSettings.fog=false;
            LightAt("Key",new Vector3(35,155,0),new Color(1,.94f,.86f),1.25f);LightAt("Fill",new Vector3(25,-45,0),new Color(.8f,.9f,1),.8f);
            var ground=GameObject.CreatePrimitive(PrimitiveType.Plane);ground.transform.localScale=Vector3.one*2;var mat=new Material(Shader.Find("Standard"));mat.color=new Color(.32f,.35f,.38f);ground.GetComponent<Renderer>().sharedMaterial=mat;
            var camObj=new GameObject("Preview camera");var cam=camObj.AddComponent<Camera>();cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.27f,.29f,.32f);cam.fieldOfView=35;
            Portrait(cam,0,"Naruto-Front.png");Portrait(cam,28,"Naruto-ThreeQuarter.png");Portrait(cam,90,"Naruto-Side.png");Portrait(cam,180,"Naruto-Back.png");
            var orbitObject=new GameObject("Camera input checks");var orbit=orbitObject.AddComponent<LumiCameraRig>();orbit.Initialize(null,model.transform,Color.gray,new Vector2(100,150));
            if(Mathf.Abs(orbit.FollowDistance-5.8f)>.01f)throw new Exception("Third person camera is too distant.");
            orbit.ApplyLookDelta(new Vector2(36,0));if(Mathf.Abs(orbit.Yaw-90)>.01f || Vector3.Dot(orbit.FlatForward,Vector3.right)<.999f)throw new Exception("Horizontal mouse look and movement basis disagree.");
            orbit.ApplyLookDelta(new Vector2(0,-1000));if(orbit.Pitch!=65)throw new Exception("Upper vertical clamp failed.");
            orbit.ApplyLookDelta(new Vector2(0,1000));if(orbit.Pitch!=-12)throw new Exception("Lower vertical clamp failed.");
            orbit.ApplyLookDelta(new Vector2(1440,0));if(Mathf.Abs(orbit.Yaw-90)>.01f)throw new Exception("Full orbit introduces drift.");
            File.WriteAllText("Logs/NarutoQA/BatchReview.txt","PASS: clean chibi mesh, 11 animation bones, valid skin indices and bindposes.\nPASS: 5.8 m third person follow, mouse yaw, camera relative movement basis, pitch limits, full orbit.\nRendered front, side, back and three quarter views.\n");
            AssetDatabase.SaveAssets();EditorApplication.Exit(0);
        }
        catch(Exception error){File.WriteAllText("Logs/NarutoQA/BatchReview.txt","FAIL: "+error);Debug.LogException(error);EditorApplication.Exit(1);}
    }
    private static void LightAt(string name,Vector3 angle,Color color,float intensity){var obj=new GameObject(name);obj.transform.rotation=Quaternion.Euler(angle);var l=obj.AddComponent<Light>();l.type=LightType.Directional;l.color=color;l.intensity=intensity;l.shadows=LightShadows.Soft;}
    private static void Portrait(Camera camera,float yaw,string file)
    {
        Vector3 center=Vector3.up*1.17f;camera.transform.position=center+Quaternion.Euler(0,yaw,0)*Vector3.forward*4.5f;camera.transform.LookAt(center);
        var rt=new RenderTexture(768,960,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
        var image=new Texture2D(768,960,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,768,960),0,0);image.Apply();File.WriteAllBytes("Logs/NarutoQA/"+file,image.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(image);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
    }
}
