using System;
using System.Collections.Generic;
using System.IO;
using IOships;

namespace ContainerGenerator
{
    /// <summary>
    /// Basic container generator
    /// </summary>
    public class ContainerGenerator
    {
        private readonly Random _rng = new Random();
        public List<Container> Containers { get; private set; }

        /// <summary>
        /// Generates specified amount of conatiners, initial ID can be specified for multiple file consistency sake
        /// </summary>
        /// <param name="amount">Amount of containers</param>
        /// <param name="filename">Name of target generation file</param>
        public void Generate(uint amount, string filename = null)
        {
            Containers = new List<Container>(capacity: (int) amount);

            for (int i = 0; i < amount; ++i)
                Containers.Add(new Container(
                    width: _SampleGaussian(_rng, 2, 1),
                    depth: _SampleGaussian(_rng, 3, 2),
                    turnCreated: null));

            if (!(filename is null))
                _SaveToFile(filename);
        }

        /// <summary>
        /// Saves generated containers to csv file
        /// </summary>
        /// <param name="filename">Filename</param>
        private void _SaveToFile(string filename)
        {
            if (Containers == null)
                throw new Exception("Containers object was not initialized");

            int lSize = Containers.Count;
            using (var data = new StreamWriter(path: filename + ".csv"))
            {
                data.WriteLine("id;width;depth");

                for (int i = 0; i < lSize; ++i)
                    data.WriteLine(value: $"{Containers[i].ID};{Containers[i].Width};{Containers[i].Depth}");
            }
        }

        /// <summary>
        /// Auxialiary random number generation function with gaussian distribution
        /// </summary>
        /// <param name="random">Random number generator object</param>
        /// <param name="mean">Distribution's mean</param>
        /// <param name="stddev">Standard deviation</param>
        private static int _SampleGaussian(Random random, double mean, double stddev)
        {
            var x1 = 1 - random.NextDouble();
            var x2 = 1 - random.NextDouble();

            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            var result = (int) (y1 * stddev + mean);

            if (result <= 0)
                result = (int) mean;

            return result;
        }

        public List<Container> GetContList() => Containers;

        public static int SampleGaussian(Random random, double mean, double stddev) =>
            _SampleGaussian(random, mean, stddev);

        public static void Main()
        {
            var cg = new ContainerGenerator();
            cg.Generate(150, "containers");
        }
    }
}