using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BXFW.UI
{
    /// <summary>
    /// UI <see cref="Selectable"/> that also invokes an event when held down.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class HoldButton : Selectable
    {
        [System.Serializable]
        public class ButtonEvent : UnityEvent { }

        [Header(":: Settings")]
        public bool clickable = true;
        public bool holdable = true;
        public float holdTime = .3f;
        [Tooltip("Whether if the holding should ignore Time.timeScale. (Use Time.unscaledDeltaTime)")]
        public bool ignoreTimeScale = false;

        /// <summary>
        /// Called when the button was clicked fully (OnPointerUp usually).
        /// </summary>
        [Header(":: Events")]
        [DrawIf(nameof(clickable))] public ButtonEvent OnClickEvent;
        /// <summary>
        /// Called when the pointer is pressed down.
        /// <br>Useful for determining when the button was interacted with.</br>
        /// </summary>
        [DrawIf(nameof(clickable))] public ButtonEvent OnPressDownEvent;
        /// <summary>
        /// Called when the button was held for <see cref="holdTime"/>.
        /// </summary>
        [DrawIf(nameof(holdable))] public ButtonEvent OnHoldEvent;

        private float currentHoldTimer = 0f; // Timer for hold.
        private bool isPointerDown = false;  // Status variable for whether if holding.
        private bool isHoldEvent = false;    // Status variable for not invoking click event on hold event.

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!IsInteractable())
            {
                return;
            }

            base.OnPointerDown(eventData);

            if (clickable)
            {
                OnPressDownEvent?.Invoke();
            }

            isPointerDown = true;
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!IsInteractable())
            {
                return;
            }

            base.OnPointerUp(eventData);

            isPointerDown = false;

            if (!isHoldEvent)
            {
                if (clickable)
                {
                    OnClickEvent?.Invoke();
                }
            }
            else
            {
                isHoldEvent = false;
            }
        }

        // Tick hold timer
        private void Update()
        {
            if (!holdable)
            {
                return;
            }

            // Hold update
            if (!isPointerDown)
            {
                currentHoldTimer = 0f;
                return;
            }

            currentHoldTimer += ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            if (currentHoldTimer >= holdTime)
            {
                OnHoldEvent?.Invoke();

                // Set these to end the hold state
                isPointerDown = false;
                isHoldEvent = true;

                // Don't simulate 'OnPointerUp', it gets called when the pointer is actually up.
            }
        }
    }
}
