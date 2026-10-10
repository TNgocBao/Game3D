using UnityEngine;

namespace LumiAdventure
{
    /// <summary>Shows four compact shield emblems around the player while armor remains.</summary>
    public sealed class LumiShieldFormation : MonoBehaviour
    {
        private readonly MeshRenderer[] shields = new MeshRenderer[4];
        private Camera viewCamera;
        private static Mesh shieldMesh;
        private static Material shieldMaterial;

        public void Build()
        {
            Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
            for (int i = 0; i < shields.Length; i++)
            {
                GameObject item = new GameObject("Shield direction " + i, typeof(MeshFilter), typeof(MeshRenderer));
                item.transform.SetParent(transform, false);
                item.transform.localPosition = directions[i] * .78f;
                item.transform.localScale = Vector3.one * .46f;
                item.GetComponent<MeshFilter>().sharedMesh = ShieldMesh();
                MeshRenderer renderer = item.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = ShieldMaterial();
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                shields[i] = renderer;
            }
        }

        private void LateUpdate()
        {
            if (viewCamera == null) viewCamera = Camera.main;
            float pulse = .44f + Mathf.Sin(Time.time * 3.2f) * .025f;
            for (int i = 0; i < shields.Length; i++) if (shields[i] != null)
            {
                MeshFilter filter = shields[i].GetComponent<MeshFilter>();
                if (filter.sharedMesh == null) filter.sharedMesh = ShieldMesh();
                if(shields[i].sharedMaterial==null||shields[i].sharedMaterial.shader==null)shields[i].sharedMaterial=ShieldMaterial();
                if (viewCamera != null) shields[i].transform.rotation = viewCamera.transform.rotation;
                shields[i].transform.localScale = Vector3.one * pulse;
            }
        }

        private static Mesh ShieldMesh()
        {
            if (shieldMesh != null) return shieldMesh;
            Vector3[] vertices =
            {
                new Vector3(-.48f,.42f,0), new Vector3(0,.58f,0), new Vector3(.48f,.42f,0),
                new Vector3(.39f,-.08f,0), new Vector3(0,-.58f,0), new Vector3(-.39f,-.08f,0)
            };
            int[] front = { 0,1,5, 1,4,5, 1,2,4, 2,3,4 };
            int[] triangles = new int[front.Length * 2];
            for (int i = 0; i < front.Length; i++)
            {
                triangles[i] = front[i];
                triangles[front.Length + i] = front[front.Length - 1 - i];
            }
            shieldMesh = new Mesh { name = "Procedural armor shield" };
            shieldMesh.vertices = vertices;
            shieldMesh.triangles = triangles;
            shieldMesh.RecalculateNormals();
            shieldMesh.RecalculateBounds();
            return shieldMesh;
        }

        private static Material ShieldMaterial()
        {
            if(shieldMaterial==null)shieldMaterial=new Material(Shader.Find("Sprites/Default")){name="Stable armor shield mesh",color=new Color(.55f,.88f,1f,.9f)};
            return shieldMaterial;
        }
    }
}
