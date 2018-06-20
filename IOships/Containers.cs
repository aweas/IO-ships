using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace IOships
{
    /// <summary>
    /// Basic structure that will represent every single one of our containers
    /// </summary>
    public struct Container
    {
        public ulong Timestamp { get; }
        public readonly Guid ID;
        public int Width { get; }
        public int Depth { get; }
        public int Size => Width * Depth;

        /// <summary>
        /// Simple auxiliary int to guid converter - move to tools for organisation sake
        /// </summary>
        /// <param name="value">Int value for conversion</param>
        /// <returns>Returns guid</returns>
        public static Guid ToGuid(int value)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        /// <summary>
        /// Creates container structure with given parameters
        /// </summary>
        /// <param name="timestamp">Scheduled time of loading</param>
        /// <param name="id">Container's unique id</param>
        /// <param name="width">Width of the container</param>
        /// <param name="depth">Depth of the container</param>
        public Container(ulong timestamp, int id, int width, int depth)
        {
            Timestamp = timestamp;
            Width = width;
            Depth = depth;
            ID = ToGuid(id);
        }
    }

    /// <summary>
    /// Provides wrapper for List of Containers so that new ships can be added seamlessly
    /// </summary>
    public class ContainersCollection : List<Container>
    {
        private void Add(ulong timestamp, int id, int width, int depth)
        {
            Add(new Container(timestamp, id, width, depth));
        }

        /* TODO: check for duplicates (nah - *hope* for no duplicates), checking whole list every time we add container is a waste of cycles */
        /// <summary>
        /// Loads containers from .csv files
        /// </summary>
        /// <param name="filename">Path to .csv file</param>
        public void LoadCsv(string filename)
        {
            using (var data = new StreamReader(filename))
            {
                string line;
                ulong lastTimestamp = 0;
                var i = 0;

                while ((line = data.ReadLine()) != null)
                {
                    line = line.Replace("[", "").Replace("]", "");

                    if (!Regex.IsMatch(line, @"^(\d+,){4}\d+$"))                            // would not hardcode ',' as a separator if not for the regex   
                        throw new FormatException("Improper format, line: " + i);           /* TODO: add all sorts of error messages everywhere */

                    List<string> aux = line.Split(',').ToList();

                    var ts = ulong.Parse(aux[0]);
                    var id = int.Parse(aux[1]);
                    var w = int.Parse(aux[2]);
                    var d = int.Parse(aux[3]);

                    if (w < 1 && w > 10 && d < 1 && d > 10)
                        throw new FormatException("Number(s) not in range, line: " + i);     /* TODO: same as above */

                    if (ts < lastTimestamp)
                        throw new FormatException("Incorrect load date, line: " + i);         /* TODO: again - errors, warnings etc. - you get the idea */

                    lastTimestamp = ts;
                    Add(ts, id, w, d);
                    ++i;
                }
            }
        }

        /// <summary>
        /// Calculates statistics for containers in hold
        /// </summary>
        /// <param name="currentTurn">Current turn in simulation</param>
        /// <returns>Dictionary of containers' age and count</returns>
        /*public Dictionary<int, int> GetAgeAndCount(int currentTurn)   // TODO: delete or modify to fit the new implementation
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
        }*/

        public override string ToString()
        {
            var res = "";
            foreach (Container i in this)
                res += i.ID + "; ";

            return res;
        }

        /// <summary>
        /// Remove container with given ID
        /// </summary>
        /// <param name="ID">ID of container to remove</param>
        public void Remove(Guid ID)
        {
            foreach (Container c in this)
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