using System;
using UnityEngine;
using BXFW.Tweening.Next.Events;
using System.Text;

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
        protected float m_Duration = 0f;

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
        protected float m_Delay = 0f;

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
        /// Waits the <see cref="Delay"/> again when the tween loops.
        /// </summary>
        public bool WaitDelayOnLoop => m_WaitDelayOnLoop;
        /// <summary>
        /// <inheritdoc cref="WaitDelayOnLoop"/> <c>[Tweenable Internal, Serialized]</c>
        /// </summary>
        [SerializeField, InspectorConditionalDraw(nameof(IsLoopable))]
        protected bool m_WaitDelayOnLoop = true;

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
        /// Whether if the 'EaseCurve' should be used.
        /// <br>Setting this will not touch the internal <see cref="m_EaseCurve"/> value.</br>
        /// </summary>
        public bool UseEaseCurve
        {
            get { return m_UseEaseCurve; }
            set
            {
                m_UseEaseCurve = value;
                if (!value)
                {
                    return;
                }

                // Set 'm_EaseCurve' to a value if it's null when the curve is set..
                m_EaseCurve ??= AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }
        }
        /// <summary>
        /// <inheritdoc cref="UseEaseCurve"/>
        /// </summary>
        [SerializeField]
        private bool m_UseEaseCurve;

        /// <summary>
        /// <inheritdoc cref="Ease"/> <br/><c>[Tweenable Internal, Serialized]</c>
        /// </summary>
        [SerializeField, InspectorConditionalDraw(nameof(UseEaseCurve), ConditionInverted = true)]
        private EaseType m_Ease = EaseType.QuadOut;
        /// <summary>
        /// The internal cached ease function.
        /// <br>Use the <see cref="EaseFunction"/> to ensure a non-null easing function.</br>
        /// </summary>
        protected BXSEaseAction m_EaseFunction;
        /// <summary>
        /// The easing function set by setting the <see cref="Ease"/>.
        /// </summary>
        public BXSEaseAction EaseFunction
        {
            get
            {
                m_EaseFunction ??= (f) => BXTweenEase.Methods[m_Ease](f);

                return m_EaseFunction;
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
        [SerializeField, InspectorConditionalDraw(nameof(UseEaseCurve))]
        protected AnimationCurve m_EaseCurve;
        /// <summary>
        /// Evaluates the current selected easing curve.
        /// <br>This respects the <see cref="Clamp01EasingSetter"/> and <see cref="UseEaseCurve"/> settings.</br>
        /// </summary>
        protected virtual float EvaluateEasing(float t)
        {
            float returnValue = UseEaseCurve ? EaseCurve.Evaluate(t) : EaseFunction(t);
            if (Clamp01EasingSetter)
                returnValue = Math.Clamp(returnValue, 0f, 1f);

            return returnValue;
        }

        /// <summary>
        /// The speed that this tween will run. 1 is normal speed and default value.
        /// <br>This value can't be negative. It being negative will cause the <see cref="BXSTween.RunTweenable(IBXSTweenRunner, BXSTweenable)"/>
        /// to hang on this tween, but won't make this tween invalid.</br>
        /// </summary>
        public float Speed
        {
            get { return m_Speed; }
            protected set { m_Speed = Math.Max(0f, value); }
        }
        /// <summary>
        /// <inheritdoc cref="Speed"/>
        /// </summary>
        [SerializeField, Clamp(0f, 65535f)]
        private float m_Speed = 1f;

        /// <summary>
        /// Whether if this tween is relative to it's ending value.
        /// <br>If this is true, the tween should calculate it's ending value as <c>StartValue + EndValue</c>.</br>
        /// <br>Setting this value will also get the <c>StartValue</c> everytime <see cref="Play"/> is called.</br>
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
        /// Whether if this tween should ignore timeScale while updating.
        /// </summary>
        public bool IgnoreTimeScale => m_IgnoreTimeScale;
        [SerializeField]
        protected bool m_IgnoreTimeScale = false;

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

        // -- Events
        // The events can be changed either manually or by using the setter methods by classes overriding this
        /// <summary>
        /// Called with <see cref="Play"/> of the tween.
        /// </summary>
        public BXSAction OnPlayAction;
        /// <summary>
        /// Called on the start of the tween once.
        /// <br>After the <see cref="Delay"/> has been waited out.</br>
        /// </summary>
        public BXSAction OnStartAction;
        /// <summary>
        /// Called every tick of this tween, after the tween has waited out it's delay and while it's running.
        /// </summary>
        public BXSAction OnTickAction;
        /// <summary>
        /// Called when the tween is to be paused.
        /// <br><see cref="Pause"/> function is one of the triggers.</br>
        /// </summary>
        public BXSAction OnPauseAction;
        /// <summary>
        /// Called when the tween repeats. (with the same priority as <see cref="OnEndAction"/>)
        /// <br>The tween is not reset when this is called, but it has to be reset.</br>
        /// <br>This DOES NOT get called when the tween completely ends, use <see cref="OnEndAction"/> for that.</br>
        /// </summary>
        public BXSAction OnRepeatAction; 
        /// <summary>
        /// Called when the tween completely ends.
        /// </summary>
        public BXSAction OnEndAction;

        // -- Control Events
        /// <summary>
        /// The condition for elapsing the tween or not.
        /// <br>This function should return true constantly unless the tween shouldn't elapse more.</br>
        /// <br>This will suspend the tween until this condition is true.</br>
        /// </summary>
        public BXSTickConditionAction TickConditionAction { get; protected set; }

        /// <summary>
        /// Clears the <see cref="OnStartAction"/>
        /// </summary>
        public void ClearStartAction()
        {
            OnStartAction = null;
        }
        /// <summary>
        /// Clears the <see cref="OnTickAction"/>.
        /// </summary>
        public void ClearUpdateAction()
        {
            OnTickAction = null;
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
        public float DelayElapsed { get; protected internal set; }
        /// <summary>
        /// The current elapsed value for this tween.
        /// <br>This value only resets when the <see cref="Reset"/> is called, which is done by the stop action.</br>
        /// </summary>
        public float CurrentElapsed { get; protected internal set; }
        /// <summary>
        /// The remaining loops that this tween has.
        /// <br>Only decrements until the 0.</br>
        /// </summary>
        public int RemainingLoops { get; protected internal set; } = 0;
        /// <summary>
        /// Whether if the tween has started.
        /// </summary>
        public bool IsPlaying { get; protected set; }
        /// <summary>
        /// Whether if the tween is paused.
        /// <br>This depends on several factors, such as whether if the tween was elasped at all and if it is running currently or not.</br>
        /// </summary>
        public bool IsPaused => !IsPlaying && (DelayElapsed > float.Epsilon || CurrentElapsed > float.Epsilon);
        // -- Starting State
        /// <summary>
        /// The duration when the <see cref="Play"/> is called.
        /// </summary>
        public float StartingDuration { get; protected set; }
        /// <summary>
        /// The delay when the <see cref="Play"/> is called.
        /// </summary>
        public float StartingDelay { get; protected set; }
        /// <summary>
        /// The count of loops when the <see cref="Play"/> is called.
        /// </summary>
        public int StartingLoops { get; protected set; }
        /// <summary>
        /// Whether if the tweenable is valid.
        /// <br>If this is <see langword="false"/>, the playing methods (<see cref="Play"/>, <see cref="Pause"/>, <see cref="Stop"/>) won't work.</br>
        /// </summary>
        public virtual bool IsValid => true;

        /// <summary>
        /// Is set true if <see cref="Play"/> was called once. (note : only for runtime)
        /// </summary>
        public bool HasPlayedOnce { get; protected set; } = false;
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
        /// The tween value to be evaluated. Do the interpolation in this function here.
        /// </summary>
        /// <param name="t">A linear value that goes from 0-&gt;1. You can set this to <see cref="CurrentElapsed"/> or your custom elapsing variable.</param>
        public abstract void EvaluateTween(float t);

        /// <summary>
        /// Copies the values of <paramref name="tweenable"/> to this method. (everything except the State values)
        /// <br>If the Context types don't match the copying must be only done for the base <see cref="BXSTweenable"/>.</br>
        /// </summary>
        /// <typeparam name="T">Type of the tweenable to copy from.</typeparam>
        public virtual void CopyFrom<T>(T tweenable)
            where T : BXSTweenable
        {
            // Copy the base values to this
            m_Duration = tweenable.m_Duration;
            m_Delay = tweenable.m_Delay;
            m_LoopCount = tweenable.m_LoopCount;
            m_LoopType = tweenable.m_LoopType;
            m_Ease = tweenable.m_Ease;
            m_EaseCurve = tweenable.m_EaseCurve;
            m_Speed = tweenable.m_Speed;
            m_IsEndValueRelative = tweenable.m_IsEndValueRelative;
            m_Clamp01EasingSetter = tweenable.m_Clamp01EasingSetter;

            m_TickType = tweenable.m_TickType;
            m_IgnoreTimeScale = tweenable.m_IgnoreTimeScale;
            m_ID = tweenable.m_ID;
            m_IDObject = tweenable.m_IDObject;

            OnPlayAction = tweenable.OnPlayAction;
            OnStartAction = tweenable.OnStartAction;
            OnTickAction = tweenable.OnTickAction;
            OnRepeatAction = tweenable.OnRepeatAction;
            OnPauseAction = tweenable.OnPauseAction;
            OnEndAction = tweenable.OnEndAction;
            TickConditionAction = tweenable.TickConditionAction;

            // Other values will be copied by the override casting the values to itself...
        }

        /// <summary>
        /// Starts the tween.
        /// <br>If the tween is already running, calling this will stop it.</br>
        /// <br>The base method calls the events and sets <see cref="IsPlaying"/> to true, please call it.</br>
        /// </summary>
        public virtual void Play()
        {
            if (!IsValid)
            {
                // Debug Log
                return;
            }

            if (IsPlaying)
                Stop();

            // Assign run temps
            StartingDuration = Duration;
            StartingDelay = Delay;
            StartingLoops = LoopCount;
            // Set state
            IsPlaying = true;
            OnPlayAction?.Invoke();
            
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
            if (!IsValid)
            {
                // Debug Log
                return;
            }

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
            if (!IsValid)
            {
                // Debug Log
                return;
            }

            IsPlaying = false;
            OnEndAction?.Invoke();
            BXSTween.RunningTweens.Remove(this);

            Reset();
        }
        /// <summary>
        /// Resets the state. (like the elapsed, the current loop count [only resets when !<see cref="IsPlaying"/>], whether if values are switched etc.)
        /// <br>Calling this while the tween is playing will reset all progress.</br>
        /// </summary>
        public virtual void Reset()
        {
            DelayElapsed = 0f;
            CurrentElapsed = 0f;
            if (!IsPlaying)
                RemainingLoops = LoopCount;
            IsTargetValuesSwitched = false;
        }

        /// <summary>
        /// Converts a <see cref="BXSTweenable"/> to string.
        /// <br>NOTE: Only use this for debugging, this will return a large string!</br>
        /// </summary>
        public override string ToString()
        {
            return ToString(false);
        }
        /// <summary>
        /// Converts a <see cref="BXSTweenable"/> to string.
        /// </summary>
        /// <param name="simpleString">Whether to return a shorter, simpler string. Use this option if you are gonna call this method often.</param>
        /// <param name="pSep">The property seperator for values.</param>
        public virtual string ToString(bool simpleString, char pSep = ',')
        {
            if (simpleString)
            {
                return $"[BXSTweenable(play={IsPlaying})] Duration={m_Duration}{pSep} Delay={m_Delay}{pSep} Loops={m_LoopCount}{pSep} Speed={m_Speed}{pSep} ID={m_ID}{pSep} IDObj={m_IDObject}";
            }

            StringBuilder sb = new StringBuilder(512);
            sb.Append("[BXSTweenable(play=").Append(IsPlaying).Append(")]")
                .Append(pSep).Append(" Duration=").Append(m_Duration)
                .Append(pSep).Append(" Delay=").Append(m_Delay)
                .Append(pSep).Append(" LoopCount=").Append(m_LoopCount)
                .Append(pSep).Append(" Ease=").Append(m_Ease)
                .Append(pSep).Append(" EaseCurve=").Append(m_EaseCurve)
                .Append(pSep).Append(" Speed=").Append(m_Speed)
                .Append(pSep).Append(" IgnoreTimeScale=").Append(m_IgnoreTimeScale)
                .Append(pSep).Append(" EndValueRelative=").Append(m_IsEndValueRelative)
                .Append(pSep).Append(" Clamp01Easing=").Append(m_Clamp01EasingSetter)
                .Append(pSep).Append(" TickType=").Append(m_TickType)
                .Append(pSep).Append(" ID=").Append(m_ID)
                .Append(pSep).Append(" IDObject=").Append(m_IDObject)
                .Append(pSep).Append(" PlayAction=").Append(OnPlayAction)
                .Append(pSep).Append(" StartAction=").Append(OnStartAction)
                .Append(pSep).Append(" UpdateAction=").Append(OnTickAction)
                .Append(pSep).Append(" PauseAction=").Append(OnPauseAction)
                .Append(pSep).Append(" RepeatAction=").Append(OnRepeatAction)
                .Append(pSep).Append(" EndAction=").Append(OnEndAction)
                .Append(pSep).Append(" TickConditionAction=").Append(TickConditionAction);

            return sb.ToString();
        }
    }
}
