using UnityEngine;

namespace LumiAdventure
{
    public class LumiDirectionArrow : MonoBehaviour
    {
        private Transform player;
        private Transform goal;
        private Renderer[] renderers;

        public void Initialize(Transform targetPlayer, Transform targetGoal)
        {
            player = targetPlayer;
            goal = targetGoal;
            Material yellow = LumiFactory.Material("DirectionArrow", new Color(1f, 0.8f, 0.05f, 0.65f), true, true);
            LumiFactory.Primitive("Shaft", PrimitiveType.Cube, transform, Vector3.zero, new Vector3(0.22f, 0.12f, 1.1f), yellow, false);
            var left = LumiFactory.Primitive("Arrow L", PrimitiveType.Cube, transform, new Vector3(-0.23f, 0f, 0.62f), new Vector3(0.18f, 0.12f, 0.65f), yellow, false);
            left.transform.localRotation = Quaternion.Euler(0f, -38f, 0f);
            var right = LumiFactory.Primitive("Arrow R", PrimitiveType.Cube, transform, new Vector3(0.23f, 0f, 0.62f), new Vector3(0.18f, 0.12f, 0.65f), yellow, false);
            right.transform.localRotation = Quaternion.Euler(0f, 38f, 0f);
            renderers = GetComponentsInChildren<Renderer>();
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (player == null || goal == null) return;
            transform.position = player.position + Vector3.up * (2.8f + Mathf.Sin(Time.time * 3f) * 0.18f);
            Vector3 direction = goal.position - player.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.1f) transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            float alpha = 0.35f + Mathf.PingPong(Time.time * 1.2f, 0.45f);
            foreach (var item in renderers)
            {
                Color color = item.material.color;
                color.a = alpha;
                item.material.color = color;
            }
        }
    }
}
