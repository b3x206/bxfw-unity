using UnityEngine;

namespace BXFW.Tweening
{
    /// <summary>
    /// Custom Lerp methods used by BXTween.
    /// <br>Interpolates other math (or non-math) data types.</br>
    /// </summary>
    public static class BXTweenCustomLerp
    {
        /// <summary>
        /// Interpolates a Matrix4x4.
        /// <br>This can be used for interpolating such things as <see cref="Camera.projectionMatrix"/> and others.</br>
        /// </summary>
        public static Matrix4x4 MatrixLerp(Matrix4x4 src, Matrix4x4 dest, float time)
        {
            return MatrixLerpUnclamped(src, dest, Mathf.Clamp01(time));
        }
        /// <summary>
        /// Interpolates a Matrix4x4. (Unclamped)
        /// <br>This can be used for interpolating such things as <see cref="Camera.projectionMatrix"/> and others.</br>
        /// </summary>
        public static Matrix4x4 MatrixLerpUnclamped(Matrix4x4 src, Matrix4x4 dest, float time)
        {
            Matrix4x4 ret = new Matrix4x4();

            for (int i = 0; i < 16; i++)
            {
                ret[i] = Mathf.LerpUnclamped(src[i], dest[i], time);
            }

            return ret;
        }

        // Fixed the canvas get code by just going : RectTransform.rect.center
        // Yes, i do make unreasonable coding choices
        /// <summary>
        /// <br>Interpolates a rect transform from <paramref name="start"/> to <paramref name="end"/>.</br>
        /// <br>(parameter <paramref name="time"/> is clamped between 0-1)</br>
        /// </summary>
        public static void LerpRectTransform(Rect start, Rect end, float time, RectTransform target)
        {
            LerpRectTransformUnclamped(start, end, Mathf.Clamp01(time), target);
        }
        /// <summary>
        /// <br>Interpolates a rect transform from <paramref name="start"/> to <paramref name="end"/>.</br>
        /// </summary>
        public static void LerpRectTransformUnclamped(Rect start, Rect end, float time, RectTransform target)
        {
            target.localPosition = Vector2.Lerp(start.center, end.center, time);
            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(start.width, end.width, time));
            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Lerp(start.height, end.height, time));
        }
    }
}
