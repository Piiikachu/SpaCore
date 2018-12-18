using System;
using System.Collections.Generic;
using System.Text;

namespace SpaCore
{
    public partial class Grid
    {
        public void AllocateSurfArrays()
        {
            csurfs = new List<int>();
            csplits = new List<int>();
            csubs = new List<int>();
        }

        public void ClearSurf()
        {
            //int dimension = sparta.domain.dimension;
            //Surf surf = sparta.surf;
            //int ncorner = (dimension == 2) ? 4 : 8;
            //double[] lo, hi;
            //CellType celltype = CellType.UNKNOWN;
            //if (!surf.exist)
            //{
            //    celltype = CellType.OUTSIDE;
            //}

            csurfs.Clear();
            csplits.Clear();
            csubs.Clear();
            sparta.DumpMessage("GridSurf->ClearSurf: might be wrong");
        }
        public void SurftoGrid(bool flag,bool outflag=true)
        {
            Domain domain = sparta.domain;
            Surf surf = sparta.surf;
            ChildCell c;
            SplitInfo s;
            Cut2d cut2D;

            int nsurf=0;
            double[] lo, hi, vols;
            double[] xsplit = new double[3];

            int dim = domain.dimension;
            double[] slo = surf.bblo;
            double[] shi = surf.bbhi;
            if (dim == 3)
            {
                sparta.DumpError("Grid->SurftoGrid: 3d");
            }
            cut2D = new Cut2d(sparta, domain.axisymmetric);

            for (int icell = 0; icell < nlocal; icell++)
            {
                if (cells[icell].nsplit <= 0)
                {
                    continue;
                }
                lo = cells[icell].lo;
                hi = cells[icell].hi;
                if (BoxOverlap(lo, hi, slo, shi) == 0)
                {
                    continue;
                }
                if (dim == 3)
                {
                    sparta.DumpError("Grid->SurftoGrid: 3d");
                }
                else
                {
                    nsurf = cut2D.SurftoGrid(cells[icell].id, cells[icell].lo, cells[icell].hi, csurfs, maxsurfpercell);
                }
                if (nsurf<0)
                {
                    sparta.DumpError("Too many surfs in one cell");
                }
                if (nsurf!=0)
                {
                    cinfo[icell].type = CellType.OVERLAP;
                    cells[icell].nsurf = nsurf;
                    cells[icell].csurfs = csurfs.ToArray();
                    
                }
            }

            if (outflag)
            {
                SurftoGridStats();
            }


            throw new NotImplementedException();
        }

        private void SurftoGridStats()
        {
            double cmax, len;
            int dim = sparta.domain.dimension;

            int scount = 0;
            int stotal = 0;
            int smax = 0;

            double sratio = double.MaxValue;

            foreach (ChildCell cell in cells)
            {
                if (cell.nsplit <= 0) continue;
                if (cell.nsurf!=0) scount++;
                stotal += cell.nsurf;
                smax = Math.Max(smax, cell.nsurf);

                cmax = Math.Max(cell.hi[0] - cell.lo[0],
                       cell.hi[1] - cell.lo[1]);
                if (dim == 3)
                    cmax = Math.Max(cmax, cell.hi[2] - cell.lo[2]);

                if (dim == 2)
                {
                    for (int i = 0; i < cell.nsurf; i++)
                    {
                        len = sparta.surf.LineSize(cell.csurfs[i]);
                        sratio = Math.Min(sratio, len / cmax);
                    }
                }
                else if (dim == 3)
                {
                    sparta.DumpError("Grid->SurftoGrid: 3d");

                    //for (int i = 0; i < cell.nsurf; i++)
                    //{
                    //    sparta.surf.tri_size(cell.csurfs[i], len);
                    //    sratio = Math.Min(sratio, len / cmax);
                    //}
                }
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("  {0} = cells with surfs\n", scount);
            sb.AppendFormat("  {0} = total surfs in all grid cells\n", stotal);
            sb.AppendFormat("  {0} = max surfs in one grid cell\n", smax);
            sb.AppendFormat("  {0} = min surf-size/cell-size ratio\n", sratio);
            sparta.DumpMessage(sb.ToString());
        }
    }
}
