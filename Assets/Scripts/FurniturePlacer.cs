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
using UnityEngine.UIElements;
using TMPro;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;


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
    public GameObject preferencesPanel;

    public TMP_Dropdown roomType;
    public TMP_Dropdown style;
    public GameObject instructionPanel;
    string uploadURL = "http://172.18.161.14:4000/api/image/upload";
    string baseURL = "http://172.18.161.14:4000/";
    public UnityEngine.UI.Image uploadImage; // Assign your UI Image component here
    public GameObject loadingPanel; 
    public UnityEngine.UI.Slider loadingSlider;
    public GameObject scrollView;
    String roomtypetext;
    String styletext;
    public UnityEngine.UI.Image targetImage;
    byte[] imageBytes;
    private ARCameraManager arCameraManager;
    private RawImage displayImage;
    string filepath;
    private List<GameObject> placedFurniture = new List<GameObject>(); // placed furniture list for managing selection
    public GameObject buttonParent;
    GameObject[] canvasElements;

    [System.Serializable]
    public class UploadedFiles
    {
        public string filename;
        public string filepath;
    }

    [System.Serializable]
    public class ResponseData
    {
        public UploadedFiles uploaded_files;
        public bool success;
    }
    
    [System.Serializable]
    public class InputData
    {
        public string style;
        public string room_type; // Note: Use snake_case to match your JSON structure
        public string room_image_path;

    }

    [System.Serializable]
    public class UploadData
    {
        public InputData input;
        public string room_image_path;

        public UploadData(InputData input, string room_image_path)
        {
            this.input = input;
            this.room_image_path = room_image_path;
        }
    }
    [System.Serializable]
    public class ScoresResponse
    {
       public string furniture_path;
        public float score;
    };

    [System.Serializable]
    public class PredictionResponse {
        public ScoresResponse[] response;
        public bool success;
    }

    PredictionResponse predictionResponse;

    void Start()
    {
        scrollView.SetActive(true);

        instructionPanel.SetActive(true);
        if (arCameraManager == null)
        {
            arCameraManager = GetComponent<ARCameraManager>();
        }

        raycastManager = FindObjectOfType<ARRaycastManager>();

        pointerObj = this.transform.GetChild(0).gameObject;
        pointerObj.SetActive(false);
        UploadImage();


        roomType.onValueChanged.AddListener(delegate {
            roomTypeValueChanged(roomType);
        });        
        
        style.onValueChanged.AddListener(delegate {
            styleValueChanged(style);
        });



    }

    private void roomTypeValueChanged(TMP_Dropdown roomType)
    {
        int index = roomType.value;

        // Get the text of the selected option
        roomtypetext = roomType.options[index].text;
    }

        private void styleValueChanged(TMP_Dropdown style)
    {
        int index = style.value;

        // Get the text of the selected option
        styletext = style.options[index].text;
    }

    public void UploadImage()
    {


       // StartCoroutine(GetRequest("http://192.168.43.140:4000/api/image"));

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

    /*
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
    */

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
        }
    }


    private IEnumerator Upload(byte[] imageData)
    {
        Debug.Log(uploadURL);
        Debug.Log("Image" + imageData);
        Debug.Log("Dropdown 1" + roomtypetext);
        Debug.Log("Dropdown 2" + styletext);

        WWWForm form = new WWWForm();
        // Log image data length for debugging
        Debug.Log($"Image data length: {imageData.Length}");


        form.AddBinaryData("file", imageData, "image.png", "image/*");

        // Create the UnityWebRequest for POST
        // using (UnityWebRequest www = UnityWebRequest.Post(uploadURL, form))
        using (UnityWebRequest www = UnityWebRequest.Post(uploadURL, form))
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
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(responseText);
                filepath = responseData.uploaded_files.filepath;
                Debug.Log("Filepath: " + filepath);

            }
        }

        // Example data to upload


           
        // Create the input data
        InputData inputData = new() { style = "modern",
        room_type = "living",
        room_image_path = "/home/sujanbaskota/Desktop/python/decorators/decorators_server/" + filepath
    };
        inputData.style = "modern";
        inputData.room_type= "living";
        inputData.room_image_path = "/home/sujanbaskota/Desktop/python/decorators/decorators_server/" + filepath;
        // UploadData uploadData = new UploadData(inputData, roomImagePath);

        // Serialize to JSON
        string jsonData = JsonUtility.ToJson(inputData);
         Debug.Log("json " + jsonData);

         // Activate the scroll view (if needed)


        form = new WWWForm();
        form.AddField("style", "modern");
        form.AddField("roomType", "bedroom");
        form.AddField("roomImagePath", "/home/sujanbaskota/Desktop/python/decorators/decorators_server/"+ filepath);
        

        //var jsonData = "{{ \"input\": { \"style\": \"modern\",\"room_type\":\"living\"},\"room_image_path\": \"/home/sujanbaskota/Desktop/python/decorators/decorators_server/uploads/Empty-Room-Decluttering-R.jpg\"}";
        // Create a UnityWebRequest for sending JSON data
        using (UnityWebRequest www = UnityWebRequest.Post($"{baseURL}api/predict/now", jsonData, "application/json"))
        {

            // Set the content type to application/json

            // Send the request and wait for a response
            yield return www.SendWebRequest();

            // Check for errors
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: "+ www.error);
            }
            else
            {
                Debug.Log("Success.");
                string responseText = www.downloadHandler.text;
                predictionResponse = JsonUtility.FromJson<PredictionResponse>(responseText);
                foreach (ScoresResponse scoreResponse in predictionResponse.response)
                {
                    Debug.Log("SR_Furniture Path:" + scoreResponse.furniture_path);
                    Debug.Log("SR_Score:" + scoreResponse.score);
                }
            }
                    EnableButtons();

        }
    }



    public void GetRecommendations()
    {
        preferencesPanel.SetActive(false);
        Debug.Log("Image chha ki chaina" + imageBytes.ToString());
        StartCoroutine(Upload(imageBytes));

    }

    public void CaptureImage()
    {
        instructionPanel.SetActive(false);
        Camera camera = Camera.main;
        int width = Screen.width / 2; 
        int height = Screen.height / 2;

        RenderTexture rt = new(width, height, 24);
        camera.targetTexture = rt;

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = rt;

        camera.Render();

        Texture2D image = new(width, height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();

        targetImage.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.zero);


        camera.targetTexture = null;
        RenderTexture.active = currentRT;
        byte[] imageBytes = image.EncodeToPNG();

        Destroy(rt);
    preferencesPanel.SetActive(true);

    }


    private void EnableButtons()
    {
        canvasElements = buttonParent.GetComponentsInChildren<GameObject>(true);

        Debug.Log("Canvas Elements" + canvasElements);
        foreach (var item in canvasElements)
        {
            foreach (ScoresResponse scoreResponse in predictionResponse.response)
            {
                if (item.name+".png" == scoreResponse.furniture_path.ToString())
                {
                    Debug.Log(item + " match " + scoreResponse.furniture_path.ToString());
                    item.SetActive(true);
                }
                else
                {
                    item.SetActive(false);

                }
            }
        }

    }

}
