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
        // Since all Event related accesses are read-only, no need to call Event.Use
        // Though i really don't know what 'Event.Use' is used for
        // Oh well, this is probably fine.

        // 'GetKey' / Poll related inputs.
        private readonly HashSet<KeyCode> editorInputBuffer = new HashSet<KeyCode>();
     
        private static string GetWarn_EventCurrentNull([CallerMemberName] string methodName = "<undefined_method>")
        {
            return string.Format("[EditorInput::{0}] Called method while the 'Event.current' is null. Only call this from input polled from 'OnGUI'.", methodName);
        }

        // was planning to use a static event thing, but the unity 'globalEventHandler' doesn't capture mouse clicks
        // Maybe i could hook the entire unity window (or all created windows) but that won't work at all.
        //static EditorInput()
        //{
        //    // No events on the EditorApplication.update
        //    // So unity moment

        //    System.Reflection.FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler", System.Reflection.BindingFlags. | System.Reflection.BindingFlags.NonPublic);
        //    EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction)info.GetValue(null);
        //    value += PollEvents;
        //    info.SetValue(null, value);
        //}
        public void PollEvents()
        {
            // This is for the 'GetKey' methods without down or up events.
            // editorInputBuffer should only contain unique values instead (that's why it's an hashset)
            // It will be iterated every EditorApplication globalEventHandler
            if (Event.current == null)
            {
                Debug.LogWarning(GetWarn_EventCurrentNull());
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
                    Debug.LogWarning(GetWarn_EventCurrentNull());
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
                    Debug.LogWarning(GetWarn_EventCurrentNull());
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
                    Debug.LogWarning(GetWarn_EventCurrentNull());
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
                    Debug.LogWarning(GetWarn_EventCurrentNull());
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
                    Debug.LogWarning(GetWarn_EventCurrentNull());
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
                Debug.LogWarning(GetWarn_EventCurrentNull());
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
                Debug.LogWarning(GetWarn_EventCurrentNull());
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
