using UnityEngine;

public enum CelestialBodyType
{
    Planet,
    Sun,
    Moon,
    Exoplanet
}

[System.Serializable]
public class CelestialBodyData
{
    public string name;
    public CelestialBodyType bodyType;
    public Color labelColor;
    public float displaySize;
    
    // For planets/sun/moon - calculated each frame
    public float ra;
    public float dec;
    
    // For exoplanets - fixed coordinates
    public float fixedRa;
    public float fixedDec;
    public string hostStar;      // name of the host star
    public int planetCount;      // how many planets in the system
}