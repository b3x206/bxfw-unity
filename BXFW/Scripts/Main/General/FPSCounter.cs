using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Counts fps in an easier way.
    /// <br>Call <see cref="Update"/> on every <c>Update()</c> for constant measuring.</br>
    /// </summary>
    /// <example>
    /// <![CDATA[
    /// using BXFW;
    /// using UnityEngine;
    /// 
    /// /// <summary>
    /// /// Draws a FPS counter to the top-left of the screen.
    /// /// <br>Uses Unity IMGUI to do this, you can use anything that can draw text as long as the counter is updated that way.</br>
    /// /// </summary>
    /// public class CounterExample : MonoBehaviour
    /// {
    ///     public FPSCounter counter;
    ///     
    ///     private void Update()
    ///     {
    ///         counter.Update();
    ///     }
    ///     private void OnGUI()
    ///     {
    ///         // FPSCounter.ToString already appends ' FPS' to the end.
    ///         GUI.Box(new Rect(10, 10, 150, 18), counter.ToString());
    ///     }
    /// }
    /// ]]>
    /// </example>
    [System.Serializable]
    public sealed class FPSCounter
    {
        /// <summary>
        /// The refresh rate of the fps. If set to a negative value (or zero), it will refresh on all frames.
        /// </summary>
        [Range(-.01f, 1f)] public float refreshTime = -.01f;

        private float m_CurrentFPS = -1f;
        /// <summary>
        /// Current framerate of the game. Returns an accurate value if <see cref="Update"/> is called properly.
        /// </summary>
        public float CurrentFPS 
        { 
            get
            {
                return m_CurrentFPS;
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

                m_CurrentFPS = value;
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
        public void Update()
        {
            float timeElapsed = Time.unscaledDeltaTime;

            // We already updated this frame, return.
            if (PrevTimeElapsed == timeElapsed)
            {
                return;
            }

            // Smooth delta time starts to fade into the current Time.deltaTime when it's getting more constant
            // So just use a Mathf.MoveToward thing (smoothDeltaTime's n value is (time - prevTime) * 0.5f)
            // Smooth out the elapsed time (if a previous reference point exists)
            if (PrevTimeElapsed > 0f)
            {
                timeElapsed = Mathf.MoveTowards(PrevTimeElapsed, timeElapsed, Mathf.Abs(timeElapsed - PrevTimeElapsed) * 0.5f);
            }

            if (FPSTimer <= 0)
            { 
                FPSTimer = refreshTime;
            }
            else
            { 
                FPSTimer -= timeElapsed;
            }

            // If statement is seperated for getting 'TimeElapsed' more accurately.
            if (FPSTimer <= 0f)
            {
                if (timeElapsed <= 0f)
                {
                    timeElapsed = 0.016f;
                }

                CurrentFPS = (int)(1f / timeElapsed);
            }

            // If CurrentFPS wasn't set. 'RefreshTime' delays the update rate.
            if (CurrentFPS < 0f)
            {
                // Current framerate isn't set on first frames (Update Delay), so we use the targetFrameRate.
                CurrentFPS = 60;
            }

            // Set previous 'TimeElapsed' for avoiding updating multiple times in a frame.
            PrevTimeElapsed = timeElapsed;
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
        /// <br>This string representation is like <c><see cref="CurrentFPS"/>.ToString() FPS</c></br>
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} FPS", CurrentFPS);
        }
        /// <summary>
        /// <inheritdoc cref="ToString()"/>
        /// </summary>
        /// <param name="floatFmt">Format argument for the <see cref="CurrentFPS"/> value.</param>
        public string ToString(string floatFmt)
        {
            return string.Format("{0} FPS", CurrentFPS.ToString(floatFmt));
        }

        public FPSCounter()
        { }
    }
}
