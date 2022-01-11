using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldMine : Entity
{
    public int MaxGold;
    public int CurrentGold;

    protected override void Awake()
    {
        base.Awake();
        CurrentGold = MaxGold;
    }

    public int MineGold(int capacity)
    {
        if (CurrentGold >= capacity)
            return capacity;
        else
            return CurrentGold;
    }
}
