using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LumiAdventure
{
    public interface ILumiDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(int amount, Vector3 hitPoint);
    }

    public class LumiHitFlash : MonoBehaviour
    {
        private Renderer[] renderers;
        private readonly List<Color> colors = new List<Color>();
        private Coroutine routine;
        private MaterialPropertyBlock tint;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
            tint = new MaterialPropertyBlock();
            foreach (var item in renderers)
            {
                colors.Add(item.sharedMaterial.HasProperty("_Color") ? item.sharedMaterial.color : Color.white);
            }
        }

        public void Flash(Color color)
        {
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(FlashRoutine(color));
        }

        private IEnumerator FlashRoutine(Color color)
        {
            for (int pulse = 0; pulse < 2; pulse++)
            {
                SetColor(color);
                yield return new WaitForSeconds(0.08f);
                Restore();
                yield return new WaitForSeconds(0.07f);
            }
            Restore();
            routine = null;
        }

        private void SetColor(Color color)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) { renderers[i].GetPropertyBlock(tint); tint.SetColor("_Color", color); renderers[i].SetPropertyBlock(tint); }
            }
        }

        private void Restore()
        {
            for (int i = 0; i < renderers.Length && i < colors.Count; i++)
            {
                if (renderers[i] != null) renderers[i].SetPropertyBlock(null);
            }
        }
    }

    public class LumiProjectile : MonoBehaviour
    {
        public LumiTransientPool Pool { get; set; }
        private Vector3 direction;
        private float speed;
        private float life;
        private int damage;
        private bool fromPlayer;
        private LumiGame game;
        private Transform shooter;
        private readonly RaycastHit[] traceHits=new RaycastHit[32];

        public void Initialize(LumiGame owner, Vector3 velocityDirection, float velocity, int hitDamage, bool playerShot,Transform source)
        {
            game = owner;
            direction = velocityDirection.normalized;
            speed = velocity;
            damage = hitDamage;
            fromPlayer = playerShot;
            shooter=source;
            life = 4f;
        }

        private void Update()
        {
            if (!game.IsPlaying) return;
            float distance = speed * Time.deltaTime;
            if (Trace(transform.position,direction,distance,out RaycastHit hit))
            {
                var damageable = FindDamageable(hit.collider);
                if (damageable != null)
                {
                    bool validTarget = (fromPlayer && damageable is LumiEnemy) || (!fromPlayer && damageable is LumiPlayer);
                    if (validTarget) damageable.TakeDamage(damage, hit.point);
                }
                game.SpawnImpact(hit.point, fromPlayer ? new Color(0.2f, 0.9f, 1f) : new Color(1f, 0.2f, 0.15f));
                Release();
                return;
            }

            transform.position += direction * distance;
            transform.Rotate(300f * Time.deltaTime, 420f * Time.deltaTime, 0f);
            life -= Time.deltaTime;
            if (life <= 0f) Release();
        }

        private void Release(){if(Pool!=null)Pool.Return(this);else Destroy(gameObject);}

        public bool Trace(Vector3 origin,Vector3 aim,float distance,out RaycastHit closest)
        {
            int count=Physics.SphereCastNonAlloc(origin,.13f,aim,traceHits,distance,~0,QueryTriggerInteraction.Ignore);
            RaycastHit[] hits=traceHits;
            if(count==traceHits.Length){hits=Physics.SphereCastAll(origin,.13f,aim,distance,~0,QueryTriggerInteraction.Ignore);count=hits.Length;}
            closest=default;float nearest=float.PositiveInfinity;
            for(int i=0;i<count;i++)
            {
                Collider collider=hits[i].collider;
                if(collider==null || (shooter!=null && collider.transform.IsChildOf(shooter)))continue;
                if(hits[i].distance>=nearest)continue;
                nearest=hits[i].distance;closest=hits[i];
            }
            return nearest<float.PositiveInfinity;
        }

        private static ILumiDamageable FindDamageable(Collider target)
        {
            var behaviours = target.GetComponentsInParent<MonoBehaviour>();
            foreach (var behaviour in behaviours)
            {
                if (behaviour is ILumiDamageable damageable) return damageable;
            }
            return null;
        }
    }
}

