using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoneGenMenu : MonoBehaviour
{
    [SerializeField] string MenuTag;

    GameObject _contents;

    static Dictionary<string, DoneGenMenu> _menuDict = new Dictionary<string, DoneGenMenu>();
    static Stack<DoneGenMenu> _menuStack = new Stack<DoneGenMenu>();

    public bool IsOpen => _contents.activeSelf;
    public static bool IsMenuOpen => _menuStack.Count > 0;

    void Awake()
    {
        _contents = transform.GetChild(0).gameObject;

        if (_contents.activeSelf)
            _contents.SetActive(false);
    }

    void Start()
    {
        if (_menuDict.ContainsKey(MenuTag))
            _menuDict[MenuTag] = this;
        else
            _menuDict.Add(MenuTag, this);

        Init();
    }
    protected virtual void Init() { }

    public void Swap()
    {
        CloseAll();
        Open();
    }

    public void Open()
    {
        if (_menuStack.Count == 0 || _menuStack.Peek() != this)
        {
            _contents.SetActive(true);
            _menuStack.Push(this);
            OnOpen();
        }
    }
    protected virtual void OnOpen() { }
    public void Close()
    {
        while (_menuStack.Peek() != this)
        {
            _menuStack.Peek().Close();
        }

        OnClose();
        _menuStack.Pop();
        _contents.SetActive(false);
    }

    protected virtual void OnClose() { }

    public virtual void Escape()
    {
        Close();
    }


    // Static methods
    public static void OpenMenu(string menuTag)
    {
        if (!_menuDict.ContainsKey(menuTag))
        {
            Debug.LogError("Attempting to open missing menu with tag * " + menuTag + " *");
            return;
        }

        DoneGenMenu menu = _menuDict[menuTag];
        menu.Open();
    }

    public static void SwapMenu(string menuTag)
    {
        CloseAll();
        OpenMenu(menuTag);
    }

    public static DoneGenMenu OpenPopup(string menuTag)
    {
        if (!_menuDict.ContainsKey(menuTag))
        {
            Debug.LogError("Attempting to open missing menu with tag * " + menuTag + " *");
            return null;
        }

        DoneGenMenu menu = _menuDict[menuTag];
        menu.Open();
        return menu;
    }

    public static void EscapeCurrent()
    {
        if (_menuStack.Count > 0)
            _menuStack.Peek().Escape();
    }

    public static void CloseAll()
    {
        while (_menuStack.Count > 0)
        {
            _menuStack.Peek().Close();
        }
    }

    public static bool IsMenuTagOpen(string tag)
    {
        foreach (var menu in _menuStack)
        {
            if (string.Equals(menu.MenuTag, tag))
                return true;
        }
        return false;
    }
}
