using System;
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
        var probabilitySum = moves.Sum(); // could be something other than 1

        var pickedRandomValue = _rnd.NextDouble() * probabilitySum;
        var currentValueSum = 0f;
        foreach (var move in moves.GetAllValidMoves())
        {
            currentValueSum += moves[move]; // add current probability
            if (currentValueSum >= pickedRandomValue)
            {
                return move; // picked value was surpassed, return current move
            }
        }

        throw new Exception("no move was picked"); // this shouldn't happen lol
    }

    public bool RandomBool(float probability)
    {
        return (float)_rnd.NextDouble() <= probability;
    }

    public T RandomChoice<T>(T[] array)
    {
        var index = _rnd.Next(array.Length);
        return array[index];
    }

    public (int, int) GetRandomPosition(Map map)
    {
        return (_rnd.Next(map.width), _rnd.Next(map.height));
    }
}