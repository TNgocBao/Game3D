using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LumiAdventure;
using UnityEditor;
using UnityEngine;

public static class LumiShippudenNarutoBuild
{
    private const string Source="Assets/Art/NarutoShippuden/source/Naruto-Shippuden.fbx";
    private const string Prefab="Assets/Resources/NarutoChibi/Naruto.prefab";

    [MenuItem("Naruto/Build Shippuden character rig")]
    public static void Build()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before rebuilding Naruto.");
        if(!File.Exists(Source))throw new FileNotFoundException("Shippuden FBX was not copied into the project.",Source);
        var importer=AssetImporter.GetAtPath(Source) as ModelImporter;
        if(importer==null)throw new InvalidOperationException("Unity did not recognize the Naruto FBX.");
        importer.animationType=ModelImporterAnimationType.Generic;
        importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
        importer.importAnimation=true;
        importer.importBlendShapes=true;
        importer.importCameras=false;
        importer.importLights=false;
        importer.optimizeGameObjects=false;
        importer.importNormals=ModelImporterNormals.Import;
        importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
        importer.SaveAndReimport();
        var model=AssetDatabase.LoadAssetAtPath<GameObject>(Source);
        if(model==null)throw new InvalidOperationException("Unity could not load the Naruto model.");
        var instance=PrefabUtility.InstantiatePrefab(model) as GameObject;
        if(instance==null)throw new InvalidOperationException("Unity could not instantiate the FBX skeleton.");
        var armature=instance.GetComponentInChildren<Animator>(true);
        if(armature!=null)armature.enabled=false;
        var renderers=instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        if(renderers.Length==0)throw new InvalidOperationException("The FBX has no skinned mesh.");
        int vertexCount=renderers.Sum(r=>r.sharedMesh!=null?r.sharedMesh.vertexCount:0);
        var bones=renderers.SelectMany(r=>r.bones).Where(b=>b!=null).Distinct().ToArray();
        if(bones.Length<100 || vertexCount<10000)throw new InvalidOperationException("The imported rig or mesh is incomplete.");

        var textureMap=new Dictionary<string,Texture2D>(StringComparer.OrdinalIgnoreCase)
        {
            ["body"]=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/NarutoShippuden/textures/Body.png"),
            ["eye"]=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/NarutoShippuden/textures/Eye.png"),
            ["headband"]=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/NarutoShippuden/textures/Headband1.png"),
            ["jacket"]=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/NarutoShippuden/textures/jacket.png"),
            ["wep"]=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/NarutoShippuden/textures/Weapon.png")
        };
        var root=new GameObject("Naruto Shippuden");root.layer=30;
        instance.transform.SetParent(root.transform,false);
        foreach(Transform child in root.GetComponentsInChildren<Transform>(true))child.gameObject.layer=30;
        var bodyRenderer=renderers[0];
        var bounds=bodyRenderer.bounds;
        for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);
        if(bounds.size.y<1 || bounds.size.y>4)throw new InvalidOperationException("Unexpected Shippuden character height: "+bounds.size.y);
        instance.transform.localPosition+=Vector3.up*(-bounds.min.y);
        foreach(var renderer in renderers)
        {
            var materials=renderer.sharedMaterials;
            for(int i=0;i<materials.Length;i++)
            {
                var original=materials[i];string name=original!=null?original.name.ToLowerInvariant():string.Empty;
                var mat=new Material(Shader.Find("Standard")){name="Shippuden "+(original!=null?original.name:"Material "+i)};
                mat.SetFloat("_Glossiness",name.Contains("eye")?.64f:name.Contains("wep")?.48f:.34f);
                Texture2D texture=null;
                foreach(var pair in textureMap)if(name.Contains(pair.Key) && pair.Value!=null){texture=pair.Value;break;}
                if(name.Contains("sphere"))
                {
                    mat.mainTexture=null;mat.color=new Color(.075f,.085f,.11f);
                    mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",new Color(.012f,.019f,.032f));
                }
                else
                {
                    if(texture==null)texture=textureMap["body"];
                    mat.mainTexture=texture;mat.color=Color.white;
                }
                string path="Assets/Resources/NarutoChibi/Shippuden-Material-"+materials.Length+"-"+i+".mat";
                var saved=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(saved==null){AssetDatabase.CreateAsset(mat,path);saved=mat;}
                else{EditorUtility.CopySerialized(mat,saved);UnityEngine.Object.DestroyImmediate(mat);}
                materials[i]=saved;
            }
            renderer.sharedMaterials=materials;
            RemoveStaveSubmeshes(renderer);UnifyGoldenEyes(renderer);
            renderer.updateWhenOffscreen=true;
            var localBounds=renderer.localBounds;localBounds.Expand(new Vector3(.25f,.25f,.25f));renderer.localBounds=localBounds;
        }
        root.AddComponent<LumiInfantryMotion>();
        var head=FindBone(root.transform,"head head","head head extra");
        if(head==null)throw new InvalidOperationException("The rig is missing its head bone.");
        var socket=new GameObject("Eye camera socket").transform;socket.SetParent(root.transform,false);
        socket.localPosition=root.transform.InverseTransformPoint(head.position)+Vector3.up*.10f;
        
        Directory.CreateDirectory("Assets/Resources/NarutoChibi");
        PrefabUtility.SaveAsPrefabAsset(root,Prefab);
        AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        File.WriteAllText("Logs/NarutoQA/ShippudenImport.txt","PASS: "+vertexCount+" skinned vertices, "+bones.Length+" source bones, "+renderers.Length+" skinned renderers, "+materialsSummary(renderers)+"; model height "+bounds.size.y.ToString("F2")+" m; head camera socket set.");
        UnityEngine.Object.DestroyImmediate(root);
        Debug.Log("Built Shippuden Naruto rig from source FBX: "+vertexCount+" vertices, "+bones.Length+" bones.");
    }

    private static Transform FindBone(Transform root,params string[] names)
    {
        foreach(var transform in root.GetComponentsInChildren<Transform>(true))
            foreach(var name in names)if(string.Equals(transform.name,name,StringComparison.OrdinalIgnoreCase))return transform;
        return null;
    }
    private static string materialsSummary(SkinnedMeshRenderer[] renderers)=>renderers.Sum(r=>r.sharedMaterials.Length)+" material slots";
    [MenuItem("Naruto/Remove hand staves from current prefab")]
    public static void RemoveHandStaves()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        GameObject root=PrefabUtility.LoadPrefabContents(Prefab);
        try
        {
            foreach(SkinnedMeshRenderer renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true)){RemoveStaveSubmeshes(renderer);UnifyGoldenEyes(renderer);}
            PrefabUtility.SaveAsPrefabAsset(root,Prefab);AssetDatabase.SaveAssets();
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }
    private static void UnifyGoldenEyes(SkinnedMeshRenderer renderer)
    {
        var materials=renderer.sharedMaterials;int left=Array.FindIndex(materials,m=>m!=null&&m.name.ToLowerInvariant().Contains("eyel")),right=Array.FindLastIndex(materials,m=>m!=null&&m.name.ToLowerInvariant().Contains("eye"));if(left<0||right<0||left==right||renderer.sharedMesh==null)return;
        const string meshPath="Assets/Art/NarutoShippuden/Derived/Naruto-GoldenEyes.asset";var current=renderer.sharedMesh;
        var sourceMesh=AssetDatabase.LoadAllAssetsAtPath(Source).OfType<Mesh>().FirstOrDefault();

        if(sourceMesh!=null){var mesh=UnityEngine.Object.Instantiate(sourceMesh);for(int slot=0;slot<materials.Length;slot++)if(materials[slot]!=null && (materials[slot].name.Contains("wep01")||materials[slot].name.Contains("wep02")))mesh.SetTriangles(new int[0],slot,false);mesh.name="Naruto without staves, matching golden eyes";var vertices=mesh.vertices;var uv=mesh.uv;int[] l=mesh.GetTriangles(left).Distinct().ToArray(),r=mesh.GetTriangles(right).Distinct().ToArray();foreach(int[] eyeVertices in new[]{l,r}){float minX=eyeVertices.Min(i=>vertices[i].x),maxX=eyeVertices.Max(i=>vertices[i].x),minZ=eyeVertices.Min(i=>vertices[i].z),maxZ=eyeVertices.Max(i=>vertices[i].z);foreach(int i in eyeVertices)uv[i]=new Vector2(.5f+((vertices[i].x-minX)/(maxX-minX)-.5f)*.38f,.5f+((vertices[i].z-minZ)/(maxZ-minZ)-.5f)*.38f);}mesh.uv=uv;var saved=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);if(saved==null){AssetDatabase.CreateAsset(mesh,meshPath);saved=mesh;}else{EditorUtility.CopySerialized(mesh,saved);UnityEngine.Object.DestroyImmediate(mesh);}renderer.sharedMesh=saved;
        }
        var eye=materials[left];eye.shader=Shader.Find("Lumi/Golden Eyes");eye.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/NarutoShippuden/textures/Eye.png");eye.color=Color.white;EditorUtility.SetDirty(eye);materials[right]=eye;renderer.sharedMaterials=materials;
    }
    private static void RemoveStaveSubmeshes(SkinnedMeshRenderer renderer)
    {
        if(renderer.sharedMesh==null)return;
        int[] slots=renderer.sharedMaterials.Select((m,i)=>new{m,i}).Where(x=>x.m!=null && (x.m.name.Contains("wep01") || x.m.name.Contains("wep02"))).Select(x=>x.i).ToArray();
        if(slots.Length==0)return;
        const string path="Assets/Art/NarutoShippuden/Derived/Naruto-NoHandStaves.asset";
        Mesh mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(renderer.sharedMesh==mesh)return;
        ModelImporter importer=AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(renderer.sharedMesh)) as ModelImporter;
        if(importer!=null && !importer.isReadable){importer.isReadable=true;importer.SaveAndReimport();}
        Mesh derivative=UnityEngine.Object.Instantiate(renderer.sharedMesh);derivative.name="Naruto rig without hand staves";
        foreach(int slot in slots)if(slot<derivative.subMeshCount)derivative.SetTriangles(new int[0],slot,false);
        Directory.CreateDirectory("Assets/Art/NarutoShippuden/Derived");AssetDatabase.Refresh();
        if(mesh==null){AssetDatabase.CreateAsset(derivative,path);mesh=derivative;}
        else{EditorUtility.CopySerialized(derivative,mesh);UnityEngine.Object.DestroyImmediate(derivative);}
        renderer.sharedMesh=mesh;
    }
}

