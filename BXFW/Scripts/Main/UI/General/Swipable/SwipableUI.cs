﻿using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace BXFW.UI
{
    /// <summary>
    /// Swipable UI that can swipe through fixed width menus.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SwipableUI : InteractableUIBehaviour, IDragHandler, IEndDragHandler, IScrollHandler
    {
        /// <summary>
        /// A unity event that takes an int.
        /// </summary>
        [Serializable]
        public class MenuChangeEvent : UnityEvent<int> { }

        [Header(":: Settings")]
        [SerializeField, Clamp(-1, int.MaxValue)]
        private int m_MenuCount = -1;
        /// <summary>
        /// The menu count. When set defines the size of the menus.
        /// </summary>
        public int MenuCount
        {
            get { return m_MenuCount; }
            set { m_MenuCount = value; OnMenuCountChanged?.Invoke(); }
        }
        /// <summary>
        /// An action (for <see cref="SwipableUIProgressDisplay"/>), invoked when variable <see cref="MenuCount"/> is changed.
        /// </summary>
        public event Action OnMenuCountChanged;
        /// <summary>
        /// The ui transform clamp. This clamps transforms position if there's clamping.
        /// </summary>
        public float swipeDragClampLength = 200f;
        /// <summary>
        /// The swipe threshold. This controls the amount of swipe required to go to the next menu.
        /// </summary>
        [Range(0.01f, 2f)] public float swipePercentageThreshold = .25f;
        /// <summary>
        /// The swipe animation duration. 
        /// This controls the length of animation of the swipe after <see cref="IEndDragHandler.OnEndDrag(PointerEventData)"/> is called.
        /// </summary>
        [Range(0.01f, 32f)] public float swipeTweenDuration = .2f;
        /// <summary>
        /// Whether if this <see cref="SwipableUI"/> has scrollability.
        /// </summary>
        public bool IsScrollable => scrollSensitivity > 0f;
        /// <summary>
        /// Controls the scroll sensitivity (of the scroll handler).
        /// </summary>
        public float scrollSensitivity = 0f;
        /// <summary>
        /// Cooldown time for scrolling delta applying callback of OnEndDrag.
        /// </summary>
        [DrawIf(nameof(IsScrollable)), Clamp(0f, float.MaxValue)]
        public float scrollWaitTime = .16f;

        [Header(":: References")]
        [SerializeField] private RectTransform m_ItemContainer;
        /// <summary>
        /// The target rect transform that contains the items.
        /// <br>The container should be with the same width of the items if possible, as the container is used for width.</br>
        /// </summary>
        public RectTransform ItemContainer
        {
            get
            {
                if (m_ItemContainer == null)
                {
                    if (!TryGetComponent(out m_ItemContainer))
                    {
                        throw new NullReferenceException($"[SwipableUI::Awake] The object \"{transform.GetPath()}\" doesn't have a ItemContainer assigned. SwipableUI won't work without a rect transform.");
                    }
                }

                return m_ItemContainer;
            }
        }

        [Header(":: Current")]
        [SerializeField, ReadOnlyView] private int m_CurrentMenu;
        /// <summary>
        /// The current menu. Changing this will go into the target menu.
        /// <br>The value is clamped between 0 and <see cref="MenuCount"/>.</br>
        /// </summary>
        public int CurrentMenu
        {
            get
            {
                return m_CurrentMenu;
            }
            set
            {
                // Clamp menuValue
                int menuValue = Mathf.Clamp(value, 0, MenuCount < 0 ? int.MaxValue : MenuCount - 1);
                // Delta should use the clamped value to avoid bugs
                float menuDelta = menuValue - m_CurrentMenu;

                // Change menus if there's delta
                if (!Mathf.Approximately(menuDelta, 0f))
                {
                    // Get location
                    Vector2 newLocation = m_containerInitialPosition + (new Vector2(-ItemContainer.rect.width, 0f) * menuDelta);

                    // Interpolate to new location
                    SmoothSwipe(ItemContainer.localPosition, newLocation, swipeTweenDuration);
                    m_containerInitialPosition = newLocation;

                    // Set values (only when there's delta)
                    m_CurrentMenu = menuValue;
                    OnMenuChangeEvent?.Invoke(m_CurrentMenu);
                }
            }
        }
        /// <summary>
        /// <see cref="UnityEvent"/> invoked when <see cref="CurrentMenu"/> is set or <see cref="IEndDragHandler.OnEndDrag(PointerEventData)"/>(with difference) is called.
        /// </summary>
        public MenuChangeEvent OnMenuChangeEvent;

        // -- State
        /// <summary>
        /// Initial container position.
        /// <br>Used in a reseting state if the swiping past to the next menu condition wasn't satisfied.</br>
        /// </summary>
        private Vector2 m_containerInitialPosition;

        #region Init
        protected override void Awake()
        {
            // Get initial positions
            m_containerInitialPosition = ItemContainer.localPosition;
            // Call base awake (even though there's nothing in UIBehaviour)
            base.Awake();
        }
        #endregion

        #region Menu Drag
        public void OnDrag(PointerEventData data)
        {
            if (!Interactable)
            {
                return;
            }

            float swipeDelta = data.pressPosition.x - data.position.x; // The difference between the start point and end point.

            // Only apply swipe clamping if the MenuCount is in valid range.
            if (MenuCount > 0)
            {
                if (!Mathf.Approximately(swipeDragClampLength, 0f))
                {
                    if (m_CurrentMenu <= 0)
                    {
                        // We are swiping LTR and we should clamp.
                        swipeDelta = Mathf.Clamp(swipeDelta, -swipeDragClampLength, (ItemContainer.rect.width * (MenuCount - 1)) + swipeDragClampLength);
                    }
                    else if (m_CurrentMenu >= MenuCount - 1)
                    {
                        // We are swiping RTL and we should clamp.
                        swipeDelta = Mathf.Clamp(swipeDelta, -((ItemContainer.rect.width * (MenuCount - 1)) + swipeDragClampLength), swipeDragClampLength);
                    }
                    else
                    {
                        // Clamp swipe completely using width bounds
                        // tested : works fine on even numbers of menus, don't care about scrolling this much anyways, it's disabled ootb
                        swipeDelta = Mathf.Clamp(
                            swipeDelta,
                            // menuCount = 3
                            // currentMenu = 0, 1, 2
                            -((ItemContainer.rect.width * (MenuCount - (1 + m_CurrentMenu))) + swipeDragClampLength), // 0 + clamp
                            (ItemContainer.rect.width * (MenuCount - (1 + m_CurrentMenu))) + swipeDragClampLength     // 2 + clamp 
                        );
                    }
                }
            }

            float posX = m_containerInitialPosition.x - swipeDelta; // Local position to set.
            ItemContainer.localPosition = new Vector2(posX, m_containerInitialPosition.y);
        }
        public void OnEndDrag(PointerEventData data)
        {
            if (!Interactable)
            {
                return;
            }

            float percentage = (data.pressPosition.x - data.position.x) / ItemContainer.rect.width;

            if (Mathf.Abs(percentage) >= swipePercentageThreshold)
            {
                Vector2 newLocation = m_containerInitialPosition;
                // Swipe RTL (menu increment)
                if (percentage > 0f)
                {
                    newLocation += new Vector2(-ItemContainer.rect.width, 0);

                    // Do not toggle the dragging routine if we in last possible menu.
                    if (m_CurrentMenu >= MenuCount - 1)
                    {
                        SmoothSwipe(ItemContainer.localPosition, m_containerInitialPosition, swipeTweenDuration);
                        return;
                    }

                    m_CurrentMenu++;
                    OnMenuChangeEvent?.Invoke(m_CurrentMenu);
                }
                // Swipe LTR (menu decrement)
                else if (percentage < 0f)
                {
                    newLocation += new Vector2(ItemContainer.rect.width, 0);

                    // Do not toggle the end dragging routine if we in 0'th menu.
                    if (m_CurrentMenu <= 0)
                    {
                        SmoothSwipe(ItemContainer.localPosition, m_containerInitialPosition, swipeTweenDuration);
                        return;
                    }

                    m_CurrentMenu--;
                    OnMenuChangeEvent?.Invoke(m_CurrentMenu);
                }

                // Do swiping to new location
                SmoothSwipe(ItemContainer.localPosition, newLocation, swipeTweenDuration);
                m_containerInitialPosition = newLocation;
            }
            else
            {
                SmoothSwipe(ItemContainer.localPosition, m_containerInitialPosition, swipeTweenDuration);
            }
        }

        #region Mouse Scrolling
        /// <summary>
        /// Current scrolling cooldown before applying <see cref="OnEndDrag(PointerEventData)"/>.
        /// </summary>
        private float m_currentScrollCooldown = 0f;
        /// <summary>
        /// The given scroll delta by the mouse scroll.
        /// </summary>
        private Vector2 m_currentScroll;
        /// <summary>
        /// The current scrolling event data.
        /// </summary>
        private PointerEventData m_scrollEventData;
        private void Update()
        {
            if (m_scrollEventData == null || !IsScrollable)
            {
                return;
            }

            // Tick the scroll cooldown
            // This will only proceed if there's no scroll delta.
            if (m_currentScrollCooldown >= scrollWaitTime)
            {
                m_scrollEventData.position = m_currentScroll;
                OnEndDrag(m_scrollEventData);
                m_scrollEventData = null;
            }

            m_currentScrollCooldown += Time.deltaTime;
        }
        public void OnScroll(PointerEventData data)
        {
            // Disable scroll if no sensitivity
            if (!IsScrollable)
            {
                return;
            }

            if (m_scrollEventData == null)
            {
                m_scrollEventData = data;
                m_currentScroll = data.position;
                m_scrollEventData.pressPosition = data.position;
            }

            m_currentScrollCooldown = 0f;
            m_currentScroll += data.scrollDelta * (scrollSensitivity * 50f);
            m_scrollEventData.position = m_currentScroll;

            OnDrag(m_scrollEventData);
        }
        #endregion

        #region Swipe Tween
        /// <summary>
        /// Currently running <see cref="SmoothSwipe(Vector2, Vector2, float)"/> routine.
        /// </summary>
        private Coroutine m_SmoothSwipeRoutine;
        /// <summary>
        /// Interpolates the swiping transition smoothly.
        /// <br>Manages the <see cref="m_SmoothSwipeRoutine"/> by itself.
        /// If there's already a routine/tween running this method will stop it.</br>
        /// </summary>
        private void SmoothSwipe(Vector2 startPos, Vector2 endPos, float duration)
        {
            if (m_SmoothSwipeRoutine != null)
            {
                StopCoroutine(m_SmoothSwipeRoutine);
            }

            m_SmoothSwipeRoutine = StartCoroutine(SmoothSwipeRoutine(startPos, endPos, duration));
        }

        /// <summary>
        /// Interpolates the swiping transition.
        /// </summary>
        private IEnumerator SmoothSwipeRoutine(Vector2 startPos, Vector2 endPos, float duration)
        {
            float t = 0f;

            while (t <= 1.0)
            {
                t += Time.deltaTime / duration;

                ItemContainer.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));

                yield return null;
            }

            ItemContainer.localPosition = endPos;
            m_SmoothSwipeRoutine = null;
        }
        #endregion

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Color gColor = Gizmos.color;
            RectTransform rTransform = ItemContainer == null ? GetComponent<RectTransform>() : ItemContainer;

            if (m_CurrentMenu >= MenuCount)
            {
                // Show the gizmo on right (According to menu).
                Gizmos.color = Color.green;
                var linePos = new Vector2((rTransform.position.x + ((rTransform.rect.width / 2f) + swipeDragClampLength)) * Mathf.Abs(MenuCount), rTransform.position.y);
                Gizmos.DrawLine(linePos + new Vector2(0f, 100f), linePos - new Vector2(0f, 100f));
            }
            if (m_CurrentMenu <= 0)
            {
                // Show the gizmo on left.
                Gizmos.color = Color.red;
                var linePos = new Vector2(rTransform.position.x - ((rTransform.rect.width / 2f) + swipeDragClampLength), rTransform.position.y);
                Gizmos.DrawLine(linePos + new Vector2(0f, 100f), linePos - new Vector2(0f, 100f));
            }

            Gizmos.color = gColor;
        }
#endif
    }
}
