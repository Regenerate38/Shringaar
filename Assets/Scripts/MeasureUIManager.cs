using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

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

                for (int i = 0; i < lineRenderers.Count; i++)
                {
                    LineRenderer lineRenderer = lineRenderers[i];
                    List<TextMeshPro> textMeshPros = textMeshProsLists[i];

                    // Get the number of points in the LineRenderer
                    int pointCount = lineRenderer.positionCount;
                    Vector3[] positions = new Vector3[pointCount];
                    lineRenderer.GetPositions(positions);

                    for (int j = 0; j < pointCount - 1; j++)
                    {
                        // Ensure textMeshPros has the expected number of elements
                        if (j < textMeshPros.Count)
                        {
                            Debug.Log($"(Point {j} vector: {positions[j]}, Point {j + 1} vector: {positions[j + 1]}, Text: {textMeshPros[j].text})");
                        }
                        else
                        {
                            Debug.LogWarning($"TextMeshPro element missing for Point {j} in LineRenderer {i}");
                        }
                    }
                }
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

    void HandleRetry()
    {
        // InstantiateNewLineRenderer();
    }
}
