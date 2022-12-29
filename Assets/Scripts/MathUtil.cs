using System;

public class MathUtil
{
    public static double GeometricDistribution(int x, float p)
    {
        return Math.Pow(1 - p, x - 1) * p;
    }
}