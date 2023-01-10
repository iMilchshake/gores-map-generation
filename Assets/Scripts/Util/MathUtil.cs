using System;

namespace Util
{
    public abstract class MathUtil
    {
        public static float GeometricDistribution(int x, float p)
        {
            return (float)Math.Pow(1 - p, x - 1) * p;
        }

        public static bool CheckFloat(float a, float b)
        {
            return Math.Abs(a - b) < 1e-5;
        }

        static int Min4(int a, int b, int c, int d)
        {
            var min = a;
            if (b < min)
                min = b;
            if (c < min)
                min = c;
            if (d < min)
                min = d;
            return min;
        }

        static int SafeAdd(int baseValue, int incraseBy)
        {
            if (baseValue == int.MaxValue)
                return int.MaxValue;

            return baseValue + incraseBy;
        }

        public static void DistanceTransformChamfer(int[,] distance)
        {
            // chamfer 3-4 distance transform 
            var width = distance.GetLength(0);
            var height = distance.GetLength(1);

            // forward pass
            for (int x = 1; x < width - 1; x++) // left to right
            {
                for (int y = height - 2; y >= 1; y--) // top to bottom
                {
                    if (distance[x, y] == 0) // skip this cell if distance is already 0
                        continue;

                    var above = SafeAdd(distance[x, y + 1], 3);
                    var left = SafeAdd(distance[x - 1, y], 3);
                    var topLeft = SafeAdd(distance[x - 1, y + 1], 4);
                    var topRight = SafeAdd(distance[x + 1, y + 1], 4);

                    var best = Min4(above, left, topLeft, topRight);
                    if (best < distance[x, y])
                        distance[x, y] = best;
                }
            }

            // backwards pass
            for (int x = width - 2; x >= 1; x--) // right to left
            {
                for (int y = 1; y < height - 1; y++) // bottom to top
                {
                    if (distance[x, y] == 0) // skip this cell if distance is already 0
                        continue;

                    var down = SafeAdd(distance[x, y - 1], 3);
                    var right = SafeAdd(distance[x + 1, y], 3);
                    var botLeft = SafeAdd(distance[x - 1, y - 1], 4);
                    var botRight = SafeAdd(distance[x + 1, y - 1], 4);

                    var best = Min4(down, right, botLeft, botRight);
                    if (best < distance[x, y])
                        distance[x, y] = best;
                }
            }
        }


        public static void DistanceTransform(int[,] distance, DistanceTransformMethod distanceTransformMethod)
        {
            switch (distanceTransformMethod)
            {
                case DistanceTransformMethod.Chamfer:
                    DistanceTransformChamfer(distance);
                    break;
                case DistanceTransformMethod.Cityblock:
                    DistanceTransformCityBlock(distance);
                    break;
                default:
                    DistanceTransformChamfer(distance);
                    break;
            }
        }

        public static void DistanceTransformCityBlock(int[,] distance)
        {
            var width = distance.GetLength(0);
            var height = distance.GetLength(1);

            // forward pass
            for (int x = 0; x < width; x++) // left to right
            {
                for (int y = height - 1; y >= 0; y--) // top to bottom
                {
                    if (distance[x, y] == 0) // skip this cell if distance is already 0
                        continue;

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
                    if (distance[x, y] == 0) // skip this cell if distance is already 0
                        continue;

                    var below = y > 0 ? distance[x, y - 1] : int.MaxValue;
                    var right = x < width - 1 ? distance[x + 1, y] : int.MaxValue;
                    var best = Math.Min(below, right);
                    var newValue = best < int.MaxValue ? best + 1 : int.MaxValue;
                    if (newValue < distance[x, y])
                        distance[x, y] = newValue;
                }
            }
        }
    }
}