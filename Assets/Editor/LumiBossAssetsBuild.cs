using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using LumiAdventure;
[InitializeOnLoad] public static class LumiBossAssetsBuild {
 public static readonly string[] Names={"Hashirama","Gaara","Onoki","Raikage","Mei"};
 static LumiBossAssetsBuild(){EditorApplication.update+=Poll;}
 static void Poll(){if(!File.Exists("Temp/BossAssetsBuild.request")||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;File.Delete("Temp/BossAssetsBuild.request");try{Build();}catch(Exception e){Debug.LogException(e);File.WriteAllText("Logs/BossAssetsQA/Import.txt",e.ToString());}}
 [MenuItem("Naruto/Build Blender boss assets")] public static void Build(){Directory.CreateDirectory("Assets/Resources/LumiBosses");Directory.CreateDirectory("Assets/Art/Bosses/Materials");AssetDatabase.Refresh();
 foreach(string name in Names)for(int skill=1;skill<=2;skill++){
 
 string key=name+"_Skill"+skill,path="Assets/Art/Bosses/Models/"+key+".fbx";
 var importer=AssetImporter.GetAtPath(path) as ModelImporter;if(importer==null)throw new Exception("Missing Blender FBX "+path);
 importer.importCameras=false;importer.importLights=false;importer.addCollider=false;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
 importer.importAnimation=false;
 importer.SaveAndReimport();var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);var root=new GameObject(key);var visual=UnityEngine.Object.Instantiate(model,root.transform,false);
 try{foreach(var renderer in visual.GetComponentsInChildren<Renderer>()){var mats=renderer.sharedMaterials;for(int i=0;i<mats.Length;i++){var original=mats[i];string matPath="Assets/Art/Bosses/Materials/"+key+"_"+i+"_"+(original!=null?original.name:"Material")+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);if(mat==null){mat=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(mat,matPath);}mat.color=original!=null?original.color:Color.white;mat.SetFloat("_Glossiness",.1f);if(skill>0 && name!="Hashirama" && name!="Gaara"){mat.shader=Shader.Find("Vanguard/Chakra");mat.SetFloat("_Energy",2);mat.SetFloat("_Density",.9f);}if(name=="Mei" && skill==2){mat.shader=Shader.Find("Standard");mat.color=new Color(.5f,.85f,.65f,.07f);mat.SetFloat("_Mode",3);mat.SetInt("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.SrcAlpha);mat.SetInt("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);mat.SetInt("_ZWrite",0);mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");mat.renderQueue=3000;}EditorUtility.SetDirty(mat);mats[i]=mat;}renderer.sharedMaterials=mats;if(name=="Onoki" || name=="Raikage" || (name=="Mei" && skill==2))renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;}

 PrefabUtility.SaveAsPrefabAsset(root,"Assets/Resources/LumiBosses/"+key+".prefab");}finally{UnityEngine.Object.DestroyImmediate(root);}
 }AssetDatabase.SaveAssets();File.WriteAllText("Logs/BossAssetsQA/Import.txt","PASS: ten skill mesh prefabs; existing boss characters retained and Built-in materials.");}
}