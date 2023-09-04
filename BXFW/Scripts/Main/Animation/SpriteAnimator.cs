using System;
using UnityEngine;
using UnityEngine.UI;

namespace BXFW
{
    /// TODO : Make this class abstract and take a SetFrame function with the given settable type for value to set.
    /// <summary>
    /// An animation runner that does not hold grudges to the variable that is going to be animated.
    /// </summary>
    public class SpriteAnimator : MonoBehaviour
    {
        [Serializable]
        public class SpriteAnimSequence
        {
            /// <summary>
            /// Name of the sequence.
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
            /// Animation frames.
            /// </summary>
            public Sprite[] frameSpriteArray;

            public Sprite this[int index]
            {
                get { return frameSpriteArray[index]; }
                set { frameSpriteArray[index] = value; }
            }

            public float Duration { get { return frameSpriteArray.Length * frameMS; } }
        }

        [Header(":: Animation")]
        public SpriteRenderer animateSprite;
        public Image animateImage;
        [SerializeField] private int _currentAnimIndex = 0;
        public int CurrentAnimIndex
        {
            get { return animations.Length <= 0 ? -1 : Mathf.Clamp(_currentAnimIndex, 0, animations.Length - 1); }
            set { _currentAnimIndex = Mathf.Clamp(value, 0, animations.Length - 1); }
        }
        public SpriteAnimSequence CurrentAnimation
        {
            get
            {
                // Don't throw exceptions
                if (CurrentAnimIndex < 0)
                    return null;

                return animations[CurrentAnimIndex];
            }
        }
        public SpriteAnimSequence[] animations = new SpriteAnimSequence[1];

        /// <summary>
        /// This setting 'probably' constraints maximum fps (if the fps is already higher than frameMS)
        /// </summary>
        public bool useFixedUpdate = false;
        /// <summary>
        /// This setting is only valid when <see cref="animateInCoroutine"/> is <see langword="true"/>!
        /// <br>Animates the sprite independent of the <see cref="Time.timeScale"/></br>
        /// </summary>
        public bool overrideTimeScale = false;
        /// <summary>
        /// Plays animation on <c>Start()</c>.
        /// </summary>
        public bool playOnStart = false;

        // :: Runtime / Status
        /// <summary>
        /// Whether if the animation is playing.
        /// </summary>
        public bool IsPlaying { get; private set; } = false;
        /// <summary>
        /// Current frame of the animation.
        /// </summary>
        public int CurrentFrame { get; private set; } = 0;
        /// <summary> Current animation timer. </summary>
        private float timer;
        /// <summary> The starting sprite. When the anim finishes this will be set to <see cref="animateSprite"/>. </summary> 
        public Sprite initialSprite;

        private void Start()
        {
            if (animateSprite == null && !TryGetComponent(out animateSprite) &&
                animateImage == null && !TryGetComponent(out animateImage))
                Debug.LogError($"[SpriteAnimator::Start] Cannot set initialSprite : animateSprite is null on object '{name}'.");
            else
            {
                GatherInitialSprite();
            }

            if (animations.Length < 0) // Array to play is invalid.
            {
                Debug.LogWarning($"[SpriteAnimator::Start] Cannot start animation : there is no animations on object '{name}'.");
                enabled = false;
            }
            else if (playOnStart) // Array to play is valid, allow play on start.
            {
                Play();
            }
        }
        /// <summary>
        /// Gathers the <see cref="initialSprite"/> variable from existing fields. (image/sprite rend)
        /// <br>Call if sprite is changed!</br>
        /// </summary>
        public void GatherInitialSprite()
        {
            if (animateImage != null)
                initialSprite = animateImage.sprite;

            if (animateSprite != null)
                initialSprite = animateSprite.sprite;
        }
        private void SetInitialObjectSprite(Sprite sInitial)
        {
            if (animateImage != null)
                animateImage.sprite = sInitial;

            if (animateSprite != null)
                animateSprite.sprite = sInitial;
        }
        private void Update()
        {
            if (useFixedUpdate)
                return;
            if (!IsPlaying)
                return;

            timer += overrideTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            UpdateAnimator();
        }
        private void FixedUpdate()
        {
            if (!useFixedUpdate)
                return;
            if (!IsPlaying)
                return;

            timer += overrideTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            UpdateAnimator();
        }

        private bool isUpdateAnimatorStop = false;
        private void UpdateAnimator()
        {
            if (CurrentAnimation == null)
            {
                Debug.LogError($"[SpriteAnimator::UpdateAnimator] Current animation is null! On Object : {name}");
                return;
            }

            var frameMS = CurrentAnimation.frameMS;
            var loop = CurrentAnimation.loop;

            if (frameMS <= 0f)
                return;

            if (timer >= frameMS)
            {
                timer -= frameMS;

                // CurrentFrame starts from 0.
                // We also want to show the first frame.
                if (animateSprite != null)
                    animateSprite.sprite = CurrentAnimation[CurrentFrame];
                if (animateImage != null)
                    animateImage.sprite = CurrentAnimation[CurrentFrame];

                if (loop)
                    CurrentFrame = (CurrentFrame + 1) % CurrentAnimation.frameSpriteArray.Length;
                else
                {
                    var currentFrameSet = CurrentFrame + 1;
                    CurrentFrame = Mathf.Clamp(currentFrameSet, 0, CurrentAnimation.frameSpriteArray.Length - 1);

                    if (CurrentFrame != currentFrameSet)
                    {
                        // Don't call 'Stop()' here to keep the sprite on the last one.
                        IsPlaying = false;
                        isUpdateAnimatorStop = true;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Plays the <see cref="CurrentAnimation"/>.
        /// </summary>
        public void Play()
        {
            if (IsPlaying)
            {
                // GetInitialSprite();
                Stop();
            }
            else if (!isUpdateAnimatorStop)
            {
                // This means that we aren't stuck on the animation sprite & not playing
                // We can safely gather the sprite
                GatherInitialSprite();
            }

            CurrentFrame = 0;
            IsPlaying = true;
            isUpdateAnimatorStop = false;

            // Immediately set the sprite to the first
            SetInitialObjectSprite(CurrentAnimation[CurrentFrame]);
        }
        /// <summary>
        /// Plays the <see cref="SpriteAnimSequence"/> in <see cref="animations"/> with matching id.
        /// </summary>
        public void Play(string id)
        {
            if (IsPlaying)
            {
                // Update initialSprite
                // GetInitialSprite();
                Stop();
            }
            else if (!isUpdateAnimatorStop)
            {
                // This means that we aren't stuck on the animation sprite
                // We can safely gather the sprite
                GatherInitialSprite();
            }

            // Find sequentially as animations are not sorted in an ordinal way or at all.
            bool foundID = false;
            for (int i = 0; i < animations.Length; i++)
            {
                if (animations[i].name == id)
                {
                    _currentAnimIndex = i;
                    foundID = true;
                    break;
                }
            }

            if (!foundID)
                Debug.LogWarning($"[SpriteAnimator::Play(id)] Couldn't find ID : \"{id}\". Playing current animation.");

            CurrentFrame = 0;
            IsPlaying = true;
            isUpdateAnimatorStop = false;

            // Immediately set the sprite to the first
            SetInitialObjectSprite(CurrentAnimation[CurrentFrame]);
        }

        /// <summary>
        /// Stops the animation while keeping <see cref="CurrentFrame"/> in it's place.
        /// </summary>
        public void Pause()
        {
            IsPlaying = false;
        }
        /// <summary>
        /// Stops the animation & resets everything.
        /// </summary>
        public void Stop()
        {
            IsPlaying = false;
            isUpdateAnimatorStop = false;

            SetInitialObjectSprite(initialSprite);
            CurrentFrame = 0;
        }
    }
}
