using System;
using System.Collections.Generic;
using System.Text;

namespace SpaCore
{
    public class Surf
    {
        public enum Tally { TALLYAUTO, TALLYREDUCE, TALLYLOCAL };         // same as Update
        public enum Region { REGION_ALL, REGION_ONE, REGION_CENTER };      // same as Grid
        public enum enum1 { TYPE, MOLECULE, ID };
        public enum enum2 { LT, LE, GT, GE, EQ, NEQ, BETWEEN };
        private const int MAXGROUP = 32;
        private SPARTA sparta;
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
            public int id;
            public int type, mask;          // type and mask of the element
            public int isc, isr;            // index of surface collision and reaction models
                                            // -1 if unassigned
            public double[] p1, p2;              // indices of points in line segment
                                                 // rhand rule: Z x (p2-p1) = outward normal
            public double[] norm;         // outward normal to line segment
            public Line()
            {
                norm = new double[3];
                p1 = new double[3];
                p2 = new double[3];
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

        public List<Point> pts;               // global list of points
        public List<Line> lines;              // global list of lines
        public List<Tri> tris;                // global list of tris
        public int npoint, nline, ntri;    // number of each

        public int[] mysurfs;             // indices of surf elements I own
        public int nlocal;               // # of surf elements I own

        public int nsc, nsr;              // # of surface collision and reaction models
                                          //      class SurfCollide **sc;   // list of surface collision models
                                          //class SurfReact **sr;     // list of surface reaction models

        public int pushflag;             // set to 1 to push surf pts near grid cell faces
        public double pushlo, pushhi;     // lo/hi ranges to push on
        public double pushvalue;         // new position to push to
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
                inversemask[i] = bitmask[i] ^ ~0;

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
            this.sparta = sparta;
        }

        public void Grow()
        {
            lines = new List<Line>();
        }

        public static double LineSize(double[] x1, double[] x2)
        {
            double[] delta = new double[3];
            MathExtra.SubVector(x1, x2, ref delta);
            return MathExtra.LenVector(delta);
        }

        public void ComputeLineNormal()
        {
            double[] z = new double[] { 0,0,1}, delta = new double[3];

            //parallel
            foreach (Line l in lines)
            {
                MathExtra.SubVector(l.p2, l.p1, ref delta);
                MathExtra.CrossVector(z, delta, ref l.norm);
                MathExtra.NormVector(ref l.norm);
                l.norm[2] = 0.0;
            }
        }

        public void CheckPointInside()
        {
            int nbad = 0;
            double[] x;
            Domain domain = sparta.domain;
            int dim = domain.dimension;
            double[] boxlo = domain.boxlo;
            double[] boxhi = domain.boxhi;
            if (dim == 2)
            {


                //parallel
                foreach (Line l in lines)
                {
                    x = l.p1;
                    if (x[0] < boxlo[0] || x[0] > boxhi[0] || x[1] < boxlo[1] || x[1] > boxhi[1] || x[2] < boxlo[2] || x[2] > boxhi[2]) nbad++;
                    x = l.p2;
                    if (x[0] < boxlo[0] || x[0] > boxhi[0] || x[1] < boxlo[1] || x[1] > boxhi[1] || x[2] < boxlo[2] || x[2] > boxhi[2]) nbad++;
                }

            }
            else
            {
                sparta.DumpError("Surf->CheckPointInside: dim==3");
            }

            if (nbad != 0)
            {
                sparta.DumpError(string.Format("{0} surface points are not inside simulation box", nbad));
            }
        }

        public void CheckWatertight2d(int nline)
        {
            //todo:Check if the 
            //throw new NotImplementedException();
            sparta.DumpMessage("Surf->CheckWatertight2d: this is the function that checks if the surf is closed");
        }

        internal void SetupSurf()
        {

            int i, j;

            int n = nElement();
            nlocal = n;
            mysurfs = new int[nlocal];

            nlocal = 0;
            for (int m = 0; m < n; m++)
            {
                mysurfs[nlocal++] = m;
            }

            for (j = 0; j < 3; j++)
            {
                bblo[j] = double.MaxValue;
                bbhi[j] = -double.MaxValue;
            }

            double[] x;
            if (sparta.domain.dimension == 2)
            {
                foreach (Line l in lines)
                {
                    x = l.p1;
                    for (j = 0; j < 2; j++)
                    {
                        bblo[j] = Math.Min(bblo[j], x[j]);
                        bbhi[j] = Math.Max(bbhi[j], x[j]);
                    }
                    x = l.p2;
                    for (j = 0; j < 2; j++)
                    {
                        bblo[j] = Math.Min(bblo[j], x[j]);
                        bbhi[j] = Math.Max(bbhi[j], x[j]);
                    }
                }
                bblo[2] = sparta.domain.boxlo[2];
                bbhi[2] = sparta.domain.boxhi[2];
            }
            else
            {
                sparta.DumpError("Surf->SetupSurf: dim==3");
            }


        }

        private int nElement()
        {
            if (sparta.domain.dimension == 2)
            {
                return nline;
            }
            else
            {
                return ntri;
            }
        }
    }
}
