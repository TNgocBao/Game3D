using UnityEngine;

namespace LumiAdventure
{
    public class LumiCameraRig : MonoBehaviour
    {
        private Transform target;
        private Camera viewCamera;
        private LumiGame game;
        [SerializeField] private float distance=10f;
        [SerializeField] private float pitch=26f;
        [SerializeField] private float yaw=0f;
        private Vector3 focus;
        private Vector2 mapSize;
        private bool closeView=true;
        private readonly RaycastHit[] cameraHits=new RaycastHit[24];

        public Camera ViewCamera => viewCamera;
        public Vector3 FlatForward
        {
            get
            {
                Vector3 forward = viewCamera.transform.forward;
                forward.y = 0f;
                return forward.sqrMagnitude > 0.01f ? forward.normalized : Vector3.forward;
            }
        }

        public void Initialize(LumiGame owner, Transform followTarget, Color background,Vector2 bounds)
        {
            game = owner;
            target = followTarget;
            mapSize=bounds;
            viewCamera = gameObject.AddComponent<Camera>();
            viewCamera.orthographic = false;
            viewCamera.fieldOfView = 60f;
            viewCamera.nearClipPlane = 0.15f;
            viewCamera.farClipPlane = 180f;
            viewCamera.clearFlags = CameraClearFlags.SolidColor;
            viewCamera.backgroundColor = background;
            gameObject.tag="MainCamera";
            gameObject.AddComponent<AudioListener>();
            focus=FocusPoint();
            transform.rotation=Quaternion.Euler(pitch,yaw,0f);
            transform.position=focus-transform.forward*distance;

        }

        private void LateUpdate()
        {
            if (target == null || game == null) return;

            if(game.IsPlaying && !game.IsPointerOverUi())
            {
                if(Input.GetMouseButton(1))
                {
                    yaw+=Input.GetAxis("Mouse X")*2.5f;
                    pitch=Mathf.Clamp(pitch-Input.GetAxis("Mouse Y")*2,16f,64f);
                }
                distance=Mathf.Clamp(distance-Input.mouseScrollDelta.y,5f,32f);
                if(Input.GetKeyDown(KeyCode.V)){closeView=!closeView;distance=closeView?10:26;pitch=closeView?26:48;}
                if(Input.GetKeyDown(KeyCode.Home)){yaw=0;pitch=26;distance=10;closeView=true;}
            }
            focus=Vector3.Lerp(focus,FocusPoint(),1-Mathf.Exp(-7*Time.unscaledDeltaTime));
            Quaternion rotation=Quaternion.Euler(pitch,yaw,0f);
            Vector3 behind=-(rotation*Vector3.forward);
            float visibleDistance=distance;
            int count=Physics.SphereCastNonAlloc(focus,.28f,behind,cameraHits,distance,~0,QueryTriggerInteraction.Ignore);
            for(int i=0;i<count;i++)
            {
                Collider obstacle=cameraHits[i].collider;
                if(obstacle==null || obstacle.transform.IsChildOf(target))continue;
                Renderer renderer=obstacle.GetComponent<Renderer>();
                if(renderer!=null && !renderer.enabled)continue;
                if(obstacle.GetComponentInParent<LumiEnemy>()!=null)continue;
                visibleDistance=Mathf.Min(visibleDistance,Mathf.Max(1.5f,cameraHits[i].distance-.35f));
            }
            transform.position=focus+behind*visibleDistance;
            transform.rotation=rotation;

        }

        private Vector3 FocusPoint()
        {
            return target.position+Vector3.up*1.25f;
        }
    }
}

