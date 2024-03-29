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
        private Vector2 lastPreferredValues = Vector2.zero;
        private bool targetTextChanged = true;

        protected override RectTransform ResizeTarget
        {
            get
            {
                // If you don't return null if target is null it throw error
                // (because we try to refer the target's rectTransform)
                if (target == null)
                {
                    return null;
                }

                return target.rectTransform;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            // base.ShouldUpdate() gets the 'GetTargetSize' which causes massive lags if the text content is large
            // For this, only get the preferred values if the text was changed
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChangedEvent);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            TMPro_EventManager.TEXT_CHANGED_EVENT?.Remove(OnTextChangedEvent);
        }

        private void OnTextChangedEvent(Object textObj)
        {
            if (textObj == target)
            {
                targetTextChanged = true;
            }
        }

        protected override void OnLateUpdate()
        {
            // there can be only 1 graphic anyways
            // though it would have been better if i cached it
            // (but also there's no performance diff because unity made comparing with null intensive so lol)
            if (target != null && TryGetComponent(out Graphic g))
            {
                g.enabled = !string.IsNullOrWhiteSpace(target.text);
            }
        }

        protected override Vector2 GetTargetSize()
        {
#if UNITY_EDITOR
            // Editor only check, as the UICustomResizer doesn't work on the editor..
            if (!Application.isPlaying)
            {
                lastPreferredValues = target.GetPreferredValues();
            }
            else
#endif
            if (targetTextChanged)
            {
                // No dirty/caching exists on 'GetPrefferedValues'
                // Use the 'target.preferredWidth/target.preferredHeight' instead
                // But this version does not support / take account of the margins? what the hell?
                // Eh whatever this thing allocates enough garbage and has performance loss anyways.
                lastPreferredValues = new Vector2(target.preferredWidth, target.preferredHeight);
                targetTextChanged = false;
            }

            return lastPreferredValues;
        }
    }
}
