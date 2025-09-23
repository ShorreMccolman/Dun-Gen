using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen
{
    public enum EBranchType
    {
        Shoot,
        Snake,
        Bridge,
        Crank,
        Count
    }

    public abstract class BranchGenerator
    {
        public abstract bool Create(MapData map, MapTile startingCell);
    }
}