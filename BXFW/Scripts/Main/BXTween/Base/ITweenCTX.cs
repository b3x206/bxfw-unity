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
        // -- Tween
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
        /// Repeat type of this tween.
        /// </summary>
        public RepeatType RepeatType { get; }
        /// <summary>
        /// Whether if the tween is running.
        /// </summary>
        public bool IsRunning { get; }
        /// <summary>
        /// Type of the easing in this tween.
        /// </summary>
        public EaseType Easing { get; }
        /// <summary>
        /// The elapsed value of the tween context.
        /// </summary>
        public float CurrentElapsed { get; }
        /// <summary>
        /// Whether to ignore the timescale.
        /// </summary>
        public bool IgnoreTimeScale { get; }
        /// <summary>
        /// Target object of the tween.
        /// <br>It is <i>recommended</i> for this to be assigned to a valid object.</br>
        /// </summary>
        UnityEngine.Object TargetObject { get; }

        // -- Event
        /// <summary>
        /// Called when the tween starts.
        /// </summary>
        public BXTweenMethod OnStartAction { get; set; }
        /// <summary>
        /// Called when the tween ends.
        /// </summary>
        public BXTweenMethod OnEndAction { get; set; }

        /// <summary>
        /// Since BXTween is not based on a time evaluation system, this is a variable used for a bad workaround.
        /// <br>BXSTween solves this, however only the older commits may be compatible with the bxfw-legacy branch..</br>
        /// </summary>
        public BXTweenMethod SequenceOnEndAction { get; set; }

        /// <summary>
        /// Clears the interface <see cref="OnStartAction"/>.
        /// </summary>
        public void ClearStartingEvents();
        /// <summary>
        /// Clears the interface <see cref="OnEndAction"/>.
        /// </summary>
        public void ClearEndingEvents();

        /// <summary>
        /// Stop the tween that is under this context.
        /// </summary>
        public void StopTween();

        /// <summary>
        /// Start the tween that is under this context.
        /// </summary>
        public void StartTween();
    }
}
