using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOships
{
    public enum LoadingMode { Elementwise, Collectionwise }

    /// <summary>
    /// Interface which needs to be implemented by all shipwise strategies that ships can take to load cargo
    /// </summary>
    public interface IShipwiseStrategy
    {
        /// <summary>
        /// Function that generates containers list for selected ship
        /// </summary>
        /// <param name="ship">Ship that wants to generate data</param>
        /// <returns>List of containers that this ship will take</returns>
        ContainersCollection GenerateData(Ship ship);
    }

    /// <summary>
    /// Interface which needs to be implemented by all collectionwise strategies that ships can take to load cargo
    /// </summary>
    public interface ICollectionwiseStrategy
    {
        /// <summary>
        /// Function that generates containers list for selected collection
        /// </summary>
        /// <param name="ships">Collection that wants to generate data</param>
        /// <returns>Dictionary of ship ID's and list of their containers</returns>
        Dictionary<int, ContainersCollection> GenerateData(CargoShipCollection ships);
    }


    /// <summary>
    /// Placeholder strategy that will generate random number of containers and wait for up to 10 seconds
    /// </summary>
    public class RandomShipStrategy: IShipwiseStrategy
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Container RandomContainer(int seed)
        {
            Random r = new Random(seed);
            
            return new Container(r.Next(), r.Next(), r.Next(), r.Next(), r.Next());
        }

        public ContainersCollection GenerateData(Ship ship)
        {
            int seed = ship.ID;

            logger.Trace("Starting data generation for container {0}", ship.ID);

            ContainersCollection data = new ContainersCollection();
            Random r = new Random();
            int cap = r.Next(1, 100);

            for (int i = 0; i < cap; i++)
                data.Add(RandomContainer(seed));

            logger.Trace("Finished data generation for container {0}", ship.ID);

            int delay = r.Next(1, 100) * 10;
            System.Threading.Thread.Sleep(delay);
            return data;
        }
    }

    /// <summary>
    /// Placeholder strategy for collectionwise data generation
    /// </summary>
    public class RandomCollectionStrategy : ICollectionwiseStrategy
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Container RandomContainer()
        {
            Random r = new Random();

            return new Container(r.Next(), r.Next(), r.Next(), r.Next(), r.Next());
        }

        public Dictionary<int, ContainersCollection> GenerateData(CargoShipCollection ships)
        {
            logger.Trace("Starting data generation");

            Random r = new Random();

            Dictionary<int, ContainersCollection> res = new Dictionary<int, ContainersCollection>();
            foreach (Ship s in ships)
            {
                ContainersCollection data = new ContainersCollection();

                int cap = r.Next(1, 100);

                for (int i = 0; i < cap; i++)
                    data.Add(RandomContainer());

                res.Add(s.ID, data);

                int delay = r.Next(1, 100) * 10;
                System.Threading.Thread.Sleep(delay);
            }
            logger.Trace("Finished data generation");
            return res;
        }
    }

}
