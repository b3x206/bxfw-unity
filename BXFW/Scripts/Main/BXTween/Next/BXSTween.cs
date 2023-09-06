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
    /// Rule #2 = idk (maybe avoid stuff like virtual methods becuase vtable overhead?)
    /// 
    /// TODO : A lot of things
    /// But first to decide what things i am gonna do.
    /// Probably it is so that 'BXSTweenable' implementing context can have a lot of features and controlability and the rest will be more normal.
    /// * Error handling for running tweens so that those tweens wouldn't hang the main running thread
    /// * A logger class for BXFW (general purpose)
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
            // Do this so that the jit is generated beforehand and so that
            // the first started tween acceesing this class doesn't allocate much garbage.
            Type bxTwEaseType = typeof(BXTweenEase);
            RuntimeHelpers.RunClassConstructor(bxTwEaseType.TypeHandle);
#endif
        }

        // -- Runtime
        /// <summary>
        /// The <see cref="BXSTween"/> runner.
        /// <br>This controls the running operations. To set this value to something else use the </br>
        /// </summary>
        public static IBXSTweenRunner MainRunner { get; private set; }

        /// <summary>
        /// The <see cref="BXSTween"/> logger.
        /// <br>Only to be used by BXSTween classes and the <see cref="IBXSTweenRunner"/> initializing this class.</br>
        /// </summary>
        internal static Logger MainLogger { get; private set; }

        /// <summary>
        /// The list of all running tweens.
        /// <br>Unless absolutely necessary, there is no need to change the contents of this.</br>
        /// <br>Can use the <see cref="BXSTweenable"/> methods on tweens here.</br>
        /// </summary>
        public static readonly List<BXSTweenable> RunningTweens = new List<BXSTweenable>(50);

        /// <summary>
        /// Initializes the <see cref="IBXSTweenRunner"/> <paramref name="runner"/> with logger <paramref name="logger"/>.
        /// </summary>
        public static void Initialize(IBXSTweenRunner runner, Logger logger)
        {
            // Call this first to call all events on the 'OnRunnerExit'
            MainRunner?.Kill();

            // Set the 'MainRunner'
            MainRunner = runner;
            // Hook the tween runner
            HookTweenRunner(runner);

            // Hook the logger
            SetLogger(logger);
        }
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

            bool isFirstRun = tween.LoopsElapsed == tween.StartingLoopCount;

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
            }

            MainRunner = null;
        }
        #endregion
    }
}