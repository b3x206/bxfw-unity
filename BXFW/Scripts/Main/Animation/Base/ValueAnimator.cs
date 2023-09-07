using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BXFW
{
    /// <summary>
    /// Animates a value without holding grudges to the animated values or it's initial values.
    /// <br>Only sets the values of an animation while <see cref="IsPlaying"/>.</br>
    /// </summary>
    /// <typeparam name="TValue">The type of value to animate. This is often preferred to be a value of a target component.</typeparam>
    public abstract class ValueAnimator<TValue> : ValueAnimatorBase
    {
        /// <summary>
        /// A value animation sequence used to animate values.
        /// </summary>
        [Serializable]
        public class ValueAnim : Sequence
        {
            public override int FrameCount => valueFrames == null ? 0 : valueFrames.Length;

            /// <summary>
            /// Animation frames of given animator.
            /// </summary>
            public TValue[] valueFrames;
            /// <summary>
            /// Get a value at index shorthand.
            /// </summary>
            public TValue this[int index]
            {
                get => valueFrames[index];
                set => valueFrames[index] = value;
            }

            public override void Clear()
            {
                valueFrames = new TValue[0];
            }
        }

        // -- Settings
        [Header(":: Animation")]
        [SerializeField] private int m_CurrentAnimIndex = 0;
        /// <summary>
        /// Index of the current animation assigned to be played.
        /// </summary>
        public override int CurrentAnimIndex
        {
            get { return animations.Length <= 0 ? -1 : Mathf.Clamp(m_CurrentAnimIndex, 0, animations.Length - 1); }
            set { m_CurrentAnimIndex = Mathf.Clamp(value, 0, animations.Length - 1); }
        }
        public override int AnimationCount => animations.Length;
        /// <summary>
        /// The current animation assigned to be played.
        /// </summary>
        public ValueAnim CurrentAnimation
        {
            get
            {
                // Index property being lower than 0 = No animations
                if (CurrentAnimIndex < 0)
                    return null;

                return animations[CurrentAnimIndex];
            }
        }
        /// <summary>
        /// List of the contained animations.
        /// </summary>
        public ValueAnim[] animations = new ValueAnim[1];
        /// <summary>
        /// Plays animation on <c>Start()</c>.
        /// </summary>
        public bool playOnStart = false;
        /// <summary>
        /// Determines which update will the animation will be played on.
        /// </summary>
        public BehaviourUpdateMode animUpdateMode = BehaviourUpdateMode.Update;
        /// <summary>
        /// This setting is only valid when <see cref="animateInCoroutine"/> is <see langword="true"/>!
        /// <br>Animates the sprite independent of the <see cref="Time.timeScale"/></br>
        /// </summary>
        public bool ignoreTimeScale = false;

        // -- State
        /// <summary>
        /// Whether if the animation is playing.
        /// </summary>
        public bool IsPlaying { get; private set; } = false;
        /// <summary>
        /// Returns whether if '<see cref="GatherInitialValue"/>' was called once atleast.
        /// <br>Used to validate <see cref="initialValue"/> if there's none.</br>
        /// </summary>
        public bool IsInitialized { get; private set; } = false;
        /// <summary>
        /// Current frame of the animation.
        /// </summary>
        public int CurrentFrame { get; private set; } = 0;
        /// <summary>
        /// Current animation timer.
        /// <br>Tick this if you want to completely change the <see cref="UpdateAnimator(float)"/> behaviour.</br>
        /// </summary>
        protected float m_timer;
        /// <summary>
        /// The starting value for the animation.
        /// <br>The <see cref="AnimatedValue"/> is set to this when the animation is <see cref="Stop"/></br>
        /// </summary> 
        public TValue initialValue;

        /// <summary>
        /// Returns whether if the initial value is null.
        /// <br>This is only needed internally as unity objects don't work with normal null comparison,
        /// but the other scripts accessing this class can use the <typeparamref name="TValue"/>'s equality comparer.</br>
        /// </summary>
        protected virtual bool InitialValueIsNull
        {
            get
            {
                if (initialValue is Object initialUnityObject)
                {
                    return initialUnityObject == null;
                }

                // Normal object comparison
                return initialValue == null;
            }
        }

        // -- Abstract Class
        /// <summary>
        /// The value that is going to be animated by the inheriting type <see cref="ValueAnim"/>.
        /// </summary>
        public abstract TValue AnimatedValue { get; protected set; }

        private void Start()
        {
            GatherInitialValue();

            if (animations.Length < 0) // Array to play is invalid.
            {
                Debug.LogWarning($"[ValueAnimator::Start] Cannot start animation : there is no animations on object '{name}'.");
                enabled = false;
            }
            else if (playOnStart) // Array to play is valid, allow play on start.
            {
                Play();
            }
        }
        /// <summary>
        /// Gathers the <see cref="initialValue"/> variable from existing <see cref="AnimatedValue"/>.
        /// </summary>
        public void GatherInitialValue()
        {
            initialValue = AnimatedValue;
            IsInitialized = true;
        }

        protected virtual void Update()
        {
            if (!IsPlaying || animUpdateMode != BehaviourUpdateMode.Update)
                return;

            UpdateAnimator(ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);
        }
        protected virtual void FixedUpdate()
        {
            if (!IsPlaying || animUpdateMode != BehaviourUpdateMode.FixedUpdate)
                return;

            UpdateAnimator(ignoreTimeScale ? Time.fixedUnscaledDeltaTime : Time.fixedDeltaTime);
        }
        protected virtual void LateUpdate()
        {
            if (!IsPlaying || animUpdateMode != BehaviourUpdateMode.LateUpdate)
                return;

            UpdateAnimator(ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);
        }

        protected bool isUpdateAnimatorStop = false;
        /// <summary>
        /// Updates the animator itself depending on the settings.
        /// </summary>
        protected virtual void UpdateAnimator(float deltaTime)
        {
            if (CurrentAnimation == null)
            {
                Debug.LogError($"[ValueAnimator::UpdateAnimator] Current animation on \"{name}\" is null, stopping.");
                Stop();
                return;
            }

            m_timer += deltaTime;

            var frameMS = CurrentAnimation.frameMS;
            var loop = CurrentAnimation.loop;

            if (frameMS <= 0f)
                return;

            // Lower timer + increment animation
            if (m_timer >= frameMS)
            {
                m_timer -= frameMS;

                // CurrentFrame starts from 0.
                // We also want to show the first frame.                
                AnimatedValue = CurrentAnimation[CurrentFrame];

                if (loop)
                {
                    // Animation is forever looping
                    CurrentFrame = (CurrentFrame + 1) % CurrentAnimation.valueFrames.Length;
                }
                else
                {
                    // Check the last frame
                    int currentFrameSet = CurrentFrame + 1;
                    CurrentFrame = Mathf.Clamp(currentFrameSet, 0, CurrentAnimation.valueFrames.Length - 1);

                    // Do a psuedo-stop if the last frame
                    if (CurrentFrame != currentFrameSet)
                    {
                        // Don't call 'Stop()' here to keep the sprite on the last one.
                        // Calling stop will reset to the initial frame.
                        IsPlaying = false;
                        isUpdateAnimatorStop = true; // Set the psuedo-stop value
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Plays the <see cref="CurrentAnimation"/>.
        /// </summary>
        public override void Play()
        {
            if (IsPlaying)
            {
                Stop();
            }
            else if (!isUpdateAnimatorStop)
            {
                // This means that we aren't stuck on the animation value & not playing
                // We can safely gather the initial value without it being a initial value of the given animation
                GatherInitialValue();
            }

            CurrentFrame = 0;
            IsPlaying = true;
            isUpdateAnimatorStop = false;

            // Immediately set the value to the first
            AnimatedValue = CurrentAnimation[CurrentFrame];
        }
        /// <summary>
        /// Plays the <see cref="ValueAnim"/> in <see cref="animations"/> with matching id.
        /// </summary>
        public override void Play(string id)
        {
            if (IsPlaying)
            {
                // Update
                Stop();
            }
            else if (!isUpdateAnimatorStop)
            {
                GatherInitialValue();
            }

            // Find sequentially as animations are not sorted.
            bool foundID = false;
            for (int i = 0; i < animations.Length; i++)
            {
                if (animations[i].name.Equals(id, StringComparison.Ordinal))
                {
                    m_CurrentAnimIndex = i;
                    foundID = true;
                    break;
                }
            }

            if (!foundID)
                Debug.LogWarning($"[ValueAnimator::Play(string)] Couldn't find ID : \"{id}\". Playing current animation.");

            CurrentFrame = 0;
            IsPlaying = true;
            isUpdateAnimatorStop = false;

            // Immediately set the value to the first
            AnimatedValue = CurrentAnimation[CurrentFrame];
        }
        /// <summary>
        /// Stops the animation while keeping <see cref="CurrentFrame"/> in it's place.
        /// </summary>
        public override void Pause()
        {
            IsPlaying = false;
        }
        /// <summary>
        /// Stops the animation & resets everything.
        /// </summary>
        public override void Stop()
        {
            // Reset state
            IsPlaying = false;
            isUpdateAnimatorStop = false;
            CurrentFrame = 0;

            // Set value to initial (if initialized)
            if (InitialValueIsNull)
            {
                if (!IsInitialized)
                {
                    GatherInitialValue();
                }
            }

            AnimatedValue = initialValue;
        }
    }
}
