using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Production: Command
{
    public int productionDuration;
    public int goldCost;
    public int lumberCost;
    [CanBeNull] public string unitSpawned;
    //public Building newBuilding; for upgrading the building
    //public Upgrade upgradeProvided;

    public Production(string productionName, string spriteName, bool hasTargetEntity, bool hasTargetPoint, int productionDuration, int goldCost, int lumberCost, [CanBeNull] string unitSpawned): base(productionName, spriteName, hasTargetEntity, hasTargetPoint)
    {
        this.productionDuration = productionDuration;
        this.goldCost = goldCost;
        this.lumberCost = lumberCost;
        if (unitSpawned != null)
            this.unitSpawned = unitSpawned;
    }
}
