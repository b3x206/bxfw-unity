using System;
using UnityEngine;
using BXFW.Tweening.Next.Events;

namespace BXFW.Tweening.Next
{
    /// <summary>
    /// Contains a typed context.
    /// <br>The actual setters are contained here, along with the other values.</br>
    /// <br>This context handles most of the type related things.</br>
    /// </summary>
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
        /// When called, switches the <see cref="StartValue"/> and <see cref="EndValue"/>.
        /// </summary>
        protected override void OnSwitchTargetValues()
        {
            // EndValue is StartValue
            // StartValue is EndValue (this is called because the valeus has to switch)
            // So switch it regardless of value safety.
            (StartValue, EndValue) = (EndValue, StartValue);
        }

        /// <summary>
        /// The function to get the 'StartValue'.
        /// <br>Used when <see cref="BXSTweenable.IsEndValueRelative"/> and the tween was run, the <see cref="GetNewStartValue"/> is called.</br>
        /// </summary>
        public BXSGetterAction<TValue> GetterAction { get; protected set; }
        /// <summary>
        /// The setter action, called when the tweening is being done.
        /// </summary>
        public BXSSetterAction<TValue> SetterAction { get; protected set; }
        /// <summary>
        /// Gathers the <see cref="StartValue"/> using <see cref="GetterAction"/>.
        /// </summary>
        protected void GetNewStartValue()
        {
            StartValue = GetterAction();
        }

        // -- Interpolation
        /// <summary>
        /// The linear interpolation method to override for the setter of this <typeparamref name="TValue"/> context.
        /// </summary>
        public abstract BXSLerpAction<TValue> LerpAction { get; }
        /// <summary>
        /// An action used for adding two <typeparamref name="TValue"/>'s to each other.
        /// </summary>
        public abstract BXSMathAction<TValue> AddValueAction { get; }
        // - Overrides
        /// <summary>
        /// Evaluates the <see cref="SetterAction"/> with <see cref="LerpAction"/>.
        /// </summary>
        protected internal override void EvaluateTween(float t)
        {
            // Check easing clamping
            float easedTime = EvaluateEasing(t);

            SetterAction(LerpAction(StartValue, EndValue, easedTime));
        }

        // -- Daisy Chain Setters
        /// <summary>
        /// Sets the duration of the tween.
        /// <br>Has effect and will change the duration after the tween was started.</br>
        /// </summary>
        public BXSTweenContext<TValue> SetDuration(float duration)
        {
            m_Duration = duration;
            return this;
        }
        /// <summary>
        /// Sets delay.
        /// <br>Has no effect if the tween has it's <see cref="BXSTweenable.DelayElapsed"/> ticked to 1.</br>
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

            return this;
        }
        /// <summary>
        /// Sets whether if the <see cref="EndValue"/> is relative.
        /// <br>If this is the case, every time the tween is started or repeated, the <see cref="EndValue"/> will be gathered.</br>
        /// </summary>
        public BXSTweenContext<TValue> SetIsEndRelative(bool isRelative)
        {
            m_IsEndValueRelative = isRelative;

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

            m_ID = BXSTween.MainRunner.GetObjectID(m_IDObject);

            return this;
        }

        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnStartAction"/> event.
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
                    SetStartActionValue(action);
                    break;
            }
            return this;
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnUpdateAction"/> event.
        /// </summary>
        public BXSTweenContext<TValue> SetUpdateAction(BXSAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            switch (setMode)
            {
                case EventSetMode.Subtract:
                    OnUpdateAction -= action;
                    break;
                case EventSetMode.Add:
                    OnUpdateAction += action;
                    break;

                default:
                case EventSetMode.Equals:
                    SetUpdateActionValue(action);
                    break;
            }
            return this;
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnPauseAction"/> event.
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
                    SetPauseActionValue(action);
                    break;
            }
            return this;
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnEndAction"/> event.
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
                    SetEndActionValue(action);
                    break;
            }
            return this;
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.TickConditionAction"/> action.
        /// <br>Return the suitable <see cref="TickConditionSuspendType"/> in the function.</br>
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
            base.Play();

            /// Calculate Start/End values
            /// The <see cref="EvaluateTween(float)"/> will do the interpolation.
            if (IsEndValueRelative)
            {
                GetNewStartValue();

                // Check relativeness
                if (IsEndValueRelative)
                {
                    EndValue = AddValueAction(StartValue, EndValue);
                }
            }
        }
    }
}
