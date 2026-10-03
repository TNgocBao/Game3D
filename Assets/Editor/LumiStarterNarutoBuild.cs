using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LumiAdventure;
public static class LumiStarterNarutoBuild
{
 public static void Run()
 {
  try{Build();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
 }
 [MenuItem("Naruto/Rebuild Starter character")]
 public static void Build()
 {
  const string input="Assets/Art/NarutoStarter/NarutoStarter.fbx",dir="Assets/Resources/NarutoChibi";
  AssetDatabase.ImportAsset(input,ImportAssetOptions.ForceSynchronousImport);
  var source=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(input));
  var original=source.GetComponentInChildren<SkinnedMeshRenderer>();
  if(original==null)throw new Exception("Missing Blender skin");
  var root=new GameObject("Naruto Starter");root.layer=30;
  var mesh=UnityEngine.Object.Instantiate(original.sharedMesh);mesh.name="Naruto Starter Rigged Skin";
  var vertices=mesh.vertices;var normals=mesh.normals;var matrix=original.transform.localToWorldMatrix;
  for(int i=0;i<vertices.Length;i++){vertices[i]=matrix.MultiplyPoint3x4(vertices[i]);normals[i]=matrix.MultiplyVector(normals[i]).normalized;}
  mesh.vertices=vertices;mesh.normals=normals;
  var transforms=new Transform[original.bones.Length];
  for(int i=0;i<transforms.Length;i++){transforms[i]=new GameObject(original.bones[i].name).transform;transforms[i].position=original.bones[i].position;transforms[i].SetParent(root.transform,true);transforms[i].rotation=Quaternion.identity;}
  for(int i=0;i<transforms.Length;i++){
   var parent=original.bones[i].parent;int index=Array.IndexOf(original.bones,parent);
   if(index>=0)transforms[i].SetParent(transforms[index],true);
  }
  mesh.bindposes=transforms.Select(t=>t.worldToLocalMatrix*root.transform.localToWorldMatrix).ToArray();mesh.RecalculateBounds();
  Directory.CreateDirectory(dir);string meshPath=dir+"/Naruto-Starter.asset";
  if(AssetDatabase.LoadAssetAtPath<Mesh>(meshPath)!=null)AssetDatabase.DeleteAsset(meshPath);
  AssetDatabase.CreateAsset(mesh,meshPath);
  var renderer=root.AddComponent<SkinnedMeshRenderer>();renderer.sharedMesh=mesh;renderer.bones=transforms;renderer.rootBone=transforms.First(t=>t.name=="Rig root");renderer.localBounds=mesh.bounds;renderer.updateWhenOffscreen=true;
  var materials=new Material[original.sharedMaterials.Length];
  for(int i=0;i<materials.Length;i++){
   var imported=original.sharedMaterials[i];var mat=new Material(Shader.Find("Standard"));mat.name=imported.name;mat.color=ReferenceColor(imported.name);mat.SetFloat("_Glossiness",imported.name.Contains("Metal")?.48f:.18f);mat.SetFloat("_Metallic",imported.name.Contains("Metal")?.55f:0);
   string path=dir+"/Starter-"+i+".mat";var saved=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(saved==null){AssetDatabase.CreateAsset(mat,path);saved=mat;}else{EditorUtility.CopySerialized(mat,saved);UnityEngine.Object.DestroyImmediate(mat);}materials[i]=saved;
  }
  renderer.sharedMaterials=materials;root.AddComponent<LumiInfantryMotion>();
  var head=transforms.First(t=>t.name=="Head pivot");var socket=new GameObject("Eye camera socket").transform;socket.SetParent(head,false);socket.position=head.position+Vector3.up*.0423f;
  PrefabUtility.SaveAsPrefabAsset(root,dir+"/Naruto.prefab");AssetDatabase.SaveAssets();
  Directory.CreateDirectory("Logs/NarutoQA");File.WriteAllText("Logs/NarutoQA/StarterImport.txt","PASS: Starter rigged skin, "+mesh.vertexCount+" vertices, "+transforms.Length+" bones, "+materials.Length+" material slots. Bounds: "+mesh.bounds);
  UnityEngine.Object.DestroyImmediate(source);UnityEngine.Object.DestroyImmediate(root);
 }
 private static Color ReferenceColor(string name)
 {
  if(name=="Skin")return new Color(0.949f,0.7216f,0.5373f);
  if(name=="SkinShadow")return new Color(0.8235f,0.5294f,0.3725f);
  if(name=="Orange")return new Color(0.9373f,0.4f,0.102f);
  if(name=="OrangeLight")return new Color(1.0f,0.5569f,0.1725f);
  if(name=="Navy")return new Color(0.1098f,0.1647f,0.2824f);
  if(name=="Blue")return new Color(0.1647f,0.2392f,0.4f);
  if(name=="Metal")return new Color(0.6824f,0.7333f,0.7882f);
  if(name=="MetalDark")return new Color(0.3569f,0.4039f,0.4627f);
  if(name=="Yellow")return new Color(0.9804f,0.7765f,0.1922f);
  if(name=="YellowLight")return new Color(1.0f,0.8706f,0.3569f);
  if(name=="EyeWhite")return new Color(0.9765f,0.9647f,0.9137f);
  if(name=="Iris")return new Color(0.1725f,0.5176f,0.8627f);
  if(name=="Pupil")return new Color(0.0588f,0.1176f,0.1882f);
  if(name=="Black")return new Color(0.0745f,0.0902f,0.1255f);
  if(name=="Red")return new Color(0.7529f,0.1882f,0.2157f);
  if(name=="Bandage")return new Color(0.8863f,0.8588f,0.7961f);
  if(name=="Pouch")return new Color(0.5647f,0.4863f,0.4196f);
  return Color.gray;
 }
}
