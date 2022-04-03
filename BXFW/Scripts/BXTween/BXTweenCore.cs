/// BXTween :
///     It does tweens.
/// 
/// How do use? :
///     <example>
///     // basically dotween but crappier
///     objectOrPropertyThatHasTypeThatSupportsBXTweenExtensions.BXTw{ExtensionContext}({EndValue}, {Duration});
///     // or you can start a manual one
///     BXTween.To((supportedType t) => { /* Setter function goes here, example one looks like : */ variableThatNeedTween.somethingThatIsFloat = t; }, {StartValue}, {EndValue}, {Duration});
///     
///     // even better, you can declare a BXTweenProperty{Type goes here, don't use the generic version}
///     // This example code interpolates the alpha of a canvas group.
///     ... declared in the monobehaviour, serializable variable scope 'public or private with serialize field', 
///     BXTweenPropertyFloat interpolateThing = new BXTweenPropertyFloat({DurationDefault}, {DelayDefault}, {Curve/Ease Overshoot allow}, {CurveDefault})
///     ... called inside a method, you should already know this
///     // Always setup property before calling StartTween!!
///     interpolateThing.SetupProperty((float f) => { canvasGroupThatIsDeclared.alpha = f; }); // you can also declare start-end values like ({StartValue}, {EndValue}, {Setter})
///     interpolateThing.StartTween(canvasGroupThatIsDeclared.alpha, 1f);
///     // congrats now you know how to use bxtween enjoy very performance
///     </example>
/// 
/// What else?  :
///     1 : Has many features (bugs, but i call them features)
///     2 : Runs with very high performance (about 1 tween per 3 hours!! [that is equal to 1,54320987654321e-6 tweens per second] wow such fast)
///     3 : Very coding pattern (what)
/// 
/// But why did you spend effort on this? there are better alternatives :
///     I don't know, but yeah
///
/// How long have you been writing this? :
///     For 2 years (actually i probably spent like 2 weeks on initial writing, which is the most of the functionality, then it's just bug fixes and some duct tape)

/** -------------------------------------------------- 
// GENERAL TODO :
// 1 : <see cref="BXFW.Tweening.BXTweenCTX{T}"/>'s <see cref="BXFW.Tweening.RepeatType.PingPong"/> doesn't work as ping-pong,
//     but rather as <see cref="BXFW.Tweening.RepeatType.Reset"/>. Reset seems to work fine.
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
using System.Text;

#if UNITY_EDITOR
using BXFW.Tweening.Editor;
#endif
using static BXFW.Tweening.BXTween;

namespace BXFW.Tweening
{
    /// Solution for stylized print strings.
    /// <summary>
    /// Constant strings for <see cref="BXTween"/> messages.
    /// <br>Doesn't apply styling on builds of the game.</br>
    /// </summary>
    public static class BXTweenStrings
    {
        // -- Formatters
        #region (Private) Rich Text Formatting
        private static string FmtRichText(string RichFmtTarget, bool Bold, Color FmtColor)
        {
#if UNITY_EDITOR
            // Format colors
            string logColorTag = string.Format("<color=#{0}>", ColorUtility.ToHtmlStringRGB(FmtColor).ToLowerInvariant());
            // Split and cover the \n's with the color tag for proper coloring.
            StringBuilder fmtTarget = new StringBuilder(RichFmtTarget.Length);
            foreach (var c in RichFmtTarget)
            {
                if (c == '\n')
                {
                    // last </color> is added on the last line anyways
                    // This append looks like = '</color>\n<color=#{0}>'
                    fmtTarget.Append(string.Format("</color>\n{0}", logColorTag));
                    Debug.Log(fmtTarget.ToString());
                    continue;
                }

                fmtTarget.Append(c);
            }

            return string.Format(Bold ? "<b>{1}{0}</color></b>" : "{1}{0}</color>", fmtTarget.ToString(), logColorTag);
#else
            return RichFormatTarget;
#endif
        }
        internal static string LogRich(string s, bool Bold = false)
        {
            return FmtRichText(s, Bold, CurrentSettings.LogColor);
        }
        internal static string LogDiagRich(string s, bool Bold = false)
        {
            return FmtRichText(s, Bold, CurrentSettings.LogDiagColor);
        }
        internal static string WarnRich(string s, bool Bold = false)
        {
            return FmtRichText(s, Bold, CurrentSettings.WarnColor);
        }
        internal static string ErrRich(string s, bool Bold = false)
        {
            return FmtRichText(s, Bold, CurrentSettings.ErrColor);
        }
        #endregion

#if UNITY_EDITOR
        // -- Debug Tools
        /// <summary>
        /// <c>EDITOR ONLY</c> | List all registered fields in <see cref="BXTweenStrings"/>.
        /// <br>Outputs log to Debug.Log.</br>
        /// </summary>
        public static void ListStrings()
        {
            Debug.Log("<b>[ListStrings] Listing all fields in 'BXTweenStrings'.</b>");

            foreach (FieldInfo field in typeof(BXTweenStrings).GetFields())
            {
                // Pass null as it's static.
                Debug.Log(string.Format("Field | Name : {0} | ToString : {1}", field.Name, field.GetValue(null)));
            }

            Debug.Log("<b>[ListStrings] Note that dynamic strings are not outputtable.</b>");
        }
#endif
        // -- Strings
        #region Data
        public const string SettingsResourceCreatePath = "";
        public const string SettingsResourceCreateName = "BXTweenSettings.asset";
        #endregion

        #region Info-Logs
        // Non-Dynamic
        /// <see cref="BXTween.To"/>
        public static readonly string Log_BXTwDurationZero =
            string.Format("{0} {1}",
                LogRich("[BXTween]->", true),
                LogRich("The duration is less than zero. Please note that the return is null."));
        // Dynamic
        public static string GetLog_BXTwCTXCustomCurveInvalid(EaseType TypeAttemptSet)
        {
            return string.Format
                (   // Main String
                    "{0} {1}",
                    // Format List
                    LogRich("[BXTweenCTX::SetEase]->", true),
                    LogRich(string.Format("Cannot set ease to predefined {0} mode because there is a Custom Ease Curve set.\nPlease set custom curve to null (Use ::SetCustomCurve()) if you want to set predefined eases.", TypeAttemptSet.ToString()))
                );
        }
        public static string GetLog_BXTwSettingsOnCreate(string createDir)
        {
            // Do not format this string if you don't want free stackoverflows
            return string.Format
                (   // Main String
                    "{0} {1}",
                    // Format List
                    "[BXTweenSettings::GetBXTweenSettings]->",
                    string.Format("Created BXTweenSettings on directory: '{0}'", createDir)
                );
        }

        #region Verbose tween information logs
        // Basically BXTweenSettings.diagnosticMode
        // IDK : Maybe this should be editor only?
        internal static string Log_BXTwCTXGetDebugFormatInfo<T>(BXTweenCTX<T> gContext)
        {
            return string.Format(@"Tween Info => '{0}' with target '{1}'
Tween Details : Duration={2} StartVal={3} EndVal={4} HasEndActions={5} InvokeActionsOnManualStop={6}.",
                    gContext.ToString(), gContext.TargetObj, gContext.Duration, gContext.StartValue, gContext.EndValue, gContext.OnEndAction == null, gContext.InvokeEventOnStop);
        }
        public static string GetDLog_BXTwCTXOnCtor<T>(BXTweenCTX<T> gContext)
        {
            return string.Format
                (   // Main String
                    "{0} {1}",
                    // Format List
                    LogDiagRich("[BXTweenCTX::.ctor()]->", true),
                    LogRich(string.Format("Constructed tween : \"{0}\".", Log_BXTwCTXGetDebugFormatInfo(gContext)))
                );
        }
        public static string GetDLog_BXTwCTXOnStart<T>(BXTweenCTX<T> gContext)
        {
            return string.Format
                (   // Main String
                    "{0} {1}",
                    // Format List
                    LogDiagRich("[BXTweenCTX::StartTween]->", true),
                    LogRich(string.Format("Called 'StartTween()' on context \"{0}\".", Log_BXTwCTXGetDebugFormatInfo(gContext)))
                );
        }
        public static string GetDLog_BXTwCTXOnStop<T>(BXTweenCTX<T> gContext)
        {
            return string.Format
                (   // Main String
                    "{0} {1}",
                    // Format List
                    LogDiagRich("[BXTweenCTX::StopTween]->", true),
                    LogRich(string.Format("Called 'StopTween()' on context \"{0}\".", Log_BXTwCTXGetDebugFormatInfo(gContext)))
                );
        }
        public static string GetDLog_BXTwCTXOnUpdateContextCoroutine<T>(BXTweenCTX<T> gContext)
        {
            return string.Format
                (   // Main String
                    "{0} {1}",
                    // Format List
                    LogDiagRich("[BXTweenCTX::UpdateContextCoroutine]->", true),
                    LogRich(string.Format("Called 'UpdateContextCoroutine()' on context \"{0}\".", Log_BXTwCTXGetDebugFormatInfo(gContext)))
                );
        }
        public static readonly string DLog_BXTwCTXTimeCurveAlreadyNull =
            string.Format("{0} {1}",
                LogDiagRich("[BXTweenCTX::SetCustomCurve]", true),
                LogRich("The tween time curve (related stuff) is already null. You are setting it null again"));
        #endregion

        #endregion

        #region Warnings
        // Non-Dynamic
        // All
        /// <see cref="BXTweenCore"/>
        public static readonly string Warn_BXTwCoreAlreadyExist =
            string.Format("{0} {1}",
                WarnRich("[BXTweenCore::Editor_InitilazeBXTw]->", true),
                LogRich("The BXTween core is already init and it already contains a editor object."));
        public static readonly string Warn_BXTwPropertyTwNull =
            string.Format("{0} {1}",
                WarnRich("[BXTweenContext]", true),
                LogRich("The tween property is null. Make sure you assign all properties in your inspector. If you did that, it's probably an internal error."));
        public static readonly string Warn_BXTwCTXTimeCurveNull =
            string.Format("{0} {1}",
                WarnRich("[BXTweenCTX::SetCustomCurve]", true),
                LogRich("The tween time curve (related stuff) is null. Make sure you assign all properties in your inspector. If you did that, it's probably an internal error."));
        public static readonly string Warn_BXTwCurrentIteratorNull =
            string.Format("{0} {1}",
                WarnRich("[BXTween::StopTween]->", true),
                LogRich("The current running coroutine is null. Probably an internal error, that is not very important."));
        public static readonly string Warn_BXTwCoreNotInit =
            string.Format("{0} {1}",
                WarnRich("[BXTweenCore]->", true),
                LogRich("The 'Current' reference is null. Re-initilazing. This can happen after script compiles and such."));
#if UNITY_EDITOR // Editor Only
        /// <see cref="BXTween.To"/> on editor.
        public static readonly string Warn_EditorBXTwCoreNotInit =
            string.Format("{0} {1}",
                WarnRich("[BXTween::To(OnEditor)]->", true),
                LogRich("Please make sure you initilaze BXTween for editor playback. (by calling Editor_InitilazeBXTw())"));
#endif
        // Dynamic

        #endregion

        #region Errors
        // Non-Dynamic
        public static readonly string Err_BXTwCoreFailInit =
            string.Format("{0} {1}",
                ErrRich("[BXTweenCore(Critical)]->", true),
                LogRich("The 'Current' reference is null and failed to initilaze. Make sure the Core initilazes properly."));
        /// <summary>
        /// General failure message for any of the <see cref="BXTween.To"/> methods failing.
        /// </summary>
        public static readonly string Err_BXTwToCtxFail =
            string.Format("{0} {1}",
                ErrRich("[BXTween::To]->", true),
                LogRich("Failed to return a valid context!"));
        public static readonly string Err_BXTwCTXFailUpdate =
            string.Format("{0} {1}",
                ErrRich("[BXTweenCTX(Critical)]->", true),
                LogRich("The 'IteratorCoroutine' given variable is null even after update."));
        public static readonly string Err_BXTwSettingsNoResource =
            string.Format("{0} {1}",
                "[BXTweenSettings::GetBXTweenSettings]->",
                "No resource was generated in editor. Returning default 'ScriptableObject'.");

        public static readonly string Err_SetterFnNull =
            string.Format("{0} {1}",
                ErrRich("[BXTween(General Error)]->", true),
                LogRich("The given setter function is null or broken. This can happen in these classes : 'BXTweenCTX<T>', 'BXTween(To Methods)' or 'BXTweenProperty<T>'."));
        public static readonly string Err_TargetNull =
            string.Format("{0} {1}",
                ErrRich("[BXTween::To(Error, Extension Method)]->", true),
                LogRich("The given target is null. Returning null context, expect exceptions."));
        public static readonly string Err_BXTwCTXFailUpdateCoroutine =
            string.Format("{0} {1}",
                ErrRich("[BXTweenCTX::UpdateContextCoroutine]->", true),
                LogRich("Failed to update the coroutine context."));

        public static readonly string Err_BXTwCTXSetterNull =
            string.Format("{0} {1}",
                ErrRich("[BXTweenCTX::SetSetter]->"),
                LogRich("Couldn't set action as the variable passed was null."));
        public static readonly string Err_BXTwCTXNoIterator =
            string.Format("{0} {1}",
                ErrRich("[BXTweenCTX(General Error)]->"),
                LogRich("The IteratorCoroutine variable is null."));
        public static readonly string Err_BXTweenSettingsModified =
           string.Format("{0} {1}",
                ErrRich("[BXTweenSettings::IsModified]->"),
                LogRich("BXTweenSettings was modified. Doing what i was told in BXTweenPersistentSettings."));

        // Dynamic (Needs to be generated dynamically or smth)
        /// <see cref="BXTween.To"/> methods.
        public static string GetErr_NonTweenableType(string ReasonNotTwn)
        {
            return string.Format
                (   // Main String
                    "{0} {1}",
                    // Format List
                    ErrRich("[BXTweenCore(Error)]->", true),
                    LogRich(string.Format("The type ({0}) is not tweenable!", ReasonNotTwn))
                );
        }
        public static string GetErr_ContextInvalidMsg(string ReasonInvalid)
        {
            return string.Format
                (   // Main String
                    "{0} {1}",
                    // Format List
                    ErrRich("[BXTweenCore(Critical)]", true),
                    LogRich(string.Format("The given context is not valid. Reason : \"{0}\"", ReasonInvalid))
                );
        }
        public static string GetErr_BXTwCTXCtorExcept(Exception e)
        {
            if (e == null)
                e = new Exception("ERROR : The passed exception was null.");

            return string.Format
                (   // Main String
                    "{0} {1}",
                    // Format List
                    ErrRich("[BXTweenCTX::(Critical)]", true),
                    LogRich(
                        string.Format(
                        "An exception occured while constructing class.\n--Exception Details--\nMsg:{0}\nStackTrace:{1}",
                        e.Message, e.StackTrace))
                );
        }
        #endregion
    }

    #region BXTween Core Classes
    /// <summary>
    /// Core of the BXTweenCore.
    /// <br>Dispatches the coroutines.</br>
    /// </summary>
    [ExecuteAlways()]
    public class BXTweenCore : MonoBehaviour
    {
        #region BXTweenCore Functions
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void InitilazeBXTw()
        {
            // Unity Editor Checks
#if UNITY_EDITOR
            if (Current != null)
            { EditorDisposeBXTwObject(); }
#endif

            // Standard creation
            GameObject twObject = new GameObject("BXTweenCore");
            DontDestroyOnLoad(twObject);

            var BXTwComponent = twObject.AddComponent<BXTweenCore>();
            Current = BXTwComponent;
        }

        #region Editor Playback (Experimental)
#if UNITY_EDITOR
        private GameObject previewObject;
        /// <summary>
        /// Initilaze BXTween on editor.
        /// The object gets destroyed after the tweens are done.
        /// </summary>
        internal static void EditorInitilazeBXTw()
        {
            if (Current != null)
            {
                if (Current.previewObject != null)
                {
                    Debug.LogWarning(BXTweenStrings.Warn_BXTwCoreAlreadyExist);
                    return;
                }
            }

            GameObject previewObject = new GameObject
            {
                name = "Preview BXTweenCore",
                tag = "EditorOnly"
            };

            var BXTWComp = previewObject.AddComponent<BXTweenCore>();
            Current = BXTWComp;
            Current.previewObject = previewObject;
        }
        /// <summary>
        /// Dispose the object.
        /// </summary>
        public static void EditorDisposeBXTwObject()
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
        // (maybe) TODO : Make a generic lerp 'To' method, setting using a dictionary of generic delegates for lerping.
        public IEnumerator To(BXTweenCTX<float> ctx)
        {
            // Notes (Do not copy to other methods) :
            // Note that do your enchantments in the 'float' To Method,
            // than port it over to more complicated types.
            // Note that this is boilerplate code as everything here is kinda type independent except for the method
            // (thank god for 'var' keyword)

            // FIXME : Fix this goto thing (turn it into a while loop, the c# compiler does that, but i need code cleanness)
        _Start:
            // -- Check Context
            if (!ctx.ContextIsValid)
            {
                Debug.LogError(BXTweenStrings.GetErr_ContextInvalidMsg(ctx.DebugGetContextInvalidMsg()));
                yield break;
            }

            // -- Start Tween Coroutine -- //
            yield return new WaitForEndOfFrame();
            if (ctx.StartDelay > 0f)
            { yield return new WaitForSeconds(ctx.StartDelay); }

            // Start Interpolator
            float Elapsed = 0f;
            bool UseCustom = ctx.CustomTimeCurve != null;
            while (Elapsed <= 1f)
            {
                // Check if the timescale is tampered with
                // if it's below zero, just skip the frame
                if (Time.timeScale <= 0f)
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
                // Invoke ending method on repeat.
                if (ctx.InvokeEventOnRepeat)
                {
                    if (ctx.OnEndAction != null)
                    {
                        ctx.OnEndAction();
                    }
                    if (ctx.OnEndAction_UnityEvent != null)
                    {
                        ctx.OnEndAction_UnityEvent.Invoke(ctx);
                    }
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

            // NOTE : Call this before actions to stop annoying stuff from happening.
            // Call this to avoid IEnumerator errors.
            // As this is technically a (OnTweenEnd) call.
            ctx.StopTween();
        }
        public IEnumerator To(BXTweenCTX<Color> ctx)
        {
        _Start:

            // -- Check Context
            if (!ctx.ContextIsValid)
            {
                Debug.LogError(BXTweenStrings.GetErr_ContextInvalidMsg(ctx.DebugGetContextInvalidMsg()));
                yield break;
            }

            // -- Start Tween Coroutine -- //
            yield return new WaitForEndOfFrame();
            if (ctx.StartDelay > 0f)
            { yield return new WaitForSeconds(ctx.StartDelay); }

            // Start Interpolator
            float Elapsed = 0f;
            bool UseCustom = ctx.CustomTimeCurve != null;
            while (Elapsed <= 1f)
            {
                // Check if the timescale is tampered with
                // if it's below zero, just skip the frame
                if (Time.timeScale <= 0f)
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
                    if (ctx.OnEndAction != null)
                    {
                        ctx.OnEndAction.Invoke();
                    }
                    if (ctx.OnEndAction_UnityEvent != null)
                    {
                        ctx.OnEndAction_UnityEvent.Invoke(ctx);
                    }
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

            // Call this to avoid IEnumerator errors.
            // As this is technically a (OnTweenEnd) call.
            ctx.StopTween();
        }
        public IEnumerator To(BXTweenCTX<Vector2> ctx)
        {
        _Start:

            // -- Check Context
            if (!ctx.ContextIsValid)
            {
                Debug.LogError(BXTweenStrings.GetErr_ContextInvalidMsg(ctx.DebugGetContextInvalidMsg()));
                yield break;
            }

            // -- Start Tween Coroutine -- //
            yield return new WaitForEndOfFrame();
            if (ctx.StartDelay > 0f)
            { yield return new WaitForSeconds(ctx.StartDelay); }

            // Start Interpolator
            float Elapsed = 0f;
            bool UseCustom = ctx.CustomTimeCurve != null;
            while (Elapsed <= 1f)
            {
                // Check if the timescale is tampered with
                // if it's below zero, just skip the frame
                if (Time.timeScale <= 0f)
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
                    if (ctx.OnEndAction != null)
                    {
                        ctx.OnEndAction.Invoke();
                    }
                    if (ctx.OnEndAction_UnityEvent != null)
                    {
                        ctx.OnEndAction_UnityEvent.Invoke(ctx);
                    }
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

            // Call this to avoid IEnumerator errors.
            // As this is technically a (OnTweenEnd) call.
            ctx.StopTween();
        }
        public IEnumerator To(BXTweenCTX<Vector3> ctx)
        {
        _Start:

            // -- Check Context
            if (!ctx.ContextIsValid)
            {
                Debug.LogError(BXTweenStrings.GetErr_ContextInvalidMsg(ctx.DebugGetContextInvalidMsg()));
                yield break;
            }

            // -- Start Tween Coroutine -- //
            yield return new WaitForEndOfFrame();
            if (ctx.StartDelay > 0f)
            { yield return new WaitForSeconds(ctx.StartDelay); }

            // Start Interpolator
            float Elapsed = 0f;
            bool UseCustom = ctx.CustomTimeCurve != null;
            while (Elapsed <= 1f)
            {
                // Check if the timescale is tampered with
                // if it's below zero, just skip the frame
                if (Time.timeScale <= 0f)
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
                    if (ctx.OnEndAction != null)
                    {
                        ctx.OnEndAction.Invoke();
                    }
                    if (ctx.OnEndAction_UnityEvent != null)
                    {
                        ctx.OnEndAction_UnityEvent.Invoke(ctx);
                    }
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

            // Call this to avoid IEnumerator errors.
            // As this is technically a (OnTweenEnd) call.
            ctx.StopTween();
        }
        public IEnumerator To(BXTweenCTX<Quaternion> ctx)
        {
        _Start:

            // -- Check Context
            if (!ctx.ContextIsValid)
            {
                Debug.LogError(BXTweenStrings.GetErr_ContextInvalidMsg(ctx.DebugGetContextInvalidMsg()));
                yield break;
            }

            // -- Start Tween Coroutine -- //
            yield return new WaitForEndOfFrame();
            if (ctx.StartDelay > 0f)
            { yield return new WaitForSeconds(ctx.StartDelay); }

            // Start Interpolator
            float Elapsed = 0f;
            bool UseCustom = ctx.CustomTimeCurve != null;
            while (Elapsed <= 1f)
            {
                // Check if the timescale is tampered with
                // if it's below zero, just skip the frame
                if (Time.timeScale <= 0f)
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
                    if (ctx.OnEndAction != null)
                    {
                        ctx.OnEndAction.Invoke();
                    }
                    if (ctx.OnEndAction_UnityEvent != null)
                    {
                        ctx.OnEndAction_UnityEvent.Invoke(ctx);
                    }
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

            // Call this to avoid IEnumerator errors.
            // As this is technically a (OnTweenEnd) call.
            ctx.StopTween();
        }
        public IEnumerator To(BXTweenCTX<Matrix4x4> ctx)
        {
        _Start:

            // -- Check Context
            if (!ctx.ContextIsValid)
            {
                Debug.LogError(BXTweenStrings.GetErr_ContextInvalidMsg(ctx.DebugGetContextInvalidMsg()));
                yield break;
            }

            // -- Start Tween Coroutine -- //
            yield return new WaitForEndOfFrame();
            if (ctx.StartDelay > 0f)
            { yield return new WaitForSeconds(ctx.StartDelay); }

            // Start Interpolator
            float Elapsed = 0f;
            bool UseCustom = ctx.CustomTimeCurve != null;
            while (Elapsed <= 1f)
            {
                // Check if the timescale is tampered with
                // if it's below zero, just skip the frame
                if (Time.timeScale <= 0f)
                { yield return null; }

                // Set lerp (Conditional for unclamped)
                var SetValue = ctx.UseUnclampedLerp
                    ? BXTweenCustomLerp.MatrixLerpUnclamped(ctx.StartValue, ctx.EndValue, ctx.TimeSetLerp(Elapsed))
                    : BXTweenCustomLerp.MatrixLerp(ctx.StartValue, ctx.EndValue, ctx.TimeSetLerp(Elapsed));

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
                    if (ctx.OnEndAction != null)
                    {
                        ctx.OnEndAction.Invoke();
                    }
                    if (ctx.OnEndAction_UnityEvent != null)
                    {
                        ctx.OnEndAction_UnityEvent.Invoke(ctx);
                    }
                }

                // Repeat Amount Rules : If repeat amount is bigger than 0, subtract until 0
                // If repeat amount is not 0 (negative number), repeat indefinetly until StopTween() is called.
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

            // Call this to avoid IEnumerator errors.
            // As this is technically a (OnTweenEnd) call.
            ctx.StopTween();
        }

        // -- Animation To Methods
        /*
        private IEnumerator To(BXTweenCTX<int> aCtx, int FPS = 60)
        {
            if (!aCtx.ContextIsValid)
            {
                Debug.LogError(BXTweenStrings.GetErr_ContextInvalidMsg(aCtx.Debug_GetContextValidMsg()));
                yield break;
            }

            // Wait for all the commands to get through.
            yield return new WaitForEndOfFrame();

            if (aCtx.StartDelay > 0f)
            { yield return new WaitForSeconds(aCtx.StartDelay); }

            // (1000 / FPS) returns ms delay, splitting with 1000 to make it float delay.
            // TODO : Make this function proper and find a more efficient way of waiting for seconds.
            // - Maybe try FixedUpdate delegate running coroutine tied to BXTweenCore?
            for (int i = 0; i < aCtx.EndValue; i++)
            {
                // Check if the timescale is tampered with
                // if it's below zero, just skip the frame
                if (Time.timeScale <= 0f)
                { yield return null; }

                aCtx.SetterFunction(i);

                // Wait for FPS tick
                // TODO : Why
                yield return new WaitForSeconds(1000 / FPS / 1000f);
                //yield return null;
            }

            yield return null;
        }
        */
        #endregion
    }

    /// <summary>
    /// Custom Lerp methods used by BXTween.
    /// </summary>
    public static class BXTweenCustomLerp
    {
        public static Matrix4x4 MatrixLerp(Matrix4x4 src, Matrix4x4 dest, float time)
        {
            return MatrixLerpUnclamped(src, dest, Mathf.Clamp01(time));
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
    public static class BXTween
    {
        public static BXTweenCore Current;
        public static List<ITweenCTX> CurrentRunningTweens = new List<ITweenCTX>();
        private static BXTweenSettings _CurrentSettings;
        public static BXTweenSettings CurrentSettings
        {
            get
            {
                if (_CurrentSettings == null)
                    _CurrentSettings = BXTweenSettings.Instance;

                if (_CurrentSettings == null)
                {
                    // We are still null, create instance at given const resources directory.
                    // Maybe we can add a EditorPref for creation directory?
                    _CurrentSettings = BXTweenSettings.CreateEditorInstance(BXTweenStrings.SettingsResourceCreatePath, BXTweenStrings.SettingsResourceCreateName);
                }

                return _CurrentSettings;
            }
        }

        private static readonly MethodInfo[] BXTweenMethods = typeof(BXTween).GetMethods();

        #region Utility
        /// <summary>
        /// Utility for checking if the type is tweenable.
        /// </summary>
        /// <param name="t">Type to check.</param>
        /// <returns>Bool = If it's tweenable | MethodInfo = For usage if tweenable</returns>
        public static KeyValuePair<bool, MethodInfo> IsTweenableType(Type t)
        {
            // Get method if it's tweenable (dynamic using reflection)
            var info = BXTweenMethods.Single(x => x.Name == nameof(To) && x.GetParameters()[0].ParameterType == t);
            return new KeyValuePair<bool, MethodInfo>(info != null, info);
        }
        /// <summary>
        /// Check the status of <see cref="BXTweenCore"/> <see cref="Current"/> variable.
        /// </summary>
        /// <returns>Whether if the tweening 'engine' is ok.</returns>
        public static bool CheckStatus()
        {
            // Editor checks
            if (Current == null)
            {
                if (!Application.isPlaying && Application.isEditor)
                {
#if UNITY_EDITOR
                    Debug.LogWarning(BXTweenStrings.Warn_EditorBXTwCoreNotInit);
                    BXTweenCore.EditorInitilazeBXTw();
#endif
                }
                else // Not editor or playing
                {
                    // Re-initilaze on editor.
                    Debug.LogWarning(BXTweenStrings.Warn_BXTwCoreNotInit);
                    BXTweenCore.InitilazeBXTw();
                }
            }
            // It still is null, print error and return false.
            if (Current == null)
            {
                Debug.LogError(BXTweenStrings.Err_BXTwCoreFailInit);
                return false;
            }

            return true;
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
        /// <param name="TargetObject">Control the null conditions and setters. This is the object that the tween runs on.</param>
        /// <param name="StartTween">Should the tween start immediately after creation?</param>
        public static BXTweenCTX<float> To(float StartValue, float TargetValue, float Duration, BXTweenSetMethod<float> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check Core --     //
            if (!CheckStatus())
            {
                Debug.LogError(BXTweenStrings.Err_BXTwToCtxFail);
                return null;
            }

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(BXTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0f)
            {
                Debug.Log(BXTweenStrings.Log_BXTwDurationZero);
                Setter(TargetValue);

                return null;
            }

            // -- Make Context -- //
            // Note that the lambda expression '(BXTweenCTX<T> ctx) => { return Current.To(ctx); }' is used to refresh the coroutine with current context.
            var Context = new BXTweenCTX<float>(StartValue, TargetValue, TargetObject, Duration, Setter, (BXTweenCTX<float> ctx) => { return Current.To(ctx); });

            if (StartTween)
                Context.StartTween();
            
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
        public static BXTweenCTX<Color> To(Color StartValue, Color TargetValue, float Duration, BXTweenSetMethod<Color> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check Core --     //
            if (!CheckStatus())
            {
                Debug.LogError(BXTweenStrings.Err_BXTwToCtxFail);
                return null;
            }

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(BXTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0f)
            {
                Debug.Log(BXTweenStrings.Log_BXTwDurationZero);
                Setter(TargetValue);

                return null;
            }

            // -- Make Context -- //
            // Note that the lambda expression '(BXTweenCTX<T> ctx) => { return Current.To(ctx); }' is used to refresh the coroutine with current context.
            var Context = new BXTweenCTX<Color>(StartValue, TargetValue, TargetObject, Duration, Setter, (BXTweenCTX<Color> ctx) => { return Current.To(ctx); });

            if (StartTween)
                Context.StartTween();

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
        public static BXTweenCTX<Vector2> To(Vector2 StartValue, Vector2 TargetValue, float Duration, BXTweenSetMethod<Vector2> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check Core --     //
            if (!CheckStatus())
            {
                Debug.LogError(BXTweenStrings.Err_BXTwToCtxFail);
                return null;
            }

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(BXTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0f)
            {
                Debug.Log(BXTweenStrings.Log_BXTwDurationZero);
                Setter(TargetValue);

                return null;
            }

            // -- Make Context -- //
            // Note that the lambda expression '(BXTweenCTX<T> ctx) => { return Current.To(ctx); }' is used to refresh the coroutine with current context.
            var Context = new BXTweenCTX<Vector2>(StartValue, TargetValue, TargetObject, Duration, Setter, (BXTweenCTX<Vector2> ctx) => { return Current.To(ctx); });

            if (StartTween)
                Context.StartTween();

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
        public static BXTweenCTX<Vector3> To(Vector3 StartValue, Vector3 TargetValue, float Duration, BXTweenSetMethod<Vector3> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check Core --     //
            if (!CheckStatus())
            {
                Debug.LogError(BXTweenStrings.Err_BXTwToCtxFail);
                return null;
            }

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(BXTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0f)
            {
                Debug.Log(BXTweenStrings.Log_BXTwDurationZero);
                Setter(TargetValue);

                return null;
            }

            // -- Make Context -- //
            // Note that the lambda expression '(BXTweenCTX<T> ctx) => { return Current.To(ctx); }' is used to refresh the coroutine with current context.
            var Context = new BXTweenCTX<Vector3>(StartValue, TargetValue, TargetObject, Duration, Setter, (BXTweenCTX<Vector3> ctx) => { return Current.To(ctx); });

            if (StartTween)
                Context.StartTween();

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
        public static BXTweenCTX<Quaternion> To(Quaternion StartValue, Quaternion TargetValue, float Duration, BXTweenSetMethod<Quaternion> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check Core --     //
            if (!CheckStatus())
            {
                Debug.LogError(BXTweenStrings.Err_BXTwToCtxFail);
                return null;
            }

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(BXTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0f)
            {
                Debug.Log(BXTweenStrings.Log_BXTwDurationZero);
                Setter(TargetValue);

                return null;
            }

            // -- Make Context -- //
            // Note that the lambda expression '(BXTweenCTX<T> ctx) => { return Current.To(ctx); }' is used to refresh the coroutine with current context.
            var Context = new BXTweenCTX<Quaternion>(StartValue, TargetValue, TargetObject, Duration, Setter, (BXTweenCTX<Quaternion> ctx) => { return Current.To(ctx); });

            if (StartTween)
                Context.StartTween();

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
        public static BXTweenCTX<Matrix4x4> To(Matrix4x4 StartValue, Matrix4x4 TargetValue, float Duration, BXTweenSetMethod<Matrix4x4> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // -- Check Core --     //
            if (!CheckStatus())
            {
                Debug.LogError(BXTweenStrings.Err_BXTwToCtxFail);
                return null;
            }

            // -- Check Method Parameters -- //
            if (Setter == null)
            {
                Debug.LogError(BXTweenStrings.Err_SetterFnNull);
                return null;
            }
            if (Duration <= 0f)
            {
                Debug.Log(BXTweenStrings.Log_BXTwDurationZero);
                Setter(TargetValue);

                return null;
            }

            // -- Make Context -- //
            // Note that the lambda expression '(BXTweenCTX<T> ctx) => { return Current.To(ctx); }' is used to refresh the coroutine with current context.
            var Context = new BXTweenCTX<Matrix4x4>(StartValue, TargetValue, TargetObject, Duration, Setter, (BXTweenCTX<Matrix4x4> ctx) => { return Current.To(ctx); });

            if (StartTween)
                Context.StartTween();

            // Return Context
            return Context;
        }
        #endregion

        #region Reflection To
        /// <summary>
        /// Create a tween manually. Note that you have to pass a tweenable type.
        /// (The ones that exist in <see cref="BXTween"/> class.)
        /// </summary>
        /// <param name="StartValue">The start value.</param>
        /// <param name="TargetValue">The end value.</param>
        /// <param name="Setter">The setter. Pass your function.</param>
        /// <param name="Duration">Length of tween.</param>
        /// <param name="TargetObject">Control the null conditions and setters.</param>
        /// <param name="StartTween">Should the tween start immediately after creation?</param>
        public static BXTweenCTX<T> GenericTo<T>(T StartValue, T TargetValue, float Duration, BXTweenSetMethod<T> Setter,
            UnityEngine.Object TargetObject = null, bool StartTween = true)
        {
            // Call helper method
            var ValCheckPair = IsTweenableType(typeof(T));

            // Check Tweenable
            if (ValCheckPair.Key)
            {
                /// We get method <see cref="BXTween.To"/> returned from this class.
                return (BXTweenCTX<T>)ValCheckPair.Value.Invoke(null, new object[] { StartValue, TargetValue, Duration, Setter, 
                        TargetObject, StartTween });
            }
            else
            {
                Debug.LogError(BXTweenStrings.GetErr_NonTweenableType(typeof(T).ToString()));
                return null;
            }
        }
        #endregion

        #endregion

        // TODO : Put this to a seperate class in a seperate file.
        // Maybe call the file 'BXTweenExtensions'?
        #region Shortcuts for Unity Objects

        #region TextMeshPro
        /// <see cref="TextMeshProUGUI"/>
        public static BXTweenCTX<float> BXTwFadeAlpha(this TextMeshProUGUI target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.alpha, LastValue, Duration, (float f) => { target.alpha = f; }, target);

            return Context;
        }
        public static BXTweenCTX<Color> BXTwChangeColor(this TextMeshProUGUI target, Color LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.color, LastValue, Duration, (Color c) => { target.color = c; }, target);

            return Context;
        }
        #endregion

        #region UnityEngine.UI
        /// <see cref="CanvasGroup"/>
        public static BXTweenCTX<float> BXTwFadeAlpha(this CanvasGroup target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.alpha, LastValue, Duration, (float f) => { target.alpha = f; }, target);

            return Context;
        }

        /// <see cref="Image"/>
        public static BXTweenCTX<Color> BXTwChangeColor(this Image target, Color LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.color, LastValue, Duration, (Color c) => { target.color = c; }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwFadeAlpha(this Image target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.color.a, LastValue, Duration,
                (float f) => { target.color = new Color(target.color.r, target.color.g, target.color.b, f); }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwInterpFill(this Image target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.fillAmount, LastValue, Duration, (float f) => { target.fillAmount = f; }, target);

            return Context;
        }

        /// <see cref="RectTransform"/>
        public static BXTweenCTX<Vector3> BXTwChangePos(this RectTransform target, Vector3 LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position, LastValue, Duration, (Vector3 v) => { target.position = v; }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwChangePosX(this RectTransform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position.x, LastValue, Duration,
                (float f) => { target.position = new Vector3(f, target.position.y, target.position.z); }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwChangePosY(this RectTransform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position.y, LastValue, Duration,
                (float f) => { target.position = new Vector3(target.position.x, f, target.position.z); }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwChangePosZ(this RectTransform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position.z, LastValue, Duration,
                (float f) => { target.position = new Vector3(target.position.x, target.position.y, f); }, target);

            return Context;
        }
        public static BXTweenCTX<Vector3> BXTwChangeAnchoredPosition(this RectTransform target, Vector2 LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.anchoredPosition, LastValue, Duration,
                (Vector3 v) => { target.anchoredPosition = v; }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwChangeAnchorPosX(this RectTransform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.anchoredPosition.x, LastValue, Duration,
                (float f) => { target.anchoredPosition = new Vector2(f, target.anchoredPosition.y); }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwChangeAnchorPosY(this RectTransform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.anchoredPosition.y, LastValue, Duration,
                (float f) => { target.anchoredPosition = new Vector2(target.anchoredPosition.x, f); }, target);

            return Context;
        }
        #endregion

        #region Standard (UnityEngine)
        /// <see cref="Transform">
        public static BXTweenCTX<Vector3> BXTwChangePos(this Transform target, Vector3 LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position, LastValue, Duration,
                (Vector3 v) => { target.position = v; }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwChangePosX(this Transform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position.x, LastValue, Duration,
                (float f) => { target.position = new Vector3(f, target.position.y, target.position.z); }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwChangePosY(this Transform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position.y, LastValue, Duration,
                (float f) => { target.position = new Vector3(target.position.x, f, target.position.z); }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwChangePosZ(this Transform target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.position.z, LastValue, Duration,
                (float f) => { target.position = new Vector3(target.position.x, target.position.y, f); }, target);

            return Context;
        }
        public static BXTweenCTX<Quaternion> BXTwRotate(this Transform target, Quaternion LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.rotation, LastValue, Duration,
                (Quaternion q) => { target.rotation = q; }, target);

            return Context;
        }
        public static BXTweenCTX<Vector3> BXTwRotateEuler(this Transform target, Vector3 LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.rotation.eulerAngles, LastValue, Duration,
                (Vector3 f) => { target.rotation = Quaternion.Euler(f); }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwRotateAngleAxis(this Transform target, float LastValue, Vector3 Axis, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
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
        public static BXTweenCTX<Color> BXTwChangeColor(this Material target, Color LastValue, float Duration, string PropertyName = "_Color")
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.GetColor(PropertyName), LastValue, Duration,
                (Color c) => { target.SetColor(PropertyName, c); }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwFadeAlpha(this Material target, float LastValue, float Duration, string PropertyName = "_Color")
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
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
        public static BXTweenCTX<Color> BXTwChangeColor(this SpriteRenderer target, Color LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.color, LastValue, Duration,
                (Color c) => { target.color = c; }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwFadeAlpha(this SpriteRenderer target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
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
        public static BXTweenCTX<float> BXTwChangeFOV(this Camera target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }
            var Context = To(target.fieldOfView, LastValue, Duration,
                (float f) => { target.fieldOfView = f; }, target);

            return Context;
        }
        public static BXTweenCTX<Matrix4x4> BXTwChangeProjectionMatrix(this Camera target, Matrix4x4 LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }
            var Context = To(target.projectionMatrix, LastValue, Duration,
                (Matrix4x4 m) => { target.projectionMatrix = m; }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwChangeOrthoSize(this Camera target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.orthographicSize, LastValue, Duration,
                (float f) => { target.orthographicSize = f; }, target);

            return Context;
        }
        public static BXTweenCTX<Color> BXTwChangeBGColor(this Camera target, Color LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.backgroundColor, LastValue, Duration,
                (Color c) => { target.backgroundColor = c; }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwChangeNearClipPlane(this Camera target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
                return null;
            }

            var Context = To(target.nearClipPlane, LastValue, Duration,
                (float f) => { target.nearClipPlane = f; }, target);

            return Context;
        }
        public static BXTweenCTX<float> BXTwChangeFarClipPlane(this Camera target, float LastValue, float Duration)
        {
            if (target == null)
            {
                Debug.LogError(BXTweenStrings.Err_TargetNull);
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

    #region BXTween Enums
    /// <summary>
    /// Variable changing mode for <see cref="BXTweenCTX{T}"/>.
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

    #region BXTween Ease Classes
    /// <summary>
    /// Includes the hard coded ease types.
    /// To create custom ease types use the <see cref="AnimationCurve"/>. (in BXTween context field : <see cref="BXTweenCTX{T}.SetCustomCurve(AnimationCurve, bool)"/>.
    /// </summary>
    public static class BXTweenEase
    {
        /// <summary>
        /// All ease methods in a hashmap.
        /// NOTE : Read-only in runtime.
        /// </summary>
        public static readonly IDictionary<EaseType, BXTweenEaseSetMethod> EaseMethods = new Dictionary<EaseType, BXTweenEaseSetMethod>
        {
            // None = Linear
            // The option 'None' was added to detect default settings.
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
        // Note : All ease methods change between -Infinity~Infinity.
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
            var tVal = (t *= 2f) == 2f ? 1f : t < 1f ? -0.5f * (Mathf.Pow(2f, 10f * (t -= 1)) * Mathf.Sin((t - 0.1125f) * (2f * Mathf.PI) / 0.45f)) :
                ((Mathf.Pow(2f, -10f * (t -= 1f)) * Mathf.Sin((t - 0.1125f) * (2f * Mathf.PI) / 0.45f) * 0.5f) + 1f);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float CircularIn(float t, bool clamped = true)
        {
            var tVal = -(Mathf.Sqrt(1 - (t * t)) - 1);
            return clamped ? Mathf.Clamp01(tVal) : tVal;
        }
        public static float CircularOut(float t, bool clamped = true)
        {
            var tVal = Mathf.Sqrt(1f - ((t -= 1f) * t));
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

    #region BXTween Delegates
    // TODO : Put this to a seperate files with an 'Events' namespace.
    // -- Standard c#
    /// <summary>
    /// A blank delegate.
    /// </summary>
    public delegate void BXTweenMethod();
    /// <summary>
    /// Delegate with generic. Used for setter.
    /// </summary>
    /// <typeparam name="T">Type. Mostly struct types.</typeparam>
    /// Note that i might constraint 'T' only to struct, but idk.
    /// <param name="value">Set value.</param>
    public delegate void BXTweenSetMethod<in T>(T value);
    /// <summary>
    /// Tween easing method,
    /// <br>Used in <see cref="BXTweenEase"/>.</br>
    /// </summary>
    /// <param name="time">Time value. Interpolate time linearly if possible.</param>
    /// <returns>Interpolation value (usually between 0-1)</returns>
    public delegate float BXTweenEaseSetMethod(float time, bool clamped = true);

    // -- Unity c#
    /// <summary>
    /// Unity event for <see cref="BXTweenProperty{T}"/> and <see cref="BXTweenCTX{T}"/>
    /// </summary>
    [Serializable]
    public sealed class BXTweenUnityEvent : UnityEvent<ITweenCTX>
    { }
    #endregion

    #region BXTween Property Bases
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

        [SerializeField] protected bool _UseTweenCurve = true;
        [SerializeField] protected bool _AllowInterpolationEaseOvershoot = false;
        [SerializeField] protected AnimationCurve _TweenCurve;
        [SerializeField] protected EaseType _TweenEase = EaseType.QuadInOut;
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
        public AnimationCurve TweenCurve
        {
            get { return _TweenCurve; }
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
                    _TweenCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
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
    /// <br>Used for creating convient context, editable by the inspector.</br>
    /// <br>
    /// NOTE : Do not create a public field with this class's generic versions. 
    /// Instead inherit from types that have defined the '<typeparamref name="T"/>'. (otherwise unity doesn't serialize it)
    /// </br>
    /// </summary>
    [Serializable]
    public class BXTweenProperty<T> : BXTweenPropertyBase
    {
        #region Generic Variables
        // -----  that don't need editor ----- //
        public BXTweenSetMethod<T> SetAction
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
        public BXTweenCTX<T> TwContext { get; private set; }
        public bool IsValidContext => IsTweenableType(typeof(T)).Key;

        // ---- Private ---- //
        private BXTweenSetMethod<T> m_Setter;
        public bool IsSetup => m_Setter != null;
        #endregion

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

            TwContext = ctx;

            // ** Gather values from context.
            _Duration = ctx.Duration;
            _Delay = ctx.StartDelay;
            _TweenCurve = ctx.CustomTimeCurve;
            m_Setter = ctx.SetterFunction;
            // ** Set the other options from the property.
            UpdateProperty();

            //ctx.SetEase()
            //ctx.SetCustomCurve(_TweenCurve, !_AllowCustomCurveOvershoot);
            //ctx.SetEndingAction(OnEndAction);
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

            m_Setter = Setter;
            TwContext = GenericTo(StartValue, EndValue, _Duration, Setter, null, false);
            // We could call 'UpdateProperty' here, but it calls 'SetSetter' which used to 'try' the setter to see whether
            // if the setter was throwing an exception. So we just set the ease properties.
            // While this is bad, it will stay this way as in future 'UpdateProperty' caused some hard to notice bugs.
            TwContext.SetEase(_TweenEase).SetCustomCurve(UseTweenCurve ? _TweenCurve : null, !_AllowInterpolationEaseOvershoot);
        }
        public void SetupProperty(BXTweenSetMethod<T> Setter)
        {
            SetupProperty(default, default, Setter);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the property's variables after something is changed.
        /// </summary>
        public override void UpdateProperty()
        {
            if (TwContext == null) return;

            // -- Set the settings
            // This class is essentially a settings wrapper.
            TwContext.SetDelay(_Delay).SetDuration(_Duration).SetEase(_TweenEase).
                SetCustomCurve(UseTweenCurve ? _TweenCurve : null, !_AllowInterpolationEaseOvershoot).
                SetRepeatAmount(_RepeatAmount).SetRepeatType(_TweenRepeatType);

            // -- Null checks (for the ending actions, we still check null while invoking those)
            if (OnEndAction != null)
            {
                TwContext.SetEndingAction(OnEndAction);
            }

            // Basically whenever we call 'SetSetter', the BXTweenContext tries whether if the setter is valid by calling it on a try block
            // This type of error checking is dumb so i removed it.
            // Thus making a need of 'applySetter' parameter obsolete.
            if (m_Setter != null)
            {
                TwContext.SetSetter(m_Setter);
            }
        }

        public void StartTween(T StartValue, T EndValue, BXTweenSetMethod<T> Setter = null)
        {
            if (m_Setter == null && Setter == null)
            {
                Debug.LogError(BXTweenStrings.Err_SetterFnNull);
                return;
            }

            if (TwContext == null) SetupProperty(StartValue, EndValue, Setter);

            // Make sure to set these values
            TwContext.SetStartValue(StartValue).SetEndValue(EndValue);

            StartTween();
        }

        public void StartTween()
        {
            // ** Parameterless 'StartTween()'
            // This takes the parameters from the tween Context.
            if (TwContext == null)
            {
                Debug.LogWarning(BXTweenStrings.Warn_BXTwPropertyTwNull);
                return;
            }

            // Stop tween under control 
            var invokeEventOnStop = TwContext.InvokeEventOnStop;
            TwContext.SetInvokeActionOnStop(InvokeEventOnManualStop);
            Debug.Log($"Time = {Time.time} Set InvokeEventOnStop to => {TwContext.InvokeEventOnStop} | Previous was => {invokeEventOnStop}.");
            if (TwContext.IsRunning)
                TwContext.StopTween();
            TwContext.SetInvokeActionOnStop(invokeEventOnStop);

            TwContext.StartTween();
        }

        public void StopTween()
        {
            if (TwContext == null)
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
        public BXTweenPropertyFloat(float dur, float delay = 0f, bool exPol = false, AnimationCurve c = null)
        {
            _Duration = dur;
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
        public BXTweenPropertyVector2(float dur, float delay = 0f, bool exPol = false, AnimationCurve c = null)
        {
            _Duration = dur;
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
        public BXTweenPropertyVector3(float dur, float delay = 0f, bool exPol = false, AnimationCurve c = null)
        {
            _Duration = dur;
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
        public BXTweenPropertyColor(float dur, float delay = 0f, bool exPol = false, AnimationCurve c = null)
        {
            _Duration = dur;
            _Delay = delay;
            _AllowInterpolationEaseOvershoot = exPol;
            if (c != null)
            {
                _TweenCurve = c;
            }
        }
    }
    #endregion

    #endregion

    #region BXTween Context
    /// <summary>Generic tween interface. Used for storing tweens in a generic agnostic way.</summary>
    public interface ITweenCTX
    {
        /// <summary>
        /// Target object of the tween.
        /// <br>It is <c>recommended</c> for this to be assigned to a valid object.</br>
        /// </summary>
        UnityEngine.Object TargetObj { get; }

        /// <summary>
        /// Start the tween that is under this context.
        /// </summary>
        void StartTween();

        /// <summary>
        /// Stop the tween that is under this context.
        /// </summary>
        void StopTween();
    }

    /// <summary>Tween context. Not serializable, but contains the currently running tween data.</summary>
    public sealed class BXTweenCTX<T> : ITweenCTX
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
        public bool IsValuesSwitched { get { return _IsValuesSwitched || !(RepeatType == RepeatType.Reset || RepeatAmount == 0); } }

        public bool ContextIsValid
        {
            get
            {
                return StartValue != null && EndValue != null &&
                    SetterFunction != null && IteratorCoroutine != null && (TargetObject_IsOptional || TargetObj != null);
            }
        }

        // -- Pausing
        public T PauseValue { get; private set; }
        public float CoroutineElapsed;

        // --- Interpolation
        public EaseType TweenEaseType { get; private set; } = CurrentSettings.DefaultEaseType;
        public AnimationCurve CustomTimeCurve { get; private set; } = null;
        public bool UseCustomTwTimeCurve { get { return CustomTimeCurve != null; } }
        public bool UseUnclampedLerp { get; private set; } = false;
        // -- Setter (subpart of Interpolation)
        /// <summary>
        /// Time interpolation.
        /// <br>Set when you set a <see cref="SetCustomCurve(AnimationCurve, bool)"/> or <see cref="SetEase(EaseType, bool)"/>.</br>
        /// </summary>
        public Func<float, float> TimeSetLerp { get; private set; }
        public BXTweenSetMethod<T> SetterFunction { get; private set; }
        public BXTweenMethod OnEndAction { get; private set; }
        public BXTweenMethod PersistentOnEndAction { get; private set; }
        public BXTweenUnityEvent OnEndAction_UnityEvent { get; private set; }

        // --- Target (Identifier and Null checks)
        private readonly UnityEngine.Object _TargetObj;
        public UnityEngine.Object TargetObj { get { return _TargetObj; } }
        public bool TargetObject_IsOptional { get { return _TargetObj == null; } }
        public IEnumerator IteratorCoroutine { get { return _IteratorCoroutine; } }

        // --- Private Fields
        // Coroutine / Iterator 
        private Func<BXTweenCTX<T>, IEnumerator> _TweenIteratorFn;   // Delegate to get coroutine suitable for this class
        private IEnumerator _IteratorCoroutine;                     // Current setup iterator (not running)
        private IEnumerator _CurrentIteratorCoroutine;              // Current running iterator
        #endregion

        #region Variable Setter
        public BXTweenCTX<T> ClearEndingAction()
        {
            OnEndAction = null;

            return this;
        }
        /// <summary>
        /// Sets an event to be occured in end.
        /// </summary>
        public BXTweenCTX<T> SetEndingAction(BXTweenMethod Event, VariableChangeMode mode = VariableChangeMode.ChangeMode_Add)
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
        public BXTweenCTX<T> SetEndingAction(BXTweenUnityEvent Event)
        {
            OnEndAction_UnityEvent = Event;

            UpdateContextCoroutine();
            return this;
        }
        public BXTweenCTX<T> SetInvokeActionOnStop(bool value)
        {
            InvokeEventOnStop = value;

            UpdateContextCoroutine();
            return this;
        }
        /// <summary>
        /// Sets the duration.
        /// </summary>
        /// <param name="dur">Duration to set.</param>
        public BXTweenCTX<T> SetDuration(float dur)
        {
            Duration = dur;

            UpdateContextCoroutine();
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

            UpdateContextCoroutine();
            return this;
        }
        /// <summary>
        /// Sets starting delay. Has no effect if the tween has already started.
        /// Anything below zero(including zero) is a special value for no delay.
        /// Randomizes the delay between 2 random float values.
        /// </summary>
        /// <param name="delay">The delay for tween to wait.</param>
        public BXTweenCTX<T> SetRandDelay(float min_delay, float max_delay)
        {
            StartDelay = UnityEngine.Random.Range(min_delay, max_delay);

            UpdateContextCoroutine();
            return this;
        }
        /// <summary>
        /// Sets repeat amount. '0' is default for no repeat. Anything lower than '0' is infinite repeat.
        /// </summary>
        public BXTweenCTX<T> SetRepeatAmount(int repeat)
        {
            RepeatAmount = repeat;

            UpdateContextCoroutine();
            return this;
        }
        /// <summary>
        /// Sets repeat type.
        /// </summary>
        public BXTweenCTX<T> SetRepeatType(RepeatType type)
        {
            RepeatType = type;

            UpdateContextCoroutine();
            return this;
        }
        public BXTweenCTX<T> SetInvokeEventOnRepeat(bool value)
        {
            InvokeEventOnRepeat = value;

            UpdateContextCoroutine();
            return this;
        }
        public BXTweenCTX<T> SetEase(EaseType ease, bool Clamp01 = true)
        {
            if (UseCustomTwTimeCurve)
            {
                Debug.Log(BXTweenStrings.GetLog_BXTwCTXCustomCurveInvalid(ease));
                return this;
            }

            // Note : This is only set for getting read-only data.
            TweenEaseType = ease;

            // Setup curve
            var EaseMethod = BXTweenEase.EaseMethods[TweenEaseType];
            TimeSetLerp = (float progress) => { return EaseMethod.Invoke(progress, Clamp01); };

            // Update
            UpdateContextCoroutine();
            return this;
        }
        /// <summary> Sets a custom animation curve. Pass null to disable custom curve. </summary>
        /// <param name="c">Curve to set.</param>
        /// <param name="Clamp01">Should the curve be clamped?</param>
        public BXTweenCTX<T> SetCustomCurve(AnimationCurve c, bool Clamp01 = true)
        {
            // Check curve status
            if (c == null)
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
            SetterFunction = setter;
            UpdateContextCoroutine();
            return this;
        }
        public BXTweenCTX<T> SetStartValue(T sValue)
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
        public BXTweenCTX<T> SetEndValue(T eValue)
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

            if (CurrentSettings.diagnosticMode)
            {
                Debug.Log(BXTweenStrings.GetDLog_BXTwCTXOnUpdateContextCoroutine(this));
            }

            return _IteratorCoroutine != null;
            // -- Reflection Method --
            // _IteratorCoroutine = (IEnumerator)typeof(BXTweenCore).GetMethods().
            //    Single(x => x.Name == nameof(BXTweenCore.To) && x.GetParameters()[0].ParameterType == GetType()).
            //    Invoke(Current, new object[] { this });
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
                SetterFunction += (T valuePause) => { PauseValue = valuePause; }; // Add pausing delegate to the setter function.
                PersistentOnEndAction = PersistentEndMethod;

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
                Debug.LogError(BXTweenStrings.GetErr_BXTwCTXCtorExcept(e));
            }

            if (CurrentSettings.diagnosticMode)
            {
                Debug.Log(BXTweenStrings.GetDLog_BXTwCTXOnCtor(this));
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
                    Debug.LogError(BXTweenStrings.Err_BXTwCTXFailUpdate);
                    return;
                }
            }
#if UNITY_EDITOR
            // Unity Editor
            if (!Application.isPlaying && Application.isEditor)
            {
                EditModeCoroutineExec.StartCoroutine(IteratorCoroutine);
                return;
            }
#endif

            // Default behaviour.
            if (IsRunning)
            {
                StopTween();
            }

            // Standard
            if (!UpdateContextCoroutine())
            {
                Debug.LogError(BXTweenStrings.Err_BXTwCTXFailUpdateCoroutine);
            }

            _CurrentIteratorCoroutine = IteratorCoroutine;
            Current.StartCoroutine(_CurrentIteratorCoroutine);
            IsRunning = true;

            if (CurrentSettings.diagnosticMode)
            {
                Debug.Log(BXTweenStrings.GetDLog_BXTwCTXOnStart(this));
            }
            
            CurrentRunningTweens.Add(this);
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
                Debug.LogError(BXTweenStrings.Err_BXTwCTXNoIterator);
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
                // Coroutine should stop itself HOWEVER when stop is not called by BXTweenCore.To
                // it needs to STOP manually.
                Current.StopCoroutine(_CurrentIteratorCoroutine);
                _CurrentIteratorCoroutine = null;
            }
            else
            {
                // _CurrentIteratorCoroutine is null.
                if (CurrentSettings.diagnosticMode)
                {
                    Debug.LogWarning(BXTweenStrings.Warn_BXTwCurrentIteratorNull);
                }
            }

            IsRunning = false;

            // If we call these on the ending of the BXTweenCore's To method, the ending method after delay doesn't work.
            // The reason is that in BXTweenProperty we call 'StopTween' when we call 'StartTween'
            // So yeah, i need to find a more elegant solution to that.
            if (InvokeEventOnStop)
            {
                Debug.Log($"[BXTween::InvokeEventOnManualStop] Invoking ending events. Tween Info : {BXTweenStrings.Log_BXTwCTXGetDebugFormatInfo(this)}");

                if (OnEndAction != null)
                    OnEndAction.Invoke();
                if (PersistentOnEndAction != null)
                    PersistentOnEndAction.Invoke();
                if (OnEndAction_UnityEvent != null)
                    OnEndAction_UnityEvent.Invoke(this);
            }

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
        /// <returns>Returns the message.</returns>
        /// Note that this is a bad way of getting the debug context message.
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

            // -------------------------------------- These might happen due to user error or dev error.
            if (SetterFunction == null)
            {
                return "(Context)The given setter function is null. If you called 'BXTween.To()' manually please make sure you set a setter function, otherwise it's a developer error.";
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
