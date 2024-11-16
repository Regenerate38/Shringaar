using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScannerManager : MonoBehaviour
{
    private Button scanner;
    private bool scanning;
    private float lastLinearAccelerationX;
    private float gravityX;
    private float elapsedTime = 0f; // Timer to track elapsed time
    private const float LOG_INTERVAL = 0.1f; // Interval for logging speed
    private const float GRAVITY_FILTER_FACTOR = 0.8f; // Adjust this to fine-tune gravity filtering

    void Start()
    {
        scanner = GetComponent<Button>();
        scanner.onClick.AddListener(HandleScan);
        gravityX = Input.acceleration.x; // Initialize gravity vector for X-axis
        lastLinearAccelerationX = 0f;
    }

    void Update()
    {
        GetSpeed();
    }

    void HandleScan()
    {
        if (!scanning) StartScan();
        else StopScan();
    }

    void StartScan()
    {
        scanning = true;
        scanner.image.color = Color.red;
        elapsedTime = 0f; // Reset timer when scanning starts
    }

    void StopScan()
    {
        scanning = false;
        scanner.image.color = Color.white;
    }

    void GetSpeed()
    {
        if (!scanning) return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= LOG_INTERVAL)
        {
            float rawAccelerationX = Input.acceleration.x;

            // Separate gravity and linear acceleration using a low-pass filter for the X-axis
            gravityX = Mathf.Lerp(gravityX, rawAccelerationX, GRAVITY_FILTER_FACTOR);
            float linearAccelerationX = rawAccelerationX - gravityX;

            const int SPEED_SCALE = 100000;
            int speed;

            float deltaAccelerationX = linearAccelerationX - lastLinearAccelerationX;
            speed = (int)(Mathf.Abs(deltaAccelerationX) * LOG_INTERVAL * SPEED_SCALE / 50);
            lastLinearAccelerationX = linearAccelerationX;

            Debug.Log("Horizontal Speed (X-Axis Only): " + speed);
            // You can add conditions for specific speed ranges
            // if (speed < 2) Debug.Log("Speed too Slow.");
            // else if (speed > 10) Debug.Log("Speed too Fast.");

            elapsedTime = 0f; // Reset the timer
        }
    }
}
