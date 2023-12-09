using System.Collections.Generic;
using UnityEngine;

namespace BXFW
{
    public class ParallaxBackgroundGroup : MonoBehaviour
    {
        /// <summary>
        /// List of the generated backgrounds.
        /// </summary>
        public List<ParallaxBackgroundObj> ParallaxBGObjs = new List<ParallaxBackgroundObj>();
        /// <summary>
        /// Shorthand variable for <see cref="List{T}.Count"/> of <see cref="ParallaxBGObjs"/>.
        /// </summary>
        public int ChildAmount => ParallaxBGObjs.Count;

        public bool UseGlobalGroupColor = false;
        [SerializeField] private Color _GroupColor = Color.white;
        /// <summary>
        /// The global color for the current group.
        /// </summary>
        public Color GroupColor
        {
            get { return _GroupColor; }
            set
            {
                _GroupColor = value;

                if (!UseGlobalGroupColor)
                {
                    return;
                }

                foreach (var obj in ParallaxBGObjs)
                {
                    if (obj == null)
                    {
                        Debug.LogWarning($"[ParallaxBackgroundGroup::(set)GroupColor] There are null objects in group '{name}'.");
                        continue;
                    }

                    obj.TilingSpriteRendererComponent.Color = _GroupColor;
                }
            }
        }
        public TransformAxis2D ScrollAxis = TransformAxis2D.XAxis;
        public Camera TargetCamera;

        private void Awake()
        {
            if (TargetCamera == null)
            {
                Debug.LogWarning($"[ParallaxBackground] You didn't assign a target camera in GameObject with name : \"{name}\". Assigning main camera instead.");
                TargetCamera = Camera.main;
            }
        }
    }
}
