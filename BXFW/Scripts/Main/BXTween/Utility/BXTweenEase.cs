using UnityEngine;
using BXFW.Tweening.Events;
using System.Collections.Generic;
using BXFW.Tweening.Next.Events;

namespace BXFW.Tweening
{
    /// <summary>
    /// Easing type of the tweenings, the easing functions are gathered from the <see cref="BXTweenEase"/>.
    /// <br>
    /// You can see an approximation curve in any <see cref="EaseType"/> property selector or look at the website https://easings.net/
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
    /// To create custom ease curves you can use the <see cref="AnimationCurve"/>.
    /// </summary>
    public static class BXTweenEase
    {
        /// This generates garbage when the initial BXTween is done.
        /// <br>To avoid this, there are several approaches;</br>
        /// <br>A : Use a switch statement : this has the inefficiency of not being dictionary and the switch statement has to be evaluated always</br>
        /// <br>However it is probably a no-alloc thing and it will be similar + custom easings could be simpler and more ergonomic.</br>
        /// <br>Or B : This probably doesn't generate garbage in il2cpp</br>
        /// <br>C : Use a capacity as the { } constructor probably doesn't allocate capacity by the compiler.
        /// (this cut off 1kb of garbage, larger capacities brought back the 1kb garbage)</br>
        /// <br>Or D : Access 'BXTweenEase' on the IBXSTweenRunner when the application launches? (This cut off 5kb garbage when a new tween runs)</br>
        /// Or E : DONT USE A DICTIONARY (It's not a good hashmap/whatever the hell it's internally is)
        /// <summary>
        /// All hardcoded ease methods in a dictionary.
        /// <br>All values from the <see cref="EaseType"/> enumeration exists in here.</br>
        /// </summary>
        public static readonly IReadOnlyDictionary<EaseType, BXTweenEaseSetMethod> LegacyMethods = new Dictionary<EaseType, BXTweenEaseSetMethod>(28)
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

        ///// <summary>
        ///// Returns a eased in value.
        ///// </summary>
        //public static float EasedValue(float time, EaseType easing)
        //{
        //    return GetEaseMethod(easing)(time);
        //}

        // Dictionary is slower than a switch statement
        /// <summary>
        /// Get the easing method (as <see cref="BXSEaseAction"/>).
        /// <br>Cache the resulting method for better performance, but this is faster than the dictionary method.</br>
        /// </summary>
        /// <param name="easing">Easing to get it's corresponding float interpolation method.</param>
        /// <returns>
        /// The resulting ease method according to <paramref name="easing"/>. 
        /// The result is never null and invalid <paramref name="easing"/> values are always treated as <see cref="EaseType.Linear"/>.
        /// </returns>
        public static BXSEaseAction GetEaseMethod(EaseType easing)
        {
            // Generated using python, probably took longer than hand writing it but idc lol
            switch (easing)
            {
                case EaseType.QuadIn:
                    return QuadIn;
                case EaseType.QuadOut:
                    return QuadOut;
                case EaseType.QuadInOut:
                    return QuadInOut;
                case EaseType.CubicIn:
                    return CubicIn;
                case EaseType.CubicOut:
                    return CubicOut;
                case EaseType.CubicInOut:
                    return CubicInOut;
                case EaseType.QuartIn:
                    return QuartIn;
                case EaseType.QuartOut:
                    return QuartOut;
                case EaseType.QuartInOut:
                    return QuartInOut;
                case EaseType.QuintIn:
                    return QuintIn;
                case EaseType.QuintOut:
                    return QuintOut;
                case EaseType.QuintInOut:
                    return QuintInOut;
                case EaseType.BounceIn:
                    return BounceIn;
                case EaseType.BounceOut:
                    return BounceOut;
                case EaseType.BounceInOut:
                    return BounceInOut;
                case EaseType.ElasticIn:
                    return ElasticIn;
                case EaseType.ElasticOut:
                    return ElasticOut;
                case EaseType.ElasticInOut:
                    return ElasticInOut;
                case EaseType.CircularIn:
                    return CircularIn;
                case EaseType.CircularOut:
                    return CircularOut;
                case EaseType.CircularInOut:
                    return CircularInOut;
                case EaseType.SinusIn:
                    return SinusIn;
                case EaseType.SinusOut:
                    return SinusOut;
                case EaseType.SinusInOut:
                    return SinusInOut;
                case EaseType.ExponentialIn:
                    return ExponentialIn;
                case EaseType.ExponentialOut:
                    return ExponentialOut;
                case EaseType.ExponentialInOut:
                    return ExponentialInOut;

                default:
                case EaseType.Linear:
                    return Linear;
            }
        }

        /// <summary>
        /// Returns a eased in value.
        /// </summary>
        /// The inline version for getting the eased value, without any delegate stuff.
        /// Because the delegate stuff allocates garbage and i have enough garbage in my pc such as my code lol.
        public static float EasedValue(float time, EaseType easing)
        {
            // PYTHON BLYAT, thanks for having 73 different method names
            switch (easing)
            {
                default:
                case EaseType.Linear:
                    {
                        float tVal = time;
                        return tVal;
                    }
                case EaseType.QuadIn:
                    {
                        float tVal = time * time;
                        return tVal;
                    }
                case EaseType.QuadOut:
                    {
                        float tVal = time * (2f - time);
                        return tVal;
                    }
                case EaseType.QuadInOut:
                    {
                        float tVal = time < 0.5f ? 2f * time * time : -1f + ((4f - (2f * time)) * time);
                        return tVal;
                    }
                case EaseType.CubicIn:
                    {
                        float tVal = time * time * time;
                        return tVal;
                    }
                case EaseType.CubicOut:
                    {
                        float tSub = 1f - time;
                        float tVal = 1f - (tSub * tSub * tSub);
                        return tVal;
                    }
                case EaseType.CubicInOut:
                    {
                        float tVal = time < 0.5f ? 4f * time * time * time : ((time - 1f) * ((2f * time) - 2f) * ((2 * time) - 2)) + 1f;
                        return tVal;
                    }
                case EaseType.QuartIn:
                    {
                        float tVal = time * time * time * time;
                        return tVal;
                    }
                case EaseType.QuartOut:
                    {
                        float tInv = 1f - time; // inverted t (assuming t = clamped between 0-1)
                        float tVal = 1f - (tInv * tInv * tInv * tInv);
                        return tVal;
                    }
                case EaseType.QuartInOut:
                    {
                        float tI2v = (-2 * time) + 2; // (-2 * x) + 2
                        float tVal = time < 0.5f ? 8f * time * time * time * time : 1f - ((tI2v * tI2v * tI2v * tI2v) / 2f);
                        return tVal;
                    }
                case EaseType.QuintIn:
                    {
                        float tVal = time * time * time * time * time;
                        return tVal;
                    }
                case EaseType.QuintOut:
                    {
                        float tInv = 1f - time; // inverted t (assuming t = clamped between 0-1)
                        float tVal = 1f - (tInv * tInv * tInv * tInv * tInv);
                        return tVal;
                    }
                case EaseType.QuintInOut:
                    {
                        float tI2v = (-2 * time) + 2; // (-2 * x) + 2
                        float tVal = time < 0.5f ? 16f * time * time * time * time * time : 1f - ((tI2v * tI2v * tI2v * tI2v * tI2v) / 2f);
                        return tVal;
                    }
                case EaseType.BounceIn:
                    {
                        float tVal = 1f - BounceOut(1f - time);
                        return tVal;
                    }
                case EaseType.BounceOut:
                    {
                        float tVal = time < 0.363636374f ? 7.5625f * time * time : time < 0.727272749f ? (7.5625f * (time -= 0.545454562f) * time) + 0.75f : time < 0.909090936f ? (7.5625f * (time -= 0.8181818f) * time) + 0.9375f : (7.5625f * (time -= 21f / 22f) * time) + (63f / 64f);
                        return tVal;
                    }
                case EaseType.BounceInOut:
                    {
                        float tVal = time < 0.5f ? BounceIn(time * 2f) * 0.5f : (BounceOut((time * 2f) - 1f) * 0.5f) + 0.5f;
                        return tVal;
                    }
                case EaseType.ElasticIn:
                    {
                        float tVal = -(Mathf.Pow(2, 10 * (time -= 1)) * Mathf.Sin((time - (0.3f / 4f)) * (2 * Mathf.PI) / 0.3f));
                        return tVal;
                    }
                case EaseType.ElasticOut:
                    {
                        float tVal = time == 1f ? 1f : 1f - ElasticIn(1f - time);
                        return tVal;
                    }
                case EaseType.ElasticInOut:
                    {
                        float tVal = (time *= 2f) == 2f ? 
                            1f : time < 1f ? -0.5f * (Mathf.Pow(2f, 10f * (time -= 1)) * Mathf.Sin((time - 0.1125f) * (2f * Mathf.PI) / 0.45f))
                            : ((Mathf.Pow(2f, -10f * (time -= 1f)) * Mathf.Sin((time - 0.1125f) * (2f * Mathf.PI) / 0.45f) * 0.5f) + 1f);
                        return tVal;
                    }
                case EaseType.CircularIn:
                    {
                        float tVal = -(Mathf.Sqrt(1 - (time * time)) - 1);
                        return tVal;
                    }
                case EaseType.CircularOut:
                    {
                        float tVal = Mathf.Sqrt(1f - ((time -= 1f) * time));
                        return tVal;
                    }
                case EaseType.CircularInOut:
                    {
                        float tVal = (time *= 2f) < 1f ? -1f / 2f * (Mathf.Sqrt(1f - (time * time)) - 1f) : 0.5f * (Mathf.Sqrt(1 - ((time -= 2) * time)) + 1);
                        return tVal;
                    }
                case EaseType.SinusIn:
                    {
                        float tVal = -Mathf.Cos(time * (Mathf.PI * 0.5f)) + 1f;
                        return tVal;
                    }
                case EaseType.SinusOut:
                    {
                        float tVal = Mathf.Sin(time * (Mathf.PI * 0.5f));
                        return tVal;
                    }
                case EaseType.SinusInOut:
                    {
                        float tVal = -0.5f * (Mathf.Cos(Mathf.PI * time) - 1f);
                        return tVal;
                    }
                case EaseType.ExponentialIn:
                    {
                        float tVal = Mathf.Pow(2f, 10f * (time - 1f));
                        return tVal;
                    }
                case EaseType.ExponentialOut:
                    {
                        float tVal = Mathf.Sin(time * (Mathf.PI * 0.5f));
                        return tVal;
                    }
                case EaseType.ExponentialInOut:
                    {
                        float tVal = -0.5f * (Mathf.Cos(Mathf.PI * time) - 1f);
                        return tVal;
                    }
            }
        }

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
