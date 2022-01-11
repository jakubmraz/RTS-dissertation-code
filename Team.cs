using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour
{
    public bool isPlayer;
    
    public int gold;
    public int lumber;

    private int baseFood = 4;

    public List<ActiveEntity> teamEntities;

    public List<Building> GetBuildings()
    {
        List<Building> buildings = new List<Building>();
        foreach (var entity in teamEntities)
        {
            if (entity is Building building)
                buildings.Add(building);
        }

        return buildings;
    }

    public List<Unit> GetUnits()
    {
        List<Unit> units = new List<Unit>();
        foreach (var entity in teamEntities)
        {
            if (entity is Unit unit)
                units.Add(unit);
        }

        return units;
    }

    public int GetFoodConsumed()
    {
        int consumption = 0;
        foreach (var unit in GetUnits())
        {
            consumption += unit.foodConsumption;
        }

        return consumption;
    }

    public int GetFoodMax()
    {
        int max = baseFood;
        foreach (var building in GetBuildings())
        {
            max += building.foodProvided;
        }

        return max;
    }
}
