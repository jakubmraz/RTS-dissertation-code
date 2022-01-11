using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownHall : Building
{
    public override void OnProductionFinished()
    {
        Debug.Log("merp merp");
    }
}
