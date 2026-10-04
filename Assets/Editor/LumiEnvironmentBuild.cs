using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class LumiEnvironmentBuild
{
    private static readonly string[] Names={"tree_001","house2","house3","house4","house6","House_01_full","House_04_full"};
    static LumiEnvironmentBuild(){EditorApplication.delayCall+=AutoPrepare;}
    private static void AutoPrepare()
    {
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.delayCall+=AutoPrepare;return;}
        if(File.Exists("Assets/Art/Environment/ImportManifest.json") && !File.Exists("Assets/Resources/LumiEnvironment/tree_001.prefab"))Prepare();
    }
    [MenuItem("Naruto/Prepare environment assets")]
    public static void Prepare()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before preparing environment assets.");
        Directory.CreateDirectory("Assets/Resources/LumiEnvironment");Directory.CreateDirectory("Assets/Art/Environment/Materials");AssetDatabase.Refresh();
        Shader wind=Shader.Find("Lumi/Environment Tree Wind");if(wind==null)throw new InvalidOperationException("Tree wind shader did not import.");
        foreach(string name in Names)
        {
            bool tree=name=="tree_001";string pack=name.StartsWith("House_")?"Medieval":"Styloo";
            string source="Assets/Art/Environment/"+pack+"/Models/"+name+".obj";
            ModelImporter importer=AssetImporter.GetAtPath(source) as ModelImporter;
            if(importer==null)throw new InvalidOperationException("Model did not import: "+source);
            importer.importNormals=ModelImporterNormals.Calculate;importer.importCameras=false;importer.importLights=false;importer.addCollider=false;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.SaveAndReimport();
            GameObject model=AssetDatabase.LoadAssetAtPath<GameObject>(source);
            GameObject root=new GameObject(name);GameObject visual=UnityEngine.Object.Instantiate(model,root.transform,false);visual.name="Visual";
            try
            {
                Renderer[] renderers=visual.GetComponentsInChildren<Renderer>();if(renderers.Length==0)throw new InvalidOperationException("No mesh: "+name);
                Bounds bounds=renderers[0].bounds;foreach(Renderer r in renderers)bounds.Encapsulate(r.bounds);
                float factor=1/Mathf.Max(bounds.size.y,.001f);visual.transform.localScale=Vector3.one*factor;
                visual.transform.localPosition=-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z)*factor;
                foreach(Renderer r in renderers)
                {
                    Material[] mats=r.sharedMaterials;
                    for(int i=0;i<mats.Length;i++)
                    {
                        string materialName=mats[i]!=null?mats[i].name:name+"_"+i;
                        string path="Assets/Art/Environment/Materials/"+materialName+".mat";
                        Material mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                        if(mat==null){mat=new Material(tree?wind:Shader.Find("Standard"));AssetDatabase.CreateAsset(mat,path);}
                        mat.shader=tree?wind:Shader.Find("Standard");mat.color=mats[i]!=null?mats[i].color:Color.white;
                        mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Environment/"+pack+"/Models/"+materialName+".png");
                        if(mat.HasProperty("_Glossiness"))mat.SetFloat("_Glossiness",.08f);
                        EditorUtility.SetDirty(mat);mats[i]=mat;
                    }
                    r.sharedMaterials=mats;
                }
                if(tree)
                {
                    CapsuleCollider collider=root.AddComponent<CapsuleCollider>();collider.center=new Vector3(0,.24f,0);collider.height=.48f;collider.radius=.055f;
                }
                else
                {
                    BoxCollider collider=root.AddComponent<BoxCollider>();collider.center=new Vector3(0,.46f,0);collider.size=new Vector3(bounds.size.x*factor*.92f,.92f,bounds.size.z*factor*.92f);
                }
                PrefabUtility.SaveAsPrefabAsset(root,"Assets/Resources/LumiEnvironment/"+name+".prefab");
            }
            finally{UnityEngine.Object.DestroyImmediate(root);}
        }
        AssetDatabase.SaveAssets();Directory.CreateDirectory("Logs/EnvironmentQA");File.WriteAllText("Logs/EnvironmentQA/Import.txt","PASS: seven environment prefabs prepared; Built-in materials and tree wind shader assigned.");
        Debug.Log("Environment: imported houses and wind tree are ready.");
    }
}
