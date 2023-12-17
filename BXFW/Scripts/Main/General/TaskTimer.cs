using System;
using System.Collections.Generic;
using System.Linq;

namespace BXFW
{
    /// <summary>
    /// Schedules timed actions (basically a timer).
    /// <br>Takes argumentless delegates.</br>
    /// </summary>
    public static class TaskTimer
    {
        // The more i look into 'Schedule', the word, the more meaningless it sounds
        // But whatever, this will do.
        // ----
        // Times and manages tasks. Kinda like the 'Invoke' keyword, but it supports delegates as 'Invoke' should.

        /// <summary>
        /// A mutable state delay action.
        /// </summary>
        public class ScheduledAction : IEquatable<ScheduledAction>
        {
            /// <summary>
            /// Action to call and remove the <see cref="ScheduledAction"/> when the timer runs out.
            /// </summary>
            public readonly Action targetAction;
            /// <summary>
            /// Ticking type to check the ScheduledAction on.
            /// </summary>
            public TickType targetTickType = TickType.Variable;
            /// <summary>
            /// Timer to wait for.
            /// </summary>
            public float timer = 0f;
            /// <summary>
            /// Frames to wait for.
            /// </summary>
            public int waitFrames = -1;
            /// <summary>
            /// Whether to wait for this ScheduledAction using tick counts instead of waiting a set time.
            /// </summary>
            public bool UseWaitFrames { get; private set; }
            /// <summary>
            /// Whether to ignore the time scale while ticking.
            /// </summary>
            public bool ignoreTimeScale = false;

            /// <summary>
            /// WeakReference to the caller object.
            /// <br>If this reference is dead, the ScheduledAction may stop.</br>
            /// </summary>
            public readonly WeakReference caller;

            // Test whether if the 'caller' is actually a UnityEngine.Object
            // Because in that case, the weakref will stay alive BUT
            // the underlying Unity Object inner pointer (on a c# managed object that actually exists,
            // but the c++ object pointer is nullptr) will throw many exceptions

            /// <summary>
            /// Gets the hashcode of the caller.
            /// </summary>
            public int CallerHash => CallerExists ? caller.Target.GetHashCode() : 0;
            /// <summary>
            /// Returns whether if this ScheduledAction requires the caller to be alive.
            /// <br>This returns whether if the <see cref="caller"/> <see cref="WeakReference"/> exists.
            /// (If this <see cref="ScheduledAction"/> was constructed without a caller</br>
            /// </summary>
            public bool RequiresCallerAlive => caller != null;
            /// <summary>
            /// Returns whether if the caller actually exists and is alive.
            /// </summary>
            public bool CallerExists => (caller?.IsAlive ?? false) && !UnitySafeEqualityComparer.Default.Equals(caller.Target, null);

            public ScheduledAction(Action target, float timer, object caller, TickType tickType, bool ignoreTimeScale)
            {
                targetAction = target;
                this.timer = timer;
                if (!UnitySafeEqualityComparer.Default.Equals(caller, null))
                {
                    this.caller = new WeakReference(caller);
                }

                UseWaitFrames = false;
                targetTickType = tickType;
                this.ignoreTimeScale = ignoreTimeScale;
            }
            public ScheduledAction(Action target, object caller, int waitFrames, TickType tickType, bool ignoreTimeScale)
            {
                targetAction = target;
                this.waitFrames = waitFrames;
                if (!UnitySafeEqualityComparer.Default.Equals(caller, null))
                {
                    this.caller = new WeakReference(caller);
                }

                UseWaitFrames = true;
                targetTickType = tickType;
                this.ignoreTimeScale = ignoreTimeScale;
            }

            public bool Equals(ScheduledAction action)
            {
                if (action is null)
                {
                    return false;
                }

                return GetHashCode() == action.GetHashCode();
            }
            public bool Equals(Action action, object caller)
            {
                return GetHashCode() == HashCode.Combine(action, caller?.GetHashCode() ?? 0);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(targetAction, CallerHash);
            }
        }

        /// <summary>
        /// Current timer's runner.
        /// <br>There's no fallback for this except for <see cref="MainTickRunner"/>.</br>
        /// </summary>
        private static ITickRunner m_MainRunner;
        /// <summary>
        /// The <see cref="TaskTimer"/> runner.
        /// <br>If the <see cref="Initialize(Func{ITickRunner})"/> was called validly once by this then this value will not be null if requested.</br>
        /// <br>This controls the running operations. To set this value to something else use the <see cref="Initialize(Func{ITickRunner})"/> method.</br>
        /// </summary>
        public static ITickRunner MainRunner => m_MainRunner;

        /// <summary>
        /// Initializes the task scheduler.
        /// </summary>
        /// <param name="runner"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void Initialize(ITickRunner runner)
        {
            if (runner == null)
            {
                throw new ArgumentNullException(nameof(runner), "[Timer::Initialize] Failed to initialize timer. Given argument was null.");
            }

            // Hook into 'm_MainRunner'
            m_MainRunner = runner;
            m_MainRunner.OnTick += OnTick;
            m_MainRunner.OnFixedTick += OnFixedTick;
            m_MainRunner.OnExit += OnExit;
        }

        static TaskTimer()
        {
            // Initialize on demand (this will also initialize the 'MainTickRunner')
            Initialize(MainTickRunner.Instance);
        }

        /// <summary>
        /// List of the currently scheduled actions.
        /// </summary>
        private static readonly List<ScheduledAction> scheduledActions = new List<ScheduledAction>();

        /// <summary>
        /// Schedules an action to be called.
        /// </summary>
        /// <param name="action">Event to call when the timer runs out.</param>
        /// <param name="delay">Delay to wait out.</param>
        /// <param name="caller">The caller that calls this scheduling task to wait out <paramref name="delay"/>.</param>
        /// <param name="tickType">Tick type to wait this scheduled task as.
        /// <see cref="TickType.Fixed"/> may synchronize with the physics updating (or constant tick updating rate) of your engine but may be less accurate.
        /// </param>
        /// <param name="ignoreTimeScale">Whether to ignore a timescale provided by the <see cref="MainRunner"/>.</param>
        public static void Schedule(Action action, float delay, object caller, TickType tickType, bool ignoreTimeScale)
        {
            scheduledActions.Add(new ScheduledAction(action, delay, caller, tickType, ignoreTimeScale));
        }
        /// <summary>
        /// Queues an action to be called.
        /// </summary>
        /// <param name="action">Event to call when the timer runs out.</param>
        /// <param name="waitFrames">
        /// Amount of <paramref name="tickType"/> frames to wait out.
        /// If you are not defining a <see cref="TickType"/> this is <see cref="TickType.Variable"/> ticks.
        /// </param>
        /// <param name="caller">The caller that calls this scheduling task to wait out <paramref name="waitFrames"/>.</param>
        /// <param name="tickType">Tick type to wait out.</param>
        /// <param name="ignoreTimeScale">
        /// Whether to ignore a timescale provided by the <see cref="MainRunner"/>.
        /// <br>NOTE : For scheduling waiting frame counts, this DOES NOT get affected by the timescale until the timescale reaches zero.</br>
        /// </param>
        public static void ScheduleFrames(Action action, int waitFrames, object caller, TickType tickType, bool ignoreTimeScale)
        {
            scheduledActions.Add(new ScheduledAction(action, caller, waitFrames, tickType, ignoreTimeScale));
        }
        /// <summary>
        /// Queues an action to be called.
        /// <br>Checks the timer in <see cref="TickType.Variable"/> ticking and does not ignore timescale.</br>
        /// </summary>
        /// <inheritdoc cref="Schedule(Action, float, object, TickType, bool)"/>
        public static void Schedule(Action action, float delay, object caller)
        {
            Schedule(action, delay, caller, TickType.Variable, false);
        }
        /// <summary>
        /// Queues an action to be called.
        /// <br>Waits out given amount of frames in <see cref="TickType.Variable"/> tick.</br>
        /// </summary>
        /// <inheritdoc cref="ScheduleFrames(Action, int, object, TickType, bool)"/>
        public static void ScheduleFrames(Action action, int waitFrames, object caller)
        {
            ScheduleFrames(action, waitFrames, caller, TickType.Variable, false);
        }

        /// <summary>
        /// Returns whether if the given parameters have a queued task in some way to be called.
        /// <br>Newly created delegates (with different captures) may not return accurate values.</br>
        /// </summary>
        public static bool HasScheduledTask(Action action, object caller)
        {
            int indexOfTarget = scheduledActions.IndexOf((a) => a.Equals(action, caller));
            return indexOfTarget >= 0;
        }
        /// <summary>
        /// Returns whether if the given <paramref name="caller"/> has any tasks scheduled for it. 
        /// </summary>
        public static bool HasScheduledTask(object caller)
        {
            return scheduledActions.Any(action => action.CallerHash == (caller?.GetHashCode() ?? 0));
        }

        /// <summary>
        /// Stops a queue called object.
        /// </summary>
        /// <returns>Whether if a queue call was stopped.</returns>
        public static bool StopScheduledTask(Action action, object caller)
        {
            int indexOfTarget = scheduledActions.IndexOf((a) => a.Equals(action, caller));
            if (indexOfTarget < 0)
            {
                return false;
            }

            scheduledActions.RemoveAt(indexOfTarget);
            return true;
        }
        /// <summary>
        /// Stops all delay calls requested from <paramref name="caller"/>.
        /// </summary>
        /// <param name="caller">The caller object to stop it's all calls. If this is null, all null object calls are stopped.</param>
        /// <param name="invokeAllActions">Whether to invoke all of the scheduled actions for <paramref name="caller"/>.</param>
        public static void StopAllScheduledTasks(object caller, bool invokeAllActions)
        {
            for (int i = scheduledActions.Count - 1; i >= 0; i--)
            {
                ScheduledAction action = scheduledActions[i];
                if (action.CallerHash == (caller?.GetHashCode() ?? 0))
                {
                    if (invokeAllActions)
                    {
                        action.targetAction();
                    }

                    scheduledActions.RemoveAt(i);
                    continue;
                }
            }
        }
        /// <summary>
        /// <inheritdoc cref="StopAllScheduledTasks(object, bool)"/>
        /// <br>Does not invoke any of the task's actions scheduled for <paramref name="caller"/>.</br>
        /// </summary>
        /// <inheritdoc cref="StopAllScheduledTasks(object, bool)"/>
        public static void StopAllScheduledTasks(object caller)
        {
            StopAllScheduledTasks(caller, false);
        }
        /// <summary>
        /// Stops all of the tasks.
        /// <br>Use with caution.</br>
        /// </summary>
        public static void StopAllScheduledTasks()
        {
            scheduledActions.Clear();
        }

        private static void OnTick(ITickRunner runner)
        {
            // Iterate all queued actions
            for (int i = scheduledActions.Count - 1; i >= 0; i--)
            {
                ScheduledAction action = scheduledActions[i];
                if (TickScheduledAction(action, runner, TickType.Variable))
                {
                    scheduledActions.RemoveAt(i);
                    continue;
                }
            }
        }
        private static void OnFixedTick(ITickRunner runner)
        {
            // Iterate all queued actions
            for (int i = scheduledActions.Count - 1; i >= 0; i--)
            {
                ScheduledAction action = scheduledActions[i];
                if (TickScheduledAction(action, runner, TickType.Fixed))
                {
                    scheduledActions.RemoveAt(i);
                    continue;
                }
            }
        }
        private static void OnExit(bool isApplicationQuit)
        {
            if (isApplicationQuit)
            {
                StopAllScheduledTasks();
            }
        }
        /// <summary>
        /// Ticks a given action by it's parameters.
        /// </summary>
        /// <param name="action">Action to tick.</param>
        /// <param name="runner">Runner that ticks it.</param>
        /// <returns>Whether if this action should be removed.</returns>
        private static bool TickScheduledAction(in ScheduledAction action, ITickRunner runner, TickType type)
        {
            if (action.targetTickType != type)
            {
                return false;
            }

            if (action.RequiresCallerAlive)
            {
                // Should be removed, has a caller but it's dead.
                if (!action.CallerExists)
                {
                    return true;
                }
            }

            if (action.UseWaitFrames)
            {
                if (action.waitFrames <= 0)
                {
                    action.targetAction?.Invoke();
                    return true;
                }

                if (action.ignoreTimeScale || runner.TimeScale > 0f)
                {
                    action.waitFrames--;
                }
            }
            else
            {
                if (action.timer <= 0f)
                {
                    action.targetAction?.Invoke();
                    return true;
                }

                float deltaTime = type switch
                {
                    TickType.Fixed => runner.FixedUnscaledDeltaTime,
                    _ => runner.UnscaledDeltaTime
                };

                if (!action.ignoreTimeScale)
                {
                    deltaTime *= runner.TimeScale;
                }

                action.timer -= deltaTime;
            }

            return false;
        }
    }
}
