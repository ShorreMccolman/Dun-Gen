using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DunGen;

public class DemoToolMenu : DoneGenMenu
{
    protected override void Init()
    {
        Open();
    }

    public void FPDungeonPressed()
    {
        GenerationSettings settings = new GenerationSettings()
        {
            GameStyle = EGameStyle.FirstPerson,
            GridWidth = 36,
            GridHeight = 20,
            PrimaryRooms = new ValueRange(25, 35)
        };

        DungeonBuilder.Instance.BuildAndLaunch(settings);
        Close();
    }

    public void TDDungeonPressed()
    {
        GenerationSettings settings = new GenerationSettings()
        {
            GameStyle = EGameStyle.TopDown,
        };

        DungeonBuilder.Instance.BuildAndLaunch(settings);
        Close();
    }

    public void ControlPanelPressed()
    {
        SwapMenu("ControlPanel");
    }

    public void QuitToDesktopPressed()
    {
        Application.Quit();
    }
}