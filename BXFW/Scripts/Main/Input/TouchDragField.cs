using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BXFW
{
    /// <summary>
    /// Touch field with movement delta.
    /// </summary>
    public class TouchDragField : Selectable, IDragHandler
    {
        /// <summary>
        /// Movement delta for the TouchField.
        /// </summary>
        public Vector2 DragDelta { get; private set; }
        /// <summary>
        /// Whether if the drag field is being pressed / dragged.
        /// </summary>
        public bool Pressed { get; private set; }

        public bool UseDragHandler = true;

        [Serializable]
        public sealed class DragEvent : UnityEvent<Vector2> { }
        public DragEvent OnDragField;

        protected int PointerId = 0;
        private Vector2 PointerOld;
        private Touch? currentTouch;

        // Update Movement
        private void Update()
        {
            if (UseDragHandler)
                return;

            if (Pressed)
            {
                // Cache the touch if pressed
                // The pointerID is not expected to change
                if (!currentTouch.HasValue)
                {
                    currentTouch = Input.GetTouch(PointerId);
                }

                if (PointerId >= 0 && PointerId < Input.touchCount)
                {
                    DragDelta = currentTouch.Value.position - PointerOld;
                    PointerOld = currentTouch.Value.position;
                }
                else
                {
                    DragDelta = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - PointerOld;
                    PointerOld = Input.mousePosition;
                }

                if (DragDelta != Vector2.zero)
                    OnDragField?.Invoke(DragDelta);
            }
            else
            {
                currentTouch = null;
                DragDelta = Vector2.zero;
            }
        }

        // Event
        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            Pressed = true;
            PointerId = eventData.pointerId;
            PointerOld = eventData.position;
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            Pressed = false;
            DragDelta = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!UseDragHandler) return;
            if (!IsInteractable()) return;
 
            DragDelta = eventData.delta;
            if (eventData.delta != Vector2.zero)
                OnDragField?.Invoke(eventData.delta);
        }
    }
}