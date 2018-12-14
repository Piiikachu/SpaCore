using System;
using System.Collections.Generic;
using System.Text;

namespace SpaCore
{
    public class Timer
    {
        public static double getTime()
        {
            return DateTime.Now.Ticks/10e6;
        }

        public Timer(SPARTA sparta)
        {

        }
    }
}
