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
        [SerializeField] protected UnityEngine.Object _TargetObject;

        [SerializeField] protected bool _UseTweenCurve = false;
        [SerializeField] protected bool _AllowInterpolationEaseOvershoot = false;
        [SerializeField] protected AnimationCurve _TweenCurve = DEFAULT_TWCURVE_VALUE;
        [SerializeField] protected EaseType _TweenEase = EaseType.QuadInOut;
        /// <summary>
        /// When this option is <see langword="true"/>, the <see cref="BXTweenCTX{T}.OnEndAction"/> is invoked when 
        /// <see cref="BXTweenCTX{T}.StopTween"/> is called explicitly (without being called by the coroutine end)
        /// </summary>
        public bool InvokeEventOnManualStop = false;
        /// <summary>
        /// An assignable tween field, called when the <c>TwContext</c> (on the inherting generic property class) ends.
        /// </summary>
        public BXTweenUnityEvent OnEndAction;

        /// <summary>
        /// Duration of the tween.
        /// </summary>
        public float Duration
        {
            get { return _Duration; }
            set
            {
                _Duration = value;

                UpdateProperty();
            }
        }
        /// <summary>
        /// Delay of the tween. (Time to wait in seconds before actually starting the main loop)
        /// </summary>
        public float Delay
        {
            get { return _Delay; }
            set
            {
                _Delay = value;

                UpdateProperty();
            }
        }
        /// <summary>
        /// Repeat amount.
        /// <br>0 means no repeat, -1 (and lower) means infinite</br>
        /// </summary>
        public int RepeatAmount
        {
            get { return _RepeatAmount; }
            set
            {
                _RepeatAmount = value;

                UpdateProperty();
            }
        }
        /// <summary>
        /// The target object of the tween.
        /// <br>If this is destroyed, the tween will stop. To get a log when it stops,
        /// switch <see cref="BXTweenSettings.diagnosticMode"/> to be true in <see cref="CurrentSettings"/>.</br>
        /// <br>This also prevents the setter throwing an exception. Basically this object, if not null when the tween was started, is a stop condition.</br>
        /// </summary>
        public UnityEngine.Object TargetObject
        {
            get
            {
                return _TargetObject;
            }
            set
            {
                _TargetObject = value;

                UpdateProperty();
            }
        }
        /// <summary>
        /// Repeat type of the tween.
        /// </summary>
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
        protected readonly static AnimationCurve DEFAULT_TWCURVE_VALUE = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        /// <summary>
        /// Curve of the tween. This shouldn't return null, as it assigns itself to a default.
        /// <br>Setting this to a non-null/null value won't disable/enable the <see cref="TweenEase"/> (unlike <see cref="BXTweenCTX{T}"/>), 
        /// you need to also enable <see cref="UseTweenCurve"/>.</br>
        /// </summary>
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
        /// <summary>
        /// Easing of the tween, using the built-in hardcoded <see cref="BXTweenEase"/> methods.
        /// </summary>
        public EaseType TweenEase
        {
            get { return _TweenEase; }
            set
            {
                _TweenEase = value;

                UpdateProperty();
            }
        }
        /// <summary>
        /// Allows usage of the assigned <see cref="TweenCurve"/>.
        /// </summary>
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
        public abstract ITweenCTX IContext { get; }

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
    /// <br>BXTween contains types with <see cref="SerializableAttribute"/> for <typeparamref name="T"/> as follows;</br>
    /// <br><see cref="BXTweenPropertyFloat"/>, <see cref="BXTweenPropertyVector2"/>, <see cref="BXTweenPropertyVector3"/> and <see cref="BXTweenPropertyColor"/>.</br>
    /// <br>Other types that <see cref="BXTween"/>'s "To" methods supported can be created by defining a class for those types.</br>
    /// <br>If unity finally supports this feature (serializing generic classes that isn't only "List&lt;T&gt;"), you can ignore the previous message.</br>
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

        public override ITweenCTX IContext => TwContext;

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
        /// <summary>
        /// Copies data of this generated context into the given context parameter.
        /// <br>NOTE : <paramref name="ctx"/> will be overwritten, if it's not <see langword="null"/> this will probably cause data loss.
        /// The <paramref name="ctx"/> will also be stopped if it's running.</br>
        /// </summary>
        /// <param name="ctx">Ref context passed.</param>
        public void CopyIntoCTX(ref BXTweenCTX<T> ctx)
        {
            UpdateProperty(); // Update property first

            if (ctx != null)
            {
                // stop ctx to avoid problems
                ctx.StopTween();
            }

            // Do copy
            ctx = new BXTweenCTX<T>(TwContext);
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
                SetRepeatAmount(_RepeatAmount).SetRepeatType(_TweenRepeatType).SetTargetObject(_TargetObject);

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
        /// <summary>
        /// Starts the property's tween.
        /// <br>If <see cref="SetupProperty(BXTweenSetMethod{T})"/> was not called before this method, you must specify a setter, otherwise it will error out.</br>
        /// </summary>
        /// <param name="StartValue">Starting value of the tween. Overrides the previous values set to the <see cref="TwContext"/>.</param>
        /// <param name="EndValue">Ending value of the tween. Overrides the previous values set to the <see cref="TwContext"/>.</param>
        /// <param name="Setter">
        /// The setter value of the <see cref="TwContext"/>. If left blank, <see cref="SetupProperty(BXTweenSetMethod{T})"/> is assumed to be called.
        /// <br/>
        /// <br>If it is not left blank and there is already a valid setter (i.e <see cref="SetupProperty(BXTweenSetMethod{T})"/> is called), the <see cref="TwContext"/>'s setter will be overriden with what you specified.</br>
        /// </param>
        /// <returns><see langword="true"/> if the tween was started successfully.</returns>
        public bool StartTween(T StartValue, T EndValue, BXTweenSetMethod<T> Setter = null)
        {
            if (!IsSetup)
            {
                if (Setter == null)
                {
                    Debug.LogError(BXTweenStrings.Err_SetterFnNull);
                    return false;
                }

                // Caller did not call SetupProperty if the setter isn't null.
                // Do it kindly for the caller implicitly.
                SetupProperty(StartValue, EndValue, Setter);
            }
            else
            {
                if (Setter != null)
                {
                    // Already setup, but wanting to change the setter.
                    _Setter = Setter;
                }

                // Set these values as they are not changed (not calling SetupProperty) when it is already setup
                TwContext.SetStartValue(StartValue).SetEndValue(EndValue);
            }

            // Update the 'TwContext' because the 'TwContext' may be modified externally and it may not have matching settings with this context.
            // (also sets the _Setter into a valid value)
            UpdateProperty();

            return StartTween();
        }
        /// <summary>
        /// Starts the tween without any parameters.
        /// </summary>
        /// <returns><see langword="true"/> if the tween was started successfully.</returns>
        public bool StartTween()
        {
            // ** Parameterless 'StartTween()'
            // This takes the parameters from the tween Context.
            if (_TwContext == null)
            {
                Debug.LogWarning(BXTweenStrings.Warn_BXTwPropertyTwNull);
                return false;
            }

            // Stop tween under control 
            var invokeEventOnStop = TwContext.InvokeEventOnStop;
            TwContext.SetInvokeEventsOnStop(InvokeEventOnManualStop);
            if (TwContext.IsRunning)
                StopTween();
            TwContext.SetInvokeEventsOnStop(invokeEventOnStop);

            TwContext.StartTween();
            return true;
        }
        /// <summary>
        /// Stops the tween. Calling this without <see cref="SetupProperty(BXTweenSetMethod{T})"/> will just print a warning.
        /// </summary>
        public void StopTween()
        {
            // Not a complete fail state, just specifies that the tween was not init.
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