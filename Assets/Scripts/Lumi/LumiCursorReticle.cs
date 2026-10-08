using UnityEngine;
using UnityEngine.UI;

namespace LumiAdventure
{
    public sealed class LumiCursorReticle:MonoBehaviour
    {
        private RectTransform rect;
        private RectTransform parent;
        private LumiGame game;
        private void Awake(){rect=GetComponent<RectTransform>();parent=rect.parent as RectTransform;game=FindObjectOfType<LumiGame>();}
        private void LateUpdate()
        {
            if(game!=null&&game.Controls.UsesMobileControls)
            {
                if(LumiMobileInput.PointerAimEnabled && LumiMobileInput.HasAim && RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,LumiMobileInput.AimScreenPoint,null,out Vector2 aim))rect.anchoredPosition=aim;
                else rect.anchoredPosition=Vector2.zero;
                return;
            }
            if(Cursor.lockState==CursorLockMode.Locked){rect.anchoredPosition=Vector2.zero;return;}
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,Input.mousePosition,null,out Vector2 point))rect.anchoredPosition=point;
        }
    }
}
