using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XD_DBDW_Server
{
    static class Hanning
    {
        public static double[] HanningWindow(int N)
        {
            double[] sig = new double[N];
            for (int i = 0; i < N; i++)
            {
                sig[i] = 0.5 * (1 - Math.Cos(2 * Math.PI * (i + 1) / (N + 1)));
            }
            return sig;
        }
    }
}
