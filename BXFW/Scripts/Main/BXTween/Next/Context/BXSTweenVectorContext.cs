using System;
using UnityEngine;
using BXFW.Tweening.Next.Events;

namespace BXFW.Tweening.Next
{
    /// <summary>
    /// Contains a context that uses Vector2.
    /// </summary>
    [Serializable]
    public sealed class BXSTweenVector2Context : BXSTweenContext<Vector2>
    {
        public override BXSLerpAction<Vector2> LerpAction => Vector2.LerpUnclamped;
        
        /// <summary>
        /// Makes a blank context. Has no duration or anything.
        /// </summary>
        public BXSTweenVector2Context()
        { }
        /// <inheritdoc cref="BXSTweenVector2Context(float, float, int, EaseType, float)"/>
        public BXSTweenVector2Context(float duration)
        {
            SetDuration(duration);
        }
        /// <inheritdoc cref="BXSTweenVector2Context(float, float, int, EaseType, float)"/>
        public BXSTweenVector2Context(float duration, float delay)
        {
            SetDuration(duration).SetDelay(delay);
        }
        /// <inheritdoc cref="BXSTweenVector2Context(float, float, int, EaseType, float)"/>
        public BXSTweenVector2Context(float duration, float delay, int loopCount)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount);
        }
        /// <inheritdoc cref="BXSTweenVector2Context(float, float, int, EaseType, float)"/>
        public BXSTweenVector2Context(float duration, float delay, int loopCount, float speed)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetSpeed(speed);
        }
        /// <inheritdoc cref="BXSTweenVector2Context(float, float, int, EaseType, float)"/>
        public BXSTweenVector2Context(float duration, float delay, int loopCount, EaseType easing)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetEase(easing, true);
        }
        /// <summary>
        /// Makes a <see cref="BXSTweenVector2Context"/> with predefined settings.
        /// </summary>
        public BXSTweenVector2Context(float duration, float delay, int loopCount, EaseType easing, float speed)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetEase(easing, true).SetSpeed(speed);
        }
    }

    /// <summary>
    /// Contains a context that uses Vector3.
    /// </summary>
    [Serializable]
    public sealed class BXSTweenVector3Context : BXSTweenContext<Vector3>
    {
        public override BXSLerpAction<Vector3> LerpAction => Vector3.LerpUnclamped;

        /// <summary>
        /// Makes a blank context. Has no duration or anything.
        /// </summary>
        public BXSTweenVector3Context()
        { }
        /// <inheritdoc cref="BXSTweenVector3Context(float, float, int, EaseType, float)"/>
        public BXSTweenVector3Context(float duration)
        {
            SetDuration(duration);
        }
        /// <inheritdoc cref="BXSTweenVector3Context(float, float, int, EaseType, float)"/>
        public BXSTweenVector3Context(float duration, float delay)
        {
            SetDuration(duration).SetDelay(delay);
        }
        /// <inheritdoc cref="BXSTweenVector3Context(float, float, int, EaseType, float)"/>
        public BXSTweenVector3Context(float duration, float delay, int loopCount)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount);
        }
        /// <inheritdoc cref="BXSTweenVector3Context(float, float, int, EaseType, float)"/>
        public BXSTweenVector3Context(float duration, float delay, int loopCount, float speed)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetSpeed(speed);
        }
        /// <inheritdoc cref="BXSTweenVector3Context(float, float, int, EaseType, float)"/>
        public BXSTweenVector3Context(float duration, float delay, int loopCount, EaseType easing)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetEase(easing, true);
        }
        /// <summary>
        /// Makes a <see cref="BXSTweenVector3Context"/> with predefined settings.
        /// </summary>
        public BXSTweenVector3Context(float duration, float delay, int loopCount, EaseType easing, float speed)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetEase(easing, true).SetSpeed(speed);
        }
    }

    /// <summary>
    /// Contains a context that uses Vector4.
    /// </summary>
    [Serializable]
    public sealed class BXSTweenVector4Context : BXSTweenContext<Vector4>
    {
        public override BXSLerpAction<Vector4> LerpAction => Vector4.LerpUnclamped;
        
        /// <summary>
        /// Makes a blank context. Has no duration or anything.
        /// </summary>
        public BXSTweenVector4Context()
        { }
        /// <inheritdoc cref="BXSTweenVector4Context(float, float, int, EaseType, float)"/>
        public BXSTweenVector4Context(float duration)
        {
            SetDuration(duration);
        }
        /// <inheritdoc cref="BXSTweenVector4Context(float, float, int, EaseType, float)"/>
        public BXSTweenVector4Context(float duration, float delay)
        {
            SetDuration(duration).SetDelay(delay);
        }
        /// <inheritdoc cref="BXSTweenVector4Context(float, float, int, EaseType, float)"/>
        public BXSTweenVector4Context(float duration, float delay, int loopCount)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount);
        }
        /// <inheritdoc cref="BXSTweenVector4Context(float, float, int, EaseType, float)"/>
        public BXSTweenVector4Context(float duration, float delay, int loopCount, float speed)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetSpeed(speed);
        }
        /// <inheritdoc cref="BXSTweenVector4Context(float, float, int, EaseType, float)"/>
        public BXSTweenVector4Context(float duration, float delay, int loopCount, EaseType easing)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetEase(easing, true);
        }
        /// <summary>
        /// Makes a <see cref="BXSTweenVector4Context"/> with predefined settings.
        /// </summary>
        public BXSTweenVector4Context(float duration, float delay, int loopCount, EaseType easing, float speed)
        {
            SetDuration(duration).SetDelay(delay).SetLoopCount(loopCount).SetEase(easing, true).SetSpeed(speed);
        }
    }
}
