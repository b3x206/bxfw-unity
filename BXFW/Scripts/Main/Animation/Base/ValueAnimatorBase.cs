using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Contains a base for a value animator.
    /// <br>Used for matching the inspector and containing the playing methods.</br>
    /// </summary>
    public abstract class ValueAnimatorBase : MonoBehaviour
    {
        /// <summary>
        /// Defines a sequence of values.
        /// </summary>
        [Serializable]
        public abstract class Sequence
        {
            /// <summary>
            /// Name of the animation.
            /// </summary>
            public string name = "None";
            /// <summary>
            /// Milliseconds to wait to update the frame.
            /// <br>This also changes the speed.</br>
            /// </summary>
            public float frameMS = .040f; // Default value is 25 fps
            /// <summary>
            /// Whether if the animation should loop.
            /// </summary>
            public bool loop = false;

            /// <summary>
            /// Size of the frames of this sequence.
            /// </summary>
            public abstract int FrameCount { get; }
            /// <summary>
            /// Duration of this sequence.
            /// </summary>
            public virtual float Duration => FrameCount * frameMS;
        }

        /// <summary>
        /// Currently selected animation's index.
        /// </summary>
        public abstract int CurrentAnimIndex { get; set; }
        /// <summary>
        /// Size of the animations.
        /// </summary>
        public abstract int AnimationCount { get; }

        /// <summary>
        /// Plays the current contained animation.
        /// </summary>
        public abstract void Play();
        /// <summary>
        /// Plays the animation with given <paramref name="id"/>.
        /// </summary>
        public abstract void Play(string id);
        /// <summary>
        /// Pauses the currently playing animation.
        /// </summary>
        public abstract void Pause();
        /// <summary>
        /// Stops the 
        /// </summary>
        public abstract void Stop();
    }
}
