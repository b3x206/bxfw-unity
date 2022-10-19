/// BXTween Version : 0.6
///                  (state, major, minor) : 
///                  state => Stability/usability state : 0 is alpha, 1 is going to be a 'betaish' release
///                  major => Major revision introducing features & api change (this is going to be very messy in alpha)
///                  minor => minor bug fixes (hotfixes also enter to this category)
/// <remarks>
/// BXTween :
///     It does tweens.
/// 
/// How do use? :
///     <example>
///     // basically dotween but crappier
///     objectOrPropertyThatHasTypeThatSupportsBXTweenExtensions.BXTw{ExtensionContext}({EndValue}, {Duration});
///     // or you can start a manual one (note : method signature may change)
///     BXTween.To({StartValue}, {EndValue}, {Duration}, (supportedType t) => 
///     {
///         // Setter function goes here, example one looks like :
///         variableThatNeedTween.somethingThatIsTweenable = t; 
///     });
///     
///     // even better, you can declare a BXTweenProperty{Type goes here, don't use the generic version if you want it to appear in the inspector}
///     // .. This example code interpolates the alpha of a canvas group.
///     ... declared in the monobehaviour/scriptableobject/anything unity can serialize, serializable variable scope 'public or private with serialize field', 
///     BXTweenPropertyFloat interpolateThing = new BXTweenPropertyFloat({DurationDefault}, {DelayDefault}, {Curve/Ease Overshoot allow}, {CurveDefault})
///     ... called inside a method, you should already know this
///     // Always setup property before calling StartTween!!
///     if (!interpolateThing.IsSetup)
///         interpolateThing.SetupProperty((float f) => { canvasGroupThatExists.alpha = f; }); // you can also declare start-end values like ({StartValue}, {EndValue}, {Setter})
///     
///     interpolateThing.StartTween(canvasGroupThatExists.alpha, 1f);
///     // congrats now you know how to use bxtween enjoy very performance
///     </example>
/// 
/// What else?  :
///     1 : Has many features (bugs, but i call them features)
///     2 : Runs with very high performance (about 1 tween per 3 hours!! [that is equal to 1,54320987654321e-6 tweens per second] wow such fast)
///     3 : Very coding pattern (what)
/// 
/// But why did you spend effort on this? there are better alternatives :
///     I don't know
/// 
/// Okay, but i am still not convinced yet.
///     cool, i will convince
///     It has, uh, properties. yeah and it also has lackluster support of shortcut methods
///     In fact half of the source code lines are shortcut methods (this (type name) variableName).
/// 
/// </remarks>

/** -------------------------------------------------- 
/// General TODO:
/// 1: Lower boilerplate (Flags.VeryHard | Flags.GoneWrong)
/// 'Partially done as the 'To' methods are lower boilerplated'
* -------------------------------------------------- **/

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using BXFW.Tweening.Editor;
#endif
using BXFW.Tweening.Events;
using static BXFW.Tweening.BXTween;

namespace BXFW.Tweening
{
    /// <summary>
    /// Core of the BXTweenCore.
    /// <br>Dispatches the coroutines.</br>
    /// </summary>
    [ExecuteAlways]
    public class BXTweenCore : MonoBehaviour
    {
        // -- Initilazation
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void InitilazeBXTw()
        {
            // Unity Editor Checks
#if UNITY_EDITOR
            if (Current != null)
            { EditorDisposeBXTwObject(); }
#endif

            if (!CurrentSettings.enableBXTween)
            {
                Debug.Log(BXTweenStrings.DLog_BXTwDisabled);
                return;
            }

            // Standard creation
            GameObject twObject = new GameObject("BXTweenCore");
            DontDestroyOnLoad(twObject);

            var BXTwComponent = twObject.AddComponent<BXTweenCore>();
            Current = BXTwComponent;
        }
        // -- Editor Initilazation (Experimental)
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

        // -- To Methods (Tween Coroutine Provider)
        /// <summary>
        /// The internal 'To' method. Takes a setter and a context.
        /// <br>Doesn't do error checking to see whether if <typeparamref name="T"/> is tweenable.</br>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctx"></param>
        /// <param name="lerpMethod"></param>
        /// <returns></returns>
        public IEnumerator GenericTo<T>(BXTweenCTX<T> ctx, BXTweenLerpMethod<T> lerpMethod)
        {
            // Main Loop (with repeat)
            // c# info : 'do {} while' is used to make the loop atleast invoke once (ctx.RepeatAmount can be 0).
            do
            {
                // Check validity + get all settings 'WaitForEndOfFrame'
                if (!ctx.ContextIsValid)
                {
                    Debug.LogError(BXTweenStrings.GetErr_ContextInvalidMsg(ctx.DebugGetContextInvalidMsg()));
                    yield break;
                }
                yield return new WaitForEndOfFrame();

                // Delay (don't do delay if the context was paused)
                if (ctx.StartDelay > 0f && ctx.CurrentElapsed <= float.Epsilon /* equal to : !ctx.WasPaused*/)
                {
                    if (!CurrentSettings.ignoreTimeScale)
                        yield return new WaitForSeconds(ctx.StartDelay);
                    else
                        yield return new WaitForSecondsRealtime(ctx.StartDelay);
                }

                // Main loop
                float Elapsed = ctx.CurrentElapsed;
                bool UseCustom = ctx.CustomTimeCurve != null;
                while (Elapsed <= 1f)
                {
                    // We added option to ignore the timescale, so this is standard procedure.
                    if (!CurrentSettings.ignoreTimeScale)
                    {
                        // Check if the timescale is tampered with
                        // if it's below zero, just skip the frame
                        if (Time.timeScale <= 0f)
                        { yield return null; }
                    }

                    // Set lerp
                    // NOTE : Always use 'LerpUnclamped' as the clamping is already done (or not done) in TimeSetLerp.

                    var SetValue = lerpMethod.Invoke(ctx.StartValue, ctx.EndValue, ctx.TimeSetLerp(Elapsed));
                    ctx.CurrentElapsed = Elapsed;

                    try
                    {
                        ctx.SetterFunction(SetValue);
                    }
                    catch (Exception e)
                    {
                        // Exception occured, ignore (unless it's diagnostic mode)
                        if (CurrentSettings.diagnosticMode)
                        {
                            Debug.LogWarning(BXTweenStrings.DLog_BXTwWarnExceptOnCoroutine(e));
                        }

                        ctx.StopTween();
                        yield break;
                    }

                    Elapsed += (CurrentSettings.ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) / ctx.Duration;
                    yield return null;
                }
                try
                {
                    // End tween loop
                    ctx.SetterFunction(ctx.EndValue);
                }
                catch (Exception e)
                {
                    // Exception occured, ignore (unless it's diagnostic mode)
                    if (CurrentSettings.diagnosticMode)
                    {
                        Debug.LogWarning(BXTweenStrings.DLog_BXTwWarnExceptOnCoroutine(e));
                    }
                    ctx.StopTween();
                    yield break;
                }

                // Repeat Amount Rules : If repeat amount is bigger than 0, subtract until 0
                // If repeat amount is not 0 (negative number), repeat indefinetly until StopTween() was called.
                if (ctx.RepeatAmount > 0)
                {
                    ctx.SetRepeatAmount(ctx.RepeatAmount - 1);

                    // Repeat
                    // Invoke ending method on repeat.
                    if (ctx.InvokeEventOnRepeat)
                    {
                        ctx.InvokeEndingEventsOnStop();
                    }
                }
                // Do a swap between values.
                if (ctx.RepeatType == RepeatType.PingPong)
                {
                    ctx.SwitchStartEndValues();
                }
            }
            while (ctx.RepeatAmount != 0);

            // End tween call (also calls ending events and does neccessary clean-up)
            ctx.StopTween();
        }

        // The other 'To' methods return an iterator so the coroutines can be manually managed
        // The generic one is also usable to public, as long as you supply your own lerp method.
        public IEnumerator To(BXTweenCTX<int> ctx)
        {
            // Floats can't convert to integer unless there's an explicit cast
            // Unity should have added int lerp but idc.
            static int lerpUnclampedDelegate(int from, int to, float time) { return (int)Mathf.LerpUnclamped(from, to, time); }
            yield return GenericTo(ctx, lerpUnclampedDelegate);
        }
        public IEnumerator To(BXTweenCTX<float> ctx)
        {
            yield return GenericTo(ctx, Mathf.LerpUnclamped);
        }
        public IEnumerator To(BXTweenCTX<Color> ctx)
        {
            yield return GenericTo(ctx, Color.LerpUnclamped);
        }
        public IEnumerator To(BXTweenCTX<Vector2> ctx)
        {
            yield return GenericTo(ctx, Vector2.LerpUnclamped);
        }
        public IEnumerator To(BXTweenCTX<Vector3> ctx)
        {
            yield return GenericTo(ctx, Vector3.LerpUnclamped);
        }
        public IEnumerator To(BXTweenCTX<Quaternion> ctx)
        {
            yield return GenericTo(ctx, Quaternion.SlerpUnclamped);
        }
        public IEnumerator To(BXTweenCTX<Matrix4x4> ctx)
        {
            yield return GenericTo(ctx, BXTweenCustomLerp.MatrixLerpUnclamped);
        }
    }
}
