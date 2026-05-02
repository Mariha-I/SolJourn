using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class SearchUI : MonoBehaviour
{
    [Header("References")]
    public Camera           mainCamera;
    public StarRenderer     starRenderer;
    public ConstellationManager constellationManager;
    public CelestialBodyRenderer celestialRenderer;

    [Header("UI Colors")]
    public Color panelColor       = new Color(0.05f, 0.05f, 0.15f, 0.95f);
    public Color categoryColor    = new Color(0.2f,  0.3f,  0.6f,  1f);
    public Color itemColor        = new Color(0.1f,  0.1f,  0.2f,  1f);
    public Color accentColor      = new Color(0.4f,  0.6f,  1.0f,  1f);
    public Color planetLabelColor = new Color(1.0f,  0.8f,  0.3f,  1f);
    public Color exoplanetLabelColor = new Color(0.4f, 1.0f, 0.7f, 1f);
    public Color constellationLabelColor = new Color(0.7f, 0.5f, 1.0f, 1f);

    // UI elements
    private Canvas          canvas;
    private GameObject      searchPanel;
    private GameObject      infoPanel;
    private TMP_InputField  searchInput;
    private GameObject      searchButton;
    private ScrollRect      scrollRect;
    private Transform       listContent;
    private bool            isPanelOpen = false;

    // Camera animation
    private bool        isAnimating  = false;
    private Quaternion  targetRot;
    private float       animSpeed    = 2.0f;

    // Catalog
    private List<CelestialCatalog.CatalogEntry> allEntries 
        = new List<CelestialCatalog.CatalogEntry>();

    void Start()
    {
        BuildCatalog();
        BuildUI();
    }

    // ─── Catalog ──────────────────────────────────────────────────────────────

    void BuildCatalog()
    {
        // Solar system bodies
        string[] solarBodies = { 
            "Sun", "Mercury", "Venus", "Moon", "Mars", 
            "Jupiter", "Saturn", "Uranus", "Neptune" };

        foreach (string name in solarBodies)
        {
            allEntries.Add(new CelestialCatalog.CatalogEntry {
                name        = name,
                category    = name == "Moon" ? "Moon" : name == "Sun" ? "Star" : "Planet",
                parentName  = "Solar System",
                description = CelestialCatalog.PlanetDescriptions.ContainsKey(name)
                              ? CelestialCatalog.PlanetDescriptions[name] : "",
                isDynamic   = true
            });
        }

        // Moons
        var moonParents = new Dictionary<string, string>()
        {
            {"Phobos","Mars"},{"Deimos","Mars"},
            {"Io","Jupiter"},{"Europa","Jupiter"},
            {"Ganymede","Jupiter"},{"Callisto","Jupiter"},
            {"Titan","Saturn"},{"Enceladus","Saturn"},
            {"Triton","Neptune"},{"Charon","Pluto"}
        };

        foreach (var kvp in moonParents)
        {
            allEntries.Add(new CelestialCatalog.CatalogEntry {
                name        = kvp.Key,
                category    = "Moon",
                parentName  = kvp.Value,
                description = CelestialCatalog.MoonDescriptions.ContainsKey(kvp.Key)
                              ? CelestialCatalog.MoonDescriptions[kvp.Key] : "",
                isDynamic   = true
            });
        }

        // Exoplanets
        var exoCoords = new Dictionary<string, (float ra, float dec)>()
        {
            {"Proxima Centauri b", (14.495f, -62.676f)},
            {"51 Pegasi b",        (22.957f,  20.769f)},
            {"HD 209458 b",        (22.030f,  18.884f)},
            {"Kepler-22b",         (19.299f,  47.887f)},
            {"TRAPPIST-1b",        ( 1.512f,  -5.041f)},
            {"55 Cancri e",        ( 8.905f,  28.330f)},
            {"Kepler-442b",        (19.013f,  47.458f)},
            {"HD 40307 g",         ( 5.954f, -60.012f)},
            {"Tau Ceti e",         ( 1.734f, -15.937f)},
            {"GJ 667C c",          (17.629f, -34.993f)}
        };

        foreach (var kvp in exoCoords)
        {
            allEntries.Add(new CelestialCatalog.CatalogEntry {
                name        = kvp.Key,
                category    = "Exoplanet",
                parentName  = "Exoplanets",
                description = CelestialCatalog.ExoplanetDescriptions.ContainsKey(kvp.Key)
                              ? CelestialCatalog.ExoplanetDescriptions[kvp.Key] : "",
                ra          = kvp.Value.ra,
                dec         = kvp.Value.dec,
                isDynamic   = false
            });
        }

        // Constellations - grab from ConstellationManager at runtime
        // We add these after Start via a coroutine
        StartCoroutine(AddConstellationEntries());
    }

    IEnumerator AddConstellationEntries()
    {
        yield return new WaitForSeconds(0.5f); // wait for ConstellationManager to init

        if (constellationManager == null) yield break;

        foreach (var con in constellationManager.GetConstellationList())
        {
            allEntries.Add(new CelestialCatalog.CatalogEntry {
                name       = con.fullName,
                category   = "Constellation",
                parentName = "Constellations",
                description = "",
                ra          = 0, dec = 0,
                isDynamic   = false
            });
        }
    }

    // ─── UI Construction ──────────────────────────────────────────────────────

    void BuildUI()
    {
        // Root canvas
        GameObject canvasObj = new GameObject("SearchCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode 
            = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        // ── Search Button ──────────────────────────────────────────────────────
        searchButton = CreatePanel(canvasObj.transform, 
            new Vector2(70, 70), new Vector2(0, 0),
            new Vector2(60, -60), panelColor);

        
        // Anchor to top-left
        RectTransform btnRect = searchButton.GetComponent<RectTransform>();
        btnRect.anchorMin = btnRect.anchorMax = new Vector2(0, 1);
        btnRect.pivot     = new Vector2(0, 1);
        btnRect.anchoredPosition = new Vector2(20, -20);

        // Search icon text
        TextMeshProUGUI btnIcon = CreateTMPText(
            searchButton.transform, "🔍", 30, TextAlignmentOptions.Center);
        StretchRect(btnIcon.GetComponent<RectTransform>());

        // Button component
        Button btn = searchButton.AddComponent<Button>();
        btn.targetGraphic = searchButton.GetComponent<Image>();
        btn.onClick.AddListener(ToggleSearchPanel);

        // ── Search Panel ───────────────────────────────────────────────────────
        searchPanel = CreatePanel(canvasObj.transform,
            new Vector2(380, 600), Vector2.zero, Vector2.zero, panelColor);

        RectTransform panelRect = searchPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot     = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(20, -100);

        searchPanel.SetActive(false);

        // Search input
        GameObject inputObj = new GameObject("SearchInput");
        inputObj.transform.SetParent(searchPanel.transform);
        Image inputBg = inputObj.AddComponent<Image>();
        inputBg.color = new Color(0.15f, 0.15f, 0.3f, 1f);

        RectTransform inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0, 1);
        inputRect.anchorMax = new Vector2(1, 1);
        inputRect.pivot     = new Vector2(0.5f, 1);
        inputRect.offsetMin = new Vector2(10, 0);
        inputRect.offsetMax = new Vector2(-10, 0);
        inputRect.sizeDelta = new Vector2(0, 45);
        inputRect.anchoredPosition = new Vector2(0, -10);

        searchInput = inputObj.AddComponent<TMP_InputField>();

        // Placeholder
        GameObject placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(inputObj.transform);
        TextMeshProUGUI placeholderText = placeholder.AddComponent<TextMeshProUGUI>();
        placeholderText.text      = "Search stars, planets, constellations...";
        placeholderText.fontSize  = 14;
        placeholderText.color     = new Color(0.5f, 0.5f, 0.7f, 1f);
        placeholderText.fontStyle = FontStyles.Italic;
        StretchRect(placeholderText.GetComponent<RectTransform>(), 8);

        // Input text
        GameObject inputTextObj = new GameObject("Text");
        inputTextObj.transform.SetParent(inputObj.transform);
        TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
        inputText.fontSize = 14;
        inputText.color    = Color.white;
        StretchRect(inputText.GetComponent<RectTransform>(), 8);

        searchInput.textViewport    = inputRect;
        searchInput.textComponent   = inputText;
        searchInput.placeholder     = placeholderText;
        searchInput.onValueChanged.AddListener(OnSearchChanged);

        // Scroll view
        GameObject scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(searchPanel.transform);
        Image scrollBg = scrollObj.AddComponent<Image>();
        scrollBg.color = Color.clear;

        RectTransform scrollRect2 = scrollObj.GetComponent<RectTransform>();
        scrollRect2.anchorMin = Vector2.zero;
        scrollRect2.anchorMax = Vector2.one;
        scrollRect2.offsetMin = new Vector2(10, 10);
        scrollRect2.offsetMax = new Vector2(-10, -65);

        scrollRect = scrollObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform);
        viewport.AddComponent<Image>().color = Color.clear;
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        RectTransform vpRect = viewport.GetComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.offsetMin = vpRect.offsetMax = Vector2.zero;

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform);
        listContent = content.transform;

        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot     = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing            = 4;
        vlg.padding            = new RectOffset(4, 4, 4, 4);
        vlg.childControlWidth  = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;

        content.AddComponent<ContentSizeFitter>().verticalFit 
            = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = vpRect;
        scrollRect.content  = contentRect;

        // ── Info Panel ─────────────────────────────────────────────────────────
        infoPanel = CreatePanel(canvasObj.transform,
            new Vector2(320, 250), Vector2.zero, Vector2.zero, panelColor);

        RectTransform infoRect = infoPanel.GetComponent<RectTransform>();
        infoRect.anchorMin = infoRect.anchorMax = new Vector2(0.5f, 0);
        infoRect.pivot     = new Vector2(0.5f, 0);
        infoRect.anchoredPosition = new Vector2(0, 20);
        infoPanel.SetActive(false);

        CanvasGroup btnGroup = searchButton.AddComponent<CanvasGroup>();
        btnGroup.alpha = 0f;
        btnGroup.blocksRaycasts = false;
        StartCoroutine(FadeInButton(btnGroup, 6.0f, 1.5f));

        PopulateList("");
    }

    private IEnumerator ShowButtonAfterDelay(float delay)
    {
        // Wait for the specified time
        yield return new WaitForSeconds(delay);
        
        // Check if the button exists, then enable it
        if (searchButton != null)
        {
            searchButton.SetActive(true);
        }
    }

    private IEnumerator FadeInButton(CanvasGroup group, float delay, float fadeDuration)
    {
        // 1. Wait for the initial delay
        yield return new WaitForSeconds(delay);

        // 2. Gradually increase alpha
        float currentTime = 0f;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, currentTime / fadeDuration);
            yield return null; // Wait for the next frame
        }

        // 3. Ensure it's exactly 1 at the end
        group.alpha = 1f;
        group.blocksRaycasts = true;
    }

    // ─── List Population ──────────────────────────────────────────────────────

    void PopulateList(string filter)
    {
        foreach (Transform child in listContent)
            Destroy(child.gameObject);

        bool showAll = string.IsNullOrEmpty(filter);

        if (showAll)
        {
            // Grouped view
            BuildCategory("☀️  Solar System", GetEntriesByParent("Solar System"), filter);
            BuildCategory("🌙  Moons", GetEntriesByCategory("Moon"), filter);
            BuildCategory("🌌  Exoplanets", GetEntriesByCategory("Exoplanet"), filter);
            BuildCategory("⭐  Constellations", GetEntriesByCategory("Constellation"), filter);
        }
        else
        {
            // Flat filtered view
            foreach (var entry in allEntries)
            {
                if (entry.name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    BuildItem(entry);
            }
        }
        // Force layout rebuild
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(
        listContent.GetComponent<RectTransform>());
    }

    List<CelestialCatalog.CatalogEntry> GetEntriesByParent(string parent)
    {
        return allEntries.FindAll(e => e.parentName == parent);
    }

    List<CelestialCatalog.CatalogEntry> GetEntriesByCategory(string cat)
    {
        return allEntries.FindAll(e => e.category == cat);
    }

    void BuildCategory(string title, 
        List<CelestialCatalog.CatalogEntry> entries, string filter)
    {
        if (entries.Count == 0) return;

        // Category header
        GameObject header = new GameObject("Header_" + title);
        header.transform.SetParent(listContent);

        Image headerBg = header.AddComponent<Image>();
        headerBg.color = categoryColor;

        LayoutElement le = header.AddComponent<LayoutElement>();
        le.preferredHeight = 36;

        TextMeshProUGUI headerText = CreateTMPText(
            header.transform, title, 15, TextAlignmentOptions.MidlineLeft);
        RectTransform ht = headerText.GetComponent<RectTransform>();
        StretchRect(ht, 10);

        foreach (var entry in entries)
            BuildItem(entry);
    }

    void BuildItem(CelestialCatalog.CatalogEntry entry)
    {
        GameObject item = new GameObject("Item_" + entry.name);
        item.transform.SetParent(listContent);

        Image itemBg = item.AddComponent<Image>();
        itemBg.color = itemColor;

        Button btn = item.AddComponent<Button>();

        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.2f, 0.25f, 0.45f, 1f);
        cb.pressedColor     = accentColor;
        btn.colors = cb;
        btn.targetGraphic = itemBg;

        LayoutElement le = item.AddComponent<LayoutElement>();
        le.preferredHeight = 44;

        // Item name
        Color nameCol = GetEntryColor(entry);
        TextMeshProUGUI nameText = CreateTMPText(
            item.transform, entry.name, 14, TextAlignmentOptions.MidlineLeft);
        nameText.color = nameCol;
        RectTransform nt = nameText.GetComponent<RectTransform>();
        nt.anchorMin = new Vector2(0, 0);
        nt.anchorMax = new Vector2(0.7f, 1);
        nt.offsetMin = new Vector2(12, 0);
        nt.offsetMax = Vector2.zero;

        // Category badge
        TextMeshProUGUI catText = CreateTMPText(
            item.transform, entry.category, 10, TextAlignmentOptions.MidlineRight);
        catText.color = new Color(0.5f, 0.6f, 0.8f, 1f);
        RectTransform ct = catText.GetComponent<RectTransform>();
        ct.anchorMin = new Vector2(0.7f, 0);
        ct.anchorMax = new Vector2(1f, 1f);
        ct.offsetMin = Vector2.zero;
        ct.offsetMax = new Vector2(-8, 0);

        var capturedEntry = entry;
        btn.onClick.AddListener(() => OnItemSelected(capturedEntry));
    }

    Color GetEntryColor(CelestialCatalog.CatalogEntry entry)
    {
        switch (entry.category)
        {
            case "Planet": case "Star": return planetLabelColor;
            case "Exoplanet":          return exoplanetLabelColor;
            case "Constellation":      return constellationLabelColor;
            default:                   return Color.white;
        }
    }

    // ─── Selection & Camera ───────────────────────────────────────────────────

    void OnItemSelected(CelestialCatalog.CatalogEntry entry)
    {
        searchPanel.SetActive(false);
        isPanelOpen = false;

        Vector3 worldPos = GetEntryWorldPos(entry);
        if (worldPos == Vector3.zero) return;

        // Point camera at target
        Vector3 dir = worldPos.normalized;
        targetRot   = Quaternion.LookRotation(dir);
        isAnimating = true;

        StartCoroutine(AnimateCameraAndShowInfo(entry, worldPos));
    }

    IEnumerator AnimateCameraAndShowInfo(
        CelestialCatalog.CatalogEntry entry, Vector3 worldPos)
    {
        // Animate camera
        float t = 0f;
        Quaternion startRot = mainCamera.transform.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime * animSpeed;
            mainCamera.transform.rotation = Quaternion.Slerp(
                startRot, targetRot, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        mainCamera.transform.rotation = targetRot;
        isAnimating = false;

        ShowInfoPanel(entry, worldPos);
    }

    void ShowInfoPanel(CelestialCatalog.CatalogEntry entry, Vector3 worldPos)
    {
        infoPanel.SetActive(true);

        // Clear old content
        foreach (Transform child in infoPanel.transform)
            Destroy(child.gameObject);

        float lat = GetLat();
        float lon = GetLon();
        DateTime utcNow = DateTime.UtcNow;

        // Get current Alt/Az
        float ra  = entry.isDynamic ? GetDynamicRa(entry, utcNow)  : entry.ra;
        float dec = entry.isDynamic ? GetDynamicDec(entry, utcNow) : entry.dec;

        var (alt, az) = AstroMath.RaDecToAltAz(ra, dec, lat, lon, utcNow);

        // Camera orientation
        Vector3 euler = mainCamera.transform.eulerAngles;

        // Build info text
        string aboveBelow = alt >= 0 ? "above" : "below";
        string infoText =
            $"<color=#AACCFF><b>{entry.name}</b></color>\n" +
            $"<size=85%><color=#8899BB>{entry.category}" +
            (string.IsNullOrEmpty(entry.parentName) ? "" : $"  ·  {entry.parentName}") +
            $"</color></size>\n\n" +
            $"<b>Position</b>\n" +
            $"Altitude:  {alt:F1}°  ({aboveBelow} horizon)\n" +
            $"Azimuth:  {az:F1}°\n\n" +
            $"<b>Camera Orientation</b>\n" +
            $"Heading:  {euler.y:F1}°\n" +
            $"Pitch:      {euler.x:F1}°\n" +
            $"GPS:        {lat:F4}°, {lon:F4}°\n\n";

        if (!string.IsNullOrEmpty(entry.description))
            infoText += $"<size=85%><color=#AAAACC>{entry.description}</color></size>";

        // Info text label
        GameObject textObj = new GameObject("InfoText");
        textObj.transform.SetParent(infoPanel.transform);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text           = infoText;
        tmp.fontSize       = 13;
        tmp.color          = Color.white;
        tmp.enableWordWrapping = true;

        RectTransform tr = tmp.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(12, 40);
        tr.offsetMax = new Vector2(-12, -12);

        // Resize info panel to fit content
        RectTransform infoRect = infoPanel.GetComponent<RectTransform>();
        infoRect.sizeDelta = new Vector2(320, 
            string.IsNullOrEmpty(entry.description) ? 200 : 320);

        // Close button
        GameObject closeBtn = CreatePanel(infoPanel.transform,
            new Vector2(30, 30), Vector2.zero, Vector2.zero, 
            new Color(0.3f, 0.1f, 0.1f, 1f));

        RectTransform cbRect = closeBtn.GetComponent<RectTransform>();
        cbRect.anchorMin = cbRect.anchorMax = new Vector2(1, 1);
        cbRect.pivot     = new Vector2(1, 1);
        cbRect.anchoredPosition = new Vector2(-4, -4);

        TextMeshProUGUI closeIcon = CreateTMPText(
            closeBtn.transform, "✕", 14, TextAlignmentOptions.Center);
        StretchRect(closeIcon.GetComponent<RectTransform>());

        Button closeBtnComp = closeBtn.AddComponent<Button>();
        closeBtnComp.targetGraphic = closeBtn.GetComponent<Image>();
        closeBtnComp.onClick.AddListener(() => infoPanel.SetActive(false));
    }

    // ─── Position Helpers ─────────────────────────────────────────────────────

    Vector3 GetEntryWorldPos(CelestialCatalog.CatalogEntry entry)
    {
        float lat = GetLat();
        float lon = GetLon();
        DateTime utcNow = DateTime.UtcNow;

        float ra, dec;

        if (entry.isDynamic)
        {
            ra  = GetDynamicRa(entry, utcNow);
            dec = GetDynamicDec(entry, utcNow);
        }
        else
        {
            ra  = entry.ra;
            dec = entry.dec;
        }

        if (entry.category == "Constellation")
        {
            Vector3 center = constellationManager.GetConstellationCenter(entry.name);
            if (center != Vector3.zero) return center;
        }

        var (alt, az) = AstroMath.RaDecToAltAz(ra, dec, lat, lon, utcNow);
        return AstroMath.AltAzToWorld(alt, az, starRenderer.skyRadius);
    }

    float GetDynamicRa(CelestialCatalog.CatalogEntry entry, DateTime utc)
    {
        switch (entry.name)
        {
            case "Sun":  return OrbitalMechanics.SunPosition(utc).ra;
            case "Moon": return OrbitalMechanics.MoonPosition(utc).ra;
            default:
                if (Enum.TryParse(entry.name, out Planet p))
                    return OrbitalMechanics.PlanetPosition(p, utc).ra;
                return 0;
        }
    }

    float GetDynamicDec(CelestialCatalog.CatalogEntry entry, DateTime utc)
    {
        switch (entry.name)
        {
            case "Sun":  return OrbitalMechanics.SunPosition(utc).dec;
            case "Moon": return OrbitalMechanics.MoonPosition(utc).dec;
            default:
                if (Enum.TryParse(entry.name, out Planet p))
                    return OrbitalMechanics.PlanetPosition(p, utc).dec;
                return 0;
        }
    }

    // ─── Event Handlers ───────────────────────────────────────────────────────

    void OnSearchChanged(string value) => PopulateList(value);

    void ToggleSearchPanel()
    {
        isPanelOpen = !isPanelOpen;
        searchPanel.SetActive(isPanelOpen);
        if (isPanelOpen) PopulateList("");
    }

    // ─── UI Helpers ───────────────────────────────────────────────────────────

    GameObject CreatePanel(Transform parent, Vector2 size, 
        Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject obj = new GameObject("Panel");
        obj.transform.SetParent(parent);

        Image img = obj.AddComponent<Image>();
        img.color = color;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin  = anchorMin;
        rt.anchorMax  = anchorMax;
        rt.sizeDelta  = size;
        rt.localScale = Vector3.one;
        return obj;
    }

    TextMeshProUGUI CreateTMPText(Transform parent, string text, 
        float fontSize, TextAlignmentOptions align)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent);

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.alignment = align;
        tmp.color     = Color.white;

        obj.GetComponent<RectTransform>().localScale = Vector3.one;
        return tmp;
    }

    void StretchRect(RectTransform rt, float padding = 0)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padding, padding);
        rt.offsetMax = new Vector2(-padding, -padding);
        rt.localScale = Vector3.one;
    }

    float GetLat() => starRenderer.sensors != null && starRenderer.sensors.locationReady
        ? starRenderer.sensors.latitude  : starRenderer.latitude;
    float GetLon() => starRenderer.sensors != null && starRenderer.sensors.locationReady
        ? starRenderer.sensors.longitude : starRenderer.longitude;
}