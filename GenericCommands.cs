using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GenericCommands
{
    public static List<Command> CommandList = new List<Command>()
    {
        new Command("Move", "Icons/Commands/move", true, true),
        new Command("Stop", "Icons/Commands/stop", false, false),
        new Command("Hold", "Icons/Commands/hold", false, false),
        new Command("Attack", "Icons/Commands/attack", true, true),
        new Command("Patrol", "Icons/Commands/patrol", false, true)
    };
}
