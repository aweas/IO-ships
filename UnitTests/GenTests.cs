﻿using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass()]
    public class GenTests
    {
        [TestMethod()]
        public void GenerateTest()
        {
            var generator = new ContainerGenerator.ContainerGenerator();
            generator.Generate(10);

            Assert.AreEqual(10, generator.Containers.Count);
        }

        [TestMethod()]
        public void SampleGaussianTest()
        {
            for (int i = 0; i < 1000; i++)
                ContainerGenerator.ContainerGenerator.SampleGaussian(new Random(), 5, 5);
        }

        [TestMethod()]
        public void FileCreationTest()
        {
            var generator = new ContainerGenerator.ContainerGenerator();
            generator.Generate(10, "containers");

            using (var data = new StreamReader(path: "containers.csv"))
            {
                var a = (char) data.Read();

                for (int i = 0; i < 6; i++)
                    a = (char) data.Read();

                Assert.AreEqual('t', a);
            }
        }
    }
}