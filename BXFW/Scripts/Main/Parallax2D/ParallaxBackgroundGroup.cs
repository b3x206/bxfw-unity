using System.Collections.Generic;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Contains the <see cref="ParallaxBackgroundLayer"/>'s backgrounds list + nicely manages those.
    /// <br>Most of the things + setup are done in the editor for the group.</br>
    /// </summary>
    public class ParallaxBackgroundGroup : MonoBehaviour
    {
        /// <summary>
        /// List of the generated backgrounds.
        /// </summary>
        public List<ParallaxBackgroundLayer> Backgrounds = new List<ParallaxBackgroundLayer>();
        /// <summary>
        /// Shorthand variable for <see cref="List{T}.Count"/> of <see cref="Backgrounds"/>.
        /// </summary>
        public int ChildLength => Backgrounds.Count;

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

                foreach (var obj in Backgrounds)
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
