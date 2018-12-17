using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SpaCore
{
    public class Particle
    {
        public bool exist;
        public bool sorted;
        public Int64 nglobal;
        public int nlocal;
        public int maxlocal;


        public class Species
        {                                       // info on each particle species, read from file
            public string id;            // species ID
            public double molwt;           // molecular weight
            public double mass;            // molecular mass
            public double specwt;          // species weight
            public double charge;          // multiple of electron charge
            public double rotrel;          // inverse rotational relaxation number
            public double[] rottemp;      // rotational temperature(s)
            public double[] vibtemp;      // vibrational tempearture(s)
            public double[] vibrel;       // inverse vibrational relaxation number(s)
            public int[] vibdegen;        // vibrational mode degeneracies
            public int rotdof, vibdof;      // rotational/vibrational DOF
            public int nrottemp, nvibmode;  // # of rotational/vibrational temps/modes defined
            public int internaldof;        // 1 if either rotdof or vibdof != 0
            public int vibdiscrete_read;   // 1 if species.vib file read for this species
            public Species()
            {
                rottemp = new double[3];
                vibtemp = new double[4];
                vibrel = new double[4];
                vibdegen = new int[4];
            }
        }


        public class RotFile
        {
            public char[] id;
            public double[] rottemp;
            public int ntemp;
            public RotFile()
            {
                id = new char[16];
                rottemp = new double[4];
            }
        }

        public class VibFile
        {
            char[] id;
            double[] vibrel;
            double[] vibtemp;
            int[] vibdegen;
            int nmode;
            public VibFile()
            {
                id = new char[16];
                vibrel = new double[4];
                vibtemp = new double[4];
                vibdegen = new int[4];
            }
        }

        public List<Species> species;
        public int nspecies;
        public int maxvibmode;

        public List<Mixture> mixture;
        public int nmixture;

        public class OnePart
        {
            public int id;                 // particle ID
            public int ispecies;           // particle species index
            public int icell;              // which local Grid::cells the particle is in
            public double[] x;            // particle position
            public double[] v;            // particle velocity
            public double erot;            // rotational energy
            public double evib;            // vibrational energy
            public int flag;               // used for migration status
            public double dtremain;        // portion of move timestep remaining
            public double weight;          // particle or cell weight, if weighting enabled
            public OnePart()
            {
                x = new double[3];
                v = new double[3];
            }
        }

        public void AddMixture(string[] arg)
        {
            int narg = arg.Length;
            if (narg < 1)
            {
                sparta.DumpError("Illegal mixture command");
            }
            // imix = index if mixture ID already exists
            // else instantiate a new mixture

            int imix = FindMixture(arg[0]);
            if (imix < 0)
            {
                imix = nmixture;
                nmixture++;
                mixture.Add(new Mixture(sparta, arg[0]));
            }
            mixture[imix].Command(arg.Length, arg);
        }

        public void AddSpecies(string[] arg)
        {
            int narg = arg.Length;
            int i, j, n;
            if (narg < 2)
            {
                sparta.DumpError("Illegal species command");
            }
            // nfilespecies = # of species defined in file
            // filespecies = list of species defined in file

            nfile = maxfile = 0;
            try
            {
                fp = new FileStream(arg[0], FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (FileNotFoundException e)
            {
                throw e;
            }

            ReadSpeciesFile();
            int newspecies = 0;
            for (int iarg = 1; iarg < narg; iarg++)
            {
                if (arg[iarg].Equals("air"))
                {
                    sparta.DumpError("Particle->AddSpecies: complete air");

                }
                else
                {
                    newspecies++;
                }
            }

            string[] names = new string[newspecies];
            newspecies = 0;
            for (int iarg = 1; iarg < narg; iarg++)
            {
                if (arg[iarg].Equals("air"))
                {
                    sparta.DumpError("Particle->AddSpecies: complete air");
                }
                else
                {
                    names[newspecies++] = arg[iarg];
                }
            }

            GrowSpecies();

            int imix_all = FindMixture("all");
            int imix_species = FindMixture("species");

            for (i = 0; i < newspecies; i++)
            {
                for (j = 0; j < nspecies; j++)
                    if (names[i].Equals(species[j].id)) break;
                if (j < nspecies) sparta.DumpError("Species ID is already defined");
                for (j = 0; j < nfile; j++)
                    if (names[i].Equals(filespecies[j].id)) break;
                if (j == nfile)
                    sparta.DumpError("Species ID does not appear in species file");
                //memcpy(&species[nspecies], &filespecies[j], sizeof(Species));
                species.Add(filespecies[j]);
                nspecies++;

                mixture[imix_all].AddSpeciesDefault(species[nspecies - 1].id);
                mixture[imix_species].AddSpeciesDefault(species[nspecies - 1].id);
            }




        }

        private void GrowSpecies()
        {
            if (species == null)
            {
                species = new List<Species>();
            }
        }

        private void ReadSpeciesFile()
        {
            filespecies = new List<Species>();
            using (StreamReader sr = new StreamReader(fp))
            {
                string str;
                string[] words;
                while ((str = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        continue;
                    }
                    if (str.StartsWith("#"))
                    {
                        continue;
                    }
                    words = Regex.Split(str, @"\s+");
                    if (words.Length != 10)
                    {
                        sparta.DumpError("Incorrect line format in species file");
                    }
                    Species sp = new Species();
                    sp.id = words[0];

                    sp.molwt = double.Parse(words[1]);
                    sp.mass = double.Parse(words[2]);
                    sp.rotdof = int.Parse(words[3]);
                    sp.rotrel = double.Parse(words[4]);
                    sp.vibdof = int.Parse(words[5]);
                    sp.vibrel[0] = double.Parse(words[6]);
                    sp.vibtemp[0] = double.Parse(words[7]);
                    sp.specwt = double.Parse(words[8]);
                    sp.charge = double.Parse(words[9]);

                    if (sp.rotdof > 0 || sp.vibdof > 0)
                    {
                        sp.internaldof = 1;
                    }
                    else
                    {
                        sp.internaldof = 0;
                    }
                    // error checks
                    // NOTE: allow rotdof = 3 when implement rotate = DISCRETE

                    if (sp.rotdof != 0 && sp.rotdof != 2)
                        sparta.DumpError("Invalid rotational DOF in species file");
                    //if (sp.rotdof != 0 && sp.rotdof != 2 && sp.rotdof != 3)
                    //  error->all(FLERR,"Invalid rotational DOF in species file");

                    if (sp.vibdof < 0 || sp.vibdof > 8 || sp.vibdof % 2 != 0)
                        sparta.DumpError("Invalid vibrational DOF in species file");

                    // initialize additional rotation/vibration fields
                    // may be overwritten by rotfile or vibfile

                    sp.nrottemp = 0;
                    sp.nvibmode = sp.vibdof / 2;

                    sp.rottemp[0] = sp.rottemp[1] = sp.rottemp[2] = 0.0;
                    sp.vibtemp[1] = sp.vibtemp[2] = sp.vibtemp[3] = 0.0;
                    sp.vibrel[1] = sp.vibrel[2] = sp.vibrel[3] = 0.0;
                    sp.vibdegen[0] = sp.vibdegen[1] = sp.vibdegen[2] = sp.vibdegen[3] = 0;

                    sp.vibdiscrete_read = 0;

                    filespecies.Add(sp);
                    nfile++;


                }
            }
        }

        public class OnePartRestart
        {
            int id;                 // particle ID
            int ispecies;           // particle species index
            int icell;          // cell ID the particle is in
            int nsplit;             // 1 for unsplit cell
                                    // else neg of sub cell index (0 to Nsplit-1)
            double[] x;            // particle position
            double[] v;            // particle velocity
            double erot;            // rotational energy
            double evib;            // vibrational energy
            public OnePartRestart()
            {
                x = new double[3];
                v = new double[3];
            }
        }

        public void Sort()
        {
            sorted = true;

            if (maxsort < maxlocal)
            {

            }
            throw new NotImplementedException();
        }

        public OnePart particles;

        int[] next;                // index of next particle in each grid cell


        protected int maxgrid;              // max # of indices first can hold
        protected int maxsort;              // max # of particles next can hold
        protected int maxspecies;           // max size of species list

        protected FileStream fp;                 // file pointer for species, rotation, vibration
        protected int nfile;         // # of species read from file
        protected int maxfile;       // max size of filespecies list

        protected List<Species> filespecies;     // list of species read from file
        protected List<RotFile> filerot;         // list of species rotation info read from file
        protected List<VibFile> filevib;         // list of species vibration info read from file

        protected RanPark wrandom;   // RNG for particle weighting

        // extra custom vectors/arrays for per-particle data
        // ncustom > 0 if there are any extra arrays
        // these varaiables are private, others above are public

        protected string[] ename;             // name of each attribute

        protected int ncustom_ivec;         // # of integer vector attributes
        protected int ncustom_iarray;       // # of integer array attributes
        protected int[] icustom_ivec;        // index into ncustom for each integer vector
        protected int[] icustom_iarray;      // index into ncustom for each integer array
        protected int[] eicol;               // # of columns in each integer array (esize)

        protected int ncustom_dvec;         // # of double vector attributes
        protected int ncustom_darray;       // # of double array attributes
        protected int[] icustom_dvec;        // index into ncustom for each double vector
        protected int[] icustom_darray;      // index into ncustom for each double array
        protected int[] edcol;               // # of columns in each double array (esize)

        protected int[] custom_restart_flag; // flag on each custom vec/array read from restart
                                             // used to delete them if not redefined in 
                                             // restart script


        private SPARTA sparta;

        public Particle(SPARTA sparta)
        {
            this.sparta = sparta;
            exist = sorted = false;

            particles = null;

            nspecies = maxspecies = 0;
            species = null;
            maxvibmode = 0;

            //maxgrid = 0;
            //cellcount = null;
            //first = null;
            maxsort = 0;
            next = null;

            // create two default mixtures


            mixture = new List<Mixture>();
            nmixture = 0;

            string[] newarg = new string[1];
            newarg[0] = "all";
            AddMixture(1, newarg);
            newarg[0] = "species";
            AddMixture(1, newarg);

            // custom per-particle vectors/arrays

            //ncustom = 0;
            ename = null;
            //etype = esize = ewhich = null;

            ncustom_ivec = ncustom_iarray = 0;
            icustom_ivec = icustom_iarray = null;
            //eivec = null;
            //eiarray = null;
            eicol = null;

            ncustom_dvec = ncustom_darray = 0;
            icustom_dvec = icustom_darray = null;
            //edvec = null;
            //edarray = null;
            edcol = null;

            custom_restart_flag = null;

            // RNG for particle weighting

            wrandom = null;

            //copy = copymode = 0;
        }

        public int FindSpecies(string id)
        {
            for (int i = 0; i < nspecies; i++)
            {
                if (id.Equals(species[i].id))
                {
                    return i;
                }

            }
            return -1;
        }

        private void AddMixture(int narg, string[] arg)
        {
            if (narg < 1) sparta.DumpMessage("Illegal mixture command");

            // imix = index if mixture ID already exists
            // else instantiate a new mixture

            int imix = FindMixture(arg[0]);

            if (imix < 0)
            {

                imix = nmixture;
                nmixture++;
                mixture.Add(new Mixture(sparta, arg[0]));
            }

            mixture[imix].Command(narg, arg);
        }

        private int FindMixture(string id)
        {


            for (int i = 0; i < nmixture; i++)
                if (string.Equals(id, mixture[i].id)) return i;
            return -1;
        }
    }
}
