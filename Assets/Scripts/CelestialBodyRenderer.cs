using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;

public class CelestialBodyRenderer : MonoBehaviour
{
    [Header("References")]
    public StarRenderer starRenderer;
    public Camera mainCamera;

    [Header("Colors")]
    public Color planetColor    = new Color(1.0f, 0.8f, 0.3f, 1f);  // warm yellow
    public Color sunColor       = new Color(1.0f, 0.95f, 0.4f, 1f); // bright yellow
    public Color moonColor      = new Color(0.9f, 0.9f, 1.0f, 1f);  // cool white
    public Color exoplanetColor = new Color(0.4f, 1.0f, 0.7f, 1f);  // teal green

    [Header("Sizes")]
    public float planetSize    = 4.0f;
    public float sunSize       = 8.0f;
    public float moonSize      = 6.0f;
    public float exoplanetSize = 2.5f;

    [Header("Label Settings")]
    public float labelFontSize  = 150f;
    public float fadeAngle      = 30f;
    public float fadeSpeed      = 3f;

    // Internal
    private List<CelestialBodyData>     bodies      = new List<CelestialBodyData>();
    private Dictionary<string, GameObject> bodyObjs = new Dictionary<string, GameObject>();
    private Dictionary<string, TextMeshPro> bodyLabels 
        = new Dictionary<string, TextMeshPro>();

    void Start()
    {
        InitializeBodies();
        SpawnBodies();
    }

    void InitializeBodies()
    {
        // Sun
        bodies.Add(new CelestialBodyData {
            name = "Sun", bodyType = CelestialBodyType.Sun,
            labelColor = sunColor, displaySize = sunSize });

        // Moon
        bodies.Add(new CelestialBodyData {
            name = "Moon", bodyType = CelestialBodyType.Moon,
            labelColor = moonColor, displaySize = moonSize });

        // Planets
        string[] planetNames = { 
            "Mercury", "Venus", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune" };
        foreach (string pName in planetNames)
        {
            bodies.Add(new CelestialBodyData {
                name = pName, bodyType = CelestialBodyType.Planet,
                labelColor = planetColor, displaySize = planetSize });
        }

        // Exoplanets - fixed RA/Dec coordinates
        // Format: name, hostStar, RA (hours), Dec (degrees), planetCount
        var exoplanets = new (string name, string host, float ra, float dec, int count)[]
        {
            ("Proxima Centauri b", "Proxima Centauri", 14.495f, -62.676f, 1),
            ("51 Pegasi b",        "51 Pegasi",        22.957f,  20.769f, 1),
            ("HD 209458 b",        "HD 209458",        22.030f,  18.884f, 1),
            ("Kepler-22b",         "Kepler-22",        19.299f,  47.887f, 1),
            ("TRAPPIST-1b",        "TRAPPIST-1",        1.512f,  -5.041f, 7),
            ("55 Cancri e",        "55 Cancri",         8.905f,  28.330f, 5),
            ("Kepler-442b",        "Kepler-442",       19.013f,  47.458f, 1),
            ("HD 40307 g",         "HD 40307",          5.954f, -60.012f, 6),
            ("Tau Ceti e",         "Tau Ceti",          1.734f, -15.937f, 4),
            ("GJ 667C c",          "GJ 667C",          17.629f, -34.993f, 3),
        };

        foreach (var ep in exoplanets)
        {
            bodies.Add(new CelestialBodyData {
                name        = ep.name,
                bodyType    = CelestialBodyType.Exoplanet,
                labelColor  = exoplanetColor,
                displaySize = exoplanetSize,
                fixedRa     = ep.ra,
                fixedDec    = ep.dec,
                hostStar    = ep.host,
                planetCount = ep.count
            });
        }
    }

    void SpawnBodies()
    {
        DateTime utcNow = DateTime.UtcNow;
        float lat = GetLat();
        float lon = GetLon();

        foreach (CelestialBodyData body in bodies)
        {
            // Get position
            float ra, dec;
            GetBodyRaDec(body, utcNow, out ra, out dec);
            body.ra  = ra;
            body.dec = dec;

            var (alt, az) = AstroMath.RaDecToAltAz(ra, dec, lat, lon, utcNow);
            Vector3 pos = AstroMath.AltAzToWorld(alt, az, starRenderer.skyRadius);

            // Create body object
            GameObject obj = new GameObject($"Body_{body.name}");
            obj.transform.position = Vector3.zero;

            // Visual — a quad like stars but bigger
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.SetParent(obj.transform);
            quad.transform.position  = pos;
            quad.transform.LookAt(Vector3.zero);
            quad.transform.Rotate(0, 180, 0);
            quad.transform.localScale = Vector3.one * body.displaySize;

            // Material with body color
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = body.labelColor;
            mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            quad.GetComponent<Renderer>().material = mat;

            // Remove collider
            Destroy(quad.GetComponent<MeshCollider>());

            bodyObjs[body.name] = obj;

            // Label
            GameObject labelObj = new GameObject($"Label_{body.name}");
            labelObj.transform.SetParent(obj.transform);
            labelObj.transform.position = pos * 0.96f;
            labelObj.transform.LookAt(Vector3.zero);
            labelObj.transform.Rotate(0, 180, 0);

            TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();

            // Different label text for exoplanets
            if (body.bodyType == CelestialBodyType.Exoplanet)
                tmp.text = $"{body.name}\n<size=60%>{body.hostStar}</size>";
            else
                tmp.text = body.name;

            tmp.fontSize  = labelFontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = new Color(
                body.labelColor.r, body.labelColor.g, body.labelColor.b, 0f);

            RectTransform rect = labelObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600, 200);

            bodyLabels[body.name] = tmp;
        }
    }

    void Update()
    {
        DateTime utcNow = DateTime.UtcNow;
        float lat = GetLat();
        float lon = GetLon();

        foreach (CelestialBodyData body in bodies)
        {
            // Update position (planets move)
            if (body.bodyType != CelestialBodyType.Exoplanet)
            {
                float ra, dec;
                GetBodyRaDec(body, utcNow, out ra, out dec);
                body.ra  = ra;
                body.dec = dec;

                var (alt, az) = AstroMath.RaDecToAltAz(ra, dec, lat, lon, utcNow);
                Vector3 pos = AstroMath.AltAzToWorld(alt, az, starRenderer.skyRadius);

                // Update quad position
                if (bodyObjs.ContainsKey(body.name))
                {
                    Transform quad = bodyObjs[body.name].transform.GetChild(0);
                    quad.position = pos;
                    quad.LookAt(Vector3.zero);
                    quad.Rotate(0, 180, 0);

                    // Update label position
                    if (bodyLabels.ContainsKey(body.name))
                    {
                        bodyLabels[body.name].transform.position = pos * 0.96f;
                        bodyLabels[body.name].transform.LookAt(Vector3.zero);
                        bodyLabels[body.name].transform.Rotate(0, 180, 0);
                    }
                }
            }

            // Fade label based on camera angle
            FadeLabel(body);
        }
    }

    void FadeLabel(CelestialBodyData body)
    {
        if (!bodyLabels.ContainsKey(body.name)) return;

        TextMeshPro tmp = bodyLabels[body.name];
        Vector3 pos = tmp.transform.position;

        Vector3 dirToBody = (pos - mainCamera.transform.position).normalized;
        float angle = Vector3.Angle(mainCamera.transform.forward, dirToBody);

        float targetAlpha = angle < fadeAngle
            ? Mathf.InverseLerp(fadeAngle, fadeAngle * 0.3f, angle)
            : 0f;

        Color c = tmp.color;
        c.a = Mathf.MoveTowards(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
        tmp.color = c;
    }

    void GetBodyRaDec(CelestialBodyData body, DateTime utc, 
                      out float ra, out float dec)
    {
        switch (body.bodyType)
        {
            case CelestialBodyType.Sun:
                (ra, dec) = OrbitalMechanics.SunPosition(utc);
                break;
            case CelestialBodyType.Moon:
                (ra, dec) = OrbitalMechanics.MoonPosition(utc);
                break;
            case CelestialBodyType.Planet:
                Planet p = (Planet)Enum.Parse(typeof(Planet), body.name);
                (ra, dec) = OrbitalMechanics.PlanetPosition(p, utc);
                break;
            default:
                ra  = body.fixedRa;
                dec = body.fixedDec;
                break;
        }
    }

    float GetLat() => starRenderer.sensors != null && starRenderer.sensors.locationReady
        ? starRenderer.sensors.latitude  : starRenderer.latitude;
    float GetLon() => starRenderer.sensors != null && starRenderer.sensors.locationReady
        ? starRenderer.sensors.longitude : starRenderer.longitude;
}