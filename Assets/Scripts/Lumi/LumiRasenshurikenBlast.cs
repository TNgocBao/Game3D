using System.Collections.Generic;
using UnityEngine;

namespace LumiAdventure
{
    // Three bounded damage pulses share one total damage budget, with per-target deduplication.
    public sealed class LumiRasenshurikenBlast:MonoBehaviour
    {
        private LumiGame game;
        private LumiNarutoSkills caster;
        private Transform sphere;
        private ParticleSystem particles;
        private Renderer[] renderers;
        private LineRenderer[] ribbons;
        private MaterialPropertyBlock energy;
        private readonly HashSet<LumiEnemy> victims=new HashSet<LumiEnemy>();
        private readonly HashSet<LumiVillageBoss> bosses=new HashSet<LumiVillageBoss>();
        private float elapsed,radius,duration;
        private int totalDamage,pulses;
        public static readonly List<LumiRasenshurikenBlast> Active=new List<LumiRasenshurikenBlast>();
        private void OnEnable(){Active.Add(this);}
        private void OnDisable(){Active.Remove(this);}
        public int PulsesApplied=>pulses;
        public float DamageRadius=>radius;

        public void Initialize(LumiGame owner,LumiNarutoSkills source,float blastRadius,int damage,float seconds)
        {
            game=owner;caster=source;radius=Mathf.Max(.5f,blastRadius);totalDamage=Mathf.Max(0,damage);duration=Mathf.Max(.9f,seconds);
            sphere=LumiChakraVisual.Vortex(transform,1).transform;sphere.localScale=Vector3.one*.15f;
            particles=LumiChakraVisual.WindParticles(transform,radius);renderers=sphere.GetComponentsInChildren<Renderer>();ribbons=sphere.GetComponentsInChildren<LineRenderer>();energy=new MaterialPropertyBlock();
        }
        private void Update()
        {
            if(game==null || caster==null){Destroy(gameObject);return;}
            if(!game.IsPlaying)return;
            elapsed+=Time.deltaTime;
            float expand=Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/.22f));
            float fade=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(duration*.65f,duration,elapsed));
            sphere.localScale=Vector3.one*radius*Mathf.Max(.04f,expand)*(1+.025f*Mathf.Sin(elapsed*40));
            foreach(Renderer renderer in renderers)
            {
                renderer.GetPropertyBlock(energy);energy.SetFloat("_Energy",fade*1.15f);energy.SetFloat("_Density",fade*.65f);renderer.SetPropertyBlock(energy);
            }
            foreach(LineRenderer line in ribbons){Color a=line.startColor,b=line.endColor;a.a=fade*.7f;b.a=fade*.8f;line.startColor=a;line.endColor=b;}
            while(pulses<3 && elapsed>=.18f+pulses*.28f)
            {
                victims.Clear();bosses.Clear();int damage=totalDamage/3+(pulses<totalDamage%3?1:0);
                if(damage>0)caster.DamageArea(transform.position,radius,damage,victims,false,false,bosses);
                pulses++;if(pulses>1)particles.Emit(30);
            }
            if(elapsed>=duration)Destroy(gameObject);
        }
    }
}
