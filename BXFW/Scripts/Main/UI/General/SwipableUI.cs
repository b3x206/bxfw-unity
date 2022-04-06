using System.Collections;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace BXFW.UI
{
    public class SwipableUI : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        #region Event Class
        [System.Serializable]
        public class SwipableUIOnMenuChangeEvent : UnityEvent<int> { }
        #endregion

        #region Variables
        [Header(":: Settings")]
        public int ClampItemMenu = -1;
        public Vector2 ClampItemContainer;
        [Range(0.01f, 2f)] public float PercentSwipeToOtherMenuThreshold = .3f;
        [Range(0.01f, 32f)] public float SwipeToOtherMenuAnimTime = .2f;

        [Header(":: References")]
        public RectTransform ItemContainer;

        [Header(":: Current")]
        public SwipableUIOnMenuChangeEvent OnMenuChangeEvent;
        [SerializeField] private int _CurrentMenu;
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
            float swipedifference = data.pressPosition.x - data.position.x;
            var posX = ClampItemContainer == Vector2.zero ? ContainerInitialPosition.x - swipedifference : Mathf.Clamp(ContainerInitialPosition.x - swipedifference,
                ClampItemContainer.x, ClampItemContainer.y);

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
                    if (_CurrentMenu <= 0f)
                    {
                        ResetDragDefault();
                        return;
                    }
                    _CurrentMenu--;
                    OnMenuChangeEvent?.Invoke(_CurrentMenu);
                }

                if (ClampItemContainer != Vector2.zero)
                {
                    if (ItemContainer.localPosition.x <= ClampItemContainer.x || ItemContainer.localPosition.x >= ClampItemContainer.y)
                    {
                        ResetDragDefault();
                        return;
                    }
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

        /// maybe use CTween here?
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
            var prevColor = Gizmos.color;

            if (ClampItemContainer != Vector2.zero)
            {
                if (ItemContainer == null)
                {
                    if (!TryGetComponent(out ItemContainer))
                    {
                        return;
                        // Debug.LogError($"[SwipableUI::Awake] The object \"{name}\" doesn't have a ItemContainer assigned. Please assign one.");
                    }
                }

                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(new Vector3(ItemContainer.transform.position.x + ClampItemContainer.x, ItemContainer.transform.position.y),
                    new Vector3(20f, ItemContainer.rect.height));
                Gizmos.DrawWireCube(new Vector3(ItemContainer.transform.position.x + ClampItemContainer.y, ItemContainer.transform.position.y),
                    new Vector3(20f, ItemContainer.rect.height));
            }

            Gizmos.color = prevColor;
        }
#endif
    }
}