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
        public readonly int Width;
        public readonly int Depth;
        public int Size => Width * Depth;

        public readonly int ID;
        public readonly int TurnCreated;

        /// <summary>
        /// Creates container structure with given parameters
        /// </summary>
        /// <param name="width">Width of the container</param>
        /// <param name="depth">Depth of the container</param>
        /// <param name="id">Container ID</param>
        /// <param name="turnCreated">Turn in which the container arrived</param>
        public Container(int width, int depth, int id, int? turnCreated)
        {
            Width = width;
            Depth = depth;
            ID = id;

            if (turnCreated != null)
                TurnCreated = (int) turnCreated;
            else
                TurnCreated = 0;
        }
    }

    /// <summary>
    /// Provides wrapper for List of Containers so that new ships can be added seamlessly
    /// </summary>
    public class ContainersCollection : List<Container>
    {
        public void Add(int width, int depth, int id, int turn)
        {
            Add(new Container (width, depth, id, turn));
        }

        public void Add(ContainersCollection conts)
        {
            foreach (var a in conts)
                Add(a);
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

                    var id = int.Parse(aux[0]);
                    var w = int.Parse(aux[1]);
                    var d = int.Parse(aux[2]);

                    Add(w,d,id,turn);
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
        /// Adds randomly created containers. Placeholder method, it should be read from csv
        /// </summary>
        /// <param name="num">Number of containers to add</param>
        /// <param name="turn">Number of current turn</param>
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
                    Remove(c);
        }
    }
}
