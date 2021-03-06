﻿using System;
using System.Linq;
using System.Threading;

namespace Utils.DataBindings.PerformanceTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var numberOfUpdates = 1000;

            TestClass left = new TestClass();
            TestClass right = new TestClass();
            right.Name = string.Empty;

            string[] values = new string[numberOfUpdates];
            for (int i = 0; i < numberOfUpdates; i++)
            {
                values[i] = RandomString(5);
            }

            using (new PerformanceMonitor("Ordinary assigment"))
            {
                for (int i = 0; i < numberOfUpdates; i++)
                {
                    left.Name = values[i];
                    right.Name = left.Name;
                }
            }

            left.Name = null;
            IBinding bind = null;

            using (new PerformanceMonitor("Create Binding"))
            {
                for (int i = 0; i < numberOfUpdates; i++)
                {
                    bind = Binding.Create(() => left.Name, () => right.Name);
                }
            }


            //Thread.Sleep(4000);

            //using (new PerformanceMonitor("Binding assigment"))
            //{
            //    for (int i = 0; i < numberOfUpdates; i++)
            //    {
            //        left.Name = values[i];
            //    }
            //}

            Console.ReadLine();
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
