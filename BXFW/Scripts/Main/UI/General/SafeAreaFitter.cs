using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BXFW.UI
{
    #pragma warning disable IDE0052 // Unused method warning, visual studio falsely assumes that unity functions are not called.

    /// <summary>
    /// Resizes a <see cref="RectTransform"/> to fit <see cref="Screen.safeArea"/>.
    /// <br>Useful for fitting gui to a phone with notch.</br>
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        /// <summary>
        /// Whether if to allow resizing of rect after the initial <see cref="Awake"/> calculation.
        /// </summary>
        public bool AllowResize = true;

        private void Awake()
        {
            Resize();
        }

        private void Update()
        {
            if (AllowResize) return; // Size can be modified after initial calculation. 

            if (transform.hasChanged)
                Resize();
        }

        private void Resize()
        {
            var rTransform = GetComponent<RectTransform>();
            var safeArea = Screen.safeArea;

            var anchorMin = safeArea.position;
            var anchorMax = anchorMin + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rTransform.anchorMin = anchorMin;
            rTransform.anchorMax = anchorMax;
        }
    }
}