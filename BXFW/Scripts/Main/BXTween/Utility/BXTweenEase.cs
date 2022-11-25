using UnityEngine;
using BXFW.Tweening.Events;
using System.Collections.Generic;

namespace BXFW.Tweening
{
    /// <summary>
    /// Ease type.
    /// </summary>
    /// See this website explaining ease types : https://easings.net/
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
    /// To create custom ease types use the <see cref="AnimationCurve"/>. (in BXTween context field : <see cref="BXTweenCTX{T}.SetCustomCurve(AnimationCurve, bool)"/>.
    /// </summary>
    public static class BXTweenEase
    {
        /// <summary>
        /// All hardcoded ease methods in a hashmap.
        /// </summary>
        public static readonly IReadOnlyDictionary<EaseType, BXTweenEaseSetMethod> EaseMethods = new Dictionary<EaseType, BXTweenEaseSetMethod>
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
        // Note : All ease methods change between -Infinity~Infinity.
        // Clamping is done by setting a bool.
        private static float Linear(float t, bool clamped = true)
        {
            var tVal = t;
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float QuadIn(float t, bool clamped = true)
        {
            var tVal = t * t;
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float QuadOut(float t, bool clamped = true)
        {
            var tVal = t * (2f - t);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float QuadInOut(float t, bool clamped = true)
        {
            var tVal = t < 0.5f ? 2f * t * t : -1f + ((4f - (2f * t)) * t);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float CubicIn(float t, bool clamped = true)
        {
            var tVal = t * t * t;
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float CubicOut(float t, bool clamped = true)
        {
            var tVal = ((t - 1f) * t * t) + 1f;
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float CubicInOut(float t, bool clamped = true)
        {
            var tVal = t < 0.5f ? 4f * t * t * t : ((t - 1f) * ((2f * t) - 2f) * ((2 * t) - 2)) + 1f;
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float QuartIn(float t, bool clamped = true)
        {
            var tVal = t * t * t * t;
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float QuartOut(float t, bool clamped = true)
        {
            var tVal = 1f - ((t - 1f) * t * t * t);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float QuartInOut(float t, bool clamped = true)
        {
            var tVal = t < 0.5f ? 8f * t * t * t * t : 1f - (8f * (t - 1f) * t * t * t);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float QuintIn(float t, bool clamped = true)
        {
            var tVal = t * t * t * t * t;
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float QuintOut(float t, bool clamped = true)
        {
            var tVal = 1f + ((t - 1f) * t * t * t * t);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float QuintInOut(float t, bool clamped = true)
        {
            var tVal = t < 0.5f ? 16f * t * t * t * t * t : 1f + (16f * (t - 1f) * t * t * t * t);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float BounceIn(float t, bool clamped = true)
        {
            var tVal = 1f - BounceOut(1f - t);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float BounceOut(float t, bool clamped = true)
        {
            var tVal = t < 0.363636374f ? 7.5625f * t * t : t < 0.727272749f ? (7.5625f * (t -= 0.545454562f) * t) + 0.75f : t < 0.909090936f ? (7.5625f * (t -= 0.8181818f) * t) + 0.9375f : (7.5625f * (t -= 21f / 22f) * t) + (63f / 64f);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float BounceInOut(float t, bool clamped = true)
        {
            var tVal = t < 0.5f ? BounceIn(t * 2f) * 0.5f : (BounceOut((t * 2f) - 1f) * 0.5f) + 0.5f;
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float ElasticIn(float t, bool clamped = true)
        {
            var tVal = -(Mathf.Pow(2, 10 * (t -= 1)) * Mathf.Sin((t - (0.3f / 4f)) * (2 * Mathf.PI) / 0.3f));
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float ElasticOut(float t, bool clamped = true)
        {
            var tVal = t == 1f ? 1f : 1f - ElasticIn(1f - t);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float ElasticInOut(float t, bool clamped = true)
        {
            var tVal = (t *= 2f) == 2f ? 1f : t < 1f ? -0.5f * (Mathf.Pow(2f, 10f * (t -= 1)) * Mathf.Sin((t - 0.1125f) * (2f * Mathf.PI) / 0.45f)) :
                ((Mathf.Pow(2f, -10f * (t -= 1f)) * Mathf.Sin((t - 0.1125f) * (2f * Mathf.PI) / 0.45f) * 0.5f) + 1f);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float CircularIn(float t, bool clamped = true)
        {
            var tVal = -(Mathf.Sqrt(1 - (t * t)) - 1);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float CircularOut(float t, bool clamped = true)
        {
            var tVal = Mathf.Sqrt(1f - ((t -= 1f) * t));
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float CircularInOut(float t, bool clamped = true)
        {
            var tVal = (t *= 2f) < 1f ? -1f / 2f * (Mathf.Sqrt(1f - (t * t)) - 1f) : 0.5f * (Mathf.Sqrt(1 - ((t -= 2) * t)) + 1);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float SinusIn(float t, bool clamped = true)
        {
            var tVal = -Mathf.Cos(t * (Mathf.PI * 0.5f)) + 1f;
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float SinusOut(float t, bool clamped = true)
        {
            var tVal = Mathf.Sin(t * (Mathf.PI * 0.5f));
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float SinusInOut(float t, bool clamped = true)
        {
            var tVal = -0.5f * (Mathf.Cos(Mathf.PI * t) - 1f);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float ExponentialIn(float t, bool clamped = true)
        {
            var tVal = Mathf.Pow(2f, 10f * (t - 1f));
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float ExponentialOut(float t, bool clamped = true)
        {
            var tVal = Mathf.Sin(t * (Mathf.PI * 0.5f));
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        private static float ExponentialInOut(float t, bool clamped = true)
        {
            var tVal = -0.5f * (Mathf.Cos(Mathf.PI * t) - 1f);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        #endregion
    }
}
