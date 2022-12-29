using UnityEngine;
using Random = System.Random;

public class RandomGenerator
{
    private Random _rnd;

    public RandomGenerator(int seed)
    {
        _rnd = new Random(seed);
    }

    public Vector2Int GetRandomDirectionVector()
    {
        return new Vector2Int(_rnd.Next(-1, 2), _rnd.Next(-1, 2)); // returns one of [-1, 0, 1] for x and y
    }

    public Vector2Int PickRandomMove(MoveArray moves)
    {
        return Vector2Int.zero;
    }
}