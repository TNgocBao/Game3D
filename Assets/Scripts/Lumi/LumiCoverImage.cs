using UnityEngine;
using UnityEngine.UI;

namespace LumiAdventure
{
    public sealed class LumiCoverImage : RawImage
    {
        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            UpdateCrop();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateCrop();
        }

        public void UpdateCrop()
        {
            if (texture == null || rectTransform.rect.width <= 0 || rectTransform.rect.height <= 0) return;
            float source = texture.width / (float)texture.height;
            float destination = rectTransform.rect.width / rectTransform.rect.height;
            uvRect = source > destination
                ? new Rect((1 - destination / source) * .5f, 0, destination / source, 1)
                : new Rect(0, (1 - source / destination) * .5f, 1, source / destination);
        }
    }
}
