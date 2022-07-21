using TMPro;
using UnityEngine;

namespace BXFW.UI
{
    /// <summary>
    /// Size constraint, containing the entire 'TextMeshPro' text inside as a rect transform.
    /// <br>Useful for stuff such as "Resizing Text backgrounds" and others.</br>
    /// </summary>
    [ExecuteAlways, RequireComponent(typeof(RectTransform))]
    public class TMPTextUIResizer : UICustomResizer
    {
        [Header(":: References")]
        [SerializeField] private TMP_Text target;

        protected override RectTransform ObjectTarget
        {
            get
            {
                if (target == null)
                    return null; // This is a bad coding pattern moment, we have to do it like this to make it a RectTransform
                                 // Otherwise we get a null exception (basically null is error handling for the base class)

                return target.rectTransform;
            }
        }

        protected override Vector2 GetTargetSize()
        {
            return target.GetPreferredValues();
        }
    }
}