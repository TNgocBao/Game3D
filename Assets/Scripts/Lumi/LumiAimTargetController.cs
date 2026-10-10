using System.Collections.Generic;
using UnityEngine;

namespace LumiAdventure
{
    /// <summary>Owns the 30 metre auto-target state independently from movement and attacks.</summary>
    public sealed class LumiAimTargetController : MonoBehaviour
    {
        public const float LockRange = 30f;
        private readonly List<Component> candidates = new List<Component>(32);
        private LumiGame game;
        private LumiPlayer player;
        private Component target;
        private LumiTargetMarker marker;
        private bool hadTarget;

        public bool AutoEnabled { get; private set; } = true;
        public bool HasTarget => IsValid(target);
        public Component Target => HasTarget ? target : null;
        public Vector3 TargetPoint => target is LumiEnemy enemy ? enemy.AimPoint : target is LumiVillageBoss boss ? boss.AimPoint : transform.position;
        public float TargetHealthFraction => target is LumiEnemy enemy ? enemy.HealthFraction : target is LumiVillageBoss boss ? boss.HealthFraction : 0f;

        public void Initialize(LumiGame owner, LumiPlayer controlledPlayer)
        {
            game = owner;
            player = controlledPlayer;
        }

        private void Update()
        {
            if (game == null || player == null || !game.IsPlaying) return;
            if (game.Controls.UsesPcControls)
            {
                if (Input.GetKeyDown(KeyCode.Q)) Cycle();
                if (Input.GetKeyDown(KeyCode.G)) Toggle();
            }
            if (!AutoEnabled) { ClearTarget(); return; }

            if (HasTarget)
            {
                if (DistanceTo(target) > LockRange)
                {
                    SetAutoEnabled(false, "MỤC TIÊU ĐÃ RA NGOÀI 30M");
                    return;
                }
                EnsureMarker();
                return;
            }

            Component nearest = FindNearest();
            if (nearest != null) Select(nearest);
            else if (hadTarget) SetAutoEnabled(false, "KHÔNG CÒN MỤC TIÊU TRONG 30M");
        }

        public void Cycle()
        {
            if (!AutoEnabled) SetAutoEnabled(true, null);
            CollectCandidates();
            if (candidates.Count == 0)
            {
                SetAutoEnabled(false, "KHÔNG CÓ MỤC TIÊU TRONG 30M");
                return;
            }
            candidates.Sort((a,b)=>DistanceTo(a).CompareTo(DistanceTo(b)));
            int index = target == null ? -1 : candidates.IndexOf(target);
            Select(candidates[(index + 1) % candidates.Count]);
            game.ShowToast("ĐÃ ĐỔI MỤC TIÊU", new Color(1f,.82f,.3f));
        }

        public void Toggle()
        {
            if (AutoEnabled) SetAutoEnabled(false, "ĐÃ TẮT TỰ CHỌN MỤC TIÊU");
            else
            {
                SetAutoEnabled(true, null);
                Component nearest = FindNearest();
                if (nearest == null) SetAutoEnabled(false, "KHÔNG CÓ MỤC TIÊU TRONG 30M");
                else { Select(nearest); game.ShowToast("ĐÃ BẬT TỰ CHỌN MỤC TIÊU", new Color(.35f,1f,.55f)); }
            }
        }

        public void SetAutoEnabled(bool enabled, string message)
        {
            AutoEnabled = enabled;
            if (!enabled) ClearTarget();
            if (!string.IsNullOrEmpty(message) && game != null) game.ShowToast(message, enabled ? new Color(.35f,1f,.55f) : new Color(1f,.62f,.25f));
        }

        private Component FindNearest()
        {
            CollectCandidates();
            Component nearest = null; float best = LockRange;
            foreach (Component candidate in candidates)
            {
                float distance = DistanceTo(candidate);
                if (distance <= best) { best = distance; nearest = candidate; }
            }
            return nearest;
        }

        private void CollectCandidates()
        {
            candidates.Clear();
            foreach (LumiEnemy enemy in LumiEnemy.Active)
                if (enemy != null && enemy.IsAlive && DistanceTo(enemy) <= LockRange) candidates.Add(enemy);
            if (game.VillageBoss != null && game.VillageBoss.IsAlive && DistanceTo(game.VillageBoss) <= LockRange) candidates.Add(game.VillageBoss);
        }

        private float DistanceTo(Component candidate)
        {
            Vector3 point = candidate is LumiEnemy enemy ? enemy.AimPoint : candidate is LumiVillageBoss boss ? boss.AimPoint : candidate.transform.position;
            point.y = transform.position.y;
            return Vector3.Distance(transform.position, point);
        }

        private bool IsValid(Component candidate)
        {
            if (candidate is LumiEnemy enemy) return enemy != null && enemy.IsAlive;
            if (candidate is LumiVillageBoss boss) return boss != null && boss.IsAlive;
            return false;
        }

        private void Select(Component candidate)
        {
            target = candidate; hadTarget = true; EnsureMarker();
        }

        private void EnsureMarker()
        {
            if (marker == null)
            {
                GameObject markerObject = new GameObject("Selected target arrow");
                markerObject.transform.SetParent(transform.parent, false);
                marker = markerObject.AddComponent<LumiTargetMarker>();
                marker.Initialize(this);
            }
        }

        private void ClearTarget()
        {
            target = null;
            if (marker != null) Destroy(marker.gameObject);
            marker = null;
        }
    }
}
