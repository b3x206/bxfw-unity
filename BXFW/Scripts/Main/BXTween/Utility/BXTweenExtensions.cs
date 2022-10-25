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
        public static BXTweenCTX<Color> BXTwChangeColor(this TextMeshProUGUI target, Color LastValue, float Duration)
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
        public static BXTweenCTX<Color> BXTwChangeColor(this Image target, Color LastValue, float Duration)
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
        public static BXTweenCTX<Vector3> BXTwChangePos(this RectTransform target, Vector3 LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position, LastValue, Duration, (Vector3 v) => { target.position = v; }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwChangePosX(this RectTransform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position.x, LastValue, Duration,
                (float f) => { target.position = new Vector3(f, target.position.y, target.position.z); }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwChangePosY(this RectTransform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position.y, LastValue, Duration,
                (float f) => { target.position = new Vector3(target.position.x, f, target.position.z); }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwChangePosZ(this RectTransform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position.z, LastValue, Duration,
                (float f) => { target.position = new Vector3(target.position.x, target.position.y, f); }, target);

            return Context;
        }
        public static BXTweenCTX<Vector3> BXTwChangeAnchoredPosition(this RectTransform target, Vector2 LastValue, float Duration)
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
        public static BXTweenCTX<float> BXTwChangeAnchorPosX(this RectTransform target, float LastValue, float Duration)
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
        public static BXTweenCTX<float> BXTwChangeAnchorPosY(this RectTransform target, float LastValue, float Duration)
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

        /// <summary>
        /// NOTE : If you want accurate tweening in this method with rect transforms, please read.
        /// <br>1 : Get the RectTransform's rect using <see cref="BXTweenCustomLerp.GetCanvasRect(RectTransform)"/>.</br>
        /// <br/>
        /// <br>And that's it. For other rect purposes, use as you wish.</br>
        /// </summary>
        public static BXTweenCTX<float> BXTwChangeRect(this RectTransform target, Rect LastValue, float Duration)
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
                BXTweenCustomLerp.LerpRectTransformUnclamped(rectStart, rectEnd, f, target);
            }, target);

            return Context;
        }

        /// <summary>
        /// NOTE : This shortcut is more limited than others.
        /// <br/>
        /// <br>You cannot change the parameters without creating new tween.</br>
        /// <br>The 'time' parameter always goes between 0 to 1. (curves can be unclamped)</br>
        /// </summary>
        public static BXTweenCTX<float> BXTwChangeRect(this RectTransform target, RectTransform other, float Duration)
        {
            return BXTwChangeRect(target, other.rect, Duration);
        }
        #endregion

        #region Standard (UnityEngine)
        /// <see cref="Transform">
        public static BXTweenCTX<Vector3> BXTwChangePos(this Transform target, Vector3 LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position, LastValue, Duration,
                (Vector3 v) => { target.position = v; }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwChangePosX(this Transform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position.x, LastValue, Duration,
                (float f) => { target.position = new Vector3(f, target.position.y, target.position.z); }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwChangePosY(this Transform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position.y, LastValue, Duration,
                (float f) => { target.position = new Vector3(target.position.x, f, target.position.z); }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwChangePosZ(this Transform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position.z, LastValue, Duration,
                (float f) => { target.position = new Vector3(target.position.x, target.position.y, f); }, target);

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
        public static BXTweenCTX<Color> BXTwChangeColor(this Material target, Color LastValue, float Duration, string PropertyName = "_Color")
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
        public static BXTweenCTX<Color> BXTwChangeColor(this SpriteRenderer target, Color LastValue, float Duration)
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
        public static BXTweenCTX<float> BXTwChangeFOV(this Camera target, float LastValue, float Duration)
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
        public static BXTweenCTX<Matrix4x4> BXTwChangeProjectionMatrix(this Camera target, Matrix4x4 LastValue, float Duration)
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
        public static BXTweenCTX<float> BXTwChangeOrthoSize(this Camera target, float LastValue, float Duration)
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
        public static BXTweenCTX<Color> BXTwChangeBGColor(this Camera target, Color LastValue, float Duration)
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
        public static BXTweenCTX<float> BXTwChangeNearClipPlane(this Camera target, float LastValue, float Duration)
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
        public static BXTweenCTX<float> BXTwChangeFarClipPlane(this Camera target, float LastValue, float Duration)
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
        #endregion
    }
}
