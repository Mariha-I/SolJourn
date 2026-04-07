using UnityEngine;
using System;

public class StarRenderer : MonoBehaviour
{
    public StarDatabase database;
    public float skyRadius = 450f;
    public float latitude  =  40.7f;  // hardcode your city for now
    public float longitude = -74.0f;

    void Start()
    {
        SpawnStars();
    }

    void SpawnStars()
    {
        DateTime utcNow = DateTime.UtcNow;

        foreach (StarData star in database.stars)
        {
            var (alt, az) = AstroMath.RaDecToAltAz(
                star.ra, star.dec, latitude, longitude, utcNow);

            // Only render stars above horizon
            if (alt < 0) continue;

            Vector3 pos = AstroMath.AltAzToWorld(alt, az, skyRadius);

            // Create a small quad for each star
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.transform.position = pos;
            go.transform.localScale = StarSize(star.magnitude);
            go.transform.LookAt(Vector3.zero);  // face camera at center
            go.transform.Rotate(0, 180, 0);     // flip to face inward

            // Color by magnitude (brighter = slightly larger/whiter)
            Renderer r = go.GetComponent<Renderer>();
            r.material.color = StarColor(star.magnitude);
        }
    }

    Vector3 StarSize(float magnitude)
    {
        // Magnitude scale: lower number = brighter = bigger
        float size = Mathf.Lerp(0.8f, 0.1f, Mathf.InverseLerp(-1.5f, 6.5f, magnitude));
        return new Vector3(size, size, size);
    }

    Color StarColor(float magnitude)
    {
        // Bright stars slightly warm, dim stars slightly cool
        float t = Mathf.InverseLerp(-1.5f, 6.5f, magnitude);
        return Color.Lerp(Color.white, new Color(0.6f, 0.7f, 1f), t);
    }
}