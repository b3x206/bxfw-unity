using UnityEngine;

[System.Serializable]
public class CustomInputEvent
{
    public KeyCode[] KeyCodeReq;

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