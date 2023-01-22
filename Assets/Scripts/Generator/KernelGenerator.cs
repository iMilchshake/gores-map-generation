using System;
using System.Linq;
using MonoBehaviour;
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
        public KernelSizeConfig[] kernelConfig;
        private RandomGenerator _rndGen;
        private float _circularity;
        private int _size;
        private float _outerCircularity;
        private int _outerSize;
        private bool[,] _currentKernel;
        private bool[,] _currentOuterKernel;

        public KernelGenerator(KernelSizeConfig[] kernelConfig, int size, float circularity, RandomGenerator rndGen)
        {
            this.kernelConfig = kernelConfig;
            _rndGen = rndGen;
            _circularity = circularity;
            _size = size;
            _outerCircularity = circularity;
            _outerSize = size;

            ValidateConfig();
            UpdateKernel();
        }

        // sanity check -> do all probabilities sum up to 1? Yes this is useful, i already fucked them up 3 times now help
        private void ValidateConfig()
        {
            var sizeProbabilitySum = kernelConfig.Sum(c => c.SizeProbability);
            if (!MathUtil.CheckFloatEqual(sizeProbabilitySum, 1.0f))
                throw new ArithmeticException("size probabilities dont sum up to 1");

            foreach (var sizeConfig in kernelConfig)
            {
                var circularityProbabilitySum = sizeConfig.CirularicyProbabilities.Sum(c => c.Probability);
                if (!MathUtil.CheckFloatEqual(circularityProbabilitySum, 1.0f))
                    throw new ArithmeticException(
                        $"circularity probabilities dont sum up to 1 for size={sizeConfig.Size}");
            }
        }

        public void Mutate(MapGenerationConfig config)
        {
            var updateSize = _rndGen.RandomBool(config.kernelSizeChangeProb);
            var updateCircularity = _rndGen.RandomBool(config.kernelCircularityChangeProb);

            if (updateSize)
            {
                // update inner kernel
                var probabilities = kernelConfig.Select(c => c.SizeProbability).ToArray();
                var sizes = kernelConfig.Select(c => c.Size).ToArray();
                var selectedSize = _rndGen.RandomRouletteSelect(sizes, probabilities);
                _size = selectedSize;

                // update outer kernel
                _outerSize = _rndGen.RandomBool(config.kernelOuterSizeMarginProb) ? _size + 2 : _size;
            }

            if (updateCircularity)
            {
                // get correct sizeConfig
                var index = Array.FindIndex(kernelConfig, sizeConfig => sizeConfig.Size == _size);

                var circularities = kernelConfig[index].CirularicyProbabilities.Select(c => c.Circularity).ToArray();
                var probabilities = kernelConfig[index].CirularicyProbabilities.Select(c => c.Probability).ToArray();
                var selectedCircularity = _rndGen.RandomRouletteSelect(circularities, probabilities);

                _circularity = selectedCircularity;
                _outerCircularity = _rndGen.RandomBool(config.kernelOuterCircularityProb) ? _circularity : 0.0f;
            }

            // if a change occured -> update current kernel
            if (updateSize || updateCircularity)
            {
                UpdateKernel();
            }
        }

        private void UpdateKernel()
        {
            _currentKernel = GetKernel(_size, _circularity);
            _currentOuterKernel = GetKernel(_outerSize, _outerCircularity);
        }

        public bool[,] GetCurrentKernel()
        {
            return _currentKernel;
        }

        public bool[,] GetCurrentOuterKernel()
        {
            return _currentOuterKernel;
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

        public void ForceKernelConfig(int size, float circularity, int outerSize, float outerCircularity)
        {
            _size = size;
            _circularity = circularity;
            _outerSize = outerSize;
            _outerCircularity = outerCircularity;

            UpdateKernel();
        }
    }
}