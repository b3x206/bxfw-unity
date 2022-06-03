using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;

namespace BXFW.UI
{
    /// TODO : This is crying for optimization (especially when there's a lot of ui's)
    /// <summary>
    /// Size constraint, containing the entire 'TextMeshPro' text inside as a rect transform.
    /// <br>Useful for stuff such as "Resizing Text backgrounds" and others.</br>
    /// </summary>
    [ExecuteAlways, RequireComponent(typeof(RectTransform))]
    public class TMPTextUIResizer : UIBehaviour/*, ILayoutController, ILayoutSelfController*/
    {
        [Header(":: Settings")]
        public float paddingX = 0f;
        public float paddingY = 0f;
        public bool applyX = true, applyY = true;
        public TextAlignment alignPivot = TextAlignment.Left;
        [Header(":: Reference")]
        [SerializeField] private TMP_Text target;

        // <summary>
        // Used as an 'SizeDelta' constraint.
        // Apparently using this causes the rect transform bug (maybe dirty thing? but content size fitter work well? wtf)
        // </summary>
        //private DrivenRectTransformTracker tracker;
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

        //protected override void OnEnable()
        //{
        //    base.OnEnable();
        //    tracker.Add(this, RectTransform, DrivenTransformProperties.SizeDeltaX | DrivenTransformProperties.SizeDeltaY);
        //}
        //protected override void OnDisable()
        //{
        //    tracker.Clear();
        //    LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
        //    base.OnDisable();
        //}

        //private void SetDirty()
        //{
        //    if (IsActive())
        //    {
        //        LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
        //    }
        //}

        private void Update()
        {
            UpdateRectTransform();
        }

        protected bool ShouldUpdate()
        {
            if (target == null)
                return false;

            if (!target.gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
                return false;
            }
            else
            {
                gameObject.SetActive(true);
            }

            if (CurrentPrefValues == prevPrefValues)
                return false; // prefValues != prevPrefValues beyond this point

            return true;
        }
        protected void UpdateRectTransform()
        {
            if (!ShouldUpdate())
                return;

            Canvas.ForceUpdateCanvases(); // Call this || yield return new WaitForEndOfFrame()
            // DO NOT USE THE 

            var rectWidth = RectTransform.rect.width;
            var rectHeight = RectTransform.rect.height;

            if (applyX)
            {
                switch (alignPivot)
                {
                    case TextAlignment.Left:
                        {
                            var setSize = CurrentPrefValues.x + paddingX;
                            var offsetSize = (setSize - rectWidth) / 2f;

                            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, setSize);
                            transform.localPosition = new Vector2(transform.localPosition.x + offsetSize, transform.localPosition.y);
                        }
                        break;
                    case TextAlignment.Center:
                        {
                            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CurrentPrefValues.x + paddingX);
                        }
                        break;
                    case TextAlignment.Right:
                        {
                            var setSize = CurrentPrefValues.x + paddingX;
                            var offsetSize = (setSize - rectWidth) / 2f;

                            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, setSize);
                            transform.localPosition = new Vector2(transform.localPosition.x - offsetSize, transform.localPosition.y);
                        }
                        break;
                }
            }

            if (applyY)
            {
                switch (alignPivot)
                {
                    case TextAlignment.Left:
                        {
                            var setSize = CurrentPrefValues.y + paddingY;
                            var offsetSize = (setSize - rectHeight) / 2f;
                            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, setSize);

                            transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y - offsetSize);
                        }
                        break;
                    case TextAlignment.Center:
                        {
                            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CurrentPrefValues.y + paddingY);
                        }
                        break;
                    case TextAlignment.Right:
                        {
                            var setSize = CurrentPrefValues.y + paddingY;
                            var offsetSize = (setSize - rectHeight) / 2f;

                            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, setSize);
                            transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y + offsetSize);
                        }
                        break;
                }
            }

            prevPrefValues = CurrentPrefValues;
        }
    }
}