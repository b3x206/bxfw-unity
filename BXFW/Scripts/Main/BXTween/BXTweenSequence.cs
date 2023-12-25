using System;
using System.Linq;
using System.Collections;
using BXFW.Tweening.Events;
using System.Collections.Generic;
using UnityEngine;

namespace BXFW.Tweening
{
    /// <summary>
    /// A list of tweens that can be run sequentially or programatically.
    /// <br>This is not tied inside the <see cref="BXTween"/> system, as <see cref="BXTween"/> was not built with that in mind.</br>
    /// </summary>
    [Serializable]
    public class BXTweenSequence : IEnumerable<ITweenCTX>
    {
        public class RunnableTweenContext : IEquatable<RunnableTweenContext>, IComparable<RunnableTweenContext>
        {
            public int priority;
            public ITweenCTX tween;

            public RunnableTweenContext(int priority, ITweenCTX ctx)
            {
                this.priority = priority;
                tween = ctx;
            }

            public int CompareTo(RunnableTweenContext other)
            {
                if (other == null)
                {
                    return 1;
                }

                return priority.CompareTo(other.priority);
            }

            public bool Equals(RunnableTweenContext other)
            {
                if (other == null)
                {
                    return false;
                }

                return priority == other.priority && tween == other.tween;
            }
        }
        // Maybe do this as a queue? idk.
        /// <summary>
        /// Contains the runnable tweens.
        /// </summary>
        private List<RunnableTweenContext> m_Tweens = new List<RunnableTweenContext>();

        /// <summary>
        /// Current ID that is being run.
        /// </summary>
        private int currentRunPriority = 0;

        /// <summary>
        /// Called on the end of sequence.
        /// </summary>
        public BXTweenMethod OnSequenceEnd;
        /// <summary>
        /// Called when a tween ends in the sequence.
        /// <br>ID is given with 'Key'.</br>
        /// </summary>
        public BXTweenSetMethod<KeyValuePair<int, ITweenCTX>> OnIndiviualTweenEnd;
        /// <summary>
        /// Whether if the sequence is running.
        /// </summary>
        public bool IsRunning { get; private set; } = false;
        /// <summary>
        /// Last <see cref="RunnableTweenContext.priority"/> in the list <see cref="m_Tweens"/>.
        /// <br>Returns -1 as the start id if no tweens exist. This will be the last group id used.</br>
        /// </summary>
        public int LastPriority
        {
            get
            {
                RunnableTweenContext lastContext = m_Tweens.LastOrDefault();
                return lastContext != null ? lastContext.priority : -1;
            }
        }
        /// <summary>
        /// The sequence duration.
        /// <br>This depends on the added tweens, if no tweens this will be zero.</br>
        /// <br>This calculates all tweens duration + delay.</br>
        /// </summary>
        public float Duration
        {
            get
            {
                float duration = 0f;
                int checkPriority = 0;
                float maxDuration = 0f;
                for (int i = 0; i < m_Tweens.Count; i++)
                {
                    var runnable = m_Tweens[i];

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
                    if (i == m_Tweens.Count - 1)
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
        public void Join(ITweenCTX ctx)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx), "[BXTweenSequence::Join] Given context parameter is null.");
            }

            if (ctx.IsRunning)
            {
                ctx.StopTween();
            }

            if (LastPriority <= -1)
            {
                Debug.LogWarning(string.Format("[BXTweenSequence::Join] Tried to join context {0} without any tweens appended. Appending first tween.", ctx));
                Append(ctx);
                return;
            }

            m_Tweens.Add(new RunnableTweenContext(LastPriority, ctx));
        }
        /// <summary>
        /// Joins a tween to the start.
        /// <br>Priority will be zero for the given <paramref name="ctx"/>.</br>
        /// </summary>
        /// <param name="ctx"></param>
        public void JoinFirst(ITweenCTX ctx)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx), "[BXTweenSequence::JoinFirst] Given context parameter is null.");
            }

            if (ctx.IsRunning)
            {
                ctx.StopTween();
            }

            m_Tweens.Insert(0, new RunnableTweenContext(0, ctx));
        }
        /// <summary>
        /// Appends a tween with new id.
        /// <br>Appended tween contexts will be waited sequentially.</br>
        /// </summary>
        public void Append(ITweenCTX ctx)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx), "[BXTweenSequence::Append] Given context parameter is null.");
            }

            if (ctx.IsRunning)
            {
                ctx.StopTween();
            }

            m_Tweens.Add(new RunnableTweenContext(LastPriority + 1, ctx));
        }
        /// <summary>
        /// Prepends the tween to the start (id = 0).
        /// <br>Shifts all runnable added tweens by 1.</br>
        /// </summary>
        public void Prepend(ITweenCTX ctx)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx), "[BXTweenSequence::Prepend] Given context parameter is null.");
            }

            if (ctx.IsRunning)
            {
                ctx.StopTween();
            }

            foreach (var runnable in m_Tweens)
            {
                runnable.priority += 1;
            }

            m_Tweens.Insert(0, new RunnableTweenContext(0, ctx));
        }

        /// <summary>
        /// Starts running the sequence.
        /// <br>Restarts running if the <see cref="IsRunning"/> is true.</br>
        /// </summary>
        public void Play()
        {
            if (m_Tweens.Count <= 0)
            {
                throw new NullReferenceException("[BXTweenSequence::Run] The sequence has no runnable tweens!");
            }

            // Already running.
            if (IsRunning)
            {
                Stop();
            }

            IsRunning = true;
            currentRunPriority = 0;
            RunRecursive();
        }
        /// <summary>
        /// Runs the contexts recursively.
        /// <br>Uses the <see cref="currentRunPriority"/> to keep track of id.</br>
        /// </summary>
        private void RunRecursive()
        {
            ITweenCTX longestDurationCtx = null;
            foreach (var runnable in m_Tweens.Where(rt => rt.priority == currentRunPriority))
            {
                float longestDuration = longestDurationCtx != null ?
                    (longestDurationCtx.Duration + longestDurationCtx.Delay) : float.NegativeInfinity;

                if ((runnable.tween.Duration + runnable.tween.Delay) > longestDuration)
                {
                    longestDurationCtx = runnable.tween;
                }

                runnable.tween.StartTween();
                runnable.tween.SequenceOnEndAction += () =>
                {
                    OnIndiviualTweenEnd?.Invoke(new KeyValuePair<int, ITweenCTX>(runnable.priority, runnable.tween));
                };
            }

            if (longestDurationCtx == null)
            {
                throw new NullReferenceException(string.Format("[BXTweenSequence::RunRecursive] Sequence id={0} does not have any tweens on it.", currentRunPriority));
            }

            longestDurationCtx.SequenceOnEndAction += () =>
            {
                if (currentRunPriority <= LastPriority)
                {
                    currentRunPriority++;
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
        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            OnSequenceEnd?.Invoke();
            IsRunning = false;
            currentRunPriority = 0;
            foreach (var runnable in m_Tweens)
            {
                runnable.tween.SequenceOnEndAction = null;
                runnable.tween.StopTween();
            }
        }

        /// <summary>
        /// Creates a blank sequence.
        /// </summary>
        public BXTweenSequence() { }
        /// <summary>
        /// Reserves a capacity for the tweens.
        /// </summary>
        public BXTweenSequence(int capacity)
        {
            m_Tweens.Capacity = capacity;
        }
        /// <summary>
        /// Adds the enumerable tweens to be run sequentially.
        /// </summary>
        public BXTweenSequence(IEnumerable<ITweenCTX> tweens)
        {
            int i = 0;
            foreach (var ctx in tweens)
            {
                m_Tweens.Add(new RunnableTweenContext(i, ctx));

                i++;
            }
        }

        /// <summary>
        /// Returns a tween in given index.
        /// <br>The priority is not contained with this tween.</br>
        /// </summary>
        public ITweenCTX this[int index]
        {
            get { return m_Tweens[index].tween; }
            set
            {
                if (value == null)
                {
                    throw new NullReferenceException(string.Format("[BXTweenSequence::(set)this[{0}]] Given value for index '{0}' is null.", index));
                }

                m_Tweens[index].tween = value;
            }
        }
        public int Count
        {
            get { return m_Tweens.Count; }
        }
        public int IDCount(int id)
        {
            return m_Tweens.Count((rt) => rt.priority == id);
        }
        public IEnumerator<ITweenCTX> GetEnumerator()
        {
            return m_Tweens.Cast((rc) => rc.tween).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        // idk what to make out of this method
        // but this has a list soo
        // maybe this may cause leaks if there's no references to the added tweens
        // so also dispose those? or maybe get a weakref? idk man.
        //public void Dispose()
        //{
        //    m_Tweens.Clear();
        //}
    }
}
