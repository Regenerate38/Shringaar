using UnityEngine;

public class AccelerometerMovement : MonoBehaviour
{
    private float velocityZ = 0f; // Initial velocity
    public float decelerationFactor = 0.95f; // Factor for smooth deceleration
    public float threshold = 0.1f; // Movement threshold to detect no movement
    public float maxSpeed = 5f; // Maximum speed
    public float accelerationSmoothing = 0.1f; // Smoothing factor for acceleration input

    void Update()
    {
        // Time since last frame
        float deltaTime = Time.deltaTime;

        // Get accelerometer data
        Vector3 acceleration = Input.acceleration;
        float accelerationZ = acceleration.z;

        //Debug.Log("Speed Acceleration Z " + acceleration.z);
        //Debug.Log("Speed Acceleration X " + acceleration.x);
        //Debug.Log("Speed Acceleration Y " + acceleration.y);
        // Integrate smoothed acceleration to get velocity
        velocityZ += accelerationZ * deltaTime;

        // Check if movement is below threshold and apply deceleration
        if (Mathf.Abs(accelerationZ) < threshold)
        {
            velocityZ *= decelerationFactor; // Smoothly reduce speed
        }

        // Clamp the velocity within defined limits
        velocityZ = Mathf.Clamp(velocityZ, -maxSpeed, maxSpeed);

        Debug.Log("Speed" + velocityZ);
            }
}