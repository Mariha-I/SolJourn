using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ConstellationLine
{
    public int starIdA;
    public int starIdB;
}

[System.Serializable]
public class ConstellationData
{
    public string abbreviation;
    public List<ConstellationLine> lines = new List<ConstellationLine>();
    public Vector3 centerPoint;   // average position of all stars
    public string fullName;       // we'll add this later
}