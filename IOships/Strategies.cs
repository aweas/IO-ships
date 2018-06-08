using System;
using System.Collections.Generic;
using System.Linq;

namespace IOships
{
    public struct Coords
    {
        public readonly int X;
        public readonly int Y;

        public Coords(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class InstructionsHelper
    {
        private readonly bool[,] _occupied;

        private readonly int _maxWidth;
        private readonly int _maxDepth;
        private int _occupiedTilesCount;

        public Dictionary<Coords, Guid> Instructions;

		public int GetWidth()
		{
			return _maxWidth;
		}

		public int GetDepth()
		{
			return _maxDepth;
		}

		public bool IsOccupied(int x, int y)
        {
            return _occupied[x, y];
        }

        public InstructionsHelper(Ship s)
        {
            _occupied = new bool[s.Width, s.Depth];
            for (var i = 0; i < s.Width; i++)
            for (var j = 0; j < s.Depth; j++)
                _occupied[i, j] = false;


            _maxWidth = s.Width;
            _maxDepth = s.Depth;

            Instructions = new Dictionary<Coords, Guid>();
        }

        public int GetPercentageFilled()
        {
            float sum = GetOccupiedTilesCount();

            var percentageFilled = (int) (sum / (_maxWidth * _maxDepth) * 100);

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

            Instructions.Add(new Coords(x, y), container.ID);
        }

		public Coords NextOccupyableCoords(Container c, int x = 0, int y = 0)
		{
			for(int i = 0; i<_maxWidth; ++i)
				for(int j = 0; j<_maxDepth; ++j)
					if(CanOccupy(c,i,j))
						return new Coords(i, j);

			return new Coords(-1, -1);
		}

		public Coords OccupantCoords(Container container)		/* TODO: check if error branch executes correctly */
		{
			Coords entry;

			try                                                 // getting coords struct of selected container
			{
				entry = Instructions.FirstOrDefault(e => e.Value == container.ID).Key;  
			}
			catch(ArgumentNullException)
			{
				return new Coords(-1, -1);
			}

			return entry;
		}

		public void RemoveOccupant(Container container)
		{
			Coords _coords = OccupantCoords(container);			// checking if container was found in cargo (Instructions)
			if (_coords.X < 0 || _coords.Y < 0)
				return;

			int x = _coords.X;
			int y = _coords.Y;

			for (var i = x; i < x + container.Width; i++)		// erasing container and coord occupying info from cargo
				for (var j = y; j < y + container.Depth; j++)
					_occupied[i, j] = false;

			Instructions.Remove(new Coords(x, y));
		}

        public IEnumerable<string> RowVisualisation()
        {
            for (var i = 0; i < _maxDepth; i++)
            {
                var row = "";
                for (var j = 0; j < _maxWidth; j++)
                {
                    if (Instructions.Keys.Contains(new Coords(j, i)))
                        row += "X";
                    else
                        row += _occupied[j, i] ? "1" : "0";
                }

                yield return row;
            }
        }

        public bool IsContPresent(Guid id)
        {
            return Instructions.ContainsValue(id);
        }
    }

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
        Dictionary<int, InstructionsHelper> GenerateData(CargoShipCollection ships, ref ContainersCollection containers);

        string ToString();
    }

    /// <inheritdoc />
    /// <summary>
    /// Placeholder strategy for collectionwise data generation
    /// </summary>
    public class RandomCollectionStrategy : IStrategy
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Dictionary<int, InstructionsHelper> GenerateData(CargoShipCollection ships, ref ContainersCollection containers)
        {
            Logger.Trace("Starting data generation");

            Random r = new Random();

            Dictionary<int, InstructionsHelper> res = new Dictionary<int, InstructionsHelper>();
            foreach (Ship s in ships)
            {
                var helper = new InstructionsHelper(s);

                for (int i = 0; i < 20; i++)
                {
                    int id = r.Next(containers.Count - 1);
                    int x = r.Next(s.Width - 1);
                    int y = r.Next(s.Depth - 1);

                    if (!helper.CanOccupy(containers[id], x, y))
                        continue;

                    helper.Occupy(containers[id], x, y);
                    containers.RemoveAt(id);
                }

                res.Add(s.ID, helper);
            }

            Logger.Trace("Finished data generation");
            return res;
        }
    }

    public class IterativeStrategy : IStrategy
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Dictionary<int, InstructionsHelper> GenerateData(CargoShipCollection ships, ref ContainersCollection containers)
        {
            Logger.Trace("Starting data generation");

            List<Container> sorted = containers.OrderBy(s => s.TurnCreated).ThenByDescending(s => s.Size).ToList();

            containers.RecreateFromList(sorted);

            Dictionary<int, InstructionsHelper> res = new Dictionary<int, InstructionsHelper>();
            foreach (Ship s in ships)
            {
                InstructionsHelper helper = FillShip(containers, s);

                res.Add(s.ID, helper);
            }

            Logger.Trace("Finished data generation");
            return res;
        }

        private static InstructionsHelper FillShip(ContainersCollection containers, Ship s)
        {
            var helper = new InstructionsHelper(s);

            for (var y = 0; y < s.Depth; y++)
            {
                for (var x = 0; x < s.Width; x++)
                {
                    if (helper.IsOccupied(x, y))
                        continue;

                    for (var i = 0; i < containers.Count; i++)
                    {
                        var c = containers[i];

                        if (!helper.CanOccupy(c, x, y))
                            continue;

                        helper.Occupy(c, x, y);
                        containers.Remove(c);
                    }
                }
            }

            return helper;
        }

        public override string ToString()
        {
            return "Iterative";
        }
    }
}