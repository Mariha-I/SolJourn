using UnityEngine;

public class CelestialClickHandler : MonoBehaviour
{
    public CelestialInfoPopup popup;
    public string bodyName;
    public float  ra, dec;
    public bool   isDynamic;
    public Camera mainCam;

    public float tapRadius = 80f; // pixels

    void Update()
    {
        bool tapped = false;
        Vector2 tapPos = Vector2.zero;

        if (Input.touchCount == 1 &&
            Input.GetTouch(0).phase == TouchPhase.Began)
        {
            tapped = true;
            tapPos = Input.GetTouch(0).position;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            tapped = true;
            tapPos = Input.mousePosition;
        }

        if (!tapped) return;

        // Convert this object's world position to screen position
        Vector3 screenPos = mainCam.WorldToScreenPoint(transform.position);

        // If behind camera, ignore
        if (screenPos.z < 0) return;

        float dist = Vector2.Distance(tapPos, new Vector2(screenPos.x, screenPos.y));

        if (dist < tapRadius)
        {
            popup.ShowInfo(bodyName, ra, dec, isDynamic);
        }
    }
}