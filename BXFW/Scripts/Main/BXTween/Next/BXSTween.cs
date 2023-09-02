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
    /// ---------
    /// <summary>
    /// A simpler tick based tweening engine.
    /// <br>This tweening engine focuses on flexibility and simplicity.</br>
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

                if (tween.TickType == TickType.Variable)
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

                if (tween.TickType == TickType.Fixed)
                {
                    RunTweenable(runner, tween);
                }
            }
        }

        /// <summary>
        /// Runs a tweenable.
        /// <br>The <paramref name="tween"/> itself contains the state.</br>
        /// </summary>
        private static void RunTweenable(IBXSTweenRunner runner, BXSTweenable tween)
        {

        }

        /// <summary>
        /// The exit method for the <see cref="MainRunner"/>.
        /// </summary>
        private static void OnTweenRunnerExit(bool applicationQuit)
        {
            // The tween is dead, set 'MainRunner' to null

            // If we are quitting app as well clear the BXSTween runnings.
            if (applicationQuit)
            {
                Clear(); 
            }
        }

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
        #endregion
    }
}