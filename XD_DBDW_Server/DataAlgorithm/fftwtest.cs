using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
//using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Net.Sockets;
using System.Net;

using System.Runtime.InteropServices;
using System;

namespace XD_DBDW_Server
{
    class fftwtest : IDisposable
    {
        //qhw fftw
        double[] din, dout;
        GCHandle hdin, hdout;
        IntPtr dplan;

        public fftwtest(int n)
        {
            //double fftw;
            din = new double[2 * n];
            dout = new double[2 * n];
            hdin = GCHandle.Alloc(din, GCHandleType.Pinned);
            hdout = GCHandle.Alloc(dout, GCHandleType.Pinned);
            dplan = fftw.dft_1d(n, hdin.AddrOfPinnedObject(), hdout.AddrOfPinnedObject(), fftw_direction.Forward, fftw_flags.Measure);
        }

        // Tests a single plan, displaying results
        //plan: Pointer to plan to test
        public void TestPlan(IntPtr plan)
        {
            int start = System.Environment.TickCount;
            //  for (int i = 0; i < 10000; i++)
            fftwf.execute(plan);

            //a: adds, b: muls, c: fmas
            double a = 0, b = 0, c = 0;
            fftwf.flops(plan, ref a, ref b, ref c);
        }

        #region doublefftw

        public void TestPlandouble(IntPtr plan)
        {
            fftw.execute(plan);
        }

        public void setdin(double[] dou)
        {
            for (int i = 0; i < dou.Length; i++)
                din[i] = dou[i];
        }

        public void testdplan()
        {
            TestPlandouble(dplan);
        }

        public double[] getdout()
        {
            return dout;
        }

        public double[] getdout2()
        {
            double[] val = new double[dout.Length];
            for (int i = 0; i < dout.Length; i++)
                val[i] = dout[i];

            return val;

        }


        #endregion

        // Releases all memory used by FFTW/C#
        public void Dispose()
        {
            fftw.destroy_plan(dplan);
            hdin.Free();
            hdout.Free();
        }
    }
}