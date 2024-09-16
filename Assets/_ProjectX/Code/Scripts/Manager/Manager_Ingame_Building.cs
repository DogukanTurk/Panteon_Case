using System.Collections.Generic;
using DOT.Utilities;
using Unity.Mathematics;
using UnityEngine;

public class Manager_Ingame_Building : SingletonComponent<Manager_Ingame_Building>
{
    /* ------------------------------------------ */

    public List<int2> OccupiedByBuildingsPositions = new List<int2>();

    /* ------------------------------------------ */

    private List<GameObject> _buildings = new();

    /* ------------------------------------------ */

    public bool AddBuilding(int2 position, int2 size, GameObject building)
    {
        if (AddBuilding(position, size))
        {
            _buildings.Add(building);
            return true;
        }

        return false;
    }

    public bool AddBuilding(int2 position, int2 size)
    {
        if (CanBuild(position, size))
        {
            for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                OccupiedByBuildingsPositions.Add(new int2(position.x + x, position.y + y));

            return true;
        }

        return false;
    }

    public bool CanBuild(int2 position, int2 size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (OccupiedByBuildingsPositions.Contains(new int2(position.x + x, position.y + y)))
                    return false;
            }
        }

        return true;
    }

    public void RemoveBuilding(int2 position, int2 size, GameObject building)
    {
        if (building != null && _buildings.Contains(building))
            _buildings.Remove(building);

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                OccupiedByBuildingsPositions.Remove(new int2(position.x + x, position.y + y));
            }
        }
    }

    /* ------------------------------------------ */
}