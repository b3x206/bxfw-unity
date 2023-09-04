using System;
using UnityEngine;
using BXFW.Tweening.Next.Events;

namespace BXFW.Tweening.Next
{
    /// <summary>
    /// Contains a context that uses Color.
    /// </summary>
    [Serializable]
    public sealed class BXSTweenColorContext : BXSTweenContext<Color>
    {
        public override BXSLerpAction<Color> LerpAction => Color.LerpUnclamped;
        public override BXSMathAction<Color> AddValueAction => (Color lhs, Color rhs) => lhs + rhs;

        /// <summary>
        /// Makes a blank context. Has no duration or anything.
        /// </summary>
        public BXSTweenColorContext()
        { }
        /// <inheritdoc cref="BXSTweenColorContext(float, float, int, EaseType, float)"/>
        public BXSTweenColorContext(float duration)
        {
            SetDuration(duration);
        }
        /// <inheritdoc cref="BXSTweenColorContext(float, float, int, EaseType, float)"/>
        public BXSTweenColorContext(float duration, float delay)
        {
            SetDuration(duration).SetDelay(delay);
        }
        /// <inheritdoc cref="BXSTweenColorContext(float, float, int, EaseType, float)"/>
        public BXSTweenColorContext(float duration, float delay, int loopCount)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount);
        }
        /// <inheritdoc cref="BXSTweenColorContext(float, float, int, EaseType, float)"/>
        public BXSTweenColorContext(float duration, float delay, int loopCount, float speed)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetSpeed(speed);
        }
        /// <inheritdoc cref="BXSTweenColorContext(float, float, int, EaseType, float)"/>
        public BXSTweenColorContext(float duration, float delay, int loopCount, EaseType easing)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetEase(easing, true);
        }
        /// <summary>
        /// Makes a <see cref="BXSTweenColorContext"/> with predefined settings.
        /// </summary>
        public BXSTweenColorContext(float duration, float delay, int loopCount, EaseType easing, float speed)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetEase(easing, true).SetSpeed(speed);
        }
    }
}
