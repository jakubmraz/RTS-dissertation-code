using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGhost : MonoBehaviour
{
    private Transform visual;
    private Building building;

    private void Start()
    {
        RefreshVisual();
    }

    private void LateUpdate()
    {
        if (building != null)
        {
            Vector3 targetPosition = GridBuildingSystem.Instance.GetMouseWorldSnappedPosition();
            Debug.Log(targetPosition);
            targetPosition.y = 1f;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 15f);
        }
    }

    private void RefreshVisual()
    {
        if (visual != null)
        {
            Destroy(visual.gameObject);
            visual = null;
        }

        if (building != null)
        {
            visual = Instantiate(building.visual, Vector3.zero, Quaternion.identity);
            visual.parent = transform;
            visual.localPosition = Vector3.zero;
            visual.localEulerAngles = Vector3.zero;
            SetLayerRecursive(visual.gameObject, 6);
        }
    }

    private void SetLayerRecursive(GameObject targetGameObject, int layer)
    {
        targetGameObject.layer = layer;
        foreach (Transform child in targetGameObject.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    public void HideBuildingGhost()
    {
        building = null;
        RefreshVisual();
    }

    public void ShowBuildingGhost(Building building)
    {
        this.building = building;
        RefreshVisual();
    }

}

