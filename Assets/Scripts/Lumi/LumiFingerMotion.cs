using System.Collections.Generic;
using UnityEngine;
namespace LumiAdventure {
// Retain imported bind rotations and bend each phalanx toward its own palm.
public sealed class LumiFingerMotion:MonoBehaviour {
 sealed class Joint {public Transform bone;public Quaternion rest;public Vector3 axis;public float angle;public string finger;}
 sealed class Palm {public Transform bone;public Quaternion rest;public Vector3 forward,normal;public bool left;}
 readonly List<Palm> palms=new List<Palm>();
 readonly List<Joint> joints=new List<Joint>();
 public int JointCount=>joints.Count;
 public float Curl {get;private set;}=1;
 public void Initialize(){if(joints.Count>0)return;foreach(string side in new[]{"left","right"}){
 Transform hand=Find("arm "+side+" hand"),index=Find("arm "+side+" finger index 0"),pinky=Find("arm "+side+" finger pinky 0"),middle=Find("arm "+side+" finger middle 0");if(hand==null||index==null||pinky==null||middle==null)continue;
 Vector3 forward=(middle.position-hand.position).normalized,across=(index.position-pinky.position).normalized;Vector3 palm=Vector3.Cross(across,forward)*(side=="left"?1:-1);palms.Add(new Palm{bone=hand,rest=hand.localRotation,forward=hand.InverseTransformDirection(forward),normal=hand.InverseTransformDirection(palm),left=side=="left"});
 foreach(string finger in new[]{"index","middle","ring","pinky","thumb"})for(int segment=0;segment<3;segment++){
 Transform bone=Find("arm "+side+" finger "+finger+" "+segment),tip=Find("arm "+side+" finger "+finger+" "+(segment==2?"2_end":(segment+1).ToString()));if(bone==null||tip==null)continue;
 Vector3 axis=Vector3.Cross((tip.position-bone.position).normalized,palm).normalized;
 if(finger=="thumb" && segment==0)axis=Vector3.Cross((tip.position-bone.position).normalized,(middle.position-bone.position).normalized).normalized;
 joints.Add(new Joint{bone=bone,rest=bone.localRotation,axis=bone.InverseTransformDirection(axis),angle=finger=="thumb"?(segment==0?28:48):(segment==0?52:segment==1?82:65),finger=finger});
 }} }
 Transform Find(string name){foreach(var bone in GetComponentsInChildren<Transform>(true))if(bone.name==name)return bone;return null;}
 public void PosePalms(Transform root,float seal,bool charging,bool overhead,float response){foreach(var p in palms){Quaternion rest=p.bone.parent.rotation*p.rest;Quaternion target=rest;if(charging){Vector3 facing=overhead?root.up:root.TransformDirection(new Vector3(-.65f,.1f,.75f)).normalized;target=Quaternion.FromToRotation(rest*p.normal,facing)*rest;}Quaternion sealPose=Quaternion.LookRotation(root.up,p.left?root.right:-root.right)*Quaternion.Inverse(Quaternion.LookRotation(p.forward,p.normal));target=Quaternion.Slerp(target,sealPose,seal);p.bone.rotation=Quaternion.Slerp(p.bone.rotation,target,seal>.5f?Mathf.Max(response,1-Mathf.Exp(-22*Time.deltaTime)):response);}}
 public void Apply(float curl,float seal,float response){Initialize();Curl=Mathf.Lerp(Curl,curl,response);foreach(var j in joints){float amount=Mathf.Lerp(curl,j.finger=="index"||j.finger=="middle"?0f:.88f,seal);Quaternion target=j.rest*Quaternion.AngleAxis(j.angle*amount,j.axis);j.bone.localRotation=Quaternion.Slerp(j.bone.localRotation,target,response);}}
}
}