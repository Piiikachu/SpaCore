using System;
using System.Collections.Generic;
using System.Text;

namespace SpaCore
{
    public partial class Update
    {
        private SPARTA sparta;
        public Int64 ntimestep;               // current timestep
        public int nsteps;                     // # of steps to run
        public int runflag;                    // 0 for unset, 1 for run
        public Int64 firststep, laststep;      // 1st & last step of this run
        public Int64 beginstep, endstep;       // 1st and last step of multiple runs
        public int first_update;               // 0 before initial update, 1 after
        public double dt;                      // timestep size

        public string unit_style;      // style of units used throughout simulation
        public double boltz;          // Boltzmann constant (eng/degree K)
        public double mvv2e;          // conversion of mv^2 to energy

        public double fnum;           // ratio of real particles to simulation particles
        public double nrho;           // number density of background gas
        public double[] vstream;     // streaming velocity of background gas
        public double temp_thermal;   // thermal temperature of background gas
        public double[] gravity;     // acceleration vector of gravity

        public int nmigrate;          // # of particles to migrate to new procs
        public int[] mlist;            // indices of particles to migrate

        // current step counters
        public int niterate;          // iterations of move/comm
        public int ntouch_one;        // particle-cell touches
        public int ncomm_one;         // particles migrating to new procs
        public int nboundary_one;     // particles colliding with global boundary
        public int nexit_one;         // particles exiting outflow boundary
        public int nscheck_one;       // surface elements checked for collisions
        public int nscollide_one;     // particle/surface collisions

        public Int64 first_running_step; // timestep running counts start on
        public int niterate_running;      // running count of move/comm interations
        public Int64 nmove_running;      // running count of total particle moves
        public Int64 ntouch_running;     // running count of current step counters
        public Int64 ncomm_running;
        public Int64 nboundary_running;
        public Int64 nexit_running;
        public Int64 nscheck_running;
        public Int64 nscollide_running;

        public int nstuck;                // # of particles stuck on surfs and deleted

        public int reorder_period;        // # of timesteps between particle reordering

        public int copymode;          // 1 if copy of class (prevents deallocation of
        public RanMars ranmaster;   // master random number generator

        public double[] rcblo, rcbhi;    // debug info from RCB for dump image


        protected int maxmigrate;            // max # of particles in mlist
                                             //protected  RanPark random;     // RNG for particle timestep moves

        protected int collide_react;         // 1 if any SurfCollide or React classes defined
        protected int nsc, nsr;               // copy of Collide/React data in Surf class
                                              //protected  SurfCollide **sc;    
                                              //protected  SurfReact **sr;

        protected int bounce_tally;               // 1 if any bounces are ever tallied
        protected int nslist_compute;             // # of computes that tally surf bounces
        protected int nblist_compute;             // # of computes that tally boundary bounces
        protected Compute[] slist_compute;  // list of all surf bounce Computes
        protected Compute[] blist_compute;  // list of all boundary bounce Computes

        protected int nsurf_tally;         // # of Cmp tallying surf bounce info this step
        protected int nboundary_tally;     // # of Cmp tallying boundary bounce info this step
        protected Compute[] slist_active;   // list of active surf Computes this step
        protected Compute[] blist_active;   // list of active boundary Computes this step

        protected int surf_pre_tally;       // 1 to log particle stats before surf collide
        protected int boundary_pre_tally;   // 1 to log particle stats before boundary collide

        //protected int collide_react_setup();
        //protected void collide_react_update();

        //protected int bounce_setup();
        //protected void bounce_set(bigint);
        //protected void reset_timestep(bigint);


        public Update(SPARTA sparta)
        {
            vstream = new double[3];
            gravity = new double[3];
            rcblo = new double[3];
            rcbhi = new double[3];

            ntimestep = 0;
            firststep = laststep = 0;

            beginstep = endstep = 0;
            runflag = 0;

            unit_style = null;

            SetUnit("si");

            fnum = 1.0;
            nrho = 1.0;
            vstream[0] = vstream[1] = vstream[2] = 0.0;
            temp_thermal = 273.15;
            gravity[0] = gravity[1] = gravity[2] = 0.0;

            maxmigrate = 0;
            mlist = null;

            nslist_compute = nblist_compute = 0;
            slist_compute = blist_compute = null;
            slist_active = blist_active = null;

            ranmaster = new RanMars(sparta);

            reorder_period = 0;

            copymode = 0;
            this.sparta = sparta;
        }

        private void SetUnit(string str)
        {
            switch (str)
            {
                case "cgs":
                    boltz = 1.3806488e-16;
                    mvv2e = 1.0;
                    dt = 1.0;
                    break;
                case "si":
                    boltz = 1.3806488e-23;
                    mvv2e = 1.0;
                    dt = 1.0;
                    break;
                default:
                    sparta.DumpError("Illegal units command");
                    break;
            }
            unit_style = str;
        }

        public void Global(string[] arg)
        {
            int narg = arg.Length;
            if (narg < 1)
            {
                sparta.DumpError("Illegal global command");
            }
            int iarg = 0;
            while (iarg < narg)
            {
                switch (arg[iarg])
                {
                    case "fnum":
                        if (iarg + 2 > narg) sparta.DumpError("Illegal global command");
                        fnum = double.Parse(arg[iarg + 1]);
                        if (fnum <= 0.0) sparta.DumpError("Illegal global command");
                        iarg += 2;
                        break;
                    case "nrho":
                        if (iarg + 2 > narg) sparta.DumpError("Illegal global command");
                        nrho = double.Parse(arg[iarg + 1]);
                        if (nrho <= 0.0) sparta.DumpError("Illegal global command");
                        iarg += 2;
                        break;
                    case "gridcut":
                        Grid grid = sparta.grid;
                        if (iarg + 2 > narg) sparta.DumpError("Illegal global command");
                        grid.cutoff = double.Parse(arg[iarg + 1]);
                        if (grid.cutoff < 0.0 && grid.cutoff != -1.0)
                            sparta.DumpError("Illegal global command");
                        // force ghost info to be regenerated with new cutoff
                        sparta.DumpMessage("grid.remove_ghosts()");
                        iarg += 2;
                        break;
                    case "comm/sort":
                        if (iarg + 2 > narg) sparta.DumpError("Illegal global command");
                        if (arg[iarg+1].Equals("yes"))
                        {
                            sparta.DumpMessage("comm sort yes");
                        }
                        else if (arg[iarg+1].Equals("no"))
                        {
                            sparta.DumpMessage("comm sort no");
                        }
                        else sparta.DumpError("Illegal global command");
                        iarg += 2;
                        break;
                    default:
                        sparta.DumpError(string.Format("Unknown Global arguments {0}", arg[iarg]));
                        break;
                }
            }


        }



    }
}
