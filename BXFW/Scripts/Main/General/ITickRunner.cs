using System;

namespace BXFW
{
    /// <summary>
    /// Runs and supports a ticking class.
    /// <br>Each class that require a generic updating method may request any <see cref="ITickRunner"/>s.</br>
    /// <br/>
    /// <br>Basically, an interface that exposes the Unity MonoBehaviour updates to generic c#.</br>
    /// <br>The implementators call the given interface events from the implementator, which invoke the events.</br>
    /// </summary>
    public interface ITickRunner
    {
        /// <summary>
        /// An exit action for the tick runner.
        /// </summary>
        /// <param name="applicationQuit">Whether if the application is quitting.</param>
        public delegate void ExitAction(bool applicationQuit);

        /// <summary>
        /// The amount of frames or ticks that this runner had.
        /// </summary>
        public int ElapsedTickCount { get; }
        /// <summary>
        /// A unscaled delta time definition.
        /// <br>Return main thread's delta time if unsure.</br>
        /// </summary>
        public float UnscaledDeltaTime { get; }
        /// <summary>
        /// Whether if this runner supports <see cref="OnFixedTick"/>.
        /// </summary>
        public bool SupportsFixedTick { get; }
        /// <summary>
        /// The fixed tick delta time if <see cref="SupportsFixedTick"/> is true.
        /// <br>This value should be ignored if the runner doesn't support fixed tick.</br>
        /// </summary>
        public float FixedUnscaledDeltaTime { get; }
        /// <summary>
        /// The current time scale for this runner.
        /// <br>Return 1f or <c>Time.timeScale</c> (for unity) if unsure.</br>
        /// </summary>
        public float TimeScale { get; }

        // -- Events
        /// <summary>
        /// Should be invoked when the runner is initialized for the first time.
        /// <br>Hook into Awake method if unsure. (not to the RuntimeInitializeOnLoadMethodAttribute method)</br>
        /// </summary>
        public event Action OnStart;
        /// <summary>
        /// A tick method, should be invoked every tick regardless of time scaling.
        /// This method should tick regardless of support and should be ticked usually from the main thread.
        /// <br>Hook into Update/FixedUpdate if unsure.</br>
        /// </summary>
        public event Action<ITickRunner> OnTick;
        /// <summary>
        /// A fixed tick method. Called fixed times per second (FixedUpdate)
        /// <br>This should only be used/called if the <see cref="SupportsFixedTick"/> is true.</br>
        /// </summary>
        public event Action<ITickRunner> OnFixedTick;
        /// <summary>
        /// Should be invoked when the runner is closed/destroyed/disposed.
        /// <br>Hook into OnApplicationQuit+<see cref="Kill"/> if unsure.</br>
        /// </summary>
        public event ExitAction OnExit;

        // -- Management
        /// <summary>
        /// When called should stop/destroy the runner, marking it as non-needed.
        /// </summary>
        public void Kill();
    }
}
