using UnityEngine;

namespace LumiAdventure
{
    /// <summary>Small red world-space arrow that follows the selected enemy.</summary>
    public sealed class LumiTargetMarker : MonoBehaviour
    {
        private LumiAimTargetController targeting;
        private Camera viewCamera;
        private Transform healthFill;

        public void Initialize(LumiAimTargetController owner)
        {
            targeting = owner;
            Mesh mesh = new Mesh { name = "Target arrow mesh" };
            mesh.vertices = new[]
            {
                new Vector3(-.34f,.62f,0), new Vector3(.34f,.62f,0), new Vector3(0,0,0),
                new Vector3(-.1f,1f,0), new Vector3(.1f,1f,0), new Vector3(.1f,.55f,0), new Vector3(-.1f,.55f,0)
            };
            mesh.triangles = new[] { 0,1,2, 2,1,0, 3,4,5, 3,5,6, 5,4,3, 6,5,3 };
            mesh.RecalculateBounds();
            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            gameObject.AddComponent<MeshRenderer>().sharedMaterial = LumiFactory.Material("Selected target red",new Color(1f,.06f,.03f),true);
            CreateHealthBar();
        }

        private void CreateHealthBar()
        {
            GameObject track=new GameObject("Selected target health track",typeof(MeshFilter),typeof(MeshRenderer));track.transform.SetParent(transform,false);
            track.GetComponent<MeshFilter>().sharedMesh=Quad(-.72f,.72f,1.12f,1.30f);
            track.GetComponent<MeshRenderer>().sharedMaterial=LumiFactory.Material("Selected target health background",new Color(.035f,.025f,.03f,.92f),true);
            GameObject fill=new GameObject("Selected target health fill",typeof(MeshFilter),typeof(MeshRenderer));fill.transform.SetParent(transform,false);
            fill.GetComponent<MeshFilter>().sharedMesh=Quad(0,1.34f,1.15f,1.27f);
            fill.GetComponent<MeshRenderer>().sharedMaterial=LumiFactory.Material("Selected target health red",new Color(.96f,.08f,.08f),true);
            healthFill=fill.transform;healthFill.localPosition=new Vector3(-.67f,0,-.01f);
        }

        private static Mesh Quad(float left,float right,float bottom,float top)
        {
            Mesh mesh=new Mesh{name="Target health quad"};
            mesh.vertices=new[]{new Vector3(left,bottom,0),new Vector3(right,bottom,0),new Vector3(right,top,0),new Vector3(left,top,0)};
            mesh.triangles=new[]{0,2,1,0,3,2,1,2,0,2,3,0};mesh.RecalculateBounds();return mesh;
        }

        private void LateUpdate()
        {
            if (targeting == null || !targeting.HasTarget) { Destroy(gameObject); return; }
            if (viewCamera == null) viewCamera = Camera.main;
            transform.position = targeting.TargetPoint + Vector3.up * (.8f + Mathf.Sin(Time.time * 5f) * .08f);
            if (viewCamera != null) transform.rotation = Quaternion.LookRotation(transform.position - viewCamera.transform.position, Vector3.up);
            if(healthFill!=null)healthFill.localScale=new Vector3(Mathf.Clamp01(targeting.TargetHealthFraction),1,1);
        }
    }
}
