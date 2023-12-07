using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BXFW
{
    /// <summary>
    /// <see cref="KeyCode"/> event that can be remapped from inspector.
    /// </summary>
    /// <br>FIXME / TODO : Polling does not work on lower frame rates.</br>
    [Serializable]
    public class CustomInputEvent : IEquatable<CustomInputEvent>
    {
        /// <summary>
        /// Type of the InputEvent.
        /// <br>Setting this to <see langword="true"/> requires <see cref="Poll"/> to be called every frame.</br>
        /// <br>By setting this value, you have to check the input on a different update,
        /// as this will keep the last state until one of the <c>IsKey</c> methods are invoked.</br>
        /// </summary>
        public bool isPolled = false;
        /// <summary>
        /// List of the keys that this event listens for.
        /// </summary>
        [SearchableKeyCodeField, FormerlySerializedAs("KeyCodeReq")]
        public List<KeyCode> keyBinds;

        [Flags]
        public enum InputEventType
        {
            None = 0,
            IsKey = 1 << 0,
            IsKeyDown = 1 << 1,
            IsKeyUp = 1 << 2,
        }
        private InputEventType m_PollCurrentType;
        private InputEventType PollCurrentType 
        { 
            get { return m_PollCurrentType; } 
            set
            {
                m_PollCurrentType = value;
                pollTime = Time.time;
            }
        }
        private KeyCode pollCurrentKey;
        private float   pollTime;

        // The previous version of this was a hack written for 'Fall Xtra'
        // This CustomInputEvent now allows external fake inputs
        // Note that this is completely unrelated to polling (but technically this is also polled)
        private InputEventType sentInputType = InputEventType.None;
        /// <summary>
        /// Sends an event to this CustomInputEvent.
        /// <br>Can be used to do movement using the unity UI or faking inputs.</br>
        /// </summary>
        /// <param name="type">
        /// Type of the input to send. The appopriate method (after using the InputEvent) will return true.
        /// Can be bitwise combined to allow multiple key events to return true, but the first method checking for the sent input event will always use the given type.
        /// <br>If this type is <see cref="InputEventType.IsKey"/>, this event will be persistent until you call this method with <see cref="InputEventType.None"/>.</br>
        /// </param>
        public void SendEvent(InputEventType type)
        {
            sentInputType = type;
        }

        /// <summary>
        /// Polls event.
        /// <br>Call from <c>Update()</c>. If <see cref="isPolled"/> is <see langword="false"/> then this method does nothing.</br>
        /// </summary>
        public void Poll()
        {
            if (!isPolled)
            {
                return;
            }

            foreach (KeyCode key in keyBinds)
            {
                if (Input.GetKey(key))
                {
                    pollCurrentKey = key;
                    break;
                }
            }

            bool isKey = pollCurrentKey != KeyCode.None;

            if (isKey)
            {
                switch (PollCurrentType)
                {
                    case InputEventType.None:
                        PollCurrentType = InputEventType.IsKeyDown; // only 1 event
                        break;
                    // Pressed for more than 2 frames
                    case InputEventType.IsKeyDown:
                        PollCurrentType = InputEventType.IsKeyDown | InputEventType.IsKey; // 2 events invoked and awaiting to be used.
                        break;
                }
            }
            else
            {
                PollCurrentType = InputEventType.IsKeyUp; // only 1 event
            }
        }
        /// <summary>
        /// Uses the polled event.
        /// <br>Returns <c>None &amp; 0f</c> / default values if <see cref="isPolled"/> is <see langword="false"/>.</br>
        /// <br>Note that after using the event, the event values are reset.</br>
        /// </summary>
        public void Use(out InputEventType type, out KeyCode key, out float time)
        {
            type = PollCurrentType;
            key  = pollCurrentKey;
            time = pollTime;

            switch (PollCurrentType)
            {
                case InputEventType.None:
                    if (!isPolled)
                    {
                        Debug.LogWarning("[CustomKeyEvent::Use] Called use even though this event isn't polled. Set 'isPolled' to true to fix this.");
                    }

                    break;

                default:
                    PollCurrentType = InputEventType.None;
                    break;

                case InputEventType.IsKeyDown | InputEventType.IsKey:
                    PollCurrentType = InputEventType.IsKey; // IsKeyDown event should be now used.
                    break;
                case InputEventType.IsKey: // do nothing as this event is disabled from Poll()
                    break;
            }

            pollCurrentKey = KeyCode.None;
        }
        /// <summary>
        /// Uses the polled event.
        /// <br>Returns <c><see cref="InputEventType.None"/> &amp; <see cref="KeyCode.None"/></c> / 
        /// default values if <see cref="isPolled"/> is <see langword="false"/>.</br>
        /// <br>Note that after using the event, the event values are reset.</br>
        /// </summary>
        public void Use(out InputEventType type, out KeyCode key)
        {
            Use(out type, out key, out float _);
        }
        /// <summary>
        /// Uses the polled event.
        /// <br>Returns <c><see cref="InputEventType.None"/></c> / default values if <see cref="isPolled"/> is <see langword="false"/>.</br>
        /// <br>Note that after using the event, the event values are reset.</br>
        /// </summary>
        public void Use(out InputEventType type)
        {
            Use(out type, out KeyCode _, out float _);
        }

        public bool IsKey()
        {
            if (sentInputType != InputEventType.None)
            {
                bool result = (sentInputType & InputEventType.IsKey) == sentInputType;
                // Persistent 'IsKey' event, don't reset
                if (!result)
                {
                    // Input got consumed because it's not persistent
                    sentInputType = InputEventType.None;
                }
                return result;
            }

            if (isPolled)
            {
                Use(out InputEventType current);
                return (current & InputEventType.IsKey) == InputEventType.IsKey;
            }

            bool IsInvokable = false;

            foreach (KeyCode key in keyBinds)
            {
                if (Input.GetKey(key))
                {
                    IsInvokable = true;
                    break;
                }
            }

            return IsInvokable;
        }
        public bool IsKeyDown()
        {
            if (sentInputType != InputEventType.None)
            {
                bool result = (sentInputType & InputEventType.IsKeyDown) == sentInputType;
                sentInputType = InputEventType.None;
                return result;
            }

            if (isPolled)
            {
                Use(out InputEventType current);
                return (current & InputEventType.IsKeyDown) == InputEventType.IsKeyDown;
            }

            bool isInvokable = false;
            foreach (KeyCode key in keyBinds)
            {
                if (Input.GetKeyDown(key))
                {
                    isInvokable = true;
                    break;
                }
            }

            return isInvokable;
        }
        public bool IsKeyUp()
        {
            if (sentInputType != InputEventType.None)
            {
                bool result = (sentInputType & InputEventType.IsKeyUp) == sentInputType;
                sentInputType = InputEventType.None;
                return result;
            }

            if (isPolled)
            {
                Use(out InputEventType current);
                return (current & InputEventType.IsKeyUp) == InputEventType.IsKeyUp;
            }

            // -- Non-polled
            bool isInvokable = false;
            foreach (KeyCode key in keyBinds)
            {
                if (Input.GetKeyUp(key))
                {
                    isInvokable = true;
                    break;
                }
            }

            return isInvokable;
        }

        /// <summary>
        /// Creates a blank CustomInputEvent.
        /// </summary>
        public CustomInputEvent()
        {
            keyBinds = new List<KeyCode>();
        }
        /// <summary>
        /// Creates a CustomInputEvent with <see cref="KeyCode"/>s assigned.
        /// </summary>
        /// <param name="keyCodes"></param>
        /// <exception cref="ArgumentNullException"/>
        public CustomInputEvent(List<KeyCode> keyCodes)
        {
            if (keyCodes == null)
            {
                throw new ArgumentNullException(nameof(keyCodes), "[CustomInputEvent::ctor()] Given List<KeyCode> parameter is null.");
            }

            // For a participation trophy i added this ctor, enjoy (if you absolutely need 10us more performance)
            // This class will migrate to a 'List<KeyCode>' because then now you can add new keys from code.
            keyBinds = keyCodes;
        }
        /// <summary>
        /// Creates a CustomInputEvent with <see cref="KeyCode"/>s assigned.
        /// </summary>
        /// <param name="keyCodes"></param>
        /// <exception cref="ArgumentNullException"/>
        public CustomInputEvent(IList<KeyCode> keyCodes)
        {
            if (keyCodes == null)
            {
                throw new ArgumentNullException(nameof(keyCodes), "[CustomInputEvent::ctor()] Given 'keyCodes' parameter is null.");
            }

            // While casting to 'List<KeyCode>' would have been faster, this method is way more type safe.
            // This class is meant to be constructed in the inspector anyways (or a MonoBehaviour ctor)
            // In a case of a MonoBehaviour ctor, it will be worse but still bearable as
            // 'CustomInputEvent' is meant to be attached to stuff like the main player (the input giver)
            // + IEnumerable isn't that big of a performance penalty, unless you add the entire KeyCode list
            // If this causes issues in the future, know that this is because i was lazy
            keyBinds = new List<KeyCode>(keyCodes);
        }
        /// <summary>
        /// Creates a CustomInputEvent with <see cref="KeyCode"/>s assigned.
        /// <br>Can define whether if this event is polled by default or not.</br>
        /// </summary>
        public CustomInputEvent(bool polled, IList<KeyCode> keyCodes) : this(keyCodes)
        {
            isPolled = polled;
        }

        public static implicit operator bool(CustomInputEvent iEvent)
        {
            return iEvent.IsKey();
        }
        // Interfaces does not support conversion 'operator'
        // So for the time being use the definite constructors.
        public static implicit operator CustomInputEvent(KeyCode[] keyCodes)
        {
            return new CustomInputEvent(keyCodes);
        }
        public static implicit operator CustomInputEvent(List<KeyCode> keyCodes)
        {
            return new CustomInputEvent(keyCodes);
        }

        public static bool operator ==(CustomInputEvent lhs, CustomInputEvent rhs)
        {
            if (Equals(lhs, null))
            {
                return Equals(rhs, null);
            }

            return lhs.Equals(rhs);
        }
        public static bool operator !=(CustomInputEvent lhs, CustomInputEvent rhs)
        {
            return !(lhs == rhs);
        }

        public bool Equals(CustomInputEvent other)
        {
            if (other is null)
            {
                return false;
            }

            return Enumerable.SequenceEqual(keyBinds, other.keyBinds);
        }
        public override bool Equals(object obj)
        {
            if (obj is CustomInputEvent e)
            {
                return Equals(e);
            }

            return false;
        }
        public override string ToString()
        {
            // Average 'KeyCode' key name string length : 4.157131960335621
            StringBuilder sb = new StringBuilder(keyBinds.Count * 4);
            for (int i = 0; i < keyBinds.Count; i++)
            {
                sb.Append($"{keyBinds[i]}");

                if (i != keyBinds.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            return sb.ToString();
        }
        public override int GetHashCode()
        {
            int hashCode = 1734127663;
            hashCode = (hashCode * -1521134295) + EqualityComparer<List<KeyCode>>.Default.GetHashCode(keyBinds);
            return hashCode;
        }
    }
}