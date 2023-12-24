using UnityEngine;

namespace BXFW.UI
{
    /// <summary>
    /// Resizes a <see cref="UnityEngine.RectTransform"/> to fit <see cref="Screen.safeArea"/>.
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

        private Vector2 m_previousSizeDelta;
        private RectTransform m_RectTransform;
        public RectTransform RectTransform
        {
            get
            {
                if (m_RectTransform == null)
                {
                    m_RectTransform = GetComponent<RectTransform>();
                }

                return m_RectTransform;
            }
        }

        private void Awake()
        {
            Resize();
        }

        private void Update()
        {
            if (!AllowResize)
            {
                // Size can be modified after initial calculation. 
                return;
            }

            // In a case of the Rect Transform resizing unintentionally
            // Resize the SafeAreaFitter's RectTransform again.
            if (transform.hasChanged && m_previousSizeDelta != RectTransform.sizeDelta)
            {
                Resize();
            }
        }

        /// <summary>
        /// Resizes to fit to the <see cref="Screen.safeArea"/>.
        /// </summary>
        /// <param name="heightOffsetTop">
        /// Offset of the safe area.
        /// Modify if dynamic content is displayed.
        /// Values that are 0 or lower is ignored and the previous offset will be used.
        /// </param>
        /// <param name="heightOffsetBottom">
        /// Offset of the safe area.
        /// Modify if dynamic content is displayed.
        /// Values that are 0 or lower is ignored and the previous offset will be used.
        /// </param>
        public void Resize(float heightOffsetTop = -1f, float heightOffsetBottom = -1f)
        {
            Rect safeArea = Screen.safeArea;

            if (heightOffsetTop >= 0f)
            {
                m_OffsetTop = heightOffsetTop;
            }

            if (heightOffsetBottom >= 0f)
            {
                m_OffsetBottom = heightOffsetBottom;
            }

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

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = anchorMin + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            RectTransform.anchorMin = anchorMin;
            RectTransform.anchorMax = anchorMax;
            // Resize expand (to anchors)
            RectTransform.offsetMin = Vector2.zero;
            RectTransform.offsetMax = Vector2.zero;

            // Store size delta
            m_previousSizeDelta = RectTransform.sizeDelta;
        }
    }
}
