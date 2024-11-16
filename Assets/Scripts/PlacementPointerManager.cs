using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using TMPro;

public class PlacementPointerManager : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] GameObject placementPointer;
    [SerializeField] TextMeshPro textMeshPro;
    GameObject spawnedPlacementPointer;
    TouchControls controls;
    ARRaycastManager aRRaycastManager;
    List<ARRaycastHit> hits = new();
    void Awake()
    {
        aRRaycastManager = GetComponent<ARRaycastManager>();
        controls = new TouchControls();
        // controls.Control.Touch.performed += _ => isPressed = true;
        // controls.Control.Touch.canceled += _ => isPressed = false;
        controls.Control.Touch.performed += ctx =>
        {
            if (ctx.control.device is not Pointer device) return;
            OnPress(device.position.ReadValue());
        };

    }

    // Update is called once per frame
    // void Update()
    // {
    //     if (Pointer.current == null || !isPressed) return;
    //     Vector2 touchPosition = Pointer.current.position.ReadValue();
    //     if (!aRRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon)) return;
    //     Pose hitPose = hits[0].pose;
    //     if (spawnedPlacementPointer == null) { spawnedPlacementPointer = Instantiate(placementPointer, hitPose.position, hitPose.rotation); return; }
    //     spawnedPlacementPointer.transform.position = hitPose.position;
    //     spawnedPlacementPointer.transform.rotation = hitPose.rotation;
    //     Vector3 lookPos = Camera.main.transform.position - spawnedPlacementPointer.transform.position;
    //     lookPos.y = 0;
    //     spawnedPlacementPointer.transform.rotation = Quaternion.LookRotation(lookPos);
    // }

    void OnEnable() { controls.Control.Enable(); }
    void OnDisable() { controls.Control.Disable(); }

    void OnPress(Vector3 touchPosition)
    {
        if (Input.GetTouch(0).position.IsPointOverUIObject()) return;
        if (!aRRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon)) return;

        Pose hitPose = hits[0].pose;

        if (lineRenderer.positionCount > 0)
        {
            float distance = Vector3.Distance(touchPosition, lineRenderer.GetPosition(lineRenderer.positionCount - 1));
            distance = ((int)(distance * 1000)) / 10f;
            if (distance < 5f) return;
        }

        spawnedPlacementPointer = Instantiate(placementPointer, hitPose.position, hitPose.rotation);
        DrawLine(hitPose.position, spawnedPlacementPointer);
    }

    void DrawLine(Vector3 hitPosition, GameObject spawnedPlacementPointer)
    {
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, hitPosition);
        if (lineRenderer.positionCount < 2) return;
        Vector3 previousPointPosition = lineRenderer.GetPosition(lineRenderer.positionCount - 2);

        float distance = Vector3.Distance(hitPosition, previousPointPosition);
        distance = ((int)(distance * 1000)) / 10f;


        TextMeshPro distanceText = Instantiate(textMeshPro);
        distanceText.text = distance < 100 ? "" + distance + " cm" : "" + distance / 100f + " m";

        Vector3 directionVector = previousPointPosition - hitPosition;
        Vector3 normalVector = spawnedPlacementPointer.transform.up;

        Vector3 upwardVector = Vector3.Cross(directionVector, normalVector).normalized;
        Quaternion rotation = Quaternion.LookRotation(-normalVector, upwardVector);

        distanceText.transform.rotation = rotation;
        distanceText.transform.position = hitPosition + directionVector * 0.5f + upwardVector * 0.025f;
    }
}
