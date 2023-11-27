using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// Handles input operations in editor.
    /// <br>Create a 'EditorInput' object on the thing you want to capture input on, and call <see cref="PollEvents"/> on the suitable method.</br>
    /// </summary>
    public class EditorInput
    {
        // Since all Event related accesses are read-only for this class, no need to call Event.Use
        // But this class is kinda dumb on it's concept so idk what to do about it.
        // Ah well it's probably fine.

        // 'GetKey' / Poll related inputs.
        private readonly HashSet<KeyCode> editorInputBuffer = new HashSet<KeyCode>();
     
        private static string GetWarningEventCurrentNull([CallerMemberName] string methodName = "<unknown method>")
        {
            return string.Format("[EditorInput::{0}] Called method while the 'Event.current' is null. Only call this from input polled from 'OnGUI'.", methodName);
        }

        public void PollEvents()
        {
            // This is for the 'GetKey' methods without down or up events.
            // editorInputBuffer should only contain unique values instead (that's why it's an hashset)
            // It will be iterated every EditorApplication globalEventHandler
            if (Event.current == null)
            {
                Debug.LogWarning(GetWarningEventCurrentNull());
                return;
            }

            switch (Event.current.type)
            {
                default:
                    break;

                case EventType.MouseDown:
                    // Convert mouse -> KeyCode
                    editorInputBuffer.Add((KeyCode)((int)KeyCode.Mouse0 + Event.current.button));
                    break;
                case EventType.KeyDown:
                    editorInputBuffer.Add(Event.current.keyCode);
                    break;

                case EventType.MouseUp:
                    editorInputBuffer.Remove((KeyCode)((int)KeyCode.Mouse0 + Event.current.button));
                    break;
                case EventType.KeyUp:
                    editorInputBuffer.Remove(Event.current.keyCode);
                    break;
            }
        }

        /// <summary>
        /// Delta of the mouse.
        /// </summary>
        public Vector2 mouseDelta
        {
            get
            {
                if (Event.current == null)
                {
                    Debug.LogWarning(GetWarningEventCurrentNull());
                    return Vector2.zero;
                }

                return Event.current.delta;
            }
        }
        /// <summary>
        /// Absolute mouse position on the editor.
        /// <br>If you are calling this from OnGUI, the position is already converted to your gui, no need to convert it.</br>
        /// <br>Use <see cref="GUIUtility.GUIToScreenPoint(Vector2)"/> to convert it to your gui.</br>
        /// </summary>
        public Vector2 mousePosition
        {
            get
            {
                if (Event.current == null)
                {
                    Debug.LogWarning(GetWarningEventCurrentNull());
                    return Vector2.zero;
                }

                return Event.current.mousePosition;
            }
        }
        /// <summary>
        /// Whether if the mouse is dragged.
        /// </summary>
        public bool isMouseDrag
        {
            get
            {
                if (Event.current == null)
                {
                    Debug.LogWarning(GetWarningEventCurrentNull());
                    return false;
                }

                return Event.current.type == EventType.MouseDrag;
            }
        }

        /// <summary>
        /// Returns whether if any of the keys are held.
        /// </summary>
        public bool anyKey
        {
            get
            {
                return editorInputBuffer.Count != 0;
            }
        }
        /// <summary>
        /// Returns whether if any of the keys were pressed down on this editor frame. (Current event)
        /// </summary>
        public bool anyKeyDown
        {
            get
            {
                if (Event.current == null)
                {
                    Debug.LogWarning(GetWarningEventCurrentNull());
                    return false;
                }

                return Event.current.type == EventType.MouseDown || Event.current.type == EventType.KeyDown;
            }
        }
        /// <summary>
        /// Mouse scroller delta.
        /// </summary>
        public Vector2 scrollDelta
        {
            get
            {
                if (Event.current == null)
                {
                    Debug.LogWarning(GetWarningEventCurrentNull());
                    return Vector2.zero;
                }

                return Event.current.type == EventType.ScrollWheel ? Event.current.delta : Vector2.zero;
            }
        }

        /// <summary>
        /// Returns whether if the key is held.
        /// </summary>
        public bool GetKey(KeyCode key)
        {
            return editorInputBuffer.Contains(key);
        }

        /// <summary>
        /// Returns whether if the key is down.
        /// <br>Unlike <see cref="GetKey(KeyCode)"/>, this invokes once.</br>
        /// </summary>
        public bool GetKeyDown(KeyCode key)
        {
            if (Event.current == null)
            {
                Debug.LogWarning(GetWarningEventCurrentNull());
                return false;
            }

            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    return (KeyCode)((int)KeyCode.Mouse0 + Event.current.button) == key;
                case EventType.KeyDown:
                    return Event.current.keyCode == key;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns whether if the key was up.
        /// <br>Unlike <see cref="GetKey(KeyCode)"/>, this invokes once.</br>
        /// </summary>
        public bool GetKeyUp(KeyCode key)
        {
            if (Event.current == null)
            {
                Debug.LogWarning(GetWarningEventCurrentNull());
                return false;
            }

            switch (Event.current.type)
            {
                case EventType.MouseUp:
                    return (KeyCode)((int)KeyCode.Mouse0 + Event.current.button) == key;
                case EventType.KeyUp:
                    return Event.current.keyCode == key;

                default:
                    return false;
            }
        }
    }
}
