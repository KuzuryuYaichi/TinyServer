using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.ComponentCs;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

namespace XD_DBDW_Server
{
    public class DataFrequencySpectrum
    {
        GCHandle hdin, hdout;
        IntPtr dplan;
        private DataProcessing m_DataProcessing;

        public delegate void PassPowerAndFreq(PowerAndFreq t);
        public event PassPowerAndFreq passPowerAndFreq;
        public DataFrequencySpectrum(DataProcessing m_DataProcessing)
        {
            this.m_DataProcessing = m_DataProcessing;
            IQdata = new double[19200*64][];
            for (int i = 0; i < IQdata.Length; i++)
            {
                IQdata[i] = new double[20];
            }
            m_queue = new AsyncDataQueue<IQDataAndFreq>();
            m_queue.TEvent += new THandler<IQDataAndFreq>(m_queue_TEvent);
        }

        private AsyncDataQueue<IQDataAndFreq> m_queue;
        private int m_SmoothNum = 1;
        private int SmoothIndex = 0;
        double[][] IQdata;

        public void SelectSmoothNum(int SmoothNum)
        {
            ClearQueue();
            m_SmoothNum = SmoothNum;
        }

        public void PushData(IQDataAndFreq t)
        {
            m_queue.Enqueue(t);
        }

        public void ClearQueue()
        {
            m_queue.ClearQueue();
        }

        void m_queue_TEvent(IQDataAndFreq t)
        {
            double[] outData = new double[t.Data.Length];
            int mm_SmoothNum = m_SmoothNum;
            double offset = 0;
            int m_RFGain;
            int m_DigitalGain;

            if (t.Type == 3)
            {
                Array.Copy(t.Data, 0, outData, 0, t.Data.Length);
                for (int k = 0; k < outData.Length; k++)
                {
                    IQdata[k][SmoothIndex] = outData[k];
                }
               
                offset = 133 - 11.85;
            }
            else
            {
                hdin = GCHandle.Alloc(t.Data, GCHandleType.Pinned);
                hdout = GCHandle.Alloc(outData, GCHandleType.Pinned);
                dplan = fftw.dft_1d(t.Data.Length / 2, hdin.AddrOfPinnedObject(), hdout.AddrOfPinnedObject(), fftw_direction.Forward, fftw_flags.Measure);
                fftw.execute(dplan);
                fftw.destroy_plan(dplan);
                hdin.Free();
                hdout.Free();
                for (int k = 0; k < outData.Length / 2; k++)
                {
                    double dou1 = Math.Pow(outData[k * 2], 2.0);
                    double dou2 = Math.Pow(outData[k * 2 + 1], 2.0);
                    double dou3 = dou1 + dou2;
                    IQdata[k][SmoothIndex] = Math.Pow(dou3, 0.5);
                }
                int Resolution_Offset = (int)(6 * (Math.Log(t.Data.Length, 2) - 10));
                if (t.Type == 2)
                    offset = 133 - 7 + Resolution_Offset;
                else if(t.Type == 1)
                    offset = 133 + 8.63 + Resolution_Offset + (m_DataProcessing.DigitialGain_24db ? 24 : 0) + (m_DataProcessing.LowNoise ? 12 : 0);
            }

            double[] power = null;

            if (t.Type == 3)
            {
                power = new double[outData.Length];
                for (int i = 0; i < outData.Length; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < mm_SmoothNum; j++)
                        sum += IQdata[i][j];
                    sum /= mm_SmoothNum;
                    m_DigitalGain = Convert.ToInt32(t.Digital_Gain[i * 57 / outData.Length]);
                    m_RFGain = Convert.ToInt32(t.RF_Gain[i / outData.Length]);
                    power[i] = sum / 10 - offset + m_DigitalGain + m_RFGain;
                }
            }
            else if (t.Type == 2)
            {
                power = new double[outData.Length / 2];
                for (int i = 0; i < outData.Length / 2; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < mm_SmoothNum; j++)
                        sum += IQdata[i][j];
                    sum /= mm_SmoothNum;
                    m_DigitalGain = Convert.ToInt32(t.Digital_Gain[i * 40 / outData.Length]);
                    m_RFGain = Convert.ToInt32(t.RF_Gain[0]);
                    power[i] = Math.Log10(sum) * 20 - offset + m_DigitalGain + m_RFGain;
                }
            }
            else
            {
                power = new double[outData.Length / 2];
                for (int i = 0; i < outData.Length / 2; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < mm_SmoothNum; j++)
                        sum += IQdata[i][j];
                    sum /= mm_SmoothNum;
                    power[i] = Math.Log10(sum) * 20 - offset;
                }
            }

            //平滑处理
            if (++SmoothIndex >= mm_SmoothNum)
            {
                SmoothIndex = 0;
            }

            //数据传递
            PowerAndFreq nPowerAndFreq = new PowerAndFreq();
            power.Reverse<double>();//倒谱处理
            long powerLength = power.Length / 2;
            if (t.Type == 3)
            {
                nPowerAndFreq.Power = power;
            }
            else
            {
                nPowerAndFreq.Power = new double[power.Length];
                Array.Copy(power, powerLength, nPowerAndFreq.Power, 0, powerLength);
                Array.Copy(power, 0, nPowerAndFreq.Power, powerLength, powerLength);
            }

            switch(t.Type)
            {
                case 1:
                {
                    if(m_DataProcessing.nbddcbandwidth >= 1 && m_DataProcessing.nbddcbandwidth <= 6)
                    {
                        double off = 0.00425 - (m_DataProcessing.nbddcbandwidth - 1) * 10 * 0.00005;
                        nPowerAndFreq.StartFreq = t.StartFreq - off;
                        nPowerAndFreq.StopFreq = t.StopFreq + off;
                    }
                    else if (m_DataProcessing.nbddcbandwidth >= 9 && m_DataProcessing.nbddcbandwidth <= 15)
                    {
                        double off = 0.005 - (m_DataProcessing.nbddcbandwidth - 9) * 0.0005;
                        nPowerAndFreq.StartFreq = t.StartFreq - off;
                        nPowerAndFreq.StopFreq = t.StopFreq + off;
                    }
                    else if (m_DataProcessing.nbddcbandwidth == 30)
                    {
                        nPowerAndFreq.StartFreq = t.StartFreq - 0.0089;
                        nPowerAndFreq.StopFreq = t.StopFreq + 0.0089;
                    }
                    else if (m_DataProcessing.nbddcbandwidth == 50)
                    {
                        nPowerAndFreq.StartFreq = t.StartFreq - 0.0228;
                        nPowerAndFreq.StopFreq = t.StopFreq + 0.0228;
                    }
                    break;
                }
                case 2:
                {
                    nPowerAndFreq.StartFreq = t.StartFreq - 0.07 - 0.640 / power.Length;
                    nPowerAndFreq.StopFreq = t.StopFreq + 0.07;
                    break;
                }
                case 3:
                {
                    nPowerAndFreq.StartFreq = t.StartFreq;
                    nPowerAndFreq.StopFreq = t.StopFreq;
                    break;
                }
            }
            SaveFrequencySpectrum(nPowerAndFreq.Power);
            if (passPowerAndFreq != null)
            {
                passPowerAndFreq(nPowerAndFreq);
            }            
        }

        private void SaveFrequencySpectrum(double[] data)
        {
            if (m_FlagSave)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    sw.WriteLine(data[i].ToString());
                }
                sw.Close();
                m_FlagSave = false;
            }
        }

        /// <summary>
        /// 数据存储路径
        /// </summary>
        private string path;
        /// <summary>
        /// 数据流
        /// </summary>
        private FileStream fs;

        private StreamWriter sw;
        /// <summary>
        /// 数据存储标志位
        /// </summary>
        private bool m_FlagSave = false;

        #region 存储原始IQ数据
        public void StartSave()
        {
            if (m_FlagSave == true)
                return;
            path = @"\...\...\RecvData\new" + DateTime.Now.Date.ToString("yyyy-MM-dd");
            DirectoryInfo fi = new DirectoryInfo(path);
            if (!fi.Exists)
                fi.Create();
            fs = new FileStream(path + "\\" + DateTime.Now.ToString("MM-dd-HH-mm") +  ".txt", FileMode.OpenOrCreate | FileMode.Append);
            sw = new StreamWriter(fs);
            m_FlagSave = true;
        }
        #endregion
    }
}
