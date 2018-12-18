using System;
using System.Collections.Generic;

namespace SpaCore
{
    public class Cut2d
    {
        private SPARTA sparta;
        private bool axisymmetric;
        public int npushmax;

        public int[] npushcell;


        private int id;
        private double[] lo, hi;
        private double[] pushlo_vec;
        private double[] pushhi_vec;
        private double[] pushvalue_vec;

        List<int> surfs;
        private int nsurf;

        public Cut2d(SPARTA sparta, bool axisymmetric)
        {
            this.sparta = sparta;
            this.axisymmetric = axisymmetric;
            Surf surf = sparta.surf;
            npushmax = 2;

            npushcell = new int[4];

            pushlo_vec = new double[3];
            pushhi_vec = new double[3];
            pushvalue_vec = new double[3];

            pushlo_vec[0] = -1.0;
            pushhi_vec[0] = 1.0;
            pushvalue_vec[0] = 0.0;

            pushlo_vec[1] = -1.0;
            pushhi_vec[1] = 1.0;
            pushvalue_vec[1] = 0.0;

            switch (surf.pushflag)
            {
                case 0: npushmax = 0; break;
                case 2:
                    npushmax = 3;
                    pushlo_vec[2] = surf.pushlo;
                    pushhi_vec[2] = surf.pushhi;
                    pushvalue_vec[2] = surf.pushvalue;
                    break;
                default:
                    break;
            }

        }

        public int SurftoGrid(int id_caller, double[] lo_caller, double[] hi_caller, List<int> csurfs, int max)
        {
            Surf surf = sparta.surf;
            id = id_caller;
            lo = lo_caller;
            hi = hi_caller;
            surfs = csurfs;

            List<Surf.Line> lines = surf.lines;
            int nline = surf.nline;

            double[] x1, x2;

            nsurf = 0;
            int m = -1;
            foreach (Surf.Line l in lines)
            {
                m++;
                x1 = l.p1;
                x2 = l.p2;

                if (Math.Max(x1[0], x2[0]) < lo[0]) continue;
                if (Math.Min(x1[0], x2[0]) > hi[0]) continue;
                if (Math.Max(x1[1], x2[1]) < lo[1]) continue;
                if (Math.Min(x1[1], x2[1]) > hi[1]) continue;

                if (Cliptest(x1, x2))
                {
                    if (nsurf == max)
                    {
                        return -1;
                    }
                    nsurf++;
                    surfs.Add(m);
                }

            }
            return nsurf;
        }

        public bool Cliptest(double[] p, double[] q)
        {
            double x, y;
            if (p[0] >= lo[0] && p[0] <= hi[0] && p[1] >= lo[1] && p[1] <= hi[1])
                return true;
            if (q[0] >= lo[0] && q[0] <= hi[0] && q[1] >= lo[1] && q[1] <= hi[1])
                return true;

            double[] a = new double[2], b = new double[2];

            a[0] = p[0]; a[1] = p[1];
            b[0] = q[0]; b[1] = q[1];

            if (a[0] < lo[0] && b[0] < lo[0]) return false;
            if (a[0] < lo[0] || b[0] < lo[0])
            {
                y = a[1] + (lo[0] - a[0]) / (b[0] - a[0]) * (b[1] - a[1]);
                if (a[0] < lo[0])
                {
                    a[0] = lo[0]; a[1] = y;
                }
                else
                {
                    b[0] = lo[0]; b[1] = y;
                }
            }
            if (a[0] > hi[0] && b[0] > hi[0]) return false;
            if (a[0] > hi[0] || b[0] > hi[0])
            {
                y = a[1] + (hi[0] - a[0]) / (b[0] - a[0]) * (b[1] - a[1]);
                if (a[0] > hi[0])
                {
                    a[0] = hi[0]; a[1] = y;
                }
                else
                {
                    b[0] = hi[0]; b[1] = y;
                }
            }

            if (a[1] < lo[1] && b[1] < lo[1]) return false;
            if (a[1] < lo[1] || b[1] < lo[1])
            {
                x = a[0] + (lo[1] - a[1]) / (b[1] - a[1]) * (b[0] - a[0]);
                if (a[1] < lo[1])
                {
                    a[0] = x; a[1] = lo[1];
                }
                else
                {
                    b[0] = x; b[1] = lo[1];
                }
            }
            if (a[1] > hi[1] && b[1] > hi[1]) return false;
            if (a[1] > hi[1] || b[1] > hi[1])
            {
                x = a[0] + (hi[1] - a[1]) / (b[1] - a[1]) * (b[0] - a[0]);
                if (a[1] > hi[1])
                {
                    a[0] = x; a[1] = hi[1];
                }
                else
                {
                    b[0] = x; b[1] = hi[1];
                }
            }

            return true;


        }
    }
}