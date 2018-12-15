using System;
using System.Collections.Generic;

namespace SpaCore
{
    public class BalanceGrid
    {
        private SPARTA sparta;
        private const double ZEROPARTICLE=0.1;

        private enum Bstyle { NONE, STRIDE, CLUMP, BLOCK, RANDOM, PROC, BISECTION };
        private enum RCBtype { CELL, PARTICLE };
        public BalanceGrid(SPARTA sparta)
        {
            this.sparta = sparta;
        }

        public void Command(string[] arg)
        {
            Grid grid = sparta.grid;
            int narg = arg.Length;
            if (!grid.exist)
                sparta.DumpError("Cannot balance grid before grid is defined");

            if (narg < 1) sparta.DumpError("Illegal balance_grid command");

            Bstyle bstyle=Bstyle.NONE;
            Order order;
            int px, py, pz;
            RCBtype rcbwt=RCBtype.CELL;
            int rcbflip;

            switch (arg[0])
            {
                case "rcb":
                    if (narg != 2 && narg != 3)
                        sparta.DumpError("Illegal balance_grid command");
                    bstyle = Bstyle.BISECTION;
                    switch (arg[1])
                    {
                        case "cell": rcbwt = RCBtype.CELL; break;
                        case "part": rcbwt = RCBtype.PARTICLE; break;
                        default:
                            sparta.DumpError("Illegal balance_grid command");
                            break;
                    }

                    // undocumented optional 3rd arg
                    // rcbflip = 3rd arg = 1 forces rcb.compute() to flip sign
                    //           of all grid cell "dots" to force totally different
                    //           assignment of grid cells to procs and induce
                    //           complete rebalance data migration
                    rcbflip = 0;
                    if (narg == 3) rcbflip = int.Parse(arg[2]);
                    break;

                default:
                    sparta.DumpError("BalanceGrid.Command(): complete arguments");
                    break;
            }
            if (bstyle==Bstyle.STRIDE||bstyle==Bstyle.CLUMP||bstyle==Bstyle.BLOCK)
            {
                if (!grid.uniform)
                    sparta.DumpError( "Invalid balance_grid style for non-uniform grid");
            }

            double time1 = Timer.getTime();

            List<Grid.ChildCell> cells = grid.cells;
            List<Grid.ChildInfo> cinfo = grid.cinfo;
            int nglocal = grid.nlocal;
            switch (bstyle)
            {
                case Bstyle.NONE:
                    break;
                case Bstyle.STRIDE:
                    break;
                case Bstyle.CLUMP:
                    break;
                case Bstyle.BLOCK:
                    break;
                case Bstyle.RANDOM:
                    break;
                case Bstyle.PROC:
                    break;
                case Bstyle.BISECTION:
                    RCB rcb = new RCB(sparta);

                    double[,] x;
                    x=new double[ nglocal, 3];

                    double[] lo,hi;

                    int nbalance = 0;
                    for (int icell = 0; icell < nglocal; icell++)
                    {
                        if (cells[icell].nsplit <= 0) continue;
                        lo = cells[icell].lo;
                        hi = cells[icell].hi;
                        x[nbalance,0] = 0.5 * (lo[0] + hi[0]);
                        x[nbalance,1] = 0.5 * (lo[1] + hi[1]);
                        x[nbalance,2] = 0.5 * (lo[2] + hi[2]);
                        nbalance++;
                    }

                    double[] wt;
                    if (rcbwt ==RCBtype.PARTICLE)
                    {
                        //sparta.particle.Sort();
                        //int zero = 0;
                        //int n;
                        //wt = new double[nglocal];
                        //nbalance = 0;
                        //for (int icell = 0; icell < nglocal; icell++)
                        //{
                        //    if (cells[icell].nsplit <= 0) continue;
                        //    n = cinfo[icell].count;
                        //    if (n!=0) wt[nbalance++] = n;
                        //    else
                        //    {
                        //        wt[nbalance++] = ZEROPARTICLE;
                        //        zero++;
                        //    }
                        //}
                        sparta.DumpError("BalanceGrid->Command: complete sorted by particle");
                    }

                    //rcb.Compute(nbalance, x, wt, rcbflip);
                    //rcb.Invert();

                    //nbalance = 0;
                    //int[] sendproc = rcb.Sendproc;
                    //for (int icell = 0; icell < nglocal; icell++)
                    //{
                    //    if (cells[icell].nsplit <= 0) continue;
                    //    cells[icell].proc = sendproc[nbalance++];
                    //}
                    //nmigrate = nbalance - rcb.nkeep;
                    
                    break;
                default:
                    break;
            }


        }
    }
}