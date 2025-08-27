using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;

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

    protected override void OnOpen()
    {
        XDimension.text = "36";
        YDimension.text = "20";
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
}
