using System;
using Unity.Collections;
using UnityEngine;
using Random = System.Random;

public class RandomGenerator
{
    public Random Rnd;

    public RandomGenerator(int seed)
    {
        Rnd = new Random(seed);
    }

    public Vector2Int GetRandomDirectionVector()
    {
        return new Vector2Int(Rnd.Next(-1, 2), Rnd.Next(-1, 2)); // returns one of [-1, 0, 1] for x and y
    }

    public Vector2Int PickRandomMove(MoveArray moves)
    {
        var probabilitySum = moves.Sum(); // could be something other than 1

        var pickedRandomValue = Rnd.NextDouble() * probabilitySum;
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
        return (float)Rnd.NextDouble() <= probability;
    }

    public T RandomChoice<T>(T[] array)
    {
        var index = Rnd.Next(array.Length);
        return array[index];
    }

    public (int, int) GetRandomPosition(Map map)
    {
        return (Rnd.Next(map.width), Rnd.Next(map.height));
    }

    public (int, int) GetRandomPositionWithType(Map map, BlockType type)
    {
        var xPos = 0;
        var yPos = 0;
        while (map[xPos, yPos] != type) // TODO: this could get stuck if type is not present...
        {
            (xPos, yPos) = GetRandomPosition(map);
        }

        return (xPos, yPos);
    }
}