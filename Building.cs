using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using UnityEngine;
using UnityEngine.AI;

public class Building : ActiveEntity
{
    public Transform visual;
    [SerializeField] private Transform constructionModel;
    [SerializeField] private Transform regularModel;
    private NavMeshObstacle navMeshObstacle;

    public int width;
    public int height;
    private Vector2Int origin;

    public int foodProvided;

    public List<string> availableProductions;

    public bool constructing;

    protected override void Awake()
    {
        base.Awake();
        navMeshObstacle = GetComponent<NavMeshObstacle>();
    }

    public void QueueThisBuilding()
    {
        SetLayerRecursive(visual.gameObject, 6);
        navMeshObstacle.enabled = false;
    }

    private void SetLayerRecursive(GameObject targetGameObject, int layer)
    {
        targetGameObject.layer = layer;
        foreach (Transform child in targetGameObject.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    public List<Vector2Int> GetGridPositionList(Vector2Int offset)
    {
        List<Vector2Int> gridPositionList = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gridPositionList.Add(offset + new Vector2Int(x, y));
            }
        }
        return gridPositionList;
    }

    public List<Vector2Int> GetGridPositionList()
    {
        return GetGridPositionList(origin);
    }

    public void SetOrigin(Vector2Int origin)
    {
        this.origin = origin;
    }

    public virtual void StartProduction()
    {

    }

    public virtual void OnStartProduction()
    {

    }

    public virtual void OnProductionFinished()
    {

    }

    internal void ActivateBuilding(ActiveEntity entity)
    {
        SetLayerRecursive(visual.gameObject, 0);
        navMeshObstacle.enabled = true;
        team = entity.team;
        team.teamEntities.Add(this);

        StartConstruction(entity);
    }

    private void StartConstruction(ActiveEntity entity)
    {
        regularModel.gameObject.SetActive(false);
        constructionModel.gameObject.SetActive(true);

        StartCoroutine(ConstructionCoroutine());
    }

    private IEnumerator ConstructionCoroutine()
    {
        constructing = true;
        currentHP = 1;

        for (int i = 0; i < 60; i++)
        {
            currentHP += maxHP / 60f;
            yield return new WaitForSeconds(1f);
        }

        constructing = false;
        constructionModel.gameObject.SetActive(false);
        regularModel.gameObject.SetActive(true);
    }
}
