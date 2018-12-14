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

        public int box_exist;                    // 0 = not yet created, 1 = exists
        public int dimension;                    // 2,3
        public int axisymmetric;                 // 1 for yes, 0 for no, only allowed in 2d
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


            box_exist = 0;
            dimension = 3;
            axisymmetric = 0;
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
    }
}
