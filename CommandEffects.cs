using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

public class CommandEffects : MonoBehaviour
{
    private List<Command> commands;
    private Vector3 cheatVector;
    private Entity cheatEntity;
    private bool waiting;

    private Core core;
    [SerializeField] private UI ui;

    void Awake()
    {
        core = GetComponent<Core>();
        waiting = false;
        cheatVector = new Vector3();
        commands = new List<Command>();
        foreach (var command in GenericCommands.CommandList)
        {
            commands.Add(command);
        }
        foreach (var command in Productions.ProductionsList)
        {
            commands.Add(command);
        }
        foreach (var command in Abilities.AbilityList)
        {
            commands.Add(command);
        }
        foreach (var command in BuildingCommands.BuildingsList)
        {
            commands.Add(command);
        }
    }

    public void ExecuteCommand(Command command, ActiveEntity actor)
    {
        StartCoroutine(StartCommandCoroutine(command, actor));
    }

    private IEnumerator StartCommandCoroutine(Command command, ActiveEntity actor)
    {
        cheatEntity = null;
        cheatVector = Vector3.zero;
        command.SetActor(actor);

        if (command.hasTargetEntity && !command.hasTargetPoint)
        {
            yield return StartCoroutine(GetTargetEntity());
            if (cheatEntity == null)
            {
                yield break;
            }
        }

        if (!command.hasTargetEntity && command.hasTargetPoint)
        {
            yield return StartCoroutine(GetTargetPoint());
            if (cheatEntity == null && cheatVector == Vector3.zero)
            {
                yield break;
            }
        }

        if (command.hasTargetEntity && command.hasTargetPoint)
        {
            yield return StartCoroutine(GetTargetEntityOrPoint());
            if (cheatEntity == null && cheatVector == Vector3.zero)
            {
                yield break;
            }
        }

        int productionDuration = 0;
        //if building check money and lumber requirements
        if (command is Production production)
            productionDuration = production.productionDuration;

        switch (command.commandName)
        {
            case "Peasant":
                ExecuteCommandEffect("Spawn Peasant", actor, null, new Vector3(), productionDuration);
                break;
            
            case "Gather":
                if (cheatEntity is GoldMine)
                {
                    actor.OnCommanded();
                    ExecuteCommandEffect("Gather Gold", actor, cheatEntity, new Vector3(), 0);
                }
                else if (cheatEntity is Trees)
                {
                    actor.OnCommanded();
                    ExecuteCommandEffect("Gather Lumber", actor, cheatEntity, new Vector3(), 0);
                }
                else
                {
                    Debug.Log("Invalid Target");
                }
                break;
            case "Attack":
                actor.OnAttackCommanded();
                ExecuteCommandEffect("Attack", actor, cheatEntity, cheatVector, 0);
                break;
            case "Move":
                actor.OnMove();
                ExecuteCommandEffect("Move", actor, cheatEntity, cheatVector, 0);
                break;
            case "Stop":
                ExecuteCommandEffect("Stop", actor, cheatEntity, cheatVector, 0);
                break;
            case "Hold":
                ExecuteCommandEffect("Hold", actor, cheatEntity, cheatVector, 0);
                break;
            case "Patrol":
                actor.OnCommanded();
                ExecuteCommandEffect("Patrol", actor, cheatEntity, cheatVector, 0);
                break;
            case "Construct":
                ui.FillBuildCommands(actor);
                break;
            case "Cancel":
                ui.FillCommands(actor);
                break;
            case "Barracks":
                ExecuteCommandEffect("Construct Barracks", actor, cheatEntity, cheatVector, 0);
                break;
        }
    }

    private IEnumerator GetTargetEntity()
    {
        core.EntityTargetingMode();
        waiting = true;
        while (waiting)
        {
            yield return new WaitForSeconds(.1f);
        }
    }

    public void SendBackEntity([CanBeNull] Entity entity)
    {
        cheatEntity = entity;
        waiting = false;
    }

    public void SendBackPoint(Vector3 vector)
    {
        cheatVector = vector;
        waiting = false;
    }

    public IEnumerator GetTargetPoint()
    {
        core.PointTargetingMode();
        waiting = true;
        while (waiting)
        {
            yield return new WaitForSeconds(.1f);
        }
    }

    private IEnumerator GetTargetEntityOrPoint()
    {
        core.EitherTargetingMode();
        waiting = true;
        while (waiting)
        {
            yield return new WaitForSeconds(.1f);
        }
    }

    private void ExecuteCommandEffect(string name, ActiveEntity actor, Entity targetEntity, Vector3 targetPoint, int productionTime)
    {
        StartCoroutine(ExecuteCommandCoroutine(name, actor, targetEntity, targetPoint, productionTime));
    }

    private IEnumerator ExecuteCommandCoroutine(string name, ActiveEntity actor, Entity targetEntity,
        Vector3 targetPoint, int productionTime)
    {
        Building buildingPrefab;

        switch (name)
        {
            case "Spawn Peasant":
                yield return StartCoroutine(ProductionCoroutine(productionTime));

                Peasant peasantPrefab = Resources.Load<Peasant>("Prefabs/Peasant");
                Peasant newPeasant = Instantiate(peasantPrefab,
                    new Vector3(actor.transform.position.x, actor.transform.position.y,
                        actor.transform.position.z - 10), Quaternion.identity).GetComponent<Peasant>();
                newPeasant.team = actor.team;
                newPeasant.team.teamEntities.Add(newPeasant);
                break;
            case "Gather Gold":
                if(actor is Peasant peasant)
                    peasant.GoHarvestGold(targetEntity as GoldMine);
                break;
            case "Gather Lumber":
                if (actor is Peasant peasant1)
                    peasant1.GoHarvestLumber(targetEntity.transform.position);
                break;
            case "Attack":
                if (targetEntity)
                {
                    actor.Attack(targetEntity);
                }
                else if (targetPoint != Vector3.zero)
                {
                    Debug.Log(actor.entityName + " attack moving to " + targetPoint);
                    actor.AttackMove(targetPoint);
                }
                break;
            case "Move":
                if (targetEntity)
                {
                    Debug.Log("following " + targetEntity.entityName);
                    actor.Follow(targetEntity);
                }
                else if (targetPoint != Vector3.zero)
                {
                    actor.MoveToLocation(targetPoint);
                }
                break;
            case "Stop":
                actor.Stop();
                break;
            case "Hold":
                actor.HoldPosition();
                break;
            case "Patrol":
                actor.Patrol(targetPoint);
                break;
            case "Construct Barracks":
                buildingPrefab = Resources.Load<Building>("Prefabs/Barracks");
                core.ConstructionMode(buildingPrefab);
                break;
        }
    }

    private IEnumerator ProductionCoroutine(int duration)
    {
        int i = 0;
        while (i < duration)
        {
            yield return new WaitForSeconds(1f);
            i++;
            Debug.Log(i);
            //update actor's current production and its completion %
        }
    }
}
