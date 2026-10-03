using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using LumiAdventure;

public static class LumiNarutoValidation
{
    [MenuItem("Vanguard/Verify Naruto villages and capture artwork")]
    public static void Run()
    {
        if(!Application.isPlaying){Debug.LogWarning("Enter Play Mode first.");return;}
        LumiGame game=Object.FindObjectOfType<LumiGame>();if(game!=null)game.StartCoroutine(Check(game));
    }
    private static IEnumerator Check(LumiGame game)
    {
        Directory.CreateDirectory("Logs/NarutoQA");Directory.CreateDirectory("Assets/Resources/LumiReference");
        var errors=new List<string>();int unlocked=PlayerPrefs.GetInt("Lumi.Unlocked",1);
        string[] keys={"VillageLeaf","VillageSand","VillageStone","VillageCloud","VillageMist"};
        for(int level=1;level<=5;level++)
        {
            game.StartLevel(level);yield return null;
            LumiVillageBoss[] bosses=Object.FindObjectsOfType<LumiVillageBoss>();
            if(bosses.Length!=1 || bosses[0].BossName!=LumiVillageBoss.Names[level-1])errors.Add("Village "+level+": wrong boss count/name");
            if(game.BossCleared)errors.Add("Village "+level+": boss already cleared");
            game.WinLevel();if(!game.IsPlaying || PlayerPrefs.GetInt("Lumi.Unlocked",1)!=unlocked)errors.Add("Village "+level+": boss gate allowed premature victory");
            LumiGoal goal=Object.FindObjectOfType<LumiGoal>();var path=new NavMeshPath();
            if(goal==null || Vector3.Distance(game.Player.transform.position,goal.transform.position)<120 || !NavMesh.CalculatePath(game.Player.transform.position,goal.transform.position,NavMesh.AllAreas,path) || path.status!=NavMeshPathStatus.PathComplete)errors.Add("Village "+level+": distant gate unreachable");
            if(Object.FindObjectsOfType<LumiPickup>().Length!=9)errors.Add("Village "+level+": missing pickup");
            LumiNarutoSkills skills=game.Player.GetComponent<LumiNarutoSkills>();
            if(skills==null || game.Player.ChakraHand==null || game.Player.GetComponentInChildren<LumiWeaponVisual>()!=null)errors.Add("Village "+level+": Naruto rig/combat incorrect");
            if(LumiEnemy.Active.Count==0 || game.Player.GetComponentInChildren<LumiInfantryMotion>()==null)errors.Add("Village "+level+": no animated enemies/player");
            AudioSource music=game.Audio.GetComponents<AudioSource>()[0];if(music.clip==null || !music.isPlaying || !music.loop)errors.Add("Village "+level+": music missing");
            if(!skills.TryBasicAttack() || skills.TryBasicAttack())errors.Add("Village "+level+": normal attack cooldown bypass");
            skills.Cast(3);yield return null;
            if(LumiShadowClone.Active.Count!=3)errors.Add("Village "+level+": expected three shadow clones, got "+LumiShadowClone.Active.Count);
            foreach(LumiShadowClone clone in LumiShadowClone.Active.ToArray())clone.Disperse();yield return null;
            if(LumiShadowClone.Active.Count!=0)errors.Add("Village "+level+": clone cleanup failed");
            skills.Cast(1);if(skills.Remaining(1)<=0)errors.Add("Village "+level+": Rasengan cooldown did not start");
            yield return new WaitForSeconds(.8f);
            skills.Cast(2);yield return new WaitForSeconds(.4f);
            if(Object.FindObjectsOfType<LumiChakraProjectile>().Length!=1)errors.Add("Village "+level+": Rasenshuriken was not launched");
            game.PauseGame();
            Capture(game.CameraRig.ViewCamera,new Vector3(27,27,-42),new Vector3(0,2,-8),"Logs/NarutoQA/"+keys[level-1]+"-Gameplay.png");
            LumiVillageBoss boss=game.VillageBoss;
            Capture(game.CameraRig.ViewCamera,boss.transform.position+new Vector3(0,2.3f,4.5f),boss.transform.position+Vector3.up*1.35f,"Logs/NarutoQA/Boss"+level+".png");
            game.ResumeGame();
            boss.TakeDamage(1000,boss.AimPoint);if(!game.BossCleared || boss.IsAlive)errors.Add("Village "+level+": defeating boss did not open gate");
            yield return null;
        }
        if(PlayerPrefs.GetInt("Lumi.Unlocked",1)!=unlocked)errors.Add("Validation changed progression");
        game.ShowLevelMenu();AssetDatabase.Refresh();
        string report=errors.Count==0?"PASS: five named villages, exactly one correct Kage per village, locked boss gates, complete navigation to exits >=120m, nine pickups, Naruto rig and independent attack cooldowns, three clones and cleanup, Rasengan cooldown, launched Rasenshuriken, looping music, boss defeat opens gate, progression preserved, real scene artwork captured.":"FAIL:\n"+string.Join("\n",errors);
        File.WriteAllText("Logs/NarutoQA/Validation.txt",report);Debug.Log(report);
    }
    private static void Capture(Camera camera,Vector3 position,Vector3 look,string path)
    {
        Vector3 previous=camera.transform.position;Quaternion rotation=camera.transform.rotation;RenderTexture old=camera.targetTexture;float fov=camera.fieldOfView;
        RenderTexture render=RenderTexture.GetTemporary(1280,720,24);RenderTexture active=RenderTexture.active;Texture2D texture=new Texture2D(1280,720,TextureFormat.RGB24,false);
        try{camera.transform.position=position;camera.transform.LookAt(look);camera.fieldOfView=50;camera.targetTexture=render;camera.Render();RenderTexture.active=render;texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());}
        finally{camera.transform.position=previous;camera.transform.rotation=rotation;camera.fieldOfView=fov;camera.targetTexture=old;RenderTexture.active=active;RenderTexture.ReleaseTemporary(render);Object.Destroy(texture);}
    }
}
