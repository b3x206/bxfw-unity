using System.Collections;

using UnityEngine;
using UnityEngine.EventSystems;

namespace BXFW.UI
{
    /// <summary>
    /// Resizes itself according to rect transform constraint. (Acts like an <see cref="UnityEngine.UI.ContentSizeFitter"/>)
    /// <br>The inheriting class can use the '<see cref="ObjectTarget"/>' field to anything else ui (or into RectTransform : <see cref="RectTransformUIResizer"/>)</br>
    /// </summary>
    [ExecuteAlways, RequireComponent(typeof(RectTransform))]
    public abstract class UICustomResizer : UIBehaviour
    {
        [Header(":: Settings")]
        public float paddingX = 0f;
        public float paddingY = 0f;
        public bool applyX = true, applyY = true;
        [Tooltip("Disables this gameObject if the target is disabled too.")]
        public bool disableIfTargetIs = false;
        public TextAnchor alignPivot = TextAnchor.MiddleCenter;

        /// <summary>
        /// The target object to be resized.
        /// </summary>
        protected abstract RectTransform ObjectTarget { get; }
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
        /// Given target values with padding.
        /// </summary>
        public Vector2 CurrentTargetValues
        {
            get
            {
                if (ObjectTarget == null)
                    return Vector2.zero;

                return GetTargetSize() + new Vector2(paddingX, paddingY);
            }
        }
        private RectTransform _rectTransform;
        /// <summary>
        /// Rect transform attached to this <see cref="Component.gameObject"/>.
        /// </summary>
        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();

                return _rectTransform;
            }
        }

        private Coroutine currentRoutine;
        // Manage
        protected override void Awake()
        {
            base.Awake();

            currentRoutine = StartCoroutine(UpdateCoroutine());
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            // Restart routine as the object/behaviour is re-enabled.
            if (currentRoutine == null)
            {
                currentRoutine = StartCoroutine(UpdateCoroutine());
            }

            UpdateRectTransform();
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            currentRoutine = null;
            // Coroutine is stopped when the entire gameobject is disabled anyways
            // But just the behaviour disabling will keep the routine running
            // We don't want that.
            // (note : this may not set the entire coroutine stopping as this is probably called when the behaviour disables so we just stop the coroutines here)
            StopAllCoroutines();
        }
        /// <summary>
        /// An update method that is called with the coroutine.
        /// <br>This is run on the end of this frame (<see cref="WaitForEndOfFrame"/>).</br>
        /// <br>It does not run if the current object is destroyed or disabled, it will be re-run when it gets enabled or created again.</br>
        /// </summary>
        protected virtual void OnCoroutineUpdate() { }

        // Update
        // -- Editor Update
        protected virtual void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UpdateRectTransform();
            }
#endif
        }
        protected bool ShouldUpdate()
        {
            // Check target
            if (ObjectTarget == null)
                return false;

            // Check if target is enabled (note : this object is disabled in update if the target is disabled)
            // Disabling the object here, unity doesn't allow it.
            // (this was the case in the previous event based update, it may have been changed)
            if (!ObjectTarget.gameObject.activeInHierarchy)
                return false;

            // Check preferenced values
            if (CurrentTargetValues == prevTargetValues)
                return false;

            return true;
        }
        private IEnumerator UpdateCoroutine()
        {
            for (;;)
            {
                // Coroutine will be broken / disabled when the
                // A : GameObject is destroy/kil
                // B : GameObject is disable
                // no.
                // It won't stop when the behaviour is disabled but we intercept 'OnDisable' to disable the routine
                // This coroutine waits until end of frame for 'RectTransform' calculations to be done
                // TODO : Maybe use 'LateUpdate'?
                yield return new WaitForEndOfFrame();

                if (ObjectTarget != null)
                {
                    // Disabling the 'gameObject' does kill the coroutine, so use a coroutine dispatcher.
                    // Disable object if the target is disabled too.
                    
                    if (!ObjectTarget.gameObject.activeInHierarchy && 
                        disableIfTargetIs)
                    {
                        gameObject.SetActive(false);
                    }
                    else
                    {
                        gameObject.SetActive(true);
                    }
                }

                UpdateRectTransform();
                OnCoroutineUpdate();
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
        }
    }
}