using System;

public class KernelGenerator
{
    public static bool[,] GetCircularKernel(int size)
    {
        throw new Exception("not implemented");
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