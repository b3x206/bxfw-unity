using System;
using System.Reflection;
using System.Collections;

using UnityEngine;
using BXFW.Tweening.Events;
using BXFW.Tweening.Editor;
using static BXFW.Tweening.BXTween;

namespace BXFW.Tweening
{
    /// <summary>
    /// Variable changing mode.
    /// <br><see cref="Add"/> : Adds to the variable, if adding is allowed by the method.</br>
    /// <br><see cref="Equals"/> : Sets directly to the variable, if setting is allowed by the method.</br>
    /// <br><see cref="Subtract"/> : Removes from the variable, if removing is allowed by the method.</br>
    /// </summary>
    public enum VariableSetMode
    {
        Add = 0,
        Equals = 1,
        Subtract = 2
    }
    /// <summary>
    /// The repeat types of the tweens.
    /// <br><see cref="PingPong"/> goes back to the end smoothly and counts a repeat.</br>
    /// <br><see cref="Reset"/> goes back instantly and goes back to the target value.</br>
    /// </summary>
    public enum RepeatType
    {
        PingPong = 0,
        Reset = 1
    }
    /// <summary>
    /// Tween context.
    /// Not serializable, but contains the currently running tween data.
    /// <br>For a serializable alternative, use the <see cref="BXTweenProperty{T}"/>'s already defined classes 
    /// (non-generic ones, you can also define custom classes for it by deriving from the given class)</br>
    /// </summary>
    public sealed class BXTweenCTX<T> : ITweenCTX
    {
        // ---- Variables ---- //
        // Should be read-only for 'public' and only be able to set from methods (for daisy chaining of methods). 
        // Most of the info is contained in the here.
        // -- Standard Values
        public T StartValue { get; private set; }
        public T EndValue { get; private set; }
        // -- Settings
        /// <summary>
        /// Whether if context should invoke <see cref="OnEndAction"/> when <see cref="StopTween"/> is called.
        /// This enables / disables ending actions.
        /// </summary>
        public bool InvokeEventOnStop { get; private set; } = true;
        public float Duration { get; private set; } = 0f;
        public float StartDelay { get; private set; } = 0f;
        public int RepeatAmount { get; private set; } = 0;
        public RepeatType RepeatType { get; private set; } = CurrentSettings.DefaultRepeatType;
        public bool InvokeEventOnRepeat { get; private set; } = true;
        // -- Status
        public bool IsRunning { get; private set; } = false;
        public bool IsPaused
        {
            get { return CurrentElapsed != 0 && !IsRunning; }
        }
        /// <summary>
        /// Helper variable for <see cref="IsValuesSwitched"/>.
        /// </summary>
        private bool _IsValuesSwitched = false;
        /// <summary>
        /// Get whether the values are switched under repeat type <see cref="RepeatType.PingPong"/>.
        /// <br>Usually returns false if the repeat type is <see cref="RepeatType.Reset"/> 
        ///     or if repeat amount is zero.</br>
        /// </summary>
        public bool IsValuesSwitched
        {
            get { return _IsValuesSwitched && RepeatType != RepeatType.Reset; }
        }
        /// <summary>
        /// Whether if the context is valid.
        /// <br>Context validity depends on 3 things (most of the time) -> </br>
        /// <br>1 : Whether if the <see cref="SetterFunction"/> is null or not.</br>
        /// <br>2 : Whether if the <see cref="IteratorCoroutine"/> is null or not.</br>
        /// <br>3 : If <see cref="TargetObjectIsOptional"/> is false and the <see cref="TargetObject"/> is whether null or not (if target object IS optional this is ignored)</br>
        /// </summary>
        public bool ContextIsValid
        {
            get
            {
                // Start value or the EndValue is, most of the time, not null. (because the T is generally struct)
                // A failsafe was added to check if the typeof(T) is struct (IsValueType)
                return SetterFunction != null && IteratorCoroutine != null && // Step 1 and 2
                    (typeof(T).IsValueType || (StartValue != null && EndValue != null)) && // Struct / Class check (check values if class)
                    (TargetObjectIsOptional || TargetObject != null); // Step 3
            }
        }
        // -- Target (Identifier and Null checks)
        private readonly UnityEngine.Object _TargetObj;
        public UnityEngine.Object TargetObject { get { return _TargetObj; } }
        public bool TargetObjectIsOptional { get { return _TargetObj == null; } }
        public IEnumerator IteratorCoroutine { get { return _IteratorCoroutine; } }
        // -- Pausing
        /// <summary>
        /// The current set value of the coroutine.
        /// <br>Generally set in the <see cref="SetterFunction"/>.</br>
        /// </summary>
        public T CurrentValue { get; private set; }
        /// <summary>
        /// Current 'elapsed' local variable of the tween. Used for pause function.
        /// <br>A value that goes from 0 to 1. (Doesn't reset when the tween is stopped)</br>
        /// <br>NOTE : This value only moves linearly. To add ease use the <see cref="BXTweenEase"/> methods.</br>
        /// </summary>
        public float CurrentElapsed { get; internal set; }
        /// <summary>
        /// The coroutine elapsed value.
        /// <br>This value is bigger than -1 when the coroutine starts.</br>
        /// </summary>
        public float CoroutineElapsed = -1f;
        // -- Interpolation
        /// <summary>
        /// Ease type of the tween.
        /// <br>Can be set using <see cref="SetEase(EaseType, bool)"/>.</br>
        /// </summary>
        public EaseType TweenEaseType { get; private set; } = CurrentSettings.DefaultEaseType;
        /// <summary>
        /// Custom easing curve.
        /// <br>Can be set using <see cref="SetCustomCurve(AnimationCurve, bool)"/>.</br>
        /// </summary>
        public AnimationCurve CustomTimeCurve { get; private set; } = null;
        /// <summary>
        /// Boolean to whether to use 
        /// </summary>
        public bool UseCustomTwTimeCurve { get { return CustomTimeCurve != null; } }
        // -- Setter (subpart of Interpolation)
        /// <summary>
        /// Time interpolation.
        /// <br>Set when you set a <see cref="SetCustomCurve(AnimationCurve, bool)"/> or <see cref="SetEase(EaseType, bool)"/>.</br>
        /// </summary>
        public BXTweenEaseSetMethod TimeSetLerp { get; private set; }
        public BXTweenSetMethod<T> SetterFunction { get; private set; }
        public BXTweenMethod OnEndAction { get; private set; }
        public BXTweenMethod PersistentOnEndAction { get; private set; }
        public BXTweenUnityEvent OnEndActionUnityEvent { get; private set; }

        // --- Private Fields
        // Coroutine / Iterator 
        private readonly Func<BXTweenCTX<T>, IEnumerator> _GetTweenIteratorFn;   // Delegate to get coroutine suitable for this class
        private IEnumerator _IteratorCoroutine;                     // Current setup iterator (not running)
        private IEnumerator _CurrentIteratorCoroutine;              // Current running iterator

        #region Variable Setter
        public BXTweenCTX<T> ClearEndingAction()
        {
            OnEndAction = null;

            return this;
        }
        /// <summary>
        /// Sets an event to be occured in end.
        /// </summary>
        public BXTweenCTX<T> SetEndingEvent(BXTweenMethod Event, VariableSetMode mode = VariableSetMode.Add)
        {
            switch (mode)
            {
                case VariableSetMode.Add:
                    OnEndAction += Event;
                    break;
                case VariableSetMode.Equals:
                    OnEndAction = Event;
                    break;
                case VariableSetMode.Subtract:
                    OnEndAction -= Event;
                    break;
            }

            return this;
        }
        /// <summary>
        /// Sets an event to be occured in end.
        /// </summary>
        public BXTweenCTX<T> SetEndingEvent(BXTweenUnityEvent Event)
        {
            OnEndActionUnityEvent = Event;

            return this;
        }
        public BXTweenCTX<T> SetInvokeEventsOnStop(bool value)
        {
            InvokeEventOnStop = value;

            return this;
        }
        /// <summary>
        /// Sets the duration.
        /// </summary>
        /// <param name="dur">Duration to set.</param>
        public BXTweenCTX<T> SetDuration(float dur)
        {
            Duration = dur;

            return this;
        }
        /// <summary>
        /// Sets starting delay. Has no effect if the tween has already started. 
        /// (Info : Can be applied after construction of the tween as we wait for end of frame.)
        /// Anything below zero (including zero) is a special value for no delay.
        /// </summary>
        /// <param name="delay">The delay for tween to wait.</param>
        public BXTweenCTX<T> SetDelay(float delay)
        {
            StartDelay = delay;

            return this;
        }
        /// <summary>
        /// Sets starting delay. Has no effect if the tween has already started.
        /// Anything below zero(including zero) is a special value for no delay.
        /// Randomizes the delay between 2 random float values.
        /// </summary>
        /// <param name="minDelay">The delay for tween to wait.</param>
        /// <param name="maxDelay">The delay for tween to wait.</param>
        public BXTweenCTX<T> SetRandDelay(float minDelay, float maxDelay)
        {
            return SetDelay(UnityEngine.Random.Range(minDelay, maxDelay));
        }
        /// <summary>
        /// Sets repeat amount. '0' is default for no repeat. Anything lower than '0' is infinite repeat.
        /// </summary>
        public BXTweenCTX<T> SetRepeatAmount(int repeat)
        {
            RepeatAmount = repeat;

            return this;
        }
        /// <summary>
        /// Sets repeat type.
        /// </summary>
        public BXTweenCTX<T> SetRepeatType(RepeatType type)
        {
            if (IsRunning && type == RepeatType.Reset)
            {
                // Set start and end values to normal
                if (IsValuesSwitched)
                {
                    // No longer switched.
                    SwitchStartEndValues();
                }
            }

            RepeatType = type;

            return this;
        }
        /// <summary>
        /// Sets whether to invoke the <see cref="OnEndAction"/>'s when the tween repeats.
        /// </summary>
        public BXTweenCTX<T> SetInvokeEventsOnRepeat(bool value)
        {
            InvokeEventOnRepeat = value;

            return this;
        }
        /// <summary>
        /// Sets the easing of the tween.
        /// </summary>
        public BXTweenCTX<T> SetEase(EaseType ease, bool Clamp01 = true)
        {
            // Setup curve 
            TweenEaseType = ease;

            if (UseCustomTwTimeCurve)
            {
                if (CurrentSettings.diagnosticMode)
                {
                    Debug.Log(BXTweenStrings.GetLog_BXTwCTXCustomCurveInvalid(ease));
                }

                return this;
            }

            var EaseMethod = BXTweenEase.EaseMethods[TweenEaseType];
            TimeSetLerp = (float progress, bool _) => { return EaseMethod.Invoke(progress, Clamp01); };

            return this;
        }
        /// <summary> Sets a custom animation curve. Pass null to disable custom curve. </summary>
        /// <param name="curve">Curve to set.</param>
        /// <param name="clamp">Should the curve be clamped?</param>
        public BXTweenCTX<T> SetCustomCurve(AnimationCurve curve, bool clamp = true)
        {
            // Check curve status
            if (curve == null)
            {
                // The curve is already null, warn the user.
                if (!UseCustomTwTimeCurve)
                {
                    if (CurrentSettings.diagnosticMode)
                    {
                        Debug.LogWarning(BXTweenStrings.DLog_BXTwCTXTimeCurveAlreadyNull);
                    }

                    return this;
                }

                // User wants to disable CustomCurve.
                CustomTimeCurve = null;
                // Reset curve delegate
                SetEase(TweenEaseType);
                return this;
            }

            // Set variable for context.
            CustomTimeCurve = curve;

            // Clamp value between 0-1
            if (clamp)
            {
                TimeSetLerp = (float progress, bool _) => { return Mathf.Clamp01(CustomTimeCurve.Evaluate(Mathf.Clamp01(progress))); };
            }
            else
            {
                TimeSetLerp = (float progress, bool _) => { return CustomTimeCurve.Evaluate(Mathf.Clamp01(progress)); };
            }

            return this;
        }
        /// <summary>
        /// Sets a custom setter. 
        /// Ignores null values completely as a setter is a critical part of the context.
        /// </summary>
        /// <param name="setter">Setter to set.</param>
        public BXTweenCTX<T> SetSetter(BXTweenSetMethod<T> setter)
        {
            // -- Check Setter
            if (setter == null)
            {
                Debug.LogError(BXTweenStrings.Err_BXTwCTXSetterNull);
                return this;
            }

            // -- Set Setter
            SetterFunction = setter; // Subtract this as we are going to reuse the pause setter delegate.
            SetterFunction += (T sValue) => { CurrentValue = sValue; };
            return this;
        }
        /// <summary>
        /// Sets a starting value to the tween.
        /// </summary>
        public BXTweenCTX<T> SetStartValue(T sValue)
        {
            StartValue = sValue;

            return this;
        }
        /// <summary>
        /// Sets an ending value to the tween.
        /// </summary>
        public BXTweenCTX<T> SetEndValue(T eValue)
        {
            EndValue = eValue;

            return this;
        }
        /// <summary>
        /// Switches the start and the end value (for <see cref="RepeatType.PingPong"/>).
        /// <br>Internally sets the variable <see cref="IsValuesSwitched"/>.</br>
        /// </summary>
        internal void SwitchStartEndValues()
        {
            _IsValuesSwitched = !_IsValuesSwitched;
#if CSHARP_7_3_OR_NEWER
            // Swap variables (nice)
            (StartValue, EndValue) = (EndValue, StartValue);
#else
            // Swap variables without tuples
            T sValue = StartValue;

            StartValue = EndValue;
            EndValue = sValue;
#endif
        }

        /// <summary>
        /// Updates coroutine on the <see cref="IteratorCoroutine"/> variable.
        /// If this is not updated, the coroutine does invoke, but with previous values.
        /// Always call this method when you do set on this context.
        /// Info : If this is called while the tween is running, the current running iterator will be backed up.
        /// </summary>
        /// <returns>Whether the updating is successful or not.</returns>
        public bool UpdateContextCoroutine()
        {
            // Note : If this doesn't work, use the reflection method instead of 'passed coroutine delegate' method.
            _IteratorCoroutine = _GetTweenIteratorFn.Invoke(this);

            if (CurrentSettings.diagnosticMode)
            {
                Debug.Log(BXTweenStrings.GetDLog_BXTwCTXOnUpdateContextCoroutine(this));
            }

            return _IteratorCoroutine != null;
        }
        #endregion

        /// <summary>
        /// The main constructor.
        /// </summary>
        public BXTweenCTX(T StartVal, T EndVal, UnityEngine.Object TargetO, float TDuration,
            // Func Variable
            BXTweenSetMethod<T> SetFunc, Func<BXTweenCTX<T>, IEnumerator> GetTweenIterFn,
            BXTweenMethod PersistentEndMethod = null)
        {
            try
            {
                // Public
                StartValue = StartVal;
                EndValue = EndVal;
                SetterFunction = SetFunc; // Setup setter function.
                SetterFunction += (T pValue) => { CurrentValue = pValue; }; // Add pausing delegate to the setter function.
                PersistentOnEndAction = PersistentEndMethod;

                Duration = TDuration;

                // Private
                _TargetObj = TargetO;
                _GetTweenIteratorFn = GetTweenIterFn;

                // Functions to update (like ease)
                SetEase(TweenEaseType);
            }
            catch (Exception e)
            {
                // Error handling
                Debug.LogError(BXTweenStrings.GetErr_BXTwCTXCtorExcept(e));
            }

            if (CurrentSettings.diagnosticMode)
            {
                Debug.Log(BXTweenStrings.GetDLog_BXTwCTXOnCtor(this));
            }
        }

        #region Start-Stop
        /// <summary>
        /// Starts the tween.
        /// </summary>
        public void StartTween()
        {
#if UNITY_EDITOR
            // Unity Editor
            if (!Application.isPlaying && Application.isEditor)
            {
                EditModeCoroutineExec.StartCoroutine(IteratorCoroutine);
                return;
            }
#endif
            /// Note : The duration can be 0 or less in <see cref="BXTweenProperty{T}"/>
            /// Because of this we will check the duration.
            /// While this check is bad, there's no other way of enforcing it.
            if (Duration <= 0f)
            {
                // Call the setter with last value & ignore other stuff.
                Debug.Log(BXTweenStrings.GetLog_BXTwDurationZero());
                SetterFunction.Invoke(EndValue);
                return;
            }

            // Checks
            if (IsRunning)
                StopTween();
            if (!UpdateContextCoroutine())
            {
                // Iterator coroutine failed.
                if (IteratorCoroutine == null)
                {
                    Debug.LogError(BXTweenStrings.Err_BXTwCTXFailUpdate);
                    return;
                }

                // There already is a IteratorCoroutine variable that's valid.
                Debug.LogError(BXTweenStrings.Err_BXTwCTXFailUpdateCoroutine);
            }

            // Value
            _CurrentIteratorCoroutine = IteratorCoroutine;
            // CurrentElapsed is always reset as 'StopTween' has to be called to end a tween, even automatically.
            IsRunning = true;

            // Dispatch routine
            Current.StartCoroutine(_CurrentIteratorCoroutine);
            CurrentRunningTweens.Add(this);

            if (CurrentSettings.diagnosticMode)
            {
                Debug.Log(BXTweenStrings.GetDLog_BXTwCTXOnStart(this));
            }
        }
        /// <summary>
        /// Pauses the tween, keeping the value.
        /// <br>This is untested, use at your own risk.</br>
        /// </summary>
        public void PauseTween()
        {
            if (!IsRunning)
            {
                if (CurrentSettings.diagnosticMode)
                    Debug.Log(BXTweenStrings.DLog_BXTwCTXStopInvalidCall);

                return;
            }
#if UNITY_EDITOR
            // Unity Editor Stop
            if (!Application.isPlaying && Application.isEditor)
            {
                EditModeCoroutineExec.StopCoroutine(IteratorCoroutine);
            }
            else if (_CurrentIteratorCoroutine != null)
#else
            // Player Stop
            // Not unity editor, can check if we are running normally.
            if (_CurrentIteratorCoroutine != null)
#endif
            {
                // Coroutine should stop itself HOWEVER when stop is not called by BXTweenCore.To it needs to stop 'manually'.
                Current.StopCoroutine(_CurrentIteratorCoroutine);
                _CurrentIteratorCoroutine = null;
            }
            else if (CurrentSettings.diagnosticMode) // Log errors
            {
                // _CurrentIteratorCoroutine is null.
                Debug.LogWarning(BXTweenStrings.Warn_BXTwCurrentIteratorNull);
            }

            // Value reset (only some values, others are kept)
            // The generic coroutine takes the 'CurrentElapsed' value, Don't unswitch values if repeating.
            CurrentRunningTweens.Remove(this);
            IsRunning = false;

            // Log
            if (CurrentSettings.diagnosticMode)
            {
                Debug.Log(BXTweenStrings.GetDLog_BXTwCTXOnPause(this));
            }

            // Update
            if (!UpdateContextCoroutine())
            {
                Debug.LogError(BXTweenStrings.Err_BXTwCTXFailUpdateCoroutine);
            }
        }
        /// <summary>
        /// Stops the tween.
        /// </summary>
        public void StopTween()
        {
            if (!IsRunning)
            {
                if (CurrentSettings.diagnosticMode)
                    Debug.Log(BXTweenStrings.DLog_BXTwCTXStopInvalidCall);

                return;
            }
#if UNITY_EDITOR
            // Unity Editor Stop
            if (!Application.isPlaying && Application.isEditor)
            {
                EditModeCoroutineExec.StopCoroutine(IteratorCoroutine);
            }
            else if (_CurrentIteratorCoroutine != null)
#else
            // Player Stop
            // Not unity editor, can check if we are running normally.
            if (_CurrentIteratorCoroutine != null)
#endif
            {
                // Coroutine should stop itself HOWEVER when stop is not called by BXTweenCore.To it needs to stop 'manually'.
                Current.StopCoroutine(_CurrentIteratorCoroutine);
                _CurrentIteratorCoroutine = null;
            }
            else if (CurrentSettings.diagnosticMode) // Log errors
            {
                // _CurrentIteratorCoroutine is null.
                Debug.LogWarning(BXTweenStrings.Warn_BXTwCurrentIteratorNull);
            }

            // Value reset
            CurrentRunningTweens.Remove(this);
            IsRunning = false;
            CurrentElapsed = 0f;
            if (IsValuesSwitched)
            {
                // No longer switched.
                SwitchStartEndValues();
            }

            // If we call these on the ending of the BXTweenCore's To method, the ending method after delay doesn't work.
            // The reason is that in BXTweenProperty we call 'StopTween' when we call 'StartTween'
            if (InvokeEventOnStop)
            {
                try
                {
                    // Apparently an exception can occur if the 'OnEndAction' accesses objects after destruction by external forces
                    // Try mitigating that
                    if (OnEndAction != null)
                        OnEndAction.Invoke();
                    if (PersistentOnEndAction != null)
                        PersistentOnEndAction.Invoke();
                    if (OnEndActionUnityEvent != null)
                        OnEndActionUnityEvent.Invoke(this);
                }
                catch (Exception e)
                {
                    if (CurrentSettings.diagnosticMode)
                    {
                        Debug.LogWarning(BXTweenStrings.DLog_BXTwWarnExceptOnStop(e));
                    }
                }
            }
            // Update
            if (!UpdateContextCoroutine())
            {
                Debug.LogError(BXTweenStrings.Err_BXTwCTXFailUpdateCoroutine);
            }

            // Log
            if (CurrentSettings.diagnosticMode)
            {
                Debug.Log(BXTweenStrings.GetDLog_BXTwCTXOnStop(this));
            }
        }
        #endregion

        #region Debug
        public override string ToString()
        {
            return string.Format("BXTWContext | Type : {0}", typeof(T).ToString());
        }
        /// <summary>
        /// Get the message for 'why context wasnt valid?'.
        /// <br>Returns a unknown message for no reason or unknown reason.</br>
        /// </summary>
        /// <returns>The message.</returns>
        public string DebugGetContextInvalidMsg()
        {
            // These two will probably never happen as supported tween properties are mostly structs.
            if (StartValue == null)
            {
                return "(Context)The given start value is null.";
            }
            if (EndValue == null)
            {
                return "(Context)The given end value is null.";
            }

            // These might happen due to user error.
            if (SetterFunction == null)
            {
                return "(Context)The given setter function is null. If you called 'BXTween.To()' manually please make sure you set a setter function, otherwise it's a developer error.";
            }
            // And these are edge cases where there's something wrong internally.
            if (IteratorCoroutine == null)
            {
                return "(Context)The coroutine given is null. Probably an internal issue.";
            }
            if (TargetObject == null)
            {
                return "(Context)The target object is null. Make sure you do not destroy it.";
            }

            // And this means that the code has no idea, enable debugger pls.
            return "(Context)Unknown or no reason.";
        }
        #endregion
    }
}