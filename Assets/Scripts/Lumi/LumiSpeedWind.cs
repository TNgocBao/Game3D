using UnityEngine;

namespace LumiAdventure
{
    public sealed class LumiSpeedWind:MonoBehaviour
    {
        [SerializeField] private TrailRenderer[] trails=new TrailRenderer[3];
        private LumiPlayer player;
        private LumiGame game;
        private Vector3 previous;
        private static Material windMaterial;
        public bool IsEmitting{get;private set;}
        private void OnEnable()
        {
            if(player==null)player=GetComponent<LumiPlayer>();
            if(game==null)game=GetComponentInParent<LumiGame>();
            TrailRenderer[] existing=GetComponentsInChildren<TrailRenderer>();
            if(existing.Length==3)trails=existing;
            previous=transform.position;
        }
        public void Initialize(LumiPlayer owner,LumiGame session)
        {
            player=owner;game=session;previous=transform.position;
            trails=new TrailRenderer[3];
            if(windMaterial==null)windMaterial=new Material(Shader.Find("Sprites/Default")){name="Soft speed wind"};
            for(int i=0;i<trails.Length;i++)
            {
                GameObject streak=new GameObject("Speed wind "+i);streak.transform.SetParent(transform,false);
                TrailRenderer trail=streak.AddComponent<TrailRenderer>();trails[i]=trail;
                trail.sharedMaterial=windMaterial;trail.time=.38f+i*.055f;trail.minVertexDistance=.045f;
                trail.widthCurve=AnimationCurve.EaseInOut(0,.07f,1,0);
                trail.startColor=new Color(.72f,.95f,1,.7f);trail.endColor=new Color(.9f,1,1,0);
                trail.numCornerVertices=3;trail.numCapVertices=3;trail.emitting=false;
                trail.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;trail.receiveShadows=false;
            }
        }
        private void LateUpdate()
        {
            if(player==null || game==null)return;
            Vector3 delta=transform.position-previous;delta.y=0;
            IsEmitting=game.IsPlaying && player.IsAlive && player.IsSpeedBoosted && Time.deltaTime>0 && delta.magnitude/Time.deltaTime>.8f;
            Vector3 forward=delta.sqrMagnitude>.00001f?delta.normalized:transform.forward;
            Vector3 side=Vector3.Cross(Vector3.up,forward);
            for(int i=0;i<trails.Length;i++)
            {
                if(trails[i]==null)continue;
                trails[i].emitting=IsEmitting;
                trails[i].transform.position=transform.position-forward*(.38f+i*.08f)+side*((i-1)*.34f+Mathf.Sin(Time.time*14+i)*.04f)+Vector3.up*(.65f+i*.21f);
                if(delta.magnitude>4)trails[i].Clear();
            }
            previous=transform.position;
        }
        private void OnDisable(){foreach(TrailRenderer trail in trails)if(trail!=null){trail.emitting=false;trail.Clear();}}
    }
}
