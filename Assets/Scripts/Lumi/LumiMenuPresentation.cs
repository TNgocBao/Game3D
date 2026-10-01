using UnityEngine;
using UnityEngine.EventSystems;

namespace LumiAdventure
{
    public class LumiMenuBackdrop : MonoBehaviour
    {
        private AudioListener audioListener;
        public void Initialize()
        {
            var cameraObject=new GameObject("Menu Camera");
            cameraObject.transform.SetParent(transform,false);
            var camera=cameraObject.AddComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.025f,.04f,.045f);
            camera.cullingMask=0;
            camera.depth=-100;
            audioListener=cameraObject.AddComponent<AudioListener>();
        }
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
            if(audioListener!=null)audioListener.enabled=visible;
        }
    }
    public class LumiMenuCloud : MonoBehaviour
    {
        public float Speed;
        private void Update()
        {
            transform.Translate(Vector3.right * Speed * Time.unscaledDeltaTime, Space.World);
            if (transform.position.x > 25f) transform.position += Vector3.left * 50f;
        }
    }

    public class LumiUiMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Vector3 target = Vector3.one;
        private Vector3 baseScale = Vector3.one;
        public bool Locked;

        private void Awake() => baseScale = transform.localScale;
        public void OnPointerEnter(PointerEventData eventData) { if (!Locked) target = baseScale * 1.06f; }
        public void OnPointerExit(PointerEventData eventData) => target = baseScale;
        private void Update() => transform.localScale = Vector3.Lerp(transform.localScale, target, 12f * Time.unscaledDeltaTime);
    }

    public class LumiUiSound : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
    {
        private LumiAudio audioSource;
        private void Awake()
        {
            var game = FindObjectOfType<LumiGame>();
            if (game != null) audioSource = game.Audio;
        }
        public void OnPointerDown(PointerEventData eventData) { if (audioSource != null) audioSource.Play("ui"); }
        public void OnPointerEnter(PointerEventData eventData) { if (audioSource != null) audioSource.Play("hover", 0.35f); }
    }
}

