using System;
using UnityEngine;

namespace BXFW.Tweening.Next
{
    /// <summary>
    /// Contains a context that uses Matrix4x4.
    /// </summary>
    [Serializable]
    public sealed class BXSTweenMatrix4x4Context : BXSTweenContext<Matrix4x4>
    {
        public override Matrix4x4 LerpMethod(Matrix4x4 a, Matrix4x4 b, float time)
        {
            return MathUtility.LerpUnclamped(a, b, time);
        }

        /// <summary>
        /// Makes a blank context. Has no duration or anything.
        /// </summary>
        public BXSTweenMatrix4x4Context()
        { }
        /// <inheritdoc cref="BXSTweenMatrix4x4Context(float, float, int, EaseType, float)"/>
        public BXSTweenMatrix4x4Context(float duration)
        {
            SetDuration(duration);
        }
        /// <inheritdoc cref="BXSTweenMatrix4x4Context(float, float, int, EaseType, float)"/>
        public BXSTweenMatrix4x4Context(float duration, float delay)
        {
            SetDuration(duration).SetDelay(delay);
        }
        /// <inheritdoc cref="BXSTweenMatrix4x4Context(float, float, int, EaseType, float)"/>
        public BXSTweenMatrix4x4Context(float duration, float delay, int loopCount)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount);
        }
        /// <inheritdoc cref="BXSTweenMatrix4x4Context(float, float, int, EaseType, float)"/>
        public BXSTweenMatrix4x4Context(float duration, float delay, int loopCount, float speed)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetSpeed(speed);
        }
        /// <inheritdoc cref="BXSTweenMatrix4x4Context(float, float, int, EaseType, float)"/>
        public BXSTweenMatrix4x4Context(float duration, float delay, int loopCount, EaseType easing)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetEase(easing, true);
        }
        /// <summary>
        /// Makes a <see cref="BXSTweenMatrix4x4Context"/> with predefined settings.
        /// </summary>
        public BXSTweenMatrix4x4Context(float duration, float delay, int loopCount, EaseType easing, float speed)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetEase(easing, true).SetSpeed(speed);
        }
    }
}
