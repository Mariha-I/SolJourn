using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;

public class ConstellationManager : MonoBehaviour
{
    [Header("References")]
    public StarDatabase starDatabase;
    public StarRenderer starRenderer;
    public Camera mainCamera;

    [Header("Assets")]
    public TextAsset constellationFile;
    public TextAsset namesFile;

    [Header("Appearance")]
    public Color lineColor = new Color(0.4f, 0.6f, 1f, 0.8f);
    public float lineWidth = 0.3f;
    public float fadeAngle = 25f;
    public float fadeSpeed = 3f;

    private List<ConstellationData> constellations = new List<ConstellationData>();
    private Dictionary<string, LineRenderer[]> lineRenderers
        = new Dictionary<string, LineRenderer[]>();
    private Dictionary<string, TextMeshPro> labels
        = new Dictionary<string, TextMeshPro>();

    private Dictionary<string, string> fullNames = new Dictionary<string, string>()
    {
        {"And","Andromeda"},{"Ant","Antlia"},{"Aps","Apus"},{"Aqr","Aquarius"},
        {"Aql","Aquila"},{"Ara","Ara"},{"Ari","Aries"},{"Aur","Auriga"},
        {"Boo","Boötes"},{"Cae","Caelum"},{"Cam","Camelopardalis"},{"Cnc","Cancer"},
        {"CVn","Canes Venatici"},{"CMa","Canis Major"},{"CMi","Canis Minor"},
        {"Cap","Capricornus"},{"Car","Carina"},{"Cas","Cassiopeia"},{"Cen","Centaurus"},
        {"Cep","Cepheus"},{"Cet","Cetus"},{"Cha","Chamaeleon"},{"Cir","Circinus"},
        {"Col","Columba"},{"Com","Coma Berenices"},{"CrA","Corona Australis"},
        {"CrB","Corona Borealis"},{"Crv","Corvus"},{"Crt","Crater"},{"Cru","Crux"},
        {"Cyg","Cygnus"},{"Del","Delphinus"},{"Dor","Dorado"},{"Dra","Draco"},
        {"Equ","Equuleus"},{"Eri","Eridanus"},{"For","Fornax"},{"Gem","Gemini"},
        {"Gru","Grus"},{"Her","Hercules"},{"Hor","Horologium"},{"Hya","Hydra"},
        {"Hyi","Hydrus"},{"Ind","Indus"},{"Lac","Lacerta"},{"Leo","Leo"},
        {"LMi","Leo Minor"},{"Lep","Lepus"},{"Lib","Libra"},{"Lup","Lupus"},
        {"Lyn","Lynx"},{"Lyr","Lyra"},{"Men","Mensa"},{"Mic","Microscopium"},
        {"Mon","Monoceros"},{"Mus","Musca"},{"Nor","Norma"},{"Oct","Octans"},
        {"Oph","Ophiuchus"},{"Ori","Orion"},{"Pav","Pavo"},{"Peg","Pegasus"},
        {"Per","Perseus"},{"Phe","Phoenix"},{"Pic","Pictor"},{"Psc","Pisces"},
        {"PsA","Piscis Austrinus"},{"Pup","Puppis"},{"Pyx","Pyxis"},{"Ret","Reticulum"},
        {"Sge","Sagitta"},{"Sgr","Sagittarius"},{"Sco","Scorpius"},{"Scl","Sculptor"},
        {"Sct","Scutum"},{"Ser","Serpens"},{"Sex","Sextans"},{"Tau","Taurus"},
        {"Tel","Telescopium"},{"Tri","Triangulum"},{"TrA","Triangulum Australe"},
        {"Tuc","Tucana"},{"UMa","Ursa Major"},{"UMi","Ursa Minor"},{"Vel","Vela"},
        {"Vir","Virgo"},{"Vol","Volans"},{"Vul","Vulpecula"}
    };

    void Start()
    {
        LoadConstellationNames();
        ParseConstellations();
        BuildConstellationObjects();
    }

    void LoadConstellationNames()
    {
        if (namesFile == null) return;

        string[] lines = namesFile.text.Split('\n');
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;

            string[] parts = trimmed.Split(
                new char[]{ ' ', '\t' },
                StringSplitOptions.RemoveEmptyEntries);

            // First part is abbreviation, last part(s) are the name
            // Handle formats like "And Andromeda" or "And 00 47 Andromeda"
            if (parts.Length >= 2)
            {
                string abbrev = parts[0].Trim();
                string name   = parts[parts.Length - 1].Trim();

                // If the last part looks like a name (starts with capital letter)
                if (char.IsLetter(name[0]) && char.IsUpper(name[0]))
                    fullNames[abbrev] = name;
            }
        }

        Debug.Log($"Loaded {fullNames.Count} constellation names");
    }

    void ParseConstellations()
    {
        if (constellationFile == null)
        {
            Debug.LogError("No constellation file assigned!");
            return;
        }

        Dictionary<string, ConstellationData> conDict
            = new Dictionary<string, ConstellationData>();

        string[] lines = constellationFile.text.Split('\n');
        ConstellationData current = null;

        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            if (trimmed.StartsWith("#")) continue;

            if (trimmed.StartsWith("*"))
            {
                string name = trimmed.TrimStart('*').Trim();

                // Reverse lookup abbreviation from full name
                string abbrev = name;
                foreach (var kvp in fullNames)
                {
                    if (string.Equals(kvp.Value, name, 
                        StringComparison.OrdinalIgnoreCase))
                    {
                        abbrev = kvp.Key;
                        break;
                    }
                }

                current = new ConstellationData();
                current.abbreviation = abbrev;
                current.fullName     = name;
                conDict[abbrev]      = current;
                continue;
            }

            if (trimmed.StartsWith("[") && current != null)
            {
                string inner = trimmed
                    .Replace("[", "").Replace("]", "").Replace("\"", "").Trim();

                string[] idStrings = inner.Split(
                    new char[]{ ',' },
                    StringSplitOptions.RemoveEmptyEntries);

                List<int> chainIds = new List<int>();
                foreach (string idStr in idStrings)
                {
                    int hip;
                    if (int.TryParse(idStr.Trim(), out hip))
                        chainIds.Add(hip);
                }

                for (int i = 0; i < chainIds.Count - 1; i++)
                {
                    current.lines.Add(new ConstellationLine
                    {
                        starIdA = chainIds[i],
                        starIdB = chainIds[i + 1]
                    });
                }
            }
        }

        // Calculate center points and add to list
        foreach (var kvp in conDict)
        {
            ConstellationData con = kvp.Value;
            List<Vector3> positions = new List<Vector3>();

            foreach (ConstellationLine seg in con.lines)
            {
                if (starDatabase.starByHip.ContainsKey(seg.starIdA))
                    positions.Add(GetStarWorldPosByHip(seg.starIdA));
                if (starDatabase.starByHip.ContainsKey(seg.starIdB))
                    positions.Add(GetStarWorldPosByHip(seg.starIdB));
            }

            if (positions.Count > 0)
            {
                Vector3 sum = Vector3.zero;
                foreach (Vector3 p in positions) sum += p;
                con.centerPoint = (sum / positions.Count).normalized * skyRadius;
            }

            constellations.Add(con);
        }

        Debug.Log($"Parsed {constellations.Count} constellations");
    }

    void BuildConstellationObjects()
    {
        foreach (ConstellationData con in constellations)
        {
            GameObject conObj = new GameObject($"Con_{con.abbreviation}");
            conObj.transform.position = Vector3.zero;

            LineRenderer[] lrs = new LineRenderer[con.lines.Count];

            for (int i = 0; i < con.lines.Count; i++)
            {
                ConstellationLine seg = con.lines[i];

                if (!starDatabase.starByHip.ContainsKey(seg.starIdA) ||
                    !starDatabase.starByHip.ContainsKey(seg.starIdB))
                    continue;

                GameObject lineObj = new GameObject($"Line_{i}");
                lineObj.transform.SetParent(conObj.transform);

                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.SetPosition(0, GetStarWorldPosByHip(seg.starIdA));
                lr.SetPosition(1, GetStarWorldPosByHip(seg.starIdB));
                lr.startWidth    = lineWidth;
                lr.endWidth      = lineWidth;
                lr.material      = CreateLineMaterial();
                lr.startColor    = lineColor;
                lr.endColor      = lineColor;
                lr.useWorldSpace = true;

                lrs[i] = lr;
            }

            lineRenderers[con.abbreviation] = lrs;

            // Label
            GameObject labelObj = new GameObject($"Label_{con.abbreviation}");
            labelObj.transform.position = con.centerPoint;
            labelObj.transform.SetParent(conObj.transform);

            TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
            RectTransform rect = labelObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(800, 200);
            tmp.text      = con.fullName;
            tmp.fontSize  = 150;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = new Color(lineColor.r, lineColor.g, lineColor.b, 0f);

            labelObj.transform.LookAt(Vector3.zero);
            labelObj.transform.Rotate(0, 180, 0);
            labelObj.transform.position = con.centerPoint * 0.97f;

            labels[con.abbreviation] = tmp;

            SetConstellationAlpha(con.abbreviation, 0f);
        }
    }

    void Update()
    {
        if (mainCamera == null) return;

        foreach (ConstellationData con in constellations)
        {
            if (con == null) continue;
            float targetAlpha = GetTargetAlpha(con);
            FadeConstellation(con.abbreviation, targetAlpha);
        }
        foreach (var kvp in labels)
        {
            if (kvp.Value != null)
            {
                kvp.Value.transform.LookAt(mainCamera.transform.position);
                kvp.Value.transform.Rotate(0, 180, 0);
            }
        }
    }

    float GetTargetAlpha(ConstellationData con)
    {
        Vector3 dirToCenter = (con.centerPoint - mainCamera.transform.position).normalized;
        float angle = Vector3.Angle(mainCamera.transform.forward, dirToCenter);
        if (angle > fadeAngle) return 0f;
        return Mathf.InverseLerp(fadeAngle, fadeAngle * 0.3f, angle);
    }

    void FadeConstellation(string abbrev, float targetAlpha)
    {
        if (!lineRenderers.ContainsKey(abbrev)) return;

        foreach (LineRenderer lr in lineRenderers[abbrev])
        {
            if (lr == null) continue;
            Color c  = lr.startColor;
            c.a      = Mathf.MoveTowards(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
            lr.startColor = c;
            lr.endColor   = c;
        }

        if (labels.ContainsKey(abbrev))
        {
            Color lc = labels[abbrev].color;
            lc.a     = Mathf.MoveTowards(lc.a, targetAlpha, Time.deltaTime * fadeSpeed);
            labels[abbrev].color = lc;
        }
    }

    void SetConstellationAlpha(string abbrev, float alpha)
    {
        if (lineRenderers.ContainsKey(abbrev))
        {
            foreach (LineRenderer lr in lineRenderers[abbrev])
            {
                if (lr == null) continue;
                Color c = lineColor;
                c.a = alpha;
                lr.startColor = c;
                lr.endColor   = c;
            }
        }
        if (labels.ContainsKey(abbrev))
        {
            Color lc  = labels[abbrev].color;
            lc.a      = alpha;
            labels[abbrev].color = lc;
        }
    }

    Vector3 GetStarWorldPos(StarData star)
    {
        DateTime utcNow = DateTime.UtcNow;
        float lat = starRenderer.sensors != null && starRenderer.sensors.locationReady
            ? starRenderer.sensors.latitude  : starRenderer.latitude;
        float lon = starRenderer.sensors != null && starRenderer.sensors.locationReady
            ? starRenderer.sensors.longitude : starRenderer.longitude;

        var (alt, az) = AstroMath.RaDecToAltAz(
            star.ra, star.dec, lat, lon, utcNow);
        return AstroMath.AltAzToWorld(alt, az, skyRadius);
    }

    Vector3 GetStarWorldPosByHip(int hipId)
    {
        return GetStarWorldPos(starDatabase.starByHip[hipId]);
    }

    Material CreateLineMaterial()
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        return mat;
    }

    public List<ConstellationData> GetConstellationList()
    {
        return constellations;
    }

    public Vector3 GetConstellationCenter(string fullName)
    {
        foreach (ConstellationData con in constellations)
        {
            if (string.Equals(con.fullName, fullName, 
                StringComparison.OrdinalIgnoreCase))
                return con.centerPoint;
        }
        return Vector3.zero;
    }

    float skyRadius => starRenderer.skyRadius;
}