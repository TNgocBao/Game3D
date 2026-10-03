using UnityEngine;

namespace LumiAdventure
{
    public static class LumiShurikenArt
    {
        private static Mesh mesh;
        public static GameObject Create(Transform parent)
        {
            GameObject obj=new GameObject("Thrown steel shuriken",typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(parent,false);
            if(mesh==null)
            {
                Vector3[] vertices=new Vector3[24];int[] triangles=new int[48];int v=0,t=0;
                for(int i=0;i<4;i++)
                {
                    float a=i*Mathf.PI*.5f;Vector3 d=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0),s=new Vector3(-d.y,d.x,0);
                    vertices[v]=d*.06f-s*.085f;vertices[v+1]=d*.27f+s*.035f;vertices[v+2]=d*.06f+s*.085f;
                    vertices[v+3]=vertices[v]+Vector3.forward*.025f;vertices[v+4]=vertices[v+1]+Vector3.forward*.025f;vertices[v+5]=vertices[v+2]+Vector3.forward*.025f;
                    int[] face={0,1,2,5,4,3,0,3,1,1,3,4};foreach(int index in face)triangles[t++]=v+index;v+=6;
                }
                mesh=new Mesh{name="Four pointed ninja throwing star",vertices=vertices,triangles=triangles};mesh.RecalculateNormals();
            }
            obj.GetComponent<MeshFilter>().sharedMesh=mesh;obj.GetComponent<MeshRenderer>().sharedMaterial=LumiFactory.Material("Shuriken steel",new Color(.63f,.7f,.79f));return obj;
        }
    }
}
