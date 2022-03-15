using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Counts fps.
    /// <br>Call <see cref="UpdateFPSCounter"/> on every <c>Update()</c> [NOT <c>FixedUpdate()</c>] for accurate measuring.</br>
    /// </summary>
    [System.Serializable]
    public class FPSCounter
    {
        /// <summary>The refresh rate of the fps. If set to a negative value (or zero), it will refresh on all frames.</summary>
        [Range(-.01f, 1f)] public float RefreshTime = -.01f;
        /// <summary>Current framerate of the game. Returns an accurate value if <see cref="UpdateFPSCounter"/> is called properly.</summary>
        public float CurrentFPS { get; private set; } = -1;

        private float FPSTimer;
        private float PrevTimeElapsed = -1f;

        /// <summary>
        /// Updates the FPS counter.
        /// Call with 'Update()' function in your Monobehaviour.
        /// </summary>
        public void UpdateFPSCounter()
        {
            float TimeElapsed = Time.smoothDeltaTime;

            // We already updated this frame, return.
            if (PrevTimeElapsed == TimeElapsed) return;

            if (FPSTimer <= 0)
            { FPSTimer = RefreshTime; }
            else
            { FPSTimer -= TimeElapsed; }

            // If statement is seperated for getting 'TimeElapsed' more accurately.
            if (FPSTimer <= 0)
            {
                // Set fps to '60' if smoothDeltaTime returns 0f.
                // This fixed a bug where the starting fps is -1 and doesn't update.
                // (Basically it fixes some issues,
                //  but can be removed as the updating mechanism was modified after the last time this comment was written)
                if (TimeElapsed <= 0f)
                { TimeElapsed = 0.016f; }

                CurrentFPS = (int)(1f / TimeElapsed);
            }

            // If CurrentFPS wasn't set. 'RefreshTime' delays the update rate.
            if (CurrentFPS < 0)
            {
                // Current framerate isn't set on first frames (Update Delay), so we use the targetFrameRate.
                CurrentFPS = 60;
            }

            // Set previous 'TimeElapsed' for avoiding updating multiple times in a frame.
            PrevTimeElapsed = TimeElapsed;
        }

        /// <summary>
        /// Get the current fps as string.
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} FPS", CurrentFPS);
        }

        public FPSCounter()
        { }
    }
}