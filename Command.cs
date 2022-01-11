using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

public class Command
{
    public string commandName;
    public Sprite sprite;
    public bool hasTargetEntity;
    public bool hasTargetPoint;

    protected CommandEffects commandEffects;
    protected ActiveEntity actor;
    [CanBeNull] private Entity targetEntity;
    protected Vector3 targetPoint;

    public Command(string commandName, string spriteName, bool hasTargetEntity, bool hasTargetPoint)
    {
        this.commandName = commandName;
        this.sprite = Resources.Load<Sprite>("Sprites/" + spriteName);
        this.hasTargetEntity = hasTargetEntity;
        this.hasTargetPoint = hasTargetPoint;
    }

    public void SetActor(ActiveEntity actor)
    {
        this.actor = actor;
    }
}