using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;
using UnityEditor;

public class DemoSettingsMenu : DoneGenMenu
{
    [SerializeField] TMP_Dropdown DungeonStyle;

    [SerializeField] TMP_InputField XDimension;
    [SerializeField] TMP_InputField YDimension;

    [SerializeField] RangeSlider RoomCount;

    [SerializeField] Toggle Shoot;
    [SerializeField] Toggle Snake;
    [SerializeField] Toggle Bridge;
    [SerializeField] Toggle Crank;

    [SerializeField] DunGen.MapGenerator Generator;

    GenerationSettings _defaults;

    protected override void Init()
    {
        DoneGenSettingsData data = Resources.Load<DoneGenSettingsData>("Settings/Default");

        if(data == null)
        {
            Debug.LogError("Could not find default generation settings!!");
        }
        else
        {
            _defaults = data.Settings;
        }
    }

    protected override void OnOpen()
    {
        RestoreDefaults();
    }

    public void RestoreDefaults()
    {
        DungeonStyle.value = (int)_defaults.GameStyle;
        XDimension.text = _defaults.GridWidth.ToString();
        YDimension.text = _defaults.GridHeight.ToString();
        RoomCount.LowValue = _defaults.PrimaryRooms.Min;
        RoomCount.HighValue = _defaults.PrimaryRooms.Max;

        List<DunGen.EBranchType> types = new List<DunGen.EBranchType>(_defaults.BranchTypes);
        Shoot.isOn = types.Contains(DunGen.EBranchType.Shoot);
        Snake.isOn = types.Contains(DunGen.EBranchType.Snake);
        Bridge.isOn = types.Contains(DunGen.EBranchType.Bridge);
        Crank.isOn = types.Contains(DunGen.EBranchType.Crank);
    }

    public void PreviewCurrentSettings()
    {
        List<DunGen.EBranchType> branchTypes = new List<DunGen.EBranchType>();
        if (Shoot.isOn)
            branchTypes.Add(DunGen.EBranchType.Shoot);
        if (Snake.isOn)
            branchTypes.Add(DunGen.EBranchType.Snake);
        if (Bridge.isOn)
            branchTypes.Add(DunGen.EBranchType.Bridge);
        if (Crank.isOn)
            branchTypes.Add(DunGen.EBranchType.Crank);

        GenerationSettings settings = new GenerationSettings()
        {
            GameStyle = (EGameStyle)DungeonStyle.value,
            GridWidth = int.Parse(XDimension.text),
            GridHeight = int.Parse(YDimension.text),
            PrimaryRooms = new ValueRange((int)RoomCount.LowValue, (int)RoomCount.HighValue),
            BranchTypes = branchTypes.ToArray()
        };

        DunGen.MapData mapData = Generator.GenerateInstant(settings);
    }

    public void SaveCurrentSettings()
    {
#if UNITY_EDITOR
        string text = "Save current settings to scriptable object? Will be found at 'Assets/Resources/Settings/[filename].asset'";
#else
        string text = "This feature is not available in builds, please run in editor to s";
#endif

        PopupMenu.Popup(text, SaveAs);
    }

    void SaveAs(string fileName)
    {
#if UNITY_EDITOR
        List<DunGen.EBranchType> branchTypes = new List<DunGen.EBranchType>();
        if (Shoot.isOn)
            branchTypes.Add(DunGen.EBranchType.Shoot);
        if (Snake.isOn)
            branchTypes.Add(DunGen.EBranchType.Snake);
        if (Bridge.isOn)
            branchTypes.Add(DunGen.EBranchType.Bridge);
        if (Crank.isOn)
            branchTypes.Add(DunGen.EBranchType.Crank);

        GenerationSettings settings = new GenerationSettings()
        {
            GameStyle = (EGameStyle)DungeonStyle.value,
            GridWidth = int.Parse(XDimension.text),
            GridHeight = int.Parse(YDimension.text),
            PrimaryRooms = new ValueRange((int)RoomCount.LowValue, (int)RoomCount.HighValue),
            BranchTypes = branchTypes.ToArray()
        };

        DoneGenSettingsData data = ScriptableObject.CreateInstance<DoneGenSettingsData>();
        data.SettingsName = fileName;
        data.Settings = settings;

        AssetDatabase.CreateAsset(data, "Assets/Resources/Settings/" + fileName + ".asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
    }
}
