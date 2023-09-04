using System;
using System.Linq;
using System.Collections;
using BXFW.Tweening.Events;
using System.Collections.Generic;
using UnityEngine;
using BXFW.Tweening.Next.Events;

namespace BXFW.Tweening.Next
{
    /// <summary>
    /// A sequencer of <see cref="BXSTweenable"/>'s.
    /// <br>The setters in this class sets ALL values in <see cref="m_RunnableTweens"/> list that this sequencer has.</br>
    /// </summary>
    public sealed class BXSTweenSequence : BXSTweenable, ICollection<BXSTweenable>
    {
        /// <summary>
        /// A <see cref="BXSTweenable"/> container that has a priority.
        /// </summary>
        public class RunnableTween : IEquatable<RunnableTween>, IComparable<RunnableTween>
        {
            public int priority;
            public BXSTweenable tween;

            public RunnableTween(int order, BXSTweenable tweenable)
            {
                priority = order;
                tween = tweenable;
            }

            public int CompareTo(RunnableTween other)
            {
                if (other == null)
                    return 1;

                return priority.CompareTo(other.priority);
            }

            public bool Equals(RunnableTween other)
            {
                if (other == null)
                    return false;

                return priority == other.priority && tween == other.tween;
            }
        }

        [SerializeField]
        private SortedList<RunnableTween> m_RunnableTweens = new SortedList<RunnableTween>();

        public override void EvaluateTween(float t)
        {
            throw new NotImplementedException();
        }

        protected override void OnSwitchTargetValues()
        {
            foreach (var runnable in m_RunnableTweens)
            {
                runnable.tween.IsTargetValuesSwitched = IsTargetValuesSwitched;
            }
        }

        /// <summary>
        /// Current ID that is being run.
        /// </summary>
        private int m_currentRunPriority = 0;

        /// <summary>
        /// Called on the end of sequence.
        /// </summary>
        public BXTweenMethod OnSequenceEnd;
        /// <summary>
        /// Called when a tween ends in the sequence.
        /// <br>Index priority is given with 'Key'.</br>
        /// </summary>
        public BXSAction<KeyValuePair<int, BXSTweenable>> OnIndiviualTweenEnd;
        /// <summary>
        /// Last <see cref="RunnableTweenContext.priority"/> in the list <see cref="m_RunnableTweens"/>.
        /// <br>Returns -1 as the start id if no tweens exist. This will be the last group id used.</br>
        /// </summary>
        public int LastPriority
        {
            get
            {
                RunnableTween lastContext = m_RunnableTweens.LastOrDefault();
                return lastContext != null ? lastContext.priority : -1;
            }
        }
        /// <summary>
        /// The sequence duration.
        /// <br>This depends on the added tweens, if no tweens this will be zero.</br>
        /// <br>This calculates all tweens duration + delay.</br>
        /// </summary>
        public override float Duration
        {
            get
            {
                float duration = 0f;
                int checkPriority = 0;
                float maxDuration = 0f;
                for (int i = 0; i < m_RunnableTweens.Count; i++)
                {
                    var runnable = m_RunnableTweens[i];

                    // ID's are same
                    if (runnable.priority == checkPriority)
                    {
                        // Get duration
                        float tweenDuration = runnable.tween.Duration + runnable.tween.Delay;
                        // Check if longest in current 'checkID'
                        if (tweenDuration > maxDuration)
                        {
                            maxDuration = tweenDuration;
                        }
                    }
                    else
                    {
                        // ID's different, changed id.
                        duration += maxDuration;
                        maxDuration = 0f;
                    }

                    checkPriority = runnable.priority;
                    // Last run, add 'maxDuration' if not added
                    if (i == m_RunnableTweens.Count - 1)
                    {
                        duration += maxDuration;
                    }
                }

                return duration;
            }
        }

        /// <summary>
        /// Joins a tween to current id.
        /// <br>Joined tweens will be run at the same time.</br>
        /// </summary>
        public void Join(BXSTweenable tween)
        {
            if (tween == null)
                throw new ArgumentNullException(nameof(tween), "[BXTweenSequence::Join] Given context parameter is null.");

            if (tween.IsPlaying)
                tween.Stop();

            if (LastPriority <= -1)
            {
                Debug.LogWarning(string.Format("[BXSTweenSequence::Join] Tried to join context {0} without any tweens appended. Appending first tween.", tween));
                Append(tween);
                return;
            }

            m_RunnableTweens.Add(new RunnableTween(LastPriority, tween));
        }
        /// <summary>
        /// Joins a tween to the start.
        /// <br>Priority will be zero for the given <paramref name="ctx"/>.</br>
        /// </summary>
        public void JoinFirst(BXSTweenable ctx)
        {
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx), "[BXSTweenSequence::JoinFirst] Given context parameter is null.");

            if (ctx.IsPlaying)
                ctx.Stop();

            m_RunnableTweens.Add(new RunnableTween(0, ctx));
        }
        /// <summary>
        /// Appends a tween with new id.
        /// <br>Appended tween contexts will be waited sequentially.</br>
        /// </summary>
        public void Append(BXSTweenable ctx)
        {
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx), "[BXSTweenSequence::Append] Given context parameter is null.");

            if (ctx.IsPlaying)
                ctx.Stop();

            m_RunnableTweens.Add(new RunnableTween(LastPriority + 1, ctx));
        }
        /// <summary>
        /// Prepends the tween to the start (id = 0).
        /// <br>Shifts all runnable added tweens by 1.</br>
        /// </summary>
        public void Prepend(BXSTweenable ctx)
        {
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx), "[BXTweenSequence::Prepend] Given context parameter is null.");

            if (ctx.IsPlaying)
                ctx.Stop();

            foreach (var runnable in m_RunnableTweens)
            {
                runnable.priority += 1;
            }
            // Sort manually becuase IComparable changes doesn't notify the 'm_RunnableTweens'
            m_RunnableTweens.Sort();

            m_RunnableTweens.Append(new RunnableTween(0, ctx));
        }

        /// <summary>
        /// Starts running the sequence.
        /// <br>Restarts running if the <see cref="IsRunning"/> is true.</br>
        /// </summary>
        public override void Play()
        {
            if (m_RunnableTweens.Count <= 0)
            {
                throw new NullReferenceException("[BXTweenSequence::Run] The sequence has no runnable tweens!");
            }

            base.Play();

            m_currentRunPriority = 0;
            RunRecursive();
        }
        /// <summary>
        /// Runs the contexts recursively.
        /// <br>Uses the <see cref="m_currentRunPriority"/> to keep track of id.</br>
        /// </summary>
        private void RunRecursive()
        {
            BXSTweenable longestDurationCtx = null;
            foreach (var runnable in m_RunnableTweens.Where(rt => rt.priority == m_currentRunPriority))
            {
                float longestDuration = longestDurationCtx != null ?
                    (longestDurationCtx.Duration + longestDurationCtx.Delay) : float.NegativeInfinity;

                if ((runnable.tween.Duration + runnable.tween.Delay) > longestDuration)
                    longestDurationCtx = runnable.tween;

                runnable.tween.Play();
                runnable.tween.OnEndAction += () =>
                {
                    OnIndiviualTweenEnd?.Invoke(new KeyValuePair<int, BXSTweenable>(runnable.priority, runnable.tween));
                };
            }

            if (longestDurationCtx == null)
                throw new NullReferenceException(string.Format("[BXTweenSequence::RunRecursive] Sequence id={0} does not have any tweens on it.", m_currentRunPriority));

            longestDurationCtx.OnEndAction += () =>
            {
                if (m_currentRunPriority <= LastPriority)
                {
                    m_currentRunPriority++;
                    RunRecursive();
                }
                else
                {
                    Stop();
                }
            };
        }
        /// <summary>
        /// Stops the sequence. (if running)
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            m_currentRunPriority = 0;
            foreach (var runnable in m_RunnableTweens)
            {
                runnable.tween.ClearEndAction();
                runnable.tween.Stop();
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
            foreach (var ctx in tweens)
            {
                m_RunnableTweens.Add(new RunnableTween(i, ctx));

                i++;
            }
        }

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
                    throw new NullReferenceException(string.Format("[BXTweenSequence::(set)this[{0}]] Given value for index '{0}' is null.", index));

                m_RunnableTweens[index].tween = value;
            }
        }
        public int Count => m_RunnableTweens.Count;

        public bool IsReadOnly => false;

        public int PriorityCount(int priority)
        {
            return m_RunnableTweens.Count((rt) => rt.priority == priority);
        }

        /// <summary>
        /// Same as calling <see cref="Append(BXSTweenable)"/>.
        /// </summary>
        public void Add(BXSTweenable item)
        {
            Append(item);
        }

        public void Clear()
        {
            m_RunnableTweens.Clear();
        }

        public bool Contains(BXSTweenable item)
        {
            return m_RunnableTweens.Any((runnable) => runnable.tween == item);
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
                throw new ArgumentNullException(nameof(array));

            if (m_RunnableTweens.Count + arrayIndex > array.Length)
                throw new ArgumentException($"[BXSTweenSequence::CopyTo] Source array was not long enough. array.Length={array.Length}, arrayIndex={arrayIndex}", nameof(array));

            if (arrayIndex < 0 || arrayIndex >= array.Length)
                throw new IndexOutOfRangeException($"[BXSTweenSequence::CopyTo] Given 'arrayIndex' is out of range. arrayIndex={arrayIndex}");

            for (int i = arrayIndex; i < m_RunnableTweens.Count; i++)
            {
                // Copy
                array[i] = m_RunnableTweens[i].tween;
            }
        }

        public bool Remove(BXSTweenable item)
        {
            return m_RunnableTweens.RemoveAll(x => x.tween == item) > 0;
        }

        public IEnumerator<BXSTweenable> GetEnumerator()
        {
            return m_RunnableTweens.Cast((runnable) => runnable.tween).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
