using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// TODO create a custom inspector for tile set objects that visually indicates how to setup tiles for users
//

namespace DunGen
{
    [CreateAssetMenu(menuName = "DoneGen/Tile Set")]
    public class DungeonTileData : ScriptableObject
    {
        public static readonly int[] Island = { 0 };
        public static readonly int[] EndPieces = { 2, 8, 16, 64 };
        public static readonly int[] WallPieces = { 31, 107, 214, 248 };
        public static readonly int[] CornerPieces = { 11, 22, 104, 208 };
        public static readonly int[] InsideCorners = { 127, 223, 251, 254 };
        public static readonly int[] HallPieces = { 24, 66 };
        public static readonly int[] TurnPieces = { 10, 18, 72, 80 };
        public static readonly int[] Intersections = { 26, 74, 82, 88, 90 };
        public static readonly int[] Transitions = { 27, 30, 75, 86, 91, 94, 95, 106, 120, 122, 123, 210, 216, 218, 222, 250 };
        public static readonly int[] RoomTiles = { 126, 219 };
        public static readonly int[] OpenSpace = { 255 };

        public GameObject ErrorTile;
        public GameObject Tile_0;
        public GameObject Tile_2;
        public GameObject Tile_8;
        public GameObject Tile_10;
        public GameObject Tile_11;
        public GameObject Tile_16;
        public GameObject Tile_18;
        public GameObject Tile_22;
        public GameObject Tile_24;
        public GameObject Tile_26;
        public GameObject Tile_27;
        public GameObject Tile_30;
        public GameObject Tile_31;
        public GameObject Tile_64;
        public GameObject Tile_66;
        public GameObject Tile_72;
        public GameObject Tile_74;
        public GameObject Tile_75;
        public GameObject Tile_80;
        public GameObject Tile_82;
        public GameObject Tile_86;
        public GameObject Tile_88;
        public GameObject Tile_90;
        public GameObject Tile_91;
        public GameObject Tile_94;
        public GameObject Tile_95;
        public GameObject Tile_104;
        public GameObject Tile_106;
        public GameObject Tile_107;
        public GameObject Tile_120;
        public GameObject Tile_122;
        public GameObject Tile_123;
        public GameObject Tile_126;
        public GameObject Tile_127;
        public GameObject Tile_208;
        public GameObject Tile_210;
        public GameObject Tile_214;
        public GameObject Tile_216;
        public GameObject Tile_218;
        public GameObject Tile_219;
        public GameObject Tile_222;
        public GameObject Tile_223;
        public GameObject Tile_248;
        public GameObject Tile_250;
        public GameObject Tile_251;
        public GameObject Tile_254;
        public GameObject Tile_255;

        public GameObject[] PremadeTiles;

        public GameObject GetTile(int id)
        {
            switch (id)
            {
                default:
                    Debug.LogError("Placing error tile with id " + id);
                    return ErrorTile;

                case 0:
                    return Tile_0;
                case 2:
                    return Tile_2;
                case 8:
                    return Tile_8;
                case 10:
                    return Tile_10;
                case 11:
                    return Tile_11;
                case 16:
                    return Tile_16;
                case 18:
                    return Tile_18;
                case 22:
                    return Tile_22;
                case 24:
                    return Tile_24;
                case 26:
                    return Tile_26;
                case 27:
                    return Tile_27;
                case 30:
                    return Tile_30;
                case 31:
                    return Tile_31;
                case 64:
                    return Tile_64;
                case 66:
                    return Tile_66;
                case 72:
                    return Tile_72;
                case 74:
                    return Tile_74;
                case 75:
                    return Tile_75;
                case 80:
                    return Tile_80;
                case 82:
                    return Tile_82;
                case 86:
                    return Tile_86;
                case 88:
                    return Tile_88;
                case 90:
                    return Tile_90;
                case 91:
                    return Tile_91;
                case 94:
                    return Tile_94;
                case 95:
                    return Tile_95;
                case 104:
                    return Tile_104;
                case 106:
                    return Tile_106;
                case 107:
                    return Tile_107;
                case 120:
                    return Tile_120;
                case 122:
                    return Tile_122;
                case 123:
                    return Tile_123;
                case 126:
                    return Tile_126;
                case 127:
                    return Tile_127;
                case 208:
                    return Tile_208;
                case 210:
                    return Tile_210;
                case 214:
                    return Tile_214;
                case 216:
                    return Tile_216;
                case 218:
                    return Tile_218;
                case 219:
                    return Tile_219;
                case 222:
                    return Tile_222;
                case 223:
                    return Tile_223;
                case 248:
                    return Tile_248;
                case 250:
                    return Tile_250;
                case 251:
                    return Tile_251;
                case 254:
                    return Tile_254;
                case 255:
                    return Tile_255;
            }
        }

        public static ECardinal GetExitDirectionForTileID(int tileID)
        {
            switch (tileID)
            {
                case 27:
                    return ECardinal.S;
                case 30:
                    return ECardinal.N;
                case 75:
                    return ECardinal.E;
                case 86:
                    return ECardinal.E;
                case 95:
                    return ECardinal.E;
                case 106:
                    return ECardinal.W;
                case 120:
                    return ECardinal.S;
                case 123:
                    return ECardinal.S;
                case 210:
                    return ECardinal.W;
                case 216:
                    return ECardinal.N;
                case 222:
                    return ECardinal.N;
                case 250:
                    return ECardinal.W;
            }

            return ECardinal.None;
        }
    }
}