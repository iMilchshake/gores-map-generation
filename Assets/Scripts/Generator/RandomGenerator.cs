using System;
using UnityEngine;
using Random = System.Random;

namespace Generator
{
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
            if (Math.Abs(moves.Sum() - 1.0f) > 1e-5)
                throw new Exception("probability doesnt sum up to 1");

            // this is a shitty way of flattening the 2D array TODO: update this when MoveArray gets reworked!
            var probabilities = new float[9];
            var index = 0;
            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    probabilities[index] = moves[x, y];
                    index++;
                }
            }

            return RandomRouletteSelect(moves.GetAllValidMoves(), probabilities);
        }


        public T RandomRouletteSelect<T>(T[] options, float[] probabilities)
        {
            // similar to https://en.wikipedia.org/wiki/Fitness_proportionate_selection
            // expects probabilities to sum to 1

            var length = options.Length;
            if (length != probabilities.Length)
                throw new Exception("inputs dont have the same length");

            var pickedRandomValue = _rnd.NextDouble();
            var currentValueSum = 0f;

            for (int index = 0; index < length; index++)
            {
                currentValueSum += probabilities[index];
                if (currentValueSum >= pickedRandomValue)
                {
                    return options[index]; // picked value was surpassed, return current option 
                }
            }

            throw new Exception("no option was selected"); // this cant really happen
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
            return (_rnd.Next(map.Width), _rnd.Next(map.Height));
        }
    }
}