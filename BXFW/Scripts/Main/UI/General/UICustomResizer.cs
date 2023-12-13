using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace BXFW.UI
{
    /// <summary>
    /// Resizes itself according to rect transform constraint. (Acts like an <see cref="ContentSizeFitter"/>)
    /// <br>The inheriting class can use the '<see cref="ResizeTarget"/>' field to anything else ui (or into RectTransform : <see cref="RectTransformUIResizer"/>)</br>
    /// </summary>
    [ExecuteAlways, RequireComponent(typeof(RectTransform))]
    public abstract class UICustomResizer : UIBehaviour
    {
        [Header(":: Settings")]
        public bool applyX = true;
        public bool applyY = true;
        [InspectorConditionalDraw(nameof(applyX))]
        public float paddingX = 0f;
        [InspectorConditionalDraw(nameof(applyX)), Clamp(0f, float.MaxValue)]
        public MinMaxValue sizeLimitX = MinMaxValue.Zero;
        [InspectorConditionalDraw(nameof(applyY))]
        public float paddingY = 0f;
        [InspectorConditionalDraw(nameof(applyY)), Clamp(0f, float.MaxValue)]
        public MinMaxValue sizeLimitY = MinMaxValue.Zero;
        [Tooltip("Disables this gameObject if the target is disabled too.")]
        public bool disableIfTargetIs = false;
        public TextAnchor alignPivot = TextAnchor.MiddleCenter;

        /// <summary>
        /// The target object to be resized.
        /// </summary>
        protected abstract RectTransform ResizeTarget { get; }
        /// <summary>
        /// Return the preferred size here.
        /// <br>The padding is done on the base class, and added into <see cref="CurrentTargetValues"/>.</br>
        /// </summary>
        protected abstract Vector2 GetTargetSize();

        /// <summary>
        /// Previously sized values.
        /// <br>The RectTransform is resized only when the 'CurrentTargetValues' are different to the 'prevTargetValues'.</br>
        /// </summary>
        private Vector2 prevTargetValues;
        /// <summary>
        /// Given target values with padding and clamping.
        /// </summary>
        public Vector2 CurrentTargetValues
        {
            get
            {
                if (ResizeTarget == null)
                {
                    return Vector2.zero;
                }

                Vector2 result = GetTargetSize() + new Vector2(paddingX, paddingY);

                if (sizeLimitX.Max > float.Epsilon)
                {
                    result.x = sizeLimitX.ClampBetween(result.x);
                }

                if (sizeLimitY.Max > float.Epsilon)
                {
                    result.y = sizeLimitY.ClampBetween(result.y);
                }

                return result;
            }
        }
        private RectTransform m_RectTransform;
        /// <summary>
        /// Rect transform attached to this <see cref="Component.gameObject"/>.
        /// </summary>
        public RectTransform RectTransform
        {
            get
            {
                if (m_RectTransform == null)
                {
                    m_RectTransform = GetComponent<RectTransform>();
                }

                return m_RectTransform;
            }
        }

        // Manage
        protected override void OnEnable()
        {
            base.OnEnable();

            UpdateRectTransform();
        }
        /// <summary>
        /// An update method that is called on the LateUpdate.
        /// <br>It does not run if the current object is destroyed or disabled, it will be re-run when it gets enabled or created again.</br>
        /// </summary>
        protected virtual void OnLateUpdate()
        { }

        // Update
        // -- Editor Update
        /// <summary>
        /// The update method.
        /// <br>The base only contains an editor method, it is <b>optional only in 
        /// standalone player</b> so the base call can be '#if'def'd to UNITY_EDITOR:</br>
        /// </summary>
        protected virtual void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UpdateRectTransform();
            }
#endif
        }
        private void LateUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            if (ResizeTarget != null)
            {
                // Disable object if the target is disabled too.
                if (!ResizeTarget.gameObject.activeInHierarchy && disableIfTargetIs)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    gameObject.SetActive(true);
                }
            }

            UpdateRectTransform();
            OnLateUpdate();
        }

        protected bool ShouldUpdate()
        {
            // Check target
            if (ResizeTarget == null)
            {
                return false;
            }

            // Check if target is enabled (note : this object is disabled in update if the target is disabled)
            // Disabling the object here, unity doesn't allow it.
            // (this was the case in the previous event based update, it may have been changed)
            if (!ResizeTarget.gameObject.activeInHierarchy)
            {
                return false;
            }

            // Check preferenced values
            if (CurrentTargetValues == prevTargetValues)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The layout group cache, is static and used with <see cref="SendUpdateToLayoutGroups"/>.
        /// </summary>
        protected static readonly List<LayoutGroup> m_layoutGroupCache = new List<LayoutGroup>(32);
        /// <summary>
        /// Calls <see cref="LayoutGroup"/>'s calculation and layouting functions.
        /// </summary>
        protected void SendUpdateToLayoutGroups()
        {
            Transform t = transform;
            // Iterate all parents
            while (t != null)
            {
                t.GetComponents(m_layoutGroupCache);
                for (int i = 0; i < m_layoutGroupCache.Count; i++)
                {
                    if (m_layoutGroupCache[i] == null)
                    {
                        continue;
                    }

                    m_layoutGroupCache[i].CalculateLayoutInputHorizontal();
                    m_layoutGroupCache[i].CalculateLayoutInputVertical();
                    m_layoutGroupCache[i].SetLayoutHorizontal();
                    m_layoutGroupCache[i].SetLayoutVertical();
                    // TODO : Determine if calling only one layout group's functions is enough.
                    // For now just call all for good measure
                }

                t = t.parent;
            }
        }
        /// <summary>
        /// Manually update rect transform to target.
        /// </summary>
        /// <param name="xAxisUpdate">Update horizontal axis.</param>
        /// <param name="yAxisUpdate">Update vertical axis.</param>
        public void UpdateRectTransform(bool xAxisUpdate = true, bool yAxisUpdate = true)
        {
            if (!ShouldUpdate())
            {
                return;
            }

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
                            var setSize = CurrentTargetValues.x + paddingX;
                            var offsetSize = (setSize - rectWidth) / 2f;

                            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, setSize);
                            transform.localPosition = new Vector2(transform.localPosition.x + offsetSize, transform.localPosition.y);
                        }
                        break;
                    case TextAnchor.UpperCenter:
                    case TextAnchor.MiddleCenter:
                    case TextAnchor.LowerCenter:
                        {
                            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CurrentTargetValues.x + paddingX);
                        }
                        break;
                    case TextAnchor.UpperRight:
                    case TextAnchor.MiddleRight:
                    case TextAnchor.LowerRight:
                        {
                            var setSize = CurrentTargetValues.x + paddingX;
                            var offsetSize = (setSize - rectWidth) / 2f;

                            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, setSize);
                            transform.localPosition = new Vector2(transform.localPosition.x - offsetSize, transform.localPosition.y);
                        }
                        break;
                }

                prevTargetValues.x = CurrentTargetValues.x;
            }

            if (applyY && yAxisUpdate)
            {
                switch (alignPivot)
                {
                    case TextAnchor.UpperLeft:
                    case TextAnchor.UpperCenter:
                    case TextAnchor.UpperRight:
                        {
                            var setSize = CurrentTargetValues.y + paddingY;
                            var offsetSize = (setSize - rectHeight) / 2f;
                            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, setSize);

                            transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y - offsetSize);
                        }
                        break;
                    case TextAnchor.MiddleLeft:
                    case TextAnchor.MiddleCenter:
                    case TextAnchor.MiddleRight:
                        {
                            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CurrentTargetValues.y + paddingY);
                        }
                        break;
                    case TextAnchor.LowerLeft:
                    case TextAnchor.LowerCenter:
                    case TextAnchor.LowerRight:
                        {
                            var setSize = CurrentTargetValues.y + paddingY;
                            var offsetSize = (setSize - rectHeight) / 2f;

                            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, setSize);
                            transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y + offsetSize);
                        }
                        break;
                }

                prevTargetValues.y = CurrentTargetValues.y;
            }

            // Update all parent LayoutGroups to recalculate
            SendUpdateToLayoutGroups();
        }
    }
}
