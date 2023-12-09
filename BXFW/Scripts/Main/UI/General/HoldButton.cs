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
        public bool Clickable = true;
        public bool Holdable = true;
        public float HoldTime = .3f;
        [Tooltip("Whether if the holding should ignore Time.timeScale. (Use Time.unscaledDeltaTime)")]
        public bool IgnoreTimeScale = false;

        [Header(":: Events")]
        public ButtonEvent OnClickEvent;
        public ButtonEvent OnHoldEvent;

        private float holdTimer = 0f;       // Timer for hold.
        private bool isPointerDown = false; // Status variable for whether if holding.
        private bool isHoldEvent = false;   // Status variable for not invoking click event on hold event.

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!IsInteractable())
            {
                return;
            }

            base.OnPointerDown(eventData);

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
                if (Clickable)
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
            if (!Holdable)
            {
                return;
            }

            // Hold update
            if (!isPointerDown)
            {
                holdTimer = 0f;
                return;
            }

            holdTimer += IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            if (holdTimer >= HoldTime)
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
