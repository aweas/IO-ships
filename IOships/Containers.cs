using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IOships
{
    /// <summary>
    /// Basic structure that will represent every single one of our containers
    /// </summary>
    public struct Container
    {
        public int Width { get; }
        public int Depth { get; }
        public int Size => Width * Depth;

        public readonly Guid ID;
        public readonly int TurnCreated;

        /// <summary>
        /// Creates container structure with given parameters
        /// </summary>
        /// <param name="width">Width of the container</param>
        /// <param name="depth">Depth of the container</param>
        /// <param name="turnCreated">Turn in which the container arrived</param>
        /// 
        public Container(int width, int depth, int? turnCreated)
        {
            Width = width;
            Depth = depth;
            ID = Guid.NewGuid();

            if (turnCreated != null)
                TurnCreated = (int) turnCreated;
            else
                TurnCreated = 0;
        }

        public Container(int width, int depth, int turnCreated, Guid id)
        {
            ID = id;
            TurnCreated = turnCreated;
            Width = width;
            Depth = depth;
        }
    }

    /// <summary>
    /// Provides wrapper for List of Containers so that new ships can be added seamlessly
    /// </summary>
    public class ContainersCollection : List<Container>
    {
        private void Add(int width, int depth, int turn, Guid id)
        {
            Add(new Container(width, depth, turn, id));
        }

        /* TODO: check for duplicates, add turns to csv files (?) */
        /// <summary>
        /// Loads containers from .csv files
        /// </summary>
        /// <param name="filename">Path to .csv file</param>
        /// <param name="turn">Specifies load turn for loaded containers</param>    
        public void LoadCsv(string filename, int turn)
        {
            using (var data = new StreamReader(filename))
            {
                data.ReadLine();
                string line;
                while( (line = data.ReadLine()) != null)
                {
                    List<string> aux = line.Split(';').ToList();

                    var id = Guid.Parse(aux[0]);
                    var w = int.Parse(aux[1]);
                    var d = int.Parse(aux[2]);

                    Add(w, d, turn, id);
                }
            }
        }

        /// <summary>
        /// Calculates statistics for containers in hold
        /// </summary>
        /// <param name="currentTurn">Current turn in simulation</param>
        /// <returns>Dictionary of containers' age and count</returns>
        public Dictionary<int, int> GetAgeAndCount(int currentTurn)
        {
            Dictionary<int, int> ages = new Dictionary<int, int>();

            foreach (Container a in this)
            {
                int key = currentTurn - a.TurnCreated;

                if (!ages.ContainsKey(key))
                    ages.Add(key, 1);
                else
                    ages[key]++;
            }

            return ages;
        }

        public override string ToString()
        {
            var res = "";
            foreach (Container i in this)
                res += i.ID+"; ";

            return res;
        }

        /// <summary>
        /// Remove container with given ID
        /// </summary>
        /// <param name="ID">ID of container to remove</param>
        public void Remove(Guid ID)
        {
            foreach(Container c in this)
                if (c.ID == ID)
                    Remove(c);
        }

        public void RecreateFromList(List<Container> listToRead)
        {
            Clear();
            foreach (var c in listToRead)
                Add(c);
        }
    }
}
