using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BXFW.UI
{
    /// <summary>
    /// Size constraint, containing the entire 'TextMeshPro' text inside as a rect transform.
    /// <br>Useful for stuff such as "Resizing Text backgrounds" and others.</br>
    /// </summary>
    [ExecuteAlways, RequireComponent(typeof(RectTransform)), DisallowMultipleComponent]
    public class TMPTextUIResizer : UICustomResizer
    {
        [Header(":: References")]
        [SerializeField] private TMP_Text target;

        protected override RectTransform ObjectTarget
        {
            get
            {
                // If you don't return null if target is null it throw error
                // (because we try to refer the target's rectTransform)
                if (target == null)
                    return null; 

                return target.rectTransform;
            }
        }
        
        protected override void OnCoroutineUpdate()
        {
            if (TryGetComponent(out Graphic g) && target != null)
                g.enabled = !string.IsNullOrWhiteSpace(target.text);
        }

        protected override Vector2 GetTargetSize()
        {
            return target.GetPreferredValues();
        }
    }
}