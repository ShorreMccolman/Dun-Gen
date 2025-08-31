using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupMenu : DoneGenMenu
{
    [SerializeField] TMP_Text Text;
    [SerializeField] TMP_InputField Input;
    [SerializeField] TMP_Text Button;

    System.Action<string> _onPressed;

    public static void Popup(string text, System.Action<string> OnPressed)
    {
        PopupMenu menu = OpenPopup("TextInputPopup") as PopupMenu;

        if (menu)
        {
            menu.Text.text = text;
            menu._onPressed = OnPressed;
            menu.Input.text = "NEW";
        }
    }

    public void ExitPressed()
    {
        Close();
    }

    public void ConfirmPressed()
    {
        _onPressed.Invoke(Input.text);
        Close();
    }
}
