using System;

public class MathUtil
{
    public static float GeometricDistribution(int x, float p)
    {
        return (float)Math.Pow(1 - p, x - 1) * p;
    }
}