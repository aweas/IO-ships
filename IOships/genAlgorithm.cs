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
		private int _specAmount = 100;
		private int _weakPerc = 60;
		private float _mutationRatePerc = 0.01F;
        private CargoShipCollection _ships;
        private ContainersCollection _containers;
		private List<Specimen> _specimens;

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
			///<returns> Returns false if ran out of free space </returns>
            public bool Repair(ContainersCollection containers) /* TODO: check all ships for space availiable and randomize from them */
			{
				int shipAmount = _shipCargo.Count;

                foreach (var c in containers)
                {
                    var contFound = false;

                    Container container = c;
                    foreach (var ship in _shipCargo)		// checking if cointainer is present on any ship
                    {
                        if (!ship.IsContPresent(container.ID)) continue;

						contFound = true;
                        break;
                    }

                    if (!contFound)							// if not present put it in the next empty space (or random place) on a random (next) ship
                    {										
						int x, y, index = _rng.Next(shipAmount); /* TODO: check all ships for space availiable and randomize only from them */
						InstructionsHelper shipSel = _shipCargo[index];
						Coords coords = shipSel.NextOccupyableCoords(container);

						if (coords.X < 0 || coords.Y < 0)   // alongside the coords used to check if theres even space available on the ship selected
							return false;

						do
						{
							x = _rng.Next(shipSel.GetWidth());
							y = _rng.Next(shipSel.GetDepth());
						}
						while (!shipSel.CanOccupy(c, x, y));

						shipSel.Occupy(c,x,y);

						_shipCargo[index] = shipSel;

                    }
				}

				return true;
			}

            ///<summary>
            /// Randomly redistributes a random amount of containers through ships
            ///</summary>														/* TODO: finish code */
            public void Mutate()												/* TODO: make code more sane - amount of allocated containers doesn't change for the whole population - can be passed as an argument*/
            {
				int currentOccupiedAmount = 0, amount;

				foreach(InstructionsHelper cargo in _shipCargo)					// counting amount of allocated containers
					currentOccupiedAmount += cargo.Instructions.Count;

				amount = _rng.Next( Math.Min(30,currentOccupiedAmount) );		// amount of containers scheduled for redistribution
                
				for( int i = 0; i < amount; ++i )								/* TODO: remove *amount* of containers from a random ship, then use repair */
				{																// selecting random ship each time
					InstructionsHelper cargo = _shipCargo[ _rng.Next(_shipCargo.Count) ];


				}
				
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

		/// <summary>
		/// Initializes specimens randomly filling each of them with all the containers,
		/// can be used to reinitialize algorithm after a purge
		/// </summary>
        public void InitialFill()			/* TODO: make code more sane */
        {
			for (int i = 0; i < _specAmount; ++i)
				_specimens.Add(new Specimen(_ships));

			foreach(var spec in _specimens)	// theoretically InitialFill() should only be used once to 
				spec.Repair(_containers);   // initialize, but adding repair *foreach* just in case
		}

		/// <summary>
		/// Combines two specimens to create a child
		/// </summary>
		/// <param name="A"></param>
		/// <param name="B"></param>
		/// <returns> Returns child specimen </returns>
		private Specimen CreateChild(Specimen A, Specimen B)
		{
			/* TODO: auxiliary function for creating a child; giving child one or more ships from each 
			parent, checking for duplicates, randomly deleting them then repair solutions, or come up 
			with a better crossover strategy */


			return new Specimen(_ships);
		}

		/// <summary>
		/// Replaces weaker specimen with new ones, handles mutation
		/// </summary>
		public void Crossover()
        {
			
			/* TODO: use ChildCreate to replace weak specimen */

			throw new NotImplementedException();
        }

		/// <summary>
		/// Evaluates all specimen, updating their fitness
		/// </summary>
        public void EvaluateSpecimens()		/* TODO: check if sorting the right way (descending order) */
		{
			foreach(var spec in _specimens)
				spec.Evaluate();

			_specimens.Sort(delegate (Specimen x, Specimen y) { return x.getFitness().CompareTo(y.getFitness()); } );
        }

        public override string ToString()
        {
            return "Genetic";
        }
    }
}