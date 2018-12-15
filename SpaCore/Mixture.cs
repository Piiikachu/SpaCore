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
        public List<int> mix2group;             // m2g[i] = group that mixture species I is in

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
        public List<double> fraction;           // relative fraction of each species
        public List<int> fraction_flag;         // 1 if user set fraction for a species
        public List<double> fraction_user;      // user fractional value

        // set by init()

        public List<double> cummulative;        // cummulative fraction for each species
        public int[] groupsize;             // # of species in each group
        public int[][] groupspecies;         // list of particle species indices in each group
        public int[] species2group;         // s2g[i] = group that particle species I is in
                                            // -1 if species I not in mixture
        public int[] species2species;       // s2s[i] = mixture species that 
                                            //   particle species I is
                                            // -1 if species I not in mixture
        public List<double> vscale;             // pre-computed velocity scale factor


        private bool all_default, species_default;

        private bool activeflag;
        private List<int> active;
        private bool copyflag;

        public Mixture(SPARTA sparta, string userid)
        {
            this.sparta = sparta;
            Particle particle = sparta.particle;
            this.userid = userid;
            vstream = new double[3];
            vstream_user = new double[3];
            id = string.Copy(userid);
            activeflag = false;
            foreach (char c in id)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    sparta.DumpMessage("Mixture ID must be alphanumeric or underscore characters");
                }
            }

            all_default = species_default = false;
            switch (id)
            {
                case "all": all_default = true; break;
                case "species": species_default = true; break;
                default:
                    break;
            }



            active = new List<int>();

            Allocate();

        }

        private void Allocate()
        {
            if (species == null)
            {
                species = new List<int>();
            }
            if (fraction == null)
            {
                fraction = new List<double>();
            }
            if (fraction_flag == null)
            {
                fraction_flag = new List<int>();
            }
            if (fraction_user == null)
            {
                fraction_user = new List<double>();
            }
            if (cummulative == null)
            {
                cummulative = new List<double>();
            }
            if (mix2group == null)
            {
                mix2group = new List<int>();
            }
            if (vscale == null)
            {
                vscale = new List<double>();
            }

        }



        public void Command(int narg, string[] arg)
        {
            if (narg < 1) sparta.DumpMessage("Illegal mixture command");

            // nsp = # of species listed before optional keywords
            // iarg = start of optional keywords

            int iarg;
            for (iarg = 1; iarg < narg; iarg++)
            {
                bool breakflag = false;
                switch (arg[iarg])
                {
                    case "nrho": breakflag = true; break;
                    case "vstream": breakflag = true; break;
                    case "temp": breakflag = true; break;
                    case "trot": breakflag = true; break;
                    case "tvib": breakflag = true; break;
                    case "frac": breakflag = true; break;
                    case "group": breakflag = true; break;
                    case "copy": breakflag = true; break;
                    case "delete": breakflag = true; break;
                    default:
                        break;
                }
                if (breakflag)
                {
                    break;
                }

            }
            int nsp = iarg - 1;

            // add_species() processes list of species
            // params() processes remaining optional keywords
            string[] args = new string[narg - iarg];
            Array.Copy(arg, iarg, args, 0, narg - iarg);
            AddSpecies(nsp, arg);
            Params(narg - iarg, args);

            // if copy keyword was used, create a new mixture via add_mixture()
            // then invoke its copy() method, passing it this mixture

            if (copyflag)
            {
                sparta.DumpMessage("copyflag");
                //particle.add_mixture(1, &arg[iarg + copyarg]);
                //particle.mixture[particle.nmixture - 1].copy(this);
            }
        }

        public void AddSpeciesDefault(string name)
        {
            Particle particle = sparta.particle;
            int index = particle.FindSpecies(name);
            //if (nspecies == maxspecies) allocate();
            species.Add(index);

            if (all_default && ngroup == 0) AddGroup("all");
            if (species_default) AddGroup(name);
            mix2group.Add(ngroup - 1);

            nspecies++;
        }

        private void AddGroup(string id)
        {
            if (groups == null)
            {
                groups = new List<string>();
            }
            groups.Add(id);
            ngroup++;
        }

        private void Params(int narg, string[] arg)
        {

            copyflag = false;
            bool deleteflag = false;
            bool fracflag = false;
            bool groupflag = false;
            double fracvalue;
            int grouparg;

            int iarg = 0;

            while (iarg < narg)
            {
                sparta.DumpMessage("complete Mixture->params");
                Console.WriteLine("arg:{0}", arg[iarg]);
                iarg++;
            }

            if (deleteflag)
            {
                sparta.DumpMessage("complete Mixture->params  deleteflag");
            }

            if (fracflag)
            {
                sparta.DumpMessage("complete Mixture->params  fracflag");
            }
            if (groupflag)
            {
                sparta.DumpMessage("complete Mixture->params  groupflag");
            }

            ShrinkGroup();
        }

        private void ShrinkGroup()
        {
            int i, nsp;

            int igroup = 0;
            while (igroup < ngroup)
            {
                sparta.DumpMessage("complete Mixture->ShrinkGroup  ");
                //nsp = 0;
                //for (i = 0; i < nspecies; i++)
                //    if (mix2group[i] == igroup) nsp++;
                //if (nsp == 0)
                //{
                //    delete[] groups[igroup];
                //    for (i = igroup; i < ngroup - 1; i++)
                //        groups[i] = groups[i + 1];
                //    for (i = 0; i < nspecies; i++)
                //        if (mix2group[i] > igroup) mix2group[i]--;
                //    ngroup--;
                //}
                //else igroup++;
            }
        }

        private void AddSpecies(int narg, string[] arg)
        {
            Particle particle = sparta.particle;
            int i, j, index;

            // activeflag = 1 if species are listed
            // active[i] = 0 if current mixture species I is not in the list
            // active[i] = 1 if species I is in the list but already existed in mixture
            // active[i] = 2 if species I was just added b/c it was in the list
            // active[i] = 3 if species is being removed from mixture via delete keyword
            //             this flag is set in params()

            if (narg != 0) activeflag = true;
            else activeflag = false;
            //todo: not necessary
            for (i = 0; i < nspecies; i++) active[i] = 0;

            for (i = 0; i < narg; i++)
            {
                index = particle.FindSpecies(arg[i]);
                if (index < 0) sparta.DumpError(string.Format("Mixture species is not defined: arg {0}",arg[i]));
                for (j = 0; j < nspecies; j++)
                    if (species[j] == index) break;
                if (j < nspecies) active[j] = 1;
                else
                {
                    if (all_default || species_default)
                        sparta.DumpMessage("Cannot add new species to mixture all or species");
                    //if (nspecies == maxspecies) Allocate();
                    active.Add(2);
                    species.Add(index);
                    nspecies++;
                }
            }
        }
    }
}