using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// <see cref="KeyCode"/> event that can be remapped from inspector.
    /// </summary>
    [Serializable]
    public class CustomInputEvent
    {
        /// <summary>
        /// Type of the InputEvent.
        /// <br>Setting this to <see langword="true"/> requires <see cref="Poll"/> to be called every frame.</br>
        /// <br>By setting this value, you have to check the input on a different update,
        /// as this will keep the last state until one of the <c>IsKey</c> methods are invoked.</br>
        /// </summary>
        public bool isPolled = false;
        public KeyCode[] KeyCodeReq;
        /// <summary>
        /// Bool to set to make <see cref="IsKey"/> or <c>(<see cref="bool"/>)<see langword="this"/></c> return true.
        /// </summary>
        public bool SetIsInvokable { get; set; } = false;
        /// <summary>
        /// Sets the event to be invokable.
        /// <br>Use <b>only</b> with method <see cref="IsKey"/>. (Other methods still poll for the key.)</br>
        /// </summary> 
        public void SetInvokeEvent(bool setValue)
        {
            SetIsInvokable = setValue;
        }

        [Flags]
        public enum EventType
        {
            None = 0,
            IsKey = 1 << 0,
            IsKeyDown = 1 << 1,
            IsKeyUp = 1 << 2,
        }
        private EventType pollCurrentType;
        private KeyCode   pollCurrentKey;
        private bool isKey;

        /// <summary>
        /// Polls event.
        /// <br>Call from <c>Update()</c>. If <see cref="isPolled"/> is <see langword="false"/> then this method does nothing.</br>
        /// </summary>
        public void Poll()
        {
            if (!isPolled)
                return;

            foreach (KeyCode key in KeyCodeReq)
            {
                if (Input.GetKey(key))
                {
                    pollCurrentKey = key;
                    break;
                }
            }

            isKey = pollCurrentKey != KeyCode.None || SetIsInvokable;

            if (isKey)
            {
                switch (pollCurrentType)
                {
                    case EventType.None:
                        pollCurrentType = EventType.IsKeyDown; // only 1 event
                        break;
                    // Pressed for more than 2 frames
                    case EventType.IsKeyDown:
                        pollCurrentType = EventType.IsKeyDown | EventType.IsKey; // 2 events invoked and awaiting to be used.
                        break;
                }
            }
            else
            {
                pollCurrentType = EventType.IsKeyUp; // only 1 event
            }
        }
        /// <summary>
        /// Uses the polled event.
        /// <br>Returns <c>None</c> / default values if <see cref="isPolled"/> is <see langword="false"/>.</br>
        /// <br>Note that after using the event, the event valeus are reset.</br>
        /// </summary>
        public void Use(out EventType t, out KeyCode k)
        {
            t = pollCurrentType;
            k = pollCurrentKey;

            switch (pollCurrentType)
            {
                default:
                    pollCurrentType = EventType.None;
                    break;

                case EventType.IsKeyDown | EventType.IsKey:
                    pollCurrentType = EventType.IsKey; // IsKeyDown event should be now used.
                    break;
                case EventType.IsKey: // do nothing as this event is disabled from Poll()
                    break;
            }

            pollCurrentKey = KeyCode.None;
        }

        public bool IsKey()
        {
            if (isPolled)
            {
                Use(out EventType current, out KeyCode _);
                return (current & EventType.IsKey) == EventType.IsKey;
            }

            bool IsInvokable = false;

            foreach (KeyCode key in KeyCodeReq)
            {
                if (Input.GetKey(key))
                {
                    IsInvokable = true;
                    break;
                }
            }

            return IsInvokable || SetIsInvokable;
        }
        public bool IsKeyDown()
        {
            if (isPolled)
            {
                Use(out EventType current, out KeyCode _);
                return (current & EventType.IsKeyDown) == EventType.IsKeyDown;
            }

            bool IsInvokable = false;

            foreach (KeyCode key in KeyCodeReq)
            {
                if (Input.GetKeyDown(key))
                {
                    IsInvokable = true;
                    break;
                }
            }

            return IsInvokable;
        }
        public bool IsKeyUp()
        {
            if (isPolled)
            {
                Use(out EventType current, out KeyCode _);
                return (current & EventType.IsKeyUp) == EventType.IsKeyUp;
            }

            bool IsInvokable = false;

            foreach (KeyCode key in KeyCodeReq)
            {
                if (Input.GetKeyUp(key))
                {
                    IsInvokable = true;
                    break;
                }
            }

            return IsInvokable;
        }

        public CustomInputEvent()
        { }
        /// <summary>
        /// Creates a CustomInputEvent with <see cref="KeyCode"/>s already assigned.
        /// </summary>
        public CustomInputEvent(KeyCode[] kCodes)
        {
            KeyCodeReq = kCodes;
        }
        public CustomInputEvent(bool polled, KeyCode[] kCodes) : this(kCodes)
        {
            isPolled = polled;
        }
        public static implicit operator bool(CustomInputEvent iEvent)
        {
            return iEvent.IsKey();
        }
        public static implicit operator CustomInputEvent(KeyCode[] KeyCodes)
        {
            return new CustomInputEvent(KeyCodes);
        }

        public static bool operator ==(CustomInputEvent lhs, CustomInputEvent rhs)
        {
            if (Equals(lhs, null))
                return Equals(rhs, null);

            return lhs.Equals(rhs);
        }
        public static bool operator !=(CustomInputEvent lhs, CustomInputEvent rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            if (obj is CustomInputEvent e)
            {
                // Check if arrays are equal
                return Enumerable.SequenceEqual(KeyCodeReq, e.KeyCodeReq);
            }

            return false;
        }
        public override string ToString()
        {
            // Inefficient, but this method should only be used for debug.
            string retString = string.Empty;
            for (int i = 0; i < KeyCodeReq.Length; i++)
            {
                var strAdd = $"{KeyCodeReq[i]}";

                if (i != KeyCodeReq.Length - 1)
                {
                    strAdd += ", ";
                }

                retString += strAdd;
            }

            return retString;
        }

        public override int GetHashCode()
        {
            // Ignore it saying use System.HashCode.Combine
            // It doesn't exist in all versions of unity
            int hashCode = 1734127663;
            hashCode = (hashCode * -1521134295) + EqualityComparer<KeyCode[]>.Default.GetHashCode(KeyCodeReq);
            hashCode = (hashCode * -1521134295) + SetIsInvokable.GetHashCode();
            return hashCode;
        }
    }
}