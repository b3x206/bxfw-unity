using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
public class UIMenu : MonoBehaviour
{
    [Serializable]
    public sealed class UIMenuEvent_Bool : UnityEvent<bool> { }

    [Serializable]
    public sealed class UIMenuEvent_MenuData : UnityEvent<UIMenu, bool> { }

    [Tooltip("Event sent whenever the ui opens or closes. Passes a bool param.")]
    public UIMenuEvent_Bool ExtraUIEvents_Simple;
    public UIMenuEvent_MenuData ExtraUIEvents;
    public bool AddSetActiveEvent = true;
    public bool IsClosed { get { return gameObject.activeInHierarchy; } }

    private RectTransform _MenuRectTransform;
    public RectTransform RectTransform
    {
        get
        {
            if (_MenuRectTransform == null)
            {
                _MenuRectTransform = GetComponent<RectTransform>();
            }

            return _MenuRectTransform;
        }
    }
    private void Awake()
    {
        if (_MenuRectTransform == null)
        {
            _MenuRectTransform = GetComponent<RectTransform>();
        }
    }

    /// <summary>
    /// <para>Opens menu.</para>
    /// <br>objAction is set to <c>() => { gameObject.SetActive(true); }</c>.</br>
    /// <br>Can be disabled using <see cref="AddSetActiveEvent"/>.</br>
    /// </summary>
    /// <param name="objAction">Action to invoke when the menu is opened.</param>
    public void OpenMenu(Action objAction = null)
    {
        if (AddSetActiveEvent)
        {
            objAction += () => { gameObject.SetActive(true); };
        }

        objAction?.Invoke();
        ExtraUIEvents?.Invoke(this, true);
        ExtraUIEvents_Simple?.Invoke(true);
    }

    /// <summary>
    /// <para>Closes menu.</para>
    /// <br>objAction is set to <c>() => { gameObject.SetActive(false); }</c>.</br>
    /// <br>Can be disabled using <see cref="AddSetActiveEvent"/>.</br>
    /// </summary>
    /// <param name="objAction">Action to invoke when the menu is opened.</param>
    public void CloseMenu(Action objAction = null)
    {
        if (AddSetActiveEvent)
        {
            objAction += () => { gameObject.SetActive(false); };
        }

        objAction?.Invoke();
        ExtraUIEvents?.Invoke(this, false);
        ExtraUIEvents_Simple?.Invoke(false);
    }
}
