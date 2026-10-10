using System.Collections;
using System.IO;
using System.Reflection;
using LumiAdventure;
using TMPro;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class LumiTargetingPreview
{
    private const string Request="Temp/TargetingPreview.request";
    private const string Running="Lumi.TargetingPreview.Running";
    private const string PreviousMode="Lumi.TargetingPreview.PreviousMode";

    static LumiTargetingPreview(){EditorApplication.update+=Poll;}

    private static void Poll()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        if(File.Exists(Request))
        {
            if(EditorApplication.isPlaying){EditorApplication.isPlaying=false;return;}
            File.Delete(Request);
            SessionState.SetInt(PreviousMode,(int)LumiControlScheme.LoadSavedMode());
            LumiControlScheme.SaveMode(LumiControlMode.Mobile);
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/3D.unity");
            SessionState.SetBool(Running,true);
            EditorApplication.isPlaying=true;
        }
        if(!SessionState.GetBool(Running,false)||!EditorApplication.isPlaying)return;
        LumiGame game=Object.FindObjectOfType<LumiGame>();if(game==null)return;
        SessionState.SetBool(Running,false);game.StartCoroutine(Capture(game));
    }

    private static IEnumerator Capture(LumiGame game)
    {
        Directory.CreateDirectory("Logs/TargetingQA");
        game.StartLevelForValidation(1);yield return null;yield return null;
        game.CameraRig.SetMode(LumiCameraMode.FreeThirdPerson,false);
        LumiEnemy enemy=LumiEnemy.Active.Count>0?LumiEnemy.Active[0]:null;
        if(enemy!=null)enemy.transform.position=game.Player.transform.position+game.Player.transform.forward*9f;
        bool enemyAwareness=true;
        FieldInfo detection=typeof(LumiEnemy).GetField("detectRange",BindingFlags.Instance|BindingFlags.NonPublic);
        foreach(LumiEnemy candidate in LumiEnemy.Active)enemyAwareness&=detection!=null&&Mathf.Approximately((float)detection.GetValue(candidate),LumiAimTargetController.LockRange);
        bool bossAwareness=false;
        if(game.VillageBoss!=null)
        {
            game.VillageBoss.transform.position=game.Player.transform.position+game.Player.transform.forward*(LumiAimTargetController.LockRange-1f);
            yield return new WaitForSecondsRealtime(.12f);bossAwareness=game.VillageBoss.Engaged;game.VillageBoss.enabled=false;
        }
        FieldInfo facingUntil=typeof(LumiPlayer).GetField("attackFacingUntil",BindingFlags.Instance|BindingFlags.NonPublic);
        FieldInfo facingDirection=typeof(LumiPlayer).GetField("attackFacingDirection",BindingFlags.Instance|BindingFlags.NonPublic);
        MethodInfo applyFacing=typeof(LumiPlayer).GetMethod("ApplyFacing",BindingFlags.Instance|BindingFlags.NonPublic);
        game.Player.transform.rotation=Quaternion.LookRotation(Vector3.back);
        facingUntil.SetValue(game.Player,Time.time+1f);facingDirection.SetValue(game.Player,Vector3.back);applyFacing.Invoke(game.Player,new object[]{Vector3.forward});
        bool firingFacingStable=Vector3.Dot(game.Player.transform.forward,Vector3.back)>.999f;
        facingUntil.SetValue(game.Player,0f);applyFacing.Invoke(game.Player,new object[]{Vector3.forward});
        bool movementFacingRestored=Vector3.Dot(game.Player.transform.forward,Vector3.forward)>.999f;
        game.Player.Targeting.SetAutoEnabled(true,null);
        yield return new WaitForSecondsRealtime(.75f);
        if(game.VillageBoss!=null)game.VillageBoss.TakeDamage(1,game.VillageBoss.AimPoint);
        yield return null;

        TMP_Text đổi=null,toggle=null,rasenganName=null,rasenshurikenName=null;
        foreach(TMP_Text label in game.GetComponentsInChildren<TMP_Text>(true))
        {
            if(label.text=="ĐỔI")đổi=label;
            if(label.text=="TẮT"||label.text=="BẬT")toggle=label;
            if(label.text=="Rasengan")rasenganName=label;
            if(label.text=="Rasenshuriken")rasenshurikenName=label;
        }
        TMP_Text rasenganHint=null;
        if(rasenganName!=null)foreach(TMP_Text label in rasenganName.transform.parent.GetComponentsInChildren<TMP_Text>(true))if(label.text=="GIỮ")rasenganHint=label;
        bool skillLabelsSeparated=rasenshurikenName!=null&&rasenganHint!=null&&!ScreenRect(rasenshurikenName.rectTransform).Overlaps(ScreenRect(rasenganHint.rectTransform));
        LumiCursorReticle reticle=Object.FindObjectOfType<LumiCursorReticle>();
        CanvasGroup reticleGroup=reticle==null?null:reticle.GetComponent<CanvasGroup>();
        bool locked=game.Player.Targeting.HasTarget;
        bool marker=Object.FindObjectOfType<LumiTargetMarker>()!=null;
        bool targetHealth=GameObject.Find("Selected target health fill")!=null;
        bool yellowPlayerArrow=GameObject.Find("Mũi tên chỉ đường")!=null;
        bool crosshairHidden=reticleGroup!=null&&reticleGroup.alpha<.1f;
        ScreenCapture.CaptureScreenshot("Logs/TargetingQA/AutoTargetMobile.png");
        yield return new WaitForSecondsRealtime(1f);

        game.Controls.SetMode(LumiControlMode.PC);game.ApplyControlScheme();
        game.Player.Targeting.SetAutoEnabled(true,null);
        yield return new WaitForSecondsRealtime(.25f);
        TMP_Text pcCycle=null;
        foreach(TMP_Text label in game.GetComponentsInChildren<TMP_Text>(true))if(label.text=="ĐỔI\nQ")pcCycle=label;
        ScreenCapture.CaptureScreenshot("Logs/TargetingQA/AutoTargetPc.png");
        yield return new WaitForSecondsRealtime(1f);

        game.Controls.SetMode(LumiControlMode.Mobile);game.ApplyControlScheme();
        yield return null;

        game.Player.Targeting.Toggle();yield return null;
        bool manual=!game.Player.Targeting.AutoEnabled&&reticleGroup!=null&&reticleGroup.alpha>.9f;
        ScreenCapture.CaptureScreenshot("Logs/TargetingQA/ManualAimMobile.png");
        yield return new WaitForSecondsRealtime(1f);

        bool pass=đổi!=null&&toggle!=null&&pcCycle!=null&&locked&&marker&&targetHealth&&!yellowPlayerArrow&&crosshairHidden&&manual&&skillLabelsSeparated&&enemyAwareness&&bossAwareness&&firingFacingStable&&movementFacingRestored;
        File.WriteAllText("Logs/TargetingQA/Validation.txt",(pass?"PASS":"FAIL")+
            "\nAuto target within 30m="+locked+"; red marker="+marker+"; target health bar="+targetHealth+"; crosshair hidden="+crosshairHidden+
            "; manual mode restores crosshair="+manual+"; yellow player arrow removed="+(!yellowPlayerArrow)+
            "; PC cycle key="+(pcCycle!=null?"Q":"missing")+"; skill labels separated="+skillLabelsSeparated+
            "; enemy/boss awareness matches 30m="+(enemyAwareness&&bossAwareness)+"; firing facing stable="+firingFacingStable+"; movement facing restored="+movementFacingRestored+
            "; mobile controls="+(đổi!=null?"ĐỔI":"missing")+"/"+(toggle!=null?toggle.text:"missing")+".\n");
        LumiControlScheme.SaveMode((LumiControlMode)SessionState.GetInt(PreviousMode,(int)LumiControlMode.Auto));
        EditorApplication.isPlaying=false;
    }

    private static Rect ScreenRect(RectTransform rect)
    {
        Vector3[] corners=new Vector3[4];rect.GetWorldCorners(corners);
        return Rect.MinMaxRect(corners[0].x,corners[0].y,corners[2].x,corners[2].y);
    }
}
