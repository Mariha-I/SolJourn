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
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();
        List<Vector2> uvs = new List<Vector2>();

        float lat = (sensors != null && sensors.locationReady) ? sensors.latitude : latitude;
        float lon = (sensors != null && sensors.locationReady) ? sensors.longitude : longitude;

        int vertIndex = 0;

        foreach (StarData star in database.stars)
        {
            var (alt, az) = AstroMath.RaDecToAltAz(
                star.ra, star.dec, lat, lon, utcNow);

            Vector3 center = AstroMath.AltAzToWorld(alt, az, skyRadius);

            float size = StarSize(star.magnitude);

            // Direction toward center of sphere
            Vector3 forward = -center.normalized;

            // Build quad basis
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized * size;
            Vector3 up = Vector3.Cross(forward, right).normalized * size;

            // Quad corners
            vertices.Add(center - right - up);
            vertices.Add(center + right - up);
            vertices.Add(center + right + up);
            vertices.Add(center - right + up);

            Color c = StarColor(star.magnitude);
            colors.Add(c);
            colors.Add(c);
            colors.Add(c);
            colors.Add(c);

            uvs.Add(new Vector2(0,0));
            uvs.Add(new Vector2(1,0));
            uvs.Add(new Vector2(1,1));
            uvs.Add(new Vector2(0,1));

            triangles.Add(vertIndex + 0);
            triangles.Add(vertIndex + 1);
            triangles.Add(vertIndex + 2);

            triangles.Add(vertIndex + 0);
            triangles.Add(vertIndex + 2);
            triangles.Add(vertIndex + 3);

            vertIndex += 4;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetColors(colors);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject starField = new GameObject("StarField");
        starField.transform.position = Vector3.zero;

        MeshFilter mf = starField.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = starField.AddComponent<MeshRenderer>();
        mr.material = CreateStarMaterial();

        Debug.Log($"Created {database.stars.Count} star quads");
    }

    Material CreateStarMaterial()
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
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