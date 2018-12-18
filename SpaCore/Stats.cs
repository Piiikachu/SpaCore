using System;

namespace SpaCore
{
    public class Stats
    {
        private SPARTA sparta;

        private string[] keyword;
        private int firststep;
        private bool flushflag, lineflag;

        private double wall0;
        private double last_tpcpu, last_spcpu;
        private double last_time;
        private Int64 last_step;

        // data used by routines that compute single values
        private int ivalue;            // integer value to print
        private double dvalue;         // double value to print
        private Int64 bivalue;        // big integer value to print
        private int ifield;            // which field in thermo output is being computed
        private int[] field2index;      // which compute,fix,variable calcs this field
        private int[] argindex1;        // indices into compute,fix scalar,vector
        private int[] argindex2;

        private int ncompute;                // # of Compute objects called by stats
        private string[] id_compute;           // their IDs

        private enum TypeFlag
        {
            INT, FLOAT, BIGINT
        }

        private enum ComputeWhich
        {
            SCALAR, VECTOR, ARRAY
        }
        private ComputeWhich[] computewhich;          // 0/1/2 if should call scalar,vector,array
        Compute[] computes;    // list of ptrs to the Compute objects

        int nfix;                    // # of Fix objects called by stats
        string[] id_fix;               // their IDs
        Fix[] fixes;           // list of ptrs to the Fix objects

        int nsurfcollide;            // # of SurfCollide objs called by stats
        string[] id_surf_collide;      // their IDs
        SurfCollide[] sc;      // list of ptrs to SurfCollide objects

        int nsurfreact;             // # of SurfReact objects called by stats
        string[] id_surf_react;       // their IDs
        SurfReact[] sr;       // list of ptrs to SurfReact objects

        int nvariable;               // # of variables evaulated by stats
        string[] id_variable;          // list of variable names
        int[] variables;              // list of Variable indices
        private delegate void Fnptr();

        private Fnptr[] vfunc;
        private TypeFlag[] vtype;

        private int nfield;
        public Stats(SPARTA sparta)
        {
            this.sparta = sparta;
            wall0 = Timer.getTime();

            string[] arg = new string[] { "step", "cpu", "np" };
            nfield = 3;

            Allocate();
            SetFields(3, arg);

            // stats_modify defaults

            flushflag = false;

            // format strings

            //string Int64_format = (string)Int64_FORMAT;

            //format_float_def = (string)"%12.8g";
            //format_int_def = (string)"%8d";
            //sprintf(format_Int64_def, "%%8%s", &Int64_format[1]);


        }

        private void SetFields(int narg, string[] arg)
        {
            Deallocate();
            // expand args if any have wildcard character "*"

            bool expand = false;
            string[] earg;
            int nargnew = sparta.input.ExpandArgs(narg, arg, 0, out earg);

            if (earg != arg) expand = true;
            arg = earg;

            nfield = nargnew;
            Allocate();
            nfield = 0;

            // customize a new keyword by adding to if statement


            for (int i = 0; i < nargnew; i++)
            {
                switch (arg[i])
                {
                    case "step": AddField("Step", ComputeStep, TypeFlag.INT); break;
                    case "cpu": AddField("CPU", ComputeCPU, TypeFlag.FLOAT); break;
                    case "np": AddField("Np", ComputeNp, TypeFlag.FLOAT); break;
                    default:
                        Console.WriteLine("complete stats->setfields,{0}", arg[i]);
                        break;
                }
            }

        }

        private void ComputeNp()
        {
            throw new NotImplementedException();
        }

        private void ComputeCPU()
        {
            throw new NotImplementedException();
        }

        private void AddField(string key, Fnptr func, TypeFlag typeFlag)
        {
            keyword[nfield] = key;
            vfunc[nfield] = func;
            vtype[nfield] = typeFlag;
            nfield++;
        }

        private void Deallocate()
        {
            int n = nfield;

            //keyword=null;
            vfunc = null;
            vtype = null;

            field2index = null;
            argindex1 = null;
            argindex2 = null;

            id_compute = null;
            computewhich = null;
            computes = null;

            id_fix = null;
            fixes = null;

            id_surf_collide = null;
            sc = null;

            id_surf_react = null;
            sr = null;

            id_variable = null;
            variables = null;
        }

        private void Allocate()
        {
            int n = nfield;
            keyword = new string[n];
            vfunc = new Fnptr[n];
            vtype = new TypeFlag[n];
            field2index = new int[n];
            argindex1 = new int[n];
            argindex2 = new int[n];

            // memory for computes, fixes, variables

            ncompute = 0;
            id_compute = new string[n];
            computewhich = new ComputeWhich[n];
            computes = new Compute[n];

            nfix = 0;
            id_fix = new string[n];
            fixes = new Fix[n];

            nsurfcollide = 0;
            id_surf_collide = new string[n];
            sc = new SurfCollide[n];

            nsurfreact = 0;
            id_surf_react = new string[n];
            sr = new SurfReact[n];

            nvariable = 0;
            id_variable = new string[n];
            variables = new int[n];
        }
        public void ComputeStep()
        {
            Console.WriteLine("compute step");
        }
    }
}