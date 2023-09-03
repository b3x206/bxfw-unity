using System.Collections.Generic;

namespace BXFW.Tweening.Next
{
    /// BXFW.Tweening.Next roadmap
    /// A tweening engine that can be attached to most things supporting c#.
    /// (but will still require things to do because unity doesn't serialize private/protected's)
    /// 
    /// Rule #1 = NO TIMERS OR COROUTINES (timer is coroutine anyways)
    /// Only functions that update things in the <see cref="RunningTweens"/> array.
    /// Rule #2 = idk (maybe avoid stuff like virtual methods becuase vtable overhead?)
    /// 
    /// TODO : A lot of things
    /// But first to decide what things i am gonna do.
    /// Probably it is so that 'BXSTweenable' implementing context can have a lot of features and controlability and the rest will be more normal.
    /// * Error handling for run tweens
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
    /// <br>So yeah don't use this, the actual <see cref="BXTween"/> is stabler (but dumber).</br>
    /// </summary>
    public static class BXSTween
    {
        // -- Constants
        /// <summary>
        /// A <see cref="BXSTweenable.ID"/> for no id.
        /// </summary>
        public const int NoID = 0;

        // -- Runtime
        /// <summary>
        /// The <see cref="BXSTween"/> runner.
        /// <br>This controls the running operations. To set this value to something else use the </br>
        /// </summary>
        public static IBXSTweenRunner MainRunner { get; private set; }
        
        /// <summary>
        /// The actual list of all running tweens.
        /// <br>The '<see cref="IReadOnlyList{T}"/>' is only used for making 'BXSTweenable's non-assignable.</br>
        /// </summary>
        public static readonly List<BXSTweenable> RunningTweens = new List<BXSTweenable>();

        /// <summary>
        /// Hooks the tween runner.
        /// </summary>
        private static void HookTweenRunner(IBXSTweenRunner runner)
        {
            runner.OnRunnerExit += OnTweenRunnerExit;
            runner.OnRunnerTick += OnTweenRunnerTick;
            runner.OnRunnerFixedTick += OnTweenRunnerFixedTick;
        }

        /// <summary>
        /// Initializes the <see cref="IBXSTweenRunner"/> <paramref name="runner"/>.
        /// </summary>
        public static void Initialize(IBXSTweenRunner runner)
        {
            // Call this first to call all events on the 'OnRunnerExit'
            MainRunner?.Kill();

            // Set the 'MainRunner'
            MainRunner = runner;
            // Hook the tween runner
            HookTweenRunner(runner);
        }

        /// <summary>
        /// Stops all tweens and clears BXSTween.
        /// </summary>
        public static void Clear()
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

        #region IBXSRunner
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
        /// Runs a tweenable.
        /// <br>The <paramref name="tween"/> itself contains the state.</br>
        /// </summary>
        public static void RunTweenable(IBXSTweenRunner runner, BXSTweenable tween)
        {
            // Checks
            if (!tween.IsValid)
            {
                tween.Stop();
                RunningTweens.Remove(tween);
                // TODO : Debug Log here
                return;
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

            // Tickability
            if (tween.TickConditionAction != null)
            {
                TickConditionSuspendType suspendType = tween.TickConditionAction();

                switch (suspendType)
                {
                    case TickConditionSuspendType.Tick:
                        return;
                    case TickConditionSuspendType.Pause:
                        tween.Pause();
                        return;
                    case TickConditionSuspendType.Stop:
                        tween.Stop();
                        return;

                    default:
                    case TickConditionSuspendType.None:
                        break;
                }
            }

            bool isFirstRun = tween.RemainingLoops == tween.LoopCount;

            // Delay
            if (tween.DelayElapsed < 1f)
            {
                // Instant finish + wait one frame
                if (!tween.IsDelayed)
                {
                    tween.DelayElapsed = 1f;
                }

                // Elapse delay further
                tween.DelayElapsed += deltaTime / tween.Delay;

                if (tween.DelayElapsed >= 1f && isFirstRun)
                {
                    tween.OnStartAction?.Invoke();
                }
                return;
            }

            // Tweening + Elapsing
            if (tween.CurrentElapsed < 1f)
            {
                tween.EvaluateTween(tween.CurrentElapsed);
                tween.OnTickAction?.Invoke();
                tween.CurrentElapsed += deltaTime / tween.Duration;

                return;
            }

            // Looping
            if (tween.RemainingLoops != 0)
            {
                if (tween.RemainingLoops > 0)
                    tween.RemainingLoops--;

                tween.OnRepeatAction?.Invoke();
                tween.Reset();
                return;
            }

            tween.Stop();
        }

        /// <summary>
        /// The exit method for the <see cref="MainRunner"/>.
        /// </summary>
        private static void OnTweenRunnerExit(bool applicationQuit)
        {
            // If we are quitting app as well clear the BXSTween runnings.
            if (applicationQuit)
            {
                Clear(); 
            }

            MainRunner = null;
        }
        #endregion
    }
}