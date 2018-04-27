using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOships
{
    /// <summary>
    /// Basic structure that will represent every single one of our containers
    /// </summary>
    public struct Container
    {
        public int width;
        public int depth;

        public int ID;
        public int turnCreated;
        public int? turnDeleted;

        /// <summary>
        /// Creates container structure with given parameters
        /// </summary>
        /// <param name="width">Width of the container</param>
        /// <param name="depth">Depth of the container</param>
        /// <param name="id">Container ID</param>
        /// <param name="turnCreated">Turn in which the container arrived</param>
        public Container(int width, int depth, int id, int turnCreated)
        {
            this.width = width;
            this.depth = depth;
            this.ID = id;
            this.turnCreated = turnCreated;
            this.turnDeleted = null;
        }
    }

    /// <summary>
    /// Provides wrapper for List<Container> so that new ships can be added seamlessly
    /// </summary>
    public class ContainersCollection : List<Container>
    {
        public void Add(int width, int depth, int id, int turn)
        {
            Add(new Container (width, depth, id, turn));
        }

        public void Add(ContainersCollection conts)
        {
            foreach (Container a in conts)
                Add(a);
        }

        /// <summary>
        /// Calculates statistics for containers in hold
        /// </summary>
        /// <param name="currentTurn">Current turn in simulation</param>
        /// <returns>Dictionary of containers' age and count</returns>
        public Dictionary<int, int> getAgeAndCount(int currentTurn)
        {
            Dictionary<int, int> ages = new Dictionary<int, int>();

            foreach (Container a in this)
            {
                int key = currentTurn - a.turnCreated;

                if (!ages.ContainsKey(key))
                    ages.Add(key, 1);
                else
                    ages[key]++;
            }

            return ages;
        }

        public override string ToString()
        {
            string res = "";
            foreach (Container i in this)
                res += i.ID.ToString()+"; ";

            return res;
        }

        //TODO: Add function that reads input from CSV
        /// <summary>
        /// Adds randomly created containers. Placeholder method, it should be read from csv
        /// </summary>
        /// <param name="num">Number of containers to add</param>
        public void AddRandom(int num, int turn)
        {
            Random random = new Random();
            for (int i = 0; i < num; i++)
                Add(random.Next(1, 10), random.Next(1, 10), random.Next(), turn);
        }

        /// <summary>
        /// Remove container with given ID
        /// </summary>
        /// <param name="ID">ID of container to remove</param>
        public void Remove(int ID)
        {
            foreach(Container c in this)
                if (c.ID == ID)
                    this.Remove(c);
        }
    }
}
