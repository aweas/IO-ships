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
        public int height;
        public int depth;

        public int id;
        public int turnCreated;
        public int? turnDeleted;

        public Container(int width, int height, int depth, int id, int turnCreated)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;
            this.id = id;
            this.turnCreated = turnCreated;
            this.turnDeleted = null;
        }
    }

    /// <summary>
    /// Provides wrapper for List<Container> so that new ships can be added seamlessly
    /// </summary>
    public class Containers : List<Container>
    {
        public void Add(int width, int height, int depth, int id)
        {
            Add(new Container (width, height, depth, id, 2));
        }

        public void Add(Containers conts)
        {
            foreach (Container a in conts)
                Add(a);
        }

        public Dictionary<int, int> getAgeAndCount(int currentTurn)
        {
            Dictionary<int, int> ages = new Dictionary<int, int>();

            foreach (Container a in this)
            {
                if (!ages.ContainsKey(a.turnCreated))
                    ages.Add(currentTurn - a.turnCreated, 1);
                else
                    ages[currentTurn - a.turnCreated]++;
            }

            return ages;
        }
    }
}
