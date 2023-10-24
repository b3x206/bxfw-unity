using System;

namespace BXFW.Tweening.Next
{
    /// <summary>
    /// Contains a context that uses floats.
    /// </summary>
    [Serializable]
    public sealed class BXSTweenFloatContext : BXSTweenContext<float>
    {
        public override float LerpMethod(float a, float b, float time)
        {
            return a + ((b - a) * time);
        }

        /// <summary>
        /// Makes a blank context. Has no duration or anything.
        /// </summary>
        public BXSTweenFloatContext()
        { }
        /// <inheritdoc cref="BXSTweenFloatContext(float, float, int, EaseType, float)"/>
        public BXSTweenFloatContext(float duration)
        {
            SetDuration(duration);
        }
        /// <inheritdoc cref="BXSTweenFloatContext(float, float, int, EaseType, float)"/>
        public BXSTweenFloatContext(float duration, float delay)
        {
            SetDuration(duration).SetDelay(delay);
        }
        /// <inheritdoc cref="BXSTweenFloatContext(float, float, int, EaseType, float)"/>
        public BXSTweenFloatContext(float duration, float delay, int loopCount)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount);
        }
        /// <inheritdoc cref="BXSTweenFloatContext(float, float, int, EaseType, float)"/>
        public BXSTweenFloatContext(float duration, float delay, int loopCount, float speed)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetSpeed(speed);
        }
        /// <inheritdoc cref="BXSTweenFloatContext(float, float, int, EaseType, float)"/>
        public BXSTweenFloatContext(float duration, float delay, int loopCount, EaseType easing)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetEase(easing, true);
        }
        /// <summary>
        /// Makes a <see cref="BXSTweenFloatContext"/> with predefined settings.
        /// </summary>
        public BXSTweenFloatContext(float duration, float delay, int loopCount, EaseType easing, float speed)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetEase(easing, true).SetSpeed(speed);
        }
    }
}
