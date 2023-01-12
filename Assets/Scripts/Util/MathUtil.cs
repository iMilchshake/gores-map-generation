using System;
using Generator;

namespace Util
{
    public abstract class MathUtil
    {
        public static float GeometricDistribution(int x, float p)
        {
            return (float)Math.Pow(1 - p, x - 1) * p;
        }

        public static bool CheckFloatEqual(float a, float b)
        {
            return Math.Abs(a - b) < 1e-5;
        }

        static float Min4(float a, float b, float c, float d)
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

        static float SafeAdd(float baseValue, float incraseBy)
        {
            if (baseValue == float.MaxValue)
                return float.MaxValue;

            return baseValue + incraseBy;
        }

        public static float[,] DistanceTransform(Map map, DistanceTransformMethod distanceTransformMethod)
        {
            return distanceTransformMethod switch
            {
                DistanceTransformMethod.ChamferScaled => DistanceTransformChamferScaled(map),
                DistanceTransformMethod.QuasiEuclidean => DistanceTransformSemiEuclidean(map),
                DistanceTransformMethod.Cityblock => DistanceTransformCityBlock(map),
                DistanceTransformMethod.Euclidean => DistanceTransformEuclideanApprox(map),
                _ => DistanceTransformCityBlock(map)
            };
        }

        private static float[,] DistanceTransform3X3(Map map, float adjacentCost, float diagonalCost)
        {
            // Generic implementation for any 3x3 distance transform depending on adjacent and diagonal costs 
            int width = map.Width;
            int height = map.Height;

            // setup distance array 
            var distance = new float[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    distance[x, y] = map[x, y] switch
                    {
                        BlockType.Hookable => 0f,
                        _ => float.MaxValue
                    };
                }
            }

            // forward pass
            for (int x = 1; x < width - 1; x++) // left to right
            {
                for (int y = height - 2; y >= 1; y--) // top to bottom
                {
                    if (distance[x, y] == 0f) // skip this cell if distance is already 0
                        continue;

                    float above = SafeAdd(distance[x, y + 1], adjacentCost);
                    float left = SafeAdd(distance[x - 1, y], adjacentCost);
                    float topLeft = SafeAdd(distance[x - 1, y + 1], diagonalCost);
                    float topRight = SafeAdd(distance[x + 1, y + 1], diagonalCost);

                    float best = Min4(above, left, topLeft, topRight);
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

                    float down = SafeAdd(distance[x, y - 1], adjacentCost);
                    float right = SafeAdd(distance[x + 1, y], adjacentCost);
                    float botLeft = SafeAdd(distance[x - 1, y - 1], diagonalCost);
                    float botRight = SafeAdd(distance[x + 1, y - 1], diagonalCost);

                    float best = Min4(down, right, botLeft, botRight);
                    if (best < distance[x, y])
                        distance[x, y] = best;
                }
            }

            return distance;
        }


        private static float[,] DistanceTransformChamferScaled(Map map)
        {
            // all costs are divided by 3 so calculated distances are on a similar scale
            return DistanceTransform3X3(map, 1f, 4f / 3f);
        }

        private static float[,] DistanceTransformCityBlock(Map map)
        {
            return DistanceTransform3X3(map, 1f, 2f);
        }

        private static float[,] DistanceTransformSemiEuclidean(Map map)
        {
            return DistanceTransform3X3(map, 1f, 1.41f);
        }

        private static float[,] DistanceTransformEuclideanApprox(Map map)
        {
            // approximated euclidean distance transform 
            int width = map.Width;
            int height = map.Height;

            // setup distance array 
            var distance = new float[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    distance[x, y] = map[x, y] switch
                    {
                        BlockType.Hookable => 0f,
                        _ => float.MaxValue
                    };
                }
            }

            int kernelSize = 9; // must be odd
            int kernelMargin = kernelSize / 2;

            // forward pass
            for (int x = kernelMargin; x < width - kernelMargin; x++) // left to right
            {
                for (int y = height - kernelMargin - 1; y >= kernelMargin; y--) // top to bottom
                {
                    if (distance[x, y] == 0) // skip this cell if distance is already 0
                        continue;

                    // calculate all euclidean distances for the given kernel size
                    float bestDistance = float.MaxValue;
                    for (int xKernel = -kernelMargin; xKernel <= kernelMargin; xKernel++)
                    {
                        for (int yKernel = -kernelMargin; yKernel <= kernelMargin; yKernel++)
                        {
                            // skip parts of the kernel from the backward mask
                            if (yKernel < 0 || (yKernel == 0 && xKernel > 0))
                                continue;

                            float dist = SafeAdd(distance[x + xKernel, y + yKernel],
                                (float)Math.Sqrt((xKernel * xKernel) + (yKernel * yKernel)));
                            if (dist < bestDistance)
                                bestDistance = dist;
                        }
                    }

                    if (bestDistance < distance[x, y])
                        distance[x, y] = bestDistance;
                }
            }

            // backwards pass
            for (int x = width - kernelMargin - 1; x >= kernelMargin; x--) // right to left
            {
                for (int y = kernelMargin; y < height - kernelMargin; y++) // bottom to top
                {
                    if (distance[x, y] == 0) // skip this cell if distance is already 0
                        continue;

                    // calculate all euclidean distances for the given kernel size
                    float bestDistance = float.MaxValue;
                    for (int xKernel = -kernelMargin; xKernel <= kernelMargin; xKernel++)
                    {
                        for (int yKernel = -kernelMargin; yKernel <= kernelMargin; yKernel++)
                        {
                            // skip parts of the kernel from the forward mask
                            if (yKernel > 0 || (yKernel == 0 && xKernel < 0))
                                continue;

                            float dist = SafeAdd(distance[x + xKernel, y + yKernel],
                                (float)Math.Sqrt((xKernel * xKernel) + (yKernel * yKernel)));
                            if (dist < bestDistance)
                                bestDistance = dist;
                        }
                    }

                    if (bestDistance < distance[x, y])
                        distance[x, y] = bestDistance;
                }
            }

            return distance;
        }
    }
}