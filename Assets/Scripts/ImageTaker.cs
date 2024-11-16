    using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImageTaker : MonoBehaviour
{
    private Button scanner;
    private bool capturing = false;
    private int captureCount = 0;

    void Start()
    {
        scanner = GetComponent<Button>();
        scanner.onClick.AddListener(HandleCapture);
    }

    void HandleCapture()
    {
        if (capturing) StopCapturing();
        else StartCapturing();
    }

    void StartCapturing()
    {
        scanner.GetComponentInChildren<TMP_Text>().text = "Stop Scan";
        scanner.GetComponentInChildren<TMP_Text>().color = Color.white;
        scanner.image.color = Color.red;
        capturing = true;

        while (capturing && captureCount < 100)
        {
            captureCount++;
            TakePhotoAsync();
        }

        StopCapturing();
    }

    void StopCapturing()
    {
        scanner.GetComponentInChildren<TMP_Text>().text = "Start Scan";
        scanner.GetComponentInChildren<TMP_Text>().color = new Color32(50, 50, 50, 255);
        scanner.image.color = Color.white;
        capturing = false;
        captureCount = 0;
    }

    async void TakePhotoAsync()
    {
        Texture2D photo = TakePhoto();
        if (photo != null)
        {
            await Task.Run(() => SavePhoto(photo));
            Destroy(photo);
        }
    }

    Texture2D TakePhoto()
    {
        Camera camera = Camera.main;
        int width = Screen.width / 2; // Reduce resolution for faster capture
        int height = Screen.height / 2;

        RenderTexture rt = new(width, height, 24);
        camera.targetTexture = rt;

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = rt;

        camera.Render();

        Texture2D image = new(width, height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();

        camera.targetTexture = null;
        RenderTexture.active = currentRT;

        Destroy(rt);
        return image;
    }

    void SavePhoto(Texture2D image)
    {
        byte[] bytes = image.EncodeToJPG(75); // Reduce quality for faster encoding
        string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(filePath, bytes);
    }
}
