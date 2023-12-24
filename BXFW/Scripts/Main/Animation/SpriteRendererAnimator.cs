using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Animates a <see cref="SpriteRenderer"/>'s sprite property.
    /// </summary>
    public sealed class SpriteRendererAnimator : ValueAnimator<Sprite>
    {
        [InspectorLine(LineColor.Gray)]
        public SpriteRenderer targetRenderer;
        public override Sprite AnimatedValue
        {
            get
            {
                if (targetRenderer == null)
                {
                    TryGetComponent(out targetRenderer);
                }

                return targetRenderer.sprite;
            }
            protected set
            {
                if (targetRenderer == null)
                {
                    TryGetComponent(out targetRenderer);
                }

                targetRenderer.sprite = value;
            }
        }
    }
}
