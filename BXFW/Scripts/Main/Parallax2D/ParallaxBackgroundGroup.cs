using System.Collections.Generic;
using UnityEngine;

namespace BXFW
{
    public class ParallaxBackgroundGroup : MonoBehaviour
    {
        /// <summary>
        /// List of the generated backgrounds.
        /// </summary>
        public List<ParallaxBackgroundObj> m_Backgrounds = new List<ParallaxBackgroundObj>();
        /// <summary>
        /// Shorthand variable for <see cref="List{T}.Count"/> of <see cref="m_Backgrounds"/>.
        /// </summary>
        public int ChildLength => m_Backgrounds.Count;

        public bool useGlobalGroupColor = false;
        [SerializeField] private Color m_GroupColor = Color.white;
        /// <summary>
        /// The global color for the current group.
        /// </summary>
        public Color GroupColor
        {
            get { return m_GroupColor; }
            set
            {
                m_GroupColor = value;

                if (!useGlobalGroupColor)
                    return;

                foreach (var obj in m_Backgrounds)
                {
                    if (obj == null)
                    {
                        Debug.LogWarning($"[ParallaxBackgroundGroup::(set)GroupColor] There are null objects in group '{name}'.");
                        continue;
                    }

                    obj.TilingRendererComponent.Color = m_GroupColor;
                }
            }
        }
        public TransformAxis2D scrollAxis = TransformAxis2D.XAxis;
        public Camera targetCamera;

        private void Awake()
        {
            if (targetCamera == null)
            {
                Debug.LogWarning($"[ParallaxBackground] No target camera assigned in GameObject with name : \"{name}\". Assigning Camera.main instead.");
                targetCamera = Camera.main;
            }
        }
    }
}
