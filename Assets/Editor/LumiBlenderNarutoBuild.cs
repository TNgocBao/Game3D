using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LumiAdventure;
public static class LumiBlenderNarutoBuild
{
 public static void Run()
 {
  try{Build();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
 }
 public static void Build()
 {
  const string input="Assets/Art/NarutoReference/Naruto-Reference-Sculpt.fbx",dir="Assets/Resources/NarutoChibi";
  AssetDatabase.ImportAsset(input,ImportAssetOptions.ForceSynchronousImport);
  var source=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(input));
  var original=source.GetComponentInChildren<SkinnedMeshRenderer>();
  if(original==null)throw new Exception("Missing Blender skin");
  var root=new GameObject("Naruto Chibi Reference");root.layer=30;
  var mesh=UnityEngine.Object.Instantiate(original.sharedMesh);mesh.name="Naruto Blender Reference Skin";
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
  Directory.CreateDirectory(dir);string meshPath=dir+"/Naruto-Blender-Reference.asset";
  if(AssetDatabase.LoadAssetAtPath<Mesh>(meshPath)!=null)AssetDatabase.DeleteAsset(meshPath);
  AssetDatabase.CreateAsset(mesh,meshPath);
  var renderer=root.AddComponent<SkinnedMeshRenderer>();renderer.sharedMesh=mesh;renderer.bones=transforms;renderer.rootBone=transforms.First(t=>t.name=="Rig root");renderer.localBounds=mesh.bounds;renderer.updateWhenOffscreen=true;
  var materials=new Material[original.sharedMaterials.Length];
  for(int i=0;i<materials.Length;i++){
   var imported=original.sharedMaterials[i];var mat=new Material(Shader.Find("Standard"));mat.name=imported.name;mat.color=ReferenceColor(imported.name);mat.SetFloat("_Glossiness",imported.name.Contains("steel")?.48f:.18f);mat.SetFloat("_Metallic",imported.name.Contains("steel")?.55f:0);
   string path=dir+"/Blender-"+i+".mat";var saved=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(saved==null){AssetDatabase.CreateAsset(mat,path);saved=mat;}else{EditorUtility.CopySerialized(mat,saved);UnityEngine.Object.DestroyImmediate(mat);}materials[i]=saved;
  }
  renderer.sharedMaterials=materials;root.AddComponent<LumiInfantryMotion>();
  var head=transforms.First(t=>t.name=="Head pivot");var socket=new GameObject("Eye camera socket").transform;socket.SetParent(head,false);socket.position=head.position+Vector3.up*.065f;
  PrefabUtility.SaveAsPrefabAsset(root,dir+"/Naruto.prefab");AssetDatabase.SaveAssets();
  Directory.CreateDirectory("Logs/NarutoQA");File.WriteAllText("Logs/NarutoQA/BlenderImport.txt","PASS: Blender reference skin, "+mesh.vertexCount+" vertices, "+transforms.Length+" bones, "+materials.Length+" material slots. Bounds: "+mesh.bounds);
  UnityEngine.Object.DestroyImmediate(source);UnityEngine.Object.DestroyImmediate(root);
 }
 private static Color ReferenceColor(string name)
 {
  if(name.Contains("Skin"))return new Color(.90f,.73f,.62f);
  if(name.Contains("blond"))return new Color(.88f,.76f,.49f);
  if(name.Contains("Orange"))return new Color(.88f,.48f,.24f);
  if(name.Contains("Navy"))return new Color(.14f,.20f,.29f);
  if(name.Contains("steel"))return new Color(.58f,.61f,.64f);
  if(name.Contains("sclera"))return new Color(.97f,.97f,.93f);
  if(name.Contains("iris"))return new Color(.16f,.46f,.61f);
  if(name.Contains("Pupil"))return new Color(.045f,.085f,.12f);
  if(name.Contains("Canvas"))return new Color(.59f,.51f,.40f);
  if(name.Contains("emblem"))return new Color(.64f,.22f,.20f);
  return new Color(.15f,.13f,.12f);
 }

}
