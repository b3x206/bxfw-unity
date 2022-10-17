using System;
using UnityEngine;

using BXFW.Tweening.Events;
using static BXFW.Tweening.BXTween;

namespace BXFW.Tweening
{
    /// <summary>
    /// Carries the base variables for the <see cref="BXTweenProperty{T}"/>.
    /// </summary>
    [Serializable]
    public abstract class BXTweenPropertyBase
    {
        [SerializeField] protected float _Duration = 1f;
        [SerializeField] protected float _Delay = 0f;
        [SerializeField] protected int _RepeatAmount = 0;
        [SerializeField] protected RepeatType _TweenRepeatType = RepeatType.PingPong;

        [SerializeField] protected bool _UseTweenCurve = false;
        [SerializeField] protected bool _AllowInterpolationEaseOvershoot = false;
        [SerializeField] protected AnimationCurve _TweenCurve;
        [SerializeField] protected EaseType _TweenEase = EaseType.QuadInOut;
        /// <summary>
        /// When this option is <see langword="true"/>, the <see cref="BXTweenCTX{T}.OnEndAction"/> is invoked when 
        /// <see cref="BXTweenCTX{T}.StopTween"/> is called explicitly (without being called by the coroutine end)
        /// </summary>
        public bool InvokeEventOnManualStop = false;
        public BXTweenUnityEvent OnEndAction;

        public float Duration
        {
            get { return _Duration; }
            set
            {
                _Duration = value;

                UpdateProperty();
            }
        }
        public float Delay
        {
            get { return _Delay; }
            set
            {
                _Delay = value;

                UpdateProperty();
            }
        }
        public int RepeatAmount
        {
            get { return _RepeatAmount; }
            set
            {
                _RepeatAmount = value;

                UpdateProperty();
            }
        }
        public RepeatType TweenRepeatType
        {
            get { return _TweenRepeatType; }
            set
            {
                _TweenRepeatType = value;

                UpdateProperty();
            }
        }
        /// <summary>Default tween curve value. Will be set if <see cref="UseTweenCurve"/> is true and <see cref="TweenCurve"/> is null.</summary>
        protected readonly AnimationCurve DEFAULT_TWCURVE_VALUE = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve TweenCurve
        {
            get
            {
                // Tween curve is null
                if (_TweenCurve == null)
                    _TweenCurve = DEFAULT_TWCURVE_VALUE;
                // Tween curve is invalid (no keys)
                if (_TweenCurve.keys.Length <= 0)
                    _TweenCurve = DEFAULT_TWCURVE_VALUE;

                return _TweenCurve;
            }
            set
            {
                if (value == null) return;

                _TweenCurve = value;

                UpdateProperty();
            }
        }
        public EaseType TweenEase
        {
            get { return _TweenEase; }
            set
            {
                _TweenEase = value;

                UpdateProperty();
            }
        }
        public bool UseTweenCurve
        {
            get { return _TweenCurve != null && _UseTweenCurve; }
            set
            {
                _UseTweenCurve = value;

                // Set default value if null.
                if (_UseTweenCurve && _TweenCurve == null)
                {
                    _TweenCurve = DEFAULT_TWCURVE_VALUE;
                }

                // Don't call 'UpdateProperty' here as the inspector calls this every frame.
            }
        }
        public bool AllowInterpolationEaseOvershoot
        {
            get { return _AllowInterpolationEaseOvershoot; }
            set
            {
                _AllowInterpolationEaseOvershoot = value;

                UpdateProperty();
            }
        }

        public abstract void UpdateProperty();
    }
    /// <summary>
    /// Tween property that contains a <see cref="BXTweenCTX{T}"/>
    /// <br>Used for creating convenient context, editable by the inspector.</br>
    /// <br/>
    /// <br>
    /// NOTE : Do not create a public field (on a monobehaviour) with this class's generic version.
    /// (you are exactly reading the description of the class that you should not create inspector properties of)
    /// </br>
    /// <br>Instead inherit from types that have defined the '<typeparamref name="T"/>'. (otherwise unity doesn't serialize it)</br>
    /// <br>If unity finally supports this feature, you can ignore the previous message.</br>
    /// </summary>
    [Serializable]
    public class BXTweenProperty<T> : BXTweenPropertyBase
    {
        // ---- Get Only ---- //
        /// <summary>
        /// Internal Tween context.
        /// <br>Only use this variable only & only inside this class.</br>
        /// </summary>
        private BXTweenCTX<T> _TwContext;
        /// <summary>
        /// The tween context.
        /// </summary>
        /// Note to BXTween developer : Don't compare this variable with null
        /// FIXME : This needs to be cleaned up, but it's the only place i will do this mess.
        public BXTweenCTX<T> TwContext
        {
            get
            {
                if (_TwContext == null)
                    Debug.LogError(BXTweenStrings.Err_BXTwPropNoTwCTX);

                return _TwContext;
            }
        }
        /// <summary>
        /// Returns whether if the given type of this property is valid.
        /// </summary>
        public bool IsValidContextType => IsTweenableType(typeof(T));

        // ---- Private ---- //
        private BXTweenSetMethod<T> _Setter;
        /// <summary>
        /// Returns whether if the context is setup.
        /// </summary>
        public bool IsSetup => _Setter != null;

        #region Ctor / Setup
        public BXTweenProperty()
        { }
        public BXTweenProperty(BXTweenCTX<T> ctx, bool stopTw = true)
        {
            // ** Stop the context and assign the context.
            if (stopTw)
            {
                ctx.StopTween();
            }

            _TwContext = ctx;

            // ** Gather values from context.
            _Duration = ctx.Duration;
            _Delay = ctx.StartDelay;
            _TweenCurve = ctx.CustomTimeCurve;
            _Setter = ctx.SetterFunction;
            // ** Set the other options from the property.
            UpdateProperty();
        }
        public static implicit operator BXTweenProperty<T>(BXTweenCTX<T> ctxEqual)
        {
            // ** Create & Return property
            return new BXTweenProperty<T>(ctxEqual);
        }
        public void SetupProperty(T StartValue, T EndValue, BXTweenSetMethod<T> Setter)
        {
            if (Setter == null)
            {
                Debug.LogError(BXTweenStrings.Err_SetterFnNull);
                return;
            }

            _Setter = Setter;

            // Only update TwContext with a new tween if it's not initialized.
            if (_TwContext == null)
            {
                _TwContext = GenericTo(StartValue, EndValue, _Duration, Setter, null, false);
            }
            else
            {
                if (CurrentSettings.diagnosticMode)
                    Debug.Log(BXTweenStrings.DLog_BXTwSetupPropertyTwCTXAlreadyExist);

                // Set the setter too.
                // Since this is called when 'StartTween' is also called, 
                TwContext.SetSetter(Setter);
            }

            UpdateProperty();
        }
        /// <summary>
        /// Sets up the <see cref="BXTweenProperty{T}"/>.
        /// <br>Calling this after the property is set is equalivent to setting the setter.</br>
        /// </summary>
        /// <param name="Setter"></param>
        public void SetupProperty(BXTweenSetMethod<T> Setter)
        {
            SetupProperty(default, default, Setter);
        }

        /// <summary>
        /// Returns whether if the property is setup.
        /// </summary>
        public static implicit operator bool(BXTweenProperty<T> property)
        {
            return property.IsSetup;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the property's variables after something is changed.
        /// </summary>
        public override void UpdateProperty()
        {
            if (_TwContext == null)
            {
                if (CurrentSettings.diagnosticMode)
                {
                    Debug.LogWarning(BXTweenStrings.DWarn_BXTwUpdatePropertyCTXNull);
                }

                return;
            }

            // -- Set the settings
            // This class is essentially a settings wrapper.
            TwContext.SetDelay(_Delay).SetDuration(_Duration).
                SetCustomCurve(UseTweenCurve ? _TweenCurve : null, !_AllowInterpolationEaseOvershoot).SetEase(_TweenEase).
                SetRepeatAmount(_RepeatAmount).SetRepeatType(_TweenRepeatType);

            // -- Null checks (for the ending actions, we still check null while invoking those)
            if (OnEndAction != null)
            {
                TwContext.SetEndingEvent(OnEndAction);
            }

            // Basically whenever we call 'SetSetter', the BXTweenContext tries whether if the setter is valid by calling it on a try block
            // This type of error checking is dumb so i removed it. (because it resets the object to c# default)
            // Thus making a need of 'applySetter' parameter obsolete.
            if (_Setter != null)
            {
                TwContext.SetSetter(_Setter);
            }
        }

        public void StartTween(T StartValue, T EndValue, BXTweenSetMethod<T> Setter = null)
        {
            if (!IsSetup)
            {
                if (Setter == null)
                {
                    Debug.LogError(BXTweenStrings.Err_SetterFnNull);
                    return;
                }

                // Setup not setup property if the setter isn't null.
                SetupProperty(StartValue, EndValue, Setter);
            }
            else if (Setter != null)
            {
                // Already setup, but wanting to change the setter.
                _Setter = Setter;
            }

            // Make sure to set these values
            TwContext.SetStartValue(StartValue).SetEndValue(EndValue);

            // Update the 'TwContext' because the 'TwContext' may be modified externally and it may not have matching settings with this context.
            UpdateProperty();

            StartTween();
        }

        public void StartTween()
        {
            // ** Parameterless 'StartTween()'
            // This takes the parameters from the tween Context.
            if (_TwContext == null)
            {
                Debug.LogWarning(BXTweenStrings.Warn_BXTwPropertyTwNull);
                return;
            }

            // Stop tween under control 
            var invokeEventOnStop = TwContext.InvokeEventOnStop;
            TwContext.SetInvokeEventsOnStop(InvokeEventOnManualStop);
            if (TwContext.IsRunning)
                StopTween();
            TwContext.SetInvokeEventsOnStop(invokeEventOnStop);

            TwContext.StartTween();
        }

        public void StopTween()
        {
            if (_TwContext == null)
            {
                Debug.LogWarning(BXTweenStrings.Warn_BXTwPropertyTwNull);
                return;
            }

            TwContext.StopTween();
        }
        #endregion
    }

    #region BXTween Property Classes
    [Serializable]
    public sealed class BXTweenPropertyFloat : BXTweenProperty<float>
    {
        public BXTweenPropertyFloat(float duration, float delay = 0f, bool exPol = false, AnimationCurve c = null)
        {
            _Duration = duration;
            _Delay = delay;
            _AllowInterpolationEaseOvershoot = exPol;
            if (c != null)
            {
                _TweenCurve = c;
            }
        }
    }
    [Serializable]
    public sealed class BXTweenPropertyVector2 : BXTweenProperty<Vector2>
    {
        public BXTweenPropertyVector2(float duration, float delay = 0f, bool exPol = false, AnimationCurve c = null)
        {
            _Duration = duration;
            _Delay = delay;
            _AllowInterpolationEaseOvershoot = exPol;
            if (c != null)
            {
                _TweenCurve = c;
            }
        }
    }
    [Serializable]
    public sealed class BXTweenPropertyVector3 : BXTweenProperty<Vector3>
    {
        public BXTweenPropertyVector3(float duration, float delay = 0f, bool exPol = false, AnimationCurve c = null)
        {
            _Duration = duration;
            _Delay = delay;
            _AllowInterpolationEaseOvershoot = exPol;
            if (c != null)
            {
                _TweenCurve = c;
            }
        }
    }
    [Serializable]
    public sealed class BXTweenPropertyColor : BXTweenProperty<Color>
    {
        public BXTweenPropertyColor(float duration, float delay = 0f, bool exPol = false, AnimationCurve c = null)
        {
            _Duration = duration;
            _Delay = delay;
            _AllowInterpolationEaseOvershoot = exPol;
            if (c != null)
            {
                _TweenCurve = c;
            }
        }
    }
    #endregion
}