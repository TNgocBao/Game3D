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
        [SerializeField] private float pitch=0f;
        [SerializeField] private float yaw=0f;
        private Vector3 focus;
        private Vector2 mapSize;
        private bool firstPerson=true;
        private LumiPlayerViewVisibility visibility;
        public bool FirstPerson=>firstPerson;
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
                return Quaternion.Euler(0,yaw,0)*Vector3.forward;
            }
        }

        public void Initialize(LumiGame owner, Transform followTarget, Color background,Vector2 bounds)
        {
            game = owner;
            target = followTarget;
            mapSize=bounds;
            viewCamera = gameObject.AddComponent<Camera>();
            viewCamera.orthographic = false;
            viewCamera.fieldOfView = 75f;
            viewCamera.nearClipPlane = .05f;
            viewCamera.farClipPlane = 180f;
            viewCamera.clearFlags = CameraClearFlags.SolidColor;
            viewCamera.backgroundColor = background;
            gameObject.tag="MainCamera";
            gameObject.AddComponent<AudioListener>();
            visibility=target.GetComponent<LumiPlayerViewVisibility>();if(visibility==null)visibility=target.gameObject.AddComponent<LumiPlayerViewVisibility>();
            visibility.Initialize();visibility.SetFirstPerson(firstPerson);
            focus=FocusPoint();
            transform.rotation=Quaternion.Euler(pitch,yaw,0f);
            transform.position=firstPerson?EyePoint():focus-transform.forward*distance;

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
                if(!firstPerson)distance=Mathf.Clamp(distance-Input.mouseScrollDelta.y*.65f,3.5f,9f);
                if(Input.GetKeyDown(KeyCode.V))SetFirstPerson(!firstPerson);
                if(Input.GetKeyDown(KeyCode.Home)){yaw=0;pitch=0;distance=5.8f;SetFirstPerson(true);}
            }
        }

        public void ApplyLookDelta(Vector2 delta)
        {
            // Mouse axes already represent motion per frame; multiplying by deltaTime makes sensitivity depend on FPS.
            yaw=Mathf.Repeat(yaw+delta.x*mouseSensitivity,360);
            pitch=Mathf.Clamp(pitch-delta.y*mouseSensitivity,-89.5f,89.5f);
            transform.rotation=Quaternion.Euler(pitch,yaw,0);
        }

        private void LateUpdate()
        {
            if (target == null || game == null) return;
            if(firstPerson){transform.SetPositionAndRotation(EyePoint(),Quaternion.Euler(pitch,yaw,0));return;}
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

        public void SetFirstPerson(bool value)
        {
            firstPerson=value;
            if(visibility!=null)visibility.SetFirstPerson(value);
            if(viewCamera!=null){viewCamera.nearClipPlane=value?.05f:.15f;viewCamera.fieldOfView=value?75:60;}
            if(target!=null){focus=FocusPoint();transform.position=value?EyePoint():focus-transform.forward*distance;}
        }
        private Transform eyeSocket;
        public Vector3 EyePoint(){if(eyeSocket==null && target!=null){foreach(var child in target.GetComponentsInChildren<Transform>())if(child.name=="Eye camera socket"){eyeSocket=child;break;}}return (eyeSocket!=null?eyeSocket.position:target.position+Vector3.up*1.585f)+FlatForward*.06f;}
        private void OnDestroy(){if(visibility!=null)visibility.SetFirstPerson(false);}

        private Vector3 FocusPoint()
        {
            return target.position+Vector3.up*1.45f;
        }
    }
}



