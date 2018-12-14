using System;
using System.Collections.Generic;
using System.Text;

namespace SpaCore
{
    public class Modify
    {
        private SPARTA sparta;
        public int nfix, maxfix;
        public int n_start_of_step, n_end_of_step;
        public int n_pergrid, n_add_particle, n_gas_react, n_surf_react;

        public Fix fix;
        public int[] fmask;                // bit mask for when each fix is applied
         
        public int ncompute, maxcompute;   // list of computes
        public Compute[] compute;


        public Modify(SPARTA sparta)
        {
            this.sparta = sparta;

            nfix = maxfix = 0;
            n_start_of_step = n_end_of_step = 0;
            ncompute = maxcompute = 0;
        }
    }
}
