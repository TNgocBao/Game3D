using UnityEngine;
using UnityEngine.UI;

namespace LumiAdventure
{
    public sealed class LumiCursorReticle:MonoBehaviour
    {
        private RectTransform rect;
        private RectTransform parent;
        private void Awake(){rect=GetComponent<RectTransform>();parent=rect.parent as RectTransform;}
        private void LateUpdate()
        {
            if(Application.isMobilePlatform)return;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,Input.mousePosition,null,out Vector2 point))rect.anchoredPosition=point;
        }
    }
}
