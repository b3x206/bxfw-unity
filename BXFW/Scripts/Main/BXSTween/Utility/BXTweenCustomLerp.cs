using UnityEngine;
using System.Runtime.CompilerServices;

namespace BXFW.Tweening
{
    /// <summary>
    /// Old custom linear interpolation methods used by BXTween.
    /// <br>Interpolates other non-mathematical data types.</br>
    /// </summary>
    public static class BXTweenCustomLerp
    {
        /// <summary>
        /// Utility method to get the "world/local spaced in canvas" rect of the <see cref="RectTransform"/>.
        /// <br>The returned rect from the <see cref="RectTransform.rect"/> has incorrect position, this fixes that.</br>
        /// <br>NOTE : Pivot of this rect is the top-right corner. To get the center from given rect use <see cref="Rect.center"/>.</br>
        /// </summary>
        /// <param name="target">Target rect transform.</param>
        public static Rect GetCanvasRect(this RectTransform target, Space space = Space.Self)
        {
            float xPos;
            float yPos;

            switch (space)
            {
                default:
                case Space.Self:
                    xPos = target.localPosition.x - (target.rect.width / 2f);
                    yPos = target.localPosition.y - (target.rect.height / 2f);
                    break;

                case Space.World:
                    xPos = target.position.x - (target.rect.width / 2f);
                    yPos = target.position.y - (target.rect.height / 2f);
                    break;
            }

            return new Rect(xPos, yPos, target.rect.width, target.rect.height);
        }

        /// <summary>
        /// <br>Interpolates a rect transform from <paramref name="start"/> to <paramref name="end"/>.</br>
        /// <br>(parameter <paramref name="time"/> is clamped between 0-1)</br>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RectTransformLerp(Rect start, Rect end, float time, RectTransform target, Space posSpace = Space.Self)
        {
            RectTransformLerpUnclamped(start, end, Mathf.Clamp01(time), target, posSpace);
        }
        /// <summary>
        /// <br>Interpolates a rect transform from <paramref name="start"/> to <paramref name="end"/>.</br>
        /// </summary>
        public static void RectTransformLerpUnclamped(Rect start, Rect end, float time, RectTransform target, Space posSpace = Space.Self)
        {
            switch (posSpace)
            {
                default:
                case Space.Self:
                    target.localPosition = Vector2.LerpUnclamped(start.center, end.center, time);
                    break;

                case Space.World:
                    target.position = Vector2.LerpUnclamped(start.center, end.center, time);
                    break;
            }

            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.LerpUnclamped(start.width, end.width, time));
            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.LerpUnclamped(start.height, end.height, time));
        }
    }
}
