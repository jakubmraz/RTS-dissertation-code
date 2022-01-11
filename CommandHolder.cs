using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class CommandHolder : MonoBehaviour
{
    public Command command;
    [SerializeField] private Core core;

    public void PassCommandToCore()
    {
        if (command != null)
            core.CommandClicked(command);
    }
}
