using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen
{
    public static class Cardinals
    {
        public static ECardinal FromCoordinateDifference(int xDiff, int yDiff)
        {
            if (xDiff == 0)
            {
                if (yDiff > 0)
                    return ECardinal.N;
                else
                    return ECardinal.S;
            }
            else if (xDiff > 0)
            {
                return ECardinal.W;
            }
            else
            {
                return ECardinal.E;
            }
        }

        public static ECardinal Rotate(ECardinal direction, bool isClockwise)
        {
            if (isClockwise)
            {
                return (ECardinal)(((int)direction + 1) % 4);
            }

            return (ECardinal)(((int)direction + 3) % 4);
        }

        public static ECardinal Flip(ECardinal direction)
        {
            return (ECardinal)(((int)direction + 2) % 4);
        }
    }
}