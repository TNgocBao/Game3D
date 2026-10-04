using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LumiAdventure;
using UnityEditor;
using UnityEngine;
[InitializeOnLoad]
public static class LumiCombatAIValidation
{
    private const string Request="Temp/CombatAIValidation.request";
    static LumiCombatAIValidation(){EditorApplication.update+=Poll;}
    private static void Poll()
    {
        if(EditorApplication.isCompiling || EditorApplication.isUpdating)return;
        if(File.Exists(Request)){if(EditorApplication.isPlaying){EditorApplication.isPlaying=false;return;}File.Delete(Request);SessionState.SetBool("Lumi.AIQA",true);EditorApplication.isPlaying=true;return;}
        if(SessionState.GetBool("Lumi.AIQA",false) && EditorApplication.isPlaying){var game=Object.FindObjectOfType<LumiGame>();if(game!=null){SessionState.SetBool("Lumi.AIQA",false);game.StartCoroutine(Check(game));}}
    }
    private static void Set(object obj,string field,object value)=>obj.GetType().GetField(field,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(obj,value);
    private static void Teleport(CharacterController c,Vector3 p){c.enabled=false;c.transform.position=p;c.enabled=true;Physics.SyncTransforms();}
    private static IEnumerator Check(LumiGame game)
    {
        var errors=new List<string>();Application.LogCallback log=(message,stack,type)=>{if(type==LogType.Exception || type==LogType.Error)errors.Add(message);};Application.logMessageReceived+=log;
        for(int level=1;level<=5;level++)
        {
            game.StartLevelForValidation(level);yield return null;
            var kage=game.VillageBoss;if(kage.MaxHealth!=(72+level*12)*2)errors.Add("Boss HP scaling failed on map "+level);
            foreach(var enemy in Object.FindObjectsOfType<LumiEnemy>()){int baseline=enemy.EnemyType==LumiEnemyType.Sprout?15:enemy.EnemyType==LumiEnemyType.Ranger?30:60;if(enemy.MaxHealth!=Mathf.RoundToInt(baseline*(1+.15f*(level-1))))errors.Add("Enemy HP scaling failed on map "+level);}
            kage.TakeDamage(kage.MaxHealth/2+1,kage.AimPoint);if(!kage.SecondPhaseUnlocked)errors.Add("Phase 2 did not unlock below 50%.");kage.Heal(kage.MaxHealth);if(!kage.SecondPhaseUnlocked)errors.Add("Healing removed phase 2.");
        }
        game.StartLevelForValidation(1);yield return null;var player=game.Player;player.enabled=false;
        foreach(var enemy in Object.FindObjectsOfType<LumiEnemy>())enemy.enabled=false;
        var boss=game.VillageBoss;boss.enabled=false;var altar=LumiHealingAltar.FindNearest(boss.transform.position);
        if(altar==null)errors.Add("Missing altar.");
        var cloneObj=new GameObject("QA clone",typeof(CharacterController));cloneObj.transform.SetParent(player.transform.parent,true);cloneObj.transform.position=boss.transform.position+Vector3.forward*1.2f;
        var clone=cloneObj.AddComponent<LumiShadowClone>();clone.Initialize(game,player,30);
        if(clone.MaxHealth!=4.5f || Mathf.Abs(clone.AttackDamage-1.2f)>.001f || Mathf.Abs(clone.MovementSpeed-player.MovementSpeed*.3f)>.001f)errors.Add("Clone 30% stats wrong.");
        if(LumiCombatTactics.SelectTarget(player,boss.transform.position,25,true)!=clone)errors.Add("Boss did not prefer clone.");
        clone.TakeDamage(1,clone.AimPoint);if(!clone.IsAlive || clone.Health!=3.5f)errors.Add("Clone dispersed after nonlethal hit.");
        clone.enabled=true;float before=boss.HealthFraction;yield return new WaitForSeconds(.65f);clone.enabled=false;LumiShadowClone.Active.Add(clone);
        if(boss.HealthFraction>=before)errors.Add("Clone did not attack boss.");
        boss.enabled=true;Set(boss,"nextSkill",Time.time+100);Set(boss,"nextMelee",0f);float cloneHP=clone.Health;yield return new WaitForSeconds(.15f);boss.enabled=false;
        if(clone.Health>=cloneHP)errors.Add("Boss did not damage clone.");
        Teleport(player.Controller,altar.transform.position+Vector3.back*20);
        var obj=new GameObject("QA low HP enemy",typeof(CharacterController));obj.transform.SetParent(player.transform.parent,true);obj.transform.position=altar.transform.position;
        var mob=obj.AddComponent<LumiEnemy>();mob.Initialize(game,player,LumiEnemyType.Golem,1);
        Teleport(clone.GetComponent<CharacterController>(),mob.transform.position+Vector3.forward*1.2f);float beforeMobHit=clone.Health;Set(mob,"nextAttack",0f);yield return new WaitForSeconds(.1f);if(clone.Health>=beforeMobHit)errors.Add("Enemy did not attack clone.");
        clone.Disperse();
        Set(mob,"health",4);
        yield return new WaitForSeconds(1.12f);if(Mathf.Abs(mob.HealthFraction-10f/60f)>.001f)errors.Add("Altar did not heal enemy by 10% max HP/s.");
        Set(boss,"health",8);Teleport(boss.GetComponent<CharacterController>(),altar.transform.position+Vector3.right*2);boss.enabled=true;Set(boss,"engaged",true);yield return new WaitForSeconds(1.12f);boss.enabled=false;
        if(boss.HealthFraction<=8f/168)errors.Add("Altar did not heal boss.");
        Teleport(player.Controller,altar.transform.position+Vector3.forward*3);float fraction=mob.HealthFraction;yield return new WaitForSeconds(1.12f);
        if(mob.HealthFraction>fraction)errors.Add("Contested altar continued healing.");
        if(LumiCombatTactics.SelectTarget(player,mob.transform.position,25,false)==null)errors.Add("Enemy lost player target.");
        mob.enabled=false;
        var shotObj=new GameObject("QA approaching chakra");shotObj.transform.position=mob.AimPoint+Vector3.back*5;
        var shot=shotObj.AddComponent<LumiChakraProjectile>();shot.Initialize(game,player.GetComponent<LumiNarutoSkills>(),player.transform,Vector3.forward,3);shot.enabled=false;LumiChakraProjectile.Active.Add(shot);
        if(!LumiCombatTactics.TryDangerEscape(mob.transform.position,.72f,out var escape) || Mathf.Abs(escape.x-mob.transform.position.x)<1)errors.Add("No projectile dodge decision.");
        LumiChakraProjectile.Active.Remove(shot);Object.Destroy(shotObj);
        var blastObj=new GameObject("QA blast danger");blastObj.transform.position=mob.transform.position;var blast=blastObj.AddComponent<LumiRasenshurikenBlast>();blast.Initialize(game,player.GetComponent<LumiNarutoSkills>(),6,18,1.4f);blast.enabled=false;LumiRasenshurikenBlast.Active.Add(blast);
        if(!LumiCombatTactics.TryDangerEscape(mob.transform.position,.72f,out escape) || Vector3.Distance(escape,mob.transform.position)<6)errors.Add("No escape from blast zone.");
        LumiRasenshurikenBlast.Active.Remove(blast);Object.Destroy(blastObj);
        if(clone!=null){clone.TakeDamage(100,clone.AimPoint);if(clone.IsAlive)errors.Add("Clone survived lethal damage.");}
        Application.logMessageReceived-=log;Directory.CreateDirectory("Logs/CombatAIQA");File.WriteAllText("Logs/CombatAIQA/Validation.txt",(errors.Count==0?"PASS":"FAIL")+"\nFive-map HP scaling and persistent phase-2 trigger; 30% stats; clone attacks boss; boss attacks clone; nonlethal/lethal clone damage; enemy and boss healing; contested altar; projectile and blast avoidance.\n"+string.Join("\n",errors));game.ShowLevelMenu();EditorApplication.isPlaying=false;
    }
}
