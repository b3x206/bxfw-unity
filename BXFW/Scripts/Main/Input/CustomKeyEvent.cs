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
            bool IsInvokable = SetIsInvokable;

            foreach (KeyCode key in KeyCodeReq)
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
    }
}