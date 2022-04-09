using System.Collections;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace BXFW.UI
{
    /// <summary>
    /// Swipable UI canvas with menus.
    /// </summary>
    public class SwipableUI : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        #region Event Class
        [System.Serializable]
        public class SwipableUIOnMenuChangeEvent : UnityEvent<int> { }
        #endregion

        #region Variables
        [Header(":: Settings")]
        public int ClampItemMenu = -1;
        public float ClampContentDragOnMenuEnd = 200f;
        [Range(0.01f, 2f)] public float PercentSwipeToOtherMenuThreshold = .3f;
        [Range(0.01f, 32f)] public float SwipeToOtherMenuAnimTime = .2f;

        [Header(":: References")]
        public RectTransform ItemContainer;

        [Header(":: Current")]
        [SerializeField, InspectorReadOnlyView] private int _CurrentMenu;
        public SwipableUIOnMenuChangeEvent OnMenuChangeEvent;
        public int CurrentMenu
        {
            get
            {
                return _CurrentMenu;
            }
            set
            {
                var menuValue = Mathf.Clamp(value, 0, ClampItemMenu < 0 ? int.MaxValue : ClampItemMenu);
                var diffBetweenMenu = value - _CurrentMenu;

                if (diffBetweenMenu != 0)
                {
                    var newLocation = ContainerInitialPosition + (new Vector2(-ItemContainer.rect.width, 0f) * diffBetweenMenu);

                    if (CurrentSwipeToOtherCoroutine != null)
                    {
                        StopCoroutine(CurrentSwipeToOtherCoroutine);
                    }

                    CurrentSwipeToOtherCoroutine = StartCoroutine(SmoothSwipe(ItemContainer.localPosition, newLocation, SwipeToOtherMenuAnimTime));
                    ContainerInitialPosition = newLocation;
                }

                _CurrentMenu = menuValue;
                OnMenuChangeEvent?.Invoke(_CurrentMenu);
            }
        }

        // -- Private
        private Vector2 ContainerInitialPosition;
        private Coroutine CurrentSwipeToOtherCoroutine;
        #endregion

        #region Init
        private void Awake()
        {
            if (ItemContainer == null)
            {
                if (!TryGetComponent(out ItemContainer))
                {
                    Debug.LogError($"[SwipableUI::Awake] The object \"{name}\" doesn't have a ItemContainer assigned. Please assign one.");
                }
            }

            ContainerInitialPosition = ItemContainer.localPosition;
        }
        #endregion

        #region Menu Drag
        public void OnDrag(PointerEventData data)
        {
            float swipeDelta = data.pressPosition.x - data.position.x; // The difference between the start point and end point.
            // yay it worked on first try.
            // (maybe) TODO : Add smooth slowdown on swipe limit, like the clash royales and other ui options.
            if (!Mathf.Approximately(ClampContentDragOnMenuEnd, 0f))
            {
                if (_CurrentMenu >= ClampItemMenu)
                {
                    // We are swiping RTL and we should clamp.
                    swipeDelta = Mathf.Clamp(swipeDelta, -((ItemContainer.rect.width * ClampItemMenu) + ClampContentDragOnMenuEnd), ClampContentDragOnMenuEnd);
                }
                if (_CurrentMenu <= 0)
                {
                    // We are swiping LTR and we should clamp.
                    swipeDelta = Mathf.Clamp(swipeDelta, -ClampContentDragOnMenuEnd, (ItemContainer.rect.width * ClampItemMenu) + ClampContentDragOnMenuEnd);
                }
            }

            var posX = ContainerInitialPosition.x - swipeDelta; // Local position to set.
            ItemContainer.localPosition = new Vector2(posX, ContainerInitialPosition.y);
        }
        public void OnEndDrag(PointerEventData data)
        {
            float percentage = (data.pressPosition.x - data.position.x) / ItemContainer.rect.width;

            void ResetDragDefault()
            {
                if (CurrentSwipeToOtherCoroutine != null)
                {
                    StopCoroutine(CurrentSwipeToOtherCoroutine);
                }

                CurrentSwipeToOtherCoroutine = StartCoroutine(SmoothSwipe(ItemContainer.localPosition, ContainerInitialPosition, SwipeToOtherMenuAnimTime));
            }

            if (Mathf.Abs(percentage) >= PercentSwipeToOtherMenuThreshold)
            {
                var newLocation = ContainerInitialPosition;
                if (percentage > 0f)
                {
                    newLocation += new Vector2(-ItemContainer.rect.width, 0);

                    // Do not toggle the dragging routine if we in last possible menu.
                    if (_CurrentMenu >= ClampItemMenu)
                    {
                        ResetDragDefault();
                        return;
                    }
                    _CurrentMenu++;
                    OnMenuChangeEvent?.Invoke(_CurrentMenu);
                }
                else if (percentage < 0f)
                {
                    newLocation += new Vector2(ItemContainer.rect.width, 0);

                    // Do not toggle the end dragging routine if we in 0'th menu.
                    if (_CurrentMenu <= 0)
                    {
                        ResetDragDefault();
                        return;
                    }
                    _CurrentMenu--;
                    OnMenuChangeEvent?.Invoke(_CurrentMenu);
                }

                if (CurrentSwipeToOtherCoroutine != null)
                {
                    StopCoroutine(CurrentSwipeToOtherCoroutine);
                }

                CurrentSwipeToOtherCoroutine = StartCoroutine(SmoothSwipe(ItemContainer.localPosition, newLocation, SwipeToOtherMenuAnimTime));
                ContainerInitialPosition = newLocation;
            }
            else
            {
                ResetDragDefault();
            }
        }

        /// <summary>
        /// Interpolates the swiping transition.
        /// </summary>
        private IEnumerator SmoothSwipe(Vector2 startPos, Vector2 endPos, float duration)
        {
            float t = 0f;

            while (t <= 1.0)
            {
                t += Time.deltaTime / duration;

                ItemContainer.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));

                yield return null;
            }

            ItemContainer.localPosition = endPos;
            CurrentSwipeToOtherCoroutine = null;
        }
        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var gColor = Gizmos.color;
            if (_CurrentMenu >= ClampItemMenu)
            {
                // Show the gizmo on right (According to menu).
                Gizmos.color = Color.green;
                var linePos = new Vector2(ItemContainer.transform.position.x + ((ItemContainer.rect.width / 2f) - ClampContentDragOnMenuEnd), ItemContainer.transform.position.y);
                Gizmos.DrawLine(linePos + new Vector2(0f, 100f), linePos - new Vector2(0f, 100f));
            }
            if (_CurrentMenu <= 0)
            {
                // Show the gizmo on left.
                Gizmos.color = Color.red;
                var linePos = new Vector2(ItemContainer.transform.position.x - ((ItemContainer.rect.width / 2f) + ClampContentDragOnMenuEnd), ItemContainer.transform.position.y);
                Gizmos.DrawLine(linePos + new Vector2(0f, 100f), linePos - new Vector2(0f, 100f));
            }
            Gizmos.color = gColor;
        }
#endif
    }
}