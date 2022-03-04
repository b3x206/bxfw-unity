/* --------------------------------------------------- *
 * |             Created in : 25.12.2020             | *
 * |                  Made by : B3X                  | *
 * |-------------------------------------------------| *
 * | More Info : This is the second version of       | *
 * | the CTween tweening script.                     | *
 * | C stands for coroutine.                         | *
 * |                                                 | *
 * | ------------------------------------------------| */
/** -------------------------------------------------- 
// GENERAL TODO :
// 1 : <see cref="BXFW.Tweening.CTweenCTX{T}"/>'s <see cref="BXFW.Tweening.RepeatType.PingPong"/> doesn't work as ping-pong,
//     but rather as <see cref="BXFW.Tweening.RepeatType.Reset"/>. Reset seems to work fine.
// 2 : <see cref="Component.transform"/> seems to internally call <see cref="Component.GetComponent{T}"/>.
//     Make the tweening method cache the <see cref="Component.GetComponent{T}"/> call. 
// BUG FIX : 
// 1 : Creating a <see cref="BXFW.Tweening.CTweenProperty{T}"/> with delay and calling itself's <see cref="BXFW.Tweening.CTweenProperty{T}.StartTween"/> on it's
//     <see cref="BXFW.Tweening.CTweenProperty{T}.TwContext"/>'s ending action doesn't start the tween. (Only occurs on delayed tweens? need testing)
* -------------------------------------------------- **/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static BXFW.Tweening.CTween;

#if UNITY_EDITOR
using BXFW.Tweening.Editor;
#endif

namespace BXFW.Tweening
{
    // Visual studio
#pragma warning disable IDE0051 // Hide unused info

    /// Solution for stylized print strings.
    /// <summary>
    /// Constant strings for <see cref="CTween"/> messages.
    /// <para>-Note about the #if UNITY_EDITOR regions:</para>
    /// <br>----These use rich text tags which will not look pretty on stdout.</br>
    /// <br>----So write no rich text tags (pass no string which look like "") on non UNITY_EDITOR regions.</br>
    /// </summary>
    /// Maybe TODO : Use exceptions for Err, make a safe mode toggle for no exceptions & safe mode never returns null.
    internal static class CTweenStrings
    {
        // -- Formatters
        #region (Private) Rich Text Formatting
        private static string LogRich(string RichFormatTarget, bool Bold = false)
        {
#if UNITY_EDITOR
            return string.Format
                (Bold ? "<b><color=#ad9a6f>{0}</color></b>" : "<color=#ad9a6f>{0}</color>", RichFormatTarget);
#else
            return RichFormatTarget;
#endif
        }
        private static string WarnRich(string RichFormatTarget, bool Bold = false)
        {
#if UNITY_EDITOR
            return string.Format
                (Bold ? "<b><color=#ffcc00>{0}</color></b>" : "<color=#ffcc00>{0}</color>", RichFormatTarget);
#else
            return RichFormatTarget;
#endif
        }
        private static string ErrRich(string RichFormatTarget, bool Bold = false)
        {
#if UNITY_EDITOR
            return string.Format
                (Bold ? "<b><color=#8634eb>{0}</color></b>" : "<color=#8634eb>{0}</color>", RichFormatTarget);
#else
            return RichFormatTarget;
#endif
        }
        #endregion

#if UNITY_EDITOR
        // -- Debug Tools
        /// <summary>
        /// <c>EDITOR ONLY</c> | List all registered fields in <see cref="CTweenStrings"/>.
        /// <br>Outputs log to Debug.Log.</br>
        /// </summary>
        public static void ListStrings()
        {
            Debug.Log("<b>[ListStrings] Listing all fields in 'CTweenStrings'.</b>");

            foreach (FieldInfo field in typeof(CTweenStrings).GetFields())
            {
                // Pass null as it's static.
                Debug.LogFormat("Field | Name : {0} | ToString : {1}", field.Name, field.GetValue(null));
            }

            Debug.Log("<b>[ListStrings] Note that dynamic strings are not outputtable.</b>");
        }
#endif

        // -- Strings
        #region Info-Logs
        // Non-Dynamic
        /// <see cref="CTween.To"/>
        public static readonly string Log_CTwDurationZero =
            string.Format("{0}{1}",
                LogRich("[CTween]->", true),
                LogRich("The duration is less than zero. Please note that the return is null."));
        // Dynamic
        public static string GetLog_CTwCTXCustomCurveInvalid(EaseType TypeAttemptSet)
        {
            return string.Format
                (   // Main String
                    "{0}{1}",
                    // Format List
                    LogRich("[CTweenCTX::SetEase]->", true),
                    LogRich(string.Format("Cannot set ease to predefined {0} mode because there is a Custom Ease Curve set.\nPlease set custom curve to null (Use ::SetCustomCurve()) if you want to set predefined eases.", TypeAttemptSet.ToString()))
                );
        }
        #endregion

        #region Warnings
        // Non-Dynamic
        // All
        /// <see cref="CTweenCore"/>
        public static readonly string Warn_CTwCoreAlreadyExist =
            string.Format("{0}{1}",
                WarnRich("[CTweenCore::Editor_InitilazeCTw]->", true),
                LogRich("The CTween core is already init and it already contains a editor object."));
        public static readonly string Warn_CTwPropertyTwNull =
            string.Format("{0}{1}",
                WarnRich("[CTweenContext]", true),
                LogRich("The tween property is null. This might be a bad error."));
        public static readonly string Warn_CTwCTXTimeCurveNull =
            string.Format("{0}{1}",
                WarnRich("[CTweenCTX::SetCustomCurve]", true),
                LogRich("The tween property is null. This might be a bad error."));
        public static readonly string Warn_CTwCurrentIteratorNull =
            string.Format("{0}{1}",
                WarnRich("[CTween::StopTween]->", true),
                LogRich("The current running coroutine is null. Probably an internal error, that is not very important."));
#if UNITY_EDITOR // Editor Only
        /// <see cref="CTween.To"/> on editor.
        public static readonly string Warn_EditorCTwCoreNotInit =
            string.Format("{0}{1}",
                WarnRich("[CTween::To(OnEditor)]->", true),
                LogRich("Please make sure you initilaze CTween for editor playback. (by calling Editor_InitilazeCTw())"));
#endif
        // Dynamic

        #endregion

        #region Errors
        // Non-Dynamic
        public static readonly string Err_CTwCoreNotInit =
            string.Format("{0}{1}",
                ErrRich("[CTweenCore(Critical)]->", true),
                LogRich("The 'Current' reference is null. Make sure the Core initilazes properly."));
        public static readonly string Err_CTwCTXFailUpdate =
            string.Format("{0}{1}",
                ErrRich("[CTweenCTX(Critical)]->", true),
                LogRich("The 'IteratorCoroutine' given variable is null even after update."));

        public static readonly string Err_SetterFnNull =
            string.Format("{0}{1}",
                ErrRich("[CTween(General Error)]->", true),
                LogRich("The given setter function is null or broken. This can happen in these classes : 'CTweenCTX<T>', 'CTween(To Methods)' or 'CTweenProperty<T>'."));
        public static readonly string Err_TargetNull =
            string.Format("{0}{1}",
                ErrRich("[CTween(Error, Extension Method)]->", true),
                LogRich("The given target is null. Returning null context, expect exceptions."));

        public static readonly string Err_CTwCTXSetterNull =
            string.Format("{0}{1}",
                ErrRich("[CTweenCTX::SetSetter]->"),
                LogRich("Couldn't set action as the variable passed was null."));
        public static readonly string Err_CTwCTXNoIterator =
            string.Format("{0}{1}",
                ErrRich("[CTweenCTX(General Error)]->"),
                LogRich("The IteratorCoroutine variable is null."));


        // Dynamic (Needs to be generated dynamically or smth)
        /// <see cref="CTween.To"/> methods.
        public static string GetErr_NonTweenableType(string ReasonNonTwn)
        {
            return string.Format
                (   // Main String
                    "{0}{1}",
                    // Format List
                    ErrRich("[CTweenCore(Error)]->", true),
                    LogRich(string.Format("The type ({0}) is not tweenable!", ReasonNonTwn))
                );
        }
        public static string GetErr_ContextInvalidMsg(string ReasonInvalid)
        {
            return string.Format
                (   // Main String
                    "{0}{1}",
                    // Format List
                    ErrRich("[CTweenCore(Critical)]", true),
                    LogRich(string.Format("The given context is not valid. Reason : \"{0}\"", ReasonInvalid))
                );
        }
        public static string GetErr_CTwCTXSetterExcept(Exception e)
        {
            if (e == null)
                e = new Exception("ERROR : The passed exception was null.");

            return string.Format
                (   // Main String
                    "{0}{1}",
                    // Format List
                    ErrRich("[CTweenCTX::SetSetter]", true),
                    LogRich(
                        string.Format(
                        "Couldn't set action as an exception occured.\nMore detail : {0}\n{1}",
                        e.Message, e.StackTrace))
                );
        }
        public static string GetErr_CTwCTXCtorExcept(Exception e)
        {
            if (e == null)
                e = new Exception("ERROR (how) : The passed exception was null.");

            return string.Format
                (   // Main String
                    "{0}{1}",
                    // Format List
                    ErrRich("[CTweenCTX::(Critical)]", true),
                    LogRich(
                        string.Format(
                        "An exception occured while constructing class.\n--Exception Details--\nMsg:{0}\nStackTrace:{1}",
                        e.Message, e.StackTrace))
                );
        }
        #endregion
    }

    #region CTween Core Classes
    [ExecuteAlways()]
    public class CTweenCore : MonoBehaviour
    {
        #region CTweenCore Primary Functions
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitilazeCTw()
        {
            // Unity Editor Checks
#if UNITY_EDITOR
            if (Current != null)
            { Editor_DisposeCTwObject(); }
#endif

            // Standard creation
            GameObject twObject = new GameObject("CTweenCore");
            DontDestroyOnLoad(twObject);

            var CTwComponent = twObject.AddComponent<CTweenCore>();
            Current = CTwComponent;
        }

        #region Editor Playback (Experimental)
#if UNITY_EDITOR
        private GameObject previewObject;
        /// <summary>
        /// Initilaze CTween on editor.
        /// The object gets destroyed after the tweens are done.
        /// </summary>
        public static void Editor_InitilazeCTw()
        {
            if (Current != null)
            {
                if (Current.previewObject != null)
                {
                    Debug.LogWarning(CTweenStrings.Warn_CTwCoreAlreadyExist);
                    return;
                }
            }

            GameObject previewObject = new GameObject
            {
                name = "Preview CTweenCore",
                tag = "EditorOnly"
            };

            var CTwComp = previewObject.AddComponent<CTweenCore>();
            Current = CTwComp;
            Current.previewObject = previewObject;
        }
        /// <summary>
        /// Dispose the object.
        /// </summary>
        public static void Editor_DisposeCTwObject()
        {
            if (Current == null)
            {
                // Logging a message here is annoying..
                return;
            }

            DestroyImmediate(Current.previewObject);
            Current = null;
        }
#endif
        #endregion

        #endregion

        #region To Methods
        public IEnumerator To(CTweenCTX<float> ctx)
        {
        // Notes (Do not copy to other methods) :
        // Note that do your enchantments in the 'float' To Method,
        // than port it over to more complicated types.
        // Note that this is boilerplate code as everything here is kinda type independent except for the method
        // (thank god for 'var' keyword)

        _Start:

            // -- Check Context
            if (!ctx.ContextIsValid)
            {
                Debug.LogError(CTweenStrings.GetErr_ContextInvalidMsg(ctx.Debug_GetContextValidMsg()));
                yield break;
            }

            // -- Start Tween Coroutine -- //
            yield return new WaitForEndOfFrame();
            if (ctx.StartDelay > 0.0f)
            { yield return new WaitForSeconds(ctx.StartDelay); }

            // Start Interpolator
            float Elapsed = 0.0f;
            bool UseCustom = ctx.CustomTimeCurve != null;
            while (Elapsed <= 1.0f)
            {
                // Check if the timescale is tampered with
                // if it's below zero, just skip the frame
                if (Time.timeScale <= 0.0f)
                { yield return null; }

                // Set lerp (Conditional for unclamped)
                var SetValue = ctx.UseUnclampedLerp
                    ? Mathf.LerpUnclamped(ctx.StartValue, ctx.EndValue, ctx.TimeSetLerp(Elapsed))
                    : Mathf.Lerp(ctx.StartValue, ctx.EndValue, ctx.TimeSetLerp(Elapsed));

                ctx.SetterFunction(SetValue);
                Elapsed += Time.deltaTime / ctx.Duration;
                yield return null;
            }
            ctx.SetterFunction(ctx.EndValue);
            // End Interpolator

            // Repeating
            if (ctx.RepeatAmount != 0)
            {
                // Invoke Ending On Repeat?
                if (ctx.InvokeEventOnRepeat)
                {
                    ctx.OnEndAction?.Invoke();
                    ctx.OnEndAction_UnityEvent?.Invoke();
                }

                // Repeat Amount Rules : If repeat amount is bigger than 0, subtract until 0
                // If repeat amount is not 0 (negative number), repeat indefinetly until StopTween() was called.
                if (ctx.RepeatAmount > 0)
                {
                    ctx.SetRepeatAmount(ctx.RepeatAmount - 1);
                }

                // Do a swap between values.
                // TODO : Save this value switching so that when the user stops the tween it defaults back.
                // TODO Needs to be tested properly.
                if (ctx.RepeatType == RepeatType.PingPong)
                {
                    var st_value = ctx.StartValue;
                    var end_value = ctx.EndValue;

                    ctx.SetStartValue(end_value).SetEndValue(st_value);
                }

                goto _Start;
            }
            // End Repeating

            // Ending Actions
            ctx.OnEndAction?.Invoke();
            ctx.PersistentOnEndAction?.Invoke();
            ctx.OnEndAction_UnityEvent?.Invoke();

            // Call this to avoid IEnumerator errors.
            // As this is technically a (OnTweenEnd) call.
            ctx.StopTween();
        }
        public IEnumerator To(CTweenCTX<Color> ctx)
        {
        _Start:

            // -- Check Context
            if (!ctx.ContextIsValid)
            {
                Debug.LogError(CTweenStrings.GetErr_ContextInvalidMsg(ctx.Debug_GetContextValidMsg()));
                yield break;
            }

            // -- Start Tween Coroutine -- //
            yield return new WaitForEndOfFrame();
            if (ctx.StartDelay > 0.0f)
            { yield return new WaitForSeconds(ctx.StartDelay); }

            // Start Interpolator
            float Elapsed = 0.0f;
            bool UseCustom = ctx.CustomTimeCurve != null;
            while (Elapsed <= 1.0f)
            {
                // Check if the timescale is tampered with
                // if it's below zero, just skip the frame
                if (Time.timeScale <= 0.0f)
                { yield return null; }

                // Set lerp (Conditional for unclamped)
                var SetValue = ctx.UseUnclampedLerp
                    ? Color.LerpUnclamped(ctx.StartValue, ctx.EndValue, ctx.TimeSetLerp(Elapsed))
                    : Color.Lerp(ctx.StartValue, ctx.EndValue, ctx.TimeSetLerp(Elapsed));

                ctx.SetterFunction(SetValue);
                Elapsed += Time.deltaTime / ctx.Duration;
                yield return null;
            }
            ctx.SetterFunction(ctx.EndValue);
            // End Interpolator

            // Repeating
            if (ctx.RepeatAmount != 0)
            {
                // Invoke Ending On Repeat?
                if (ctx.InvokeEventOnRepeat)
                {
                    ctx.OnEndAction?.Invoke();
                    ctx.OnEndAction_UnityEvent?.Invoke();
                }

                // Repeat Amount Rules : If repeat amount is bigger than 0, subtract until 0
                // If repeat amount is not 0 (negative number), repeat indefinetly until StopTween() was called.
                if (ctx.RepeatAmount > 0)
                {
                    ctx.SetRepeatAmount(ctx.RepeatAmount - 1);
                }

                // Do a swap between values.
                if (ctx.RepeatType == RepeatType.PingPong)
                {
                    var st_value = ctx.StartValue;
                    var end_value = ctx.EndValue;

                    ctx.SetStartValue(st_value).SetEndValue(end_value);
                }

                goto _Start;
            }
            // End Repeating

            // Ending Actions
            ctx.OnEndAction?.Invoke();
            ctx.PersistentOnEndAction?.Invoke();
            ctx.OnEndAction_UnityEvent?.Invoke();

            // Call this to avoid IEnumerator errors.
            // As this is technically a (OnTweenEnd) call.
            ctx.StopTween();
        }
        public IEnumerator To(CTweenCTX<Vector2> ctx)
        {
        _Start:

            // -- Check Context
            if (!ctx.ContextIsValid)
            {
                Debug.LogError(CTweenStrings.GetErr_ContextInvalidMsg(ctx.Debug_GetContextValidMsg()));
                yield break;
            }

            // -- Start Tween Coroutine -- //
            yield return new WaitForEndOfFrame();
            if (ctx.StartDelay > 0.0f)
            { yield return new WaitForSeconds(ctx.StartDelay); }

            // Start Interpolator
            float Elapsed = 0.0f;
            bool UseCustom = ctx.CustomTimeCurve != null;
            while (Elapsed <= 1.0f)
            {
                // Check if the timescale is tampered with
                // if it's below zero, just skip the frame
                if (Time.timeScale <= 0.0f)
                { yield return null; }

                // Set lerp (Conditional for unclamped)
                var SetValue = ctx.UseUnclampedLerp
                    ? Vector2.LerpUnclamped(ctx.StartValue, ctx.EndValue, ctx.TimeSetLerp(Elapsed))
                    : Vector2.Lerp(ctx.StartValue, ctx.EndValue, ctx.TimeSetLerp(Elapsed));

                ctx.SetterFunction(SetValue);
                Elapsed += Time.deltaTime / ctx.Duration;
                yield return null;
            }
            ctx.SetterFunction(ctx.EndValue);
            // End Interpolator

            // Repeating
            if (ctx.RepeatAmount != 0)
            {
                // Invoke Ending On Repeat?
                if (ctx.InvokeEventOnRepeat)
                {
                    ctx.OnEndAction?.Invoke();
                    ctx.OnEndAction_UnityEvent?.Invoke();
                }

                // Repeat Amount Rules : If repeat amount is bigger than 0, subtract until 0
                // If repeat amount is not 0 (negative number), repeat indefinetly until StopTween() was called.
                if (ctx.RepeatAmount > 0)
                {
                    ctx.SetRepeatAmount(ctx.RepeatAmount - 1);
                }

                // Do a swap between values.
                if (ctx.RepeatType == RepeatType.PingPong)
                {
                    var st_value = ctx.StartValue;
                    var end_value = ctx.EndValue;

                    ctx.SetStartValue(st_value).SetEndValue(end_value);
                }

                goto _Start;
            }
            // End Repeating

            // Ending Actions
            ctx.OnEndAction?.Invoke();
            ctx.PersistentOnEndAction?.Invoke();
            ctx.OnEndAction_UnityEvent?.Invoke();

            // Call this to avoid IEnumerator errors.
            // As this is technically a (OnTweenEnd) call.
            ctx.StopTween();
        }
        public IEnumerator To(CTweenCTX<Vector3> ctx)
        {
        _Start:

            // -- Check Context
            if (!ctx.ContextIsValid)
            {
                Debug.LogError(CTweenStrings.GetErr_ContextInvalidMsg(ctx.Debug_GetContextValidMsg()));
                yield break;
            }

            // -- Start Tween Coroutine -- //
            yield return new WaitForEndOfFrame();
            if (ctx.StartDelay > 0.0f)
            { yield return new WaitForSeconds(ctx.StartDelay); }

            // Start Interpolator
            float Elapsed = 0.0f;
            bool UseCustom = ctx.CustomTimeCurve != null;
            while (Elapsed <= 1.0f)
            {
                // Check if the timescale is tampered with
                // if it's below zero, just skip the frame
                if (Time.timeScale <= 0.0f)
                { yield return null; }

                // Set lerp (Conditional for unclamped)
                var SetValue = ctx.UseUnclampedLerp
                    ? Vector3.LerpUnclamped(ctx.StartValue, ctx.EndValue, ctx.TimeSetLerp(Elapsed))
                    : Vector3.Lerp(ctx.StartValue, ctx.EndValue, ctx.TimeSetLerp(Elapsed));

                ctx.SetterFunction(SetValue);
                Elapsed += Time.deltaTime / ctx.Duration;
                yield return null;
            }
            ctx.SetterFunction(ctx.EndValue);
            // End Interpolator

            // Repeating
            if (ctx.RepeatAmount != 0)
            {
                // Invoke Ending On Repeat?
                if (ctx.InvokeEventOnRepeat)
                {
                    ctx.OnEndAction?.Invoke();
                    ctx.OnEndAction_UnityEvent?.Invoke();
                }

                // Repeat Amount Rules : If repeat amount is bigger than 0, subtract until 0
                // If repeat amount is not 0 (negative number), repeat indefinetly until StopTween() was called.
                if (ctx.RepeatAmount > 0)
                {
                    ctx.SetRepeatAmount(ctx.RepeatAmount - 1);
                }

                // Do a swap between values.
                if (ctx.RepeatType == RepeatType.PingPong)
                {
                    var st_value = ctx.StartValue;
                    var end_value = ctx.EndValue;

                    ctx.SetStartValue(st_value).SetEndValue(end_value);
                }

                goto _Start;
            }
            // End Repeating

            // Ending Actions
            ctx.OnEndAction?.Invoke();
            ctx.PersistentOnEndAction?.Invoke();
            ctx.OnEndAction_UnityEvent?.Invoke();

            // Call this to avoid IEnumerator errors.
            // As this is technically a (OnTweenEnd) call.
            ctx.StopTween();
        }
        public IEnumerator To(CTweenCTX<Quaternion> ctx)
        {
        _Start:

            // -- Check Context
            if (!ctx.ContextIsValid)
            {
                Debug.LogError(CTweenStrings.GetErr_ContextInvalidMsg(ctx.Debug_GetContextValidMsg()));
                yield break;
            }

            // -- Start Tween Coroutine -- //
            yield return new WaitForEndOfFrame();
            if (ctx.StartDelay > 0.0f)
            { yield return new WaitForSeconds(ctx.StartDelay); }

            // Start Interpolator
            float Elapsed = 0.0f;
            bool UseCustom = ctx.CustomTimeCurve != null;
            while (Elapsed <= 1.0f)
            {
                // Check if the timescale is tampered with
                // if it's below zero, just skip the frame
                if (Time.timeScale <= 0.0f)
                { yield return null; }

                // Set lerp (Conditional for unclamped)
                var SetValue = ctx.UseUnclampedLerp
                    ? Quaternion.SlerpUnclamped(ctx.StartValue, ctx.EndValue, ctx.TimeSetLerp(Elapsed))
                    : Quaternion.Slerp(ctx.StartValue, ctx.EndValue, ctx.TimeSetLerp(Elapsed));

                ctx.SetterFunction(SetValue);
                Elapsed += Time.deltaTime / ctx.Duration;
                yield return null;
            }
            ctx.SetterFunction(ctx.EndValue);
            // End Interpolator

            // Repeating
            if (ctx.RepeatAmount != 0)
            {
                // Invoke Ending On Repeat?
                if (ctx.InvokeEventOnRepeat)
                {
                    ctx.OnEndAction?.Invoke();
                    ctx.OnEndAction_UnityEvent?.Invoke();
                }

                // Repeat Amount Rules : If repeat amount is bigger than 0, subtract until 0
                // If repeat amount is not 0 (negative number), repeat indefinetly until StopTween() was called.
                if (ctx.RepeatAmount > 0)
                {
                    ctx.SetRepeatAmount(ctx.RepeatAmount - 1);
                }

                // Do a swap between values.
                if (ctx.RepeatType == RepeatType.PingPong)
                {
                    var st_value = ctx.StartValue;
                    var end_value = ctx.EndValue;

                    ctx.SetStartValue(st_value).SetEndValue(end_value);
                }

                goto _Start;
            }
            // End Repeating

            // Ending Actions
            ctx.OnEndAction?.Invoke();
            ctx.PersistentOnEndAction?.Invoke();
            ctx.OnEndAction_UnityEvent?.Invoke();

            // Call this to avoid IEnumerator errors.
            // As this is technically a (OnTweenEnd) call.
            ctx.StopTween();
        }
        public IEnumerator To(CTweenCTX<Matrix4x4> ctx)
        {
        _Start:

            // -- Check Context
            if (!ctx.ContextIsValid)
            {
                Debug.LogError(CTweenStrings.GetErr_ContextInvalidMsg(ctx.Debug_GetContextValidMsg()));
                yield break;
            }

            // -- Start Tween Coroutine -- //
            yield return new WaitForEndOfFrame();
            if (ctx.StartDelay > 0.0f)
            { yield return new WaitForSeconds(ctx.StartDelay); }

            // Start Interpolator
            float Elapsed = 0.0f;
            bool UseCustom = ctx.CustomTimeCurve != null;
            while (Elapsed <= 1.0f)
            {
                // Check if the timescale is tampered with
                // if it's below zero, just skip the frame
                if (Time.timeScale <= 0.0f)
                { yield return null; }

                // Set lerp (Conditional for unclamped)
                var SetValue = ctx.UseUnclampedLerp
                    ? CTweenCustomLerp.MatrixLerpUnclamped(ctx.StartValue, ctx.EndValue, ctx.TimeSetLerp(Elapsed))
                    : CTweenCustomLerp.MatrixLerp(ctx.StartValue, ctx.EndValue, ctx.TimeSetLerp(Elapsed));

                ctx.SetterFunction(SetValue);
                Elapsed += Time.deltaTime / ctx.Duration;
                yield return null;
            }
            ctx.SetterFunction(ctx.EndValue);
            // End Interpolator

            // Repeating
            if (ctx.RepeatAmount != 0)
            {
                // Invoke Ending On Repeat?
                if (ctx.InvokeEventOnRepeat)
                {
                    ctx.OnEndAction?.Invoke();
                    ctx.OnEndAction_UnityEvent?.Invoke();
                }

                // Repeat Amount Rules : If repeat amount is bigger than 0, subtract until 0
                // If repeat amount is not 0 (negative number), repeat indefinetly until StopTween() was called.
                if (ctx.RepeatAmount > 0)
                {
                    ctx.SetRepeatAmount(ctx.RepeatAmount - 1);
                }

                // Do a swap between values.
                if (ctx.RepeatType == RepeatType.PingPong)
                {
                    var st_value = ctx.StartValue;
                    var end_value = ctx.EndValue;

                    ctx.SetStartValue(st_value).SetEndValue(end_value);
                }

                goto _Start;
            }
            // End Repeating

            // Ending Actions
            ctx.OnEndAction?.Invoke();
            ctx.PersistentOnEndAction?.Invoke();
            ctx.OnEndAction_UnityEvent?.Invoke();

            // Call this to avoid IEnumerator errors.
            // As this is technically a (OnTweenEnd) call.
            ctx.StopTween();
        }

        // -- Animated To (maybe)
        /*
        private IEnumerator To(CTweenCTX<int> f_ctx, int FPS = 60)
        {
            if (!f_ctx.ContextIsValid)
            {
                Debug.LogError(CTweenStrings.GetErr_ContextInvalidMsg(f_ctx.Debug_GetContextValidMsg()));
                yield break;
            }

            // Wait for all the commands to get through.
            yield return new WaitForEndOfFrame();

            if (f_ctx.StartDelay > 0.0f)
            { yield return new WaitForSeconds(f_ctx.StartDelay); }

            // (1000 / FPS) returns ms delay, splitting with 1000 to make it float delay.
            // TODO : Make this function proper and find a more efficient way of waiting for seconds.
            // - Maybe try FixedUpdate delegate running coroutine tied to CTweenCore?
            // yield return new WaitForSeconds((1000 / FPS) / 1000f);

            yield return null;
        }
        */
        #endregion
    }

    /// <summary>
    /// Custom Lerp methods used by CTween.
    /// </summary>
    public static class CTweenCustomLerp
    {
        public static Matrix4x4 MatrixLerp(Matrix4x4 src, Matrix4x4 dest, float time)
        {
            Matrix4x4 ret = new Matrix4x4();

            for (int i = 0; i < 16; i++)
            {
                ret[i] = Mathf.Lerp(src[i], dest[i], time);
            }

            return ret;
        }
        public static Matrix4x4 MatrixLerpUnclamped(Matrix4x4 src, Matrix4x4 dest, float time)
        {
            Matrix4x4 ret = new Matrix4x4();

            for (int i = 0; i < 16; i++)
            {
                ret[i] = Mathf.LerpUnclamped(src[i], dest[i], time);
            }

            return ret;
        }
    }

    /// <summary>
    /// Coroutine based tweening system.
    /// Contains all the base clases and others.
    /// </summary>
    public static class CTween
    {
        public static CTweenCore Current;
        public static List<ITweenCTX> CurrentRunningTweens = new List<ITweenCTX>();

        private static readonly MethodInfo[] CTweenMethods = typeof(CTween).GetMethods();

        #region Utility
        /// <summary>
        /// Utility for checking if the type is tweenable.
        /// </summary>
        /// <param name="t">Type to check.</param>
        /// <returns>Bool = If it's tweenable | MethodInfo = For usage if tweenable</returns>
        public static KeyValuePair<bool, MethodInfo> IsTweenableType(Type t)
        {
            /* Get method if it's tweenable (dynamic) */
            var info = CTweenMethods.Single(x => x.Name == nameof(To) && x.GetParameters()[0].ParameterType == t);
            return new KeyValuePair<bool, MethodInfo>(info != null, info);
        }
        /// <summary>
        /// Get the status of CTween.
        /// </summary>
        public static string CTweenStatus()
        {
            return string.Format("Current Running Tween Amount : {0}", CurrentRunningTweens.Count);
        }
        #endregion

        #region Context Creation (To Methods)

        #region Static To
        /// <summary>
        /// Create a tween manually using this method.
        /// </summary>
        /// <param name="StartValue">The start value.</param>
        /// <param name="TargetValue">The end value.</param>
        /// <param name="Setter">The setter. Pass your function.</param>
        /// <param name="Duration">Length of tween.</param>
        /// <param name="TargetObject">Control the null conditions and setters.</param>
        /// <param name="StartTween">Should the tween start immediately after creation?</param>
        public static CTweenCTX<float> To(float StartValue, float TargetValue, float Duration, CTwSetMethod<float> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check CTweenCore --     //
#if UNITY_EDITOR
            // Editor checks
            if (Current == null && !Application.isPlaying && Application.isEditor)
            {
                Debug.LogWarning(CTweenStrings.Warn_EditorCTwCoreNotInit);
                CTweenCore.Editor_InitilazeCTw();
            }
#endif
            // Runtime checks
            if (Current == null)
            {
                Debug.LogError(CTweenStrings.Err_CTwCoreNotInit);
                return null;
            }
            // -- End Check CTweenCore -- //

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(CTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0.0f)
            {
                Debug.Log(CTweenStrings.Log_CTwDurationZero);
                Setter?.Invoke(TargetValue);
                return null;
            }
            // -- End Check Method Params -- //

            // -- Make Context -- //
            var Context = new CTweenCTX<float>
                (StartValue, TargetValue, TargetObject, Duration, Setter,
                // Note that the below method is special, it returns the target coroutine with new contexts.
                (CTweenCTX<float> ctx) => { return Current.To(ctx); });
            if (StartTween)
            { Context.StartTween(); }
            CurrentRunningTweens.Add(Context);

            // Return Context
            return Context;
        }
        /// <summary>
        /// Create a tween manually using this method.
        /// </summary>
        /// <param name="StartValue">The start value.</param>
        /// <param name="TargetValue">The end value.</param>
        /// <param name="Setter">The setter. Pass your function.</param>
        /// <param name="Duration">Length of tween.</param>
        /// <param name="TargetObject">Control the null conditions and setters.</param>
        /// <param name="StartTween">Should the tween start immediately after creation?</param>
        public static CTweenCTX<Color> To(Color StartValue, Color TargetValue, float Duration, CTwSetMethod<Color> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check CTweenCore --     //
#if UNITY_EDITOR
            // Editor checks
            if (Current == null && !Application.isPlaying && Application.isEditor)
            {
                Debug.LogWarning(CTweenStrings.Warn_EditorCTwCoreNotInit);
                CTweenCore.Editor_InitilazeCTw();
            }
#endif
            // Runtime checks
            if (Current == null)
            {
                Debug.LogError(CTweenStrings.Err_CTwCoreNotInit);
                return null;
            }
            // -- End Check CTweenCore -- //

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(CTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0.0f)
            {
                Debug.Log(CTweenStrings.Log_CTwDurationZero);
                Setter?.Invoke(TargetValue);
                return null;
            }
            // -- End Check Method Params -- //

            // -- Make Context -- //
            var Context = new CTweenCTX<Color>
                (StartValue, TargetValue, TargetObject, Duration, Setter,
                // Note that the below method is special, it returns the target coroutine with new contexts.
                (CTweenCTX<Color> ctx) => { return Current.To(ctx); });
            if (StartTween)
            { Context.StartTween(); }
            CurrentRunningTweens.Add(Context);

            // Return Context
            return Context;
        }
        /// <summary>
        /// Create a tween manually using this method.
        /// </summary>
        /// <param name="StartValue">The start value.</param>
        /// <param name="TargetValue">The end value.</param>
        /// <param name="Setter">The setter. Pass your function.</param>
        /// <param name="Duration">Length of tween.</param>
        /// <param name="TargetObject">Control the null conditions and setters.</param>
        /// <param name="StartTween">Should the tween start immediately after creation?</param>
        public static CTweenCTX<Vector2> To(Vector2 StartValue, Vector2 TargetValue, float Duration, CTwSetMethod<Vector2> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check CTweenCore --     //
#if UNITY_EDITOR
            // Editor checks
            if (Current == null && !Application.isPlaying && Application.isEditor)
            {
                Debug.LogWarning(CTweenStrings.Warn_EditorCTwCoreNotInit);
                CTweenCore.Editor_InitilazeCTw();
            }
#endif
            // Runtime checks
            if (Current == null)
            {
                Debug.LogError(CTweenStrings.Err_CTwCoreNotInit);
                return null;
            }
            // -- End Check CTweenCore -- //

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(CTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0.0f)
            {
                Debug.Log(CTweenStrings.Log_CTwDurationZero);
                Setter?.Invoke(TargetValue);
                return null;
            }
            // -- End Check Method Params -- //

            // -- Make Context -- //
            var Context = new CTweenCTX<Vector2>
                (StartValue, TargetValue, TargetObject, Duration, Setter,
                // Note that the below method is special, it returns the target coroutine with new contexts.
                (CTweenCTX<Vector2> ctx) => { return Current.To(ctx); });
            if (StartTween)
            { Context.StartTween(); }
            CurrentRunningTweens.Add(Context);

            // Return Context
            return Context;
        }
        /// <summary>
        /// Create a tween manually using this method.
        /// </summary>
        /// <param name="StartValue">The start value.</param>
        /// <param name="TargetValue">The end value.</param>
        /// <param name="Setter">The setter. Pass your function.</param>
        /// <param name="Duration">Length of tween.</param>
        /// <param name="TargetObject">Control the null conditions and setters.</param>
        /// <param name="StartTween">Should the tween start immediately after creation?</param>
        public static CTweenCTX<Vector3> To(Vector3 StartValue, Vector3 TargetValue, float Duration, CTwSetMethod<Vector3> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check CTweenCore --     //
#if UNITY_EDITOR
            // Editor checks
            if (Current == null && !Application.isPlaying && Application.isEditor)
            {
                Debug.LogWarning(CTweenStrings.Warn_EditorCTwCoreNotInit);
                CTweenCore.Editor_InitilazeCTw();
            }
#endif
            // Runtime checks
            if (Current == null)
            {
                Debug.LogError(CTweenStrings.Err_CTwCoreNotInit);
                return null;
            }
            // -- End Check CTweenCore -- //

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(CTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0.0f)
            {
                Debug.Log(CTweenStrings.Log_CTwDurationZero);
                Setter?.Invoke(TargetValue);
                return null;
            }
            // -- End Check Method Params -- //

            // -- Make Context -- //
            var Context = new CTweenCTX<Vector3>
                (StartValue, TargetValue, TargetObject, Duration, Setter,
                // Note that the below method is special, it returns the target coroutine with new contexts.
                (CTweenCTX<Vector3> ctx) => { return Current.To(ctx); });
            if (StartTween)
            { Context.StartTween(); }
            CurrentRunningTweens.Add(Context);

            // Return Context
            return Context;
        }
        /// <summary>
        /// Create a tween manually using this method.
        /// </summary>
        /// <param name="StartValue">The start value.</param>
        /// <param name="TargetValue">The end value.</param>
        /// <param name="Setter">The setter. Pass your function.</param>
        /// <param name="Duration">Length of tween.</param>
        /// <param name="TargetObject">Control the null conditions and setters.</param>
        /// <param name="StartTween">Should the tween start immediately after creation?</param>
        public static CTweenCTX<Quaternion> To(Quaternion StartValue, Quaternion TargetValue, float Duration, CTwSetMethod<Quaternion> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check CTweenCore --     //
#if UNITY_EDITOR
            // Editor checks
            if (Current == null && !Application.isPlaying && Application.isEditor)
            {
                Debug.LogWarning(CTweenStrings.Warn_EditorCTwCoreNotInit);
                CTweenCore.Editor_InitilazeCTw();
            }
#endif
            // Runtime checks
            if (Current == null)
            {
                Debug.LogError(CTweenStrings.Err_CTwCoreNotInit);
                return null;
            }
            // -- End Check CTweenCore -- //

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(CTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0.0f)
            {
                Debug.Log(CTweenStrings.Log_CTwDurationZero);
                Setter?.Invoke(TargetValue);
                return null;
            }
            // -- End Check Method Params -- //

            // -- Make Context -- //
            var Context = new CTweenCTX<Quaternion>
                (StartValue, TargetValue, TargetObject, Duration, Setter,
                // Note that the below method is special, it returns the target coroutine with new contexts.
                (CTweenCTX<Quaternion> ctx) => { return Current.To(ctx); });
            if (StartTween)
            { Context.StartTween(); }
            CurrentRunningTweens.Add(Context);

            // Return Context
            return Context;
        }
        /// <summary>
        /// Create a tween manually using this method.
        /// </summary>
        /// <param name="StartValue">The start value.</param>
        /// <param name="TargetValue">The end value.</param>
        /// <param name="Setter">The setter. Pass your function.</param>
        /// <param name="Duration">Length of tween.</param>
        /// <param name="TargetObject">Control the null conditions and setters.</param>
        /// <param name="StartTween">Should the tween start immediately after creation?</param>
        public static CTweenCTX<Matrix4x4> To(Matrix4x4 StartValue, Matrix4x4 TargetValue, float Duration, CTwSetMethod<Matrix4x4> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check CTweenCore --     //
#if UNITY_EDITOR
            // Editor checks
            if (Current == null && !Application.isPlaying && Application.isEditor)
            {
                Debug.LogWarning(CTweenStrings.Warn_EditorCTwCoreNotInit);
                CTweenCore.Editor_InitilazeCTw();
            }
#endif
            // Runtime checks
            if (Current == null)
            {
                Debug.LogError(CTweenStrings.Err_CTwCoreNotInit);
                return null;
            }
            // -- End Check CTweenCore -- //

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(CTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0.0f)
            {
                Debug.Log(CTweenStrings.Log_CTwDurationZero);
                Setter?.Invoke(TargetValue);
                return null;
            }
            // -- End Check Method Params -- //

            // -- Make Context -- //
            var Context = new CTweenCTX<Matrix4x4>
                (StartValue, TargetValue, TargetObject, Duration, Setter,
                // Note that the below method is special, it returns the target coroutine with new contexts.
                (CTweenCTX<Matrix4x4> ctx) => { return Current.To(ctx); });
            if (StartTween)
            { Context.StartTween(); }
            CurrentRunningTweens.Add(Context);

            // Return Context
            return Context;
        }
        #endregion

        #region Reflection To
        /// <summary>
        /// Create a tween manually. Note that you have to pass a tweenable type.
        /// (The ones that exist in <see cref="CTween"/> class.)
        /// </summary>
        /// <param name="StartValue">The start value.</param>
        /// <param name="TargetValue">The end value.</param>
        /// <param name="Setter">The setter. Pass your function.</param>
        /// <param name="Duration">Length of tween.</param>
        /// <param name="TargetObject">Control the null conditions and setters.</param>
        /// <param name="StartTween">Should the tween start immediately after creation?</param>
        public static CTweenCTX<T> GenericTo<T>(T StartValue, T TargetValue, float Duration, CTwSetMethod<T> Setter,
            bool StartTween = true, UnityEngine.Object TargetObject = null)
        {
            // Call helper method
            var ValCheckPair = IsTweenableType(typeof(T));

            // Check Tweenable
            if (ValCheckPair.Key)
            {
                /// We get method <see cref="CTween.To"/> returned from this class.
                return (CTweenCTX<T>)ValCheckPair.Value.Invoke
                    (null, new object[] { StartValue, TargetValue, Duration, Setter, TargetObject, StartTween });
            }
            else
            {
                Debug.LogError(CTweenStrings.GetErr_NonTweenableType(typeof(T).ToString()));
                return null;
            }
        }
        #endregion

        #endregion

        // TODO : Put this to a seperate class in a seperate file.
        // Maybe call the file 'CTweenExtensions'?
        #region Shortcuts for Unity Objects

        #region TextMeshPro
        /// <see cref="TextMeshProUGUI"/>
        public static CTweenCTX<float> CTwFadeAlpha(this TextMeshProUGUI target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.alpha, LastValue, Duration, (float f) => { target.alpha = f; }, target);

            return Context;
        }
        public static CTweenCTX<Color> CTwChangeColor(this TextMeshProUGUI target, Color LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.color, LastValue, Duration, (Color c) => { target.color = c; }, target);

            return Context;
        }
        #endregion

        #region UnityEngine.UI
        /// <see cref="CanvasGroup"/>
        public static CTweenCTX<float> CTwFadeAlpha(this CanvasGroup target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.alpha, LastValue, Duration, (float f) => { target.alpha = f; }, target);

            return Context;
        }

        /// <see cref="Image"/>
        public static CTweenCTX<Color> CTwChangeColor(this Image target, Color LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.color, LastValue, Duration, (Color c) => { target.color = c; }, target);

            return Context;
        }
        public static CTweenCTX<float> CTwFadeAlpha(this Image target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.color.a, LastValue, Duration,
                (float f) => { target.color = new Color(target.color.r, target.color.g, target.color.b, f); }, target);

            return Context;
        }
        public static CTweenCTX<float> CTwInterpFill(this Image target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.fillAmount, LastValue, Duration, (float f) => { target.fillAmount = f; }, target);

            return Context;
        }

        /// <see cref="RectTransform"/>
        public static CTweenCTX<Vector3> CTwChangePos(this RectTransform target, Vector3 LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position, LastValue, Duration, (Vector3 v) => { target.position = v; }, target);

            return Context;
        }
        public static CTweenCTX<float> CTwChangePosX(this RectTransform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position.x, LastValue, Duration,
                (float f) => { target.position = new Vector3(f, target.position.y, target.position.z); }, target);

            return Context;
        }
        public static CTweenCTX<float> CTwChangePosY(this RectTransform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position.y, LastValue, Duration,
                (float f) => { target.position = new Vector3(target.position.x, f, target.position.z); }, target);

            return Context;
        }
        public static CTweenCTX<float> CTwChangePosZ(this RectTransform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position.z, LastValue, Duration,
                (float f) => { target.position = new Vector3(target.position.x, target.position.y, f); }, target);

            return Context;
        }
        public static CTweenCTX<Vector3> CTwChangeAnchoredPosition(this RectTransform target, Vector2 LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.anchoredPosition, LastValue, Duration,
                (Vector3 v) => { target.anchoredPosition = v; }, target);

            return Context;
        }
        public static CTweenCTX<float> CTwChangeAnchorPosX(this RectTransform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.anchoredPosition.x, LastValue, Duration,
                (float f) => { target.anchoredPosition = new Vector2(f, target.anchoredPosition.y); }, target);

            return Context;
        }
        public static CTweenCTX<float> CTwChangeAnchorPosY(this RectTransform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.anchoredPosition.y, LastValue, Duration,
                (float f) => { target.anchoredPosition = new Vector2(target.anchoredPosition.x, f); }, target);

            return Context;
        }
        #endregion

        #region Standard (UnityEngine)
        /// <see cref="Transform">
        public static CTweenCTX<Vector3> CTwChangePos(this Transform target, Vector3 LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position, LastValue, Duration,
                (Vector3 v) => { target.position = v; }, target);

            return Context;
        }
        public static CTweenCTX<float> CTwChangePosX(this Transform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position.x, LastValue, Duration,
                (float f) => { target.position = new Vector3(f, target.position.y, target.position.z); }, target);

            return Context;
        }
        public static CTweenCTX<float> CTwChangePosY(this Transform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position.y, LastValue, Duration,
                (float f) => { target.position = new Vector3(target.position.x, f, target.position.z); }, target);

            return Context;
        }
        public static CTweenCTX<float> CTwChangePosZ(this Transform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position.z, LastValue, Duration,
                (float f) => { target.position = new Vector3(target.position.x, target.position.y, f); }, target);

            return Context;
        }
        public static CTweenCTX<Quaternion> CTwRotate(this Transform target, Quaternion LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.rotation, LastValue, Duration,
                (Quaternion q) => { target.rotation = q; }, target);

            return Context;
        }
        public static CTweenCTX<Vector3> CTwRotateEuler(this Transform target, Vector3 LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.rotation.eulerAngles, LastValue, Duration,
                (Vector3 f) => { target.rotation = Quaternion.Euler(f); }, target);

            return Context;
        }
        public static CTweenCTX<float> CTwRotateAngleAxis(this Transform target, float LastValue, Vector3 Axis, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            Axis = Axis.normalized;
            var EulerStartValue = 0f;
            if (Axis.x != 0f)
            {
                EulerStartValue = target.eulerAngles.x;
            }
            if (Axis.y != 0f)
            {
                EulerStartValue = target.eulerAngles.y;
            }
            if (Axis.z != 0f)
            {
                EulerStartValue = target.eulerAngles.z;
            }

            var Context = To(EulerStartValue, LastValue, Duration,
                (float f) => { target.rotation = Quaternion.AngleAxis(f, Axis); }, target);

            return Context;
        }


        /// <see cref="Material">
        public static CTweenCTX<Color> CTwChangeColor(this Material target, Color LastValue, float Duration, string PropertyName = "_Color")
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.GetColor(PropertyName), LastValue, Duration,
                (Color c) => { target.SetColor(PropertyName, c); }, target);

            return Context;
        }
        public static CTweenCTX<float> CTwFadeAlpha(this Material target, float LastValue, float Duration, string PropertyName = "_Color")
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.GetColor(PropertyName).a, LastValue, Duration,
                (float f) =>
                {
                    var c = target.GetColor(PropertyName);
                    target.SetColor(PropertyName, new Color(c.r, c.g, c.b, f));
                }, target);

            return Context;
        }

        /// <see cref="SpriteRenderer"/>
        public static CTweenCTX<Color> CTwChangeColor(this SpriteRenderer target, Color LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.color, LastValue, Duration,
                (Color c) => { target.color = c; }, target);

            return Context;
        }
        public static CTweenCTX<float> CTwFadeAlpha(this SpriteRenderer target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.color.a, LastValue, Duration,
                (float f) =>
                {
                    var c = target.color;
                    target.color = new Color(c.r, c.g, c.b, f);
                }, target);

            return Context;
        }

        /// <see cref="Camera"/>
        public static CTweenCTX<float> CTwChangeFOV(this Camera target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }
            var Context = To(target.fieldOfView, LastValue, Duration,
                (float f) => { target.fieldOfView = f; }, target);

            return Context;
        }
        public static CTweenCTX<Matrix4x4> CTwChangeProjectionMatrix(this Camera target, Matrix4x4 LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }
            var Context = To(target.projectionMatrix, LastValue, Duration,
                (Matrix4x4 m) => { target.projectionMatrix = m; }, target);

            return Context;
        }
        public static CTweenCTX<float> CTwChangeOrthoSize(this Camera target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.orthographicSize, LastValue, Duration,
                (float f) => { target.orthographicSize = f; }, target);

            return Context;
        }
        public static CTweenCTX<Color> CTwChangeBGColor(this Camera target, Color LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.backgroundColor, LastValue, Duration,
                (Color c) => { target.backgroundColor = c; }, target);

            return Context;
        }
        public static CTweenCTX<float> CTwChangeNearClipPlane(this Camera target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.nearClipPlane, LastValue, Duration,
                (float f) => { target.nearClipPlane = f; }, target);

            return Context;
        }
        public static CTweenCTX<float> CTwChangeFarClipPlane(this Camera target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(CTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.farClipPlane, LastValue, Duration,
                (float f) => { target.farClipPlane = f; }, target);

            return Context;
        }
        #endregion

        #endregion
    }
    #endregion

    #region CTween Ease Classes
    /// <summary>
    /// Includes the hard coded ease types.
    /// To create custom ease types use the graph.
    /// </summary>
    public static class CTwEase
    {
        /// <summary>
        /// All ease methods in a hashmap.
        /// </summary>
        public static Dictionary<EaseType, CTwEaseSetMethod> EaseMethods = new Dictionary<EaseType, CTwEaseSetMethod>
        {
            { EaseType.Linear, Linear },
            { EaseType.QuadIn, QuadIn },
            { EaseType.QuadOut, QuadOut },
            { EaseType.QuadInOut, QuadInOut },
            { EaseType.CubicIn, CubicIn },
            { EaseType.CubicOut, CubicOut },
            { EaseType.CubicInOut, CubicInOut },
            { EaseType.QuartIn, QuartIn },
            { EaseType.QuartOut, QuartOut },
            { EaseType.QuartInOut, QuartInOut },
            { EaseType.QuintIn, QuintIn },
            { EaseType.QuintOut, QuintOut },
            { EaseType.QuintInOut, QuintInOut },
            { EaseType.BounceIn, BounceIn },
            { EaseType.BounceOut, BounceOut },
            { EaseType.BounceInOut, BounceInOut },
            { EaseType.ElasticIn, ElasticIn },
            { EaseType.ElasticOut, ElasticOut },
            { EaseType.ElasticInOut, ElasticInOut },
            { EaseType.CircularIn, CircularIn },
            { EaseType.CircularOut, CircularOut },
            { EaseType.CircularInOut, CircularInOut },
            { EaseType.SinusIn, SinusIn },
            { EaseType.SinusOut, SinusOut },
            { EaseType.SinusInOut, SinusInOut },
            { EaseType.ExponentialIn, ExponentialIn },
            { EaseType.ExponentialOut, ExponentialOut },
            { EaseType.ExponentialInOut, ExponentialInOut }
        };

        #region Ease Methods
        // Note : All ease methods change between -Infinity-Infinity.
        // Clamping is done by setting a bool.
        public static float Linear(float t, bool clamped = true)
        {
            var tVal = t;
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float QuadIn(float t, bool clamped = true)
        {
            var tVal = t * t;
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float QuadOut(float t, bool clamped = true)
        {
            var tVal = t * (2f - t);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float QuadInOut(float t, bool clamped = true)
        {
            var tVal = t < 0.5f ? 2f * t * t : -1f + ((4f - (2f * t)) * t);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float CubicIn(float t, bool clamped = true)
        {
            var tVal = t * t * t;
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float CubicOut(float t, bool clamped = true)
        {
            var tVal = ((t - 1f) * t * t) + 1f;
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float CubicInOut(float t, bool clamped = true)
        {
            var tVal = t < 0.5f ? 4f * t * t * t : ((t - 1f) * ((2f * t) - 2f) * ((2 * t) - 2)) + 1f;
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float QuartIn(float t, bool clamped = true)
        {
            var tVal = t * t * t * t;
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float QuartOut(float t, bool clamped = true)
        {
            var tVal = 1f - ((t - 1f) * t * t * t);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float QuartInOut(float t, bool clamped = true)
        {
            var tVal = t < 0.5f ? 8f * t * t * t * t : 1f - (8f * (t - 1f) * t * t * t);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float QuintIn(float t, bool clamped = true)
        {
            var tVal = t * t * t * t * t;
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float QuintOut(float t, bool clamped = true)
        {
            var tVal = 1f + ((t - 1f) * t * t * t * t);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float QuintInOut(float t, bool clamped = true)
        {
            var tVal = t < 0.5f ? 16f * t * t * t * t * t : 1f + (16f * (t - 1f) * t * t * t * t);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float BounceIn(float t, bool clamped = true)
        {
            var tVal = 1f - BounceOut(1f - t);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float BounceOut(float t, bool clamped = true)
        {
            var tVal = t < 0.363636374f ? 7.5625f * t * t : t < 0.727272749f ? (7.5625f * (t -= 0.545454562f) * t) + 0.75f : t < 0.909090936f ? (7.5625f * (t -= 0.8181818f) * t) + 0.9375f : (7.5625f * (t -= 21f / 22f) * t) + (63f / 64f);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float BounceInOut(float t, bool clamped = true)
        {
            var tVal = t < 0.5f ? BounceIn(t * 2f) * 0.5f : (BounceOut((t * 2f) - 1f) * 0.5f) + 0.5f;
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float ElasticIn(float t, bool clamped = true)
        {
            var tVal = -(Mathf.Pow(2, 10 * (t -= 1)) * Mathf.Sin((t - (0.3f / 4f)) * (2 * Mathf.PI) / 0.3f));
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float ElasticOut(float t, bool clamped = true)
        {
            var tVal = t == 1f ? 1f : 1f - ElasticIn(1f - t);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float ElasticInOut(float t, bool clamped = true)
        {
            var tVal = (t *= 2f) == 2f ? 1f : t < 1f ? -0.5f * (Mathf.Pow(2f, 10f * (t -= 1)) * Mathf.Sin((t - 0.1125f) * (2f * Mathf.PI) / 0.45f)) : (Mathf.Pow(2f, -10f * (t -= 1f)) * Mathf.Sin((t - 0.1125f) * (2f * Mathf.PI) / 0.45f) * 0.5f + 1f);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float CircularIn(float t, bool clamped = true)
        {
            var tVal = -(Mathf.Sqrt(1 - t * t) - 1);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float CircularOut(float t, bool clamped = true)
        {
            var tVal = Mathf.Sqrt(1f - (t -= 1f) * t);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float CircularInOut(float t, bool clamped = true)
        {
            var tVal = (t *= 2f) < 1f ? -1f / 2f * (Mathf.Sqrt(1f - (t * t)) - 1f) : 0.5f * (Mathf.Sqrt(1 - ((t -= 2) * t)) + 1);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float SinusIn(float t, bool clamped = true)
        {
            var tVal = -Mathf.Cos(t * (Mathf.PI * 0.5f)) + 1f;
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float SinusOut(float t, bool clamped = true)
        {
            var tVal = Mathf.Sin(t * (Mathf.PI * 0.5f));
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float SinusInOut(float t, bool clamped = true)
        {
            var tVal = -0.5f * (Mathf.Cos(Mathf.PI * t) - 1f);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float ExponentialIn(float t, bool clamped = true)
        {
            var tVal = Mathf.Pow(2f, 10f * (t - 1f));
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float ExponentialOut(float t, bool clamped = true)
        {
            var tVal = Mathf.Sin(t * (Mathf.PI * 0.5f));
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float ExponentialInOut(float t, bool clamped = true)
        {
            var tVal = -0.5f * (Mathf.Cos(Mathf.PI * t) - 1f);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        #endregion
    }
    #endregion

    #region CTween Delegates
    // TODO : Put this to a seperate files with an 'Events' namespace.
    // -- Standard c#
    /// <summary>
    /// A blank delegate.
    /// </summary>
    public delegate void CTwMethod();
    /// <summary>
    /// Delegate with generic. Used for setter.
    /// </summary>
    /// <typeparam name="T">Type. Mostly struct types.</typeparam>
    /// Note that i might constraint 'T' only to struct, but idk.
    /// <param name="value">Set value.</param>
    public delegate void CTwSetMethod<in T>(T value);
    /// <summary>
    /// Tween easing method,
    /// <br>Used in <see cref="CTwEase"/>.</br>
    /// </summary>
    /// <param name="time">Time value. Interpolate time linearly if possible.</param>
    /// <returns>Interpolation value (usually between 0-1)</returns>
    public delegate float CTwEaseSetMethod(float time, bool clamped = true);

    // -- Unity c#
    /// <summary>
    /// Unity event for <see cref="CTweenProperty{T}"/> and <see cref="CTweenCTX{T}"/>
    /// </summary>
    [Serializable]
    public sealed class TweenCoreUnityEvent : UnityEvent
    { }
    #endregion

    #region CTween Property Bases
    /// <summary>
    /// Carries the base variables for the <see cref="CTweenProperty{T}"/>.
    /// TODO : Add custom curve allow and ease type enum. See:<see cref="EaseType"/>
    /// </summary>
    [Serializable]
    public abstract class CTweenPropertyBase
    {
        [Header("Tween Property")]
        [SerializeField] protected float _Duration = 1f;
        [SerializeField] protected float _Delay = 0f;
        // TODO : Rename to '_AllowCustomCurveOvershoot'
        [SerializeField] protected bool _AllowCustomCurveExtrapolation = false;
        [SerializeField] protected AnimationCurve _TweenCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public TweenCoreUnityEvent OnEndAction;

        // ---- Refreshing Setters ---- //
        public float Duration
        {
            get => _Duration;
            set
            {
                _Duration = value;

                UpdateProperty();
            }
        }
        public float Delay
        {
            get => _Delay;
            set
            {
                _Delay = value;

                UpdateProperty();
            }
        }
        public AnimationCurve TweenCurve
        {
            get => _TweenCurve;
            set
            {
                if (value == null) return;

                _TweenCurve = value;

                UpdateProperty();
            }
        }
        public bool AllowCustomCurveExtrapolation
        {
            get => _AllowCustomCurveExtrapolation;
            set
            {
                _AllowCustomCurveExtrapolation = value;

                UpdateProperty();
            }
        }

        public abstract void UpdateProperty();
    }
    /// <summary>
    /// <c>Experimental</c>, the tween property.
    /// Used for creating convient context, editable by the inspector.
    /// </summary>
    [Serializable]
    public class CTweenProperty<T> : CTweenPropertyBase
    {
        // ----- Generic Variables that don't need editor ----- //
        public CTwSetMethod<T> SetAction
        {
            set
            {
                // Ignore null values
                if (value == null) return;

                m_Setter = value;
                UpdateProperty();
            }
        }

        // ---- Get Only ---- //
        public CTweenCTX<T> TwContext { get; private set; }
        public bool IsValidContext => IsTweenableType(typeof(T)).Key;

        // ---- Private ---- //
        private CTwSetMethod<T> m_Setter;
        public bool IsSetup => m_Setter != null;

        #region Ctor / Setup
        public CTweenProperty()
        { }
        public CTweenProperty(CTweenCTX<T> ctx, bool stopTw = true)
        {
            // ** Stop the context and assign the context.
            if (stopTw)
            {
                ctx.StopTween();
            }

            TwContext = ctx;

            // ** Gather values from context.
            _Duration = ctx.Duration;
            _Delay = ctx.StartDelay;
            _TweenCurve = ctx.CustomTimeCurve;
            m_Setter = ctx.SetterFunction;
            ctx.SetCustomCurve(_TweenCurve, !_AllowCustomCurveExtrapolation);
            ctx.SetEndingAction(OnEndAction);
        }
        public static implicit operator CTweenProperty<T>(CTweenCTX<T> ctxEqual)
        {
            // ** Create & Return property
            return new CTweenProperty<T>(ctxEqual);
        }
        public void SetupProperty(CTwSetMethod<T> Setter)
        {
            if (Setter == null)
            {
                Debug.LogError(CTweenStrings.Err_SetterFnNull);
                return;
            }

            m_Setter = Setter;
            TwContext = GenericTo(default, default, _Duration, Setter, false);
            TwContext.SetDelay(_Delay).SetCustomCurve(_TweenCurve, !_AllowCustomCurveExtrapolation);

            if (OnEndAction != null)
            {
                TwContext.SetEndingAction(OnEndAction);
            }
        }
        public void SetupProperty(T StartValue, T EndValue, CTwSetMethod<T> Setter)
        {
            if (Setter == null)
            {
                Debug.LogError(CTweenStrings.Err_SetterFnNull);
                return;
            }

            m_Setter = Setter;
            TwContext = GenericTo(StartValue, EndValue, _Duration, Setter, false);
            TwContext.SetDelay(_Delay).SetCustomCurve(_TweenCurve, !_AllowCustomCurveExtrapolation);

            if (OnEndAction != null)
            {
                TwContext.SetEndingAction(OnEndAction);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the property's variables after something is changed.
        /// </summary>
        public override void UpdateProperty()
        {
            if (TwContext == null) return;

            // -- No null checks
            TwContext.SetDelay(_Delay).SetDuration(_Duration).SetCustomCurve(_TweenCurve, !_AllowCustomCurveExtrapolation);
            // TwContext.AllowCustomCurveExtrapolation = _AllowCustomCurveExtrapolation;

            // -- Null checks
            if (OnEndAction != null)
            {
                TwContext.SetEndingAction(OnEndAction);
            }
            if (m_Setter != null)
            {
                TwContext.SetSetter(m_Setter);
            }
        }

        public void StartTween(T StartValue, T EndValue, CTwSetMethod<T> Setter = null)
        {
            if (m_Setter == null && Setter == null)
            {
                Debug.LogError("[CTweenContext::StartTween] Null action was passed. Doing nothing.");
                return;
            }

            if (TwContext == null) SetupProperty(StartValue, EndValue, Setter);

            if (TwContext.IsRunning)
                TwContext.StopTween();

            // Make sure to set these values
            TwContext.SetStartValue(StartValue).SetEndValue(EndValue);

            TwContext.StartTween();
        }

        public void StartTween()
        {
            // ** Parameterless 'StartTween()'
            // This takes the parameters from the tween Context.
            if (TwContext == null)
            {
                Debug.LogWarning(CTweenStrings.Warn_CTwPropertyTwNull);
                return;
            }

            if (TwContext.IsRunning)
                TwContext.StopTween();

            TwContext.StartTween();
        }

        public void StopTween()
        {
            if (TwContext == null)
            {
                Debug.Log(CTweenStrings.Warn_CTwPropertyTwNull);
                return;
            }

            TwContext.StopTween();
        }
        #endregion
    }

    #region CTween Property Classes
    [Serializable]
    public sealed class CTweenPropertyFloat : CTweenProperty<float>
    {
        public CTweenPropertyFloat(float dur, float delay = 0f, bool exPol = false, AnimationCurve c = null)
        {
            _Duration = dur;
            _Delay = delay;
            _AllowCustomCurveExtrapolation = exPol;
            if (c != null)
            {
                _TweenCurve = c;
            }
        }
    }
    [Serializable]
    public sealed class CTweenPropertyVector2 : CTweenProperty<Vector2>
    {
        public CTweenPropertyVector2(float dur, float delay = 0f, bool exPol = false, AnimationCurve c = null)
        {
            _Duration = dur;
            _Delay = delay;
            _AllowCustomCurveExtrapolation = exPol;
            if (c != null)
            {
                _TweenCurve = c;
            }
        }
    }
    [Serializable]
    public sealed class CTweenPropertyVector3 : CTweenProperty<Vector3>
    {
        public CTweenPropertyVector3(float dur, float delay = 0f, bool exPol = false, AnimationCurve c = null)
        {
            _Duration = dur;
            _Delay = delay;
            _AllowCustomCurveExtrapolation = exPol;
            if (c != null)
            {
                _TweenCurve = c;
            }
        }
    }
    [Serializable]
    public sealed class CTweenPropertyColor : CTweenProperty<Color>
    {
        public CTweenPropertyColor(float dur, float delay = 0f, bool exPol = false, AnimationCurve c = null)
        {
            _Duration = dur;
            _Delay = delay;
            _AllowCustomCurveExtrapolation = exPol;
            if (c != null)
            {
                _TweenCurve = c;
            }
        }
    }
    #endregion

    #endregion

    #region CTw Enums
    /// <summary>
    /// Variable changing mode for <see cref="CTweenCTX{T}"/>.
    /// </summary>
    public enum VariableChangeMode
    {
        ChangeMode_Add = 0,
        ChangeMode_Equals = 1,
        ChangeMode_Subtract = 2
    }
    /// <summary>
    /// Ease type.
    /// </summary>
    /// See this website explaining ease types : https://easings.net/
    public enum EaseType
    {
        Linear,
        QuadIn,
        QuadOut,
        QuadInOut,
        CubicIn,
        CubicOut,
        CubicInOut,
        QuartIn,
        QuartOut,
        QuartInOut,
        QuintIn,
        QuintOut,
        QuintInOut,
        BounceIn,
        BounceOut,
        BounceInOut,
        ElasticIn,
        ElasticOut,
        ElasticInOut,
        CircularIn,
        CircularOut,
        CircularInOut,
        SinusIn,
        SinusOut,
        SinusInOut,
        ExponentialIn,
        ExponentialOut,
        ExponentialInOut
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
    #endregion

    #region CTween Context
    /// <summary>Generic tween interface. Used for storing tweens.</summary>
    public interface ITweenCTX
    {
        void StopTween();

        void StartTween();

        UnityEngine.Object TargetObj { get; }
    }

    public sealed class CTweenCTX<T> : ITweenCTX
    {
        #region Variables
        // Should be read-only and only be able to set from methods. 
        // Most of the info is contained in the here.

        // Public fields //
        // Values
        // -- Standard Values
        public T StartValue { get; private set; }
        public T EndValue { get; private set; }

        // --- Settings
        public float Duration { get; private set; } = 0.0f;
        public float StartDelay { get; private set; } = 0.0f;
        public int RepeatAmount { get; private set; } = 0;
        public RepeatType RepeatType { get; private set; } = RepeatType.PingPong;
        public bool InvokeEventOnRepeat { get; private set; } = true;

        // --- Status
        public bool IsRunning { get; private set; } = false;

        /// <summary>
        /// Helper variable for <see cref="IsValuesSwitched"/>.
        /// </summary>
        private bool _IsValuesSwitched = false;
        /// <summary>
        /// Get whether the values are switched under repeat type <see cref="RepeatType.PingPong"/>.
        /// <br>Usually returns false if the repeat type is <see cref="RepeatType.Reset"/> 
        ///     or if repeat amount is zero.</br>
        /// </summary>
        public bool IsValuesSwitched => _IsValuesSwitched || !(RepeatType == RepeatType.Reset || RepeatAmount == 0);

        public bool ContextIsValid => StartValue != null && EndValue != null &&
                    SetterFunction != null && IteratorCoroutine != null && (TargetObject_IsOptional || TargetObj != null);
        // -- Pausing
        public T PauseValue { get; private set; }
        public float CoroutineElapsed;

        // --- Interpolation
        public EaseType TweenEaseType { get; private set; } = EaseType.QuadOut;
        public AnimationCurve CustomTimeCurve { get; private set; } = null;
        public bool UseCustomTwTimeCurve => CustomTimeCurve != null;
        public bool UseUnclampedLerp { get; private set; } = false;
        // -- Setter (subpart of Interpolation)
        public Func<float, float> TimeSetLerp { get; private set; }
        public CTwSetMethod<T> SetterFunction { get; private set; }
        public CTwMethod OnEndAction { get; private set; }
        public CTwMethod PersistentOnEndAction { get; private set; }
        public TweenCoreUnityEvent OnEndAction_UnityEvent { get; private set; }

        // --- Target (Identifier and Null checks)
        private readonly UnityEngine.Object _TargetObj;
        public UnityEngine.Object TargetObj => _TargetObj;
        public bool TargetObject_IsOptional => _TargetObj == null;
        public IEnumerator IteratorCoroutine => _IteratorCoroutine;

        // --- Private Fields
        // Coroutine / Iterator 
        private Func<CTweenCTX<T>, IEnumerator> _TweenIteratorFn;   // Delegate to get coroutine suitable for this class
        private IEnumerator _IteratorCoroutine;                     // Current setup iterator (not running)
        private IEnumerator _CurrentIteratorCoroutine;              // Current running iterator
        #endregion

        #region Set Settable Variables
        public CTweenCTX<T> ClearEndingAction()
        {
            OnEndAction = null;

            return this;
        }
        /// <summary>
        /// Sets an event to be occured in end.
        /// </summary>
        public CTweenCTX<T> SetEndingAction(CTwMethod Event, VariableChangeMode mode = VariableChangeMode.ChangeMode_Add)
        {
            switch (mode)
            {
                case VariableChangeMode.ChangeMode_Add:
                    OnEndAction += Event;
                    break;
                case VariableChangeMode.ChangeMode_Equals:
                    OnEndAction = Event;
                    break;
                case VariableChangeMode.ChangeMode_Subtract:
                    OnEndAction -= Event;
                    break;
            }

            UpdateContextCoroutine();
            return this;
        }
        /// <summary>
        /// Sets an event to be occured in end.
        /// </summary>
        public CTweenCTX<T> SetEndingAction(TweenCoreUnityEvent Event)
        {
            OnEndAction_UnityEvent = Event;

            UpdateContextCoroutine();
            return this;
        }
        /// <summary>
        /// Sets the duration.
        /// </summary>
        /// <param name="dur">Duration to set.</param>
        public CTweenCTX<T> SetDuration(float dur)
        {
            Duration = dur;

            UpdateContextCoroutine();
            return this;
        }
        /// <summary>
        /// Sets starting delay. Has no effect if the tween has already started. 
        /// (Info : Can be applied after construction of the tween as we wait for end of frame.)
        /// Anything below zero(including zero) is a special value for no delay.
        /// </summary>
        /// <param name="delay">The delay for tween to wait.</param>
        public CTweenCTX<T> SetDelay(float delay)
        {
            StartDelay = delay;

            UpdateContextCoroutine();
            return this;
        }
        /// <summary>
        /// Sets starting delay. Has no effect if the tween has already started.
        /// Anything below zero(including zero) is a special value for no delay.
        /// Randomizes the delay between 2 random float values.
        /// </summary>
        /// <param name="delay">The delay for tween to wait.</param>
        public CTweenCTX<T> SetRandDelay(float min_delay, float max_delay)
        {
            StartDelay = UnityEngine.Random.Range(min_delay, max_delay);

            UpdateContextCoroutine();
            return this;
        }
        /// <summary>
        /// Sets repeat amount. '0' is default for no repeat. Anything lower than '0' is infinite repeat.
        /// </summary>
        public CTweenCTX<T> SetRepeatAmount(int repeat)
        {
            RepeatAmount = repeat;

            UpdateContextCoroutine();
            return this;
        }
        /// <summary>
        /// Sets repeat type.
        /// </summary>
        public CTweenCTX<T> SetRepeatType(RepeatType type)
        {
            RepeatType = type;

            UpdateContextCoroutine();
            return this;
        }
        public CTweenCTX<T> SetInvokeEventOnRepeat(bool value)
        {
            InvokeEventOnRepeat = value;

            UpdateContextCoroutine();
            return this;
        }
        public CTweenCTX<T> SetEase(EaseType ease, bool Clamp01 = true)
        {
            if (UseCustomTwTimeCurve)
            {
                Debug.Log(CTweenStrings.GetLog_CTwCTXCustomCurveInvalid(ease));
                return this;
            }

            // Note : This is only set for getting read-only data.
            TweenEaseType = ease;

            // Setup curve
            var EaseMethod = CTwEase.EaseMethods[TweenEaseType];
            TimeSetLerp = (float progress) => { return EaseMethod.Invoke(progress, Clamp01); };

            // Update
            UpdateContextCoroutine();
            return this;
        }
        /// <summary> Sets a custom animation curve. Pass null to disable custom curve. </summary>
        /// <param name="c">Curve to set.</param>
        /// <param name="Clamp01">Should the curve be clamped?</param>
        public CTweenCTX<T> SetCustomCurve(AnimationCurve c, bool Clamp01 = true)
        {
            // Check curve status
            if (c == null)
            {
                // The curve is already null, warn the user.
                if (!UseCustomTwTimeCurve)
                {
                    Debug.LogWarning(CTweenStrings.Warn_CTwCTXTimeCurveNull);
                    return this;
                }

                // User wants to disable CustomCurve.
                CustomTimeCurve = null;
                // Reset curve delegate
                SetEase(TweenEaseType);
                return this;
            }

            // Set variable for context.
            CustomTimeCurve = c;

            // Clamp value between 0-1
            if (Clamp01)
            { TimeSetLerp = (float progress) => { return Mathf.Clamp01(CustomTimeCurve.Evaluate(Mathf.Clamp01(progress))); }; }
            else
            { TimeSetLerp = (float progress) => { return CustomTimeCurve.Evaluate(Mathf.Clamp01(progress)); }; }

            UpdateContextCoroutine();
            return this;
        }
        /// <summary>
        /// Sets a custom setter. 
        /// Ignores null values completely as a setter is a critical part of the Core.
        /// </summary>
        /// <param name="setter">Setter to set.</param>
        public CTweenCTX<T> SetSetter(CTwSetMethod<T> setter)
        {
            // -- Check Setter
            if (setter == null)
            {
                Debug.LogError(CTweenStrings.Err_CTwCTXSetterNull);
                return this;
            }
            try
            { setter.Invoke(StartValue); }
            catch (Exception e)
            {
                Debug.LogError(CTweenStrings.GetErr_CTwCTXSetterExcept(e));
                return this;
            }

            // -- Set Setter
            UpdateContextCoroutine();
            SetterFunction = setter;
            return this;
        }
        public CTweenCTX<T> SetStartValue(T sValue)
        {
            StartValue = sValue;

            // Bad way of checking whether the values are about to be switched.
            if (sValue.Equals(EndValue))
            {
                _IsValuesSwitched = !_IsValuesSwitched;
            }

            UpdateContextCoroutine();
            return this;
        }
        public CTweenCTX<T> SetEndValue(T eValue)
        {
            EndValue = eValue;

            // Bad way of checking whether the values are about to be switched.
            if (eValue.Equals(EndValue))
            {
                _IsValuesSwitched = !_IsValuesSwitched;
            }

            UpdateContextCoroutine();
            return this;
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
            _IteratorCoroutine = _TweenIteratorFn.Invoke(this);

            return _IteratorCoroutine != null;
            // -- Reflection Method --
            // _IteratorCoroutine = (IEnumerator)typeof(CTweenCore).GetMethods().
            //    Single(x => x.Name == nameof(CTweenCore.To) && x.GetParameters()[0].ParameterType == GetType()).
            //    Invoke(Current, new object[] { this });
        }
        #endregion

        /// <summary>
        /// The main constructor.
        /// </summary>
        public CTweenCTX(T StartVal, T EndVal, UnityEngine.Object TargetO, float TDuration,
            // Func Variable
            CTwSetMethod<T> SetFunc, Func<CTweenCTX<T>, IEnumerator> GetTweenIterFn,
            CTwMethod PersistentEndMthd = null)
        {
            try
            {
                // Public
                StartValue = StartVal;
                EndValue = EndVal;
                SetterFunction = SetFunc; // Setup setter function.
                SetterFunction += (T valuePause) => { PauseValue = valuePause; }; // Add pausing
                PersistentOnEndAction = PersistentEndMthd;

                Duration = TDuration;

                // Private
                _TargetObj = TargetO;
                _TweenIteratorFn = GetTweenIterFn;

                // Functions to update (like ease)
                SetEase(TweenEaseType);
            }
            catch (Exception e)
            {
                // Error handling
                Debug.LogError(CTweenStrings.GetErr_CTwCTXCtorExcept(e));
            }
        }

        #region Start-Stop (Part of the Generic Interface)
        public void StartTween()
        {
            // Checks
            if (IteratorCoroutine == null)
            {
                // Try updating context.
                if (!UpdateContextCoroutine())
                {
                    Debug.LogError(CTweenStrings.Err_CTwCTXFailUpdate);
                    return;
                }
            }
#if UNITY_EDITOR
            // Unity Editor Check
            if (!Application.isPlaying && Application.isEditor)
            {
                EditModeCoroutineExec.StartCoroutine(IteratorCoroutine);
                return;
            }
#endif

            if (IsRunning)
            {
                StopTween();
            }

            // Standard
            if (!UpdateContextCoroutine())
            {
                Debug.LogError("<b><color=#8634eb>[CTweenCTX(Critical)]-></color></b>Tried to update context and failed miserably.");
            }
            _CurrentIteratorCoroutine = IteratorCoroutine;
            Current.StartCoroutine(_CurrentIteratorCoroutine);
            IsRunning = true;
        }

        public void PauseTween()
        {
            Debug.LogError("Pausing is TODO. Try stopping. [PauseTween]");
        }

        public void StopTween()
        {
            // Checks
            if (IteratorCoroutine == null)
            {
                Debug.LogError(CTweenStrings.Err_CTwCTXNoIterator);
                return;
            }

            if (IsValuesSwitched)
            {
                var st_value = StartValue;
                var end_value = EndValue;

                StartValue = end_value;
                EndValue = st_value;

                // No longer switched.
                _IsValuesSwitched = false;
            }

#if UNITY_EDITOR
            // Unity Editor Stop
            if (!Application.isPlaying && Application.isEditor)
            {
                EditModeCoroutineExec.StopCoroutine(IteratorCoroutine);
                return;
            }
#endif

            // Standard
            CurrentRunningTweens.Remove(this);
            if (_CurrentIteratorCoroutine != null)
            {
                Current.StopCoroutine(_CurrentIteratorCoroutine);
                _CurrentIteratorCoroutine = null;
            }
            //Showing warnings like these should be a verbose option
            //else
            //{
            //    // _CurrentIteratorCoroutine is null.
            //    Debug.LogWarning(CTweenStrings.Warn_CTwCurrentIteratorNull);
            //}

            IsRunning = false;
        }
        #endregion

        #region Debug
        public override string ToString()
        {
            return string.Format("CTWContext | Type : {0}", typeof(T).ToString());
        }
        /// <summary>
        /// Get the message for 'why context wasnt valid?'.
        /// <br>Returns a unknown message for no reason or unknown reason.</br>
        /// </summary>
        /// <returns>Returns the message.</returns>
        /// Note that this is a bad way of getting the debug context message.
        public string Debug_GetContextValidMsg()
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

            // -------------------------------------- These might happen due to user error or dev error.
            if (SetterFunction == null)
            {
                return "(Context)The given setter function is null. If you called 'CTween.To()' manually please make sure you set a setter function, otherwise it's a developer error.";
            }
            if (IteratorCoroutine == null)
            {
                return "(Context)The coroutine given is null. Probably an internal issue.";
            }
            if (TargetObj == null)
            {
                return "(Context)The target object is null. Make sure you do not destroy it.";
            }

            return "(Context)Unknown or no reason.";
        }
        #endregion
    }
    #endregion
}