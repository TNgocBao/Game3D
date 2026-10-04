using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LumiAdventure;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class LumiCharacterAnimationValidation
{
    private const string Request="Temp/CharacterAnimationValidation.request";
    static LumiCharacterAnimationValidation(){EditorApplication.update+=Poll;EditorApplication.playModeStateChanged+=OnPlay;}
    private static void Poll()
    {
        if(SessionState.GetBool("Lumi.CharacterQA",false) && EditorApplication.isPlaying && !EditorApplication.isCompiling && !EditorApplication.isUpdating)
        {
            if(Object.FindObjectOfType<LumiGame>()!=null){SessionState.SetBool("Lumi.CharacterQA",false);Run();}
            return;
        }
        if(!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)return;
        try{File.Delete(Request);}catch(IOException){return;}LumiShippudenNarutoBuild.RemoveHandStaves();SessionState.SetBool("Lumi.CharacterQA",true);EditorApplication.isPlaying=true;
    }
    private static void OnPlay(PlayModeStateChange state)
    {
        if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("Lumi.CharacterQA",false))EditorApplication.delayCall+=Poll;
    }
    [MenuItem("Naruto/Validate character motion and chakra explosion")]
    public static void Run()
    {
        LumiGame game=Object.FindObjectOfType<LumiGame>();if(Application.isPlaying && game!=null)game.StartCoroutine(Check(game));
    }
    private static IEnumerator Check(LumiGame game)
    {
        Directory.CreateDirectory("Logs/CharacterQA");var errors=new List<string>();var report=new List<string>();int unlocked=PlayerPrefs.GetInt("Lumi.Unlocked",1);
        game.StartLevelForValidation(1);yield return null;
        foreach(LumiEnemy enemy in Object.FindObjectsOfType<LumiEnemy>())enemy.enabled=false;
        game.VillageBoss.enabled=false;LumiPlayer player=game.Player;player.enabled=false;
        // Isolate combat probes from scenery and unrelated enemies in the presentation stage.
        foreach(Collider scenery in Object.FindObjectsOfType<Collider>())if(!scenery.transform.IsChildOf(player.transform))scenery.enabled=false;
        foreach(Renderer scenery in Object.FindObjectsOfType<Renderer>())if(!scenery.transform.IsChildOf(player.transform))scenery.enabled=false;
        GameObject floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.name="QA presentation floor";floor.transform.position=player.transform.position+new Vector3(0,-.04f,3);floor.transform.localScale=new Vector3(24,.05f,35);floor.GetComponent<Collider>().enabled=false;
        floor.GetComponent<Renderer>().sharedMaterial=LumiFactory.Material("QA neutral ground",new Color(.26f,.30f,.34f));floor.transform.SetParent(player.transform.parent,true);
        LumiInfantryMotion motion=player.GetComponentInChildren<LumiInfantryMotion>();LumiNarutoSkills skills=player.GetComponent<LumiNarutoSkills>();
        game.CameraRig.SetFirstPerson(false);game.CameraRig.enabled=false;game.SetMouseLook(false);
        motion.SetGrounded(true);yield return new WaitForSeconds(.4f);
        if(!motion.RigConfigured)errors.Add("Rig bone mapping failed.");
        Transform arm=Bone(motion,"leftArm"),leg=Bone(motion,"leftLeg"),knee=Bone(motion,"leftKnee"),head=Bone(motion,"head");
        foreach(string name in new[]{"leftLeg","rightLeg","leftKnee","rightKnee","leftFoot","rightFoot","leftArm","rightArm","leftElbow","rightElbow","leftHand","rightHand"})
            if(Bone(motion,name)==null)errors.Add("Missing joint: "+name);
        if(arm!=null && player.ChakraHand.position.y>arm.position.y-.25f)errors.Add("Idle left arm is not lowered.");
        if(Bone(motion,"rightArm")!=null && player.WeaponHand.position.y>Bone(motion,"rightArm").position.y-.25f)errors.Add("Idle right arm is not lowered.");
        report.Add("Idle hand heights: left="+player.ChakraHand.position.y.ToString("F3")+", right="+player.WeaponHand.position.y.ToString("F3"));
        float footSpacing=Mathf.Abs(motion.transform.InverseTransformPoint(Bone(motion,"leftFoot").position).x-motion.transform.InverseTransformPoint(Bone(motion,"rightFoot").position).x);
        if(footSpacing>.22f)errors.Add("Idle stance remains too wide: "+footSpacing);
        report.Add("Idle ankle spacing: "+footSpacing.ToString("F3")+"m");
        foreach(SkinnedMeshRenderer renderer in player.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            Material[] mats=renderer.sharedMaterials;var eyes=new List<Material>();foreach(var mat in mats)if(mat!=null && mat.name.ToLowerInvariant().Contains("eye"))eyes.Add(mat);if(eyes.Count>=2 && (eyes[0]!=eyes[1] || eyes[0].mainTexture==null || eyes[0].mainTexture.name!="Eye"))errors.Add("Eyes do not share the golden-eye material.");
            for(int slot=0;slot<mats.Length;slot++)if(mats[slot]!=null && (mats[slot].name.Contains("wep01") || mats[slot].name.Contains("wep02")))
                if(renderer.sharedMesh.GetIndexCount(slot)!=0)errors.Add("Hand stave geometry remains visible.");
        }
        var fingers=motion.GetComponent<LumiFingerMotion>();if(fingers==null || fingers.JointCount!=30)errors.Add("Missing thirty finger joints.");
        Transform digit=null;foreach(var t in motion.GetComponentsInChildren<Transform>())if(t.name=="arm left finger middle 1")digit=t;Quaternion fist=digit!=null?digit.localRotation:Quaternion.identity;
        Capture(game,player.transform.position,"Idle.png");Capture(game,player.ChakraHand.position,"FistClose.png",.65f,true);Capture(game,head.position+Vector3.up*.09f,"GoldenEyesClose.png",.65f,true);
        Quaternion a=arm.rotation,l=leg.rotation,k=knee.rotation;
        float armChange=0,legChange=0,kneeChange=0;
        for(int i=0;i<30;i++)
        {
            player.Controller.Move(Vector3.forward*5.2f*Time.deltaTime);yield return new WaitForEndOfFrame();
            armChange=Mathf.Max(armChange,Quaternion.Angle(a,arm.rotation));legChange=Mathf.Max(legChange,Quaternion.Angle(l,leg.rotation));kneeChange=Mathf.Max(kneeChange,Quaternion.Angle(k,knee.rotation));
            if(i==9)Capture(game,player.transform.position,"RunA.png");if(i==19)Capture(game,player.transform.position,"RunB.png");
        }
        if(armChange<8 || legChange<8 || kneeChange<8)errors.Add("Locomotion did not move all tested limb joints.");
        report.Add("Locomotion joint changes (degrees): arm="+armChange.ToString("F1")+", thigh="+legChange.ToString("F1")+", knee="+kneeChange.ToString("F1"));
        motion.SetGrounded(false);yield return new WaitForSeconds(.15f);Capture(game,player.transform.position,"Jump.png");motion.SetGrounded(true);yield return new WaitForSeconds(.5f);
        if(!skills.BeginCharge(1))errors.Add("Rasengan charge was rejected.");yield return new WaitForSeconds(.28f);
        float sealDistance=Vector3.Distance(player.ChakraHand.position,player.WeaponHand.position);report.Add("Seal palm distance: "+sealDistance.ToString("F3")+"m; finger joints: "+fingers.JointCount);if(sealDistance>.22f)errors.Add("Seal hands did not meet: "+sealDistance);Capture(game,player.transform.position,"HandSeal.png",2.2f);Capture(game,(player.ChakraHand.position+player.WeaponHand.position)*.5f,"HandSealClose.png",.7f,true);
        yield return new WaitForSeconds(.52f);if(digit==null || Quaternion.Angle(fist,digit.localRotation)<15)errors.Add("Charge did not soften the fist.");
        Vector3 side=motion.transform.InverseTransformPoint(player.ChakraHand.position)-motion.transform.InverseTransformPoint(arm.position);
        if(side.x>-.3f)errors.Add("Rasengan charge did not extend the hand to the side.");
        Capture(game,player.transform.position,"RasenganCharge.png");skills.ReleaseCharge(1);yield return new WaitForSeconds(.3f);Capture(game,player.transform.position,"RasenganThrust.png");yield return new WaitForSeconds(.7f);
        if(!skills.BeginCharge(2))errors.Add("Rasenshuriken charge was rejected.");yield return new WaitForSeconds(.8f);
        float over=player.ChakraHand.position.y-head.position.y;if(over<.1f)errors.Add("Rasenshuriken hand did not rise above the head.");
        report.Add("Overhead hand above head joint: "+over.ToString("F3")+"m");Capture(game,player.transform.position,"RasenshurikenCharge.png");
        skills.ReleaseCharge(2);yield return new WaitForSeconds(.38f);Capture(game,player.transform.position,"RasenshurikenThrow.png");yield return new WaitForSeconds(.5f);
        foreach(LumiChakraProjectile shot in Object.FindObjectsOfType<LumiChakraProjectile>())Object.Destroy(shot.gameObject);
        Vector3 impact=player.transform.position+Vector3.forward*7+Vector3.up*1;
        GameObject target=new GameObject("QA explosion target",typeof(CharacterController));target.transform.position=impact+Vector3.right-Vector3.up;target.transform.SetParent(player.transform.parent,true);
        LumiEnemy dummy=target.AddComponent<LumiEnemy>();dummy.Initialize(game,player,LumiEnemyType.Golem,1);dummy.enabled=false;
        GameObject duplicate=new GameObject("QA duplicate target collider",typeof(SphereCollider));duplicate.transform.SetParent(target.transform,false);duplicate.transform.localPosition=Vector3.up*.8f;duplicate.GetComponent<SphereCollider>().radius=.3f;
        yield return null;var targetBody=target.GetComponent<CharacterController>();targetBody.enabled=false;targetBody.enabled=true;targetBody.Move(Vector3.zero);Physics.SyncTransforms();
        Vector3 launch=player.transform.position+Vector3.up*2.4f;
        Ray aimRay=new Ray(launch,dummy.AimPoint-launch);Vector3 targetPoint=player.ResolveAimPoint(aimRay);
        if(Vector3.Distance(targetPoint,dummy.AimPoint)>.05f)errors.Add("Mouse ray did not resolve target: "+targetPoint+" expected "+dummy.AimPoint+" bounds "+dummy.GetComponent<CharacterController>().bounds);
        Vector3 diagonal=(targetPoint-launch).normalized;
        if(diagonal.y>=-.02f || Mathf.Abs(diagonal.x)<.01f)errors.Add("Aiming flattened the diagonal/downward target direction.");
        GameObject probe=new GameObject("QA projectile sweep");probe.transform.SetParent(player.transform.parent,true);LumiProjectile tracer=probe.AddComponent<LumiProjectile>();tracer.Initialize(game,diagonal,16,18,true,player.transform);tracer.enabled=false;
        if(!tracer.Trace(launch,diagonal,Vector3.Distance(launch,targetPoint)+1,out RaycastHit directHit,.45f) || directHit.collider.GetComponentInParent<LumiEnemy>()!=dummy)errors.Add("Diagonal chakra sweep missed the target.");
        Vector3 wideOrigin=dummy.AimPoint+Vector3.left*(dummy.GetComponent<CharacterController>().radius+.3f)+Vector3.back*3;
        if(!tracer.Trace(wideOrigin,Vector3.forward,5,out RaycastHit edgeHit,.45f) || edgeHit.collider.GetComponentInParent<LumiEnemy>()!=dummy)errors.Add("Chakra edge probe hit: "+(edgeHit.collider!=null?edgeHit.collider.name:"none"));
        report.Add("3D mouse-ray aim and chakra-radius diagonal/edge sweeps checked.");Object.Destroy(probe);
        GameObject blast=new GameObject("QA Rasenshuriken blast");blast.transform.SetParent(player.transform.parent,true);blast.transform.position=impact;
        LumiRasenshurikenBlast effect=blast.AddComponent<LumiRasenshurikenBlast>();effect.Initialize(game,skills,3,18,1.4f);
        yield return new WaitForSeconds(.35f);Capture(game,impact-Vector3.up,"RasenshurikenImpact.png",6);
        yield return new WaitForSeconds(.45f);int hp=(int)typeof(LumiEnemy).GetField("health",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(dummy);
        if(hp!=42)errors.Add("Explosion damage expected 18 once across three pulses, got "+(60-hp));
        if(effect.PulsesApplied!=3)errors.Add("Explosion did not apply exactly three pulses.");
        report.Add("Explosion target HP: 60 -> "+hp+" with a duplicate collider; pulses="+effect.PulsesApplied);
        game.PauseGame();yield return new WaitForSecondsRealtime(.25f);
        if(effect.PulsesApplied!=3)errors.Add("Paused blast changed pulse count.");game.ResumeGame();yield return new WaitForSeconds(.9f);
        if(effect!=null)errors.Add("Explosion did not clean up after its lifetime.");
        game.StartLevelForValidation(1);yield return null;foreach(var enemy in Object.FindObjectsOfType<LumiEnemy>())enemy.enabled=false;game.VillageBoss.enabled=false;game.Player.enabled=false;var quick=game.Player.GetComponent<LumiNarutoSkills>();quick.BeginCharge(2);quick.ReleaseCharge(2);if(quick.ChargingAbility!=2)errors.Add("Quick release skipped seal.");game.PauseGame();yield return new WaitForSecondsRealtime(.15f);if(quick.ChargingAbility!=2)errors.Add("Pending release ignored pause.");game.ResumeGame();yield return new WaitForSeconds(.8f);if(quick.ChargingAbility!=-1)errors.Add("Quick release never completed.");report.Add("Quick release completes seal and respects pause.");
        if(PlayerPrefs.GetInt("Lumi.Unlocked",1)!=unlocked)errors.Add("QA changed progression.");
        game.ShowLevelMenu();string result=(errors.Count==0?"PASS":"FAIL")+"\n"+string.Join("\n",report)+"\n"+string.Join("\n",errors);
        File.WriteAllText("Logs/CharacterQA/Validation.txt",result);Debug.Log(result);EditorApplication.isPlaying=false;
    }
    private static Transform Bone(LumiInfantryMotion motion,string name)=>typeof(LumiInfantryMotion).GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(motion) as Transform;
    private static void Capture(LumiGame game,Vector3 center,string name,float distance=3.4f,bool hands=false)
    {
        Camera camera=game.CameraRig.ViewCamera;camera.transform.position=center+new Vector3(distance*.8f,2.0f,distance*1.1f);camera.transform.LookAt(center+Vector3.up*1.35f);camera.fieldOfView=50;if(hands){camera.transform.position=center+game.Player.transform.forward*distance+game.Player.transform.right*.18f+Vector3.up*.10f;camera.transform.LookAt(center);}
        RenderTexture rt=RenderTexture.GetTemporary(1280,960,24);RenderTexture old=camera.targetTexture,active=RenderTexture.active;var image=new Texture2D(1280,960,TextureFormat.RGB24,false);
        try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,960),0,0);image.Apply();File.WriteAllBytes("Logs/CharacterQA/"+name,image.EncodeToPNG());}
        finally{camera.targetTexture=old;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);Object.Destroy(image);}
    }
}
