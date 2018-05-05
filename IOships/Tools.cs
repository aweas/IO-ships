using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IOships;

namespace Tools
{
    // Sorters
    public interface ISorter<T>
    {
        List<T> Sort(List<T> t);
    }

    public class ContainerSorter : ISorter<Container>
    {
        private PropertyInfo _property;

        private int GetValue(Container c)
        {
            return (int) _property.GetValue(c, null);
        }

        private void QuickSort(IList<Container> array, int left, int right)
        {
            var i = left;
            var j = right;
            Container pivot = array[(left + right) / 2];

            while (i < j)
            {
                while (GetValue(array[i]) < GetValue(pivot)) i++;
                while (GetValue(array[j]) > GetValue(pivot)) j--;
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

        public ContainerSorter OrderBy(string propertyName)
        {
            _property = typeof(Container).GetProperty(propertyName);
            return this;
        }


        public List<Container> Sort(List<Container> t)
        {
            List<Container> result = t;
            QuickSort(result, 0, t.Count - 1);
            return result;
        }
    }

    public class LayeredSorter<T>
    {
        private ISorter<T> _primary;
        private ISorter<T> _secondary;

        public LayeredSorter(ISorter<T> primary, ISorter<T> secondary)
        {
            _primary = primary;
            _secondary = secondary;
        }
    }

    // Generator process handling
    public static class ContainerGenerator
    {
        public static string Filename = "ContainerGenerator.exe";
        public static Process GeneratorProcess;

        public static void Generate()
        {
            if(Filename is null)
                throw new Exception("Generator .exe path is not set");

            if(File.Exists("containers.csv"))
                File.Delete("containers.csv");

            GeneratorProcess = new Process();
            GeneratorProcess.StartInfo.FileName = Filename;
            GeneratorProcess.StartInfo.UseShellExecute = false;
            GeneratorProcess.StartInfo.CreateNoWindow = true;

            GeneratorProcess.Start();

            GeneratorProcess.WaitForExit();
        }
    }
}