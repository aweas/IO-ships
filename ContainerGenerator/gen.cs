using System;
using System.IO;
using System.Collections.Generic;

namespace gen
{
    /// <summary>
    /// Basic structure that will represent every single one of our containers
    /// </summary>
    public struct Container
    {
        public int width, depth, ID;

        /// <summary>
        /// Creates container structure with given parameters
        /// </summary>
        /// <param name="width">Width of the container</param>
        /// <param name="depth">Depth of the container</param>
        /// <param name="id">Container ID</param>
        public Container(int width, int depth, int id)
        {
            this.width = width;
            this.depth = depth;
            this.ID = id;
        }
    }
    
    /// <summary>
    /// Basic container generator
    /// </summary>
    class gen
    {
        private Random rng = new Random();
        private List<Container> containers;

        /// <summary>
        /// Generates specified amount of conatiners, initial ID can be specified for multiple file consistency sake
        /// </summary>
        /// <param name="amount">Amount of containers</param>
        /// <param name="lastID">Initial unique id for use in labelling</param>
        public void generate(UInt32 amount, UInt32 lastID = 0)
        {
            containers = new List<Container>(capacity: (Int32)amount);

            for (Int32 i = 0; i < amount; ++i)
                containers.Add(new Container(width: SampleGaussian(rng, 3, 0.5), depth: SampleGaussian(rng, 8, 1), id: (int)(i + lastID + 1)));
        }

        /// <summary>
        /// Saves generated containers to csv file
        /// </summary>
        /// <param name="filename">Filename</param>
        public void saveToFile(string filename)
        {
            if (containers == null)
                return;

            Int32 l_size = containers.Count;
            using (StreamWriter data = new StreamWriter(path: filename +".csv"))
            {
                data.WriteLine("id;width;depth"); 
                
                for(Int32 i=0;i<l_size;++i)
                    data.WriteLine(value: $"{containers[i].ID};{containers[i].width};{containers[i].depth}");
            }
           
        }

        /// <summary>
        /// Auxialiary random number generation function with gaussian distribution
        /// </summary>
        /// <param name="random">Random number generator object</param>
        /// <param name="mean">Distribution's mean</param>
        /// <param name="stddev">Standard deviation</param>
        private static int SampleGaussian(Random random, double mean, double stddev)
        {
            int result;
            
            double x1 = 1 - random.NextDouble();
            double x2 = 1 - random.NextDouble();

            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            result = (int)(y1 * stddev + mean);
            
            if(result <= 0 ) 
                result = (int)mean;

            return result; 
        }

        public List<Container> getContList() => containers;

    }

    class test
    {
        static void Main(string[] args)
        {
            gen kek = new gen();

            kek.generate(amount: 100000);

            kek.saveToFile(filename: "kek");
        }
    }
}