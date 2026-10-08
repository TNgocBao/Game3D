using UnityEngine;

namespace LumiAdventure
{
    /// <summary>Shows four compact shield emblems around the player while armor remains.</summary>
    public sealed class LumiShieldFormation : MonoBehaviour
    {
        private readonly SpriteRenderer[] shields = new SpriteRenderer[4];
        private Camera viewCamera;

        public void Build()
        {
            Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
            for (int i = 0; i < shields.Length; i++)
            {
                GameObject item = new GameObject("Shield direction " + i, typeof(SpriteRenderer));
                item.transform.SetParent(transform, false);
                item.transform.localPosition = directions[i] * .78f;
                SpriteRenderer renderer = item.GetComponent<SpriteRenderer>();
                renderer.sprite = LumiIconLibrary.GetShieldEmblem();
                renderer.color = new Color(.75f, .94f, 1f, .84f);
                renderer.sortingOrder = 15;
                item.transform.localScale = Vector3.one * .46f;
                shields[i] = renderer;
            }
        }

        private void LateUpdate()
        {
            if (viewCamera == null) viewCamera = Camera.main;
            float pulse = .44f + Mathf.Sin(Time.time * 3.2f) * .025f;
            for (int i = 0; i < shields.Length; i++) if (shields[i] != null)
            {
                if (viewCamera != null) shields[i].transform.rotation = viewCamera.transform.rotation;
                shields[i].transform.localScale = Vector3.one * pulse;
            }
        }
    }
}
