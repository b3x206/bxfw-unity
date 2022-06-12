using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// <see cref="KeyCode"/> event that can be remapped from inspector.
    /// </summary>
    [System.Serializable]
    public class CustomInputEvent
    {
        public KeyCode[] KeyCodeReq;
        /// <summary>
        /// Bool to set to make <see cref="IsKey"/> or <c><see cref="(bool)this"/></c> return true.
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

        public bool IsKey()
        {
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
        public CustomInputEvent(KeyCode[] kCodes)
        {
            KeyCodeReq = kCodes;
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

            return base.Equals(obj);
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
            int hashCode = 1734127663;
            hashCode = hashCode * -1521134295 + EqualityComparer<KeyCode[]>.Default.GetHashCode(KeyCodeReq);
            hashCode = hashCode * -1521134295 + SetIsInvokable.GetHashCode();
            return hashCode;
        }
    }
}