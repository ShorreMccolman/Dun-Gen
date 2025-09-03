using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DunGen;

public class DemoToolMenu : DoneGenMenu
{
    [SerializeField] DoneGenSettingsData CurrentSettings;

    protected override void Init()
    {
        Open();
    }

    public void FPDungeonPressed()
    {
        DungeonBuilder.Instance.BuildAndLaunch(GetSettings());
        Close();
    }

    public void TDDungeonPressed()
    {
        GenerationSettings settings = new GenerationSettings()
        {
            GameStyle = EGameStyle.TopDown,
        };

        DungeonBuilder.Instance.BuildAndLaunch(GetSettings());
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

    GenerationSettings GetSettings()
    {
        if (CurrentSettings == null)
        {
            GenerationSettings settings = new GenerationSettings()
            {
                GameStyle = EGameStyle.FirstPerson,
                GridWidth = 36,
                GridHeight = 20,
                PrimaryRooms = new ValueRange(25, 35),
                BranchTypes = new EBranchType[] { EBranchType.Shoot, EBranchType.Snake, EBranchType.Bridge, EBranchType.Crank }
            };
            return settings;
        }
        else
        {
            return CurrentSettings.Settings;
        }
    }
}