using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOships
{
    class Tools
    {
        public interface ISorter<T>
        {
            List<T> Sort(List<T> t);
        }

        public class ContainerSorter : ISorter<Container>
        {
            private static void QuickSort(IList<Container> array, int left, int right)
            {
                //int Size = (int)typeof(Container).GetProperty("Year")?.GetValue(array[0], null);

                var i = left;
                var j = right;
                Container pivot = array[(left + right) / 2];

                while (i < j)
                {
                    while (array[i].TurnCreated < pivot.Size) i++;
                    while (array[j].Size > pivot.Size) j--;
                    if (i <= j)
                    {
                        var tmp = array[i];
                        array[i++] = array[j];
                        array[j--] = tmp;
                    }
                }

                if (left < j) QuickSort(array, left, j);
                if (i < right) QuickSort(array, i, right);
            }

            public List<Container> Sort(List<Container> t)
            {
                List<Container> result = t;
                QuickSort(result, 0, t.Count - 1);
                return result;
            }
        }
    }
}