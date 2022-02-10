using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The sub ui menu.
/// <br>Restrictions that apply to <see cref="UIMenu"/> doesn't apply to this sub menu.</br>
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UISubMenu : MonoBehaviour
{
    [Serializable]
    public sealed class UISubMenuEvent_Bool : UnityEvent<bool> { }
    public UISubMenuEvent_Bool UISubMenuEventSimple;
    [Serializable]
    public sealed class UISubMenuEvent_MenuData : UnityEvent<UISubMenu, bool> { }
    public UISubMenuEvent_MenuData UISubMenuEvent;

    private RectTransform _MenuRectTransform;
    public RectTransform MenuRectTransform
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
    public bool AddSetActiveEvent = true;
    [SerializeField] private bool _IsClosedOnStart = true;
    public bool IsClosedOnStart { get { return _IsClosedOnStart; } }

    private void Awake()
    {
        if (_MenuRectTransform == null)
        {
            _MenuRectTransform = GetComponent<RectTransform>();
        }

        if (gameObject.activeSelf)
        {
            UISubMenuEventSimple?.Invoke(!IsClosedOnStart);
            UISubMenuEvent?.Invoke(this, !IsClosedOnStart);
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
        UISubMenuEventSimple?.Invoke(true);
        UISubMenuEvent?.Invoke(this, true);
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
        UISubMenuEventSimple?.Invoke(false);
        UISubMenuEvent?.Invoke(this, false);
    }
}