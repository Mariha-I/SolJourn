using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class CelestialInfoPopup : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public StarRenderer starRenderer;

    private Canvas canvas;
    private GameObject popup;
    private TextMeshProUGUI popupText;
    private string currentTarget = "";
    private SensorCamera sensorCamera;
    private Quaternion   savedRotation;
    private Vector3      targetWorldPos;
    private Coroutine animCoroutine;

    // Colors
    private Color bgColor   = new Color(0.04f, 0.04f, 0.14f, 0.95f);
    private Color closeColor = new Color(0.35f, 0.08f, 0.08f, 1f);

    void Start()
    {
        sensorCamera = mainCamera.GetComponent<SensorCamera>();
        BuildCanvas();
        BuildPopup();
    }

    void BuildCanvas()
    {
        GameObject go = new GameObject("InfoCanvas");
        canvas = go.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        CanvasScaler cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1080, 1920);
        cs.matchWidthOrHeight  = 0.5f;

        go.AddComponent<GraphicRaycaster>();
    }

    void BuildPopup()
    {
        popup = new GameObject("Popup");
        popup.transform.SetParent(canvas.transform, false);

        Image bg = popup.AddComponent<Image>();
        bg.color = bgColor;

        RectTransform rt = popup.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(480, 320);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        // Text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(popup.transform, false);
        popupText = textGO.AddComponent<TextMeshProUGUI>();
        popupText.fontSize           = 18;
        popupText.color              = Color.white;
        popupText.enableWordWrapping = true;
        popupText.alignment          = TextAlignmentOptions.TopLeft;

        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(16, 46);
        textRT.offsetMax = new Vector2(-16, -14);

        // Close button
        GameObject closeGO = new GameObject("CloseBtn");
        closeGO.transform.SetParent(popup.transform, false);

        Image closeImg = closeGO.AddComponent<Image>();
        closeImg.color = closeColor;

        RectTransform closeRT = closeGO.GetComponent<RectTransform>();
        closeRT.anchorMin        = new Vector2(1, 1);
        closeRT.anchorMax        = new Vector2(1, 1);
        closeRT.pivot            = new Vector2(1, 1);
        closeRT.anchoredPosition = new Vector2(-6, -6);
        closeRT.sizeDelta        = new Vector2(40, 40);

        Button closeBtn = closeGO.AddComponent<Button>();
        closeBtn.targetGraphic = closeImg;
        closeBtn.onClick.AddListener(ClosePopup);

        GameObject closeTextGO = new GameObject("X");
        closeTextGO.transform.SetParent(closeGO.transform, false);
        TextMeshProUGUI closeTxt = closeTextGO.AddComponent<TextMeshProUGUI>();
        closeTxt.text      = "X";
        closeTxt.fontSize  = 18;
        closeTxt.alignment = TextAlignmentOptions.Center;
        closeTxt.color     = Color.white;

        RectTransform closeTxtRT = closeTextGO.GetComponent<RectTransform>();
        closeTxtRT.anchorMin = Vector2.zero;
        closeTxtRT.anchorMax = Vector2.one;
        closeTxtRT.offsetMin = closeTxtRT.offsetMax = Vector2.zero;

        popup.SetActive(false);
    }

    public void ShowInfo(string bodyName, float ra, float dec, bool isDynamic)
    {
        if (currentTarget == bodyName && popup.activeSelf)
        {
            ClosePopup();
            return;
        }

        currentTarget = bodyName;

        float lat    = GetLat();
        float lon    = GetLon();
        DateTime utc = DateTime.UtcNow;

        if (isDynamic)
            GetDynamicRaDec(bodyName, utc, out ra, out dec);

        var (alt, az) = AstroMath.RaDecToAltAz(ra, dec, lat, lon, utc);
        Vector3 euler = mainCamera.transform.eulerAngles;
        string above  = alt >= 0 ? "above" : "below";
        string desc   = GetDescription(bodyName);

        popupText.text =
            $"<b><color=#AADDFF>{bodyName}</color></b>\n\n" +
            $"<b>Sky Position</b>\n" +
            $"  Alt: <color=#AAFFAA>{alt:F1}°</color> ({above} horizon)" +
            $"    Az: <color=#AAFFAA>{az:F1}°</color>\n\n" +
            $"<b>Camera</b>\n" +
            $"  Heading: <color=#AAFFAA>{euler.y:F1}°</color>" +
            $"    Pitch: <color=#AAFFAA>{euler.x:F1}°</color>\n" +
            $"  GPS: <color=#AAFFAA>{lat:F3}°, {lon:F3}°</color>\n\n" +
            $"<size=85%><color=#99AABB>{desc}</color></size>";

        popup.GetComponent<RectTransform>().sizeDelta =
            string.IsNullOrEmpty(desc)
            ? new Vector2(480, 200)
            : new Vector2(480, 360);

        // Save current rotation before freezing
        savedRotation  = mainCamera.transform.rotation;
        targetWorldPos = AstroMath.AltAzToWorld(alt, az, starRenderer.skyRadius);

        // Freeze sensors
        if (sensorCamera != null)
            sensorCamera.useSensors = false;

        // Pan camera to center object above popup
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(PanToTarget(targetWorldPos));

        popup.SetActive(true);
    }

    IEnumerator PanToTarget(Vector3 worldPos)
    {
        // We want the object to appear in the upper half of the screen
        // so it sits above the popup at the bottom
        // Tilt the camera slightly downward so the object appears higher on screen
        Vector3 dir          = worldPos.normalized;
        Quaternion targetRot = Quaternion.LookRotation(dir);

        // Apply a small downward offset so object sits above the popup
        targetRot = targetRot * Quaternion.Euler(15f, 0, 0);

        Quaternion startRot = mainCamera.transform.rotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 2.0f;
            mainCamera.transform.rotation = Quaternion.Slerp(
                startRot, targetRot, Mathf.SmoothStep(0, 1, Mathf.Clamp01(t)));
            yield return null;
        }

        mainCamera.transform.rotation = targetRot;
    }

    void ClosePopup()
    {
        popup.SetActive(false);
        currentTarget = "";

        // Restore sensors — camera snaps back to device orientation
        if (sensorCamera != null)
            sensorCamera.useSensors = true;

        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
            animCoroutine = null;
        }
    }

    void GetDynamicRaDec(string name, DateTime utc, out float ra, out float dec)
    {
        switch (name)
        {
            case "Sun":
                (ra, dec) = OrbitalMechanics.SunPosition(utc);
                return;
            case "Moon":
                (ra, dec) = OrbitalMechanics.MoonPosition(utc);
                return;
            default:
                if (Enum.TryParse(name, out Planet p))
                {
                    (ra, dec) = OrbitalMechanics.PlanetPosition(p, utc);
                    return;
                }
                break;
        }
        ra = dec = 0;
    }

    string GetDescription(string name)
    {
        switch (name)
        {
            case "Sun":     return "The star at the center of our solar system. Its core reaches 15 million °C where nuclear fusion converts hydrogen to helium, releasing the energy that sustains all life on Earth.";
            case "Moon":    return "Earth's natural satellite, formed 4.5 billion years ago from debris after a Mars-sized body struck early Earth. It stabilizes Earth's axial tilt, moderating our climate.";
            case "Mercury": return "The smallest planet and closest to the Sun. Surface temperatures swing from -180°C at night to 430°C during the day — the most extreme temperature range of any planet.";
            case "Venus":   return "The hottest planet at 465°C due to a runaway greenhouse effect. It rotates backwards — the Sun rises in the west. A day on Venus is longer than its year.";
            case "Mars":    return "A cold desert world hosting Olympus Mons, the largest volcano in the solar system. Ancient river valleys suggest liquid water once flowed on its surface.";
            case "Jupiter": return "The largest planet — over twice the mass of all others combined. Its Great Red Spot is a storm raging for 350+ years. It has 95 known moons.";
            case "Saturn":  return "Known for its spectacular ring system of ice and rock. The least dense planet — it would float in water. It has 146 known moons, more than any other planet.";
            case "Uranus":  return "An ice giant tilted 98° on its side, so its poles get more sunlight than its equator. Its blue-green color comes from methane absorbing red light.";
            case "Neptune": return "The windiest planet with storms reaching 2,100 km/h. First located through math before it was seen. Its moon Triton orbits backwards and is slowly spiraling inward.";
            case "Proxima Centauri b": return "The closest known exoplanet at 4.2 light-years, orbiting in Proxima Centauri's habitable zone. Whether it holds an atmosphere against stellar flares is unknown.";
            case "51 Pegasi b":        return "The first exoplanet found orbiting a Sun-like star (1995). A hot Jupiter completing an orbit every 4 days, it changed our understanding of how planetary systems form.";
            case "HD 209458 b":        return "Known as Osiris — the first exoplanet seen transiting its star and the first found to have an evaporating atmosphere, with hydrogen streaming into space.";
            case "Kepler-22b":         return "One of the first exoplanets confirmed in a habitable zone. About 2.4x Earth's radius — whether it is rocky, oceanic or gaseous remains unknown.";
            case "TRAPPIST-1b":        return "Part of a system with seven Earth-sized planets, three in the habitable zone. A top target for atmospheric study with the James Webb Space Telescope.";
            case "55 Cancri e":        return "A super-Earth completing a year in just 18 hours. Surface temperatures may melt rock. Early data suggested a carbon-rich interior — possibly diamond.";
            case "Kepler-442b":        return "One of the most Earth-like exoplanets known, with an Earth Similarity Index of 0.84. It orbits a cooler star and receives about 70% of Earth's sunlight.";
            case "HD 40307 g":         return "A super-Earth in the habitable zone of an orange dwarf. Far enough from its star that it is unlikely to be tidally locked — one side always facing the star.";
            case "Tau Ceti e":         return "Orbiting one of the nearest Sun-like stars at 11.9 light-years. Tau Ceti closely resembles our Sun, making its planets interesting targets in the search for life.";
            case "GJ 667C c":          return "A super-Earth in the habitable zone of a red dwarf within a triple star system. It receives similar energy from its star as Earth does from the Sun.";
            default:        return "";
        }
    }

    float GetLat() =>
        starRenderer.sensors != null && starRenderer.sensors.locationReady
        ? starRenderer.sensors.latitude  : starRenderer.latitude;

    float GetLon() =>
        starRenderer.sensors != null && starRenderer.sensors.locationReady
        ? starRenderer.sensors.longitude : starRenderer.longitude;
}