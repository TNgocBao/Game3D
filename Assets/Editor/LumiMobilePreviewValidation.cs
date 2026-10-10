using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LumiAdventure;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[InitializeOnLoad]
public static class LumiMobilePreviewValidation
{
    private const string Request="Temp/MobilePreview.request";
    private const string Running="Lumi.MobilePreviewValidation";
    private const string PreviousMode="Lumi.MobilePreviewPreviousMode";

    static LumiMobilePreviewValidation(){EditorApplication.update+=Poll;}

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
        LumiGame game=Object.FindObjectOfType<LumiGame>();
        if(game==null)return;
        SessionState.SetBool(Running,false);
        game.StartCoroutine(Capture(game));
    }

    private static IEnumerator Capture(LumiGame game)
    {
        Directory.CreateDirectory("Logs/MobilePreview");
        game.StartLevelForValidation(1);
        yield return null;
        game.CameraRig.SetMode(LumiCameraMode.MovementFollow,false);
        yield return new WaitForSecondsRealtime(1f);

        Transform panel=game.transform.Find("Mobile Controls");
        if(panel==null)
        {
            foreach(Transform child in game.GetComponentsInChildren<Transform>(true))
                if(child.name=="Mobile Controls"){panel=child;break;}
        }
        LumiMobileActionButton[] actionButtons=panel==null?new LumiMobileActionButton[0]:panel.GetComponentsInChildren<LumiMobileActionButton>(true);
        int controls=actionButtons.Length;
        bool separatedComponents=game.GetComponent<LumiControlScheme>()!=null&&
            game.GetComponent<LumiLevelTimer>()!=null&&
            game.GetComponent<LumiPauseInputRouter>()!=null&&
            game.GetComponent<LumiMobileControlsView>()!=null&&
            game.Player.GetComponent<LumiPlayerMovementInput>()!=null&&
            game.Player.GetComponent<LumiPlayerCombatInput>()!=null;
        string status=panel!=null&&panel.gameObject.activeInHierarchy&&controls==4&&separatedComponents?"PASS":"FAIL";
        string details="";
        foreach(LumiMobileActionButton button in actionButtons)
        {
            RectTransform rect=button.transform as RectTransform;
            Image image=button.GetComponent<Image>();
            details+="\n"+button.Action+" @ "+rect.anchoredPosition+" sprite="+(image!=null&&image.sprite!=null);
        }
        foreach(TMP_Text label in game.GetComponentsInChildren<TMP_Text>(true))
            if(label.text=="CHẠM"||label.text=="CHUỘT"||label.text=="GIỮ C"||label.text=="GIỮ R")details+="\nSkill label: "+label.text;
        LumiMobileActionButton interact=null;
        foreach(LumiMobileActionButton button in actionButtons)if(button.Action==LumiMobileAction.Interact)interact=button;
        bool talkHiddenAway=interact!=null&&!interact.gameObject.activeSelf;
        game.ShowInteractionPrompt("Chạm NÓI để nói chuyện với NPC");
        yield return null;
        bool talkShownNear=interact!=null&&interact.gameObject.activeInHierarchy;
        game.HideInteractionPrompt();
        if(!talkHiddenAway||!talkShownNear)status="FAIL";
        details+="\nTalk hidden away="+talkHiddenAway+"; shown near NPC="+talkShownNear;
        details+="\nFeature components separated="+separatedComponents;
        Button settings=null;
        foreach(Button button in game.GetComponentsInChildren<Button>(true))
        {
            TMP_Text label=button.GetComponentInChildren<TMP_Text>(true);
            if(button.gameObject.activeInHierarchy&&button.gameObject.name=="Setting"){settings=button;break;}
        }
        bool settingsOnTop=false,settingsOpened=false,controlSettingSwitches=false;
        if(settings!=null&&EventSystem.current!=null)
        {
            var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,settings.transform.position)};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
            foreach(RaycastResult hit in hits)
            {
                Button hitButton=hit.gameObject.GetComponentInParent<Button>();
                if(hitButton!=null){settingsOnTop=hitButton==settings;break;}
            }
            settings.onClick.Invoke();yield return null;
            FieldInfo settingsField=typeof(LumiGame).GetField("settingsPanel",BindingFlags.Instance|BindingFlags.NonPublic);
            GameObject settingsPanel=settingsField==null?null:settingsField.GetValue(game) as GameObject;
            settingsOpened=settingsPanel!=null&&settingsPanel.activeInHierarchy;
            if(settingsOpened)
            {
                Button controlModeButton=null;
                foreach(Button candidate in settingsPanel.GetComponentsInChildren<Button>(true))
                {
                    TMP_Text candidateLabel=candidate.GetComponentInChildren<TMP_Text>(true);
                    if(candidateLabel!=null&&candidateLabel.text.StartsWith("KIỂU ĐIỀU KHIỂN")){controlModeButton=candidate;break;}
                }
                if(controlModeButton!=null)
                {
                    controlModeButton.onClick.Invoke();yield return null;
                    controlSettingSwitches=!panel.gameObject.activeInHierarchy&&game.Controls.UsesPcControls;
                    game.Controls.SetMode(LumiControlMode.Mobile);game.ApplyControlScheme();yield return null;
                }
            }
            MethodInfo close=typeof(LumiGame).GetMethod("CloseGameSettings",BindingFlags.Instance|BindingFlags.NonPublic);
            if(close!=null)close.Invoke(game,null);
            yield return null;
        }
        if(!settingsOnTop||!settingsOpened||!controlSettingSwitches)status="FAIL";
        details+="\nSettings receives touch="+settingsOnTop+"; opens panel="+settingsOpened+"; switches Mobile/PC="+controlSettingSwitches;
        LumiMobileActionButton menuButton=null;
        foreach(LumiMobileActionButton button in actionButtons)if(button.Action==LumiMobileAction.Menu)menuButton=button;
        bool menuPauses=false,menuResumes=false;
        if(menuButton!=null)
        {
            menuButton.OnPointerDown(null);yield return null;
            menuPauses=!game.IsPlaying&&Mathf.Approximately(Time.timeScale,0f)&&!panel.gameObject.activeInHierarchy;
            game.ResumeGame();yield return null;
            menuResumes=game.IsPlaying&&Mathf.Approximately(Time.timeScale,1f)&&panel.gameObject.activeInHierarchy;
        }
        if(!menuPauses||!menuResumes)status="FAIL";
        details+="\nMobile MENU pauses="+menuPauses+"; resume restores controls="+menuResumes;
        File.WriteAllText("Logs/MobilePreview/Validation.txt",status+"\nMobile preview enabled in Unity Editor. Active="+(panel!=null&&panel.gameObject.activeInHierarchy)+"; action buttons="+controls+"; screen="+Screen.width+"x"+Screen.height+"."+details+"\n");
        ScreenCapture.CaptureScreenshot("Logs/MobilePreview/MobileControls.png");
        yield return new WaitForSecondsRealtime(1.5f);
        LumiControlScheme.SaveMode((LumiControlMode)SessionState.GetInt(PreviousMode,(int)LumiControlMode.Auto));
        EditorApplication.isPlaying=false;
    }
}
