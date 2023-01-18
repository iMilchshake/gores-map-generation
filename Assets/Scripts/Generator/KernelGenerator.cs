using System;
using System.Linq;
using Util;

namespace Generator
{
    [Serializable]
    public struct KernelSizeConfig
    {
        public int Size;
        public float SizeProbability;
        public KernelCircularityConfig[] CirularicyProbabilities;
    }

    [Serializable]
    public struct KernelCircularityConfig
    {
        public float Circularity;
        public float Probability;
    }

    public class KernelGenerator
    {
        public KernelSizeConfig[] config;
        private float _circularity;
        private int _size;
        private bool[,] _currentKernel;

        public KernelGenerator(KernelSizeConfig[] config, int size, float circularity)
        {
            this.config = config;
            this._circularity = circularity;
            this._size = size;
            _currentKernel = GetKernel(size, this._circularity);

            ValidateConfig();
        }

        // sanity check -> do all probabilities sum up to 1? Yes this is useful, i already fucked them up 3 times now help
        private void ValidateConfig()
        {
            var sizeProbabilitySum = config.Sum(c => c.SizeProbability);
            if (!MathUtil.CheckFloatEqual(sizeProbabilitySum, 1.0f))
                throw new ArithmeticException("size probabilities dont sum up to 1");

            foreach (var sizeConfig in config)
            {
                var circularityProbabilitySum = sizeConfig.CirularicyProbabilities.Sum(c => c.Probability);
                if (!MathUtil.CheckFloatEqual(circularityProbabilitySum, 1.0f))
                    throw new ArithmeticException(
                        $"circularity probabilities dont sum up to 1 for size={sizeConfig.Size}");
            }
        }

        public void Mutate(float sizeUpdateProbability, float circularityUpdateProbability, RandomGenerator rndGen)
        {
            var updateSize = rndGen.RandomBool(sizeUpdateProbability);
            var updateCircularity = rndGen.RandomBool(circularityUpdateProbability);

            if (updateSize)
            {
                var probabilities = config.Select(c => c.SizeProbability).ToArray();
                var sizes = config.Select(c => c.Size).ToArray();
                var selectedSize = rndGen.RandomRouletteSelect(sizes, probabilities);
                _size = selectedSize;
            }

            if (updateCircularity)
            {
                // get correct sizeConfig
                var index = Array.FindIndex(config, sizeConfig => sizeConfig.Size == _size);

                var circularities = config[index].CirularicyProbabilities.Select(c => c.Circularity).ToArray();
                var probabilities = config[index].CirularicyProbabilities.Select(c => c.Probability).ToArray();
                var selectedCircularity = rndGen.RandomRouletteSelect(circularities, probabilities);
                _circularity = selectedCircularity;
            }

            // if a change occured -> update current kernel
            if (updateSize || updateCircularity)
            {
                _currentKernel = GetKernel(_size, _circularity);
            }
        }

        public bool[,] GetCurrentKernel()
        {
            return _currentKernel;
        }

        public static bool[,] GetKernel(int size, float circularity)
        {
            var kernel = new bool[size, size];
            var center = (float)(size - 1) / 2;

            // calculate radius based on the size and circularity
            var minRadius = (float)(size - 1) / 2; // min radius is from center to border
            var maxRadius = Math.Sqrt(center * center + center * center); // max radius is from center to corner
            var radius = circularity * minRadius + (1 - circularity) * maxRadius;

            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    var distance = Math.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                    if (distance <= radius)
                    {
                        kernel[x, y] = true;
                    }
                }
            }

            return kernel;
        }

        public void ForceKernelConfig(int size, float circularity)
        {
            _size = size;
            _circularity = circularity;
            _currentKernel = GetKernel(size, circularity);
        }
    }
}