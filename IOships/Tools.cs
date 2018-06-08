using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    // Sorters

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