using System;
using System.Collections;

using UnityEngine;

using BXFW.Tweening.Events;
using static BXFW.Tweening.BXTween;

/// Oh, you are actually reading this? well, this is actually not really in 'alpha' and there is a stable-ish game made (see Fall Xtra) using BXTween
/// I think at this state, BXTween is complete except for missing some features and not really following conventions for standard tweening scripts
/// Because of this, i will only add new features if i like them or i will only (only is a strong word, i could change it if there's a better way 
/// of doing stuff) change the code if there is a bug. Or i could add support to unity Dots system, if i feel enough interest to it.
/// For the time being, it works fine enough™ so yeah, this will do.
/// 
/// (alright bye)
/// 
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
///     4 : Who cares about consistency? if the code works it's good enough.
/// 
/// But why did you spend effort on this? there are better alternatives :
///     I don't know
/// 
/// Okay, but i am still not convinced yet.
///     cool, i will convince
///     It has, uh, properties. yeah and it also has lackluster support of shortcut methods
///     In fact quarter (yes quarter now we are doing REAL oop) of the source code lines are shortcut methods (this {type name}BXTw{variableName}).
/// 
/// </remarks>
/// TODO : 
/// Uh, replace this with a BXSimpleTween that is similar to this, but less features, more control over the update method and get rid of coroutines (lower gc.alloc)?
/// Basically just have delegates that do the most of the stuff.
/// Because this is not really a complete tweening solution, so go with the simpler tweening option.
/// TODO 2 :
/// Oh and also, add a 'CompilationDefineConstraints' class for defining define constraints for things if the file exists
/// (and it can disable BXTween with '#if' statement)

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
        internal static void EditorDisposeBXTwObject()
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
        public IEnumerator GenericTo<T>(BXTweenCTX<T> ctx, BXTweenLerpMethod<T> lerpMethod)
        {
            bool isOnRepeat = false;
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

                // Delay (don't do delay if the context was paused, which is checked with CurrentElapsed)
                // Also respect the tween setting for invoking delay on tween restart
                if (ctx.Delay > 0f && ctx.CurrentElapsed <= float.Epsilon)
                {
                    // Kind of boilerplatey boolean logic, but as long as it works and nobody sees it, there's no problem :)
                    if (isOnRepeat)
                    {
                        if (ctx.InvokeDelayOnRepeat)
                        {
                            if (!CurrentSettings.ignoreTimeScale)
                                yield return new WaitForSeconds(ctx.Delay);
                            else
                                yield return new WaitForSecondsRealtime(ctx.Delay);
                        }
                    }
                    else
                    {
                        if (!CurrentSettings.ignoreTimeScale)
                            yield return new WaitForSeconds(ctx.Delay);
                        else
                            yield return new WaitForSecondsRealtime(ctx.Delay);
                    }
                }

                if (ctx.OnStartAction != null)
                    ctx.OnStartAction.Invoke();

                // Main loop
                float Elapsed = ctx.CurrentElapsed;
                bool UseCustom = ctx.CustomTimeCurve != null;
                bool TargetObjectOptional = ctx.TargetObjectIsOptional; // Don't constantly get this as it can be true when the TargetObject is null.

                while (Elapsed <= 1f)
                {
                    bool canTick = true;

                    // We added option to ignore the timescale, so this is standard procedure.
                    if (!CurrentSettings.ignoreTimeScale)
                    {
                        // Check if the timescale is tampered with
                        // if it's below zero, just skip the frame
                        if (Time.timeScale <= 0f)
                            canTick = false;
                    }
                    // Tick cond check (should be true to tick)
                    if (ctx.TickTweenConditionFunction != null)
                    {
                        if (!ctx.TickTweenConditionFunction())
                        {
                            canTick = false;
                        }
                    }

                    // yield returning null on the top methods didn't respect the ticking condition
                    // These issues wouldn't have happened if i was smart enough to not use coroutines
                    if (canTick)
                    {
                        // Target object
                        if (!TargetObjectOptional && ctx.TargetObject == null)
                        {
                            if (CurrentSettings.diagnosticMode)
                            {
                                Debug.Log(BXTweenStrings.DLog_BXTwTargetObjectInvalid);
                            }

                            ctx.StopTween();
                            yield break;
                        }

                        // Set lerp
                        // NOTE : Always use 'LerpUnclamped' as the clamping is already done (or not done) in TimeSetLerp.

                        T SetValue = lerpMethod(ctx.StartValue, ctx.EndValue, ctx.TimeSetLerp(Elapsed));
                        ctx.CurrentElapsed = Elapsed;

                        try
                        {
                            ctx.SetterFunction(SetValue);
                        }
                        catch (Exception e)
                        {
                            // Exception occured, ignore (unless it's diagnostic mode or we are in editor, don't ignore if in editor.)
                            if (CurrentSettings.diagnosticMode || Application.isEditor)
                            {
                                Debug.LogWarning(BXTweenStrings.DLog_BXTwWarnExceptOnCoroutine(e));
                            }

                            ctx.StopTween();
                            yield break;
                        }

                        Elapsed += (CurrentSettings.ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) / ctx.Duration;
                        // TODO : Add useFixedTime for BXTweenCTX?
                        // yield return new WaitForFixedUpdate();
                    }
                    
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
                if (ctx.RepeatAmount != 0)
                {
                    // Infinitely loop if the 'RepeatAmount' is less than 0.
                    if (ctx.RepeatAmount > 0)
                        ctx.SetRepeatAmount(ctx.RepeatAmount - 1);

                    // Repeat
                    // Invoke ending method on repeat.
                    if (ctx.InvokeEventOnRepeat)
                    {
                        ctx.InvokeEndingEventsOnStop();
                    }

                    // Set current elapsed to 0 if we are repeating.
                    ctx.CurrentElapsed = 0f;
                    isOnRepeat = true;
                }

                // Do a swap between values.
                if (ctx.RepeatType == RepeatType.PingPong)
                {
                    ctx.SwitchStartEndValues();
                }
            }
            while (ctx.RepeatAmount != 0);

            // bool isRepeat is invalid since here
            // End tween call (also calls ending events and does neccessary clean-up)
            ctx.StopTween();
        }

        // The other 'To' methods return an iterator so the coroutines can be manually managed
        // The generic one is also usable to public, as long as you supply your own lerp method.
        public IEnumerator To(BXTweenCTX<int> ctx)
        {
            yield return GenericTo(ctx, BXTweenCustomLerp.IntLerpUnclamped);
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
