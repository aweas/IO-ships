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
        private CargoShipCollection _ships;
        private ContainersCollection _containers;

        struct Specimen
        {
            private double _fitness;
            private readonly List<InstructionsHelper> _shipCargo;

            ///<summary>
            /// Initialiyes specimen with its cargo holds/ships representation
            ///</summary>
            public Specimen(CargoShipCollection ships)
            {
                _fitness = 0;
                _shipCargo = new List<InstructionsHelper>();

                foreach (var ship in ships)
                {
                    _shipCargo.Add(new InstructionsHelper(ship));
                }
            }

            ///<summary>
            /// Allocates not allocated containers randomly throught all ships after crossover
            ///</summary>
            ///<param name="containers">Requires list of all containers, doesn't duplicate containers in hold</param>
            public void Repair(ContainersCollection containers)
            {
                foreach (var c in containers)
                {
                    var found = false;

                    Container container = c;
                    foreach (var ship in _shipCargo)
                    {
                        if (!ship.IsContPresent(container.ID)) continue;

                        found = true;
                        break;
                    }

                    if (!found)
                    {
                        /* TODO: randomize container location checking if can occupy, also check if there's still space anywhere */
                        throw new NotImplementedException();
                    }
                }
            }

            ///<summary>
            /// Randomly redistributes a random amount of containers through ships
            ///</summary>
            public void Mutate()
            {
                /* TODO: redistribute random amount of containers throught ships checking if can occupy */

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

            throw new NotImplementedException();
        }

        public void InitialFill()
        {
            /* TODO: use specimen.repair() to initially fill all ships */

            throw new NotImplementedException();
        }

        public void Crossover()
        {
            /* TODO: give child one or more ships from each parent checking for duplicates, randomly deleting them
            then repair solutions, or come up with a better crossover strategy */

            /* TODO: come up with an auxiliary function for creating a child; use this function for replacing weaker specimen only */

            throw new NotImplementedException();
        }

        public void EvaluateSpecimens()
        {
            /* TODO: use specimen.evaluate() on all specimen and then sort them accordingly */

            throw new NotImplementedException();
        }
    }
}