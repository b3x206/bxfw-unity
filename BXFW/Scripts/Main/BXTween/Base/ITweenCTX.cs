using BXFW.Tweening.Events;

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
        /// Duration of the tween context.
        /// </summary>
        public float Duration { get; }
        /// <summary>
        /// The delay to wait for when <see cref="StartTween"/> is called.
        /// </summary>
        public float Delay { get; }
        /// <summary>
        /// Amount that this tween will repeat.
        /// </summary>
        public int RepeatAmount { get; }
        /// <summary>
        /// Whether if the tween is running.
        /// </summary>
        public bool IsRunning { get; }
        /// <summary>
        /// Called when the tween ends.
        /// </summary>
        public event BXTweenMethod TweenCompleteAction;
        /// <summary>
        /// Clears the interface <see cref="TweenCompleteAction"/>.
        /// </summary>
        public void ClearCompleteAction();

        /// <summary>
        /// Target object of the tween.
        /// <br>It is <i>recommended</i> for this to be assigned to a valid object.</br>
        /// </summary>
        UnityEngine.Object TargetObject { get; }

        /// <summary>
        /// Stop the tween that is under this context.
        /// </summary>
        void StopTween();

        /// <summary>
        /// Start the tween that is under this context.
        /// </summary>
        void StartTween();
    }
}
