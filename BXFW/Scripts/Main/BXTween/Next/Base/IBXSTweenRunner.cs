using BXFW.Tweening.Next.Events;

namespace BXFW.Tweening.Next
{
    /// <summary>
    /// Contains a tweening runner.
    /// <br>Includes the necessary update hooks, timing variables, etc.</br>
    /// <br>If a runner no longer exists, it will be recreated when needed by <see cref="BXSTween"/>.</br>
    /// </summary>
    public interface IBXSTweenRunner
    {
        // -- Specs
        /// <summary>
        /// The amount of frames or ticks that this runner had.
        /// <br>Not counted by the IBXSRunner provider but you can do that, or pass something like 'Time.frameCount'.</br>
        /// </summary>
        public int ElapsedTickCount { get; }
        /// <summary>
        /// A unscaled delta time definition.
        /// <br>Return main thread's delta time if unsure.</br>
        /// </summary>
        public float UnscaledDeltaTime { get; }
        /// <summary>
        /// The current time scale for this runner.
        /// <br>Return 1 if unsure.</br>
        /// </summary>
        public float TimeScale { get; }

        // -- Events
        /// <summary>
        /// Should be invoked when the runner is initialized for the first time.
        /// <br>Hook into Awake or the RuntimeInitializeOnLoadMethodAttribute'd method if unsure.</br>
        /// </summary>
        public event BXSAction OnRunnerStart;
        /// <summary>
        /// A tick method, should be invoked every tick regardless of time scaling.
        /// This method should tick regardless of support and should be ticked usually from the main thread.
        /// <br><see cref="BXSTween"/> hooks into this method and unhooks from this method when the application is closed.</br>
        /// <br>Hook into Update/FixedUpdate if unsure.</br>
        /// </summary>
        public event BXSAction<IBXSTweenRunner> OnRunnerTick;
        // Maybe seperate these as IBXSTweenFixedTicker? or keep these.
        /// <summary>
        /// Whether if this runner supports <see cref="OnRunnerFixedTick"/>.
        /// </summary>
        public bool SupportsFixedTick { get; }
        /// <summary>
        /// The fixed tick rate if <see cref="SupportsFixedTick"/> is true.
        /// <br>This value should be ignored if the runner doesn't support fixed tick.</br>
        /// </summary>
        public int FixedTickRate { get; }
        /// <summary>
        /// A fixed tick method.
        /// <br>This should only be used if the <see cref="SupportsFixedTick"/> is true.</br>
        /// </summary>
        public event BXSAction<IBXSTweenRunner> OnRunnerFixedTick;
        /// <summary>
        /// Should be invoked when the runner is closed/destroyed/disposed.
        /// <br>Hook into OnApplicationQuit if unsure.</br>
        /// </summary>
        public event BXSExitAction OnRunnerExit;

        // -- Management
        /// <summary>
        /// When called should dispose the runner, marking it as non-needed.
        /// <br>Should call <see cref="OnRunnerExit"/> from here if plausible.</br>
        /// </summary>
        public void Kill();

        /// <summary>
        /// Returns a tweening id from the given object.
        /// <br>Implement this according to your id system, or always return <see cref="BXSTween.NoID"/> if no id.</br>
        /// </summary>
        public int GetObjectID<TDispatchObject>(TDispatchObject idObject)
            where TDispatchObject : class;
            
        /// <summary>
        /// Returns a tweening object from the given id.
        /// </summary>
        public TDispatchObject GetIDObject<TDispatchObject>(int id)
            where TDispatchObject : class;
    }
}
