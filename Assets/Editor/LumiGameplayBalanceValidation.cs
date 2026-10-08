using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LumiAdventure;
using UnityEditor;
using UnityEngine;
using TMPro;

[InitializeOnLoad]
public static class LumiGameplayBalanceValidation
{
    private const string Request="Temp/GameplayBalance.request";
    private const string Running="Lumi.GameplayBalanceValidation";
    static LumiGameplayBalanceValidation(){EditorApplication.update+=Poll;}

    private static void Poll()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        if(File.Exists(Request))
        {
            if(EditorApplication.isPlaying){EditorApplication.isPlaying=false;return;}
            File.Delete(Request);UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/3D.unity");
            SessionState.SetBool(Running,true);EditorApplication.isPlaying=true;
        }
        if(!SessionState.GetBool(Running,false)||!EditorApplication.isPlaying)return;
        LumiGame game=Object.FindObjectOfType<LumiGame>();if(game==null)return;
        SessionState.SetBool(Running,false);game.StartCoroutine(Check(game));
    }

    private static IEnumerator Check(LumiGame game)
    {
        Directory.CreateDirectory("Logs/GameplayBalanceQA");var errors=new List<string>();
        yield return new WaitForSecondsRealtime(.5f);
        bool hokageTitle=false,oldTitle=false;
        foreach(TMP_Text text in game.GetComponentsInChildren<TMP_Text>(true))
        {
            if(!text.gameObject.activeInHierarchy)continue;
            if(text.text.Contains("HÀNH TRÌNH HOKAGE"))hokageTitle=true;
            if(text.text.Contains("NGŨ ĐẠI NHẪN THÔN"))oldTitle=true;
        }
        if(!hokageTitle||oldTitle)errors.Add("Menu title was not fully changed to HÀNH TRÌNH HOKAGE");
        ScreenCapture.CaptureScreenshot("Logs/GameplayBalanceQA/MenuTitle.png");
        yield return new WaitForSecondsRealtime(.75f);
        game.StartLevelForValidation(1);yield return null;
        if(game.LevelTimer==null||Mathf.Abs(game.LevelTimer.RemainingSeconds-LumiLevelTimer.DefaultDurationSeconds)>1f)
            errors.Add("Level timer did not start at 15 minutes");
        if(LumiPlayer.MaxHealth!=20||game.Player.Health!=20)errors.Add("Player health is not 20/20");
        if(LumiPlayer.MaxArmor!=20||game.Player.Armor!=20)errors.Add("Player armor is not 20/20");

        GameObject cloneObject=new GameObject("Clone speed QA",typeof(CharacterController));
        LumiShadowClone clone=cloneObject.AddComponent<LumiShadowClone>();clone.Initialize(game,game.Player,10);
        float expectedSpeed=game.Player.MovementSpeed*.6f;
        if(Mathf.Abs(clone.MovementSpeed-expectedSpeed)>.001f)errors.Add("Clone movement speed is not double its former 30% value");
        if(Mathf.Abs(clone.MaxHealth-LumiPlayer.MaxHealth*.3f)>.001f||Mathf.Abs(clone.Armor-game.Player.Armor*.3f)>.001f)errors.Add("Clone combat attributes are no longer 30% of player");
        clone.Disperse();

        Random.State randomState=Random.state;Random.InitState(81625);int drops=0,health=0,armor=0;
        FieldInfo typeField=typeof(LumiPickup).GetField("type",BindingFlags.Instance|BindingFlags.NonPublic);
        for(int i=0;i<100;i++)
        {
            if(!game.TryDropEnemySupportItem(new Vector3(500+i*2,1,500)))continue;
            drops++;
        }
        foreach(LumiPickup pickup in Object.FindObjectsOfType<LumiPickup>())
        {
            if(!pickup.gameObject.name.StartsWith("Quái rơi"))continue;
            LumiPickupType type=(LumiPickupType)typeField.GetValue(pickup);
            if(type==LumiPickupType.Health)health++;else if(type==LumiPickupType.Armor)armor++;
            Object.Destroy(pickup.gameObject);
        }
        Random.state=randomState;
        if(drops<35||drops>65)errors.Add("Drop rate sample is outside a reasonable 50% range: "+drops+"/100");
        if(health==0||armor==0||health+armor!=drops)errors.Add("Drops are not limited to both health and armor items");

        AudioClip music=Resources.Load<AudioClip>("LumiMusic/NinjaVillage");
        if(music==null||music.length<10)errors.Add("Ninja Village background music was not imported correctly");
        File.WriteAllText("Logs/GameplayBalanceQA/Validation.txt",(errors.Count==0?"PASS":"FAIL")+
            "\nMenu title HÀNH TRÌNH HOKAGE="+hokageTitle+"; timer="+game.LevelTimer.RemainingSeconds.ToString("0.0")+"s; player 20 health / 20 armor; clone speed 60% of player; enemy support drop sample "+drops+"/100 (health "+health+", armor "+armor+"); music "+(music==null?"missing":music.length.ToString("0.0")+"s")+".\n"+string.Join("\n",errors));
        game.ShowLevelMenu();EditorApplication.isPlaying=false;
    }
}
