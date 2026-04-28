using UnityEngine;
using System;
using System.Collections.Generic;

public class StarRenderer : MonoBehaviour
{
    public StarDatabase database;
    public float skyRadius = 450f;
    public float latitude  = 43.0f;
    public float longitude = -78.8f;
    public DeviceSensors sensors;
    

    void Start()
    {
        SpawnStars();
    }

    void SpawnStars()
    {
        DateTime utcNow = DateTime.UtcNow;

        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        List<Color> colors = new List<Color>();
        float lat = (sensors != null && sensors.locationReady) ? sensors.latitude  : latitude;
        float lon = (sensors != null && sensors.locationReady) ? sensors.longitude : longitude;

        int starIndex = 0;
        foreach (StarData star in database.stars)
        {
            var (alt, az) = AstroMath.RaDecToAltAz(
                star.ra, star.dec, lat, lon, utcNow);

            Vector3 pos = AstroMath.AltAzToWorld(alt, az, skyRadius);

            vertices.Add(pos);
            indices.Add(starIndex);
            colors.Add(StarColor(star.magnitude));
            starIndex++;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Points, 0);
        mesh.RecalculateBounds();

        GameObject starField = new GameObject("StarField");
        starField.transform.position = Vector3.zero;

        MeshFilter mf = starField.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = starField.AddComponent<MeshRenderer>();
        mr.material = CreateStarMaterial();

        // Remove old StarField if it exists
        Debug.Log($"Star field created with {vertices.Count} points");
    }

    Material CreateStarMaterial()
    {
        Material mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = Color.white;
        mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        return mat;
    }

    float StarSize(float magnitude)
    {
        return Mathf.Lerp(1.5f, 0.3f, Mathf.InverseLerp(-1.5f, 6.5f, magnitude));
    }

    Color StarColor(float magnitude)
    {
        float t = Mathf.InverseLerp(-1.5f, 6.5f, magnitude);
        return Color.Lerp(Color.white, new Color(0.6f, 0.6f, 0.6f), t);
    }
}