using UnityEngine;
using System;

public static class AstroMath
{
    // Convert RA/Dec to Altitude/Azimuth
    // ra in hours, dec in degrees, lat/lon in degrees, returns (alt, az) in degrees
    public static (float alt, float az) RaDecToAltAz(
        float ra, float dec, float latitude, float longitude, DateTime utcTime)
    {
        // Step 1: Calculate Julian Date
        double jd = ToJulianDate(utcTime);
        
        // Step 2: Calculate Local Sidereal Time
        double lst = LocalSiderealTime(jd, longitude);
        
        // Step 3: Hour Angle
        double ha = lst - ra;  // in hours
        // Normalize to -12 to +12
        while (ha < -12) ha += 24;
        while (ha >  12) ha -= 24;
        
        // Convert everything to radians for trig
        double haRad  = ha  * 15.0 * Math.PI / 180.0; // hours → degrees → radians
        double decRad = dec * Math.PI / 180.0;
        double latRad = latitude * Math.PI / 180.0;
        
        // Step 4: Alt/Az formula
        double sinAlt = Math.Sin(decRad) * Math.Sin(latRad)
                      + Math.Cos(decRad) * Math.Cos(latRad) * Math.Cos(haRad);
        double alt = Math.Asin(sinAlt) * 180.0 / Math.PI;
        
        double cosAz = (Math.Sin(decRad) - Math.Sin(latRad) * sinAlt)
                     / (Math.Cos(latRad) * Math.Cos(Math.Asin(sinAlt)));
        cosAz = Math.Clamp(cosAz, -1.0, 1.0);
        double az = Math.Acos(cosAz) * 180.0 / Math.PI;
        
        // Correct azimuth quadrant
        if (Math.Sin(haRad) > 0) az = 360.0 - az;
        
        return ((float)alt, (float)az);
    }

    // Convert Alt/Az to Unity XYZ on a sphere of given radius
    public static Vector3 AltAzToWorld(float alt, float az, float radius)
    {
        float altRad = alt * Mathf.Deg2Rad;
        float azRad  = az  * Mathf.Deg2Rad;
        
        float x = radius * Mathf.Cos(altRad) * Mathf.Sin(azRad);
        float y = radius * Mathf.Sin(altRad);
        float z = radius * Mathf.Cos(altRad) * Mathf.Cos(azRad);
        
        return new Vector3(x, y, z);
    }

    static double LocalSiderealTime(double jd, float longitude)
    {
        double t = (jd - 2451545.0) / 36525.0;  // Julian centuries from J2000
        
        // Greenwich Mean Sidereal Time in degrees
        double gmst = 280.46061837
                    + 360.98564736629 * (jd - 2451545.0)
                    + 0.000387933 * t * t;
        
        // Normalize to 0-360
        gmst = gmst % 360.0;
        if (gmst < 0) gmst += 360.0;
        
        // Local Sidereal Time in hours
        double lst = (gmst + longitude) / 15.0;
        lst = lst % 24.0;
        if (lst < 0) lst += 24.0;
        
        return lst;
    }

    static double ToJulianDate(DateTime utc)
    {
        int y = utc.Year;
        int m = utc.Month;
        int d = utc.Day;
        double h = utc.Hour + utc.Minute / 60.0 + utc.Second / 3600.0;
        
        if (m <= 2) { y--; m += 12; }
        
        int a = y / 100;
        int b = 2 - a + a / 4;
        
        return Math.Floor(365.25 * (y + 4716))
             + Math.Floor(30.6001 * (m + 1))
             + d + h / 24.0 + b - 1524.5;
    }
}