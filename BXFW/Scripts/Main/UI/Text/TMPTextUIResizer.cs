using System.Collections;
using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BXFW.UI
{
    /// <summary>
    /// Size constraint, containing the entire 'TextMeshPro' text inside as a rect transform.
    /// <br>Useful for stuff such as "Resizing Text backgrounds" and others.</br>
    /// </summary>
    [ExecuteAlways, RequireComponent(typeof(RectTransform))]
    public class TMPTextUIResizer : UIBehaviour
    {
        [Header(":: Settings")]
        public float paddingX = 0f;
        public float paddingY = 0f;
        public bool applyX = true, applyY = true;
        public TextAnchor alignPivot = TextAnchor.MiddleCenter;
        [Header(":: Reference")]
        [SerializeField] private TMP_Text target;

        private Vector2 prevPrefValues;
        private Vector2 CurrentPrefValues
        {
            get
            {
                if (target == null)
                    return Vector2.zero;

                return new Vector2(target.preferredWidth + paddingX, target.preferredHeight + paddingY);
            }
        }
        private RectTransform _rectTransform;
        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();

                return _rectTransform;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            StartCoroutine(UpdateCoroutine());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            StopAllCoroutines();
        }

#if UNITY_EDITOR
        // -- Editor Update
        private void Update()
        {
            if (!Application.isPlaying)
            {
                UpdateRectTransform();
            }
        }
#endif

        protected bool ShouldUpdate()
        {
            // Check target
            if (target == null)
                return false;

            // Check if target is enabled (note : this object is disabled in update if the target is disabled)
            // Disabling the object here, unity doesn't allow it.
            if (!target.gameObject.activeInHierarchy)
                return false;

            // Check preferenced values
            if (CurrentPrefValues == prevPrefValues)
                return false;

            return true;
        }
        private IEnumerator UpdateCoroutine()
        {
            for (; ; )
            {
                yield return new WaitForEndOfFrame();

                if (target != null)
                {
                    // Disable object if the text is disabled too.
                    if (!target.gameObject.activeInHierarchy)
                    {
                        gameObject.SetActive(false);
                    }
                    else
                    {
                        gameObject.SetActive(true);
                    }
                }

                UpdateRectTransform();
            }
        }

        /// <summary>
        /// Manually update rect transform to text.
        /// </summary>
        /// <param name="xAxisUpdate">Update horizontal axis.</param>
        /// <param name="yAxisUpdate">Update vertical axis.</param>
        public void UpdateRectTransform(bool xAxisUpdate = true, bool yAxisUpdate = true)
        {
            if (!ShouldUpdate())
                return;

            // No need to update canvases if we are calling this from a waiting coroutine
            // (events only invoke once globally for this behaviour, like an static method)
            // This way we get rid of bad code & make use of events & get proper rect size
            // (Basically this method is called when the canvas is updated so no need to call Canvas.ForceUpdateCanvases)

            var rectWidth = RectTransform.rect.width;
            var rectHeight = RectTransform.rect.height;

            if (applyX && xAxisUpdate)
            {
                switch (alignPivot)
                {
                    case TextAnchor.UpperLeft:
                    case TextAnchor.MiddleLeft:
                    case TextAnchor.LowerLeft:
                        {
                            var setSize = CurrentPrefValues.x + paddingX;
                            var offsetSize = (setSize - rectWidth) / 2f;

                            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, setSize);
                            transform.localPosition = new Vector2(transform.localPosition.x + offsetSize, transform.localPosition.y);
                        }
                        break;
                    case TextAnchor.UpperCenter:
                    case TextAnchor.MiddleCenter:
                    case TextAnchor.LowerCenter:
                        {
                            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CurrentPrefValues.x + paddingX);
                        }
                        break;
                    case TextAnchor.UpperRight:
                    case TextAnchor.MiddleRight:
                    case TextAnchor.LowerRight:
                        {
                            var setSize = CurrentPrefValues.x + paddingX;
                            var offsetSize = (setSize - rectWidth) / 2f;

                            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, setSize);
                            transform.localPosition = new Vector2(transform.localPosition.x - offsetSize, transform.localPosition.y);
                        }
                        break;
                }

                prevPrefValues.x = CurrentPrefValues.x;
            }

            if (applyY && yAxisUpdate)
            {
                switch (alignPivot)
                {
                    case TextAnchor.UpperLeft:
                    case TextAnchor.UpperCenter:
                    case TextAnchor.UpperRight:
                        {
                            var setSize = CurrentPrefValues.y + paddingY;
                            var offsetSize = (setSize - rectHeight) / 2f;
                            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, setSize);

                            transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y - offsetSize);
                        }
                        break;
                    case TextAnchor.MiddleLeft:
                    case TextAnchor.MiddleCenter:
                    case TextAnchor.MiddleRight:
                        {
                            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CurrentPrefValues.y + paddingY);
                        }
                        break;
                    case TextAnchor.LowerLeft:
                    case TextAnchor.LowerCenter:
                    case TextAnchor.LowerRight:
                        {
                            var setSize = CurrentPrefValues.y + paddingY;
                            var offsetSize = (setSize - rectHeight) / 2f;

                            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, setSize);
                            transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y + offsetSize);
                        }
                        break;

                }

                prevPrefValues.y = CurrentPrefValues.y;
            }
        }
    }
}