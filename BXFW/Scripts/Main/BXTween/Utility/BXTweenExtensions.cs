using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static BXFW.Tweening.BXTween;

namespace BXFW.Tweening
{
    // Using visual studio, to get a more readable experience, use the following shortcuts : 
    // Ctrl + M and then Ctrl + O (case-insensitive, uses chord keys)
    // If you have these chord keys binded to something else, just do what the upper command does on vanilla visual studio

    /// <summary>
    /// Extension methods for shortcut calls to other objects.
    /// </summary>
    public static class BXTweenExtensions
    {
        #region TextMeshPro
        /// <see cref="TextMeshProUGUI"/>
        public static BXTweenCTX<float> BXTwFadeAlpha(this TextMeshProUGUI target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.alpha, LastValue, Duration, (float f) => { target.alpha = f; }, target);

            return Context;
        }
        public static BXTweenCTX<Color> BXTwColor(this TextMeshProUGUI target, Color LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.color, LastValue, Duration, (Color c) => { target.color = c; }, target);

            return Context;
        }
        #endregion

        #region UnityEngine.UI
        /// <see cref="CanvasGroup"/>
        public static BXTweenCTX<float> BXTwFadeAlpha(this CanvasGroup target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.alpha, LastValue, Duration, (float f) => { target.alpha = f; }, target);

            return Context;
        }

        /// <see cref="Image"/>
        public static BXTweenCTX<Color> BXTwColor(this Image target, Color LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.color, LastValue, Duration, (Color c) => { target.color = c; }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwFadeAlpha(this Image target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.color.a, LastValue, Duration,
                (float f) => { target.color = new Color(target.color.r, target.color.g, target.color.b, f); }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwInterpFill(this Image target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.fillAmount, LastValue, Duration, (float f) => { target.fillAmount = f; }, target);

            return Context;
        }

        /// <see cref="RectTransform"/>
        public static BXTweenCTX<Vector3> BXTwMoveAnchorPos(this RectTransform target, Vector2 LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.anchoredPosition, LastValue, Duration,
                (Vector3 v) => { target.anchoredPosition = v; }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwMoveAnchorPosX(this RectTransform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.anchoredPosition.x, LastValue, Duration,
                (float f) => { target.anchoredPosition = new Vector2(f, target.anchoredPosition.y); }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwMoveAnchorPosY(this RectTransform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.anchoredPosition.y, LastValue, Duration,
                (float f) => { target.anchoredPosition = new Vector2(target.anchoredPosition.x, f); }, target);

            return Context;
        }

        // TODO : Maybe add an 'Rect' context?
        // Or use an special BXTweenCTX class, saying it changes multiple values?
        // (which only use float as parameter, with name like MultiBXTweenCTX?)

        public static BXTweenCTX<float> BXTwRect(this RectTransform target, Rect LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            // Since this is a canvas item, the lossyScale will return incorrectly
            // Just check it's local scale as in canvas everything runs in a local space configuration
            if (target.localScale != Vector3.one)
            {
                Debug.LogWarning(BXTweenStrings.GetWarn_TargetConfInvalid("Target RectTransform is scaled incorrectly. (expected localScale to be Vector3.one) Tween may act incorrectly."));
            }

            var rectStart = target.rect;
            var rectEnd = LastValue;

            var Context = To(0f, 1f, Duration, (float f) =>
            {
                BXTweenCustomLerp.RectTransformLerpUnclamped(rectStart, rectEnd, f, target);
            }, target);

            return Context;
        }

        /// <summary>
        /// NOTE : This shortcut is more limited than others.
        /// (because it uses a custom lerp, and the lerp parameter is the tweened float)
        /// <br/>
        /// <br>You cannot change the parameters without creating new tween.</br>
        /// <br>The 'time' parameter always goes between 0 to 1. (curves can be unclamped)</br>
        /// </summary>
        public static BXTweenCTX<float> BXTwRect(this RectTransform target, RectTransform other, float Duration)
        {
            return BXTwRect(target, other.rect, Duration);
        }

        /// <see cref="Graphic"/>
        public static BXTweenCTX<float> BXTwFadeAlpha(this Graphic target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.color.a, LastValue, Duration, (float f) => { target.color = new Color(target.color.r, target.color.g, target.color.b, f); }, target);

            return Context;
        }
        public static BXTweenCTX<Color> BXTwColor(this Graphic target, Color LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.color, LastValue, Duration, (Color c) => { target.color = c; }, target);

            return Context;
        }
        #endregion

        #region Standard (UnityEngine)
        /// <see cref="Transform">
        public static BXTweenCTX<Vector3> BXTwMove(this Transform target, Vector3 LastValue, float Duration, Space space = Space.World)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            BXTweenCTX<Vector3> Context;
            switch (space)
            {
                default:
                case Space.World:
                    Context = To(target.position, LastValue, Duration,
                        (Vector3 v) => { target.position = v; }, target);
                    break;
                case Space.Self:
                    Context = To(target.localPosition, LastValue, Duration,
                        (Vector3 v) => { target.localPosition = v; }, target);
                    break;
            }

            return Context;
        }
        public static BXTweenCTX<float> BXTwMoveX(this Transform target, float LastValue, float Duration, Space space = Space.World)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            BXTweenCTX<float> Context;
            switch (space)
            {
                default:
                case Space.World:
                    Context = To(target.position.x, LastValue, Duration,
                        (float f) => { target.position = new Vector3(f, target.position.y, target.position.z); }, target);
                    break;
                case Space.Self:
                    Context = To(target.localPosition.x, LastValue, Duration,
                        (float f) => { target.localPosition = new Vector3(f, target.localPosition.y, target.localPosition.z); }, target);
                    break;
            }

            return Context;
        }
        public static BXTweenCTX<float> BXTwMoveY(this Transform target, float LastValue, float Duration, Space space = Space.World)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            BXTweenCTX<float> Context;
            switch (space)
            {
                default:
                case Space.World:
                    Context = To(target.position.y, LastValue, Duration,
                        (float f) => { target.position = new Vector3(target.position.x, f, target.position.z); }, target);
                    break;
                case Space.Self:
                    Context = To(target.localPosition.y, LastValue, Duration,
                        (float f) => { target.localPosition = new Vector3(target.localPosition.x, f, target.localPosition.z); }, target);
                    break;
            }

            return Context;
        }
        public static BXTweenCTX<float> BXTwMoveZ(this Transform target, float LastValue, float Duration, Space space = Space.World)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            BXTweenCTX<float> Context;
            switch (space)
            {
                default:
                case Space.World:
                    Context = To(target.position.z, LastValue, Duration,
                        (float f) => { target.position = new Vector3(target.position.x, target.position.y, f); }, target);
                    break;
                case Space.Self:
                    Context = To(target.localPosition.z, LastValue, Duration,
                        (float f) => { target.localPosition = new Vector3(target.localPosition.x, target.localPosition.y, f); }, target);
                    break;
            }

            return Context;
        }
        public static BXTweenCTX<Quaternion> BXTwRotate(this Transform target, Quaternion LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.rotation, LastValue, Duration,
                (Quaternion q) => { target.rotation = q; }, target);

            return Context;
        }
        public static BXTweenCTX<Vector3> BXTwRotateEuler(this Transform target, Vector3 LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.rotation.eulerAngles, LastValue, Duration,
                (Vector3 f) => { target.rotation = Quaternion.Euler(f); }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwRotateAngleAxis(this Transform target, float LastValue, Vector3 Axis, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            Axis = Axis.normalized;
            var EulerStartValue = 0f;
            if (Axis.x != 0f)
            {
                EulerStartValue = target.eulerAngles.x;
            }
            if (Axis.y != 0f)
            {
                EulerStartValue = target.eulerAngles.y;
            }
            if (Axis.z != 0f)
            {
                EulerStartValue = target.eulerAngles.z;
            }

            var Context = To(EulerStartValue, LastValue, Duration,
                (float f) => { target.rotation = Quaternion.AngleAxis(f, Axis); }, target);

            return Context;
        }
        public static BXTweenCTX<Vector3> BXTwScale(this Transform target, Vector3 LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.localScale, LastValue, Duration,
                (Vector3 f) => { target.localScale = f; }, target);

            return Context;
        }
        /// <summary>
        /// Same as <see cref="BXTwScale(Transform, Vector3, float)"/>, but a <see langword="new"/> <see cref="Vector3"/>() 
        /// is created with all the parameters equaling to <paramref name="LastValue"/>.
        /// </summary>
        public static BXTweenCTX<Vector3> BXTwScale(this Transform target, float LastValue, float Duration)
        {
            return BXTwScale(target, new Vector3(LastValue, LastValue, LastValue), Duration);
        }
        public static BXTweenCTX<float> BXTwScaleX(this Transform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.localScale.x, LastValue, Duration,
                (float f) => { target.localScale = new Vector3(f, target.localScale.y, target.localScale.z); }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwScaleY(this Transform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.localScale.y, LastValue, Duration,
                (float f) => { target.localScale = new Vector3(target.localScale.x, f, target.localScale.z); }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwScaleZ(this Transform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.localScale.z, LastValue, Duration,
                (float f) => { target.localScale = new Vector3(target.localScale.x, target.localScale.y, f); }, target);

            return Context;
        }

        /// <see cref="Material">
        public static BXTweenCTX<Color> BXTwColor(this Material target, Color LastValue, float Duration, string PropertyName = "_Color")
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.GetColor(PropertyName), LastValue, Duration,
                (Color c) => { target.SetColor(PropertyName, c); }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwFadeAlpha(this Material target, float LastValue, float Duration, string PropertyName = "_Color")
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.GetColor(PropertyName).a, LastValue, Duration,
                (float f) =>
                {
                    var c = target.GetColor(PropertyName);
                    target.SetColor(PropertyName, new Color(c.r, c.g, c.b, f));
                }, target);

            return Context;
        }

        /// <see cref="SpriteRenderer"/>
        public static BXTweenCTX<Color> BXTwColor(this SpriteRenderer target, Color LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.color, LastValue, Duration,
                (Color c) => { target.color = c; }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwFadeAlpha(this SpriteRenderer target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.color.a, LastValue, Duration,
                (float f) =>
                {
                    var c = target.color;
                    target.color = new Color(c.r, c.g, c.b, f);
                }, target);

            return Context;
        }

        /// <see cref="Camera"/>
        
        /// <summary>
        /// Tweens the <see cref="Camera.fieldOfView"/> property.
        /// </summary>
        public static BXTweenCTX<float> BXTwFOV(this Camera target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }
            var Context = To(target.fieldOfView, LastValue, Duration,
                (float f) => { target.fieldOfView = f; }, target);

            return Context;
        }
        /// <summary>
        /// Tweens the <see cref="Camera.projectionMatrix"/> field.
        /// <br>Useful for changing perspective from ortho to perspective, and so on.</br>
        /// </summary>
        public static BXTweenCTX<Matrix4x4> BXTwProjectionMatrix(this Camera target, Matrix4x4 LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }
            var Context = To(target.projectionMatrix, LastValue, Duration,
                (Matrix4x4 m) => { target.projectionMatrix = m; }, target);

            return Context;
        }
        /// <summary>
        /// Tweens the <see cref="Camera.orthographicSize"/> field.
        /// </summary>
        public static BXTweenCTX<float> BXTwOrthoSize(this Camera target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.orthographicSize, LastValue, Duration,
                (float f) => { target.orthographicSize = f; }, target);

            return Context;
        }
        /// <summary>
        /// Tweens the <see cref="Camera.backgroundColor"/> property.
        /// </summary>
        public static BXTweenCTX<Color> BXTwBGColor(this Camera target, Color LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.backgroundColor, LastValue, Duration,
                (Color c) => { target.backgroundColor = c; }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwNearClipPlane(this Camera target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.nearClipPlane, LastValue, Duration,
                (float f) => { target.nearClipPlane = f; }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwFarClipPlane(this Camera target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.farClipPlane, LastValue, Duration,
                (float f) => { target.farClipPlane = f; }, target);

            return Context;
        }
        
        /// <see cref="AudioSource"/>
        public static BXTweenCTX<float> BXTwFadeVolume(this AudioSource target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.volume, LastValue, Duration,
                (float f) => { target.volume = f; }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwPitch(this AudioSource target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.pitch, LastValue, Duration,
                (float f) => { target.pitch = f; }, target);

            return Context;
        }
        /// <summary>
        /// Tweens the <see cref="AudioSource.spatialBlend"/> property.
        /// <br>Target value is clamped between 0-1.</br>
        /// </summary>
        public static BXTweenCTX<float> BXTwSpatialBlend(this AudioSource target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.spatialBlend, LastValue, Duration,
                (float f) => { target.spatialBlend = Mathf.Clamp01(f); }, target);

            return Context;
        }
        #endregion
    }
}
