using System;
using System.Collections.Generic;
using System.Text;

namespace SpaCore
{
    public partial class Grid
    {
        public int IdBits(int nx, int ny, int nz)
        {
            Int64 n = ((Int64)nx) * ny * nz;
            Int64 nstore = 1;
            int nbits = 1;
            while (nstore < n)
            {
                nstore = 2 * nstore + 1;
                nbits++;
            }
            return nbits;
        }
        public void IdChildLohi(int iparent, int ichild, double[] lo, double[] hi)
        {
            ParentCell p = pcells[iparent];
            ichild--;

            int nx = p.nx;
            int ny = p.ny;
            int nz = p.nz;

            int ix = ichild % nx;
            int iy = (ichild / nx) % ny;
            int iz = ichild / (nx * ny);

            double[] plo = p.lo;
            double[] phi = p.hi;

            lo[0] = plo[0] + ix * (phi[0] - plo[0]) / nx;
            lo[1] = plo[1] + iy * (phi[1] - plo[1]) / ny;
            lo[2] = plo[2] + iz * (phi[2] - plo[2]) / nz;

            hi[0] = plo[0] + (ix + 1) * (phi[0] - plo[0]) / nx;
            hi[1] = plo[1] + (iy + 1) * (phi[1] - plo[1]) / ny;
            hi[2] = plo[2] + (iz + 1) * (phi[2] - plo[2]) / nz;

            if (ix == nx - 1) hi[0] = phi[0];
            if (iy == ny - 1) hi[1] = phi[1];
            if (iz == nz - 1) hi[2] = phi[2];

           
        }
    }
}
