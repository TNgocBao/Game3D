using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using LumiAdventure;
using TMPro;

public static class LumiPlayModeValidation
{
    [MenuItem("Vanguard/Validate five levels in Play Mode")]
    public static void Run()
    {
        if(!Application.isPlaying){Debug.LogWarning("Enter Play Mode before running Vanguard validation.");return;}
        LumiGame game=Object.FindObjectOfType<LumiGame>();
        if(game==null){Debug.LogError("Vanguard game was not initialized.");return;}
        game.StartCoroutine(Validate(game));
    }

    private static IEnumerator Validate(LumiGame game)
    {
        var failures=new List<string>();
        int unlocked=PlayerPrefs.GetInt("Lumi.Unlocked",1);
        for(int level=1;level<=5;level++)
        {
            game.StartLevel(level);
            yield return null;
            if(game.Player.Health!=15)failures.Add("Level "+level+": player HP");
            if(game.CameraRig.ViewCamera.orthographic)failures.Add("Level "+level+": expected perspective camera");
            var path=new NavMeshPath();
            LumiGoal goal=Object.FindObjectOfType<LumiGoal>();
            if(goal!=null && Vector3.Distance(game.Player.transform.position,goal.transform.position)<90)failures.Add("Level "+level+": exit should be at least 90m away");
            if(goal==null || !NavMesh.CalculatePath(game.Player.transform.position,goal.transform.position,NavMesh.AllAreas,path) || path.status!=NavMeshPathStatus.PathComplete)
                failures.Add("Level "+level+": no complete route to exit");
            LumiPickup[] items=Object.FindObjectsOfType<LumiPickup>();
            if(items.Length!=9)failures.Add("Level "+level+": expected 5 stars + 4 pickups, got "+items.Length);
            LumiEnemy[] enemies=Object.FindObjectsOfType<LumiEnemy>();
            int[] tiers=new int[3];
            foreach(LumiEnemy enemy in enemies)
            {
                tiers[(int)enemy.EnemyType-1]++;
                int expected=enemy.EnemyType==LumiEnemyType.Sprout?10:enemy.EnemyType==LumiEnemyType.Ranger?20:40;
                int hp=(int)typeof(LumiEnemy).GetField("health",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(enemy);
                if(hp!=expected)failures.Add("Level "+level+": enemy HP");
            }
            if(tiers[0]==0 || (level>=2 && tiers[1]==0) || (level>=4 && tiers[2]==0))failures.Add("Level "+level+": missing enemy tier");
            string[] boundaryNames={"Tường Bắc","Tường Nam","Tường Đông","Tường Tây"};
            foreach(string name in boundaryNames)
            {
                GameObject wall=GameObject.Find(name);
                if(wall==null || wall.GetComponent<Collider>()==null || !wall.GetComponent<Collider>().enabled)failures.Add("Level "+level+": missing boundary "+name);
            }
            game.Player.AddArmor(5);game.Player.TakeDamage(3,game.Player.transform.position);
            if(game.Player.Health!=15 || game.Player.Armor!=2)failures.Add("Level "+level+": armor absorption");
            game.Player.TakeDamage(4,game.Player.transform.position);game.Player.Heal(5);
            if(game.Player.Health!=15)failures.Add("Level "+level+": heal cap");
            game.Player.BecomeInvisible(5);
            if(!game.Player.IsInvisible)failures.Add("Level "+level+": invisibility");
            AudioSource[] audio=game.Audio.GetComponents<AudioSource>();
            if(audio.Length<2 || audio[0].clip==null || !audio[0].isPlaying || !audio[0].loop)failures.Add("Level "+level+": music loop missing");
            if(game.Player.GetComponentInChildren<LumiWeaponVisual>()==null)failures.Add("Level "+level+": chibi pistol missing");
            if(level==1)
            {
                GameObject probe=new GameObject("Projectile regression probe");
                LumiProjectile projectile=probe.AddComponent<LumiProjectile>();
                projectile.Initialize(game,Vector3.back,24,4,true,game.Player.transform);
                Vector3 center=game.Player.transform.position+Vector3.up;
                GameObject obstacle=GameObject.CreatePrimitive(PrimitiveType.Cube);
                obstacle.name="Regression target behind shooter";obstacle.transform.position=center+Vector3.back*2.5f;obstacle.transform.localScale=Vector3.one*.5f;
                Physics.SyncTransforms();
                if(!projectile.Trace(center+Vector3.forward*.8f,Vector3.back,4,out RaycastHit hit) || hit.collider!=obstacle.GetComponent<Collider>())
                    failures.Add("Backward projectile was blocked by its shooter or failed to hit the real obstacle");
                Object.Destroy(probe);Object.Destroy(obstacle);
                LumiInfantryMotion motion=game.Player.GetComponentInChildren<LumiInfantryMotion>();
                float before=motion.StridePhase;
                game.Player.GetComponent<CharacterController>().Move(Vector3.forward*.5f);
                game.Player.BoostSpeed(.4f);
                yield return new WaitForEndOfFrame();
                if(motion.StridePhase<=before)failures.Add("Stride animation did not advance with actual displacement");
                if(!game.Player.GetComponent<LumiSpeedWind>().IsEmitting)failures.Add("Speed wind did not emit while moving with buff");
                game.PauseGame();yield return new WaitForEndOfFrame();
                if(game.Player.GetComponent<LumiSpeedWind>().IsEmitting)failures.Add("Speed wind continued emitting while paused");
                game.ResumeGame();
            }
        }
        if(PlayerPrefs.GetInt("Lumi.Unlocked",1)!=unlocked)failures.Add("Validation changed progression");
        game.ShowLevelMenu();
        foreach(TextMeshProUGUI label in game.GetComponentsInChildren<TextMeshProUGUI>(true))
            if(label.font==null)failures.Add("Missing SDF font on "+label.name);
        string report=failures.Count==0?"PASS: five larger levels, routes to distant exits, enemy tiers, pickups, boundaries, health, armor, invisibility initialization, progression preservation, music loops, chibi pistols, backward projectile ownership, stride animation, speed-wind movement/pause and SDF font references.":"FAIL:\n"+string.Join("\n",failures);
        System.IO.File.WriteAllText("Logs/VanguardPlayModeValidation.txt",report);
        Debug.Log(report);
    }
}
