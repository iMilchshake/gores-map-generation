using System;

public class KernelGenerator
{
    public static bool[,] GetCircularKernel(int size, float circularity)
    {
        if (size % 2 == 0)
            throw new Exception("kernel size must be odd");

        var kernel = new bool[size, size];
        var center = size / 2;

        // calculate radius based on the size and circularity
        var minRadius = (float)(size - 1) / 2; // min radius is from center to border
        var maxRadius = Math.Sqrt(center * center + center * center); // max radius is from center to corner
        var radius = circularity * minRadius + (1 - circularity) * maxRadius;

        for (var x = 0; x < size; x++)
        {
            for (var y = 0; y < size; y++)
            {
                if (Math.Sqrt((x - center) * (x - center) + (y - center) * (y - center)) <= radius)
                {
                    kernel[x, y] = true;
                }
            }
        }

        return kernel;
    }

    public static bool[,] GetRectangleKernel(int size)
    {
        if (size % 2 == 0)
            throw new Exception("kernel size must be odd");

        var kernel = new bool[size, size];
        for (var x = 0; x < size; x++)
        {
            for (var y = 0; y < size; y++)
            {
                kernel[x, y] = true;
            }
        }

        return kernel;
    }
}