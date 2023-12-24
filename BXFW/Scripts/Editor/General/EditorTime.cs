using System;
using UnityEditor;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// Manages editor related timing.
    /// <br>(Mainly deltaTime stuff)</br>
    /// </summary>
    public static class EditorTime
    {
        /// <summary>
        /// Last <see cref="EditorApplication.timeSinceStartup"/> on the <see cref="EditorApplication.update"/>.
        /// </summary>
        private static double lastTimeSinceStart = -1d;
        /// <summary>
        /// Last delta time of the editor thread.
        /// </summary>
        public static double DeltaTimeAsDouble { get; private set; } = 0f;
        /// <inheritdoc cref="DeltaTimeAsDouble"/>
        public static float DeltaTime => (float)DeltaTimeAsDouble;
        /// <summary>
        /// The smoothed out delta time.
        /// <br>This delta time value moves from-to the next delta time value with half value stepping.</br>
        /// </summary>
        public static double SmoothDeltaTimeAsDouble { get; private set; } = 0f;
        /// <inheritdoc cref="SmoothDeltaTimeAsDouble"/>
        public static float SmoothDeltaTime => (float)SmoothDeltaTimeAsDouble;

        /// <summary>
        /// Hooks the <see cref="GetGlobalDeltaTime"/> to the <see cref="EditorApplication.update"/>.
        /// </summary>
        static EditorTime()
        {
            lastTimeSinceStart = EditorApplication.timeSinceStartup;
            EditorApplication.update += GetGlobalDeltaTime;
        }

        /// <summary>
        /// Handles the delta time related events on <see cref="EditorApplication.update"/> to get the correct delta time.
        /// </summary>
        private static void GetGlobalDeltaTime()
        {
            // Get delta time valid values
            if (lastTimeSinceStart < 0f)
            {
                lastTimeSinceStart = EditorApplication.timeSinceStartup;
            }

            DeltaTimeAsDouble = EditorApplication.timeSinceStartup - lastTimeSinceStart;
            lastTimeSinceStart = EditorApplication.timeSinceStartup;

            // Get other values
            SmoothDeltaTimeAsDouble = MathUtility.MoveTowards(SmoothDeltaTimeAsDouble, DeltaTimeAsDouble, Math.Abs(SmoothDeltaTime - DeltaTimeAsDouble) / 2f);
        }
    }
}
