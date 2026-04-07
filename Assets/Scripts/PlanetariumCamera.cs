using UnityEngine;

public class PlanetariumCamera : MonoBehaviour
{
    public float dragSpeed = 0.3f;
    public float fovMin = 10f;
    public float fovMax = 90f;

    private Vector3 lastMousePos;
    private Camera cam;

    void Start() => cam = GetComponent<Camera>();

    void Update()
    {
        HandleMouseLook();
        HandlePinchZoom();
    }

    void HandleMouseLook()
    {
        if (Input.GetMouseButtonDown(0))
            lastMousePos = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            transform.Rotate(Vector3.up,    -delta.x * dragSpeed, Space.World);
            transform.Rotate(Vector3.right,  delta.y * dragSpeed, Space.Self);
            lastMousePos = Input.mousePosition;
        }
    }

    void HandlePinchZoom()
    {
        // Mouse scroll wheel (editor / desktop)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - scroll * 10f, fovMin, fovMax);

        // Two finger pinch (mobile)
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            float prevDist = (t0.position - t0.deltaPosition -
                             (t1.position - t1.deltaPosition)).magnitude;
            float currDist = (t0.position - t1.position).magnitude;

            float diff = prevDist - currDist;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView + diff * 0.05f, fovMin, fovMax);
        }
    }
}