using UnityEngine;
using System.Collections;

public class DeviceSensors : MonoBehaviour
{
    // Current device location
    public float latitude  = 43.0f;   // fallback default
    public float longitude = -78.8f;
    public bool  locationReady = false;

    // Smoothing for compass
    private float smoothedHeading = 0f;
    private float headingSmoothSpeed = 5f;

    void Start()
    {
        StartCoroutine(StartLocationService());
        StartGyroscope();
    }

    // GPS
    IEnumerator StartLocationService()
    {
        // Check if user has location enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("Location not enabled by user");
            yield break;
        }

        Input.location.Start(10f, 10f); // accuracy: 10m, update threshold: 10m

        // Wait for service to start (max 20 seconds)
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogWarning("Unable to determine device location");
            yield break;
        }

        // Location ready
        latitude  = Input.location.lastData.latitude;
        longitude = Input.location.lastData.longitude;
        locationReady = true;
        Debug.Log($"Location ready: {latitude}, {longitude}");
    }

    // Gyroscope
    void StartGyroscope()
    {
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            Debug.Log("Gyroscope enabled");
        }
        else
        {
            Debug.LogWarning("Gyroscope not supported on this device");
        }
    }

    // Returns smoothed compass heading in degrees (0 = North)
    public float GetHeading()
    {
        float target = Input.compass.trueHeading;
        smoothedHeading = Mathf.LerpAngle(smoothedHeading, target, 
                          Time.deltaTime * headingSmoothSpeed);
        return smoothedHeading;
    }

    // Returns gyroscope rotation as a Unity quaternion
    public Quaternion GetGyroRotation()
    {
        // Raw gyro attitude
        Quaternion raw = Input.gyro.attitude;

        // Convert from gyro space to Unity space
        // Gyro uses right-handed coordinates, Unity uses left-handed
        Quaternion fix = new Quaternion(raw.x, raw.y, -raw.z, -raw.w);

        // Rotate to account for screen orientation (portrait)
        Quaternion screenFix = Quaternion.Euler(90f, 0f, 0f);

        return screenFix * fix;
    }

    void Update()
    {
        // Update location if it drifts significantly
        if (locationReady)
        {
            latitude  = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;
        }
    }
}