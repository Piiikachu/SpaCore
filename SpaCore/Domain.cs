using System;
using System.Collections.Generic;
using System.Text;

namespace SpaCore
{
    public class Domain
    {
        public enum bd
        {
            XLO, XHI, YLO, YHI, ZLO, ZHI, INTERIOR
        }

        public enum bc { PERIODIC, OUTFLOW, REFLECT, SURFACE, AXISYM };

        private SPARTA sparta;

        public bool box_exist;                    // 0 = not yet created, 1 = exists
        public int dimension;                    // 2,3
        public bool axisymmetric;                 // 1 for yes, 0 for no, only allowed in 2d
        public int boundary_collision_check;  // flag for whether init() check is required
                                              // for assign of collision models to boundaries

        public double[] boxlo, boxhi;         // box global bounds
        public double xprd, yprd, zprd;            // global box dimensions
        public double[] prd;                    // array form of dimensions

        public bc[] bflag;                     // boundary flags
        public double[,] norm;                // boundary normals

        public int surfreactany;                 // 1 if any boundary has surf reactions

        public int copy, copymode;                // 1 if copy of class (prevents deallocation of
                                                  //  base class when child copy is destroyed)

        public int nregion;                      // # of defined Regions
        public int maxregion;                    // max # regions can hold
        public Region[] regions;           // list of defined Regions


        private int[] surf_collide;              // index of SurfCollide model
        private int[] surf_react;                // index of SurfReact model
                                                 // for each bflag = SURFACE boundary

        public Domain(SPARTA sparta)
        {
            this.sparta = sparta;
            boxlo = new double[3];
            boxhi = new double[3];
            prd = new double[3];
            bflag = new bc[6];
            norm = new double[6, 3];
            surf_collide = new int[6];
            surf_react = new int[6];


            box_exist = false;
            dimension = 3;
            axisymmetric = false;
            boundary_collision_check = 1;

            for (int i = 0; i < 6; i++) bflag[i] = bc.PERIODIC;
            for (int i = 0; i < 6; i++) surf_collide[i] = surf_react[i] = -1;

            // surface normals of 6 box faces pointed inward towards particles

            norm[(int)bd.XLO, 0] = 1.0; norm[(int)bd.XLO, 1] = 0.0; norm[(int)bd.XLO, 2] = 0.0;
            norm[(int)bd.XHI, 0] = -1.0; norm[(int)bd.XHI, 1] = 0.0; norm[(int)bd.XHI, 2] = 0.0;
            norm[(int)bd.YLO, 0] = 0.0; norm[(int)bd.YLO, 1] = 1.0; norm[(int)bd.YLO, 2] = 0.0;
            norm[(int)bd.YHI, 0] = 0.0; norm[(int)bd.YHI, 1] = -1.0; norm[(int)bd.YHI, 2] = 0.0;
            norm[(int)bd.ZLO, 0] = 0.0; norm[(int)bd.ZLO, 1] = 0.0; norm[(int)bd.ZLO, 2] = 1.0;
            norm[(int)bd.ZHI, 0] = 0.0; norm[(int)bd.ZHI, 1] = 0.0; norm[(int)bd.ZHI, 2] = -1.0;

            nregion = maxregion = 0;
            regions = null;
            copy = copymode = 0;
        }

        public void SetBoundry(string[] arg)
        {
            int narg = arg.Length;

            if (box_exist)
            {
                sparta.DumpError("Boundary command after simulation box is defined");
            }
            if (narg != 3)
            {
                sparta.DumpError("Illegal boundary command");
            }

            char c;
            int m = 0;
            for (int idim = 0; idim < 3; idim++)
                for (int iside = 0; iside < 2; iside++)
                {
                    if (iside == 0) c = arg[idim][0];
                    else if (iside == 1 && arg[idim].Length == 1) c = arg[idim][0];
                    else c = arg[idim][1];

                    if (c == 'o') bflag[m] = bc.OUTFLOW;
                    else if (c == 'p') bflag[m] = bc.PERIODIC;
                    else if (c == 'r') bflag[m] = bc.REFLECT;
                    else if (c == 's') bflag[m] = bc.SURFACE;
                    else if (c == 'a') bflag[m] = bc.AXISYM;
                    else sparta.DumpError("Illegal boundary command");

                    surf_collide[m] = surf_react[m] = -1;
                    m++;
                }

            if (dimension == 2 && (bflag[(int)bd.ZLO] != bc.PERIODIC || bflag[(int)bd.ZHI] != bc.PERIODIC))
                sparta.DumpError("Z dimension must be periodic for 2d simulation");

            if (bflag[(int)bd.XLO] == bc.AXISYM || bflag[(int)bd.XHI] == bc.AXISYM ||
                bflag[(int)bd.YHI] == bc.AXISYM || bflag[(int)bd.ZLO] == bc.AXISYM || bflag[(int)bd.ZHI] == bc.AXISYM)
                sparta.DumpError("Only ylo boundary can be axi-symmetric");

            if (bflag[(int)bd.YLO] == bc.AXISYM)
            {
                axisymmetric = true;
                if (bflag[(int)bd.YHI] == bc.PERIODIC)
                    sparta.DumpError("Y cannot be periodic for axi-symmetric");
            }

            for (m = 0; m < 6; m += 2)
                if (bflag[m] == bc.PERIODIC || bflag[m + 1] == bc.PERIODIC)
                {
                    if (bflag[m] != bc.PERIODIC || bflag[m + 1] != bc.PERIODIC)
                        sparta.DumpError("Both sides of boundary must be periodic");
                }
        }

        public void PrintBox(string str)
        {
            string format = string.Format("{0}orthogonal box = ({1} {2} {3}) to ({4} {5} {6})\n", str, boxlo[0], boxlo[1], boxlo[2], boxhi[0], boxhi[1], boxhi[2]);
            sparta.DumpMessage(format);
        }
        public void SetInitialBox()
        {
            if (boxlo[0] >= boxhi[0] || boxlo[1] >= boxhi[1] || boxlo[2] >= boxhi[2])
                sparta.DumpError("Box bounds are invalid");
        }
        public void SetGlobalBox()
        {
            prd[0] = xprd = boxhi[0] - boxlo[0];
            prd[1] = yprd = boxhi[1] - boxlo[1];
            prd[2] = zprd = boxhi[2] - boxlo[2];
        }
    }
}
