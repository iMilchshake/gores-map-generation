using System;

public class MathUtil
{
    public static float GeometricDistribution(int x, float p)
    {
        return (float)Math.Pow(1 - p, x - 1) * p;
    }

    public static int[,] DistanceTransformCityBlock(int[,] array)
    {
        var width = array.GetLength(0);
        var height = array.GetLength(1);

        var distance = (int[,])array.Clone();

        // forward pass
        for (int x = 0; x < width; x++) // left to right
        {
            for (int y = height - 1; y >= 0; y--) // top to bottom
            {
                var above = y < (height - 1) ? distance[x, y + 1] : int.MaxValue;
                var left = x > 0 ? distance[x - 1, y] : int.MaxValue;
                var best = Math.Min(above, left);
                var newValue = best < int.MaxValue ? best + 1 : int.MaxValue;
                if (newValue < distance[x, y])
                    distance[x, y] = newValue;
            }
        }

        // backwards pass
        for (int x = width - 1; x >= 0; x--) // right to left
        {
            for (int y = 0; y < height; y++) // bottom to top
            {
                var below = y > 0 ? distance[x, y - 1] : int.MaxValue;
                var right = x < width - 1 ? distance[x + 1, y] : int.MaxValue;
                var best = Math.Min(below, right);
                var newValue = best < int.MaxValue ? best + 1 : int.MaxValue;
                if (newValue < distance[x, y])
                    distance[x, y] = newValue;
            }
        }

        return distance;
    }
}