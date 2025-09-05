using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PremadeTile : MonoBehaviour
{
    [SerializeField] public int Width = 1;
    [SerializeField] public int Height = 1;

    [SerializeField] public Sprite[] Sprites;

    public int[] GetGridIDs()
    {
        int[] ids = new int[Sprites.Length];

        for(int i=0;i<ids.Length;i++)
        {
            if(Sprites[i] == null)
            {
                ids[i] = -1;
            }
            else
            {
                string[] split = Sprites[i].name.Split('_');
                ids[i] = int.Parse(split[1]);
            }
        }
        return ids;
    }
}
