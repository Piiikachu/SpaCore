using System;
using System.Collections.Generic;
using System.Text;

namespace SpaCore
{
    public class Output
    {
        private SPARTA sparta;

        public Stats stats;


        public Output(SPARTA sparta)
        {
            this.sparta = sparta;
            stats = new Stats(sparta);
        }
    }
}
