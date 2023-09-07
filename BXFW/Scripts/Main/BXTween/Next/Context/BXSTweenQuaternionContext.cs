using System;
using UnityEngine;
using BXFW.Tweening.Next.Events;

namespace BXFW.Tweening.Next
{
    /// <summary>
    /// Contains a context that uses Quaternion.
    /// </summary>
    [Serializable]
    public sealed class BXSTweenQuaternionContext : BXSTweenContext<Quaternion>
    {
        public override BXSLerpAction<Quaternion> LerpAction => Quaternion.SlerpUnclamped;
        public override BXSMathAction<Quaternion> AddValueAction => (Quaternion lhs, Quaternion rhs) => lhs * rhs;

        /// <summary>
        /// Makes a blank context. Has no duration or anything.
        /// </summary>
        public BXSTweenQuaternionContext()
        { }
        /// <inheritdoc cref="BXSTweenQuaternionContext(float, float, int, EaseType, float)"/>
        public BXSTweenQuaternionContext(float duration)
        {
            SetDuration(duration);
        }
        /// <inheritdoc cref="BXSTweenQuaternionContext(float, float, int, EaseType, float)"/>
        public BXSTweenQuaternionContext(float duration, float delay)
        {
            SetDuration(duration).SetDelay(delay);
        }
        /// <inheritdoc cref="BXSTweenQuaternionContext(float, float, int, EaseType, float)"/>
        public BXSTweenQuaternionContext(float duration, float delay, int loopCount)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount);
        }
        /// <inheritdoc cref="BXSTweenQuaternionContext(float, float, int, EaseType, float)"/>
        public BXSTweenQuaternionContext(float duration, float delay, int loopCount, float speed)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetSpeed(speed);
        }
        /// <inheritdoc cref="BXSTweenQuaternionContext(float, float, int, EaseType, float)"/>
        public BXSTweenQuaternionContext(float duration, float delay, int loopCount, EaseType easing)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetEase(easing, true);
        }
        /// <summary>
        /// Makes a <see cref="BXSTweenQuaternionContext"/> with predefined settings.
        /// </summary>
        public BXSTweenQuaternionContext(float duration, float delay, int loopCount, EaseType easing, float speed)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetEase(easing, true).SetSpeed(speed);
        }
    }
}