using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;
using UnityEditor;
using DunGen;

public class DemoSettingsMenu : DoneGenMenu
{
    [SerializeField] TMP_Dropdown TileSetOptions;
    [SerializeField] TMP_Dropdown DungeonStyleOptions;
    [SerializeField] TMP_Dropdown DistributionOptions;

    [SerializeField] TMP_InputField XDimension;
    [SerializeField] TMP_InputField YDimension;

    [SerializeField] RangeSlider RoomCount;

    [SerializeField] Toggle Shoot;
    [SerializeField] Toggle Snake;
    [SerializeField] Toggle Bridge;
    [SerializeField] Toggle Crank;

    [SerializeField] Button LaunchFromPreviewButton;

    [SerializeField] MapGenerator Generator;

    GenerationSettings _defaults;

    MapData _previewData;
    GenerationSettings _previewSettings;

    DungeonTileData[] _loadedTileSets;

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

        _loadedTileSets = Resources.LoadAll<DungeonTileData>("DungeonTiles");
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        foreach(var tileSet in _loadedTileSets)
        {
            options.Add(new TMP_Dropdown.OptionData(tileSet.name));
        }
        TileSetOptions.options = options;

        List<TMP_Dropdown.OptionData> distros = new List<TMP_Dropdown.OptionData>();
        distros.Add(new TMP_Dropdown.OptionData("Fully Random"));
        distros.Add(new TMP_Dropdown.OptionData("Evenly Spread"));
        DistributionOptions.options = distros;
    }

    protected override void OnOpen()
    {
        LaunchFromPreviewButton.enabled = false;
        RestoreDefaults();
    }

    public void RestoreDefaults()
    {
        DungeonStyleOptions.value = (int)_defaults.GameStyle;
        XDimension.text = _defaults.GridWidth.ToString();
        YDimension.text = _defaults.GridHeight.ToString();
        RoomCount.LowValue = _defaults.PrimaryRooms.Min;
        RoomCount.HighValue = _defaults.PrimaryRooms.Max;

        List<EBranchType> types = new List<EBranchType>(_defaults.BranchTypes);
        Shoot.isOn = types.Contains(EBranchType.Shoot);
        Snake.isOn = types.Contains(EBranchType.Snake);
        Bridge.isOn = types.Contains(EBranchType.Bridge);
        Crank.isOn = types.Contains(EBranchType.Crank);

        DistributionOptions.value = (int)ERoomDistribution.Random;
    }

    public void PreviewCurrentSettingsPressed()
    {
        _previewSettings = GetSettingsFromControls();
        _previewData = Generator.GenerateInstant(_previewSettings);

        LaunchFromPreviewButton.enabled = true;
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
        GenerationSettings settings = GetSettingsFromControls();

        DoneGenSettingsData data = ScriptableObject.CreateInstance<DoneGenSettingsData>();
        data.SettingsName = fileName;
        data.Settings = settings;

        AssetDatabase.CreateAsset(data, "Assets/Resources/Settings/" + fileName + ".asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
    }

    GenerationSettings GetSettingsFromControls()
    {
        List<EBranchType> branchTypes = new List<EBranchType>();
        if (Shoot.isOn)
            branchTypes.Add(EBranchType.Shoot);
        if (Snake.isOn)
            branchTypes.Add(EBranchType.Snake);
        if (Bridge.isOn)
            branchTypes.Add(EBranchType.Bridge);
        if (Crank.isOn)
            branchTypes.Add(EBranchType.Crank);

        GenerationSettings settings = new GenerationSettings()
        {
            GameStyle = (EGameStyle)DungeonStyleOptions.value,
            GridWidth = int.Parse(XDimension.text),
            GridHeight = int.Parse(YDimension.text),
            PrimaryRooms = new ValueRange((int)RoomCount.LowValue, (int)RoomCount.HighValue),
            BranchTypes = branchTypes.ToArray(),
            TileSet = _loadedTileSets[TileSetOptions.value],
            RoomDistributionStyle = (ERoomDistribution)DistributionOptions.value
        };

        return settings;
    }

    public void LaunchFromSettingsPressed()
    {
        DungeonBuilder.Instance.BuildAndLaunch(GetSettingsFromControls());
        Close();
    }

    public void LaunchFromPreviewPressed()
    {
        DungeonBuilder.Instance.CopyBuildAndLaunch(_previewSettings, _previewData);
        Close();
    }
}
