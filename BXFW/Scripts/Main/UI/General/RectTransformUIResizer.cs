using System.Collections;
using UnityEngine;

namespace BXFW.UI
{
    /// INFO : The anchoring is mostly meant to be used with the target being a child object.
    /// Resizing using the 'Rect Tool' is broken unless you hold alt (because the position of the target changes which the script cannot accomodate for).
    /// <summary>
    /// Resizes a rect transform to other with options.
    /// </summary>
    public class RectTransformUIResizer : UICustomResizer
    {
        [Header(":: References")]
        [SerializeField] private RectTransform target;

        protected override RectTransform ObjectTarget
        {
            get
            {
                if (target != null)
                {
                    // If the target is set to stretch with this object (anchors min is (0, 0) and max is (1, 1) then the object padding will affect the 'GetTargetSize')
                    // Make sure the object anchor is a dot (anchors are equal)
                    if (target.anchorMin != target.anchorMax)
                    {
                        Debug.LogWarning($"[RectTransformUIResizer::(get)Target] Target with name \"{target.name}\" has custom anchor. Only use anchors that has equal max & min values.");
#if UNITY_EDITOR
                        UnityEditor.Undo.RecordObject(this, $"modify anchors of {target.name}");
                        target.anchorMin = target.anchorMax;
#else
                        target.anchorMin = target.anchorMax; // Set this randomly (will probably anchor to somewhere random)
#endif
                    }
                }

                return target;
            }
        }

        protected override Vector2 GetTargetSize()
        {
            return target.rect.size;
        }
    }
}