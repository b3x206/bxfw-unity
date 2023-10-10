using UnityEngine;

namespace BXFW.UI
{
    /// <summary>
    /// Resizes a <see cref="RectTransform"/> to fit <see cref="Screen.safeArea"/>.
    /// <br>Useful for fitting gui to a phone with notch.</br>
    /// <br>Portrait only.</br>
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        /// <summary>
        /// Whether if to allow resizing of rect after the initial <see cref="Awake"/> calculation.
        /// </summary>
        public bool AllowResize = true;
        private float m_OffsetTop = 0f;
        private float m_OffsetBottom = 0f;

        private void Awake()
        {
            Resize();
        }

        private void Update()
        {
            if (!AllowResize)
                return; // Size can be modified after initial calculation. 

            // I have no idea why i added this
            // FIXME :  The 'SafeAreaFitter' should not constantly size itself (?)
            if (transform.hasChanged)
                Resize();
        }
        
        /// <summary>
        /// Resizes the safe area.
        /// </summary>
        /// <param name="offset">Offset of the safe area. Modify if dynamic content is displayed.</param>
        public void Resize(float heightOffsetTop = -1f, float heightOffsetBottom = -1f)
        {
            var rTransform = GetComponent<RectTransform>();
            var safeArea = Screen.safeArea;

            if (heightOffsetTop >= 0f)
                m_OffsetTop = heightOffsetTop;
            if (heightOffsetBottom >= 0f)
                m_OffsetBottom = heightOffsetBottom;

            // Is portrait || Square
            // Offset the safe area
            if (Screen.height >= Screen.width)
            {
                safeArea.yMax -= m_OffsetTop;
                safeArea.yMin += m_OffsetBottom;
            }
            else
            {
                safeArea.xMin += m_OffsetTop;
                safeArea.yMin -= m_OffsetBottom;
            }

            var anchorMin = safeArea.position;
            var anchorMax = anchorMin + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rTransform.anchorMin = anchorMin;
            rTransform.anchorMax = anchorMax;
            // Resize expand (to anchors)
            rTransform.offsetMin = Vector2.zero;
            rTransform.offsetMax = Vector2.zero;
        }
    }
}