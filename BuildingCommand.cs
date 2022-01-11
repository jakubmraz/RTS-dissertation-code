using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class BuildingCommand : Command
{
    public int buildingDuration;
    public int goldCost;
    public int lumberCost;
    public string BuildingSpawned;

    public BuildingCommand(string productionName, string spriteName, bool hasTargetEntity, bool hasTargetPoint, int buildingDuration, int goldCost, int lumberCost, string buildingSpawned) : base(productionName, spriteName, hasTargetEntity, hasTargetPoint)
    {
        this.buildingDuration = buildingDuration;
        this.goldCost = goldCost;
        this.lumberCost = lumberCost;
        this.BuildingSpawned = buildingSpawned;
    }
}
