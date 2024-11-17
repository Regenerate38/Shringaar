using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using static PlacementManager;

public class MeasureUIManager : MonoBehaviour
{
    [SerializeField] Sprite stopSprite;
    [SerializeField] XROrigin xROrigin;
    [SerializeField] GameObject lineArea;
    LineRenderer lineRenderer;
    List<TextMeshPro> textMeshPros = new();
    List<GameObject> spawnedPlacementPointers = new();
    ARPlaneManager aRPlaneManager;
    PlacementPointerManager placementPointerManager;
    Sprite startSprite;
    Button startStopButton, deleteButton, retryButton;
    List<LineRenderer> lineRenderers = new();
    List<List<TextMeshPro>> textMeshProsLists = new();
    List<List<GameObject>> spawnedPlacementPointersLists = new();

    byte[] imageBytes;
    Image targetImage;

    enum ScanState
    {
        None,
        ScanningRoom,
        ScanningPlace,
    }
    ScanState currentState = ScanState.None;
    void Start()
    {
        Button[] childrenButtons = GetComponentsInChildren<Button>();

        startStopButton = childrenButtons[1];
        deleteButton = childrenButtons[2];
        retryButton = childrenButtons[3];

        startSprite = startStopButton.image.sprite;

        startStopButton.onClick.AddListener(HandleStartStop);
        deleteButton.onClick.AddListener(HandleDelete);
        retryButton.onClick.AddListener(HandleRetry);

        deleteButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);

        aRPlaneManager = xROrigin.GetComponent<ARPlaneManager>();
        aRPlaneManager.enabled = true;

        placementPointerManager = xROrigin.GetComponent<PlacementPointerManager>();

        lineRenderer = lineArea.AddComponent<LineRenderer>();

        lineRenderer.material = placementPointerManager.material;
        lineRenderer.positionCount = 0;
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.startWidth = 0.002f;
        lineRenderer.endWidth = 0.002f;
    }

    void HandleStartStop()
    {
        switch (currentState)
        {
            case ScanState.None:
                // Start scanning the room
                aRPlaneManager.enabled = true;
                startStopButton.image.sprite = stopSprite;
                deleteButton.gameObject.SetActive(true);
                retryButton.gameObject.SetActive(true);
                currentState = ScanState.ScanningRoom;
                return;

            case ScanState.ScanningRoom:
                currentState = ScanState.ScanningPlace;
                InstantiateNewLineRenderer();
                return;

            case ScanState.ScanningPlace:
                startStopButton.image.sprite = startSprite;
                deleteButton.gameObject.SetActive(false);
                retryButton.gameObject.SetActive(false);
                aRPlaneManager.enabled = false;
                startStopButton.gameObject.SetActive(false);

                OpenGallery();
                return;

        }
    }

    void InstantiateNewLineRenderer()
    {
        lineRenderers.Add(placementPointerManager.lineRenderer);
        textMeshProsLists.Add(placementPointerManager.textMeshPros);
        spawnedPlacementPointersLists.Add(placementPointerManager.spawnedPlacementPointers);
        placementPointerManager.lineRenderer = lineRenderer;
        placementPointerManager.textMeshPros = textMeshPros;
        placementPointerManager.spawnedPlacementPointers = spawnedPlacementPointers;
        lineRenderers.Add(lineRenderer);
        textMeshProsLists.Add(textMeshPros);
        spawnedPlacementPointersLists.Add(spawnedPlacementPointers);
    }

    void HandleDelete()
    {
        int pointCount = placementPointerManager.lineRenderer.positionCount;
        if (pointCount == 0 && lineRenderers.Count > 1)
        {
            placementPointerManager.lineRenderer = lineRenderers[0];
            placementPointerManager.textMeshPros = textMeshProsLists[0];
            placementPointerManager.spawnedPlacementPointers = spawnedPlacementPointersLists[0];
            lineRenderers.RemoveAt(1);
            textMeshProsLists.RemoveAt(1);
            spawnedPlacementPointersLists.RemoveAt(1);
        }
        if (pointCount < 1) return;
        pointCount--;
        placementPointerManager.lineRenderer.positionCount -= 1;
        Destroy(placementPointerManager.spawnedPlacementPointers[pointCount]);
        placementPointerManager.spawnedPlacementPointers.RemoveAt(pointCount);
        if (pointCount == 0) return;
        pointCount--;
        Destroy(placementPointerManager.textMeshPros[pointCount]);
        placementPointerManager.textMeshPros.RemoveAt(pointCount);
    }

    public void PickImage()
    {
        NativeGallery.Permission permission = NativeGallery.CheckPermission(NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
        if (permission == NativeGallery.Permission.Granted)
        {
            OpenGallery();
        }
        else if (permission == NativeGallery.Permission.ShouldAsk)
        {
            // Request permission
            NativeGallery.RequestPermissionAsync((result) =>
            {
                if (result == NativeGallery.Permission.Granted)
                {
                    OpenGallery();
                }
            }, NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
        }
    }

    private void OpenGallery()
    {
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                imageBytes = NativeGallery.LoadImageAtPath(path, -1, false).EncodeToPNG();
                Texture2D texture = NativeGallery.LoadImageAtPath(path);

                StartCoroutine(Upload(imageBytes));
            }
            else
            {
                Debug.Log("No image selected");
            }
        }, "Select an image", "image/*");
    }

    public class Furniture
    {
        public string furniture_name;
        public float x, y, orientation;
    }
    public class PostResponse
    {
        public Furniture[] data;
    }
    public class PostBody
    {
        public string url;
        public float[][] room_coord;
    };

    private IEnumerator Upload(byte[] imageData)
    {
        int pointCount = lineRenderers[0].positionCount;

        float[][] pointsArray = new float[pointCount][];
        Vector3[] positions = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++) 
            pointsArray[i] = new float[2] { positions[i].x, positions[i].y };

        Debug.Log("Logging.");
        Debug.Log("Length: "+ pointsArray.Length);
        PostBody postBody = new() { url = "https://live.staticflickr.com/6110/6362536639_67c180b7f5_b.jpg", room_coord = pointsArray };
        using (UnityWebRequest www = UnityWebRequest.Post("http://192.168.81.102:3000/api/place", JsonUtility.ToJson(postBody), "application/json"))
        {
            // Send the request and wait for a response
            yield return www.SendWebRequest();
                
            // Check for errors
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error uploading image: {www.error}");
            }
            else
            {
                Debug.Log("Image upload complete!" + www.downloadHandler.text);
                string responseText = www.downloadHandler.text;
                PostResponse postResponse = JsonUtility.FromJson<PostResponse>(responseText);
                foreach(var furniture in postResponse.data ) { Debug.Log(furniture.furniture_name); }
                //filepath = responseData.uploaded_files.filepath;
                //Debug.Log("Filepath: " + filepath);

            }
        }
    }

        void HandleRetry()
    {
        // InstantiateNewLineRenderer();
    }
}
