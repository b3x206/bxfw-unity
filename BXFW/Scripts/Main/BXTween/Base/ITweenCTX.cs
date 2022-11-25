using System;

namespace BXFW.Tweening
{
    /// <summary>
    /// Generic tween interface. Used for storing tweens in a generic agnostic way.
    /// <br>When the tween is done (on stop), the tween is removed from the list of the stored tweens.</br>
    /// <br>Tweens are not pooled currently. (low priority, the max amount of tweens i run is like, 10 anyways)</br>
    /// </summary>
    public interface ITweenCTX
    {
        /// <summary>
        /// Target object of the tween.
        /// <br>It is <c>recommended</c> for this to be assigned to a valid object.</br>
        /// </summary>
        UnityEngine.Object TargetObject { get; }

        /// <summary>
        /// Start the tween that is under this context.
        /// </summary>
        void StartTween();

        /// <summary>
        /// Stop the tween that is under this context.
        /// </summary>
        void StopTween();
    }
}
