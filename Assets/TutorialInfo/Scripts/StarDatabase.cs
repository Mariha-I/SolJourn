using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StarData
{
    public float ra;       // Right Ascension in hours
    public float dec;      // Declination in degrees
    public float magnitude;
    public string properName;
}

public class StarDatabase : MonoBehaviour
{
    public TextAsset csvFile;  // drag hygdata_v3.csv here in Inspector
    public List<StarData> stars = new List<StarData>();
    public float magnitudeCutoff = 6.5f;

    void Awake()
    {
        LoadStars();
    }

    void LoadStars()
    {
        string[] lines = csvFile.text.Split('\n');
        
        // First line is header - find column indices
        string[] headers = lines[0].Split('\t');
        int raIdx   = System.Array.IndexOf(headers, "ra");
        int decIdx  = System.Array.IndexOf(headers, "dec");
        int magIdx  = System.Array.IndexOf(headers, "mag");
        int nameIdx = System.Array.IndexOf(headers, "proper");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            
            string[] cols = lines[i].Split('\t');
            
            float mag;
            if (!float.TryParse(cols[magIdx], out mag)) continue;
            if (mag > magnitudeCutoff) continue;  // skip dim stars
            
            StarData star = new StarData();
            star.ra        = float.Parse(cols[raIdx]);
            star.dec       = float.Parse(cols[decIdx]);
            star.magnitude = mag;
            star.properName = cols[nameIdx].Trim();
            
            stars.Add(star);
        }
        
        Debug.Log($"Loaded {stars.Count} stars");
    }
}