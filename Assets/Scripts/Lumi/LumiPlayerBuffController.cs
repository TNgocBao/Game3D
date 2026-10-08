using System.Collections.Generic;
using UnityEngine;

namespace LumiAdventure
{
    /// <summary>Owns timed player modifiers. Gameplay systems only read the resulting multipliers.</summary>
    public sealed class LumiPlayerBuffController : MonoBehaviour
    {
        private readonly float[] expiresAt = new float[4];
        private readonly float[] magnitudes = { 1f, 1f, 1f, 1f };

        public float AttackMultiplier => Multiplier(LumiBuffType.Attack);
        public float SpeedMultiplier => Multiplier(LumiBuffType.Speed);
        public float DamageTakenMultiplier => IsActive(LumiBuffType.Defense) ? .5f : 1f;

        public void Apply(LumiBuffType type, float durationSeconds, float multiplier)
        {
            int index = (int)type;
            expiresAt[index] = Mathf.Max(expiresAt[index], Time.time + Mathf.Max(0f, durationSeconds));
            magnitudes[index] = Mathf.Max(magnitudes[index], multiplier);
        }

        public bool IsActive(LumiBuffType type) => expiresAt[(int)type] > Time.time;
        public float Remaining(LumiBuffType type) => Mathf.Max(0f, expiresAt[(int)type] - Time.time);

        public void CollectActive(List<LumiActiveBuff> output)
        {
            output.Clear();
            for (int i = 0; i < expiresAt.Length; i++)
            {
                LumiBuffType type = (LumiBuffType)i;
                if (IsActive(type)) output.Add(new LumiActiveBuff(type, Remaining(type), magnitudes[i]));
            }
        }

        private float Multiplier(LumiBuffType type) => IsActive(type) ? magnitudes[(int)type] : 1f;

        private void Update()
        {
            for (int i = 0; i < expiresAt.Length; i++)
                if (expiresAt[i] > 0f && expiresAt[i] <= Time.time)
                {
                    expiresAt[i] = 0f;
                    magnitudes[i] = 1f;
                }
        }
    }
}
