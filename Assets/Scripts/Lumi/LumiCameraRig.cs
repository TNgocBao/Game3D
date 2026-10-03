using UnityEngine;

namespace LumiAdventure
{
    [DefaultExecutionOrder(-100)]
    public class LumiCameraRig : MonoBehaviour
    {
        private Transform target;
        private Camera viewCamera;
        private LumiGame game;
        [SerializeField] private float distance=5.8f;
        [SerializeField] private float pitch=18f;
        [SerializeField] private float yaw=0f;
        private Vector3 focus;
        private Vector2 mapSize;
        private bool closeView=true;
        [SerializeField,Range(.1f,8f)] private float mouseSensitivity=2.5f;
        public bool MouseLookActive=>!Application.isMobilePlatform && game!=null && game.IsPlaying && Application.isFocused && Cursor.lockState==CursorLockMode.Locked;
        public bool ReacquiredThisFrame {get;private set;}
        public float Yaw=>yaw;
        public float Pitch=>pitch;
        public float FollowDistance=>distance;
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

        private void Update()
        {
            ReacquiredThisFrame=false;
            if(target==null || game==null || !game.IsPlaying || !Application.isFocused)return;
            if(!Application.isMobilePlatform && Input.GetKeyDown(KeyCode.Tab))
                game.SetMouseLook(Cursor.lockState!=CursorLockMode.Locked);
            else if(!Application.isMobilePlatform && Cursor.lockState!=CursorLockMode.Locked && Input.GetMouseButtonDown(0) && !game.IsPointerOverUi())
            {
                Vector3 pointer=Input.mousePosition;
                if(pointer.x>=0 && pointer.x<Screen.width && pointer.y>=0 && pointer.y<Screen.height){game.SetMouseLook(true);ReacquiredThisFrame=true;}
            }
            if(MouseLookActive && !ReacquiredThisFrame)
                ApplyLookDelta(new Vector2(Input.GetAxisRaw("Mouse X"),Input.GetAxisRaw("Mouse Y")));
            else if(Application.isMobilePlatform)ApplyLookDelta(LumiMobileInput.Look);
            if(MouseLookActive || Application.isMobilePlatform)
            {
                distance=Mathf.Clamp(distance-Input.mouseScrollDelta.y*.65f,3.5f,28f);
                if(Input.GetKeyDown(KeyCode.V)){closeView=!closeView;distance=closeView?5.8f:26;pitch=closeView?18:48;}
                if(Input.GetKeyDown(KeyCode.Home)){yaw=0;pitch=18;distance=5.8f;closeView=true;}
            }
        }

        public void ApplyLookDelta(Vector2 delta)
        {
            // Mouse axes already represent motion per frame; multiplying by deltaTime makes sensitivity depend on FPS.
            yaw=Mathf.Repeat(yaw+delta.x*mouseSensitivity,360);
            pitch=Mathf.Clamp(pitch-delta.y*mouseSensitivity,-12,65);
            transform.rotation=Quaternion.Euler(pitch,yaw,0);
        }

        private void LateUpdate()
        {
            if (target == null || game == null) return;
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
            return target.position+Vector3.up*1.45f;
        }
    }
}

