using UnityEngine;
using System;
using System.Collections.Generic;

public class StarRenderer : MonoBehaviour
{
    public StarDatabase database;
    public float skyRadius = 450f;
    public float latitude  = 43.0f;
    public float longitude = -78.8f;

    void Start()
    {
        SpawnStars();
    }

    void SpawnStars()
    {
        DateTime utcNow = DateTime.UtcNow;

        List<Vector3> vertices  = new List<Vector3>();
        List<int>     indices   = new List<int>();
        List<Color>   colors    = new List<Color>();

        foreach (StarData star in database.stars)
        {
            var (alt, az) = AstroMath.RaDecToAltAz(
                star.ra, star.dec, latitude, longitude, utcNow);

            Vector3 pos = AstroMath.AltAzToWorld(alt, az, skyRadius);

            // Each star is a tiny quad (2 triangles, 4 vertices)
            float size = StarSize(star.magnitude);
            Color col  = StarColor(star.magnitude);

            // Get vectors perpendicular to the view direction
            Vector3 dir   = pos.normalized;
            Vector3 right = Vector3.Cross(dir, Vector3.up).normalized;
            Vector3 up2   = Vector3.Cross(right, dir).normalized;

            int idx = vertices.Count;

            vertices.Add(pos + (-right - up2) * size);
            vertices.Add(pos + (-right + up2) * size);
            vertices.Add(pos + ( right + up2) * size);
            vertices.Add(pos + ( right - up2) * size);

            colors.Add(col);
            colors.Add(col);
            colors.Add(col);
            colors.Add(col);

            // Two triangles making a quad
            indices.Add(idx);     indices.Add(idx + 1); indices.Add(idx + 2);
            indices.Add(idx);     indices.Add(idx + 2); indices.Add(idx + 3);
        }

        // Build the mesh
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // supports >65k vertices
        mesh.SetVertices(vertices);
        mesh.SetTriangles(indices, 0);
        mesh.SetColors(colors);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Create a single GameObject to hold the mesh
        GameObject starField = new GameObject("StarField");
        starField.transform.position = Vector3.zero;

        MeshFilter mf = starField.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = starField.AddComponent<MeshRenderer>();
        mr.material = CreateStarMaterial();
    }

    Material CreateStarMaterial()
    {
        // Unlit so stars aren't affected by scene lighting
        Material mat = new Material(Shader.Find("Unlit/VertexColor"));
        if (mat.shader.name != "Unlit/VertexColor")
        {
            // Fallback if VertexColor shader isn't available
            mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = Color.white;
        }
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