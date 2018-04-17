using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOships
{
    /// <summary>
    /// Interface which needs to be implemented by all strategies that ships can take to load cargo
    /// </summary>
    public interface IStrategy
    {
        ContainersCollection GenerateData(int shipID, int? priority);
    }

    /// <summary>
    /// Placeholder strategy that will generate random number of containers and wait for up to 10 seconds
    /// </summary>
    public class RandomStrategy: IStrategy
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Container RandomContainer(int seed)
        {
            Random r = new Random(seed);
            
            return new Container(r.Next(), r.Next(), r.Next(), r.Next(), r.Next());
        }

        public ContainersCollection GenerateData(int shipID, int? priority)
        {
            int seed = priority == null ? shipID : (int)priority;

            logger.Trace("Starting data generation for container {0}", shipID);

            ContainersCollection data = new ContainersCollection();
            Random r = new Random();
            int cap = r.Next(1, 100);

            for (int i = 0; i < cap; i++)
                data.Add(RandomContainer(seed));

            logger.Trace("Finished data generation for container {0}", shipID);

            int delay = r.Next(1, 100) * 10;
            System.Threading.Thread.Sleep(delay);
            return data;
        }
    }

}
