using BXFW.Tweening.Next.Events;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BXFW.Tweening.Next
{
    /// BXFW.Tweening.Next roadmap
    /// A tweening engine that can be attached to most things supporting c#.
    /// (but will still require things to do/remove because unity doesn't serialize private/protected's)
    /// 
    /// Rule #1 = NO TIMERS OR COROUTINES (timer is coroutine anyways)
    /// Only functions that update things in the <see cref="RunningTweens"/> array.
    /// Rule #2 = Fix any GC.Alloc you see if it's fixable
    /// (Note : Mono.JIT is not fixable unless il2cpp compiled, but can be mitigated)
    /// 
    /// ---------
    /// <summary>
    /// A simpler tick based tweening engine.
    /// <br>This tweening engine focuses on flexibility. (it won't be simple due to the required features)</br>
    /// <br/>
    /// <br><see cref="BXSTween"/> is mostly going to be similar to <see cref="BXTween"/>, so you will see the reuse of most parts that are fine.</br>
    /// <br/>
    /// <b>!! CAUTION !!</b>
    /// <br><see cref="BXSTween"/> is in an experimental state, i am still unsure what to do with this, the api will be different but also do the same things.</br>
    /// <br>So yeah don't use this, the actual <see cref="BXTween"/> is stabler (but dumber and has less features).</br>
    /// </summary>
    public static class BXSTween
    {
        // -- Constants
        /// <summary>
        /// A <see cref="BXSTweenable.ID"/> for no id.
        /// </summary>
        public const int NoID = 0;

        // -- Prepare
        static BXSTween()
        {
            // Only do the initialization of other classes in jit runtimes
            // Stuff like il2cpp does compilation aot, which does not allocate garbage when a code is run for the first time
            // Note : don't initialize anything related with 'BXSTween' here as it is still not completely constructed
#if ENABLE_MONO
            // Do this so that the jit is generated beforehand and so that the
            // first started tween acceesing this class doesn't allocate much garbage.
            Type bxTwEaseType = typeof(BXTweenEase);
            RuntimeHelpers.RunClassConstructor(bxTwEaseType.TypeHandle);
#endif
        }

        // -- Runtime
        /// <summary>
        /// The <see cref="BXSTween"/> runner, but it's nullable one.
        /// </summary>
        private static IBXSTweenRunner m_MainRunner;
        /// <summary>
        /// Action used to create a <see cref="IBXSTweenRunner"/>.
        /// </summary>
        private static BXSGetterAction<IBXSTweenRunner> m_GetMainRunnerAction;
        /// <summary>
        /// The <see cref="BXSTween"/> runner.
        /// <br>If the <see cref="Initialize(BXSGetterAction{IBXSTweenRunner}, Logger)"/> was called validly once by this then this value will not be null if requested.</br>
        /// <br>This controls the running operations. To set this value to something else use the <see cref="Initialize(BXSGetterAction{IBXSTweenRunner}, Logger)"/> method.</br>
        /// </summary>
        public static IBXSTweenRunner MainRunner
        {
            get
            {
                if (m_MainRunner == null)
                {
                    if (NeedsInitialize)
                        return null;

                    Initialize(m_GetMainRunnerAction, MainLogger);
                }

                return m_MainRunner;
            }
        }
        /// <summary>
        /// Whether if the BXSTween needs it's <see cref="Initialize(BXSGetterAction{IBXSTweenRunner}, Logger)"/> called.
        /// <br>After calling <see cref="Initialize(BXSGetterAction{IBXSTweenRunner}, Logger)"/> once will make this false</br>
        /// </summary>
        public static bool NeedsInitialize => m_GetMainRunnerAction == null || MainLogger == null;

        /// <summary>
        /// The <see cref="BXSTween"/> logger.
        /// <br>Only to be used by BXSTween classes and the <see cref="IBXSTweenRunner"/> initializing this class.</br>
        /// </summary>
        internal static Logger MainLogger { get; private set; }
        /// <summary>
        /// Sets a logger.
        /// </summary>
        public static void SetLogger(Logger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "[BXSTween::SetLogger] Given argument was null.");

            MainLogger = logger;
        }

        /// <summary>
        /// The list of all running tweens.
        /// <br>Unless absolutely necessary, there is no need to change the contents of this.</br>
        /// <br>Can use the <see cref="BXSTweenable"/> methods on tweens here.</br>
        /// </summary>
        public static readonly List<BXSTweenable> RunningTweens = new List<BXSTweenable>(50);

        /// <summary>
        /// Initializes the <see cref="IBXSTweenRunner"/> <paramref name="runner"/> with logger <paramref name="logger"/>.
        /// </summary>
        /// <param name="getRunnerAction">Create the runner in this method. This is called when the BXSTween is uninitialized but.</param>
        public static void Initialize(BXSGetterAction<IBXSTweenRunner> getRunnerAction, Logger logger)
        {
            if (getRunnerAction == null)
                throw new ArgumentNullException(nameof(getRunnerAction), "[BXSTween::Initialize] Given parameter is null.");
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "[BXSTween::Initialize] Given parameter is null.");

            // Call this first to call all events on the 'OnRunnerExit'
            MainRunner?.Kill();

            // Hook the tween runner
            m_MainRunner = getRunnerAction();
            HookTweenRunner(m_MainRunner);

            // Hook the logger
            SetLogger(logger);
        }

        /// <summary>
        /// Stops all tweens of <see cref="RunningTweens"/>.
        /// </summary>
        public static void StopAllTweens()
        {
            for (int i = 0; i < RunningTweens.Count; i++)
            {
                BXSTweenable tween = RunningTweens[i];
                if (tween == null)
                    continue;

                tween.Stop();
            }

            RunningTweens.Clear();
        }

        /// <summary>
        /// Clears the <see cref="BXSTween"/>, this includes the <see cref="MainRunner"/> and <see cref="MainLogger"/>.
        /// </summary>
        public static void Clear()
        {
            StopAllTweens();
            MainRunner?.Kill();
            // Don't unset the logger.
        }

        // -- Tweening
        /// <summary>
        /// Runs a tweenable.
        /// <br>The <paramref name="tween"/> itself contains the state.</br>
        /// </summary>
        public static void RunTweenable(IBXSTweenRunner runner, BXSTweenable tween)
        {
            // Checks
            if (!tween.IsValid)
            {
                RunningTweens.Remove(tween);
                MainLogger.LogError($"[BXSTweenable::RunTweenable] Invalid tween '{tween}', stopping and removing it.");
                tween.Stop();
                return;
            }
            if (!tween.IsPlaying)
            {
                RunningTweens.Remove(tween);
                MainLogger.LogWarning($"[BXSTweenable::RunTweenable] Non playing tween '{tween}' tried to be run.");
                return;
            }

            if (tween.IsInstant)
            {
                try
                {
                    tween.OnStartAction?.Invoke();
                    tween.OnEndAction?.Invoke();
                }
                catch (Exception e)
                {
                    MainLogger.LogException($"[BXSTween::RunTweenable] OnStartAction+OnEndAction in tween '{tween}'\n", e);
                }
                tween.EvaluateTween(1f);
                tween.Stop();
                
                return;
            }

            // Tickability
            if (tween.TickConditionAction != null)
            {
                TickSuspendType suspendType = TickSuspendType.None;

                try
                {
                    suspendType = tween.TickConditionAction();
                }
                catch (Exception e)
                {
                    MainLogger.LogException($"[BXSTween::RunTweenable] TickConditionAction in tween '{tween}'\n", e);
                    suspendType = TickSuspendType.Stop;
                }

                switch (suspendType)
                {
                    case TickSuspendType.Tick:
                        return;
                    case TickSuspendType.Pause:
                        tween.Pause();
                        return;
                    case TickSuspendType.Stop:
                        tween.Stop();
                        return;

                    default:
                    case TickSuspendType.None:
                        break;
                }
            }

            // DeltaTime
            float deltaTime;
            // unity didn't support the switch expression for a long time, so no switch expression.
            switch (tween.ActualTickType)
            {
                default:
                case TickType.Variable:
                    deltaTime = runner.UnscaledDeltaTime;
                    break;
                case TickType.Fixed:
                    deltaTime = runner.FixedUnscaledDeltaTime;
                    break;
            };
            if (!tween.IgnoreTimeScale)
            {
                deltaTime *= runner.TimeScale;
            }
            deltaTime *= tween.Speed;

            bool isFirstRun = tween.LoopsElapsed == 0;
            
            // Delay
            if (tween.DelayElapsed < 1f)
            {
                // Instant finish + wait one frame
                if (tween.StartingDelay <= 0f)
                {
                    tween.DelayElapsed = 1f;
                }
                else
                {
                    // Elapse delay further
                    // (only elapse if the tween has delaying)
                    tween.DelayElapsed += deltaTime / tween.StartingDelay;
                }

                if (tween.DelayElapsed >= 1f && isFirstRun)
                {
                    try
                    {
                        tween.OnStartAction?.Invoke();
                    }
                    catch (Exception e)
                    {
                        MainLogger.LogException($"[BXSTween::RunTweenable] OnStartAction in tween '{tween}'\n", e);
                    }
                }
                return;
            }

            // Tweening + Elapsing
            if (tween.CurrentElapsed < 1f)
            {
                try
                {
                    tween.EvaluateTween(tween.CurrentElapsed);
                    tween.OnTickAction?.Invoke();
                }
                catch (Exception e)
                {
                    MainLogger.LogException($"[BXSTween::RunTweenable] EvaluateTween+OnTickAction in tween '{tween}'\n", e);
                    tween.Stop();
                }
                
                tween.CurrentElapsed += deltaTime / tween.StartingDuration;

                return;
            }
            // Base tweening ended, set 'tween.EvaluateTween' with 1f.
            tween.EvaluateTween(1f);

            // Looping (infinite loop if the 'StartingLoopCount' is less than 0
            // StartingLoopCount should be greater than 0 or less than zero to be able to loop.
            // Only do loops if there is still yet to do loops
            if (tween.StartingLoopCount < 0 || tween.LoopsElapsed < tween.StartingLoopCount)
            {
                if (tween.StartingLoopCount > 0)
                    tween.LoopsElapsed++;

                // Call this before just in case the parameters are changed
                try
                {
                    tween.OnRepeatAction?.Invoke();
                }
                catch (Exception e)
                {
                    MainLogger.LogException($"[BXSTween::RunTweenable] OnRepeatAction in tween '{tween}'\n", e);
                }

                // Reset the base while looping
                tween.Reset();
                if (tween.LoopType == LoopType.Yoyo)
                    tween.IsTargetValuesSwitched = !tween.IsTargetValuesSwitched;

                return;
            }

            // Stop + Call endings
            tween.Stop();
        }

        #region IBXSRunner
        /// <summary>
        /// Hooks the tween runner.
        /// </summary>
        private static void HookTweenRunner(IBXSTweenRunner runner)
        {
            runner.OnRunnerExit += OnTweenRunnerExit;
            runner.OnRunnerTick += OnTweenRunnerTick;
            runner.OnRunnerFixedTick += OnTweenRunnerFixedTick;
        }

        private static void OnTweenRunnerTick(IBXSTweenRunner runner)
        {
            // Iterate all tweens
            for (int i = 0; i < RunningTweens.Count; i++)
            {
                // Run those tweens (if the tick is suitable)
                BXSTweenable tween = RunningTweens[i];

                if (tween.ActualTickType == TickType.Variable)
                {
                    RunTweenable(runner, tween);
                }
            }
        }
        private static void OnTweenRunnerFixedTick(IBXSTweenRunner runner)
        {
            // Iterate all tweens
            for (int i = 0; i < RunningTweens.Count; i++)
            {
                // Run those tweens (if the tick is suitable)
                BXSTweenable tween = RunningTweens[i];

                if (tween.ActualTickType == TickType.Fixed)
                {
                    RunTweenable(runner, tween);
                }
            }
        }

        /// <summary>
        /// The exit method for the <see cref="MainRunner"/>.
        /// </summary>
        private static void OnTweenRunnerExit(bool cleanup)
        {
            // If we are quitting app as well clear the BXSTween runnings.
            if (cleanup)
            {
                StopAllTweens();
                m_GetMainRunnerAction = null;
                MainLogger = null;
            }

            m_MainRunner = null;
        }
        #endregion
    }
}