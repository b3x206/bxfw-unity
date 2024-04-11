using System;
using System.Collections.Generic;
using System.Linq;

namespace BXFW
{
    /// <summary>
    /// Schedules timed actions to be invoked on the main thread (basically a timer).
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
        public sealed class ScheduledAction : IEquatable<ScheduledAction>
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
            /// The hashcode of the caller.
            /// <br>Returns '0' if the <see cref="CallerExists"/> is <see langword="false"/>.</br>
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
            /// <br>This can be used as an subtitute for 'IsRunning' for this <see cref="ScheduledAction"/>.</br>
            /// </summary>
            public bool CallerExists => (caller?.IsAlive ?? false) && !UnitySafeObjectComparer.Default.Equals(caller.Target, null);

            /// <summary>
            /// A tuple used to tick the timer.
            /// </summary>
            internal sealed class CounterActionTuple
            {
                public float counter;
                public Action onTickAction;

                public CounterActionTuple()
                { }

                public CounterActionTuple(float counter, Action tickAction)
                {
                    this.counter = counter;
                    onTickAction = tickAction;
                }

                // This is a crappy workaround for KeyValuePair sucking because everything of the dictionary is read only anyways,
                // god forbid someone try to change dictionary values or have ref structs inside dictionaries as the value parameter..
                public void SetCounterValue(float v)
                {
                    counter = Math.Max(v, 0f);
                }
            }

            /// <summary>
            /// Used to get time-delta callbacks and control the values of the timed task.
            /// <br>This is used for actions appended to the 'ScheduledAction', like <see cref="OnSecondElapsed"/>.</br>
            /// </summary>
            internal readonly Dictionary<float, CounterActionTuple> timedTickActions = new Dictionary<float, CounterActionTuple>(ApproximateFloatComparer.Default);

            /// <summary>
            /// Set a callback (<paramref name="action"/>) to be invoked when the scheduled task has elapsed given milliseconds (<paramref name="seconds"/>).
            /// <br>Note : Since the <see cref="TaskTimer"/> runs everything on the unity main thread, if the 
            /// <paramref name="seconds"/> is too small given <paramref name="action"/> won't be called accurately.</br>
            /// <br/>
            /// <br>The given <paramref name="seconds"/> takes note of the currently remaining <see cref="timer"/> if it's running.
            /// Basically this means that the timing is calculated from the current timer, so that each given <see cref="timer"/> is divisible by <paramref name="seconds"/>.</br>
            /// <br/>
            /// <br>Even if <see cref="UseWaitFrames"/> as <see langword="true"/>, the time elapsed is still measured as seconds.</br>
            /// <br/>
            /// <br>If the framerate is too low (<see cref="ITickRunner.UnscaledDeltaTime"/> is too high) this won't tick or invoke accurately.</br>
            /// </summary>
            public ScheduledAction OnTimeElapsed(Action action, float seconds, EventSetMode setMode = EventSetMode.Add)
            {
                if (action == null)
                {
                    throw new ArgumentNullException(nameof(action), "[TaskTimer::ScheduledAction::OnTimeElapsed] Given argument was null.");
                }
                if (seconds <= (float.Epsilon * 8f))
                {
                    // don't cause DivideByZeroException
                    throw new ArgumentException("[TaskTimer::ScheduledAction::OnTimeElapsed] Given seconds parameter is less than or equal to zero.", nameof(seconds));
                }

                if (!timedTickActions.TryGetValue(seconds, out var values))
                {
                    // take account of currently elapsed timer.
                    values = new CounterActionTuple(UseWaitFrames ? 0f : (timer % seconds), new Action(action));
                    timedTickActions.Add(seconds, values);
                }
                else
                {
                    switch (setMode)
                    {
                        case EventSetMode.Equals:
                            values.onTickAction = action;
                            break;
                        case EventSetMode.Subtract:
                            values.onTickAction -= action;
                            break;
                        default:
                        case EventSetMode.Add:
                            values.onTickAction += action;
                            break;
                    }

                    timedTickActions[seconds] = values;
                }

                return this;
            }

            /// <summary>
            /// Set a callback (<paramref name="action"/>) to be invoked when the scheduled task has elapsed an second.
            /// </summary>
            public ScheduledAction OnSecondElapsed(Action action, EventSetMode setMode = EventSetMode.Add)
            {
                return OnTimeElapsed(action, 1f, setMode);
            }
            /// <summary>
            /// Set a callback (<paramref name="action"/>) to be invoked when the scheduled task has elapsed an minute.
            /// </summary>
            public ScheduledAction OnMinuteElapsed(Action action, EventSetMode setMode = EventSetMode.Add)
            {
                return OnTimeElapsed(action, 60f, setMode);
            }
            /// <summary>
            /// Set a callback (<paramref name="action"/>) to be invoked when the scheduled task has elapsed an hour.
            /// <br>This is not really be likely to be called, but whatever.</br>
            /// </summary>
            public ScheduledAction OnHourElapsed(Action action, EventSetMode setMode = EventSetMode.Add)
            {
                return OnTimeElapsed(action, 3600f, setMode);
            }

            /// <summary>
            /// Creates a <see cref="ScheduledAction"/>.
            /// <br>You aren't meant to call this directly, use <see cref="TaskTimer.Schedule"/> or <see cref="TaskTimer.ScheduleFrames"/> to get a value.</br>
            /// </summary>
            public ScheduledAction(Action target, float timer, object caller, TickType tickType, bool ignoreTimeScale)
            {
                targetAction = target;
                this.timer = timer;
                if (!UnitySafeObjectComparer.Default.Equals(caller, null))
                {
                    this.caller = new WeakReference(caller);
                }

                UseWaitFrames = false;
                targetTickType = tickType;
                this.ignoreTimeScale = ignoreTimeScale;
            }

            /// <summary>
            /// Creates a <see cref="ScheduledAction"/>.
            /// <br>You aren't meant to call this directly, use <see cref="TaskTimer.Schedule"/> or <see cref="TaskTimer.ScheduleFrames"/> to get a value.</br>
            /// </summary>
            public ScheduledAction(Action target, object caller, int waitFrames, TickType tickType, bool ignoreTimeScale)
            {
                targetAction = target;
                this.waitFrames = waitFrames;
                if (!UnitySafeObjectComparer.Default.Equals(caller, null))
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
                return GetHashCode() == HashCode.Combine(action, !UnitySafeObjectComparer.Default.Equals(caller, null) ? caller.GetHashCode() : 0);
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
        /// <br>This isn't required to be called unless you want your tick <paramref name="runner"/> to be different.</br>
        /// <br>This is because when you access the <see cref="TaskTimer"/> class in any way, the static constructor </br>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void Initialize(ITickRunner runner)
        {
            if (runner == null)
            {
                throw new ArgumentNullException(nameof(runner), "[Timer::Initialize] Failed to initialize timer. Given argument was null.");
            }

            // Unhook the previous runner (don't kill)
            if (m_MainRunner != null)
            {
                // If both objects are the same, don't do anything.
                if (UnitySafeObjectComparer.Default.Equals(m_MainRunner, runner))
                {
                    return;
                }

                m_MainRunner.OnTick -= OnTick;
                m_MainRunner.OnFixedTick -= OnFixedTick;
                m_MainRunner.OnExit -= OnExit;
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
        private static readonly List<ScheduledAction> scheduledActions = new List<ScheduledAction>(16);

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
        public static ScheduledAction Schedule(Action action, float delay, object caller, TickType tickType, bool ignoreTimeScale)
        {
            ScheduledAction schAction = new ScheduledAction(action, delay, caller, tickType, ignoreTimeScale);
            scheduledActions.Add(schAction);
            return schAction;
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
        public static ScheduledAction ScheduleFrames(Action action, int waitFrames, object caller, TickType tickType, bool ignoreTimeScale)
        {
            ScheduledAction schAction = new ScheduledAction(action, caller, waitFrames, tickType, ignoreTimeScale);
            scheduledActions.Add(schAction);
            return schAction;
        }
        /// <summary>
        /// Queues an action to be called.
        /// <br>Checks the timer in <see cref="TickType.Variable"/> ticking and does not ignore timescale.</br>
        /// </summary>
        /// <inheritdoc cref="Schedule(Action, float, object, TickType, bool)"/>
        public static ScheduledAction Schedule(Action action, float delay, object caller)
        {
            return Schedule(action, delay, caller, TickType.Variable, false);
        }
        /// <summary>
        /// Queues an action to be called.
        /// <br>Waits out given amount of frames in <see cref="TickType.Variable"/> tick.</br>
        /// </summary>
        /// <inheritdoc cref="ScheduleFrames(Action, int, object, TickType, bool)"/>
        public static ScheduledAction ScheduleFrames(Action action, int waitFrames, object caller)
        {
            return ScheduleFrames(action, waitFrames, caller, TickType.Variable, false);
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
        /// <br>If the <paramref name="caller"/> is <see langword="null"/>, existance of any scheduled actions without callers attached to it will return <see langword="true"/>.</br>
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
        /// Stops a scheduled <paramref name="action"/>.
        /// </summary>
        /// <param name="action">Scheduled action to cancel.</param>
        /// <returns><see langword="true"/> if the given <paramref name="action"/> was running and it was stopped.</returns>
        public static bool StopScheduledTask(ScheduledAction action)
        {
            int indexOfTarget = scheduledActions.IndexOf(action);
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
        /// <br>Use with caution, as this will stop all of the scheduled things.</br>
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

            // Tick the dict timers (if timers were appended to it)
            if (action.timedTickActions.Count > 0)
            {
                float deltaTime = type switch
                {
                    TickType.Fixed => runner.FixedUnscaledDeltaTime,
                    _ => runner.UnscaledDeltaTime
                };

                foreach (KeyValuePair<float, ScheduledAction.CounterActionTuple> pair in action.timedTickActions)
                {
                    float durationToInvoke = pair.Key;

                    ScheduledAction.CounterActionTuple tuple = pair.Value;
                    float tupleCounterTime = tuple.counter + deltaTime;

                    if (tuple.counter > durationToInvoke)
                    {
                        tuple.onTickAction();
                        tupleCounterTime = 0;
                    }

                    pair.Value.SetCounterValue(tupleCounterTime);
                }
            }

            return false;
        }
    }
}
