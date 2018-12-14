using System;
using System.Collections.Generic;
using System.Text;

namespace SpaCore
{
    public class Surf
    {
        public enum Tally{ TALLYAUTO, TALLYREDUCE, TALLYLOCAL };         // same as Update
        public enum Region{ REGION_ALL, REGION_ONE, REGION_CENTER };      // same as Grid
        public enum enum1{ TYPE, MOLECULE, ID };
        public enum enum2{ LT, LE, GT, GE, EQ, NEQ, BETWEEN };
        private const int MAXGROUP = 32;
        public bool exist;                // 1 if any surfaces are defined, else 0
        public bool surf_collision_check; // flag for whether init() check is required
                                          // for assign of collision models to surfs

        public double[] bblo, bbhi;   // bounding box around surfs
        public Tally tally_comm;           // style of comm for surf tallies

        public int nreact_one;           // surface reactions in current step
        public Int64 nreact_running;    // running count of surface reactions

        public int ngroup;               // # of defined groups
        public string[] gnames;            // name of each group
        public int[] bitmask;             // one-bit mask for each group
        public int[] inversemask;         // inverse mask for each group

        public class Point
        {
            public double[] x;
            public Point()
            {
                x = new double[3];
            }
        }

        public class Line
        {
            int type, mask;          // type and mask of the element
            int isc, isr;            // index of surface collision and reaction models
                                     // -1 if unassigned
            int p1, p2;              // indices of points in line segment
                                     // rhand rule: Z x (p2-p1) = outward normal
            double[] norm;         // outward normal to line segment
            public Line()
            {
                norm = new double[3];
            }

        }

        public class Tri
        {
            int type, mask;          // type and mask of the element
            int isc, isr;            // index of surface collision and reaction models
                                     // -1 if unassigned
            int p1, p2, p3;           // indices of points in triangle
                                      // rhand rule: (p2-p1) x (p3-p1) = outward normal
            double[] norm;         // outward normal to triangle
            public Tri()
            {
                norm = new double[3];
            }
        }

        List<Point> pts;               // global list of points
        List<Line> lines;              // global list of lines
        List<Tri> tris;                // global list of tris
        int npoint, nline, ntri;    // number of each

        int[] mysurfs;             // indices of surf elements I own
        int nlocal;               // # of surf elements I own

        int nsc, nsr;              // # of surface collision and reaction models
                                   //      class SurfCollide **sc;   // list of surface collision models
                                   //class SurfReact **sr;     // list of surface reaction models

        int pushflag;             // set to 1 to push surf pts near grid cell faces
        double pushlo, pushhi;     // lo/hi ranges to push on
        double pushvalue;         // new position to push to
        public Surf(SPARTA sparta)
        {
            bblo = new double[3];
            bbhi = new double[3];

            exist = false;
            surf_collision_check = true;

            gnames = new string[MAXGROUP];
            bitmask = new int[MAXGROUP];
            inversemask = new int[MAXGROUP];
            for (int i = 0; i < MAXGROUP; i++)
            {
                bitmask[i] = 1 << i;
                inversemask[i]= bitmask[i] ^ ~0;

            }

            ngroup = 1;
            gnames[0] = "all";

            npoint = nline = ntri = 0;
            //pts = NULL;
            //lines = NULL;
            //tris = NULL;
            //pushflag = 1;

            //nlocal = 0;
            //mysurfs = NULL;

            //nsc = maxsc = 0;
            //sc = NULL;

            //nsr = maxsr = 0;
            //sr = NULL;

            tally_comm = Tally.TALLYAUTO;
        }
    }
}
