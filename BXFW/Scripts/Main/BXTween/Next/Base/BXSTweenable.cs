using System;
using UnityEngine;
using BXFW.Tweening.Next.Events;

/// -- To make this file compatible with other things :
/// A : 'using' Alias 'SerializeField' with something that marks hidden fields as serializable (i.e 'ExportAttribute' in godot)
/// B : Remove animation curve (this is get as a generic Lerp

namespace BXFW.Tweening.Next
{
    /// <summary>
    /// Type of a loop in <see cref="BXSTweenable"/>'s repeats.
    /// <br><see cref="Yoyo"/> = Switches the tweening values on </br>
    /// </summary>
    public enum LoopType
    {
        Yoyo, Reset
    }
    /// <summary>
    /// The suspension type for the <see cref="BXSTweenable.TickConditionAction"/>'s suspendType return.
    /// <br><see cref="None"/>  = Tween won't suspend and will keep ticking.</br>
    /// <br><see cref="Tick"/>  = Keeps ticking the tween without elapsing it. This will make the tween keep playing but not move.</br>
    /// <br><see cref="Pause"/> = Pauses the base tween.</br>
    /// <br><see cref="Stop"/>  = Stops the base tween.</br>
    /// </summary>
    public enum TickConditionSuspendType
    {
        None, Tick, Pause, Stop
    }
    /// <summary>
    /// The targeted ticking type enumeration for <see cref="BXSTweenable"/>.
    /// </summary>
    public enum TargetTickType
    {
        Variable, Fixed
    }

    /// <summary>
    /// A class that defines what a tweenable is.
    /// <br>Any class inheriting from this moves a value from <c>a-&gt;b</c>, no exceptions.</br>
    /// </summary>
    [Serializable]
    public abstract class BXSTweenable
    {
        // -- Settings
        /// <summary>
        /// The duration of this tween.
        /// </summary>
        public float Duration => m_Duration;
        /// <summary>
        /// <inheritdoc cref="Duration"/> <br/><c>[Tweenable Internal, Serialized]</c>
        /// </summary>
        [SerializeField]
        protected float m_Duration;

        /// <summary>
        /// The Delay of this tween.
        /// </summary>
        public float Delay => m_Delay;
        /// <summary>
        /// <inheritdoc cref="Delay"/> <br/><c>[Tweenable Internal, Serialized]</c>
        /// </summary>
        [SerializeField]
        protected float m_Delay;

        /// <summary>
        /// Returns whether if this tween could loop (RepeatCount > 0)
        /// </summary>
        public bool IsLoopable => LoopCount > 0;
        /// <summary>
        /// The repeat amount of this tween.
        /// </summary>
        public int LoopCount => m_LoopCount;
        /// <summary>
        /// <inheritdoc cref="LoopCount"/> <br/><c>[Tweenable Internal, Serialized]</c>
        /// </summary>
        [SerializeField, Clamp(-1, int.MaxValue)]
        protected int m_LoopCount;
        /// <summary>
        /// The type of loop if the tween <see cref="IsLoopable"/>.
        /// </summary>
        public LoopType LoopType => m_LoopType;
        /// <summary>
        /// <inheritdoc cref="LoopType"/> <br/><c>[Tweenable Internal, Serialized]</c>
        /// </summary>
        [SerializeField, InspectorConditionalDraw(nameof(IsLoopable))]
        protected LoopType m_LoopType = LoopType.Yoyo;

        /// <summary>
        /// Type of the easing for this tweenable.
        /// </summary>
        public EaseType Ease => m_Ease;
        /// <summary>
        /// <inheritdoc cref="Ease"/> <br/><c>[Tweenable Internal, Serialized]</c>
        /// </summary>
        [SerializeField]
        protected EaseType m_Ease = EaseType.QuadOut;

        /// <summary>
        /// Whether if the 'EaseCurve' should be used.
        /// <br>Setting this will either set the internal ease curve value to </br>
        /// </summary>
        public bool UseEaseCurve
        { 
            get { return m_EaseCurve != null; } 
            set 
            { 
                if (!value) 
                {
                    m_EaseCurve = null;
                    return;
                }
                // Set 'm_EaseCurve' to a value if it's null
                m_EaseCurve ??= AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }
        }
        /// <summary>
        /// The used ease curve.
        /// <br>If this is non-null the animation curve will be used instead.</br>
        /// </summary>
        public AnimationCurve EaseCurve => m_EaseCurve;
        /// <summary>
        /// <inheritdoc cref="EaseCurve"/> <br/><c>[Tweenable Internal, Serialized]</c>
        /// </summary>
        [SerializeField]
        protected AnimationCurve m_EaseCurve;

        /// <summary>
        /// Whether if this tween is relative to it's ending value.
        /// <br>If this is true, the tween should calculate it's ending value relatively.</br>
        /// </summary>
        public bool IsEndValueRelative => m_IsEndValueRelative;
        /// <summary>
        /// <inheritdoc cref="IsEndValueRelative"/> <br/><c>[Tweenable Internal, Serialized]</c>
        /// </summary>
        [SerializeField]
        protected bool m_IsEndValueRelative;

        /// <summary>
        /// Whether the easing should be clamped between 0-1.
        /// </summary>
        public bool Clamp01EasingSetter => m_Clamp01EasingSetter;
        [SerializeField]
        protected bool m_Clamp01EasingSetter = false;

        // -- Properties
        /// <summary>
        /// The targeted ticking type.
        /// <br>Note : Usage of this depends on whether the <see cref="BXSTween.MainRunner"/> supporting <see cref="IBXSTweenRunner.SupportsFixedTick"/>.</br>
        /// <br>(basically setting this may not change the <see cref="BXSTweenable"/> behaviour, use the <see cref="ActualTickType"/> method)</br>
        /// </summary>
        public TargetTickType TickType => m_TickType;
        /// <summary>
        /// The actual tick type, depending on the <see cref="BXSTween.MainRunner"/>'s <see cref="IBXSTweenRunner.SupportsFixedTick"/>.
        /// <br>Always returns <see cref="TargetTickType.Variable"/> if it isn't supported.</br>
        /// </summary>
        public TargetTickType ActualTickType
        {
            get
            {
                if (BXSTween.MainRunner != null)
                    return BXSTween.MainRunner.SupportsFixedTick ? TickType : TargetTickType.Variable;

                // No 'MainRunner', just return the normal value.
                return TickType;
            }
        }
        /// <summary>
        /// <inheritdoc cref="TickType"/> <br/><c>[Tweenable Internal, Serialized]</c>
        /// </summary>
        [SerializeField]
        protected TargetTickType m_TickType = TargetTickType.Variable;

        /// <summary>
        /// ID of the given tweening object.
        /// <br>If this is <see cref="BXSTween.NoID"/>, no object conditions were attached 
        /// + this tween has no id and can't be accessed except for code references.</br>
        /// </summary>
        public int ID => m_ID;
        /// <summary>
        /// <inheritdoc cref="ID"/> <br/><c>[Tweenable Internal, Runtime]</c>
        /// </summary>
        protected int m_ID = BXSTween.NoID;
        /// <summary>
        /// Object reference attached to this tween that gets calculated with the ID.
        /// </summary>
        protected object m_IDObject;
        // ??
        protected internal bool IDObjectIsUnityObject { get; internal set; }

        // -- Events
        // The events can be changed either manually or by using the setter methods by classes overriding this
        /// <summary>
        /// Called on the start of the tween.
        /// </summary>
        public event BXSAction OnStartAction;
        /// <summary>
        /// Called every tick of this tween.
        /// </summary>
        public event BXSAction OnUpdateAction;
        /// <summary>
        /// Called when the tween is to be paused.
        /// </summary>
        public event BXSAction OnPauseAction;
        /// <summary>
        /// Called on the end.
        /// </summary>
        public event BXSAction OnEndAction;

        /// <summary>
        /// The <see cref="OnUpdateAction"/>, but can be called by the BXFW tweening stuff.
        /// </summary>
        internal BXSAction CallableUpdateAction => OnUpdateAction;

        /// <summary>
        /// Clears the <see cref="OnStartAction"/> as the name states.
        /// <br>These values can be set or be cleared without using daisy-chaining of the core context class.</br>
        /// </summary>
        public void ClearStartAction()
        {
            OnStartAction = null;
        }
        /// <summary>
        /// Clears the <see cref="OnUpdateAction"/>.
        /// </summary>
        public void ClearUpdateAction()
        {
            OnUpdateAction = null;
        }
        /// <summary>
        /// Clears the <see cref="OnPauseAction"/>.
        /// </summary>
        public void ClearPauseAction()
        {
            OnPauseAction = null;
        }
        /// <summary>
        /// Clears the <see cref="OnEndAction"/>.
        /// </summary>
        public void ClearEndAction()
        {
            OnEndAction = null;
        }

        // -- Control Events
        /// <summary>
        /// The condition for elapsing the tween or not.
        /// <br>This function should return true constantly unless the tween shouldn't elapse more.</br>
        /// <br>This will suspend the tween until this condition is true.</br>
        /// </summary>
        public BXSTickConditionAction TickConditionAction { get; protected set; }

        // -- State
        /// <summary>
        /// The delay elapsed value for this tween.
        /// <br>Depending on the <see cref="Delay"/> being larger than 0, this value will tick.</br>
        /// <br>No delay = always zero</br>
        /// </summary>
        public float DelayElapsed { get; protected set; }
        /// <summary>
        /// The current elapsed value for this tween.
        /// </summary>
        public float CurrentElapsed { get; protected set; }
        /// <summary>
        /// The loop count that this tween in.
        /// <br>Only increments until the <see cref="LoopCount"/>.</br>
        /// </summary>
        public int CurrentLoop { get; protected set; } = 0;
        /// <summary>
        /// Whether if the tween has started.
        /// </summary>
        public bool IsRunning { get; protected set; }
        /// <summary>
        /// Whether if the tween is paused.
        /// <br>This depends on several factors, such as whether if the tween was elasped at all and if it is running currently or not.</br>
        /// </summary>
        public bool IsPaused => !IsRunning && (DelayElapsed > Mathf.Epsilon || CurrentElapsed > Mathf.Epsilon);

        /// <summary>
        /// <inheritdoc cref="IsTargetValuesSwitched"/>
        /// </summary>
        protected bool m_IsTargetValuesSwitched = false;
        /// <summary>
        /// Whether if the target values are switched.
        /// <br>This is used with the <see cref="LoopType"/>.</br>
        /// </summary>
        public bool IsTargetValuesSwitched
        {
            get
            {
                return m_IsTargetValuesSwitched;
            }
            internal set
            {
                bool prevValue = m_IsTargetValuesSwitched;
                m_IsTargetValuesSwitched = value;

                if (prevValue != value)
                    OnSwitchTargetValues();
            }
        }
        /// <summary>
        /// Called when the target values are to be switched.
        /// <br>Use the <see cref="IsTargetValuesSwitched"/> as a reference value.</br>
        /// </summary>
        protected abstract void OnSwitchTargetValues();

        // -- Methods
        /// <summary>
        /// Starts the tween.
        /// <br>The base method calls the events and sets <see cref="IsRunning"/> to true.</br>
        /// </summary>
        public virtual void Start()
        {
            IsRunning = true;
            OnStartAction?.Invoke();
        }
        /// <summary>
        /// Keeps the current tween state and pauses the running tweening timers.
        /// <br>Calling <see cref="Stop"/> at this state will only reset the tween.</br>
        /// </summary>
        public virtual void Pause()
        {
            IsRunning = false;
            OnPauseAction?.Invoke();
        }
        /// <summary>
        /// Stops the tween.
        /// <br>The base method resets the state of the Tweenable.</br>
        /// </summary>
        public virtual void Stop()
        {
            IsRunning = false;
            OnEndAction?.Invoke();

            DelayElapsed = 0f;
            CurrentElapsed = 0f;
            CurrentLoop = 0;
            IsTargetValuesSwitched = false;
        }
    }
}
