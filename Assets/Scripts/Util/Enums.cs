﻿namespace Util
{
    public enum BlockType
    {
        Hookable,
        Unhookable,
        Freeze,
        Empty,
        Obstacle
    }


    public enum DistanceTransformMethod
    {
        Cityblock,
        ChamferScaled,
        QuasiEuclidean,
        Euclidean
    }

    public enum MapGeneratorMode
    {
        DistanceProbability,
        Tunnel // 
    }

    public enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }
}