using UnityEngine;
using System;

public static class OrbitalMechanics
{
    // Calculate Sun's RA/Dec for a given UTC time
    public static (float ra, float dec) SunPosition(DateTime utc)
    {
        double jd = AstroMath.ToJulianDate(utc);
        double n  = jd - 2451545.0;

        // Mean longitude and anomaly
        double L = (280.460 + 0.9856474 * n) % 360.0;
        double g = (357.528 + 0.9856003 * n) % 360.0;
        if (L < 0) L += 360;
        if (g < 0) g += 360;

        double gRad = g * Math.PI / 180.0;

        // Ecliptic longitude
        double lambda = L + 1.915 * Math.Sin(gRad) + 0.020 * Math.Sin(2 * gRad);
        double lambdaRad = lambda * Math.PI / 180.0;

        // Obliquity of ecliptic
        double epsilon    = 23.439 - 0.0000004 * n;
        double epsilonRad = epsilon * Math.PI / 180.0;

        // RA and Dec
        double raRad  = Math.Atan2(Math.Cos(epsilonRad) * Math.Sin(lambdaRad), 
                                    Math.Cos(lambdaRad));
        double decRad = Math.Asin(Math.Sin(epsilonRad) * Math.Sin(lambdaRad));

        double ra  = raRad  * 180.0 / Math.PI / 15.0; // convert to hours
        double dec = decRad * 180.0 / Math.PI;

        if (ra < 0) ra += 24;

        return ((float)ra, (float)dec);
    }

    // Calculate Moon's RA/Dec
    public static (float ra, float dec) MoonPosition(DateTime utc)
    {
        double jd = AstroMath.ToJulianDate(utc);
        double n  = jd - 2451545.0;

        // Simplified lunar theory
        double L = (218.316 + 13.176396 * n) % 360.0;
        double M = (134.963 + 13.064993 * n) % 360.0;
        double F = (93.272  + 13.229350 * n) % 360.0;

        if (L < 0) L += 360;
        if (M < 0) M += 360;
        if (F < 0) F += 360;

        double MRad = M * Math.PI / 180.0;
        double FRad = F * Math.PI / 180.0;

        double lambda = L + 6.289 * Math.Sin(MRad);
        double beta   = 5.128 * Math.Sin(FRad);

        double lambdaRad = lambda * Math.PI / 180.0;
        double betaRad   = beta   * Math.PI / 180.0;

        double epsilon    = 23.439;
        double epsilonRad = epsilon * Math.PI / 180.0;

        double raRad  = Math.Atan2(
            Math.Sin(lambdaRad) * Math.Cos(epsilonRad) - 
            Math.Tan(betaRad)   * Math.Sin(epsilonRad),
            Math.Cos(lambdaRad));
        double decRad = Math.Asin(
            Math.Sin(betaRad)   * Math.Cos(epsilonRad) +
            Math.Cos(betaRad)   * Math.Sin(epsilonRad) * Math.Sin(lambdaRad));

        double ra  = raRad  * 180.0 / Math.PI / 15.0;
        double dec = decRad * 180.0 / Math.PI;

        if (ra < 0) ra += 24;

        return ((float)ra, (float)dec);
    }

    // Calculate planet position using simplified orbital elements
    // Returns (ra in hours, dec in degrees)
    public static (float ra, float dec) PlanetPosition(Planet planet, DateTime utc)
    {
        double jd = AstroMath.ToJulianDate(utc);
        double T  = (jd - 2451545.0) / 36525.0; // Julian centuries from J2000

        double L, a, e, i, omega, w;

        switch (planet)
        {
            case Planet.Mercury:
                L = 252.250906 + 149472.6746358 * T;
                a = 0.387098310;
                e = 0.20563175  + 0.000020407  * T;
                i = 7.004986    - 0.0059516    * T;
                omega = 48.330893  - 0.1254229   * T;
                w     = 77.456119  + 0.1588643   * T;
                break;
            case Planet.Venus:
                L = 181.979801 + 58517.8156760 * T;
                a = 0.723329820;
                e = 0.00677192  - 0.000047765  * T;
                i = 3.394662    - 0.0008568    * T;
                omega = 76.679920  - 0.2780080   * T;
                w     = 131.563703 + 0.0048746   * T;
                break;
            case Planet.Mars:
                L = 355.433275 + 19140.2993313 * T;
                a = 1.523679342;
                e = 0.09340065  + 0.000090484  * T;
                i = 1.849726    - 0.0081477    * T;
                omega = 49.558093  - 0.2949846   * T;
                w     = 336.060234 + 0.4439016   * T;
                break;
            case Planet.Jupiter:
                L = 34.351484  + 3034.9056746 * T;
                a = 5.202603209;
                e = 0.04849793  + 0.000163225  * T;
                i = 1.303270    - 0.0019872    * T;
                omega = 100.464407 + 0.1767232   * T;
                w     = 14.331419  + 0.2155620   * T;
                break;
            case Planet.Saturn:
                L = 50.077444  + 1222.1137943 * T;
                a = 9.554909192;
                e = 0.05550825  - 0.000346641  * T;
                i = 2.488879    + 0.0025514    * T;
                omega = 113.665503 - 0.2566722   * T;
                w     = 93.057237  + 0.5665415   * T;
                break;
            case Planet.Uranus:
                L = 314.055005 + 428.4669983  * T;
                a = 19.218446062;
                e = 0.04629590  - 0.000027337  * T;
                i = 0.773197    - 0.0016869    * T;
                omega = 74.005957  + 0.0741431   * T;
                w     = 173.005291 + 0.0893212   * T;
                break;
            case Planet.Neptune:
                L = 304.348665 + 218.4862002  * T;
                a = 30.110386869;
                e = 0.00898809  + 0.000006408  * T;
                i = 1.769953    - 0.0093082    * T;
                omega = 131.784057 - 0.0061648   * T;
                w     = 48.123691  + 0.0291587   * T;
                break;
            default:
                return (0, 0);
        }

        // Normalize angles
        L = NormalizeAngle(L);
        omega = NormalizeAngle(omega);
        w     = NormalizeAngle(w);

        // Mean anomaly
        double M = NormalizeAngle(L - w);
        double MRad = M * Math.PI / 180.0;

        // Eccentric anomaly (iterative solve)
        double E = MRad;
        for (int iter = 0; iter < 10; iter++)
            E = MRad + e * Math.Sin(E);

        // True anomaly
        double v = 2.0 * Math.Atan2(
            Math.Sqrt(1 + e) * Math.Sin(E / 2),
            Math.Sqrt(1 - e) * Math.Cos(E / 2));

        // Heliocentric distance
        double r = a * (1 - e * Math.Cos(E));

        // Heliocentric coordinates in orbital plane
        double xOrbit = r * Math.Cos(v);
        double yOrbit = r * Math.Sin(v);

        // Convert to ecliptic coordinates
        double omegaRad = omega * Math.PI / 180.0;
        double wRad     = w     * Math.PI / 180.0;
        double iRad     = i     * Math.PI / 180.0;

        double argPeri = wRad - omegaRad;

        double xEcl = (Math.Cos(omegaRad) * Math.Cos(argPeri) - 
                       Math.Sin(omegaRad) * Math.Sin(argPeri) * Math.Cos(iRad)) * xOrbit +
                      (-Math.Cos(omegaRad) * Math.Sin(argPeri) - 
                        Math.Sin(omegaRad) * Math.Cos(argPeri) * Math.Cos(iRad)) * yOrbit;

        double yEcl = (Math.Sin(omegaRad) * Math.Cos(argPeri) + 
                       Math.Cos(omegaRad) * Math.Sin(argPeri) * Math.Cos(iRad)) * xOrbit +
                      (-Math.Sin(omegaRad) * Math.Sin(argPeri) + 
                        Math.Cos(omegaRad) * Math.Cos(argPeri) * Math.Cos(iRad)) * yOrbit;

        double zEcl = (Math.Sin(argPeri) * Math.Sin(iRad)) * xOrbit +
                      (Math.Cos(argPeri) * Math.Sin(iRad)) * yOrbit;

        // Convert to equatorial coordinates (subtract Earth's position for geocentric)
        var (sunRa, sunDec) = SunPosition(utc);
        double sunRaRad  = sunRa  * 15.0 * Math.PI / 180.0;
        double sunDecRad = sunDec * Math.PI / 180.0;
        double sunDist   = 1.0; // approximate 1 AU

        double xEarth = sunDist * Math.Cos(sunDecRad) * Math.Cos(sunRaRad);
        double yEarth = sunDist * Math.Cos(sunDecRad) * Math.Sin(sunRaRad);
        double zEarth = sunDist * Math.Sin(sunDecRad);

        double xGeo = xEcl - xEarth;
        double yGeo = yEcl - yEarth;
        double zGeo = zEcl - zEarth;

        // Rotate from ecliptic to equatorial
        double epsilon    = 23.439 - 0.0000004 * (jd - 2451545.0);
        double epsilonRad = epsilon * Math.PI / 180.0;

        double xEq = xGeo;
        double yEq = yGeo * Math.Cos(epsilonRad) - zGeo * Math.Sin(epsilonRad);
        double zEq = yGeo * Math.Sin(epsilonRad) + zGeo * Math.Cos(epsilonRad);

        double ra  = Math.Atan2(yEq, xEq) * 180.0 / Math.PI / 15.0;
        double dec = Math.Atan2(zEq, Math.Sqrt(xEq * xEq + yEq * yEq)) * 180.0 / Math.PI;

        if (ra < 0) ra += 24;

        return ((float)ra, (float)dec);
    }

    static double NormalizeAngle(double angle)
    {
        angle = angle % 360.0;
        if (angle < 0) angle += 360.0;
        return angle;
    }
}

public enum Planet
{
    Mercury, Venus, Mars, Jupiter, Saturn, Uranus, Neptune
}