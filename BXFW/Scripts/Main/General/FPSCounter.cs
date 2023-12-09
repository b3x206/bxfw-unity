using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Counts fps.
    /// <br>Call <see cref="UpdateFPSCounter"/> on every <c>Update()</c> [NOT <c>FixedUpdate()</c>] for measuring.</br>
    /// </summary>
    [System.Serializable]
    public class FPSCounter
    {
        /// <summary>The refresh rate of the fps. If set to a negative value (or zero), it will refresh on all frames.</summary>
        [Range(-.01f, 1f)] public float RefreshTime = -.01f;

        private float m_currentFPS = -1f;
        /// <summary>Current framerate of the game. Returns an accurate value if <see cref="UpdateFPSCounter"/> is called properly.</summary>
        public float CurrentFPS 
        { 
            get
            {
                return m_currentFPS;
            }
            set
            {
                if (MinFPS > value && value >= 0f)
                {
                    MinFPS = value;
                }

                if (MaxFPS < value)
                {
                    MaxFPS = value;
                }

                m_currentFPS = value;
            }
        }
        /// <summary>
        /// Minimum recorded frame rate.
        /// </summary>
        public float MinFPS { get; private set; } = float.MaxValue;
        /// <summary>
        /// Maximum recorded frame rate.
        /// </summary>
        public float MaxFPS { get; private set; } = -1f;

        /// <summary>
        /// Timer value used for custom refresh rates.
        /// </summary>
        private float FPSTimer;
        private float PrevTimeElapsed = -1f;

        /// <summary>
        /// Updates the FPS counter.
        /// Call with 'Update()' function in your Monobehaviour.
        /// </summary>
        public void UpdateFPSCounter()
        {
            float TimeElapsed = Time.unscaledDeltaTime;

            // We already updated this frame, return.
            if (PrevTimeElapsed == TimeElapsed)
            {
                return;
            }

            // Smooth delta time starts to fade into the current Time.deltaTime when it's getting more constant
            // So just use a Mathf.MoveToward thing (smoothDeltaTime's n value is (time - prevTime) * 0.5f)
            // Smooth out the elapsed time (if a previous reference point exists)
            if (PrevTimeElapsed > 0f)
            {
                TimeElapsed = Mathf.MoveTowards(PrevTimeElapsed, TimeElapsed, Mathf.Abs(TimeElapsed - PrevTimeElapsed) * 0.5f);
            }

            if (FPSTimer <= 0)
            { 
                FPSTimer = RefreshTime;
            }
            else
            { 
                FPSTimer -= TimeElapsed;
            }

            // If statement is seperated for getting 'TimeElapsed' more accurately.
            if (FPSTimer <= 0f)
            {
                if (TimeElapsed <= 0f)
                {
                    TimeElapsed = 0.016f;
                }

                CurrentFPS = (int)(1f / TimeElapsed);
            }

            // If CurrentFPS wasn't set. 'RefreshTime' delays the update rate.
            if (CurrentFPS < 0f)
            {
                // Current framerate isn't set on first frames (Update Delay), so we use the targetFrameRate.
                CurrentFPS = 60;
            }

            // Set previous 'TimeElapsed' for avoiding updating multiple times in a frame.
            PrevTimeElapsed = TimeElapsed;
        }
        /// <summary>
        /// Resets <see cref="MinFPS"/> and <see cref="MaxFPS"/>.
        /// <br>Useful for resetting values after initilazation.</br>
        /// </summary>
        public void ResetMinMaxFPS()
        {
            MinFPS = float.MaxValue;
            MaxFPS = -1f;
        }

        /// <summary>
        /// Get the current fps as string.
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} FPS", CurrentFPS);
        }
        public string ToString(string floatFmt)
        {
            return string.Format("{0} FPS", CurrentFPS.ToString(floatFmt));
        }

        public FPSCounter()
        { }
    }
}