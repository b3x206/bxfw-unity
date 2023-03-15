using System.Collections.Generic;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Registry of an background.
    /// <br>Maybe TODO : Remove this class as variable <see cref="ParallaxEffectAmount"/> is never used.</br>
    /// </summary>
    [System.Serializable]
    public class ParallaxBackgroundObjRegistry
    {
        public ParallaxBackgroundObj BackgroundLayer;
        public float ParallaxEffectAmount
        {
            get { return BackgroundLayer.ParallaxEffectAmount; }
        }

        public ParallaxBackgroundObjRegistry(ParallaxBackgroundObj obj)
        {
            BackgroundLayer = obj;
        }
    }

    public class ParallaxBackgroundGroup : MonoBehaviour
    {
        // INFO : To hide this list properly make a custom inspector.
        public List<ParallaxBackgroundObjRegistry> ParallaxBGObjList = new List<ParallaxBackgroundObjRegistry>();
        public int ChildAmount => ParallaxBGObjList.Count - 1;

        public bool UseGlobalGroupColor = false;
        [SerializeField] private Color _GroupColor = Color.white;
        public Color GroupColor
        {
            get { return _GroupColor; }
            set
            {
                _GroupColor = value;

                if (!UseGlobalGroupColor) return;

                foreach (var obj in ParallaxBGObjList)
                {
                    if (obj.BackgroundLayer == null)
                    {
                        Debug.LogWarning($"[ParallaxBackgroundGroup::(set)GroupColor] There are null objects in group '{name}'.");
                        continue;
                    }

                    obj.BackgroundLayer.TilingSpriteRendererComponent.Color = _GroupColor;
                }
            }
        }
        public Camera TargetCamera;
        public float LengthOffset = 0f;

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
