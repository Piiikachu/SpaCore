using System;

namespace SpaCore
{
    public class Stats
    {
        private SPARTA sparta;

        private double wall0;

        private int nfield;
        public Stats(SPARTA sparta)
        {
            this.sparta = sparta;
            wall0 = Timer.getTime();

            string[] arg = new string[] { "step","cpu","np"};
            nfield = 3;
            //todo :continue

        }
    }
}