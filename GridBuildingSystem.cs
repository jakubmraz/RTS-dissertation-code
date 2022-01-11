using System.Collections.Generic;
using System.Security.Cryptography;
using CodeMonkey.Utils;
using UnityEngine;

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem Instance { get; private set; }

    [SerializeField] private Building testBuilding;
    [SerializeField] private BuildingGhost buildingGhost;
    private Grid<GridObject> grid;
    private Core core;

    private bool buildingActive;

    public void StopBuilding()
    {
        buildingActive = false;
        buildingGhost.HideBuildingGhost();
    }

    public void StartBuilding(Building building)
    {
        buildingActive = true;
        testBuilding = building;
        buildingGhost.ShowBuildingGhost(building);
    }

    private void Awake()
    {
        Instance = this;

        int gridWidth = 100;
        int gridHeight = 100;
        float cellSize = 4f;
        grid = new Grid<GridObject>(gridWidth, gridHeight, cellSize, Vector3.zero,
            (g, x, z) => new GridObject(g, x, z));
        core = GetComponent<Core>();
    }

    private void Update()
    {
        if (buildingActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                grid.GetXZ(Mouse3D.GetMouseWorldPosition(), out int x, out int z);

                List<Vector2Int> gridPositionList = testBuilding.GetGridPositionList(new Vector2Int(x, z));

                bool canBuild = true;
                foreach (Vector2Int gridPosition in gridPositionList)
                {
                    if (!grid.GetGridObject(gridPosition.x, gridPosition.y).CanBuild())
                    {
                        canBuild = false;
                        break;
                    }
                }

                GridObject gridObject = grid.GetGridObject(x, z);

                if (canBuild)
                {
                    Building builtBuilding = Instantiate(testBuilding, grid.GetWorldPosition(x, z), Quaternion.identity);
                    builtBuilding.SetOrigin(new Vector2Int(x, z));

                    foreach (Vector2Int gridPosition in gridPositionList)
                    {
                        grid.GetGridObject(gridPosition.x, gridPosition.y).SetPlacedBuilding(builtBuilding);
                    }

                    gridObject.SetPlacedBuilding(builtBuilding);
                    core.StopConstructionMode(builtBuilding);
                }
                else
                {
                    Debug.Log("Cannot build here.");
                }
            }
        }

        //if (Input.GetMouseButtonDown(1))
        //{
        //    GridObject gridObject = grid.GetGridObject(Mouse3D.GetMouseWorldPosition());
        //    Building placedBuilding = gridObject.GetPlacedBuilding();
        //    if (placedBuilding != null)
        //    {
        //        List<Vector2Int> gridPositionList = testBuilding.GetGridPositionList();
        //        foreach (Vector2Int gridPosition in gridPositionList)
        //        {
        //            grid.GetGridObject(gridPosition.x, gridPosition.y).ClearPlacedBuilding();
        //        }
        //        Destroy(placedBuilding.gameObject);
        //    }
        //}
    }

    public void DestroyPlacedBuilding(Building building)
    {
        List<Vector2Int> gridPositionList = building.GetGridPositionList();
        foreach (Vector2Int gridPosition in gridPositionList)
        {
            grid.GetGridObject(gridPosition.x, gridPosition.y).ClearPlacedBuilding();
        }
        Destroy(building.gameObject);
    }

    public Vector3 GetMouseWorldSnappedPosition()
    {
        Vector3 mousePosition = Mouse3D.GetMouseWorldPosition();
        grid.GetXZ(mousePosition, out int x, out int z);

        if (testBuilding != null)
        {
            Vector3 placedObjectWorldPosition = grid.GetWorldPosition(x, z);
            return placedObjectWorldPosition;
        }
        else
        {
            return mousePosition;
        }
    }

    public Building GetTestBuilding()
    {
        return testBuilding;
    }

    public class GridObject
    {
        private Grid<GridObject> grid;
        private int x;
        private int z;
        private Building placedBuilding;

        public GridObject(Grid<GridObject> grid, int x, int z)
        {
            this.grid = grid;
            this.x = x;
            this.z = z;
        }

        public void SetPlacedBuilding(Building placedBuilding)
        {
            this.placedBuilding = placedBuilding;
            grid.TriggerGridObjectChanged(x, z);
        }

        public Building GetPlacedBuilding()
        {
            return placedBuilding;
        }

        public void ClearPlacedBuilding()
        {
            placedBuilding = null;
            grid.TriggerGridObjectChanged(x, z);
        }

        public bool CanBuild()
        {
            return placedBuilding == null;
        }

        public override string ToString()
        {
            return x + ", " + z + "\n" + placedBuilding;
        }
    }
}
