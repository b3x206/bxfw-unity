using System;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;

using UnityEngine;
using static BXFW.Tweening.BXTween;

namespace BXFW.Tweening
{
    /// Solution for stylized print strings. 
    /// note : (for+++hh9jhfjh??j); please put this code
    /// (thank you sister for valuable feedback, i will fix this)
    /// <summary>
    /// Constant strings for <see cref="BXTween"/> messages.
    /// <br>Doesn't apply styling on compiled builds of the game.</br>
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
                    continue;
                }

                fmtTarget.Append(c);
            }

            return string.Format(Bold ? "<b>{1}{0}</color></b>" : "{1}{0}</color>", fmtTarget.ToString(), logColorTag);
#else
            return RichFmtTarget;
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
        /// <br>This is only used for previewing all (well, most) strings nicely in a console.</br>
        /// </summary>
        internal static void ListStrings()
        {
            Debug.Log("<b>[ListStrings] Listing all fields in 'BXTweenStrings'.</b>");

            foreach (FieldInfo field in typeof(BXTweenStrings).GetFields())
            {
                // Pass null as it's static.
                Debug.Log(string.Format("Field | Name : {0} | ToString : {1}", field.Name, field.GetValue(null)));
            }

            Debug.Log("<b>[ListStrings] Note that dynamic strings are not outputtable. (because of those methods requiring parameters)</b>");
        }
#endif
        // -- Strings
        #region Data
        /// <summary>
        /// The <c>Resources</c> path for the unity.
        /// <br><c>Assets/Resources</c> is already appended to path, write a new directory to here.</br>
        /// </summary>
        public const string SettingsResourceCreatePath = "";
        /// <summary>
        /// The settings resource name.
        /// </summary>
        public const string SettingsResourceCreateName = "BXTweenSettings.asset";
        #endregion

        #region Info-Logs
        // Non-Dynamic

        // Dynamic
        public static string GetLog_BXTwDurationZero([CallerMemberName] string method = "Unknown")
        {
            return string.Format("{0} {1}",
                LogRich(string.Format("[BXTween::{0}]->", method), true),
                LogRich("The duration for tween is less than zero. Please set values higher than 0."));
        }
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

        #region Diagnostic Mode Strings
        internal static string Log_BXTwCTXGetDebugFormatInfo<T>(BXTweenCTX<T> gContext)
        {
            return string.Format(@"Tween Info => '{0}' with target '{1}'
Tween Details : Duration={2} Delay={3} StartVal={4} EndVal={5} HasEndActions={6} InvokeActionsOnManualStop={6}.",
                    // why a ternary that returns "Null" if target object is null? 
                    // because accessing the 'TargetObject' causes a MissingReferenceException if it was destroyed
                    // yeah.
                    gContext.ToString(), gContext.TargetObject == null ? "Null" : gContext.TargetObject.ToString(), gContext.Duration, gContext.StartDelay, gContext.StartValue, gContext.EndValue, gContext.OnEndAction == null, gContext.InvokeEventOnStop);
        }
        public static string DLog_BXTwCallGenericTo<T>(T StartValue, T TargetValue, float Duration, UnityEngine.Object TargetObject)
        {
            return string.Format("{0} {1}",
                LogDiagRich("[BXTween::GenericTo]", true),
                LogRich(string.Format(@"Generic To with type '{0}' is called.
Method parameters | StartValue: {1} TargetValue: {2} Duration: {3} TargetObject: {4}", typeof(T).Name,
                    StartValue, TargetValue, Duration, TargetObject)));
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
        public static string GetDLog_BXTwCTXOnPause<T>(BXTweenCTX<T> gContext)
        {
            return string.Format
                (   // Main String
                    "{0} {1}",
                    // Format List
                    LogDiagRich("[BXTweenCTX::PauseTween]->", true),
                    LogRich(string.Format("Called 'PauseTween()' on context \"{0}\".", Log_BXTwCTXGetDebugFormatInfo(gContext)))
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
        public static string DLog_BXTwSettingsCreatedNew(string createPath)
        {
            return string.Format
            (   // Main String
                "{0} {1}",
                // Format List
                LogDiagRich("[BXTween::(get)CurrentSettings]->", true),
                LogRich(string.Format("Created new settings in '{0}'.", createPath))
            );
        }
        public static string DLog_BXTwWarnExceptOnCoroutine(Exception e)
        {
            return string.Format
                (   // Main String
                    "{0} {1}",
                    // Format List
                    LogDiagRich("[BXTweenCore::To]->", true),
                    LogRich(string.Format("Coroutine stopped with exception : \"{0}\". Please make sure you are stopping tween / attaching tweens to a valid object. StackTrace:\"{1}\"", e.Message, e.StackTrace))
                );
        }
        public static string DLog_BXTwWarnExceptOnStop(Exception e)
        {
            return string.Format
                (   // Main String
                    "{0} {1}",
                    // Format List
                    LogDiagRich("[BXTweenCTX::StopTween]->", true),
                    LogRich(string.Format("Context failed to call stop action with exception : \"{0}\". StackTrace:\"{1}\"", e.Message, e.StackTrace))
                );
        }
        public static readonly string DLog_BXTwCTXTimeCurveAlreadyNull =
            string.Format("{0} {1}",
                LogDiagRich("[BXTweenCTX::SetCustomCurve]", true),
                LogRich("The tween time curve (related stuff) is already null. You are setting it null again"));
        public static readonly string DLog_BXTwTargetObjectInvalid =
            string.Format("{0} {1}",
                LogDiagRich("[BXTweenCore::To]", true),
                LogRich("TargetObject is null, but TargetObjectIsOptional is false on current tween. Stopping tween."));
        internal static readonly string DLog_BXTwComputerOnFire =
            string.Format("{0} {1}",
                LogDiagRich("[BXTween]", true),
                LogRich("BXTween has used up all your resources and crashed and set your computer on fire."));
        #endregion

        public static readonly string DLog_BXTwDisabled =
            string.Format("{0} {1}",
                LogDiagRich("[BXTween]", true),
                LogRich("BXTween is currently disabled. Calling any method will cause exceptions."));
        public static readonly string DLog_BXTwSetupPropertyTwCTXAlreadyExist = string.Format("{0} {1}",
                LogDiagRich("[BXTweenProperty[T]::SetupProperty]", true),
                LogRich("Called 'SetupProperty()' even though the tween context already isn't null."));
        public static readonly string DLog_BXTwCTXPauseInvalidCall = string.Format("{0} {1}",
                LogDiagRich("[BXTweenCTX[T]::PauseTween]", true),
                LogRich("Called 'PauseTween()' even though the tween wasn't running."));
        public static readonly string DLog_BXTwCTXStopInvalidCall = string.Format("{0} {1}",
                LogDiagRich("[BXTweenCTX[T]::StopTween]", true),
                LogRich("Called 'StopTween()' even though the tween wasn't running."));
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
                LogRich("The tween property is null. Make sure you assign all fields in your inspector / code. If you did that, it's probably an internal error."));
        public static readonly string Warn_BXTwCTXTimeCurveNull =
            string.Format("{0} {1}",
                WarnRich("[BXTweenCTX::SetCustomCurve]", true),
                LogRich("The tween time curve (related stuff) is null. Make sure you assign all properties in your inspector. If you did that, it's probably an internal error."));
        public static readonly string Warn_BXTwCurrentIteratorNull =
            string.Format("{0} {1}",
                WarnRich("[BXTween::StopTween]->", true),
                LogRich("The current running coroutine is null. Probably an internal thing that is not very important."));
        public static readonly string Warn_BXTwCoreNotInit =
            string.Format("{0} {1}",
                WarnRich("[BXTweenCore]->", true),
                LogRich("The 'Current' reference is null. Re-initilazing. This can happen after script recompiles and other factors."));

        // -- Diagnostic mode warning(s)
        public static readonly string DWarn_BXTwUpdatePropertyCTXNull =
            string.Format("{0} {1}",
                WarnRich("[BXTweenProperty[T]::UpdateProperty]->", true),
                LogRich("The '_TwContext' is null. Cannot update."));

#if UNITY_EDITOR // Editor Only
        /// <see cref="BXTween.To"/> on editor.
        public static readonly string Warn_EditorBXTwCoreNotInit =
            string.Format("{0} {1}",
                WarnRich("[BXTween::To(OnEditor)]->", true),
                LogRich("Please make sure you initilaze BXTween for editor playback. (by calling Editor_InitilazeBXTw())"));
#endif
        // Dynamic
        public static string GetWarn_TargetConfInvalid(string whatIsWrong, [CallerMemberName] string methodName = "Unknown")
        {
            return string.Format("{0} {1}",
                WarnRich(string.Format("[BXTween::{0}]->", methodName), true),
                LogRich(string.Format("Tween target (on tween extension) is configured incorrectly. Here's what's wrong : {0}", whatIsWrong)));
        }

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
                ErrRich("[BXTweenSettings::GetBXTweenSettings]->"),
                LogRich("No resource was generated in editor. Returning default 'ScriptableObject'."));

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
        public static readonly string Err_BXTwPropNoTwCTX =
           string.Format("{0} {1}",
                ErrRich("[BXTweenProperty::(get)TwContext]->"),
                LogRich("Trying to get TwContext variable even though the property wasn't setup. Please setup property before getting TwContext."));

        // Dynamic (Needs to be generated dynamically or smth)
        /// <see cref="BXTween.To"/> methods.
        public static string GetErr_NonTweenableType(string Type)
        {
            return string.Format
                (   // Main String
                    "{0} {1}",
                    // Format List
                    ErrRich("[BXTweenCore(Error)]->", true),
                    LogRich(string.Format("The type ({0}) is not tweenable!", Type))
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

        internal static string UtilityExcept_NullArgument([CallerMemberName] string method = "None")
        {
            return $"[BXTween(general utility)::{method}] Passed argument was null.";
        }
        #endregion
    }
}
