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
		private float _mutationRatePerc = 1;
		private int cycles = 10;
        private CargoShipCollection _ships;
        private ContainersCollection _containers;
		private List<Specimen> _specimens;
		private Random _rng;

		/// <summary>
		/// Struct representing a solution, holds set of instructions 
		/// for container placement and fitness of such solution
		/// </summary>
		struct Specimen
        {
			private double _fitness;
			private Random _rng;
            public List<InstructionsHelper> _shipCargo;         /* TODO, optional: <- create a manipulator method and make private again */

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
            public bool Repair(ContainersCollection containers) /* TODO, optional: check all ships for space availiable and randomize from them */
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
						int x, y, index = _rng.Next(shipAmount); /* TODO, optional: check all ships for space availiable and randomize only from them */
						InstructionsHelper shipSel = _shipCargo[index];
						Coords coords = shipSel.NextOccupyableCoords(container);

						if (coords.X < 0 || coords.Y < 0)   // alongside the coords used to check if theres even space available on the ship selected
							return false;

						do									/* TODO: change this super inefficent way of selecting a free space for a container */
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
            public void Mutate()                                /* TODO, optional: make code more sane - amount of allocated containers doesn't change for the whole population - can be passed as an argument*/
			{
				int currentOccupiedAmount = 0, amount;

				foreach(InstructionsHelper cargo in _shipCargo)					// counting amount of allocated containers
					currentOccupiedAmount += cargo.Instructions.Count;

				amount = _rng.Next( Math.Min(30,currentOccupiedAmount) );		// amount of containers scheduled for redistribution
                
				for( int i = 0; i < amount; ++i )								
				{                                                               // selecting random ship each time
		
					/* TODO: remove *amount* of containers from a random ship, then use repair */

		
				}
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
			Dictionary<int, InstructionsHelper> Solution = new Dictionary<int, InstructionsHelper>();

			InitialFill();

			for (var i = 0; i < cycles; ++i)
			{
				EvaluateSpecimens();
				Crossover();
			}

			Specimen bestSpec = _specimens[0];
			for(var i = 0; i < _ships.Count; ++i)
				Solution.Add(i, bestSpec._shipCargo[i]);

			return Solution;
        }

		public GenAlgorithm()
		{
			_rng = new Random();
			_specimens = new List<Specimen>();

			for (int i = 0; i < _specAmount; ++i)
				_specimens.Add(new Specimen(_ships));
		}

		/// <summary>
		/// Initializes specimens randomly filling each of them with all the containers,
		/// can be used to reinitialize algorithm after a purge
		/// </summary>
        private void InitialFill()                              /* TODO, optional: make code more sane */
		{
			for (int i = 0; i < _specAmount; ++i)
				_specimens[i] = new Specimen(_ships);

			foreach(var spec in _specimens)	// theoretically InitialFill() should only be used once to 
				spec.Repair(_containers);   // initialize, but adding repair *foreach* just in case
		}

		/// <summary>
		/// Combines two specimens to create a child
		/// </summary>
		/// <param name="A"></param>
		/// <param name="B"></param>
		/// <returns> Returns child specimen </returns>
		private Specimen CreateChild(Specimen A, Specimen B)    /* TODO, optional: create a less lazy implementation - actually use the second specimen in child creation */
		{
			int shAmount = _rng.Next(_ships.Count);
			Specimen C = new Specimen(_ships);

			for(var i = 0; i < shAmount; ++i)
				C._shipCargo[i] = A._shipCargo[i];

			C.Repair(_containers);

			return C;
		}

		/// <summary>
		/// Replaces weaker specimen with new ones, handles mutation
		/// </summary>
		private void Crossover()
        {
			for(var i = _weakPerc; i < _specAmount; ++i)
			{
				Specimen A, B;

				A = _specimens[ _rng.Next(_weakPerc) ];		// selects both parents from best suited specimens
				B = _specimens[ _rng.Next(_weakPerc) ];

				_specimens[i] = CreateChild(A,B);

				if (_rng.Next(100) < _mutationRatePerc)		// mutationg if rng Gods allow it
					_specimens[i].Mutate();
			}	
		}

		/// <summary>
		/// Evaluates all specimen, updating their fitness
		/// </summary>
        private void EvaluateSpecimens()						/* TODO: check if sorting the right way (descending order) */
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