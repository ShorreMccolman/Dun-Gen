using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PremadeTile))]
public class PremadeTileInspector : Editor
{
    PremadeTile _tile;
    int _width;
    int _height;

    private void OnEnable()
    {
        _tile = (PremadeTile)target;
        _width = _tile.Width;
        _height = _tile.Height;
    }

    public override void OnInspectorGUI()
    {
        _tile.Width = EditorGUILayout.IntField("Width: ", _tile.Width);
        _tile.Height = EditorGUILayout.IntField("Height: ", _tile.Height);

        if (_tile.Sprites == null || _tile.Sprites.Length != _width * _height)
        {
            _tile.Sprites = new Sprite[_width * _height];
        }

        for (int i = 0; i < _height; i++)
        {
            GUILayout.BeginHorizontal();
            for (int j = 0; j < _width; j++)
            {
                int index = j + i * _width;
                _tile.Sprites[index] = (Sprite)EditorGUILayout.ObjectField(_tile.Sprites[index], typeof(Sprite), true, GUILayout.Width(100f), GUILayout.Height(100f));
            }
            GUILayout.EndHorizontal();
        }
    }
}
