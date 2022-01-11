using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abilities
{
    public static List<Command> AbilityList = new List<Command>()
    {
        new Command("Gather", "Icons/Commands/gather", true, false),
        new Command("Construct", "Icons/Commands/construct", false, false),
        new Command("Cancel", "Icons/Commands/cancel", false, false)
    };
}
