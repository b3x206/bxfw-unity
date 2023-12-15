using System;
using BXFW.Tweening.Next.Events;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BXFW.Tweening.Next
{
    /// <summary>
    /// A simpler ticking + delta time based tweening engine.
    /// <br>This tweening code aims to make things simpler, but the code is more explicit.</br>
    /// <br/>
    /// <br>For shortcut methods, use the Window&gt;BXFW&gt;Editor Tasks and then add a <see cref="BXFW.Tweening.Next.Editor.BXSTweenExtensionGeneratorTask"/> there.</br>
    /// </summary>
    public static class BXSTween
    {
        // -- Constants
        /// <summary>
        /// A <see cref="BXSTweenable.ID"/> for no id.
        /// </summary>
        public const int NoID = 0;

        // -- Prepare
#if !UNITY_2017 || ENABLE_MONO
        static BXSTween()
        {
            // Only do the initialization of other classes in jit runtimes
            // Stuff like il2cpp does compilation aot, which does not allocate garbage when a code is run for the first time
            // Note : don't initialize anything related with 'BXSTween' here as it is still not completely constructed
            // Do this so that the jit is generated beforehand and so that the
            // first started tween acceesing this class doesn't allocate much garbage.
            Type bxTwEaseType = typeof(BXTweenEase);
            RuntimeHelpers.RunClassConstructor(bxTwEaseType.TypeHandle);
        }
#endif

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
                    if (NeedsInitializeParameters)
                    {
                        return null;
                    }

                    Initialize(m_GetMainRunnerAction, MainLogger);
                }

                return m_MainRunner;
            }
        }
        /// <summary>
        /// Whether if the BXSTween needs it's initial <see cref="Initialize(BXSGetterAction{IBXSTweenRunner}, Logger)"/> called.
        /// <br>After calling <see cref="Initialize(BXSGetterAction{IBXSTweenRunner}, Logger)"/> once will make this false.</br>
        /// </summary>
        public static bool NeedsInitializeParameters => m_GetMainRunnerAction == null || MainLogger == null;

        /// <summary>
        /// The base logger.
        /// </summary>
        private static Logger m_MainLogger;
        /// <summary>
        /// The <see cref="BXSTween"/> logger.
        /// <br>Only to be used by BXSTween classes and the <see cref="IBXSTweenRunner"/> initializing this class.</br>
        /// </summary>
        internal static Logger MainLogger
        {
            get
            {
                // Replace with a non-null value if accessed while null
                m_MainLogger ??= new Logger(Console.WriteLine, null, null, null);

                return m_MainLogger;
            }
        }

        /// <summary>
        /// The list of all running tweens.
        /// <br>Unless absolutely necessary, there is no need to change the contents of this.</br>
        /// <br>Can use the <see cref="BXSTweenable"/> methods on tweens here.</br>
        /// </summary>
        public static readonly List<BXSTweenable> RunningTweens = new List<BXSTweenable>(50);

        /// <summary>
        /// Sets a logger.
        /// </summary>
        public static void SetLogger(Logger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger), "[BXSTween::SetLogger] Given argument was null.");
            }

            m_MainLogger = logger;
        }
        /// <summary>
        /// Initializes the <see cref="IBXSTweenRunner"/> <paramref name="runner"/> with logger <paramref name="logger"/>.
        /// </summary>
        /// <param name="getRunnerAction">Create the runner in this method. This is called when the BXSTween is uninitialized but.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Initialize(BXSGetterAction<IBXSTweenRunner> getRunnerAction, Logger logger)
        {
            if (getRunnerAction == null)
            {
                throw new ArgumentNullException(nameof(getRunnerAction), "[BXSTween::Initialize] Given parameter is null.");
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger), "[BXSTween::Initialize] Given parameter is null.");
            }

            // Call this first to call all events on the 'OnRunnerExit'
            MainRunner?.Kill();

            // Hook the tween runner
            m_MainRunner = getRunnerAction();
            HookTweenRunner(m_MainRunner);
            // Get the delegates
            m_GetMainRunnerAction = getRunnerAction;

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
                {
                    continue;
                }

                tween.Stop();
            }

            RunningTweens.Clear();
        }

        /// <summary>
        /// Clears the <see cref="BXSTween"/>, this includes only the <see cref="MainRunner"/>.
        /// </summary>
        public static void Clear()
        {
            StopAllTweens();
            MainRunner?.Kill();
            // Don't unset the logger.
        }

        // -- Tweening
        /// <summary>
        /// <b>!! TODO : </b>Optimize this method, do the checks only once?
        /// <br>Runs a tweenable.</br>
        /// <br>The <paramref name="tween"/> itself contains the state.</br>
        /// </summary>
        public static void RunTweenable(ITickRunner runner, BXSTweenable tween)
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
                    // tween.Stop already calls OnEndAction
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
            switch (tween.ActualTickType)
            {
                default:
                case TickType.Variable:
                    deltaTime = runner.UnscaledDeltaTime;
                    break;
                case TickType.Fixed:
                    deltaTime = runner.FixedUnscaledDeltaTime;
                    break;
            }
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
                {
                    tween.LoopsElapsed++;
                }

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
                {
                    tween.IsTargetValuesSwitched = !tween.IsTargetValuesSwitched;
                }

                return;
            }

            // Stop + Call endings
            tween.Stop();
        }

        #region IBXSRunner
        /// <summary>
        /// Hooks the tween runner.
        /// </summary>
        private static void HookTweenRunner(ITickRunner runner)
        {
            runner.OnExit += OnTweenRunnerExit;
            runner.OnTick += OnTweenRunnerTick;

            if (runner.SupportsFixedTick)
            {
                runner.OnFixedTick += OnTweenRunnerFixedTick;
            }
        }

        private static void OnTweenRunnerTick(ITickRunner runner)
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
        private static void OnTweenRunnerFixedTick(ITickRunner runner)
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
                m_MainLogger = null;
            }

            m_MainRunner = null;
        }
        #endregion
    }
}
