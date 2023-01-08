public class ArrayUtils
{
    public static void FillArray2D<T>(T[,] array, T value)
    {
        for (int x = 0; x < array.GetLength(0); x++)
        {
            for (int y = 0; y < array.GetLength(1); y++)
            {
                array[x, y] = value;
            }
        }
    }

    public static string Array2DToString(float[,] array)
    {
        var strOut = "";
        for (var y = 0; y < array.GetLength(1); y++)
        {
            for (var x = 0; x < array.GetLength(0) - 1; x++)
            {
                strOut += array[x, y].ToString("0.00") + ",";
            }

            strOut += array[array.GetLength(0) - 1, y].ToString("0.00") + "\n";
        }

        return strOut;
    }

    public static string Array2DToString(int[,] array)
    {
        var strOut = "";
        for (var y = 0; y < array.GetLength(1); y++)
        {
            for (var x = 0; x < array.GetLength(0) - 1; x++)
            {
                strOut += array[x, y] + ",";
            }

            strOut += array[array.GetLength(0) - 1, y] + "\n";
        }

        return strOut;
    }

    public static string Array2DToString(bool[,] array)
    {
        var strOut = "";
        for (var y = 0; y < array.GetLength(1); y++)
        {
            for (var x = 0; x < array.GetLength(0) - 1; x++)
            {
                strOut += array[x, y] ? "X" : "0" + ",";
            }

            strOut += array[array.GetLength(0) - 1, y] ? "X" : "0" + "\n";
        }

        return strOut;
    }
}