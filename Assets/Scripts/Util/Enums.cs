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

    public static class BlockTypeGroup
    {
        public static bool IsSolid(this BlockType blockType)
        {
            return blockType switch
            {
                BlockType.Hookable
                    or BlockType.Unhookable
                    or BlockType.Obstacle
                    or BlockType.Platform
                    => true,
                _ => false
            };
        }

        public static bool IsAny(this BlockType blockType)
        {
            return blockType != BlockType.Empty;
        }

        public static bool IsFreezeOrEmpty(this BlockType blockType)
        {
            return blockType switch
            {
                BlockType.Freeze
                    or BlockType.MarginFreeze
                    or BlockType.Empty
                    => true,
                _ => false
            };
        }

        public static bool IsFreeze(this BlockType blockType)
        {
            return blockType switch
            {
                BlockType.Freeze or BlockType.MarginFreeze => true,
                _ => false
            };
        }
    }
}