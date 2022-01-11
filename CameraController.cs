using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float panSpeed = 15f;
    public float dragSpeed = 15f;

    private float panBorderThickness = 10;

    private Vector3 dragOrigin;
    private Vector3 oldPos;
    private bool cameraDragging;

    void LateUpdate()
    {
        Vector3 position = transform.position;

        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            oldPos = transform.position;
            cameraDragging = true;
            return;
        }

        if (Input.GetMouseButtonUp(2))
        {
            cameraDragging = false;
            return;
        }

        if (cameraDragging)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition) - dragOrigin;
            transform.position = oldPos + -SwitchYandZ(pos) * panSpeed;
        }

        if (!cameraDragging)
        {
            if (Input.GetMouseButton(2))
            {
                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
                Vector3 move = new Vector3(pos.x * dragSpeed, 0, pos.y * dragSpeed);

                transform.Translate(move, Space.World);
            }

            if (Input.mousePosition.y >= Screen.height - panBorderThickness)
            {
                position.z += panSpeed * Time.deltaTime;
            }
            if (Input.mousePosition.y <= panBorderThickness)
            {
                position.z -= panSpeed * Time.deltaTime;
            }
            if (Input.mousePosition.x >= Screen.width - panBorderThickness)
            {
                position.x += panSpeed * Time.deltaTime;
            }
            if (Input.mousePosition.x <= panBorderThickness)
            {
                position.x -= panSpeed * Time.deltaTime;
            }

            transform.position = position;
        }
    }

    Vector3 SwitchYandZ(Vector3 originalVector)
    {
        Vector3 newVector = new Vector3();
        newVector.x = originalVector.x;
        newVector.y = originalVector.z;
        newVector.z = originalVector.y;
        return newVector;
    }
}
