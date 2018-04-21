using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOships
{
    public struct Coords { public int x; public int y; }

    class InstructionsHelper
    {
        bool[,] occupied;
        int maxWidth;
        int maxDepth;

        public Dictionary<Coords, int> instructions;

        public InstructionsHelper(Ship s)
        {
            occupied = new bool[s.width, s.depth];
            for (int i = 0; i < s.width; i++)
                for (int j = 0; j < s.depth; j++)
                    occupied[i, j] = false;


            maxWidth = s.width;
            maxDepth = s.depth;

            instructions = new Dictionary<Coords, int>();
        }

        public bool CanOccupy(Container c, int x, int y)
        {
            if (x + c.width > maxWidth)
                return false;
            if (y + c.depth > maxDepth)
                return false;

            for (int i = x; i < x + c.width; i++)
                for (int j = y; j < y + c.depth; j++)
                    if (occupied[i, j])
                        return false;

            return true;
        }

        public void Occupy(Container c, int x, int y)
        {
            for (int i = x; i < x + c.width; i++)
                for (int j = y; j < y + c.depth; j++)
                    occupied[i, j] = true;

            instructions.Add(new Coords { x = x, y = y }, c.ID);
        }
    }

    public enum LoadingMode { Elementwise, Collectionwise }

    //TODO: Delete shipwise strategies, this idea is to be aborted
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
        ContainersCollection GenerateData(Ship ship, ContainersCollection containers);
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
        Dictionary<int, Dictionary<Coords, int>> GenerateData(CargoShipCollection ships, ContainersCollection containers);
    }


    /// <summary>
    /// Placeholder strategy that will generate random number of containers and wait for up to 10 seconds
    /// </summary>
    public class RandomShipStrategy: IShipwiseStrategy
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Container RandomContainer(Random r)
        {            
            return new Container(r.Next(), r.Next(), r.Next(), r.Next());
        }

        public ContainersCollection GenerateData(Ship ship, ContainersCollection containers)
        {
            logger.Trace("Starting data generation for container {0}", ship.ID);

            ContainersCollection data = new ContainersCollection();
            Random r = new Random();
            int cap = r.Next(1, 100);

            for (int i = 0; i < cap; i++)
                data.Add(RandomContainer(r));

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

            return new Container(r.Next(5), r.Next(5), r.Next(), r.Next());
        }

        public Dictionary<int, Dictionary<Coords, int>> GenerateData(CargoShipCollection ships, ContainersCollection containers)
        {
            logger.Trace("Starting data generation");

            Random r = new Random();

            Dictionary<int, Dictionary<Coords, int>> res = new Dictionary<int, Dictionary<Coords, int>>();
            foreach (Ship s in ships)
            {
                InstructionsHelper helper = new InstructionsHelper(s);

                for(int i=0; i<20; i++)
                {
                    int id = r.Next(containers.Count - 1);
                    int x = r.Next(s.width - 1);
                    int y = r.Next(s.depth - 1);

                    if (helper.CanOccupy(containers[id], x, y))
                    {
                        helper.Occupy(containers[id], x, y);
                        containers.RemoveAt(id);
                    }
                }

                res.Add(s.ID, helper.instructions);
            }
            logger.Trace("Finished data generation");
            return res;
        }
    }

}
