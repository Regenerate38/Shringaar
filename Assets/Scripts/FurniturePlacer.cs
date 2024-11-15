using cakeslice;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System;

public class PlacementManager : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public GameObject pointerObj;
    public GameObject furniturePrefab; 
    private float speedModifier = 0.0005f;
    private Vector3 translationVector;
    private GameObject activeObject;
    private float previousRotationAngle = 0f;
    private float currentRotationAngle = 0f;
    private float rotateSpeed = 2f;
    private bool isNearObject = false;
    private Vector3 demoPose;
    public ArSession session;

    public string uploadURL = "https://d474-27-34-49-81.ngrok-free.app/api/image/upload";
    public Image uiImage; // Assign your UI Image component here



    private List<GameObject> placedFurniture = new List<GameObject>(); // placed furniture list for managing selection

    void Start()
    {

        raycastManager = FindObjectOfType<ARRaycastManager>();

        pointerObj = this.transform.GetChild(0).gameObject;
        pointerObj.SetActive(false);
        UploadImage(); 
    }
    public void UploadImage()
    {
        byte[] imageBytes = GetImageBytes();
        Debug.Log("Image chha ki chaina"+imageBytes.ToString());
        StartCoroutine(Upload(imageBytes));
    }

    void Update()
    {
        List<ARRaycastHit> hitPoints = new List<ARRaycastHit>();
        raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hitPoints, TrackableType.PlaneWithinBounds);

        if (hitPoints.Count > 0 && !isNearObject)
        {
            pointerObj.transform.position = hitPoints[0].pose.position;
            pointerObj.transform.rotation = hitPoints[0].pose.rotation;
            demoPose = hitPoints[0].pose.position;

            if (!pointerObj.activeInHierarchy)
            {
                pointerObj.SetActive(true);
            }
        }
        if (Input.GetTouch(0).phase == TouchPhase.Moved && activeObject != null)
        {
            translationVector = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z);
            activeObject.transform.Translate(translationVector * Input.GetTouch(0).deltaPosition.y * speedModifier, Space.World);

            translationVector = new Vector3(Camera.main.transform.right.x, 0f, Camera.main.transform.right.z);
            activeObject.transform.Translate(translationVector * Input.GetTouch(0).deltaPosition.x * speedModifier, Space.World);
        }

        previousRotationAngle = currentRotationAngle;
        if (Input.touchCount == 2 && !Input.GetTouch(0).position.IsPointOverUIObject() && !Input.GetTouch(1).position.IsPointOverUIObject())
        {
            currentRotationAngle = Mathf.Atan((Input.GetTouch(0).position.y - Input.GetTouch(1).position.y) / (Input.GetTouch(0).position.x - Input.GetTouch(1).position.x));
            if ((currentRotationAngle - previousRotationAngle) > 0)
            {
                activeObject.transform.Rotate(0, -rotateSpeed, 0);
            }
            if ((currentRotationAngle - previousRotationAngle) < 0)
            {
                activeObject.transform.Rotate(0, rotateSpeed, 0);
            }
        }

        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            Debug.Log("Touch Coming");
            bool isOverUI = Input.GetTouch(0).position.IsPointOverUIObject();
            Debug.Log("Is pointer over UI? " + isOverUI);

            Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit raycastHit;

            if (Physics.Raycast(raycast, out raycastHit))
            {
                Debug.Log("Raycast Hit: " + raycastHit.collider.name);
                if (!isOverUI && raycastHit.collider.CompareTag("Furniture"))
                {
                    Debug.Log("Object Selected");
                    HighlightObject(raycastHit.collider.gameObject);
                }
            }
            else if (!isOverUI)
            {

                DeselectAllFurniture();
                activeObject = null;
            }
        }
        else
        {

        }
    }

    private void HighlightObject(GameObject furniture)
    {
        activeObject = furniture;

        foreach (Transform layerParent in furniture.transform) // L 1
        {
            foreach (Transform layer in layerParent) // L 2 
            {
                foreach (Transform meshParent in layer)
                {
                    foreach (Transform mesh in meshParent) // Mesh
                    {

                        if (mesh.GetComponent<MeshRenderer>() != null)
                        {

                            cakeslice.Outline outline = mesh.gameObject.AddComponent<cakeslice.Outline>();
                            outline.color = 2;

                            Debug.Log($"caskeslice.Outline added to: {mesh.name}");
                        }
                    }
                }
            }
        }

    }

    private void DeselectAllFurniture()
    {
        cakeslice.Outline[] outlines = FindObjectsOfType<cakeslice.Outline>();

        foreach (cakeslice.Outline outline in outlines)
        {
            Destroy(outline);
            Debug.Log($"Removed Outline from: {outline.gameObject.name}");
        }
        activeObject = null;
    }

    public void PlaceFurniture(GameObject furniture)
    {
        furniturePrefab = furniture;
        if (furniturePrefab != null && pointerObj.activeInHierarchy)
        {
            GameObject furnitureInstance = Instantiate(furniturePrefab, demoPose, Quaternion.identity);
            furnitureInstance.gameObject.tag = "Furniture";
            var cameraForward = Camera.current.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            furnitureInstance.transform.rotation = Quaternion.LookRotation(cameraBearing);

            furnitureInstance.SetActive(true);

            placedFurniture.Add(furnitureInstance);
            pointerObj.SetActive(false);
        }
        else
        {
            Debug.LogError("Furniture prefab is not assigned!");
        }
    }
    private IEnumerator Upload(byte[] imageData)
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormFileSection("file", imageData, "uploaded_image.png", "image/*") // Adjust MIME type if necessary
        };
        Debug.Log("Mime ready");

        using (UnityWebRequest www = UnityWebRequest.Post(uploadURL, formData))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error during upload: " + www.error);
            }
            else
            {
                Debug.Log("Upload complete: " + www.downloadHandler.text);
            }
        }
        Debug.Log("Sent");
    }
    private byte[] GetImageBytes()
    {

        Debug.Log("Mula");
        Sprite sprite = uiImage.sprite;
        Debug.Log("UiImage chha yaar" + uiImage);
        Debug.Log("Sprite chha yaar" + uiImage.sprite);
        if (sprite == null)
        {
            Debug.LogError("No sprite assigned to UI Image.");
            return null;
        }

        // Create a new Texture2D with the same dimensions as the sprite
        Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        Debug.Log("Texture first" + texture);
        
        // Set pixels from the sprite's texture
        texture.SetPixels(sprite.texture.GetPixels(
            (int)sprite.textureRect.x,
            (int)sprite.textureRect.y,
            (int)sprite.textureRect.width,
            (int)sprite.textureRect.height));

        Debug.Log("Texture second" + texture);

        texture.Apply(); // Apply changes to the texture

        return ImageConversion.EncodeToPNG(texture);  // Convert to PNG format
    }
}
