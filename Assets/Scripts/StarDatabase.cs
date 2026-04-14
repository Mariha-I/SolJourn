using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StarData
{
    public float ra;
    public float dec;
    public float magnitude;
    public string properName;
}

public class StarDatabase : MonoBehaviour
{
    public TextAsset csvFile;
    public List<StarData> stars = new List<StarData>();
    public float magnitudeCutoff = 6.5f;

    void Awake()
    {
        LoadStars();
    }

    void LoadStars()
    {
        // Strip ALL quotes from the entire file at once, then split
        string cleanText = csvFile.text.Replace("\"", "");
        string[] lines = cleanText.Split('\n');

        string[] headers = lines[0].Trim().Split(',');

        // Debug: print first 10 headers
        for (int h = 0; h < Mathf.Min(headers.Length, 10); h++)
        {
            Debug.Log($"Header[{h}] = '{headers[h]}'");
        }

        int raIdx   = -1;
        int decIdx  = -1;
        int magIdx  = -1;
        int nameIdx = -1;

        for (int h = 0; h < headers.Length; h++)
        {
            string clean = headers[h].Trim().ToLower();
            if (clean == "ra")     raIdx   = h;
            if (clean == "dec")    decIdx  = h;
            if (clean == "mag")    magIdx  = h;
            if (clean == "proper") nameIdx = h;
        }

        Debug.Log($"Column indices - RA:{raIdx} Dec:{decIdx} Mag:{magIdx} Name:{nameIdx}");

        if (raIdx == -1 || decIdx == -1 || magIdx == -1)
        {
            Debug.LogError("Could not find required columns in CSV!");
            return;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] cols = lines[i].Split(',');
            if (cols.Length <= magIdx) continue;

            float mag;
            if (!float.TryParse(cols[magIdx], out mag)) continue;
            if (mag > magnitudeCutoff) continue;

            StarData star = new StarData();
            float.TryParse(cols[raIdx],  out star.ra);
            float.TryParse(cols[decIdx], out star.dec);
            star.magnitude = mag;
            star.properName = nameIdx >= 0 ? cols[nameIdx].Trim() : "";

            stars.Add(star);
        }

        Debug.Log($"Loaded {stars.Count} stars");
    }
}