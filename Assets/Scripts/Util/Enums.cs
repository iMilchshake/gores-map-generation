namespace Util
{
    public enum BlockType
    {
        Hookable,
        Unhookable,
        Freeze,
        MarginFreeze, // counts as empty for freeze generation, but will be generated as freeze 
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

    public enum MapGeneratorState
    {
        DistanceProbability,
        Tunnel
    }
}