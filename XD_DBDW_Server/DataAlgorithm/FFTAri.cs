using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SWDirectCollect_Channel8
{
    /// <summary>
    /// FFT算法
    /// </summary>
    public class FFTAri : IDisposable
    {
        IntPtr m_AlgPtr;
        /// <summary>
        /// FFT算法初始化
        /// </summary> 
        public FFTAri()
        {
            m_AlgPtr = CreateInst();
        }


        /// <summary>
        /// 执行FFT算法
        /// </summary>
        /// <param name="inputdata"></param> 
        /// <param name="outdata"></param>
        /// <param name="iLength"></param>
        /// <param name="winType">
        /// 窗类型
        ///     WINHANN = 3;   汉宁窗
        ///     WINHAMMING= 2; 海明窗
        ///     WINBLACKMAN = 1;  布莱克曼窗
        ///     WINBARTLETT = 0; 
        ///</param>
        /// <returns></returns>
        ///
        //datLen为输出样点数
        public int Exec(float[] pfcData, float[] pOutData, int datLen, int winType = 3)
        {
            return Exec(m_AlgPtr, pfcData, pOutData, datLen, winType);
        }

        /// <summary>
        /// 释放算法
        /// </summary>
        public void Dispose()
        {
            if (m_AlgPtr != IntPtr.Zero)
            {
                DestoryInst(m_AlgPtr);
                m_AlgPtr = IntPtr.Zero;
            }
        }

        #region 互操作

        /// <summary>
        /// 
        /// </summary>
#if DEBUG
        const string DLLPATH = @".\\CommonAlgorithmD.dll";

#else
        
        const string DLLPATH = @".\\CommonAlgorithm.dll";
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idatLen"></param>
        /// <param name="iOverlayNum"></param>
        /// <returns></returns>
        [DllImport(DLLPATH,
         EntryPoint = "CreateFFTImpl",
         SetLastError = true,
         CharSet = CharSet.Ansi,
         ExactSpelling = false,
        CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr CreateInst();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDestory"></param>
        [DllImport(DLLPATH,
            EntryPoint = "DestroyFFTImpl",
            SetLastError = true,
         CharSet = CharSet.Ansi,
        ExactSpelling = false,
        CallingConvention = CallingConvention.Cdecl)]
        static extern void DestoryInst(IntPtr pDestory);

        /// <summary>
        /// 执行CplxToReal的算法
        /// </summary>
        /// <param name="pCaInst"></param>
        /// <param name="inputdata"></param>
        /// <param name="outdata"></param>
        /// <param name="iLength"></param>
        /// <param name="winType">
        /// 窗类型
        ///     WINHANN = 3;
        ///     WINHAMMING= 2;
        ///     WINBLACKMAN = 1;
        ///     WINBARTLETT = 0; 
        ///</param>
        /// <returns></returns>
        [DllImport(DLLPATH,
        EntryPoint = "DoFFTCplxToReal",
        SetLastError = true,
         CharSet = CharSet.Ansi,
        ExactSpelling = false,
        CallingConvention = CallingConvention.Cdecl)]
        static extern int Exec(IntPtr pCaInst, float[] pfcData, float[] pOutData, int iLength, int winType);

        #endregion
    }
}
