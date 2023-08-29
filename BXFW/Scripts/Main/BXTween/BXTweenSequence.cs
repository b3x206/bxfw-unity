using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BXFW.Tweening
{
    /// <summary>
    /// A list of tweens that can be run sequentially or programatically.
    /// </summary>
    [Serializable]
    public class BXTweenSequence : IEnumerable<ITweenCTX>
    {
        public class RunnableTweenContext
        {
            public int id;
            public ITweenCTX tween;

            public RunnableTweenContext(int id, ITweenCTX ctx)
            {
                this.id = id;
                tween = ctx;
            }
        }

        private List<RunnableTweenContext> m_Tweens = new List<RunnableTweenContext>();

        /// <summary>
        /// Current ID that is being run.
        /// </summary>
        private int currentRunID = 0;

        public bool IsRunning { get; private set; } = false;
        /// <summary>
        /// Last <see cref="RunnableTweenContext.id"/> in the list <see cref="m_Tweens"/>.
        /// <br>Returns -1 as the start id if no tweens exist. This will be the last group id used.</br>
        /// </summary>
        public int LastID
        {
            get
            {
                RunnableTweenContext lastContext = m_Tweens.LastOrDefault();
                return lastContext != null ? lastContext.id : -1;
            }
        }

        /// <summary>
        /// Joins a tween to current id.
        /// <br>Joined tweens will be run at the same time.</br>
        /// </summary>
        public void Join(ITweenCTX ctx)
        {
            if (ctx.IsRunning)
                ctx.StopTween();

            m_Tweens.Add(new RunnableTweenContext(LastID, ctx));
        }
        /// <summary>
        /// Appends a tween with new id.
        /// <br>Appended tween contexts will be waited sequentially.</br>
        /// </summary>
        public void Append(ITweenCTX ctx)
        {
            if (ctx.IsRunning)
                ctx.StopTween();

            m_Tweens.Add(new RunnableTweenContext(LastID + 1, ctx));
        }

        /// <summary>
        /// Starts running the sequence.
        /// </summary>
        public void Run()
        {
            if (m_Tweens.Count <= 0)
            {
                throw new NullReferenceException("[BXTweenSequence::Run] The sequence has no runnable tweens!");
            }

            IsRunning = true;
            RunRecursive();
        }
        /// <summary>
        /// Runs the contexts recursively.
        /// <br>Uses the <see cref="currentRunID"/> to keep track of id.</br>
        /// </summary>
        private void RunRecursive()
        {
            ITweenCTX longestDurationCtx = null;
            foreach (var runnable in m_Tweens.Where(rt => rt.id == currentRunID))
            {
                float longestDuration = longestDurationCtx != null ? 
                    (longestDurationCtx.Duration + longestDurationCtx.Delay) : float.NegativeInfinity;
                
                if ((runnable.tween.Duration + runnable.tween.Delay) > longestDuration)
                    longestDurationCtx = runnable.tween;
                runnable.tween.StartTween();
            }

            if (longestDurationCtx == null)
                throw new NullReferenceException(string.Format("[BXTweenSequence::RunRecursive] Sequence id={0} does not have any tweens on it.", currentRunID));

            longestDurationCtx.TweenCompleteAction += () => 
            {
                if (currentRunID <= LastID)
                {
                    currentRunID++;
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
                return;

            IsRunning = false;
            currentRunID = 0;
            foreach (var runnable in m_Tweens)
            {
                runnable.tween.ClearCompleteAction();
                runnable.tween.StopTween();
            }
        }

        public ITweenCTX this[int index]
        {
            get { return m_Tweens[index].tween; }
            set
            {
                if (value == null)
                    throw new NullReferenceException(string.Format("[BXTweenSequence::(set)this[{0}]] Given value for index is null.", value));

                m_Tweens[index].tween = value;
            }
        }
        public int Count
        {
            get { return m_Tweens.Count; }
        }
        public int IDCount(int id)
        {
            return m_Tweens.Count((rt) => rt.id == id);
        }
        public IEnumerator<ITweenCTX> GetEnumerator()
        {
            return m_Tweens.Cast((rc) => rc.tween).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
