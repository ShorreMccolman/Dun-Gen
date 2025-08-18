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

    public bool CanShoot;
    public bool CanSnake;
    public bool CanBridge;
    public bool CanCrank;
}

[CreateAssetMenu(menuName = "DoneGen/Generation Settings")]
public class DoneGenSettingsData : ScriptableObject
{
    [SerializeField] string SettingsName;
    [SerializeField] GenerationSettings Settings;
}
