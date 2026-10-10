using UnityEngine;

namespace LumiAdventure
{
    public enum LumiCameraMode { FirstPerson, FreeThirdPerson, MovementFollow }

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
        [SerializeField] private LumiCameraMode mode=LumiCameraMode.FirstPerson;
        private LumiPlayerViewVisibility visibility;
        public bool FirstPerson=>mode==LumiCameraMode.FirstPerson;
        // MovementFollow is kept as a serialized compatibility value for older saves,
        // but is no longer an exposed camera mode. Free third-person is always free-look.
        public bool MovementFacing=>false;
        public LumiCameraMode Mode=>mode;
        [SerializeField,Range(.1f,8f)] private float mouseSensitivity=2.5f;
        public bool MouseLookActive=>game!=null&&game.Controls.UsesPcControls&&game.IsPlaying&&Application.isFocused&&Cursor.lockState==CursorLockMode.Locked;
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
            mode=(LumiCameraMode)Mathf.Clamp(PlayerPrefs.GetInt("Lumi.CameraMode",0),0,1);
            if (mode==LumiCameraMode.MovementFollow) mode=LumiCameraMode.FreeThirdPerson;
            visibility.Initialize();visibility.SetFirstPerson(FirstPerson);
            LumiMobileInput.PointerAimEnabled=game.Controls.UsesMobileControls;
            focus=FocusPoint();
            transform.rotation=Quaternion.Euler(pitch,yaw,0f);
            transform.position=FirstPerson?EyePoint():focus-transform.forward*distance;

        }

        private void Update()
        {
            ReacquiredThisFrame=false;
            if(target==null || game==null || !game.IsPlaying || !Application.isFocused)return;
            if(Input.GetKeyDown(KeyCode.V)){CycleMode();return;}
            if(game.Controls.UsesPcControls&&!MovementFacing&&Input.GetKeyDown(KeyCode.Tab))
                game.SetMouseLook(Cursor.lockState!=CursorLockMode.Locked);
            else if(game.Controls.UsesPcControls&&!MovementFacing&&Cursor.lockState!=CursorLockMode.Locked&&Input.GetMouseButtonDown(0)&&!game.IsPointerOverUi())
            {
                Vector3 pointer=Input.mousePosition;
                if(pointer.x>=0 && pointer.x<Screen.width && pointer.y>=0 && pointer.y<Screen.height){game.SetMouseLook(true);ReacquiredThisFrame=true;}
            }
            if(!MovementFacing && MouseLookActive && !ReacquiredThisFrame)
                ApplyLookDelta(new Vector2(Input.GetAxisRaw("Mouse X"),Input.GetAxisRaw("Mouse Y")));
            else if(!MovementFacing&&game.Controls.UsesMobileControls)ApplyLookDelta(LumiMobileInput.Look);
            if(!FirstPerson && !MovementFacing)distance=Mathf.Clamp(distance-Input.mouseScrollDelta.y*.65f,3.5f,9f);
            if(Input.GetKeyDown(KeyCode.Home))
            {
                yaw=0;pitch=0;distance=5.8f;SetMode(LumiCameraMode.FirstPerson);
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
            if(FirstPerson){transform.SetPositionAndRotation(EyePoint(),Quaternion.Euler(pitch,yaw,0));return;}
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
            SetMode(value?LumiCameraMode.FirstPerson:LumiCameraMode.FreeThirdPerson,false);
        }
        public void CycleMode()
        {
            SetMode(mode==LumiCameraMode.FirstPerson?LumiCameraMode.FreeThirdPerson:LumiCameraMode.FirstPerson,true);
        }
        public void SetMode(LumiCameraMode value,bool notify=true)
        {
            if(value==LumiCameraMode.MovementFollow)value=LumiCameraMode.FreeThirdPerson;
            bool first=value==LumiCameraMode.FirstPerson;
            mode=value;
            if(game!=null&&game.IsPlaying)game.SetMouseLook(game.Controls.UsesPcControls);
            LumiMobileInput.PointerAimEnabled=game!=null&&game.Controls.UsesMobileControls;
            if(!LumiMobileInput.PointerAimEnabled)LumiMobileInput.HasAim=false;
            if(visibility!=null)visibility.SetFirstPerson(first);
            if(viewCamera!=null){viewCamera.nearClipPlane=first?.05f:.15f;viewCamera.fieldOfView=first?75:60;}
            if(target!=null){focus=FocusPoint();transform.position=first?EyePoint():focus-Quaternion.Euler(pitch,yaw,0)*Vector3.forward*distance;}
            PlayerPrefs.SetInt("Lumi.CameraMode",(int)mode);PlayerPrefs.Save();
            if(notify&&game!=null)game.ShowToast(mode==LumiCameraMode.FirstPerson?"GÓC NHÌN: THỨ NHẤT":"GÓC NHÌN: THỨ BA TỰ DO",new Color(.25f,.85f,1));
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



