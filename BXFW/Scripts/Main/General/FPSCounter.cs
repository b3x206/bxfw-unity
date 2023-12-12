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
    /// using System.Text;
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
    ///         
    ///         // This is a more complex IMGUI based one
    ///         // Note that IMGUI allocates more garbage than UGUI, use something like TMPro 
    ///         DrawWithMinMax();
    ///     }
    /// 
    ///     /// <summary>
    ///     /// If this is true, the FPS counter GUI will also show the Min+Max 
    ///     /// data that the <see cref="CurrentCounter"/> has calculated.
    ///     /// </summary>
    ///     private bool fpsCounterIsExpanded = false;
    ///     private GUIStyle fpsCounterMinMaxBoxStyle;
    ///     private GUIStyle boldFontBoxStyle;
    ///     private readonly StringBuilder fpsDisplayBuilder = new StringBuilder(64);
    ///     private void DrawWithMinMax()
    ///     {
    ///         float screenWidthScale = Screen.safeArea.width / 1080f;
    ///         float screenHeightScale = Screen.safeArea.height / 1920f;
    ///         
    ///         fpsCounterMinMaxBoxStyle ??= new GUIStyle(GUI.skin.box)
    ///         {
    ///             alignment = TextAnchor.UpperLeft,
    ///             padding = new RectOffset(10, 10, 10, 10),
    ///             richText = true,
    ///             fontSize = (int)(30 * screenWidthScale),
    ///         };
    ///         boldFontBoxStyle ??= new GUIStyle(GUI.skin.box)
    ///         {
    ///             alignment = TextAnchor.MiddleCenter,
    ///             fontStyle = FontStyle.Bold,
    ///             fontSize = (int)(30 * screenWidthScale)
    ///         };
    ///     
    ///         // Draw a box with the FPS counter (on top-left corner)
    ///         float counterButtonWidth = 200f * screenWidthScale, counterButtonHeight = 40f * screenHeightScale;
    ///         float counterElementsVerticalSpacing = 20f * screenHeightScale;
    ///         Rect fpsCounterDisplayBtnRect = new Rect(
    ///             Screen.safeArea.xMax - (counterButtonWidth + 10f),
    ///             Screen.safeArea.yMin + (counterButtonHeight / 2f),
    ///             counterButtonWidth,
    ///             counterButtonHeight
    ///         );
    ///         if (GUI.Button(fpsCounterDisplayBtnRect, $"{CurrentCounter.CurrentFPS:0.#} FPS", boldFontBoxStyle))
    ///         {
    ///             fpsCounterIsExpanded = !fpsCounterIsExpanded;
    ///         }
    ///     
    ///         if (fpsCounterIsExpanded)
    ///         {
    ///             // Draw the min/max + reset min/max
    ///             Rect fpsCounterMinMaxRect = new Rect(
    ///                 fpsCounterDisplayBtnRect.x,
    ///                 fpsCounterDisplayBtnRect.y + fpsCounterDisplayBtnRect.height + counterElementsVerticalSpacing,
    ///                 counterButtonWidth,
    ///                 200f * screenHeightScale
    ///             );
    ///     
    ///             fpsDisplayBuilder.Clear();
    ///             // $"<size=12>Min</size>\n{CurrentCounter.MinFPS:0.#} fps\n<size=12>Max</size>\n{CurrentCounter.MaxFPS:0.#} fps"
    ///             fpsDisplayBuilder.Append("<size=").Append(20 * screenWidthScale).Append(">Min</size>\n");
    ///             fpsDisplayBuilder.Append(CurrentCounter.MinFPS.ToString("0.#")).Append(" FPS\n<size=");
    ///             fpsDisplayBuilder.Append(20 * screenWidthScale).Append(">Max</size>\n").Append(CurrentCounter.MaxFPS.ToString("0.#")).Append(" FPS");
    ///     
    ///             GUI.Box(fpsCounterMinMaxRect, fpsDisplayBuilder.ToString(), fpsCounterMinMaxBoxStyle);
    ///     
    ///             Rect fpsCounterResetMinMaxRect = new Rect(
    ///                 fpsCounterMinMaxRect.x,
    ///                 fpsCounterMinMaxRect.y + fpsCounterMinMaxRect.height + counterElementsVerticalSpacing,
    ///                 counterButtonWidth,
    ///                 counterButtonHeight
    ///             );
    ///     
    ///             if (GUI.Button(fpsCounterResetMinMaxRect, "Reset", boldFontBoxStyle))
    ///             {
    ///                 CurrentCounter.ResetMinMaxFPS();
    ///             }
    ///         }
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
        private float fpsTimer;
        private float prevTimeElapsed = -1f;

        /// <summary>
        /// Updates the FPS counter.
        /// Call with 'Update()' function in your Monobehaviour.
        /// </summary>
        public void Update()
        {
            float timeElapsed = Time.unscaledDeltaTime;

            // We already updated this frame, return.
            if (prevTimeElapsed == timeElapsed)
            {
                return;
            }

            // Smooth delta time starts to fade into the current Time.deltaTime when it's getting more constant
            // So just use a Mathf.MoveToward thing (smoothDeltaTime's n value is (time - prevTime) * 0.5f)
            // Smooth out the elapsed time (if a previous reference point exists)
            if (prevTimeElapsed > 0f)
            {
                timeElapsed = Mathf.MoveTowards(prevTimeElapsed, timeElapsed, Mathf.Abs(timeElapsed - prevTimeElapsed) * 0.5f);
            }

            if (fpsTimer <= 0)
            { 
                fpsTimer = refreshTime;
            }
            else
            { 
                fpsTimer -= timeElapsed;
            }

            // If statement is seperated for getting 'TimeElapsed' more accurately.
            if (fpsTimer <= 0f)
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
            prevTimeElapsed = timeElapsed;
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
