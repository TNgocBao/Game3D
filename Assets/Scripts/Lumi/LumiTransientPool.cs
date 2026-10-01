using System.Collections.Generic;
using UnityEngine;

namespace LumiAdventure
{
    public sealed class LumiTransientPool:MonoBehaviour
    {
        private readonly Queue<LumiProjectile> projectiles=new Queue<LumiProjectile>();
        private readonly Queue<LumiPooledImpact> impacts=new Queue<LumiPooledImpact>();
        public LumiProjectile RentProjectile(Material material,float size)
        {
            LumiProjectile projectile;
            if(projectiles.Count>0)projectile=projectiles.Dequeue();
            else
            {
                GameObject obj=LumiFactory.Primitive("Pooled projectile",PrimitiveType.Sphere,transform,Vector3.zero,Vector3.one,material,false);
                projectile=obj.AddComponent<LumiProjectile>();projectile.Pool=this;
            }
            projectile.transform.localScale=Vector3.one*size;
            projectile.GetComponent<Renderer>().sharedMaterial=material;
            projectile.gameObject.SetActive(true);
            return projectile;
        }
        public void Return(LumiProjectile projectile){projectile.gameObject.SetActive(false);projectiles.Enqueue(projectile);}
        public void Impact(Vector3 position,Color color)
        {
            LumiPooledImpact effect;
            if(impacts.Count>0)effect=impacts.Dequeue();
            else
            {
                var obj=new GameObject("Pooled impact");obj.transform.SetParent(transform,false);
                effect=obj.AddComponent<LumiPooledImpact>();effect.Initialize(this);
            }
            effect.Emit(position,color);
        }
        public void Return(LumiPooledImpact effect){effect.gameObject.SetActive(false);impacts.Enqueue(effect);}
    }

    public sealed class LumiPooledImpact:MonoBehaviour
    {
        private ParticleSystem particles;
        private LumiTransientPool pool;
        private float ends;
        public void Initialize(LumiTransientPool owner)
        {
            pool=owner;particles=gameObject.AddComponent<ParticleSystem>();particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=particles.main;main.loop=false;main.duration=.3f;main.startLifetime=.28f;main.startSpeed=3.2f;main.startSize=.12f;main.maxParticles=18;
            var emission=particles.emission;emission.enabled=false;
            var shape=particles.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.12f;
            particles.GetComponent<ParticleSystemRenderer>().sharedMaterial=LumiFactory.Material("Pooled particles",Color.white,true,true);
        }
        public void Emit(Vector3 position,Color color)
        {
            transform.position=position;gameObject.SetActive(true);
            particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=particles.main;main.startColor=color;particles.Play();particles.Emit(12);ends=Time.time+.55f;
        }
        private void Update(){if(Time.time>=ends)pool.Return(this);}
    }
}
