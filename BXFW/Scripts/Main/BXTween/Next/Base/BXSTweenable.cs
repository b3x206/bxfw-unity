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
    /// <br><see cref="Variable"/> : The tweening will be called on a method like 'Update', where it is called every frame.</br>
    /// <br><see cref="Fixed"/> : The tweening will be called on a method like 'FixedUpdate', where it is called every n times per second.</br>
    /// </summary>
    public enum TickType
    {
        Variable, Fixed
    }

    /// <summary>
    /// A class that defines what a tweenable is.
    /// <br>Any class inheriting from this moves a value from <c>a-&gt;b</c>.</br>
    /// </summary>
    [Serializable]
    public abstract class BXSTweenable
    {
        // -- Settings
        /// <summary>
        /// Whether if the tween happens instantly (no run is going to be dispatched)
        /// <br>This also checks whether if the tween <see cref="IsDelayed"/>.</br>
        /// </summary>
        public bool IsInstant => Duration <= 0f && !IsDelayed;
        /// <summary>
        /// The duration of this tween.
        /// </summary>
        public virtual float Duration => m_Duration;
        /// <summary>
        /// <inheritdoc cref="Duration"/> <br/><c>[Tweenable Internal, Serialized]</c>
        /// </summary>
        [SerializeField]
        protected float m_Duration;

        /// <summary>
        /// Whether if the tween is a delayed one. (Delay &gt; 0f)
        /// </summary>
        public bool IsDelayed => Delay > 0f;
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
        /// Returns whether if this tween could loop (LoopCount &gt; 0)
        /// </summary>
        public bool IsLoopable => LoopCount != 0;
        /// <summary>
        /// Whether if this tween loops forever (LoopCount &lt; 0)
        /// </summary>
        public bool IsInfiniteLoop => LoopCount < 0;
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
        public EaseType Ease
        {
            get { return m_Ease; }
            protected set 
            {
                m_Ease = value;
                // TODO : Get rid of this new delegate creation
                m_EaseFunction = (f) => BXTweenEase.Methods[m_Ease](f);
            }
        }
        /// <summary>
        /// <inheritdoc cref="Ease"/> <br/><c>[Tweenable Internal, Serialized]</c>
        /// </summary>
        [SerializeField]
        private EaseType m_Ease = EaseType.QuadOut;
        /// <summary>
        /// The easing function set by the <see cref="m_Ease"/>.
        /// </summary>
        protected BXSEaseAction m_EaseFunction;

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
        /// Evaluates the current easing curve.
        /// <br>This respects the <see cref="Clamp01EasingSetter"/> and <see cref="UseEaseCurve"/> settings.</br>
        /// </summary>
        protected virtual float EvaluateEasing(float t)
        {
            float returnValue = UseEaseCurve ? EaseCurve.Evaluate(t) : m_EaseFunction(t);
            if (Clamp01EasingSetter)
                returnValue = Math.Clamp(returnValue, 0f, 1f);

            return returnValue;
        }

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
        public TickType TickType => m_TickType;
        /// <summary>
        /// The actual tick type, depending on the <see cref="BXSTween.MainRunner"/>'s <see cref="IBXSTweenRunner.SupportsFixedTick"/>.
        /// <br>Always returns <see cref="TickType.Variable"/> if it isn't supported.</br>
        /// </summary>
        public TickType ActualTickType
        {
            get
            {
                if (BXSTween.MainRunner != null)
                    return BXSTween.MainRunner.SupportsFixedTick ? TickType : TickType.Variable;

                // No 'MainRunner', just return the normal value.
                return TickType;
            }
        }
        /// <summary>
        /// <inheritdoc cref="TickType"/> <br/><c>[Tweenable Internal, Serialized]</c>
        /// </summary>
        [SerializeField]
        protected TickType m_TickType = TickType.Variable;

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

        /// <summary>
        /// Is true if <see cref="Play"/> was called once.
        /// </summary>
        public bool HasPlayedOnce { get; protected set; } = false;

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
        /// The <see cref="OnUpdateAction"/>, but can be called by the BXFW tweening stuff.
        /// </summary>
        internal BXSAction CallableUpdateAction => OnUpdateAction;
        /// <summary>
        /// Called when the tween is to be paused.
        /// </summary>
        public event BXSAction OnPauseAction;
        /// <summary>
        /// Called on the end.
        /// </summary>
        public event BXSAction OnEndAction;

        // -- Control Events
        /// <summary>
        /// The condition for elapsing the tween or not.
        /// <br>This function should return true constantly unless the tween shouldn't elapse more.</br>
        /// <br>This will suspend the tween until this condition is true.</br>
        /// </summary>
        public BXSTickConditionAction TickConditionAction { get; protected set; }

        /// <summary>Sets the <see cref="OnStartAction"/>.</summary>
        protected void SetStartActionValue(BXSAction value)
        {
            OnStartAction = value;
        }
        /// <summary>Sets the <see cref="OnUpdateAction"/>.</summary>
        protected void SetUpdateActionValue(BXSAction value)
        {
            OnUpdateAction = value;
        }
        /// <summary>Sets the <see cref="OnPauseAction"/>.</summary>
        protected void SetPauseActionValue(BXSAction value)
        {
            OnPauseAction = value;
        }
        /// <summary>Sets the <see cref="OnEndAction"/>.</summary>
        protected void SetEndActionValue(BXSAction value)
        {
            OnEndAction = value;
        }

        /// <summary>
        /// Clears the <see cref="OnStartAction"/>
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
        /// <summary>
        /// Clears the <see cref="TickConditionAction"/>.
        /// </summary>
        public void ClearTickConditionAction()
        {
            TickConditionAction = null;
        }

        /// <summary>
        /// Clears all control actions.
        /// </summary>
        public virtual void ClearAllActions()
        {
            ClearStartAction();
            ClearUpdateAction();
            ClearPauseAction();
            ClearEndAction();
            ClearTickConditionAction();
        }

        // -- State
        /// <summary>
        /// The delay elapsed value for this tween.
        /// <br>
        /// Depending on the <see cref="Delay"/> being larger than 0, this value will tick gradually.
        /// Otherwise it will be instantly set to 1 if there's no delay (except for a single frame delay for all tweens)
        /// </br>
        /// </summary>
        public float DelayElapsed { get; protected set; }
        /// <summary>
        /// The current elapsed value for this tween.
        /// </summary>
        public float CurrentElapsed { get; protected set; }
        /// <summary>
        /// The remaining loops that this tween has.
        /// <br>Only decrements until the 0.</br>
        /// </summary>
        public int CurrentLoop { get; protected set; } = 0;
        /// <summary>
        /// Whether if the tween has started.
        /// </summary>
        public bool IsPlaying { get; protected set; }
        /// <summary>
        /// Whether if the tween is paused.
        /// <br>This depends on several factors, such as whether if the tween was elasped at all and if it is running currently or not.</br>
        /// </summary>
        public bool IsPaused => !IsPlaying && (DelayElapsed > float.Epsilon || CurrentElapsed > float.Epsilon);

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
        /// Called only when the target values are to be switched.
        /// It won't be called if the <see cref="IsTargetValuesSwitched"/> is set to the same value again.
        /// <br>Use the <see cref="IsTargetValuesSwitched"/> as a reference value.</br>
        /// </summary>
        protected abstract void OnSwitchTargetValues();

        // -- Methods
        /// <summary>
        /// The tween value to be evaluated. Use the setter here.
        /// </summary>
        /// <param name="t">A linear value that goes from 0-&gt;1</param>
        protected internal abstract void EvaluateTween(float t);

        /// <summary>
        /// Copies the values of <paramref name="tweenable"/> to this method. (everything except the State values)
        /// <br>If the Context types don't match the copying must be only done for the base <see cref="BXSTweenable"/>.</br>
        /// </summary>
        /// <typeparam name="T">Type of the tweenable to copy from.</typeparam>
        public virtual void CopyFrom<T>(T tweenable)
            where T : BXSTweenable
        {
            // Copy the base values to this
            

            // Other values will be copied by the override casting the values to itself...
        }

        /// <summary>
        /// Starts the tween.
        /// <br>If the tween is already running, calling this will stop it.</br>
        /// <br>The base method calls the events and sets <see cref="IsPlaying"/> to true.</br>
        /// </summary>
        public virtual void Play()
        {
            if (IsPlaying)
                Stop();

            IsPlaying = true;
            OnStartAction?.Invoke();
            BXSTween.RunningTweens.Add(this);

            if (!HasPlayedOnce)
                HasPlayedOnce = true;
        }
        /// <summary>
        /// Keeps the current tween state and pauses the running tweening timers.
        /// <br>Calling <see cref="Stop"/> at this state will only reset the tween.</br>
        /// </summary>
        public virtual void Pause()
        {
            if (!IsPlaying)
                return;

            IsPlaying = false;
            OnPauseAction?.Invoke();
        }
        /// <summary>
        /// Stops the tween.
        /// <br>The base method resets the state of the Tweenable.</br>
        /// </summary>
        public virtual void Stop()
        {
            IsPlaying = false;
            OnEndAction?.Invoke();
            BXSTween.RunningTweens.Remove(this);

            Reset();
        }
        
        /// <summary>
        /// Resets the state. (like the elapsed, the current loop count, whether if values are switched etc.)
        /// </summary>
        protected virtual void Reset()
        {
            DelayElapsed = 0f;
            CurrentElapsed = 0f;
            CurrentLoop = LoopCount;
            IsTargetValuesSwitched = false;
        }
    }
}
