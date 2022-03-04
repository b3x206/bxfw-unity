using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BXFW
{
    /// <summary>
    /// Touch field with movement delta.
    /// </summary>
    public class TouchDragField : Selectable
    {
        /// <summary> Movement delta for the TouchField. </summary>
        public Vector2 DragDelta { get; private set; }
        /// <summary> Whether if the drag field is being dragged. </summary>
        public bool Pressed { get; private set; }

        protected int PointerId;
        private Vector2 PointerOld;

        // Update Movement
        private void Update()
        {
            if (Pressed)
            {
                if (PointerId >= 0 && PointerId < Input.touches.Length)
                {
                    DragDelta = Input.touches[PointerId].position - PointerOld;
                    PointerOld = Input.touches[PointerId].position;
                }
                else
                {
                    DragDelta = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - PointerOld;
                    PointerOld = Input.mousePosition;
                }
            }
            else
            {
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
        }
    }
}