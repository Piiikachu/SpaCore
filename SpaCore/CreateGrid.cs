using System;
using System.Collections.Generic;
using System.Text;

namespace SpaCore
{

    public enum Order { XYZ, XZY, YXZ, YZX, ZXY, ZYX };
    public enum Inside { ANY, ALL };
    public class CreateGrid
    {
        public enum Bstyle { NONE, LEVEL, STRIDE, CLUMP, BLOCK, RANDOM };
        private SPARTA sparta;
        private int dimension;

        public CreateGrid(SPARTA sparta)
        {
            this.sparta = sparta;
        }

        public void Command(string[] arg)
        {
            Domain domain = sparta.domain;
            Grid grid = sparta.grid;
            int narg = arg.Length;
            if (!domain.box_exist)
            {
                sparta.DumpError("Cannot create grid before simulation box is defined");
            }
            if (grid.exist)
            {
                sparta.DumpError("Cannot create grid when grid is already defined");
            }
            grid.exist = true;
            if (narg < 3)
            {
                sparta.DumpError("Illegal create_grid command");
            }
            int nx = int.Parse(arg[0]);
            int ny = int.Parse(arg[1]);
            int nz = int.Parse(arg[2]);

            if (nx < 1 || ny < 1 || nz < 1)
            {
                sparta.DumpError("Illegal create_grid command");
            }
            if (domain.dimension == 2 && nz != 1)
            {
                sparta.DumpError("Create_grid nz value must be 1 for a 2d simulation");
            }
            dimension = domain.dimension;

            int nlevels = 1;
            Bstyle bstyle = Bstyle.NONE;
            int px = 0;
            int py = 0;
            int pz = 0;
            int order;
            Inside inside = Inside.ANY;
            int iarg = 3;
            while (iarg < narg)
            {
                sparta.DumpError("Complete CreateGrid.Command  optional arguments");

                iarg++;
            }
            if (bstyle == Bstyle.NONE)
            {
                bstyle = Bstyle.LEVEL;
            }

            if (bstyle == Bstyle.BLOCK)
            {
                sparta.DumpError("Complete CreateGrid.Command  Bstyle.Block");
            }

            double time1 = Timer.getTime();

            int level = 1;
            int xlo, xhi, ylo, yhi, zlo, zhi;
            xlo = xhi = ylo = yhi = zlo = zhi = 1;
            iarg = 3;
            Region region = null;

            Int64 count = 0;

            int pnx, pny, pnz, ix, iy, iz, nbits, proc;
            bool pflag;
            int m, nth, idgrandparent, idparent, idchild;
            double[] lo = new double[3], hi = new double[3];
            Grid.ParentCell p;
            while (true)
            {
                if (level == 1)
                {
                    grid.AddParentCell(0, -1, nx, ny, nz, domain.boxlo, domain.boxhi);
                }
                else
                {
                    int nparent = grid.nparent;
                    int prevlevel = level - 2;

                    for (int igrandparent = 0; igrandparent < nparent; igrandparent++)
                    {
                        if (grid.pcells[igrandparent].level != prevlevel) continue;
                        p = grid.pcells[igrandparent];

                        idgrandparent = p.id;
                        nbits = p.nbits;
                        pnx = p.nx;
                        pny = p.ny;
                        pnz = p.nz;

                        m = 0;
                        for (iz = 0; iz < pnz; iz++)
                            for (iy = 0; iy < pny; iy++)
                                for (ix = 0; ix < pnx; ix++)
                                {
                                    m++;
                                    idparent = idgrandparent | (m << nbits);
                                    grid.IdChildLohi(igrandparent, m, lo, hi);
                                    if (region != null) pflag = CellInRegion(lo, hi, region, inside);
                                    else
                                    {
                                        pflag = true;
                                        if (ix + 1 < xlo || ix + 1 > xhi) pflag = false;
                                        if (iy + 1 < ylo || iy + 1 > yhi) pflag = false;
                                        if (iz + 1 < zlo || iz + 1 > zhi) pflag = false;
                                    }
                                    if (pflag)
                                        grid.AddParentCell(idparent, igrandparent, nx, ny, nz, lo, hi);
                                    else
                                    {
                                        if (count % 1 == 0)
                                            grid.AddChildCell(idparent, igrandparent, lo, hi);
                                        count++;
                                    }
                                }
                    }

                }

                // final level, add current level cells as child cells
                // loop over all parent cells to find ones at previous level
                // use their info to generate my child cells at this level
                // if BSTYLE is set, there is only 1 level, create proc's cells directly

                if (level == nlevels)
                {
                    List<Grid.ParentCell> pcells = grid.pcells;
                    int nparent = grid.nparent;
                    int prevlevel = level - 1;
                    for (int iparent = 0; iparent < nparent; iparent++)
                    {
                        if (pcells[iparent].level != prevlevel) continue;
                        p = pcells[iparent];
                        idparent = p.id;
                        nbits = p.nbits;
                        nx = p.nx;
                        ny = p.ny;
                        nz = p.nz;

                        if (bstyle == Bstyle.LEVEL)
                        {
                            int ntotal = (int)nx * ny * nz;
                            int firstproc = (int)count % 1;
                            int ifirst = 0 - firstproc + 1;
                            if (ifirst <= 0) ifirst += 1;
                            for (m = ifirst; m <= ntotal; m += 1)
                            {
                                idchild = idparent | (m << nbits);
                                grid.IdChildLohi(iparent, m, lo, hi);
                                grid.AddChildCell(idchild, iparent, lo, hi);
                            }
                            count += ntotal;

                            // loop over all child cells
                            // convert M to Nth based on order
                            // assign each cell to proc based on Nth and STRIDE or CLUMP
                        }
                        else
                        {
                            sparta.DumpError("CreateGrid->Command: more Bstyle");
                        }

                    }
                    break;




                }
                if (level == nlevels)
                {
                    break;
                }
                level++;

                switch (arg[iarg])
                {
                    //case "level"
                    default:
                        sparta.DumpError("CreateGrid->Command: complete this");
                        break;
                }

            }
            List<Grid.ParentCell> pcells1 = grid.pcells;
            int nparent1 = grid.nparent;
            for (int i = 1; i < nparent1; i++)
            {
                pcells1[pcells1[i].iparent].grandparent = 1;
            }

            if (bstyle == Bstyle.CLUMP || bstyle == Bstyle.BLOCK)
            {
                grid.clumped = true;
            }
            else
            {
                grid.clumped = false;
            }

            double time2 = Timer.getTime();
            grid.SetupOwned();
            grid.AcquireGhosts();
            grid.FindNeighbors();
            grid.CheckUniform();

            double time3 = Timer.getTime();

            double time_total = time3 - time1;

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Create {0} child grid cells\n", grid.ncell);
            sb.AppendFormat("  parent cells = {0}\n", grid.nparent);
            sb.AppendFormat("  CPU time = {0} secs\n", time_total);
            sb.AppendFormat("  create/ghost percent = {0} {1}\n", 100.0 * (time2 - time1) / time_total, 100.0 * (time3 - time2) / time_total);
            sparta.DumpMessage(sb.ToString());
            //todo:continue
        }

        private bool CellInRegion(double[] lo, double[] hi, Region region, Inside inside)
        {
            double[] x = new double[3];

            int n = 0;

            x[0] = lo[0]; x[1] = lo[1]; x[2] = lo[2];
            n += region.Match(x);
            x[0] = hi[0]; x[1] = lo[1]; x[2] = lo[2];
            n += region.Match(x);
            x[0] = lo[0]; x[1] = hi[1]; x[2] = lo[2];
            n += region.Match(x);
            x[0] = hi[0]; x[1] = hi[1]; x[2] = lo[2];
            n += region.Match(x);

            if (dimension == 3)
            {
                x[0] = lo[0]; x[1] = lo[1]; x[2] = hi[2];
                n += region.Match(x);
                x[0] = hi[0]; x[1] = lo[1]; x[2] = hi[2];
                n += region.Match(x);
                x[0] = lo[0]; x[1] = hi[1]; x[2] = hi[2];
                n += region.Match(x);
                x[0] = hi[0]; x[1] = hi[1]; x[2] = hi[2];
                n += region.Match(x);
            }

            if (inside == Inside.ANY)
            {
                if (n != 0) return true;
            }
            else if (inside == Inside.ALL)
            {
                if (dimension == 2 && n == 4) return true;
                if (dimension == 3 && n == 8) return true;
            }

            return false;
        }
    }


}