namespace Util
{
    public enum BlockType
    {
        Hookable,
        Unhookable,
        Freeze,
        Empty,
        Obstacle,
        Platform,
        Debug,
        Start,
        Spawn,
        Finish
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
        Tunnel
    }
}