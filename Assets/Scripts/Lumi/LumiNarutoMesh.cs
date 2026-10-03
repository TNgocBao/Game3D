using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LumiAdventure
{
    public static class LumiNarutoMesh
    {
        public static GameObject Create(Transform parent)
        {
            GameObject asset=Resources.Load<GameObject>("NarutoModel/Naruto");if(asset==null)return null;
            GameObject root=new GameObject("Naruto chibi • textured skeletal mesh");root.transform.SetParent(parent,false);
            GameObject model=Object.Instantiate(asset,root.transform,false);model.name="Naruto imported rig";
            foreach(Animator animator in model.GetComponentsInChildren<Animator>())animator.enabled=false;
            var skins=model.GetComponentsInChildren<SkinnedMeshRenderer>();
            Transform head=model.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name.EndsWith("_Head"));
            if(head==null || skins.Length==0){Object.Destroy(root);return null;}
            Transform[] bones=skins.SelectMany(s=>s.bones).Where(b=>b!=null).Distinct().OrderBy(Depth).ToArray();
            var positions=new Dictionary<Transform,Vector3>();var rotations=new Dictionary<Transform,Quaternion>();var scales=new Dictionary<Transform,Vector3>();
            foreach(Transform b in bones){positions[b]=b.position-root.transform.position;rotations[b]=b.rotation;scales[b]=b.lossyScale;}
            Vector3 headOrigin=head.position-root.transform.position;
            Vector3 newHead=new Vector3(headOrigin.x*1.08f,headOrigin.y*.62f,headOrigin.z*1.08f);
            foreach(Transform b in bones)
            {
                bool cranial=b==head || b.IsChildOf(head);Vector3 point=positions[b];
                Vector3 newPoint=cranial?newHead+(point-headOrigin)*1.72f:new Vector3(point.x*1.08f,point.y*.62f,point.z*1.08f);
                b.position=root.transform.position+newPoint;b.rotation=rotations[b];
                Vector3 desired=scales[b]*(cranial?1.72f:.94f),parentScale=b.parent!=null?b.parent.lossyScale:Vector3.one;
                b.localScale=new Vector3(desired.x/parentScale.x,desired.y/parentScale.y,desired.z/parentScale.z);
            }
            foreach(SkinnedMeshRenderer skin in skins)
            {
                Mesh source=skin.sharedMesh;Mesh chibi=new Mesh{name="Naruto chibi sculpt • "+source.name};
                skin.BakeMesh(chibi);chibi.boneWeights=source.boneWeights;
                Matrix4x4[] bind=new Matrix4x4[skin.bones.Length];for(int i=0;i<bind.Length;i++)bind[i]=skin.bones[i].worldToLocalMatrix*skin.transform.localToWorldMatrix;
                chibi.bindposes=bind;chibi.RecalculateBounds();skin.sharedMesh=chibi;skin.localBounds=chibi.bounds;skin.updateWhenOffscreen=true;
                Material[] materials=skin.sharedMaterials;
                for(int i=0;i<materials.Length;i++)
                {
                    Material copy=new Material(Shader.Find("Standard")){name="Naruto painted • "+materials[i].name};copy.color=Color.white;
                    copy.mainTexture=materials[i].mainTexture ?? Resources.Load<Texture2D>("NarutoModel/ntxr000");copy.SetFloat("_Glossiness",.12f);copy.SetFloat("_Metallic",0);materials[i]=copy;
                }
                skin.sharedMaterials=materials;
            }
            foreach(Transform bone in bones)
                if(bone.name.EndsWith("_LeftArm") || bone.name.EndsWith("_RightArm"))
                    bone.rotation=Quaternion.AngleAxis(-Mathf.Sign(bone.position.x-root.transform.position.x)*68,root.transform.forward)*bone.rotation;
            Bounds bounds=skins[0].bounds;foreach(var skin in skins)bounds.Encapsulate(skin.bounds);
            float height=bounds.size.y;
            if(height>.01f){float scale=1.95f/height;model.transform.localScale*=scale;model.transform.localPosition+=Vector3.up*((root.transform.position.y-bounds.min.y)*scale);}
            root.AddComponent<LumiInfantryMotion>();return root;
        }
        private static int Depth(Transform t){int count=0;while(t.parent!=null){count++;t=t.parent;}return count;}
    }
}
