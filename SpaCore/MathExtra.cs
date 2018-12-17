using System;
using System.Collections.Generic;
using System.Text;

namespace SpaCore
{
    public static class MathExtra
    {
        public static void SubVector(double[] v1, double[] v2, ref double[] ans)
        {
            ans[0] = v1[0] - v2[0];
            ans[1] = v1[1] - v2[1];
            ans[2] = v1[2] - v2[2];
        }
        public static double LenVector(double[] v)
        {
            return Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
        }

        public static void CrossVector(double[] v1, double[]v2, ref double[] ans)
        {
            ans[0] = v1[1] * v2[2] - v1[2] * v2[1];
            ans[1] = v1[2] * v2[0] - v1[0] * v2[2];
            ans[2] = v1[0] * v2[1] - v1[1] * v2[0];
        }

        public static void NormVector(ref double[] v)
        {
            double scale = 1.0 / Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
            v[0] *= scale;
            v[1] *= scale;
            v[2] *= scale;
        }
    }
}
