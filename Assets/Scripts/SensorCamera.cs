using UnityEngine;

public class SensorCamera : MonoBehaviour
{
    public DeviceSensors sensors;
    public bool useSensors = true;  // toggle in Inspector for testing

    // For editor mouse look fallback
    private Vector3 lastMousePos;
    private float mouseSpeed = 0.3f;

    // Smoothing
    private Quaternion smoothedRotation;
    private float smoothSpeed = 15f;

    void Start()
    {
        smoothedRotation = transform.rotation;
        Input.compass.enabled = true;
    }

    void Update()
    {
        if (useSensors && Application.isMobilePlatform)
        {
            SensorLook();
            PinchZoom();
        }
        else
        {
            MouseLook(); // use mouse in editor
        }
    }

    void SensorLook()
    {
        Quaternion target = sensors.GetGyroRotation();

        // Smooth the rotation to prevent jitter
        smoothedRotation = Quaternion.Slerp(
            smoothedRotation, target, Time.deltaTime * smoothSpeed);

        transform.rotation = smoothedRotation;
    }

    void MouseLook()
    {
        if (Input.GetMouseButtonDown(0))
            lastMousePos = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            transform.Rotate(Vector3.up,   -delta.x * mouseSpeed, Space.World);
            transform.Rotate(Vector3.right, delta.y * mouseSpeed, Space.Self);
            lastMousePos = Input.mousePosition;
        }

        // Scroll zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            Camera cam = GetComponent<Camera>();
            if (cam) cam.fieldOfView = Mathf.Clamp(
                cam.fieldOfView - scroll * 10f, 10f, 90f);
        }
    }
    void PinchZoom()
    {
        if (Input.touchCount != 2) return;

        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        // Distance between fingers this frame and last frame
        float prevDist = (
            (t0.position - t0.deltaPosition) - 
            (t1.position - t1.deltaPosition)
        ).magnitude;
        
        float currDist = (t0.position - t1.position).magnitude;
        float diff = prevDist - currDist;

        Camera cam = GetComponent<Camera>();
        if (cam)
        {
            cam.fieldOfView = Mathf.Clamp(
                cam.fieldOfView + diff * 0.05f, 
                10f,   // max zoom in
                90f    // max zoom out
            );
        }
    }
}