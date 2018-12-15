using System;
using System.Collections.Generic;
using System.Text;

namespace SpaCore
{
    public partial class Grid
    {
        private SPARTA sparta;

        public const int MAXGROUP = 32;
        public const int MAXSURFPERCELL = 100;
        private const int DELTA = 8192;
        public bool exist;            // 1 if grid is defined
        public bool exist_ghost;      // 1 if ghost cells exist
        public bool clumped;          // 1 if grid ownership is clumped, due to RCB
                                      // if not, some operations are not allowed

        public Int64 ncell;         // global count of child cells (unsplit+split, no sub)
        public Int64 nunsplit;      // global count of unsplit cells
        public int nsplit;           // global count of split cells
        public int nsub;             // global count of split sub cells
        public int maxsurfpercell;   // max surf elements in one child cell
        public int maxlevel;         // max level of any child cell in grid, 0 = root
        public bool uniform;          // 1 if all child cells are at same level, else 0
        public int unx, uny, unz;      // if uniform, effective global Nx,Ny,Nz of finest grid
        public double cutoff;        // cutoff for ghost cells, -1.0 = infinite
        public double cell_epsilon;  // half of smallest cellside of any cell in any dim
        public WeightFlag weightflag;   // 0/1+ for no/yes usage of cellwise fnum weighting

        public int ngroup;               // # of defined groups
        public string[] gnames;            // name of each group
        public int[] bitmask;             // one-bit mask for each group
        public int[] inversemask;         // inverse mask for each group

        public int copy, copymode;    // 1 if copy of class (prevents deallocation of
                                      //  base class when child copy is destroyed)

        public Dictionary<int, int> hash;
        public bool hashfilled;


        List<int> csurfs;
        List<int> csplits;
        List<int> csubs;

        public enum CellType { UNKNOWN, OUTSIDE, INSIDE, OVERLAP };

        // corners[i][j] = J corner points of face I of a grid cell
        // works for 2d quads and 3d hexes

        int[,] corners = new int[6, 4]{{0,2,4,6}, {1,3,5,7}, {0,1,4,5}, {2,3,6,7},
                     {0,1,2,3}, {4,5,6,7}};

        public int nlocal;                 // # of child cells I own (all 3 kinds)
        public int nghost;                 // # of ghost child cells I store (all 3 kinds)
        public int nempty;                 // # of empty ghost cells I store
        public int nunsplitlocal;          // # of unsplit cells I own
        public int nunsplitghost;          // # of ghost unsplit cells I store

        public void AddParentCell(int id, int iparent, int nx, int ny, int nz, double[] lo, double[] hi)
        {
            GrowPcells(1);

            ParentCell p = new ParentCell();
            p.id = id;
            p.mask = 1;
            if (iparent >= 0)
            {
                p.level = pcells[iparent].level + 1;
                p.nbits = pcells[iparent].nbits + pcells[iparent].newbits;
            }
            else p.level = p.nbits = 0;
            p.newbits = IdBits(nx, ny, nz);
            p.iparent = iparent;
            p.grandparent = 0;                // set by caller

            if (p.nbits + p.newbits > maxbits)
                sparta.DumpError("Cell ID has too many bits");

            p.nx = nx;
            p.ny = ny;
            p.nz = nz;
            p.lo[0] = lo[0]; p.lo[1] = lo[1]; p.lo[2] = lo[2];
            p.hi[0] = hi[0]; p.hi[1] = hi[1]; p.hi[2] = hi[2];

            pcells.Add(p);
            nparent++;


        }

        private void GrowPcells(int n)
        {
            if (pcells == null)
            {
                pcells = new List<ParentCell>();
            }
        }



        public int nsplitlocal;            // # of split cells I own
        public int nsplitghost;            // # of ghost split cells I store
        public int nsublocal;              // # of sub cells I own
        public int nsubghost;              // # of ghost sub cells I store
        public int nparent;                // # of parent cells

        public int maxlocal;               // size of cinfo

        public List<ChildCell> cells;           // list of owned and ghost child cells
        public List<ChildInfo> cinfo;           // extra info for nlocal owned cells
        public List<SplitInfo> sinfo;           // extra info for owned and ghost split cells
        public List<ParentCell> pcells;         // global list of parent cells

        protected int maxcell;             // size of cells
        protected int maxsplit;            // size of sinfo


        protected int maxparent;           // size of pcells


        protected int maxbits;             // max bits allowed in a cell ID

        protected int[] neighmask;        // bit-masks for each face in nmask
        protected int[] neighshift;       // bit-shifts for each face in nmask

        // connection between one of my cells and a neighbor cell on another proc

        struct Connect
        {
            int itype;           // type of sending cell
            int marktype;        // new type value (IN/OUT) for neighbor cell
            int jcell;           // index of neighbor cell on receiving proc (owner)
        };


        public enum bd
        {
            XLO, XHI, YLO, YHI, ZLO, ZHI, INTERIOR
        }

        public enum bc { PERIODIC, OUTFLOW, REFLECT, SURFACE, AXISYM };

        public enum rg { REGION_ALL, REGION_ONE, REGION_CENTER };      // same as Surf

        // cell type = OUTSIDE/INSIDE/OVERLAP if entirely outside/inside surfs
        //   or has any overlap with surfs including grazing or touching
        // corner point = OUTSIDE/INSIDE (not OVERLAP) if outside/inside
        //   if exactly on surf, is still marked OUTSIDE or INSIDE by cut2d/cut3d
        //   corner pts are only set if cell type = OVERLAP

        public enum enum1 { UNKNOWN, OUTSIDE, INSIDE, OVERLAP };           // several files
        public enum enum2 { NCHILD, NPARENT, NUNKNOWN, NPBCHILD, NPBPARENT, NPBUNKNOWN, NBOUND };  // Update
        public enum WeightFlag { NOWEIGHT, VOLWEIGHT, RADWEIGHT };

        public Grid(SPARTA sparta)
        {
            this.sparta = sparta;
            exist = exist_ghost = clumped = false;

            gnames = new string[MAXGROUP];
            bitmask = new int[MAXGROUP];
            inversemask = new int[MAXGROUP];
            neighmask = new int[6];
            neighshift = new int[6];

            for (int i = 0; i < MAXGROUP; i++)
            {
                bitmask[i] = 1 << i;
                inversemask[i] = bitmask[i] ^ ~0;
            }

            ngroup = 1;
            gnames[0] = "all";

            ncell = nunsplit = nsplit = nsub = 0;

            nlocal = nghost = maxlocal = maxcell = 0;
            nsplitlocal = nsplitghost = maxsplit = 0;
            nsublocal = nsubghost = 0;
            nparent = maxparent = 0;

            cells = null;
            cinfo = null;
            sinfo = null;
            pcells = null;

            maxbits = 8 * sizeof(int) - 1;

            maxsurfpercell = MAXSURFPERCELL;
            //csurfs = null; csplits = null; csubs = null;
            AllocateSurfArrays();

            neighshift[(int)bd.XLO] = 0;
            neighshift[(int)bd.XHI] = 3;
            neighshift[(int)bd.YLO] = 6;
            neighshift[(int)bd.YHI] = 9;
            neighshift[(int)bd.ZLO] = 12;
            neighshift[(int)bd.ZHI] = 15;

            neighmask[(int)bd.XLO] = 7 << neighshift[(int)bd.XLO];
            neighmask[(int)bd.XHI] = 7 << neighshift[(int)bd.XHI];
            neighmask[(int)bd.YLO] = 7 << neighshift[(int)bd.YLO];
            neighmask[(int)bd.YHI] = 7 << neighshift[(int)bd.YHI];
            neighmask[(int)bd.ZLO] = 7 << neighshift[(int)bd.ZLO];
            neighmask[(int)bd.ZHI] = 7 << neighshift[(int)bd.ZHI];

            cutoff = -1.0;
            weightflag = WeightFlag.NOWEIGHT;

            hash = new Dictionary<int, int>();
            hashfilled = false;
            copy = copymode = 0;
        }


        public class ParentCell
        {
            public int id;               // cell ID in bitwise format, 0 = root
            public int mask;                 // grid group mask
            public int level;                // level in hierarchical grid, 0 = root
            public int nbits;                // # of bits to encode my ID, also my siblings
            public int newbits;              // # of additional bits to encode my children
            public int iparent;              // index of parent, -1 if id=root
            public int grandparent;          // 1 if this cell is a grandparent, 0 if not
            public int nx, ny, nz;             // sub grid within cell
            public double[] lo, hi;       // opposite corner pts of cell
            public ParentCell()
            {
                lo = new double[3];
                hi = new double[3];
            }
        }

        public class SplitInfo
        {
        }

        public class ChildInfo
        {
            public int count;                // # of particles in this cell, 0 if split cell
            public int first;                // index of 1st particle in this cell, -1 if none

            public int mask;                 // grid group mask
            public CellType type;                 // OUTSIDE,INSIDE,OVERLAP,UNKNOWN
            public CellType[] corner;            // corner flags, 4/8 in 2d/3d
                                                 // OUTSIDE,INSIDE,UNKNOWN
                                                 // no OVERLAP is used for this anymore I think
                                                 // ordered x first, y next, z last
                                                 // for sub cells, type/corner
                                                 //   are same as in split cell containing them

            public double volume;            // flow volume of cell or sub cell
                                             // entire cell volume for split cell
            public double weight;            // fnum weighting for this cell
            public ChildInfo(int dimension)
            {
                if (dimension == 2)
                {
                    corner = new CellType[4];
                }
                else if (dimension == 3)
                {
                    corner = new CellType[8];
                }
                else
                {
                    throw new Exception("wrong dimension");
                }
            }

        }

        public class ChildCell
        {
            public int id;               // cell ID in bitwise format
            public int iparent;              // index of parent in pcells
            public int proc;                 // proc that owns this cell
            public int ilocal;               // index of this cell on owning proc
                                             // must be correct for all kinds of ghost cells

            public int[] neigh;         // info on 6 neighbor cells in cells/pcells
                                        //   that fully overlap face
                                        // order = XLO,XHI,YLO,YHI,ZLO,ZHI
                                        // nmask stores flags for what all 6 represent
                                        // if an index, store index into cells or pcells
                                        // if unknown, store ID of neighbor child cell
                                        // if non-periodic global boundary, ignored
            public int nmask;                // 3 bits for each of 6 values in neigh
                                             // 0 = index of child neighbor
                                             // 1 = index of parent neighbor
                                             // 2 = unknown child neighbor
                                             // 3 = index of PBC child neighbor
                                             // 4 = index of PBC parent neighbor
                                             // 5 = unknown PBC child neighbor
                                             // 6 = non-PBC boundary or ZLO/ZHI in 2d

            public double[] lo, hi;       // opposite corner pts of cell
            public int nsurf;                // # of surf elements in cell
                                             // -1 = empty ghost cell
            public int[] csurfs;              // indices of surf elements in cell
                                              // for sub cells, lo/hi/nsurf/csurfs
                                              //   are same as in split cell containing them

            public int nsplit;               // 1, unsplit cell
                                             // N > 1, split cell with N sub cells
                                             // N <= 0, neg of sub cell index (0 to Nsplit-1)
            public int isplit;               // index into sinfo
                                             // set for split and sub cells, -1 if unsplit

            public ChildCell()
            {
                neigh = new int[6];
                lo = new double[3];
                hi = new double[3];

            }

        }
        public void AddChildCell(int id, int iparent, double[] lo, double[] hi)
        {
            //todo: args not necessary
            GrowCells(1, 1);
            Domain domain = sparta.domain;
            int ncorner;

            if (domain.dimension == 3) ncorner = 8;
            else ncorner = 4;

            ChildCell c = new ChildCell();
            c.id = id;
            c.iparent = iparent;
            c.proc = 0;
            c.ilocal = nlocal;
            c.lo[0] = lo[0];
            c.lo[1] = lo[1];
            c.lo[2] = lo[2];
            c.hi[0] = hi[0];
            c.hi[1] = hi[1];
            c.hi[2] = hi[2];
            c.nsurf = 0;
            c.csurfs = null;
            c.nsplit = 1;
            c.isplit = -1;
            cells.Add(c);

            ChildInfo ci = new ChildInfo(domain.dimension);
            ci.count = 0;
            ci.first = -1;
            ci.mask = 1;
            ci.type = CellType.OUTSIDE;
            for (int i = 0; i < ncorner; i++) ci.corner[i] = CellType.UNKNOWN;
            ci.weight = 1.0;

            if (domain.dimension == 3)
                ci.volume = (hi[0] - lo[0]) * (hi[1] - lo[1]) * (hi[2] - lo[2]);
            else if (domain.axisymmetric)
                ci.volume = Math.PI * (hi[1] * hi[1] - lo[1] * lo[1]) * (hi[0] - lo[0]);
            else
                ci.volume = (hi[0] - lo[0]) * (hi[1] - lo[1]);

            // increment both since are adding an unsplit cell
            cinfo.Add(ci);

            nunsplitlocal++;
            nlocal++;

        }

        private void GrowCells(int v1, int v2)
        {
            if (cells == null)
            {
                cells = new List<ChildCell>();
            }
            if (cinfo == null)
            {
                cinfo = new List<ChildInfo>();
            }
        }
    }
}
