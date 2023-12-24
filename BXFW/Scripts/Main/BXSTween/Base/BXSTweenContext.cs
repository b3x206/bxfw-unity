using System;
using UnityEngine;
using BXFW.Tweening.Events;

namespace BXFW.Tweening
{
    /// <summary>
    /// Contains a typed context.
    /// <br>The actual setters are contained here, along with the other values.</br>
    /// <br>This context handles most of the type related things + <see cref="BXSTweenable"/> setters.</br>
    /// <br/>
    /// <br>Overriding classes should implement everything + a preferably public default constructor 
    /// if another constructor that isn't default was made.</br>
    /// </summary>
    [Serializable]
    public abstract class BXSTweenContext<TValue> : BXSTweenable
    {
        // -- Start/End value
        /// <summary>
        /// The current gathered starting value.
        /// </summary>
        public TValue StartValue { get; protected set; }
        /// <summary>
        /// The current gathered ending value.
        /// </summary>
        public TValue EndValue { get; protected set; }
        /// <summary>
        /// The current last value used in <see cref="EvaluateTween(float)"/> of this context.
        /// <br>
        /// This is the current interpolation value for the corresponding
        /// <see cref="BXSTweenable.CurrentElapsed"/> time between <see cref="StartValue"/> and <see cref="EndValue"/>.
        /// </br>
        /// <br>This value updates accordingly if <see cref="SetStartValue(TValue)"/> or <see cref="SetEndValue(TValue)"/> is called in any way.</br>
        /// </summary>
        public TValue CurrentValue { get; protected set; }
        /// <summary>
        /// When called, switches the <see cref="StartValue"/> and <see cref="EndValue"/>.
        /// </summary>
        protected override void OnSwitchTargetValues()
        {
            (StartValue, EndValue) = (EndValue, StartValue);
        }

        /// <summary>
        /// The function to get the 'StartValue'.
        /// <br>Can be used in tween starting by calling <see cref="SetStartValue()"/> for getting new value to interpolate from.</br>
        /// </summary>
        public BXSGetterAction<TValue> GetterAction { get; protected set; }
        /// <summary>
        /// The setter action, called when the tweening is being done.
        /// </summary>
        public BXSSetterAction<TValue> SetterAction { get; protected set; }

        // -- Interpolation
        /// <summary>
        /// The linear interpolation method to override for the setter of this <typeparamref name="TValue"/> context.
        /// <br>This expects an unclamped interpolation action.</br>
        /// </summary>
        public abstract TValue LerpMethod(TValue a, TValue b, float time);

        // - Overrides
        /// <summary>
        /// Returns whether the tween context has a setter.
        /// <br>This may also return whether if the <see cref="StartValue"/> and <see cref="EndValue"/> is not null if <typeparamref name="TValue"/> is nullable.</br>
        /// </summary>
        public override bool IsValid =>
            SetterAction != null &&
            // check if struct or not, if not a struct check nulls
            (typeof(TValue).IsValueType || (StartValue != null && EndValue != null));
        
        /// <summary>
        /// The tick type of a sequence is always to be run at the variable mode.
        /// </summary>
        public override TickType ActualTickType => TickType.Variable;

        /// <summary>
        /// Evaluates the <see cref="SetterAction"/> with <see cref="LerpMethod"/>.
        /// </summary>
        public override void EvaluateTween(float t)
        {
            // Easing checks and stuff is done on 'EvaluateEasing'.
            float easedTime = EvaluateEasing(t);
            CurrentValue = LerpMethod(StartValue, EndValue, easedTime);

            SetterAction(CurrentValue);
        }

        // -- Methods
        public override void CopyFrom<T>(T tweenable)
        {
            base.CopyFrom(tweenable);
            BXSTweenContext<TValue> tweenableAsContext = tweenable as BXSTweenContext<TValue>;
            if (tweenableAsContext == null)
            {
                return;
            }

            StartValue = tweenableAsContext.StartValue;
            EndValue = tweenableAsContext.EndValue;
            GetterAction = tweenableAsContext.GetterAction;
            SetterAction = tweenableAsContext.SetterAction;
        }

        // - Operators
        public static implicit operator bool(BXSTweenContext<TValue> context)
        {
            return context.IsValid;
        }

        // - Daisy Chain Setters
        /// <summary>
        /// Sets up context. Do this if your context is not <see cref="IsValid"/>.
        /// <br/>
        /// <br><see cref="ArgumentNullException"/> = Thrown when any of these are null : 
        /// <paramref name="startValueGetter"/> or <paramref name="setter"/>.</br>
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public BXSTweenContext<TValue> SetupContext(BXSGetterAction<TValue> startValueGetter, TValue endValue, BXSSetterAction<TValue> setter)
        {
            SetStartValue(startValueGetter).SetEndValue(endValue).SetSetter(setter);
            return this;
        }
        /// <summary>
        /// Sets up context.
        /// <br>A shortcut method for setting the setter and the start+end values.</br>
        /// </summary>
        public BXSTweenContext<TValue> SetupContext(TValue startValue, TValue endValue, BXSSetterAction<TValue> setter)
        {
            SetStartValue(startValue).SetEndValue(endValue).SetSetter(setter);
            return this;
        }

        /// <summary>
        /// Sets the start value from the getter <see cref="GetterAction"/>.
        /// <br>This can only be successfully called if there's a <see cref="GetterAction"/>, otherwise it will throw <see cref="NullReferenceException"/>.</br>
        /// </summary>
        /// <exception cref="NullReferenceException"/>
        public BXSTweenContext<TValue> SetStartValue()
        {
            if (GetterAction == null)
            {
                throw new NullReferenceException($"[BXSTweenContext<{typeof(TValue)}>::SetStartValue] Parameterless SetStartValue 'GetterAction()' value is null.");
            }

            return SetStartValue(GetterAction());
        }
        /// <summary>
        /// Sets the <see cref="GetterAction"/> value and sets the <see cref="StartValue"/> from it.
        /// </summary>
        /// <param name="getter">
        /// The getter to use. This value cannot be <see langword="null"/>.
        /// If you don't want to use a getter use the <see cref="SetStartValue(TValue)"/> without delegate and direct variable.
        /// <br>However, with a getter, you can use the parameterless <see cref="SetStartValue()"/> 
        /// to get a new value on demand (by hooking it up to events, etc.).</br>
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        public BXSTweenContext<TValue> SetStartValue(BXSGetterAction<TValue> getter)
        {
            if (getter == null)
            {
                throw new ArgumentNullException(nameof(getter), $"[BXSTweenContext<{typeof(TValue)}>::SetStartValue] Given argument is null.");
            }

            GetterAction = getter;
            return SetStartValue(GetterAction());
        }
        /// <summary>
        /// Sets the starting value.
        /// <br>This effects the tween while running.</br>
        /// </summary>
        public BXSTweenContext<TValue> SetStartValue(TValue value)
        {
            StartValue = value;
            // Set the currently elapsed value to the lerp result.
            if (!IsPlaying)
            {
                CurrentValue = LerpMethod(StartValue, EndValue, EvaluateEasing(CurrentElapsed));
            }

            return this;
        }
        /// <summary>
        /// Sets the ending value.
        /// <br>This effects the tween while running.</br>
        /// </summary>
        /// <param name="setRelative">Whether to set the end value as a relative one. Calls <see cref="SetIsEndRelative(bool)"/>.</param>
        public BXSTweenContext<TValue> SetEndValue(TValue value)
        {
            EndValue = value;
            if (!IsPlaying)
            {
                CurrentValue = LerpMethod(StartValue, EndValue, EvaluateEasing(CurrentElapsed));
            }

            return this;
        }
        /// <summary>
        /// Sets the <see cref="SetterAction"/> value.
        /// </summary>
        /// <param name="setter">The setter. This value cannot be null.</param>
        /// <exception cref="ArgumentNullException"/>
        public BXSTweenContext<TValue> SetSetter(BXSSetterAction<TValue> setter)
        {
            if (setter == null)
            {
                throw new ArgumentNullException(nameof(setter), $"[BXSTweenContext<{typeof(TValue)}>::SetSetterAction] Given argument is null.");
            }

            SetterAction = setter;
            return this;
        }

        /// <summary>
        /// Sets the duration of the tween.
        /// <br>Has no effect after the tween was started.</br>
        /// </summary>
        public BXSTweenContext<TValue> SetDuration(float duration)
        {
            m_Duration = duration;
            return this;
        }
        /// <summary>
        /// Sets delay.
        /// <br>Has no effect after the tween was started.</br>
        /// </summary>
        /// <param name="delay">The delay to wait. Values equal or lower than 0 are no delay.</param>
        public BXSTweenContext<TValue> SetDelay(float delay)
        {
            // Calculate how much percent is the set delay (unless the delay was set to <= 0, which in that case set DelayElapsed to 1)
            if (IsPlaying && DelayElapsed < 1f)
            {
                // To not throw 'DivideByZeroException', make the epsilon way larger but a non-negligable value.
                if (delay <= .000001f)
                {
                    DelayElapsed = 1f;
                }
                else
                {
                    // Calculate how much of the delay remains while the delay is being ticked
                    DelayElapsed = Math.Min(1f, m_Delay / delay);
                }
            }

            m_Delay = delay;
            return this;
        }
        /// <summary>
        /// Sets the loop count.
        /// <br>Has no effect if the tween was started.</br>
        /// </summary>
        /// <param name="count">Count to set the tween. If this is lower than 0, the tween will loop forever.</param>
        public BXSTweenContext<TValue> SetLoopCount(int count)
        {
            m_LoopCount = count;
            return this;
        }
        /// <summary>
        /// Sets the loop type.
        /// <br>This does affect the tween after it was started.</br>
        /// </summary>
        /// <param name="type">Type of the loop. See <see cref="LoopType"/>'s notes for more information.</param>
        public BXSTweenContext<TValue> SetLoopType(LoopType type)
        {
            m_LoopType = type;

            return this;
        }
        /// <summary>
        /// Sets whether to wait the <see cref="BXSTweenable.Delay"/> when the tween repeats.
        /// </summary>
        public BXSTweenContext<TValue> SetWaitDelayOnLoop(bool doWait)
        {
            m_WaitDelayOnLoop = doWait;

            return this;
        }
        /// <summary>
        /// Sets the easing type.
        /// </summary>
        /// <param name="ease">The type of easing.</param>
        /// <param name="disableEaseCurve">Disables <see cref="BXSTweenable.UseEaseCurve"/>.</param>
        public BXSTweenContext<TValue> SetEase(EaseType ease, bool disableEaseCurve = false)
        {
            // This thing's setter already updates this value.
            Ease = ease;
            if (disableEaseCurve)
            {
                UseEaseCurve = false;
            }

            return this;
        }
        /// <summary>
        /// Sets the easing curve.
        /// <br>Setting this null will disable <see cref="BXSTweenable.UseEaseCurve"/>.</br>
        /// </summary>
        /// <param name="curve">The animation curve to set.</param>
        public BXSTweenContext<TValue> SetEaseCurve(AnimationCurve curve)
        {
            m_EaseCurve = curve;

            if (m_EaseCurve == null)
            {
                UseEaseCurve = false;
            }

            return this;
        }
        /// <summary>
        /// Sets the speed of this tween.
        /// <br>Setting this value 0 or lower will make the tween not tick forward.</br>
        /// </summary>
        public BXSTweenContext<TValue> SetSpeed(float speed)
        {
            Speed = speed;

            return this;
        }
        /// <summary>
        /// Sets to whether to allow the <see cref="BXSTweenable.EvaluateEasing(float)"/> to overshoot.
        /// </summary>
        public BXSTweenContext<TValue> SetClampEasingSetter(bool doClamp)
        {
            m_Clamp01EasingSetter = doClamp;

            return this;
        }

        /// <summary>
        /// Set the ticking type of this tweenable.
        /// <br>This does affect what update the tween is running during playback.</br>
        /// </summary>
        /// <param name="type">Type of the update ticking. See <see cref="TickType"/>'s notes for more information.</param>
        public BXSTweenContext<TValue> SetTickType(TickType type)
        {
            m_TickType = type;

            return this;
        }
        /// <summary>
        /// Sets to whether ignore the time scale.
        /// <br>Setting this will run the tween unscaled except for it's <see cref="BXSTweenable.Speed"/>.</br>
        /// </summary>
        /// <param name="doIgnore"></param>
        public BXSTweenContext<TValue> SetIgnoreTimeScale(bool doIgnore)
        {
            m_IgnoreTimeScale = doIgnore;

            return this;
        }
        /// <summary>
        /// Sets the target object id.
        /// <br>Fallbacks to <see cref="object.GetHashCode"/> if there's no <see cref="BXSTween.MainRunner"/></br>
        /// </summary>
        public BXSTweenContext<TValue> SetIDObject<T>(T obj) where T : class
        {
            m_IDObject = obj;

            if (BXSTween.MainRunner == null)
            {
                // TODO : Debug log here that we can't get id? and just falling back to GetHashCode?
                m_ID = m_IDObject.GetHashCode();
                return this;
            }

            m_ID = BXSTween.MainRunner.GetIDFromObject(m_IDObject);

            return this;
        }

        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnPlayAction"/> event.
        /// <br>This is called when <see cref="BXSTweenable.Play"/> is called on this tween.</br>
        /// </summary>
        public BXSTweenContext<TValue> SetPlayAction(BXSAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            switch (setMode)
            {
                case EventSetMode.Subtract:
                    OnPlayAction -= action;
                    break;
                case EventSetMode.Add:
                    OnPlayAction += action;
                    break;

                default:
                case EventSetMode.Equals:
                    OnPlayAction = action;
                    break;
            }
            return this;
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnStartAction"/> event.
        /// <br>This is called when the tween has waited out it's delay and it is starting for the first time.</br>
        /// </summary>
        public BXSTweenContext<TValue> SetStartAction(BXSAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            switch (setMode)
            {
                case EventSetMode.Subtract:
                    OnStartAction -= action;
                    break;
                case EventSetMode.Add:
                    OnStartAction += action;
                    break;

                default:
                case EventSetMode.Equals:
                    OnStartAction = action;
                    break;
            }
            return this;
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnTickAction"/> event.
        /// <br>This is called every time the tween ticks. It is started to be called after the delay was waited out.</br>
        /// </summary>
        public BXSTweenContext<TValue> SetTickAction(BXSAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            switch (setMode)
            {
                case EventSetMode.Subtract:
                    OnTickAction -= action;
                    break;
                case EventSetMode.Add:
                    OnTickAction += action;
                    break;

                default:
                case EventSetMode.Equals:
                    OnTickAction = action;
                    break;
            }
            return this;
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnTickAction"/> event.<br/>
        /// This method is an alias for <see cref="SetTickAction(BXSAction, EventSetMode)"/>.
        /// </summary>
        public BXSTweenContext<TValue> SetUpdateAction(BXSAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            return SetTickAction(action, setMode);
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnPauseAction"/> event.
        /// <br>It is called when <see cref="BXSTweenable.Pause"/> is called on this tween.</br>
        /// </summary>
        public BXSTweenContext<TValue> SetPauseAction(BXSAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            switch (setMode)
            {
                case EventSetMode.Subtract:
                    OnPauseAction -= action;
                    break;
                case EventSetMode.Add:
                    OnPauseAction += action;
                    break;

                default:
                case EventSetMode.Equals:
                    OnPauseAction = action;
                    break;
            }
            return this;
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnRepeatAction"/> event.
        /// </summary>
        public BXSTweenContext<TValue> SetRepeatAction(BXSAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            switch (setMode)
            {
                case EventSetMode.Subtract:
                    OnRepeatAction -= action;
                    break;
                case EventSetMode.Add:
                    OnRepeatAction += action;
                    break;

                default:
                case EventSetMode.Equals:
                    OnRepeatAction = action;
                    break;
            }
            return this;
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnEndAction"/> event.
        /// <br>The difference between the <see cref="SetStopAction(BXSAction, EventSetMode)"/> 
        /// and this is that this only gets invoked when the tween ends after the tweens duration.</br>
        /// <br/>
        /// <br><b>Note : </b> If you are want to play the same tween from this tweens ending action use <see cref="SetStopAction(BXSAction, EventSetMode)"/> instead,
        /// this is due to the <see cref="BXSTweenable.Stop"/> sets <see cref="BXSTweenable.IsPlaying"/> to false immediately after this event.</br>
        /// <br>Or use <see cref="BXSTweenSequence"/> in conjuction with <see cref="BXSTweenable.AsCopy{T}"/>.</br>
        /// </summary>
        public BXSTweenContext<TValue> SetEndAction(BXSAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            switch (setMode)
            {
                case EventSetMode.Subtract:
                    OnEndAction -= action;
                    break;
                case EventSetMode.Add:
                    OnEndAction += action;
                    break;

                default:
                case EventSetMode.Equals:
                    OnEndAction = action;
                    break;
            }
            return this;
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnStopAction"/> event.
        /// <br>The difference between the <see cref="SetEndAction(BXSAction, EventSetMode)"/>
        /// and this is that this gets called both when the tween ends or when <see cref="BXSTweenable.Stop"/> gets called.</br>
        /// </summary>
        public BXSTweenContext<TValue> SetStopAction(BXSAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            switch (setMode)
            {
                case EventSetMode.Subtract:
                    OnStopAction -= action;
                    break;
                case EventSetMode.Add:
                    OnStopAction += action;
                    break;

                default:
                case EventSetMode.Equals:
                    OnStopAction = action;
                    break;
            }
            return this;
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.TickConditionAction"/> action.
        /// <br>Return the suitable <see cref="TickSuspendType"/> in the function.</br>
        /// </summary> 
        public BXSTweenContext<TValue> SetTickConditionAction(BXSTickConditionAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            switch (setMode)
            {
                case EventSetMode.Subtract:
                    TickConditionAction -= action;
                    break;
                case EventSetMode.Add:
                    TickConditionAction += action;
                    break;

                default:
                case EventSetMode.Equals:
                    TickConditionAction = action;
                    break;
            }
            return this;
        }

        // -- State
        public override void Play()
        {
            if (!IsValid)
            {
                BXSTween.MainLogger.LogWarning($"[BXSTweenContext::Play] This tweenable '{ToString()}' isn't valid. Cannot 'Play' tween.");
                return;
            }

            base.Play();
        }
    }
}
