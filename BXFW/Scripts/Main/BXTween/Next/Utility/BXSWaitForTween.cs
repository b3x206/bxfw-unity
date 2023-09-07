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
            tweenable = target;
        }
        public BXSWaitForTween(BXSTweenable target, bool waitWhilePausing)
        {
            tweenable = target;
            this.waitWhilePausing = waitWhilePausing;
        }
    }
}
