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
            // there can be only 1 graphic anyways
            // though it would have been better if i cached it
            // (but also there's no performance diff because unity made comparing with null intensive so lol)
            if (target != null && TryGetComponent(out Graphic g))
                g.enabled = !string.IsNullOrWhiteSpace(target.text);
        }

        protected override Vector2 GetTargetSize()
        {
            return target.GetPreferredValues();
        }
    }
}