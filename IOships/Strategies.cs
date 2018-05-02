using System;
using System.Collections.Generic;

namespace IOships
{
    public struct Coords { public int X; public int Y; }

    public class InstructionsHelper
    {
        private readonly bool[,] _occupied;
        private readonly int _maxWidth;
        private readonly int _maxDepth;
        private int _occupiedTilesCount;

        public readonly Dictionary<Coords, int> Instructions;

        public InstructionsHelper(Ship s)
        {
            _occupied = new bool[s.Width, s.Depth];
            for (var i = 0; i < s.Width; i++)
                for (var j = 0; j < s.Depth; j++)
                    _occupied[i, j] = false;


            _maxWidth = s.Width;
            _maxDepth = s.Depth;

            Instructions = new Dictionary<Coords, int>();
        }

        public int GetPercentageFilled()
        {
            float sum = GetOccupiedTilesCount();

            var percentageFilled = (int)(sum / (_maxWidth * _maxDepth) * 100);

            return percentageFilled;
        }

        public int GetOccupiedTilesCount()
        {
            if (_occupiedTilesCount != 0)
                return _occupiedTilesCount;

            var sum = 0;
            for (var i = 0; i < _maxWidth; i++)
            for (var j = 0; j < _maxDepth; j++)
                if (_occupied[i, j])
                    sum++;

            _occupiedTilesCount = sum;

            return _occupiedTilesCount;
        }

        public bool CanOccupy(Container c, int x, int y)
        {
            if (x + c.Width > _maxWidth)
                return false;
            if (y + c.Depth > _maxDepth)
                return false;

            for (var i = x; i < x + c.Width; i++)
                for (var j = y; j < y + c.Depth; j++)
                    if (_occupied[i, j])
                        return false;

            return true;
        }

        public void Occupy(Container container, int x, int y)
        {
            for (var i = x; i < x + container.Width; i++)
                for (var j = y; j < y + container.Depth; j++)
                    _occupied[i, j] = true;

            Instructions.Add(new Coords { X = x, Y = y }, container.ID);
        }

        public IEnumerable<string> RowVisualisation()
        {
            for (var i=0; i<_maxDepth; i++)
            {
                var row = "";
                for (var j = 0; j < _maxWidth; j++)
                    row += _occupied[j, i] ? "1" : "0";
                yield return row;
            }
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
        /// <param name="ships">Collection of ships that wants to generate data</param>
        /// <param name="containers">Collection of containers that will be available</param>
        /// <returns>Dictionary of ship ID's and list of their containers</returns>
        Dictionary<int, InstructionsHelper> GenerateData(CargoShipCollection ships, ContainersCollection containers);
    }

    /// <summary>
    /// Placeholder strategy for collectionwise data generation
    /// </summary>
    public class RandomCollectionStrategy : IStrategy
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Dictionary<int, InstructionsHelper> GenerateData(CargoShipCollection ships, ContainersCollection containers)
        {
            Logger.Trace("Starting data generation");

            Random r = new Random();

            Dictionary<int, InstructionsHelper> res = new Dictionary<int, InstructionsHelper>();
            foreach (Ship s in ships)
            {
                var helper = new InstructionsHelper(s);

                for(int i=0; i<20; i++)
                {
                    int id = r.Next(containers.Count - 1);
                    int x = r.Next(s.Width - 1);
                    int y = r.Next(s.Depth - 1);

                    if (helper.CanOccupy(containers[id], x, y))
                    {
                        helper.Occupy(containers[id], x, y);
                        containers.RemoveAt(id);
                    }
                }

                res.Add(s.ID, helper);
            }
            Logger.Trace("Finished data generation");
            return res;
        }
    }

}
