using UnityEngine;

namespace LumiAdventure
{
    // Shared imported prefabs; only visuals sway, collision stays rooted to the ground.
    public static class LumiEnvironmentAssets
    {
        private static readonly string[] HouseNames={"house2","house3","house4","house6","House_01_full","House_04_full"};
        private static readonly GameObject[] Houses=new GameObject[6];
        private static GameObject tree;

        public static GameObject House(Transform parent,Vector3 position,int level,float width,float height)
        {
            int variant=Mathf.Abs(Mathf.RoundToInt(position.x*3+position.z*7)+level)%HouseNames.Length;
            if(Houses[variant]==null)Houses[variant]=Resources.Load<GameObject>("LumiEnvironment/"+HouseNames[variant]);
            if(Houses[variant]==null)throw new System.InvalidOperationException("Missing environment prefab: "+HouseNames[variant]+". Run Naruto/Prepare environment assets.");
            GameObject root=Object.Instantiate(Houses[variant],parent,false);
            root.name="Village house "+level+" - "+HouseNames[variant];root.transform.localPosition=position;
            Vector3 size=root.GetComponent<BoxCollider>().size;
            // Uniform scale protects the supplied house proportions and existing route clearance.
            float scale=Mathf.Min(height,width/(Mathf.Max(size.x,size.z)/.92f));
            root.transform.localScale=Vector3.one*scale;
            root.transform.localRotation=Quaternion.Euler(0,position.x<0?90:-90,0);
            return root;
        }

        public static GameObject Tree(Transform parent,Vector3 position,float height)
        {
            if(tree==null)tree=Resources.Load<GameObject>("LumiEnvironment/tree_001");
            if(tree==null)throw new System.InvalidOperationException("Missing tree prefab. Run Naruto/Prepare environment assets.");
            GameObject root=Object.Instantiate(tree,parent,false);root.name="Styloo wind tree";
            root.transform.localPosition=position;root.transform.localScale=Vector3.one*height;
            root.transform.localRotation=Quaternion.Euler(0,Mathf.Repeat(position.x*31+position.z*17,360),0);
            return root;
        }
    }
}
