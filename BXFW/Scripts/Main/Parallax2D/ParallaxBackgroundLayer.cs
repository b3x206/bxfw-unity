using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Represents a scrollable by camera movement, parallaxed <see cref="TilingSpriteRenderer"/>.
    /// </summary>
    [RequireComponent(typeof(TilingSpriteRenderer))]
    public class ParallaxBackgroundLayer : MonoBehaviour
    {
        private float m_length;
        private Vector2 m_startPos;
        /// <summary>
        /// Amount of parallaxing applied to this layer.
        /// <br>Minimum value is 0, maximum is 1. Higher the value the more this layer moves.</br>
        /// </summary>
        public float parallaxEffectAmount;
        /// <summary>
        /// The parent group this layer was attached on.
        /// </summary>
        public ParallaxBackgroundGroup parentGroup;

        public TilingSpriteRenderer TilingRendererComponent
        {
            get { return m_TilingRendererComponent; }
        }
        [SerializeField] private TilingSpriteRenderer m_TilingRendererComponent;

        public void InitilazeTilingSpriteRenderer(Sprite rendSprite)
        {
            if (!TryGetComponent(out m_TilingRendererComponent))
            {
                m_TilingRendererComponent = gameObject.AddComponent<TilingSpriteRenderer>();
            }

            if (rendSprite == null)
            {
                Debug.LogError("[ParallaxBackgroundObj] Null sprite was passed.");
                return;
            }

            TilingRendererComponent.TiledSprite = rendSprite;
            TilingRendererComponent.AllowGridAxis = TransformAxis2D.XAxis;
            TilingRendererComponent.AutoTile = true;

            TilingRendererComponent.cameraResize = true;
            TilingRendererComponent.resizeTargetCamera = parentGroup.targetCamera;
        }

        private void Start()
        {
            if (parentGroup.targetCamera == null)
            {
                Debug.LogError($"[ParallaxBackground] A parent with name \"{parentGroup.name}\" doesn't have a target camera assigned.");
            }

            m_startPos.x = transform.position.x;
            m_startPos.y = transform.position.y;
            m_length = TilingRendererComponent.SingleBounds.size.x;
        }

        private void Update()
        {
            Scroll();
        }
        public void Scroll()
        {
            Vector3 positionDelta = Vector3.zero;
            if ((parentGroup.scrollAxis & TransformAxis2D.XAxis) == TransformAxis2D.XAxis)
            {
                float temp = parentGroup.targetCamera.transform.position.x * (1f - parallaxEffectAmount);
                float dist = parentGroup.targetCamera.transform.position.x * parallaxEffectAmount;

                positionDelta.x += dist;

                if (temp > m_startPos.x + m_length)
                {
                    m_startPos.x += m_length;
                }
                else if (temp < m_startPos.x - m_length)
                {
                    m_startPos.x -= m_length;
                }
            }
            if ((parentGroup.scrollAxis & TransformAxis2D.YAxis) == TransformAxis2D.YAxis)
            {
                // -- Follow the camera position in Y too
                // float yTemp = ParentGroup.TargetCamera.transform.position.y * (1f - ParallaxEffectAmount);
                // * Note that we don't need to restart / tile on Y parallax, so the 'yTemp' reset value can be commented
                // * Both axis parallax works the same.
                float yDist = parentGroup.targetCamera.transform.position.y * parallaxEffectAmount;

                positionDelta.y += yDist;
            }

            transform.position = new Vector3(m_startPos.x + positionDelta.x, m_startPos.y + positionDelta.y, transform.position.z);
        }
    }
}
