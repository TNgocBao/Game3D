using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LumiAdventure;
using TMPro;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class LumiNpcBuffValidation
{
    private const string Request="Temp/NpcBuffValidation.request";
    private const string Running="Lumi.NpcBuffValidation";
    private const string PreviousMode="Lumi.NpcBuffValidation.PreviousMode";

    static LumiNpcBuffValidation(){EditorApplication.update+=Poll;}

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
            SessionState.SetBool(Running,true);EditorApplication.isPlaying=true;
        }
        if(!SessionState.GetBool(Running,false)||!EditorApplication.isPlaying)return;
        LumiGame game=Object.FindObjectOfType<LumiGame>();if(game==null)return;
        SessionState.SetBool(Running,false);game.StartCoroutine(Check(game));
    }

    private static IEnumerator Check(LumiGame game)
    {
        Directory.CreateDirectory("Logs/NpcBuffQA");var errors=new List<string>();
        game.StartLevelForValidation(1);yield return new WaitForSecondsRealtime(.7f);
        LumiPlayer player=game.Player;
        if(game.NpcBuffs.CurrentBuff==LumiBuffType.Invisibility)errors.Add("NPC selected an unsupported fourth buff");
        ScreenCapture.CaptureScreenshot("Logs/NpcBuffQA/NpcOffer.png");yield return new WaitForSecondsRealtime(.25f);

        int[] distribution=new int[3];Random.State prior=Random.state;Random.InitState(73021);
        for(int i=0;i<330;i++){game.NpcBuffs.BeginLevel();distribution[(int)game.NpcBuffs.CurrentBuff]++;}
        Random.state=prior;
        for(int i=0;i<3;i++)if(distribution[i]<80||distribution[i]>140)errors.Add("Buff distribution outside equal range for "+i+": "+distribution[i]);

        game.NpcBuffs.BeginLevel();LumiBuffType offered=game.NpcBuffs.CurrentBuff;
        string dialogue=game.NpcBuffs.TalkTo(player);
        if(!game.NpcBuffs.Granted||!player.Buffs.IsActive(offered)||!dialogue.Contains("90 giây"))errors.Add("NPC did not grant its selected timed buff");

        player.Buffs.Apply(LumiBuffType.Attack,90,1.5f);
        if(player.ScaleOutgoingDamage(4)!=6)errors.Add("Attack ×1.5 modifier failed");
        player.Buffs.Apply(LumiBuffType.Speed,90,1.5f);
        if(Mathf.Abs(player.MovementSpeed-7.8f)>.01f)errors.Add("Speed ×1.5 modifier failed: "+player.MovementSpeed);
        player.Buffs.Apply(LumiBuffType.Defense,90,1.5f);
        int before=player.Health+player.Armor;player.TakeDamage(4,player.transform.position+Vector3.up);
        if(before-player.Health-player.Armor!=2)errors.Add("50% incoming damage modifier failed");
        player.BecomeInvisible(5);yield return null;

        LumiNpc npc=Object.FindObjectOfType<LumiNpc>();
        game.ShowDialogue("NPC",dialogue);yield return null;
        MethodInfo exit=typeof(LumiNpc).GetMethod("OnTriggerExit",BindingFlags.Instance|BindingFlags.NonPublic);
        exit.Invoke(npc,new object[]{player.Controller});yield return null;
        FieldInfo dialogueField=typeof(LumiGame).GetField("dialoguePanel",BindingFlags.Instance|BindingFlags.NonPublic);
        GameObject dialoguePanel=(GameObject)dialogueField.GetValue(game);
        if(dialoguePanel.activeSelf)errors.Add("Dialogue remained open after leaving NPC");

        LumiPickup[] pickups=Object.FindObjectsOfType<LumiPickup>();int readable=0;
        foreach(LumiPickup pickup in pickups)if(pickup.GetComponentInChildren<SpriteRenderer>()!=null)readable++;
        if(readable!=pickups.Length)errors.Add("Some pickups still use old primitive visuals: "+readable+"/"+pickups.Length);
        Transform shield=player.transform.Find("Naruto Visual/Armor Shield");
        int shieldIcons=shield==null?0:shield.GetComponentsInChildren<SpriteRenderer>(true).Length;
        if(shieldIcons!=4)errors.Add("Armor formation is not four shield icons: "+shieldIcons);

        bool setting=false;foreach(TMP_Text text in game.GetComponentsInChildren<TMP_Text>(true))if(text.text=="ESC")setting=true;
        if(!setting)errors.Add("ESC settings label missing");
        int activeBuffIcons=0;foreach(Transform child in game.GetComponentsInChildren<Transform>(true))if(child.name.StartsWith("Active buff")&&child.gameObject.activeInHierarchy)activeBuffIcons++;
        if(activeBuffIcons<3)errors.Add("Timed buff HUD did not show active icons");

        ScreenCapture.CaptureScreenshot("Logs/NpcBuffQA/NpcBuffAndItems.png");
        yield return new WaitForSecondsRealtime(.35f);
        game.Controls.SetMode(LumiControlMode.PC);game.ApplyControlScheme();yield return null;
        Transform mobile=null;foreach(Transform child in game.GetComponentsInChildren<Transform>(true))if(child.name=="Mobile Controls"){mobile=child;break;}
        bool pcHudWorks=activeBuffIcons>=3&&mobile!=null&&!mobile.gameObject.activeInHierarchy;
        if(!pcHudWorks)errors.Add("Buff HUD did not remain available in PC mode");
        ScreenCapture.CaptureScreenshot("Logs/NpcBuffQA/NpcBuffPC.png");
        File.WriteAllText("Logs/NpcBuffQA/Validation.txt",(errors.Count==0?"PASS":"FAIL")+
            "\nDistribution attack/speed/defense="+distribution[0]+"/"+distribution[1]+"/"+distribution[2]+
            "; outgoing 4→"+player.ScaleOutgoingDamage(4)+"; speed="+player.MovementSpeed.ToString("0.0")+
            "; damage 4→"+(before-player.Health-player.Armor)+"; pickup icons="+readable+"/"+pickups.Length+
            "; shield icons="+shieldIcons+"; active HUD buffs="+activeBuffIcons+"; PC HUD="+pcHudWorks+".\n"+string.Join("\n",errors));
        yield return new WaitForSecondsRealtime(1.5f);
        LumiControlScheme.SaveMode((LumiControlMode)SessionState.GetInt(PreviousMode,(int)LumiControlMode.Auto));
        EditorApplication.isPlaying=false;
    }
}
