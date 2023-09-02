using UnityEngine;
using BXFW.Tweening.Events;
using System.Collections.Generic;

namespace BXFW.Tweening
{
    /// <summary>
    /// Easing type of the tweenings, the easing functions are gathered from the <see cref="BXTweenEase"/>.
    /// <br>
    /// You can see an approximation curve in the Window &gt; BXTween &gt; Settings by selecting 
    /// the default curve or look at the website https://easings.net/
    /// </br>
    /// </summary>
    public enum EaseType
    {
        Linear,
        QuadIn,
        QuadOut,
        QuadInOut,
        CubicIn,
        CubicOut,
        CubicInOut,
        QuartIn,
        QuartOut,
        QuartInOut,
        QuintIn,
        QuintOut,
        QuintInOut,
        BounceIn,
        BounceOut,
        BounceInOut,
        ElasticIn,
        ElasticOut,
        ElasticInOut,
        CircularIn,
        CircularOut,
        CircularInOut,
        SinusIn,
        SinusOut,
        SinusInOut,
        ExponentialIn,
        ExponentialOut,
        ExponentialInOut
    }

    /// <summary>
    /// Includes the hard coded ease types.
    /// To create custom ease curves use the <see cref="AnimationCurve"/>. (in BXTween context field : <see cref="BXTweenCTX{T}.SetCustomCurve(AnimationCurve, bool)"/>.
    /// </summary>
    public static class BXTweenEase
    {
        /// <summary>
        /// All hardcoded ease methods in a dictionary.
        /// <br>All values from the <see cref="EaseType"/> enumeration exists in here.</br>
        /// </summary>
        public static readonly IReadOnlyDictionary<EaseType, BXTweenEaseSetMethod> Methods = new Dictionary<EaseType, BXTweenEaseSetMethod>
        {
            // None = Linear
            // The option 'None' was added to detect default settings.
            { EaseType.Linear, Linear },
            { EaseType.QuadIn, QuadIn },
            { EaseType.QuadOut, QuadOut },
            { EaseType.QuadInOut, QuadInOut },
            { EaseType.CubicIn, CubicIn },
            { EaseType.CubicOut, CubicOut },
            { EaseType.CubicInOut, CubicInOut },
            { EaseType.QuartIn, QuartIn },
            { EaseType.QuartOut, QuartOut },
            { EaseType.QuartInOut, QuartInOut },
            { EaseType.QuintIn, QuintIn },
            { EaseType.QuintOut, QuintOut },
            { EaseType.QuintInOut, QuintInOut },
            { EaseType.BounceIn, BounceIn },
            { EaseType.BounceOut, BounceOut },
            { EaseType.BounceInOut, BounceInOut },
            { EaseType.ElasticIn, ElasticIn },
            { EaseType.ElasticOut, ElasticOut },
            { EaseType.ElasticInOut, ElasticInOut },
            { EaseType.CircularIn, CircularIn },
            { EaseType.CircularOut, CircularOut },
            { EaseType.CircularInOut, CircularInOut },
            { EaseType.SinusIn, SinusIn },
            { EaseType.SinusOut, SinusOut },
            { EaseType.SinusInOut, SinusInOut },
            { EaseType.ExponentialIn, ExponentialIn },
            { EaseType.ExponentialOut, ExponentialOut },
            { EaseType.ExponentialInOut, ExponentialInOut }
        };

        #region Ease Methods
        // Note : All ease methods are unclamped.
        private static float Linear(float t)
        {
            var tVal = t;
            return tVal;
        }
        private static float QuadIn(float t)
        {
            var tVal = t * t;
            return tVal;
        }
        private static float QuadOut(float t)
        {
            var tVal = t * (2f - t);
            return tVal;
        }
        private static float QuadInOut(float t)
        {
            var tVal = t < 0.5f ? 2f * t * t : -1f + ((4f - (2f * t)) * t);
            return tVal;
        }
        private static float CubicIn(float t)
        {
            var tVal = t * t * t;
            return tVal;
        }
        private static float CubicOut(float t)
        {
            var tSub = 1f - t;
            var tVal = 1f - (tSub * tSub * tSub);
            return tVal;
        }
        private static float CubicInOut(float t)
        {
            var tVal = t < 0.5f ? 4f * t * t * t : ((t - 1f) * ((2f * t) - 2f) * ((2 * t) - 2)) + 1f;
            return tVal;
        }
        private static float QuartIn(float t)
        {
            var tVal = t * t * t * t;
            return tVal;
        }
        private static float QuartOut(float t)
        {
            var tInv = 1f - t; // inverted t (assuming t = clamped between 0-1)
            var tVal = 1f - (tInv * tInv * tInv * tInv);
            return tVal;
        }
        private static float QuartInOut(float t)
        {
            var tI2v = (-2 * t) + 2; // (-2 * x) + 2
            var tVal = t < 0.5f ? 8f * t * t * t * t : 1f - ((tI2v * tI2v * tI2v * tI2v) / 2f);
            return tVal;
        }
        private static float QuintIn(float t)
        {
            var tVal = t * t * t * t * t;
            return tVal;
        }
        private static float QuintOut(float t)
        {
            var tInv = 1f - t; // inverted t (assuming t = clamped between 0-1)
            var tVal = 1f - (tInv * tInv * tInv * tInv * tInv);
            return tVal;
        }
        private static float QuintInOut(float t)
        {
            var tI2v = (-2 * t) + 2; // (-2 * x) + 2
            var tVal = t < 0.5f ? 16f * t * t * t * t * t : 1f - ((tI2v * tI2v * tI2v * tI2v * tI2v) / 2f);
            return tVal;
        }
        private static float BounceIn(float t)
        {
            var tVal = 1f - BounceOut(1f - t);
            return tVal;
        }
        private static float BounceOut(float t)
        {
            var tVal = t < 0.363636374f ? 7.5625f * t * t : t < 0.727272749f ? (7.5625f * (t -= 0.545454562f) * t) + 0.75f : t < 0.909090936f ? (7.5625f * (t -= 0.8181818f) * t) + 0.9375f : (7.5625f * (t -= 21f / 22f) * t) + (63f / 64f);
            return tVal;
        }
        private static float BounceInOut(float t)
        {
            var tVal = t < 0.5f ? BounceIn(t * 2f) * 0.5f : (BounceOut((t * 2f) - 1f) * 0.5f) + 0.5f;
            return tVal;
        }
        private static float ElasticIn(float t)
        {
            var tVal = -(Mathf.Pow(2, 10 * (t -= 1)) * Mathf.Sin((t - (0.3f / 4f)) * (2 * Mathf.PI) / 0.3f));
            return tVal;
        }
        private static float ElasticOut(float t)
        {
            var tVal = t == 1f ? 1f : 1f - ElasticIn(1f - t);
            return tVal;
        }
        private static float ElasticInOut(float t)
        {
            var tVal = (t *= 2f) == 2f ? 1f : t < 1f ? -0.5f * (Mathf.Pow(2f, 10f * (t -= 1)) * Mathf.Sin((t - 0.1125f) * (2f * Mathf.PI) / 0.45f)) :
                ((Mathf.Pow(2f, -10f * (t -= 1f)) * Mathf.Sin((t - 0.1125f) * (2f * Mathf.PI) / 0.45f) * 0.5f) + 1f);
            return tVal;
        }
        private static float CircularIn(float t)
        {
            var tVal = -(Mathf.Sqrt(1 - (t * t)) - 1);
            return tVal;
        }
        private static float CircularOut(float t)
        {
            var tVal = Mathf.Sqrt(1f - ((t -= 1f) * t));
            return tVal;
        }
        private static float CircularInOut(float t)
        {
            var tVal = (t *= 2f) < 1f ? -1f / 2f * (Mathf.Sqrt(1f - (t * t)) - 1f) : 0.5f * (Mathf.Sqrt(1 - ((t -= 2) * t)) + 1);
            return tVal;
        }
        private static float SinusIn(float t)
        {
            var tVal = -Mathf.Cos(t * (Mathf.PI * 0.5f)) + 1f;
            return tVal;
        }
        private static float SinusOut(float t)
        {
            var tVal = Mathf.Sin(t * (Mathf.PI * 0.5f));
            return tVal;
        }
        private static float SinusInOut(float t)
        {
            var tVal = -0.5f * (Mathf.Cos(Mathf.PI * t) - 1f);
            return tVal;
        }
        private static float ExponentialIn(float t)
        {
            var tVal = Mathf.Pow(2f, 10f * (t - 1f));
            return tVal;
        }
        private static float ExponentialOut(float t)
        {
            var tVal = Mathf.Sin(t * (Mathf.PI * 0.5f));
            return tVal;
        }
        private static float ExponentialInOut(float t)
        {
            var tVal = -0.5f * (Mathf.Cos(Mathf.PI * t) - 1f);
            return tVal;
        }
        #endregion
    }
}
