using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using BXFW.Collections;
using BXFW.Tweening.Events;

namespace BXFW.Tweening
{
    /// <summary>
    /// A sequencer of <see cref="BXSTweenable"/>'s.
    /// <br>The setters in this class does not set any values in <see cref="m_RunnableTweens"/>.</br>
    /// <br>While this is editable in the unity editor, you cannot add sequences to it. Only change the delay length and loop count.</br>
    /// </summary>
    [Serializable]
    public sealed class BXSTweenSequence : BXSTweenable, ICollection<BXSTweenable>, IEnumerable<KeyValuePair<int, BXSTweenable>>
    {
        /// <summary>
        /// A <see cref="BXSTweenable"/> container that has a priority for sequencing.
        /// </summary>
        public class RunnableTween : IEquatable<RunnableTween>, IComparable<RunnableTween>
        {
            public int priority;
            // Unity can't serialize abstract c# objects, only ScriptableObjects (which is just fancy UnityEngine.Object)
            public BXSTweenable tween;

            public RunnableTween(int order, BXSTweenable tweenable)
            {
                priority = order;
                tween = tweenable;
            }

            public int CompareTo(RunnableTween other)
            {
                if (other == null)
                {
                    return 1;
                }

                return priority.CompareTo(other.priority);
            }

            public bool Equals(RunnableTween other)
            {
                if (other == null)
                {
                    return false;
                }

                return priority == other.priority && tween == other.tween;
            }
        }

        private SortedList<RunnableTween> m_RunnableTweens = new SortedList<RunnableTween>();

        /// <summary>
        /// Current ID that is being run.
        /// </summary>
        public int CurrentRunPriority { get; private set; } = -1;
        /// <summary>
        /// The duration elapsed by the longest tweens run.
        /// </summary>
        public float TweensTotalElapsed { get; private set; } = -1f;
        /// <summary>
        /// The <see cref="CurrentRunPriority"/>'s longest duration.
        /// </summary>
        public float CurrentPriorityTweensDuration { get; private set; } = -1f;

        /// <summary>
        /// Last <see cref="RunnableTweenContext.priority"/> in the list <see cref="m_RunnableTweens"/>.
        /// <br>Returns -1 as the start id if no tweens exist. This will be the last group id used.</br>
        /// </summary>
        public int LastPriority
        {
            get
            {
                if (m_RunnableTweens.Count > 0)
                {
                    RunnableTween lastContext = m_RunnableTweens[m_RunnableTweens.Count - 1];
                    return lastContext.priority;
                }

                return -1;
            }
        }
        /// <summary>
        /// The current priority to go into if another tween was to be <see cref="Append(BXSTweenable)"/>ed.
        /// </summary>
        public int NextPriority => LastPriority + 1;
        /// <summary>
        /// The sequence duration.
        /// <br>This depends on the added tweens, if no tweens this will be zero.</br>
        /// <br>This calculates all tweens duration + delay.</br>
        /// </summary>
        public override float Duration
        {
            get
            {
                float totalDuration = 0f;
                for (int i = 0; i < NextPriority; i++)
                {
                    totalDuration += PriorityDuration(i);
                }

                return totalDuration;
            }
        }

        /// <summary>
        /// Creates a blank sequence.
        /// </summary>
        public BXSTweenSequence() { }
        /// <summary>
        /// Reserves a capacity for the tweens.
        /// </summary>
        public BXSTweenSequence(int capacity)
        {
            m_RunnableTweens.Capacity = capacity;
        }
        /// <summary>
        /// Adds the enumerable tweens to be run sequentially.
        /// </summary>
        public BXSTweenSequence(IEnumerable<BXSTweenable> tweens)
        {
            int i = 0;
            foreach (BXSTweenable ctx in tweens)
            {
                m_RunnableTweens.Add(new RunnableTween(i, ctx));
                i++;
            }
        }

        public override bool IsValid => m_RunnableTweens.Count > 0;
        public override bool IsSequence => true;

        /// <summary>
        /// Starts running the sequence.
        /// <br>Restarts running if the <see cref="IsRunning"/> is true.</br>
        /// </summary>
        public override void Play()
        {
            // No play if no tween
            if (m_RunnableTweens.Count <= 0)
            {
                throw new NullReferenceException("[BXSTweenSequence::Run] The sequence has no runnable tweens!");
            }

            // Always set
            m_LoopType = LoopType.Reset;

            // Play
            base.Play();

            // Reset after calling first play
            // (Play does not call 'Reset' if not playing tweens,
            // which with sequences Reset always has to be called)
            Reset();
        }
        /// <summary>
        /// Stops the sequence. (if running)
        /// </summary>
        public override void Stop()
        {
            // Stop resets this, but do the stopping of children tweens after stopping this sequence first
            // The previous impl was faulty both that it didn't cache the 'TotalElapsed' and it did the comparison in reverse
            // Causing arbitrary 'OnEndAction' calls even though the child tween was meant to be stopped.
            float previousTotalElapsed = TotalElapsed;
            base.Stop();

            // Stop everything (note : Only not do this if the elapsed is 1f, which means all tweens are finished)
            // This is because the 'OnEndAction' does not get called for the child BXSTweenables
            for (int i = 0; i < m_RunnableTweens.Count && (previousTotalElapsed < 1f - float.Epsilon); i++)
            {
                m_RunnableTweens[i].tween.Stop();
            }
        }
        /// <summary>
        /// Resets the sequence variables.
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            // Only set this to very initial value (because it denotes the initial run)
            CurrentRunPriority = -1;
            // These two has to be set according to the first tween sequence state
            TweensTotalElapsed = 0f;
            CurrentPriorityTweensDuration = PriorityDuration(0);
        }

        /// <summary>
        /// Runs the sequence priorities sequentially if the necessary duration was elapsed.
        /// </summary>
        public override void EvaluateTween(float t)
        {
            // Initial run
            if (CurrentRunPriority <= -1)
            {
                CurrentRunPriority = 0;
                RunTweensInPriority(CurrentRunPriority);

                return;
            }

            // Get the current possible run priority
            float totalDurationElapsed = t * Duration;
            float currentTweenDurationElapsed = totalDurationElapsed - TweensTotalElapsed;

            // Increment to the next group if the duration elapsed was larger.
            if (currentTweenDurationElapsed > CurrentPriorityTweensDuration)
            {
                TweensTotalElapsed += CurrentPriorityTweensDuration; // Increment total elapsed from prev
                CurrentRunPriority++;
                CurrentPriorityTweensDuration = PriorityDuration(CurrentRunPriority);

                RunTweensInPriority(CurrentRunPriority);
            }
        }
        /// <summary>
        /// Does nothing in the case of a sequence.
        /// </summary>
        protected override void OnSwitchTargetValues() { }
        /// <summary>
        /// Copies the sequence runnable tweens + settings.
        /// </summary>
        public override void CopyFrom<T>(T tweenable)
        {
            base.CopyFrom(tweenable);

            BXSTweenSequence tweenableSequence = tweenable as BXSTweenSequence;
            if (tweenableSequence == null)
            {
                return;
            }

            m_RunnableTweens = new SortedList<RunnableTween>(tweenableSequence.m_RunnableTweens);
        }

        // -- List Management
        /// <summary>
        /// Returns a tween in given index.
        /// <br>The priority is not contained with this tween.</br>
        /// </summary>
        public BXSTweenable this[int index]
        {
            get { return m_RunnableTweens[index].tween; }
            set
            {
                if (value == null)
                {
                    throw new NullReferenceException(string.Format("[BXTweenSequence::(set)this[{0}]] Given value for index '{0}' is null.", index));
                }

                m_RunnableTweens[index].tween = value;
            }
        }
        /// <summary>
        /// Count of tweenables to run in this sequence.
        /// </summary>
        public int Count => m_RunnableTweens.Count;
        /// <summary>
        /// Is false.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Joins a tween to current id.
        /// <br>Joined tweens will be run at the same time.</br>
        /// </summary>
        public void Join(BXSTweenable tween)
        {
            if (tween == null)
            {
                throw new ArgumentNullException(nameof(tween), "[BXSTweenSequence::Join] Given tweenable parameter is null.");
            }

            if (tween.IsPlaying)
            {
                tween.Stop();
            }

            if (LastPriority <= -1)
            {
                BXSTween.MainLogger.LogWarning($"[BXSTweenSequence::Join] Tried to join context '{tween}' without any tweens appended. Appending first tween.");
                Append(tween);
                return;
            }

            tween.ParentTweenable = this;
            m_RunnableTweens.Add(new RunnableTween(LastPriority, tween));
            // Last priority is invalid
            m_priorityDurationCache.Remove(LastPriority);
        }
        /// <summary>
        /// Joins a tween to the start.
        /// <br>Priority will be zero for the given <paramref name="ctx"/>.</br>
        /// </summary>
        public void JoinFirst(BXSTweenable ctx)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx), "[BXSTweenSequence::JoinFirst] Given context parameter is null.");
            }

            if (ctx.IsPlaying)
            {
                ctx.Stop();
            }

            m_RunnableTweens.Add(new RunnableTween(0, ctx));
            // First priority is invalid
            m_priorityDurationCache.Remove(0);
        }
        /// <summary>
        /// Appends a tween with new priority.
        /// <br>Appended tween contexts will be waited sequentially.</br>
        /// </summary>
        public void Append(BXSTweenable ctx)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx), "[BXSTweenSequence::Append] Given context parameter is null.");
            }

            if (ctx.IsPlaying)
            {
                ctx.Stop();
            }

            m_RunnableTweens.Add(new RunnableTween(NextPriority, ctx));
            // No cache modifications required, it will be calculated when needed
        }
        /// <summary>
        /// Same as calling <see cref="Append(BXSTweenable)"/>, but for the <see cref="ICollection{T}"/>.
        /// </summary>
        public void Add(BXSTweenable item)
        {
            Append(item);
        }
        /// <summary>
        /// Prepends the tween to the start (priority = 0).
        /// <br>Shifts all runnable added tweens priorities by 1.</br>
        /// </summary>
        public void Prepend(BXSTweenable ctx)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx), "[BXTweenSequence::Prepend] Given context parameter is null.");
            }

            if (ctx.IsPlaying)
            {
                ctx.Stop();
            }

            foreach (RunnableTween runnable in m_RunnableTweens)
            {
                runnable.priority += 1;
            }
            // Sort manually becuase IComparable changes doesn't notify the 'm_RunnableTweens'
            m_RunnableTweens.Sort();

            m_RunnableTweens.Add(new RunnableTween(0, ctx));

            // Cache is invalid and i won't bother shifting all keys by 1
            // Becuase c# dictionary we can't do cool unsafe stuff
            m_priorityDurationCache.Clear();
        }
        /// <summary>
        /// Clears this sequence.
        /// <br>When an empty sequence is played, it throws a <see cref="NullReferenceException"/>.</br>
        /// </summary>
        public void Clear()
        {
            m_RunnableTweens.Clear();
            m_priorityDurationCache.Clear();
        }
        /// <summary>
        /// Whether if this sequence contains given <paramref name="item"/>.
        /// </summary>
        public bool Contains(BXSTweenable item)
        {
            return m_RunnableTweens.Any(runnable => runnable.tween == item);
        }
        /// <summary>
        /// Copies internal tweenables array to <paramref name="array"/>.
        /// <br>The <paramref name="arrayIndex"/> is used for defining the starting index of <paramref name="array"/> to start the copying from.</br>
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IndexOutOfRangeException"/>
        public void CopyTo(BXSTweenable[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0 || arrayIndex >= array.Length)
            {
                throw new IndexOutOfRangeException($"[BXSTweenSequence::CopyTo] Given 'arrayIndex' is out of range. arrayIndex={arrayIndex}");
            }

            if (m_RunnableTweens.Count + arrayIndex > array.Length)
            {
                throw new ArgumentException($"[BXSTweenSequence::CopyTo] Source array was not long enough. array.Length={array.Length}, arrayIndex={arrayIndex}", nameof(array));
            }

            for (int i = arrayIndex; i < m_RunnableTweens.Count; i++)
            {
                // Copy
                array[i] = m_RunnableTweens[i].tween;
            }
        }
        /// <summary>
        /// Removes the tweenable.
        /// <br>NOTE : This may remove a whole priority and shift other tweens if there's no tweens existing in that priority.</br>
        /// </summary>
        public bool Remove(BXSTweenable item)
        {
            bool result = false;

            for (int i = m_RunnableTweens.Count - 1; i >= 0; i--)
            {
                RunnableTween runnable = m_RunnableTweens[i];
                if (runnable.tween == item)
                {
                    // Check if this is the last element of it's priority
                    if (runnable.priority != LastPriority && PriorityCount(runnable.priority) - 1 <= 0)
                    {
                        // The priority no longer exists, do decrement everything beyond this runnable's priority
                        foreach (RunnableTween afterPriorityRunnable in GetAfterPriorityRunnables(runnable.priority))
                        {
                            afterPriorityRunnable.priority -= 1;
                        }
                        // Invalidate durations completely
                        m_priorityDurationCache.Clear();
                    }

                    m_RunnableTweens.RemoveAt(i);
                    result = true;
                }
            }

            return result;
        }
        /// <summary>
        /// Removes a whole priority from the sequence.
        /// <br/>
        /// <br><see cref="IndexOutOfRangeException"/> => Thrown when the given <paramref name="priority"/> is less than 0 or more than <see cref="LastPriority"/>.</br>
        /// </summary>
        /// <param name="priority">Priority of the tweens list to remove.</param>
        /// <exception cref="IndexOutOfRangeException"/>
        public void RemovePriority(int priority)
        {
            if (priority < 0 || priority > LastPriority)
            {
                throw new IndexOutOfRangeException($"[BXSTweenSequence::RemovePriority] Failed to remove priority '{priority}' : Index was out of range.");
            }

            bool removedAny = m_RunnableTweens.RemoveAll(runnable => runnable.priority == priority) > 0;
            if (!removedAny)
            {
                return;
            }

            // Invalidate durations
            m_priorityDurationCache.Clear();

            // Check if the last priority
            if (priority == LastPriority)
            {
                return;
            }

            // Shift runnables (no tweens exist in given priority now)
            foreach (RunnableTween afterPriorityRunnable in GetAfterPriorityRunnables(priority))
            {
                afterPriorityRunnable.priority -= 1;
            }
        }
        /// <summary>
        /// Count of tweens in given priority.
        /// </summary>
        public int PriorityCount(int priority)
        {
            return m_RunnableTweens.Count(rt => rt.priority == priority);
        }
        // Cache the results of Durations
        // Only do clear the cache if a new tween was added or removed
        private readonly Dictionary<int, float> m_priorityDurationCache = new Dictionary<int, float>();
        /// <summary>
        /// Returns the longest duration tween in given <paramref name="priority"/>.
        /// <br>Returns -1f if there's no items in <paramref name="priority"/>.</br>
        /// </summary>
        public float PriorityDuration(int priority)
        {
            if (m_priorityDurationCache.TryGetValue(priority, out float cachedDuration))
            {
                return cachedDuration;
            }

            float longestDuration = -1f;
            for (int i = 0; i < m_RunnableTweens.Count; i++)
            {
                RunnableTween runnable = m_RunnableTweens[i];
                if (runnable.priority != priority)
                {
                    continue;
                }

                // LoopCount = 0 and 1 = play once, beyond that is play multiple times
                // Infinite loops will be only waited out once per their duration and it won't be stopped.
                float duration = (runnable.tween.Duration * Math.Max(runnable.tween.LoopCount + 1, 1)) + runnable.tween.Delay;
                if (duration > longestDuration)
                {
                    longestDuration = duration;
                }
            }

            m_priorityDurationCache.Add(priority, longestDuration);
            return longestDuration;
        }
        /// <summary>
        /// Returns the priority of given <paramref name="item"/>.
        /// <br>Returns <c>-1</c> if the item does not exist in the sequence.</br>
        /// </summary>
        public int PriorityOf(BXSTweenable item)
        {
            RunnableTween r = m_RunnableTweens.Where(x => x.tween == item).FirstOrDefault();
            if (r == null)
            {
                return -1;
            }

            return r.priority;
        }

        /// <summary>
        /// Gets the runnable tweens from <see cref="m_RunnableTweens"/> of priority after given <paramref name="priority"/>.
        /// </summary>
        private IEnumerable<RunnableTween> GetAfterPriorityRunnables(int priority)
        {
            foreach (RunnableTween runnable in m_RunnableTweens)
            {
                if (runnable.priority > priority)
                {
                    yield return runnable;
                }
            }
        }
        /// <summary>
        /// Runs all tweens in given <paramref name="priority"/>.
        /// </summary>
        private void RunTweensInPriority(int priority)
        {
            for (int i = 0; i < m_RunnableTweens.Count; i++)
            {
                RunnableTween runnable = m_RunnableTweens[i];
                if (runnable.priority == priority)
                {
                    runnable.tween.Play();
                }
            }
        }

        /// <summary>
        /// The iterator that returns the tweens contained.
        /// </summary>
        public IEnumerator<BXSTweenable> GetEnumerator()
        {
            for (int i = 0; i < m_RunnableTweens.Count; i++)
            {
                yield return m_RunnableTweens[i].tween;
            }
        }
        /// <summary>
        /// The iterator that also returns the priority.
        /// </summary>
        IEnumerator<KeyValuePair<int, BXSTweenable>> IEnumerable<KeyValuePair<int, BXSTweenable>>.GetEnumerator()
        {
            for (int i = 0; i < m_RunnableTweens.Count; i++)
            {
                yield return new KeyValuePair<int, BXSTweenable>(m_RunnableTweens[i].priority, m_RunnableTweens[i].tween);
            }
        }
        /// <summary>
        /// Base 'IEnumerable'.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // -- Value Setters
        // Hmm, idk how to solve the value setting for the all tweens.
        // But for the time being, just keep your BXSTweenable references with yourself.
        // - These setters could be used for the Sequence values itself
        /// <summary>
        /// Sets the sequence delay.
        /// <br>This only effects this sequence and not the attached tweens.</br>
        /// </summary>
        public BXSTweenSequence SetDelay(float delay)
        {
            m_Delay = delay;

            return this;
        }
        /// <summary>
        /// Sets the loop count of this sequence.
        /// </summary>
        public BXSTweenSequence SetLoopCount(int loops)
        {
            m_LoopCount = loops;

            return this;
        }
        /// <summary>
        /// Sets whether to ignore time scale through this sequence.
        /// <br>This only effects this sequence and not the attached tweens.</br>
        /// </summary>
        public BXSTweenSequence SetIgnoreTimeScale(bool doIgnore)
        {
            m_IgnoreTimeScale = doIgnore;

            return this;
        }

        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnPlayAction"/> event.
        /// <br>This is called when <see cref="BXSTweenable.Play"/> is called on this tween.</br>
        /// </summary>
        public BXSTweenSequence SetPlayAction(BXSAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            switch (setMode)
            {
                case EventSetMode.Subtract:
                    OnPlayAction -= action;
                    break;
                case EventSetMode.Add:
                    OnPlayAction += action;
                    break;

                default:
                case EventSetMode.Equals:
                    OnPlayAction = action;
                    break;
            }
            return this;
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnStartAction"/> event.
        /// <br>This is called when the tween has waited out it's delay and it is starting for the first time.</br>
        /// </summary>
        public BXSTweenSequence SetStartAction(BXSAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            switch (setMode)
            {
                case EventSetMode.Subtract:
                    OnStartAction -= action;
                    break;
                case EventSetMode.Add:
                    OnStartAction += action;
                    break;

                default:
                case EventSetMode.Equals:
                    OnStartAction = action;
                    break;
            }
            return this;
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnTickAction"/> event.
        /// <br>This is called every time the tween ticks. It is started to be called after the delay was waited out.</br>
        /// </summary>
        public BXSTweenSequence SetTickAction(BXSAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            switch (setMode)
            {
                case EventSetMode.Subtract:
                    OnTickAction -= action;
                    break;
                case EventSetMode.Add:
                    OnTickAction += action;
                    break;

                default:
                case EventSetMode.Equals:
                    OnTickAction = action;
                    break;
            }
            return this;
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnTickAction"/> event.<br/>
        /// This method is an alias for <see cref="SetTickAction(BXSAction, EventSetMode)"/>.
        /// </summary>
        public BXSTweenSequence SetUpdateAction(BXSAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            return SetTickAction(action, setMode);
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnPauseAction"/> event.
        /// <br>It is called when <see cref="BXSTweenable.Pause"/> is called on this tween.</br>
        /// </summary>
        public BXSTweenSequence SetPauseAction(BXSAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            switch (setMode)
            {
                case EventSetMode.Subtract:
                    OnPauseAction -= action;
                    break;
                case EventSetMode.Add:
                    OnPauseAction += action;
                    break;

                default:
                case EventSetMode.Equals:
                    OnPauseAction = action;
                    break;
            }
            return this;
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnRepeatAction"/> event.
        /// </summary>
        public BXSTweenSequence SetRepeatAction(BXSAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            switch (setMode)
            {
                case EventSetMode.Subtract:
                    OnRepeatAction -= action;
                    break;
                case EventSetMode.Add:
                    OnRepeatAction += action;
                    break;

                default:
                case EventSetMode.Equals:
                    OnRepeatAction = action;
                    break;
            }
            return this;
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnEndAction"/> event.
        /// <br>The difference between the <see cref="SetStopAction(BXSAction, EventSetMode)"/> 
        /// and this is that this only gets invoked when the tween ends after the tweens duration.</br>
        /// <br>This does not set any of the added Tweenables actions.</br>
        /// </summary>
        public BXSTweenSequence SetEndAction(BXSAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            switch (setMode)
            {
                case EventSetMode.Subtract:
                    OnEndAction -= action;
                    break;
                case EventSetMode.Add:
                    OnEndAction += action;
                    break;

                default:
                case EventSetMode.Equals:
                    OnEndAction = action;
                    break;
            }
            return this;
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.OnStopAction"/> event.
        /// <br>The difference between the <see cref="SetEndAction(BXSAction, EventSetMode)"/>
        /// and this is that this gets called both when the tween ends or when <see cref="BXSTweenable.Stop"/> gets called.</br>
        /// <br>This does not set any of the added Tweenables actions.</br>
        /// </summary>
        public BXSTweenSequence SetStopAction(BXSAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            switch (setMode)
            {
                case EventSetMode.Subtract:
                    OnStopAction -= action;
                    break;
                case EventSetMode.Add:
                    OnStopAction += action;
                    break;

                default:
                case EventSetMode.Equals:
                    OnStopAction = action;
                    break;
            }
            return this;
        }
        /// <summary>
        /// Sets the <see cref="BXSTweenable.TickConditionAction"/> action.
        /// <br>Return the suitable <see cref="TickSuspendType"/> in the function.</br>
        /// </summary> 
        public BXSTweenSequence SetTickConditionAction(BXSTickConditionAction action, EventSetMode setMode = EventSetMode.Equals)
        {
            switch (setMode)
            {
                case EventSetMode.Subtract:
                    TickConditionAction -= action;
                    break;
                case EventSetMode.Add:
                    TickConditionAction += action;
                    break;

                default:
                case EventSetMode.Equals:
                    TickConditionAction = action;
                    break;
            }
            return this;
        }
    }
}
