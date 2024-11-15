using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class WholeRoomPlacer : MonoBehaviour
{
    public GameObject objectPrefab;
    Vector3[] closeOffsets = new Vector3[]
    {
    new Vector3(0, 0, 0),   
    new Vector3(0, 0.5f, 0), 
    new Vector3(0, -0.5f, 0), 
    //new Vector3(0.5f, 0, 0),
    //new Vector3(-0.5f, 0, 0) 
    };
    //    public ARRaycastManager raycastManager;
    private Camera arCamera;

    void Start()
    {
            arCamera = Camera.main; // Get reference to the main camera
        PlaceObject();
        //        raycastManager = FindObjectOfType<ARRaycastManager>();

    }

    void PlaceObject()
    {
        foreach (var positionOffset in closeOffsets)
        {
            
            Vector3 spawnPosition = arCamera.transform.position + arCamera.transform.TransformDirection(positionOffset);
            Quaternion spawnRotation = arCamera.transform.rotation;

            Instantiate(objectPrefab, spawnPosition, spawnRotation);
        }
           
    }
}
