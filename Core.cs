using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public enum PlayerState
{
    Default,
    Targeting,
    Building
}

public class Core : MonoBehaviour
{
    private bool targetingEntities;
    private bool targetingLocation;

    private Entity selectedEntity;
    private PlayerState playerState;

    //Player
    public Team team1;
    //Others
    public Team team2;
    public Team team3;
    public Team team4;
    //Neutrals
    public Team teamNeutrals;

    private UI ui;
    private CommandEffects commandEffects;
    private GridBuildingSystem gridBuildingSystem;

    void Awake()
    {
        ui = FindObjectOfType<UI>();
        commandEffects = GetComponent<CommandEffects>();
        gridBuildingSystem = GetComponent<GridBuildingSystem>();

        targetingEntities = false;
        targetingLocation = false;
        playerState = PlayerState.Default;
    }

    void Update()
    {
        switch (playerState)
        {
            case PlayerState.Default:
                if (Input.GetMouseButtonDown(0))
                {
                    RaycastHit raycastHit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out raycastHit, 100f))
                    {
                        if (raycastHit.transform != null && !EventSystem.current.IsPointerOverGameObject())
                        {
                            Entity hitEntity = raycastHit.transform.gameObject.GetComponent<Entity>();
                            if (hitEntity != null)
                            {
                                SelectEntity(hitEntity);
                            }
                        }
                    }
                }

                if (Input.GetMouseButtonDown(1))
                {
                    if (selectedEntity == null || !(selectedEntity is ActiveEntity activeEntity))
                        return;

                    if (activeEntity.team == team1)
                    {
                        RaycastHit raycastHit;
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(ray, out raycastHit, 100f))
                        {
                            Vector3 target = raycastHit.point;
                            if (activeEntity.canMove)
                                activeEntity.MoveToLocation(target);
                        }
                    }
                }
                break;

            case PlayerState.Targeting:
                if (Input.GetMouseButtonDown(0))
                {
                    RaycastHit raycastHit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out raycastHit, 100f))
                    {
                        if (raycastHit.transform != null && !EventSystem.current.IsPointerOverGameObject())
                        {
                            Entity hitEntity = raycastHit.transform.gameObject.GetComponent<Entity>();
                            if (hitEntity != null)
                            {
                                if (targetingEntities)
                                {
                                    commandEffects.SendBackEntity(hitEntity);
                                    StopTargetingMode();
                                }
                                else Debug.Log("Invalid target.");
                            }
                            else
                            {
                                Vector3 target = raycastHit.point;
                                if (targetingLocation)
                                {
                                    commandEffects.SendBackPoint(target);
                                    StopTargetingMode();
                                }
                                else Debug.Log("Invalid target.");
                            }
                        }
                    }
                }

                if (Input.GetMouseButtonDown(1))
                {
                    //Cancel targeting and stop whatever command was waiting for response
                }
                break;
            case PlayerState.Building:
                break;
        }

        if (team1 != null)
        {
            ui.UpdateTopUI(team1);
            foreach (ActiveEntity entity in team1.teamEntities)
            {
                entity.hpBar.UpdateHPBar(entity, true);
            }
        }

        if (team2 != null)
        {
            foreach (ActiveEntity entity in team2.teamEntities)
            {
                entity.hpBar.UpdateHPBar(entity, true);
            }
        }

        if (teamNeutrals != null)
        {
            foreach (ActiveEntity entity in teamNeutrals.teamEntities)
            {
                entity.hpBar.UpdateHPBar(entity, false);
            }
        }
    }

    public void CommandClicked(Command command)
    {
        commandEffects.ExecuteCommand(command, selectedEntity as ActiveEntity);
    }

    public void SelectEntity(Entity entity)
    {
        selectedEntity = entity;
        selectedEntity.OnSelected();
        ui.UpdateBottomUI(entity);
    }

    public void EntityTargetingMode()
    {
        playerState = PlayerState.Targeting;
        targetingEntities = true;
        targetingLocation = false;
    }

    public void PointTargetingMode()
    {
        playerState = PlayerState.Targeting;
        targetingEntities = false;
        targetingLocation = true;
    }

    internal void EitherTargetingMode()
    {
        playerState = PlayerState.Targeting;
        targetingEntities = true;
        targetingLocation = true;
    }

    private void StopTargetingMode()
    {
        playerState = PlayerState.Default;
        targetingLocation = false;
        targetingEntities = false;
    }

    public void ConstructionMode(Building building)
    {
        playerState = PlayerState.Building;
        gridBuildingSystem.StartBuilding(building);
    }

    public void StopConstructionMode(Building building)
    {
        playerState = PlayerState.Default;
        gridBuildingSystem.StopBuilding();
        ui.FillCommands(selectedEntity);

        if (building != null)
        {
            building.QueueThisBuilding();
            if (selectedEntity is Peasant peasant)
            {
                peasant.GoBuildBuilding(building);
            }
        }
    }
}
