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
        // TODO : Add more parameter options
        // TODO 2 : Ensure that the unity's stupid fake null Objects are handled
        // Maybe create a UnitySafeEqualityComparer thing that type checks the object to UnityEngine.Object?
        // Because of this, the weakref will always be alive if a UnityEngine.Object but GetHashCode will throw.

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
            public readonly TickType actionTickType;
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
            /// WeakReference to the caller object.
            /// <br>If this reference is dead, the ScheduledAction may stop.</br>
            /// </summary>
            public readonly WeakReference caller;
            /// <summary>
            /// Gets the hashcode of the caller.
            /// </summary>
            public int CallerHash => (caller?.IsAlive ?? false) ? caller.Target.GetHashCode() : 0;

            // TODO
            public bool ignoreTimeScale = false;
            public TickType targetTick = TickType.Variable;

            public ScheduledAction(Action target, float timer, object caller)
            {
                targetAction = target;
                this.timer = timer;
                if (caller != null)
                {
                    this.caller = new WeakReference(caller);
                }

                UseWaitFrames = false;
            }
            public ScheduledAction(Action target, object caller, int waitFrames)
            {
                targetAction = target;
                this.waitFrames = waitFrames;
                if (caller != null)
                {
                    this.caller = new WeakReference(caller);
                }

                UseWaitFrames = true;
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
        }

        static TaskTimer()
        {
            // Iniitalize on demand (this will also initialize the 'MainTickRunner')
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
        public static void Schedule(Action action, float delay, object caller)
        {
            scheduledActions.Add(new ScheduledAction(action, delay, caller));
        }
        /// <summary>
        /// Queues an action to be called.
        /// <br>Waits out given amount of frames in <see cref="TickType.Variable"/> tick.</br>
        /// </summary>
        /// <param name="action">Event to call when the timer runs out.</param>
        /// <param name="waitFrames">Amount of <see cref="TickType.Variable"/> frames to wait out.</param>
        public static void ScheduleFrames(Action action, int waitFrames, object caller)
        {
            scheduledActions.Add(new ScheduledAction(action, caller, waitFrames));
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
            for (int i = scheduledActions.Count; i >= 0; i--)
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
        /// <summary>
        /// Ticks a given action by it's parameters.
        /// </summary>
        /// <param name="action">Action to tick.</param>
        /// <param name="runner">Runner that ticks it.</param>
        /// <returns>Whether if this action should be removed.</returns>
        private static bool TickScheduledAction(in ScheduledAction action, ITickRunner runner, TickType type)
        {
            if (action.targetTick != type)
            {
                return false;
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
