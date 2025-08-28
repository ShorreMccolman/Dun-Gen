using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ValueRange
{
    public int Min;
    public int Max;

    public ValueRange(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public int Evaluate()
    {
        return Random.Range(Min, Max + 1);
    }
}

public enum EGameStyle
{
    FirstPerson,
    TopDown
}

[System.Serializable]
public struct GenerationSettings
{
    public EGameStyle GameStyle;
    public int GridWidth, GridHeight;
    public ValueRange PrimaryRooms;

    public DunGen.EBranchType[] BranchTypes;

    public bool CanBranch => BranchTypes != null && BranchTypes.Length > 0;

    public DunGen.EBranchType GetRandomBranchType()
    {
        if (BranchTypes == null || BranchTypes.Length == 0)
            return DunGen.EBranchType.Count;

        return BranchTypes[Random.Range(0, BranchTypes.Length)];
    }
}

[CreateAssetMenu(menuName = "DoneGen/Generation Settings")]
public class DoneGenSettingsData : ScriptableObject
{
    [SerializeField] public string SettingsName;
    [SerializeField] public GenerationSettings Settings;
}
