using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen
{
    [CreateAssetMenu(menuName = "DoneGen/Tile Component Set")]
    public class DungeonComponentData : DunGenData
    {
        [SerializeField] GameObject Floor;
        [SerializeField] GameObject Wall;
        [SerializeField] GameObject InnerCorner;

        public override GameObject GetTile(int id)
        {
            bool[] bits = new bool[8];
            for(int i=0;i<8;i++)
            {
                bits[i] = GetBitValue(id, i);
            }

            GameObject parent = new GameObject(id.ToString());

            // Floor Center
            Instantiate(Floor, Vector3.zero, Quaternion.identity, parent.transform);

            // Floor Ends
            if (bits[1])
                Instantiate(Floor, new Vector3(-1, 0, 0), Quaternion.identity, parent.transform);
            if (bits[3])
                Instantiate(Floor, new Vector3(0, 0, 1), Quaternion.identity, parent.transform);
            if (bits[4])
                Instantiate(Floor, new Vector3(0, 0, -1), Quaternion.identity, parent.transform);
            if (bits[6])
                Instantiate(Floor, new Vector3(1, 0, 0), Quaternion.identity, parent.transform);

            // Floor Corners
            if (bits[0] && bits[1] && bits[3])
                Instantiate(Floor, new Vector3(-1, 0, 1), Quaternion.identity, parent.transform);
            if (bits[1] && bits[2] && bits[4])
                Instantiate(Floor, new Vector3(-1, 0, -1), Quaternion.identity, parent.transform);
            if (bits[3] && bits[5] && bits[6])
                Instantiate(Floor, new Vector3(1, 0, 1), Quaternion.identity, parent.transform);
            if (bits[4] && bits[6] && bits[7])
                Instantiate(Floor, new Vector3(1, 0, -1), Quaternion.identity, parent.transform);

            // West Wall
            if (id == 24 || id == 88 || id == 120 || id == 216 || id == 248)
                Instantiate(Wall, new Vector3(-0.5f, 0f, 0f), Quaternion.Euler(0, -90, 0), parent.transform);
            // North Wall
            if (id == 66 || id == 82 || id == 86 || id == 210 || id == 214)
                Instantiate(Wall, new Vector3(0f, 0f, 0.5f), Quaternion.Euler(0, 0, 0), parent.transform);
            // East Wall
            if (id == 24 || id == 26 || id == 27 || id == 30 || id == 31)
                Instantiate(Wall, new Vector3(0.5f, 0f, 0f), Quaternion.Euler(0, 90, 0), parent.transform);
            // South Wall
            if (id == 66 || id == 74 || id == 75 || id == 106 || id == 107)
                Instantiate(Wall, new Vector3(0f, 0f, -0.5f), Quaternion.Euler(0, 0, 0), parent.transform);

            // North on West wall
            if (id == 2 || id == 18 || id == 22 || id == 66 || id == 82 || id == 86 || id == 210 || id == 214)
                Instantiate(Wall, new Vector3(-1f, 0f, 0.5f), Quaternion.Euler(0, 0, 0), parent.transform);
            // South on West wall
            if (id == 2 || id == 10 || id == 11 || id == 66 || id == 74 || id == 75 || id == 106 || id == 107)
                Instantiate(Wall, new Vector3(-1f, 0f, -0.5f), Quaternion.Euler(0, 0, 0), parent.transform);

            // West on North Wall
            if (id == 8 || id == 24 || id == 72 || id == 88 || id == 104 || id == 120 || id == 216 || id == 248)
                Instantiate(Wall, new Vector3(-0.5f, 0f, 1f), Quaternion.Euler(0, -90, 0), parent.transform);
            // East on North Wall
            if (id == 8 || id == 10 || id == 11 || id == 24 || id == 26 || id == 27 || id == 30 || id == 31)
                Instantiate(Wall, new Vector3(0.5f, 0f, 1f), Quaternion.Euler(0, -90, 0), parent.transform);

            // North on East wall
            if (id == 64 || id == 66 || id == 80 || id == 82 || id == 86 || id == 208 || id == 210 || id == 214)
                Instantiate(Wall, new Vector3(1f, 0f, 0.5f), Quaternion.Euler(0, 0, 0), parent.transform);
            // South on East wall
            if (id == 64 || id == 66 || id == 72 || id == 74 || id == 75 || id == 104 || id == 106 || id == 107)
                Instantiate(Wall, new Vector3(1f, 0f, -0.5f), Quaternion.Euler(0, 0, 0), parent.transform);

            // West on South Wall
            if (id == 16 || id == 24 || id == 80 || id == 88 || id == 120 || id == 208 || id == 216 || id == 248)
                Instantiate(Wall, new Vector3(-0.5f, 0f, -1f), Quaternion.Euler(0, -90, 0), parent.transform);
            // East on South Wall
            if (id == 16 || id == 18 || id == 22 || id == 24 || id == 26 || id == 27 || id == 30 || id == 31)
                Instantiate(Wall, new Vector3(0.5f, 0f, -1f), Quaternion.Euler(0, -90, 0), parent.transform);

            // North-West Inner Corner
            if (id == 16 || id == 64 || id == 80 || id == 208)
            {
                GameObject corner = Instantiate(InnerCorner, new Vector3(-0.5f, 0f, 0.5f), Quaternion.Euler(0, 90, 0), parent.transform);

                if (id == 16)
                    corner.transform.GetChild(0).gameObject.SetActive(false);
                else if (id == 64)
                    corner.transform.GetChild(1).gameObject.SetActive(false);
            }

            // North-East Inner Corner
            if (id == 2 || id == 16 || id == 18 || id == 22)
            {
                GameObject corner = Instantiate(InnerCorner, new Vector3(0.5f, 0f, 0.5f), Quaternion.Euler(0, 180, 0), parent.transform);

                if (id == 2)
                    corner.transform.GetChild(0).gameObject.SetActive(false);
                else if (id == 16)
                    corner.transform.GetChild(1).gameObject.SetActive(false);
            }

            // South-East Inner Corner
            if (id == 2 || id == 8 || id == 10 || id == 11)
            {
                GameObject corner = Instantiate(InnerCorner, new Vector3(0.5f, 0f, -0.5f), Quaternion.Euler(0, 270, 0), parent.transform);

                if (id == 8)
                    corner.transform.GetChild(0).gameObject.SetActive(false);
                else if (id == 2)
                    corner.transform.GetChild(1).gameObject.SetActive(false);
            }

            // South-West Inner Corner
            if (id == 8 || id == 64 || id == 72 || id == 104)
            {
                GameObject corner = Instantiate(InnerCorner, new Vector3(-0.5f, 0f, -0.5f), Quaternion.Euler(0, 0, 0), parent.transform);

                if (id == 64)
                    corner.transform.GetChild(0).gameObject.SetActive(false);
                else if (id == 8)
                    corner.transform.GetChild(1).gameObject.SetActive(false);
            }

            // North-West Outer Corner
            if(id == 10 || id == 26 || id == 30 || id == 74 || id == 90 || id == 94 || id == 106 || id == 122 || id == 126 || id == 218 || id == 222 || id == 250 || id == 254)
                Instantiate(InnerCorner, new Vector3(-0.5f, 0f, 0.5f), Quaternion.Euler(0, 270, 0), parent.transform);

            // North-East Outer Corner
            if(id == 72 || id == 74 || id == 75 || id == 88 || id == 90 || id == 91 || id == 94 || id == 95 || id == 216 || id == 218 || id == 219 || id == 222 || id == 223)
                Instantiate(InnerCorner, new Vector3(0.5f, 0f, 0.5f), Quaternion.Euler(0, 0, 0), parent.transform);

            // South-East Outer Corner
            if(id == 80 || id == 82 || id == 86 || id == 88 || id == 90 || id == 91 || id == 94 || id == 95 || id == 120 || id == 122 || id == 123 || id == 126 || id == 127)
                Instantiate(InnerCorner, new Vector3(0.5f, 0f, -0.5f), Quaternion.Euler(0, 90, 0), parent.transform);

            // South-West Outer Corner
            if (id == 18 || id == 26 || id == 27 || id == 82 || id == 90 || id == 91 || id == 122 || id == 123 || id == 210 || id == 218 || id == 219 || id == 250 || id == 251)
                Instantiate(InnerCorner, new Vector3(-0.5f, 0f, -0.5f), Quaternion.Euler(0, 180, 0), parent.transform);

            return parent;
        }

        public bool GetBitValue(int number, int position)
        {
            return ((number >> position) & 1) == 1;
        }
    }
}