using System;
using System.Threading;

namespace SpaCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            SPARTA sparta = new SPARTA(args);
            sparta.input.File();

            sparta.sw.Flush();
            Console.ReadKey();
        }
    }
}
