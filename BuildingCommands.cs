using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BuildingCommands
{
    public static List<BuildingCommand> BuildingsList = new List<BuildingCommand>()
    {
        new BuildingCommand("Barracks", "Icons/Commands/construct", false, false, 15, 0, 0, "Barracks"),
    };
}
