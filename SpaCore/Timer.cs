using System;
using System.Collections.Generic;
using System.Text;

namespace SpaCore
{
    public class Timer
    {
        private SPARTA sparta;
        public double[] array;

        public static double getTime()
        {
            return DateTime.Now.Ticks/10e6;
        }
        public enum TimeEnum
        {
            TIME_LOOP, TIME_MOVE, TIME_COLLIDE, TIME_SORT, TIME_COMM, TIME_MODIFY, TIME_OUTPUT, TIME_N
        }

        public Timer(SPARTA sparta)
        {
            this.sparta = sparta;
            array = new double[(int)TimeEnum.TIME_N];
        }
    }
}
