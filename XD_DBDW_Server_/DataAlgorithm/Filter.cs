using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace XD_DBDW_Server.DataAlgorithm
{
    class Filter
    {
        private static string[] fstrs = new string[20];
        private static bool[] fltparaOK = new bool[120];
        public static float[,,] filter_be = new float[120, 3, 3];
        public static float[,,] filter_ae = new float[120, 3, 3];

        //滤波器系数加载
        public static bool LoadFilterPara()
        {
            //初始化时运行一次：从预置文件中读取滤波器参数
            bool innum = false, fileok = true;
            char cc;
            string path1, readtxt, readline;
            int f, f1, i, j, s, txtlen, p1 = 0, linelen, lineno;
            for (f = 0; f < 120; f++)//0.1~10KHz带宽，若小于则归到0.1KHz，若大于10KHz则不滤波
            {
                f1 = f + 1;
                path1 = "dflib\\lowpass_filter_" + f1.ToString() + "k.dat";
                //打开文件
                if (File.Exists(path1))
                {
                    using (StreamReader sr = File.OpenText(path1))
                    {
                        readtxt = sr.ReadToEnd() + " ";//最后一数判结束
                        txtlen = readtxt.Length;
                        linelen = 0;
                        lineno = 0;
                        for (int p = 0; p < txtlen; p++)
                        {
                            cc = readtxt[p];
                            if ((cc != ' ') && (cc != '\n') && (cc != '\r'))//有效字符&& (readtxt[p] != '\t')
                            {
                                if (innum)
                                {
                                    linelen++;
                                }
                                else
                                {
                                    innum = true;
                                    p1 = p;
                                    linelen = 1;
                                }

                            }
                            else//无效字符
                            {
                                if (innum)
                                {
                                    //检完一个有效数
                                    innum = false;
                                    fstrs[lineno] = readtxt.Substring(p1, linelen);
                                    linelen = 0;
                                    lineno++;
                                }
                            }
                        }

                        if (lineno == 19)
                        {
                            fltparaOK[f] = true;
                            for (s = 0; s < 18; s++)
                            {
                                i = s / 6;
                                j = s % 6;
                                if (j < 3)
                                {
                                    filter_be[f, i, j] = Single.Parse(fstrs[s]);
                                }
                                else
                                {
                                    filter_ae[f, i, j - 3] = Single.Parse(fstrs[s]);
                                }
                            }
                            readline = fstrs[18].Trim();
                            filter_gain[f] = Single.Parse(readline);
                        }
                        else
                        {
                            fltparaOK[f] = false;//此时应处理为滤波无效
                        }
                    }
                }
                else
                {
                    fltparaOK[f] = false;//此时应处理为滤波无效
                    fileok = false;
                }
            }
            //filter_gain从文件读取的float参与运算后数值不能传到DLL？
            filter_gain[0] = 3.1341308e-004F;
            filter_gain[39] = Single.Parse("5.3023623e-003");
            filter_gain[40] = Single.Parse("5.7308317e-003");

            return (fileok);
        }


        //DDC滤波器中的单路频谱搬移(若偏频为0则自动不搬频)
        public static void SpectrumShiftDDC(int ctrfreq, float[] DatR, float[] DatI, ref int snpN, int CHCT, int iDFl)
        {
            //ctrfreq单位Hz，需要搬移的0中频相对频率
            if (ctrfreq != 0)
            {
                int ix, i, j;
                float ac, bd, ad, bc, fo_r, fo_i;

                for (i = 0; i < iDFl; i++)
                {
                    fo_r = (float)Math.Cos(2 * Math.PI * (ctrfreq / 12500.0F) * snpN);//fs=12.5KHz
                    fo_i = (float)Math.Sin(2 * Math.PI * (ctrfreq / 12500.0F) * snpN);
                    for (j = 0; j < CHCT; j++)
                    {
                        ix = (iDFl * j) + i;

                        ac = DatR[ix] * fo_r;//ac
                        bd = DatI[ix] * fo_i;//bd
                        ad = DatR[ix] * fo_i;//ad
                        bc = DatI[ix] * fo_r;//bc
                        DatR[ix] = (ac - bd);
                        DatI[ix] = (ad + bc);
                    }
                    snpN++;
                    if (snpN >= 12500)
                        snpN = 0;
                }
            }
        }

        static float[] filter_gain = new float[120];
        //单路DDC滤波器计算
        public static void FilterCacuDDC(int fno, double[,,] flt_1_w, double[,,] flt_2_w, double[,,] flt_3_w, float[,,] flt_ae, float[,,] flt_be, float[] tpDR, float[] tpDI,
            int CHCT, int iDFl)
        {
            if (fno < 120)
            {
                int i, j, ix;
                double tempdata1, tempdata2, tempdata3;

                for (i = 0; i < iDFl; i++)
                {
                    for (j = 0; j < CHCT; j++)
                    {
                        ix = (iDFl * j) + i;
                        //I数据                        
                        //第1级
                        flt_1_w[j, 0, 2] = tpDR[ix] - (flt_ae[fno, 0, 1] * flt_1_w[j, 0, 1]) - (flt_ae[fno, 0, 2] * flt_1_w[j, 0, 0]);
                        tempdata1 = flt_1_w[j, 0, 2] + (flt_be[fno, 0, 1] * flt_1_w[j, 0, 1]) + (flt_be[fno, 0, 2] * flt_1_w[j, 0, 0]);
                        flt_1_w[j, 0, 0] = flt_1_w[j, 0, 1];
                        flt_1_w[j, 0, 1] = flt_1_w[j, 0, 2];
                        //第2级
                        flt_2_w[j, 0, 2] = tempdata1 - (flt_ae[fno, 1, 1] * flt_2_w[j, 0, 1]) - (flt_ae[fno, 1, 2] * flt_2_w[j, 0, 0]);
                        tempdata2 = flt_2_w[j, 0, 2] + (flt_be[fno, 1, 1] * flt_2_w[j, 0, 1]) + (flt_be[fno, 1, 2] * flt_2_w[j, 0, 0]);
                        flt_2_w[j, 0, 0] = flt_2_w[j, 0, 1];
                        flt_2_w[j, 0, 1] = flt_2_w[j, 0, 2];
                        //第3级
                        flt_3_w[j, 0, 2] = tempdata2 - (flt_ae[fno, 2, 1] * flt_3_w[j, 0, 1]) - (flt_ae[fno, 2, 2] * flt_3_w[j, 0, 0]);
                        tempdata3 = flt_3_w[j, 0, 2] + (flt_be[fno, 2, 1] * flt_3_w[j, 0, 1]) + (flt_be[fno, 2, 2] * flt_3_w[j, 0, 0]);
                        flt_3_w[j, 0, 0] = flt_3_w[j, 0, 1];
                        flt_3_w[j, 0, 1] = flt_3_w[j, 0, 2];
                        tpDR[ix] = (float)(tempdata3 * filter_gain[fno]);

                        //Q数据
                        //第1级
                        flt_1_w[j, 1, 2] = tpDI[ix] - (flt_ae[fno, 0, 1] * flt_1_w[j, 1, 1]) - (flt_ae[fno, 0, 2] * flt_1_w[j, 1, 0]);
                        tempdata1 = flt_1_w[j, 1, 2] + (flt_be[fno, 0, 1] * flt_1_w[j, 1, 1]) + (flt_be[fno, 0, 2] * flt_1_w[j, 1, 0]);
                        flt_1_w[j, 1, 0] = flt_1_w[j, 1, 1];
                        flt_1_w[j, 1, 1] = flt_1_w[j, 1, 2];
                        //第2级
                        flt_2_w[j, 1, 2] = tempdata1 - (flt_ae[fno, 1, 1] * flt_2_w[j, 1, 1]) - (flt_ae[fno, 1, 2] * flt_2_w[j, 1, 0]);
                        tempdata2 = flt_2_w[j, 1, 2] + (flt_be[fno, 1, 1] * flt_2_w[j, 1, 1]) + (flt_be[fno, 1, 2] * flt_2_w[j, 1, 0]);
                        flt_2_w[j, 1, 0] = flt_2_w[j, 1, 1];
                        flt_2_w[j, 1, 1] = flt_2_w[j, 1, 2];
                        //第3级
                        flt_3_w[j, 1, 2] = tempdata2 - (flt_ae[fno, 2, 1] * flt_3_w[j, 1, 1]) - (flt_ae[fno, 2, 2] * flt_3_w[j, 1, 0]);
                        tempdata3 = flt_3_w[j, 1, 2] + (flt_be[fno, 2, 1] * flt_3_w[j, 1, 1]) + (flt_be[fno, 2, 2] * flt_3_w[j, 1, 0]);
                        flt_3_w[j, 1, 0] = flt_3_w[j, 1, 1];
                        flt_3_w[j, 1, 1] = flt_3_w[j, 1, 2];
                        tpDI[ix] = (float)(tempdata3 * filter_gain[fno]);
                    }
                }
            }
        }
    }
}
