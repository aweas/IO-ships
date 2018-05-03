using System;
using System.Collections.Generic;
using System.Linq;

namespace IOships
{
    /// <summary>
    /// Genetic algorithm's logic resides here
    /// </summary>
    class genAlgorithm : ICollectionwiseStrategy
    {
        CargoShipCollection ships;
        ContainersCollection containers;

        struct specimen
        {
            public double fitness;
            public List<InstructionsHelper> shipCargo;

            ///<summary>
            /// Initialiyes specimen with its cargo holds/ships representation
            ///<summary>
            public specimen(CargoShipCollection ships)
            {
                fitness = 0;
                shipCargo = new List<InstructionsHelper>();

                for(Int32 i=0;i<ships.Count;++i)
                {
                    shipCargo.Add(new InstructionsHelper(ships[i]));
                }
            }

            ///<summary>
            /// Allocates not allocated containers randomly throught all ships after crossover
            ///<summary>
            ///<param name="containers">Requires list of all containers, doesn't duplicate containers in hold</param>
            public void repair(ContainersCollection containers)
            {
                bool found;

                for(Int32 i=0;i<containers.Count;++i)
                {
                    found = false;

                    Container container = containers[i];
                    for (Int32 j = 0; j < shipCargo.Count; ++j)         // check if container is present on any ship
                        if (shipCargo[j].isContPresent(container.ID))
                        {
                            found = true;
                            break;
                        }

                    if (!found)
                    {
                        /* TODO: randomize container location checking if can occupy, also check if there's still space anywhere */
                    }

                }
            }

            ///<summary>
            /// Randomly redistributes a random amount of containers through ships
            ///<summary>
            public void mutate()
            {
                /* TODO: redistribute random amount of containers throught ships checking if can occupy */
            }

            ///<summary>
            /// Recalculates specimen's fitness
            ///<summary>
            public void evaluate()
            {
                double average,sumOfSquaresOfDifferences;
                List<double> percentages = new List<double>();

                for(Int32 i=0;i<shipCargo.Count;++i)
                    percentages.Add(shipCargo[i].GetPercentageFilled());

                average = percentages.Average();
                sumOfSquaresOfDifferences = percentages.Select(val => (val - average) * (val - average)).Sum();
                this.fitness = Math.Sqrt(sumOfSquaresOfDifferences / percentages.Count); 
            }
        }

        public Dictionary<int, InstructionsHelper> GenerateData(CargoShipCollection ships, ContainersCollection containers)
        {
            this.ships = ships;
            this.containers = containers;

            return null;
        }

        public void initialFill()
        {
            /* TODO: use specimen.repair() to initially fill all ships */
        }

        public void crossover()
        {
            /* TODO: give child one or more ships from each parent checking for duplicates, randomly deleting them
            then repair solutions, or come up with a better crossover strategy */

            /* TODO: come up with an auxiliary function for creating a child; use this function for replacing weaker specimen only */
        }

        public void evaluateSpecimens()
        {
            /* TODO: use specimen.evaluate() on all specimen and then sort them accordingly */
        }

    }
}
