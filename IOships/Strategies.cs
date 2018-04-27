using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOships
{
    public struct Coords { public int x; public int y; }

    public class InstructionsHelper
    {
        private bool[,] occupied;
        private int maxWidth;
        private int maxDepth;
        int occupiedTilesCount;

        public Dictionary<Coords, int> Instructions;

        public int GetPercentageFilled()
        {
            float sum = GetOccupiedTilesCount();

            int percentageFilled = (int)(sum / (maxWidth * maxDepth) * 100);

            return percentageFilled;
        }

        public int GetOccupiedTilesCount()
        {
            if(occupiedTilesCount==0)
            {
                int sum = 0;
                for (int i = 0; i < maxWidth; i++)
                    for (int j = 0; j < maxDepth; j++)
                        if (occupied[i, j])
                            sum++;

                occupiedTilesCount = sum;
            }

            return occupiedTilesCount;
        }

        public InstructionsHelper(Ship s)
        {
            occupied = new bool[s.width, s.depth];
            for (int i = 0; i < s.width; i++)
                for (int j = 0; j < s.depth; j++)
                    occupied[i, j] = false;


            maxWidth = s.width;
            maxDepth = s.depth;

            Instructions = new Dictionary<Coords, int>();
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

            Instructions.Add(new Coords { x = x, y = y }, c.ID);
        }
    }

    public enum LoadingMode { Iterative, Random }

    /// <summary>
    /// Interface which needs to be implemented by all collectionwise strategies that ships can take to load cargo
    /// </summary>
    public interface IStrategy
    {
        /// <summary>
        /// Function that generates containers list for selected collection
        /// </summary>
        /// <param name="ships">Collection that wants to generate data</param>
        /// <returns>Dictionary of ship ID's and list of their containers</returns>
        Dictionary<int, InstructionsHelper> GenerateData(CargoShipCollection ships, ContainersCollection containers);
    }

    /// <summary>
    /// Placeholder strategy for collectionwise data generation
    /// </summary>
    public class RandomCollectionStrategy : IStrategy
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Container RandomContainer()
        {
            Random r = new Random();

            return new Container(r.Next(5), r.Next(5), r.Next(), r.Next());
        }

        public Dictionary<int, InstructionsHelper> GenerateData(CargoShipCollection ships, ContainersCollection containers)
        {
            logger.Trace("Starting data generation");

            Random r = new Random();

            Dictionary<int, InstructionsHelper> res = new Dictionary<int, InstructionsHelper>();
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

                res.Add(s.ID, helper);
            }
            logger.Trace("Finished data generation");
            return res;
        }
    }

}
