using System;
using UnityEngine;

namespace BXFW.Tweening.Next
{
    /// <summary>
    /// Waits a tween until it's finished.
    /// <br>In case the tween throws an exception, this will stop waiting.</br>
    /// <br>If a tween is paused it won't wait unless <see cref="waitWhilePausing"/> is true.</br>
    /// </summary>
    public class BXSWaitForTween : CustomYieldInstruction
    {
        public override bool keepWaiting => tweenable.IsPlaying || (waitWhilePausing && tweenable.IsPaused);

        private readonly bool waitWhilePausing = false;
        private readonly BXSTweenable tweenable;
        public BXSWaitForTween(BXSTweenable target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target), "[BXSWaitForTween::ctor()] Given BXSTweenable target cannot be null.");

            tweenable = target;
        }
        public BXSWaitForTween(BXSTweenable target, bool waitWhilePausing)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target), "[BXSWaitForTween::ctor()] Given BXSTweenable target cannot be null.");

            tweenable = target;
            this.waitWhilePausing = waitWhilePausing;
        }
    }
}
