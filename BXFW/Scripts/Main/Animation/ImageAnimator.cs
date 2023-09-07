using UnityEngine;
using UnityEngine.UI;

namespace BXFW
{
    /// <summary>
    /// Animates an <see cref="Image"/>'s sprite property.
    /// </summary>
    public sealed class ImageAnimator : ValueAnimator<Sprite>
    {
        [InspectorLine(LineColor.Gray)]
        public Image targetImage;
        public override Sprite AnimatedValue
        {
            get
            {
                if (targetImage == null)
                    TryGetComponent(out targetImage);

                return targetImage.sprite;
            }
            protected set
            {
                if (targetImage == null)
                    TryGetComponent(out targetImage);

                targetImage.sprite = value;
            }
        }
    }
}
