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
    }
}
