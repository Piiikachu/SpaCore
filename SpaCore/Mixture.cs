using System;
using System.Collections.Generic;

namespace SpaCore
{
    public class Mixture
    {
        private SPARTA sparta;
        private string userid;

        public string id;                   // ID of mixture
        public int nspecies;               // # of species in mixture
        public List<int> species;               // species[i] = particle species index of 
                                     //              mixture species I
         
        public int ngroup;                 // # of defined groups
        public List<string> groups;              // group IDs
        public int[] mix2group;             // m2g[i] = group that mixture species I is in
         
         // global attributes
        public double nrho;                // number density
        public int nrho_flag;              // 1 if user set nrho
        public double nrho_user;           // user value
        public double[] vstream;          // stream velocity
        public int vstream_flag;           // 1 if user set vstream
        public double[] vstream_user;     // user value
        public double temp_thermal;        // thermal temperature
        public double temp_rot;            // rotational temperature
        public double temp_vib;            // vibrational temperature
        public int temp_thermal_flag;      // 1 if user set thermal temp
        public int temp_rot_flag;          // 1 if user set rotational temp
        public int temp_vib_flag;          // 1 if user set vibrational temp
        public double temp_thermal_user;   // user value
        public double temp_rot_user;       // user value
        public double temp_vib_user;       // user value
         
         // per-species attributes
        public double[] fraction;           // relative fraction of each species
        public int[] fraction_flag;         // 1 if user set fraction for a species
        public double[] fraction_user;      // user fractional value
         
         // set by init()
         
        public double[] cummulative;        // cummulative fraction for each species
        public int[] groupsize;             // # of species in each group
        public int[][] groupspecies;         // list of particle species indices in each group
        public int[] species2group;         // s2g[i] = group that particle species I is in
                                     // -1 if species I not in mixture
        public int[] species2species;       // s2s[i] = mixture species that 
                                     //   particle species I is
                                     // -1 if species I not in mixture
        public double[] vscale;             // pre-computed velocity scale factor


        private int all_default , species_default;

        public Mixture(SPARTA sparta, string userid)
        {
            this.sparta = sparta;
            this.userid = userid;
            vstream = new double[3];
            vstream_user = new double[3];
            id = string.Copy(userid);
            foreach (char c in id)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    sparta.DumpMessage("Mixture ID must be alphanumeric or underscore characters");
                }
            }

            all_default = species_default = 0;
            switch (id)
            {
                case "all":all_default=1;break;
                case "species":species_default = 1;break;
                default:
                    break;
            }
            Allocate();

        }

        private void Allocate()
        {
            species=
            throw new NotImplementedException();
        }

        internal void Command(int narg, string[] arg)
        {
            throw new NotImplementedException();
        }
    }
}