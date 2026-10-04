using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LumiAdventure {
// Vertices on each identical-weight convex hull preserve the exact skinned bounds.
internal static class LumiHitHullSamples {
 [System.Serializable] sealed class Data{public int vertexCount;public int[] indices;}
 static readonly Dictionary<Mesh,bool[]> cache=new Dictionary<Mesh,bool[]>();
 static bool configured;
 public static void ConfigureDamageLayer(GameObject root){root.layer=31;if(configured)return;for(int layer=0;layer<32;layer++)Physics.IgnoreLayerCollision(31,layer,true);configured=true;}
 public static bool[] Load(Mesh mesh,string name){if(cache.TryGetValue(mesh,out var found))return found;var text=Resources.Load<TextAsset>("LumiEnemies/"+name+"-HitHull");if(text==null)return null;var data=JsonUtility.FromJson<Data>(text.text);if(data.vertexCount!=mesh.vertexCount)return null;var selected=new bool[mesh.vertexCount];foreach(int i in data.indices){if(i<0||i>=selected.Length)return null;selected[i]=true;}cache[mesh]=selected;return selected;}
}
// Shared animation/combat driver for the approved Matatabi and Gyuki prefabs.
public sealed class LumiImportedBijuuMotion:MonoBehaviour {
 public bool Ranged;
 public bool Busy=>routine!=null;
 public BoxCollider BodyHitbox=>colliders.Count>0?colliders[0]:null;
 public bool WithinMeleeReach(Component target,float reach){if(target==null||BodyHitbox==null)return false;Vector3 at=target is LumiShadowClone clone?clone.AimPoint:target.transform.position+Vector3.up*.9f;return Vector3.Distance(at,BodyHitbox.bounds.ClosestPoint(at))<=reach;}
 LumiGame game;LumiEnemy owner;Animation clips;Transform mouth;Vector3 previous;Coroutine routine;GameObject orb;
 SkinnedMeshRenderer skin;Transform[] bones;Vector3[] vertices;BoneWeight[] weights;Matrix4x4[] bind,posed;
 readonly List<List<int>> groups=new List<List<int>>();readonly List<BoxCollider> colliders=new List<BoxCollider>();float nextHit;List<List<int>> referenceGroups;
 public void Initialize(LumiGame session,LumiEnemy enemy){game=session;owner=enemy;LumiHitHullSamples.ConfigureDamageLayer(gameObject);clips=GetComponentInChildren<Animation>();skin=GetComponentInChildren<SkinnedMeshRenderer>();previous=transform.position;foreach(var t in GetComponentsInChildren<Transform>())if(t.name=="Bijuu Mouth")mouth=t;CreateHitboxes();nextHit=Time.time+Random.Range(0f,.1f);skin.updateWhenOffscreen=false;if(!Ranged)AddFlames();}
 void CreateHitboxes(){if(skin==null||colliders.Count>0)return;vertices=skin.sharedMesh.vertices;weights=skin.sharedMesh.boneWeights;bind=skin.sharedMesh.bindposes;bones=skin.bones;posed=new Matrix4x4[bones.Length];groups.Add(new List<int>());var tails=new Dictionary<int,int>();for(int i=0;i<weights.Length;i++){var w=weights[i];int b=w.boneIndex0;float max=w.weight0;if(w.weight1>max){b=w.boneIndex1;max=w.weight1;}if(w.weight2>max){b=w.boneIndex2;max=w.weight2;}if(w.weight3>max)b=w.boneIndex3;int group=0;if(bones[b].name.StartsWith("Tail_")){if(!tails.TryGetValue(b,out group)){group=groups.Count;tails[b]=group;groups.Add(new List<int>());}}groups[group].Add(i);}referenceGroups=new List<List<int>>();foreach(var group in groups)referenceGroups.Add(new List<int>(group));var sample=LumiHitHullSamples.Load(skin.sharedMesh,Ranged?"Gyuki":"Matatabi");if(sample!=null)foreach(var group in groups)group.RemoveAll(i=>!sample[i]);foreach(var group in groups)colliders.Add(gameObject.AddComponent<BoxCollider>());RefreshHits();}
 void RefreshHitsReference(){if(skin==null)return;var space=transform.worldToLocalMatrix;for(int b=0;b<posed.Length;b++)posed[b]=space*bones[b].localToWorldMatrix*bind[b];for(int g=0;g<groups.Count;g++){Bounds bounds=new Bounds();bool first=true;foreach(int i in referenceGroups[g]){var w=weights[i];Vector3 v=vertices[i];Vector3 p=posed[w.boneIndex0].MultiplyPoint3x4(v)*w.weight0;if(w.weight1>0)p+=posed[w.boneIndex1].MultiplyPoint3x4(v)*w.weight1;if(w.weight2>0)p+=posed[w.boneIndex2].MultiplyPoint3x4(v)*w.weight2;if(w.weight3>0)p+=posed[w.boneIndex3].MultiplyPoint3x4(v)*w.weight3;if(first){bounds=new Bounds(p,Vector3.zero);first=false;}else bounds.Encapsulate(p);}colliders[g].center=bounds.center;colliders[g].size=bounds.size+Vector3.one*.035f;}}
 void RefreshHits(){if(skin==null)return;var space=transform.worldToLocalMatrix;for(int b=0;b<posed.Length;b++)posed[b]=space*bones[b].localToWorldMatrix*bind[b];for(int g=0;g<groups.Count;g++){Vector3 min=new Vector3(float.PositiveInfinity,float.PositiveInfinity,float.PositiveInfinity),max=-min;foreach(int i in groups[g]){var w=weights[i];Vector3 v=vertices[i];Vector3 p=posed[w.boneIndex0].MultiplyPoint3x4(v)*w.weight0;if(w.weight1>0)p+=posed[w.boneIndex1].MultiplyPoint3x4(v)*w.weight1;if(w.weight2>0)p+=posed[w.boneIndex2].MultiplyPoint3x4(v)*w.weight2;if(w.weight3>0)p+=posed[w.boneIndex3].MultiplyPoint3x4(v)*w.weight3;min=Vector3.Min(min,p);max=Vector3.Max(max,p);}colliders[g].center=(min+max)*.5f;colliders[g].size=max-min+Vector3.one*.035f;}}
 void LateUpdate(){if(game!=null&&game.IsPlaying&&Time.time>=nextHit){float distance=game.Player!=null?(game.Player.transform.position-transform.position).sqrMagnitude:0;nextHit=Time.time+(distance>1600?Random.Range(.22f,.28f):Random.Range(.085f,.115f));RefreshHits();}}
 void Update(){if(game==null||!game.IsPlaying||owner==null||!owner.IsAlive)return;Vector3 delta=transform.position-previous;delta.y=0;previous=transform.position;if(Busy)return;float speed=delta.magnitude/Mathf.Max(.001f,Time.deltaTime);string clip=speed>.15f?"Run":"Idle";if(clips!=null&&clips[clip]!=null){clips[clip].speed=clip=="Run"?Mathf.Clamp(speed/(Ranged?1.8f:2.4f),.7f,1.4f):1;if(!clips.IsPlaying(clip))clips.CrossFade(clip,.16f);}}
 public void Attack(Component target,int damage){if(!Busy&&target!=null)routine=StartCoroutine(Perform(target,damage));}
 IEnumerator Perform(Component target,int damage){if(clips!=null&&clips["Attack"]!=null){clips["Attack"].speed=1;clips.CrossFade("Attack",.12f);}float age=0;bool released=false;if(Ranged)orb=LumiTailedBeastBomb.CreateVisual(transform,"Gyuki charging tailed beast bomb");float duration=Ranged?2:.8f;while(age<duration&&owner!=null&&owner.IsAlive){if(game.IsPlaying){age+=Time.deltaTime;if(!Ranged&&age<.35f)owner.GetComponent<CharacterController>().Move(owner.transform.forward*Time.deltaTime);if(orb!=null){orb.transform.position=(mouth!=null?mouth.position:transform.position+Vector3.up*1.2f)+owner.transform.forward*.2f;orb.transform.localScale=Vector3.one*Mathf.Lerp(.08f,.55f,Mathf.Clamp01(age/1.15f));orb.transform.Rotate(0,180*Time.deltaTime,50*Time.deltaTime);}if(!released&&age>=(Ranged?1.15f:.38f)){released=true;if(target!=null&&((ILumiDamageable)target).IsAlive){if(Ranged){GameObject shot=orb;orb=null;shot.transform.SetParent(owner.transform.parent,true);Vector3 aim=target.transform.position+Vector3.up*.9f-shot.transform.position;shot.AddComponent<LumiTailedBeastBomb>().Initialize(game,owner.transform,aim.normalized,damage);game.Audio.Play("chakra",.65f);}else if(WithinMeleeReach(target,1.1f)){((ILumiDamageable)target).TakeDamage(damage,target.transform.position+Vector3.up*.8f);game.SpawnImpact(target.transform.position+Vector3.up*.8f,new Color(.1f,.8f,1));}}}}yield return null;}if(orb!=null){Destroy(orb);orb=null;}routine=null;}
 void AddFlames(){foreach(var t in GetComponentsInChildren<Transform>()){if(t.name!="Head"&&t.name!="Tail_01_04"&&t.name!="Tail_02_04")continue;var go=new GameObject("Matatabi chakra flame embers");go.transform.SetParent(t,false);var p=go.AddComponent<ParticleSystem>();var main=p.main;main.startLifetime=.5f;main.startSpeed=.15f;main.startSize=.04f;main.startColor=new Color(.1f,.85f,1,.65f);main.maxParticles=20;main.simulationSpace=ParticleSystemSimulationSpace.World;var emission=p.emission;emission.rateOverTime=14;var shape=p.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.1f;var renderer=p.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=LumiFactory.Material("Matatabi cyan embers",new Color(.1f,.85f,1,.65f),true,true);}}
 public void CancelAttack(){if(routine!=null)StopCoroutine(routine);routine=null;if(orb!=null)Destroy(orb);orb=null;if(clips!=null&&clips["Idle"]!=null)clips.CrossFade("Idle",.15f);}
 void OnDisable(){CancelAttack();}
}}











