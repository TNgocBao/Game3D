using UnityEngine;

namespace LumiAdventure
{
    public sealed class LumiMistZone : MonoBehaviour
    {
        private float previousFarClip;
        private float previousFogEnd;
        private Color previousFogColor;
        private bool active;

        private void OnTriggerEnter(Collider other)
        {
            LumiPlayer player=other.GetComponentInParent<LumiPlayer>();
            if(player==null || active)return;
            LumiGame game=FindObjectOfType<LumiGame>();
            if(game==null || game.CameraRig==null || game.CameraRig.ViewCamera==null)return;
            previousFarClip=game.CameraRig.ViewCamera.farClipPlane;
            previousFogEnd=RenderSettings.fogEndDistance;
            previousFogColor=RenderSettings.fogColor;
            game.CameraRig.ViewCamera.farClipPlane=Mathf.Min(previousFarClip,38f);
            RenderSettings.fogColor=Color.Lerp(previousFogColor,new Color(.7f,.86f,.9f),.62f);
            RenderSettings.fogEndDistance=Mathf.Min(previousFogEnd,32f);
            active=true;
        }

        private void OnTriggerExit(Collider other)
        {
            LumiPlayer player=other.GetComponentInParent<LumiPlayer>();
            if(player==null || !active)return;
            LumiGame game=FindObjectOfType<LumiGame>();
            if(game!=null && game.CameraRig!=null && game.CameraRig.ViewCamera!=null)
                game.CameraRig.ViewCamera.farClipPlane=previousFarClip;
            RenderSettings.fogEndDistance=previousFogEnd;
            RenderSettings.fogColor=previousFogColor;
            active=false;
        }
    }
}
