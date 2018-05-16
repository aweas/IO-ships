using System;
using System.Collections.Generic;
using System.Linq;

namespace IOships
{
    /// <summary>
    /// Genetic algorithm's logic resides here
    /// </summary>
    class GenAlgorithm : IStrategy
    {
		private int SpecAmount = 100;
        private CargoShipCollection _ships;
        private ContainersCollection _containers;
		private List<Specimen> _specimen;

		/// <summary>
		/// Struct representing a solution, holds set of instructions 
		/// for container placement and fitness of such solution
		/// </summary>
		struct Specimen
        {
            private double _fitness;
            private readonly List<InstructionsHelper> _shipCargo;
			private Random _rng;

			///<summary>
			/// Initializes specimen with its cargo holds/ships representation
			///</summary>
			public Specimen(CargoShipCollection ships)
            {
                _fitness = 0;
                _shipCargo = new List<InstructionsHelper>();
				_rng = new Random();

				foreach (var ship in ships)
                    _shipCargo.Add(new InstructionsHelper(ship));
            }

			/// <summary>
			/// Standard getter
			/// </summary>
			/// <returns></returns>
			public double getFitness()
			{
				return _fitness;
			}

            ///<summary>
            /// Allocates not yet allocated containers randomly throught all ships after crossover
            ///</summary>
            ///<param name="containers">Requires list of all containers, doesn't duplicate containers in hold</param>
            public void Repair(ContainersCollection containers)
            {
				int shipAmount = _shipCargo.Count;

                foreach (var c in containers)
                {
                    var contFound = false;

                    Container container = c;
                    foreach (var ship in _shipCargo)
                    {
                        if (!ship.IsContPresent(container.ID)) continue;

						contFound = true;
                        break;
                    }

                    if (!contFound)
                    {
						int x, y, index = _rng.Next(shipAmount);
						InstructionsHelper shipSel = _shipCargo[index];		// not sure if referencing or copied

						do
						{
							x = _rng.Next(shipSel.GetWidth());
							y = _rng.Next(shipSel.GetDepth());
						}
						while (!shipSel.CanOccupy(c, x, y));

						shipSel.Occupy(c,x,y);								// once again - check if shipSel is a reference or copy

						_shipCargo[index] = shipSel;

						/* TODO: add "no space left" check */
						throw new NotImplementedException();
                    }
                }
            }

            ///<summary>
            /// Randomly redistributes a random amount of containers through ships
            ///</summary>
            public void Mutate()
            {

                /* TODO: redistribute (delete fev containers and use repair) random amount of containers throught ships checking if can occupy */

                throw new NotImplementedException();
            }

            ///<summary>
            /// Recalculates specimen's fitness
            ///</summary>
            public void Evaluate()
            {
                var percentages = new List<double>();

                foreach (var ship in _shipCargo)
                    percentages.Add(ship.GetPercentageFilled());

                double average = percentages.Average();
                double sumOfSquaresOfDifferences = percentages.Select(val => (val - average) * (val - average)).Sum();

                this._fitness = Math.Sqrt(sumOfSquaresOfDifferences / percentages.Count);
            }
        }

        public Dictionary<int, InstructionsHelper> GenerateData(CargoShipCollection ships, ref ContainersCollection containers)
        {
            this._ships = ships;
            this._containers = containers;

			/* TODO: create main evaluation loop - check best parameters */

            throw new NotImplementedException();
        }

        public void InitialFill()			/* TODO: make code more sane */
        {
			for (int i = 0; i < SpecAmount; ++i)
				_specimen.Add(new Specimen(_ships));

			foreach(var spec in _specimen)	// theoretically InitialFill() should only be used once to 
				spec.Repair(_containers);   // initialize, but adding repair *foreach* just in case
		}

        public void Crossover()
        {
            /* TODO: give child one or more ships from each parent checking for duplicates, randomly deleting them
            then repair solutions, or come up with a better crossover strategy */

            /* TODO: come up with an auxiliary function for creating a child; use this function for replacing weaker specimen only */

            throw new NotImplementedException();
        }

        public void EvaluateSpecimens()		/* TODO: check if sorting the right way (descending order) */
		{
			foreach(var spec in _specimen)
				spec.Evaluate();

			_specimen.Sort(delegate (Specimen x, Specimen y) { return x.getFitness().CompareTo(y.getFitness()); } );
        }

        public override string ToString()
        {
            return "Genetic";
        }
    }
}