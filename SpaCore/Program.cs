using System;

namespace SpaCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            SPARTA sparta = new SPARTA(args);


            sparta.sw.Flush();
            Console.ReadKey();
        }
    }
}
