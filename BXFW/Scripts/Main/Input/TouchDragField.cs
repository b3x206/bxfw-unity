using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BXFW
{
    /// <summary>
    /// Touch field with movement delta.
    /// <br>(basically acts like a trackpad)</br>
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
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
        /// <summary>
        /// Whether to use the <see cref="IDragHandler"/>'s callback that this class implements.
        /// <br>If this is false, this will use <see cref="Input.GetTouch(int)"/> with the <see cref="IPointerDownHandler"/>'s callback pointerID.</br>
        /// <br/>
        /// <br>Basically set this <see langword="true"/> if you are using the new input system or other input system you use.</br>
        /// </summary>
        public bool useDragHandler = true;

        [Serializable]
        public sealed class DragEvent : UnityEvent<Vector2> { }
        /// <summary>
        /// Event called when the drag field was dragged on.
        /// <br>The parameter is the <see cref="DragDelta"/>.</br>
        /// </summary>
        public DragEvent OnDragField;

        protected int m_pointerId = 0;
        private Vector2 m_pointerOld;
        private Touch? m_currentTouch;

        // Update Movement
        private void Update()
        {
            if (useDragHandler)
            {
                return;
            }

            if (Pressed)
            {
                // Cache the touch if pressed
                // The pointerID is not expected to change
                if (!m_currentTouch.HasValue)
                {
                    m_currentTouch = Input.GetTouch(m_pointerId);
                }

                if (m_pointerId >= 0 && m_pointerId < Input.touchCount)
                {
                    DragDelta = m_currentTouch.Value.position - m_pointerOld;
                    m_pointerOld = m_currentTouch.Value.position;
                }
                else
                {
                    DragDelta = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - m_pointerOld;
                    m_pointerOld = Input.mousePosition;
                }

                if (DragDelta != Vector2.zero)
                {
                    OnDragField?.Invoke(DragDelta);
                }
            }
            else
            {
                m_currentTouch = null;
                DragDelta = Vector2.zero;
            }
        }

        // Event
        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!IsInteractable())
            {
                return;
            }

            Pressed = true;
            m_pointerId = eventData.pointerId;
            m_pointerOld = eventData.position;
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!IsInteractable())
            {
                return;
            }

            Pressed = false;
            DragDelta = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!useDragHandler || !IsInteractable())
            {
                return;
            }

            DragDelta = eventData.delta;
            if (eventData.delta != Vector2.zero)
            {
                OnDragField?.Invoke(eventData.delta);
            }
        }
    }
}