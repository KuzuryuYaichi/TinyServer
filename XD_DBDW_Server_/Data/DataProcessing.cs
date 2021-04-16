using System;
using System.Collections.Generic;
using Common.ComponentCs;

namespace XD_DBDW_Server
{
    public class DataProcessing
    {
        public bool DigitialGain_24db = false;
        public bool LowNoise = false;
        private XmlProcessing XmlProcessing = new XmlProcessing();
        private int m_DataPacket = 4096;

        #region 通道、分辨率、是否进行性能测试选择、确定当前子带的中心频点和带宽

        /// 选择分辨率
        private int ResolutionIndex = 0;
        private int m_Resolution = 1;
        private double[] IQDataResolution, IQDataResolutionOrg;

        public void SelectedResolution(int Resolution)//上位机界面选择分辨率
        {
            m_Resolution = Resolution;
        }

        public double nbddcbandwidth = 0.05;//初始化窄带带宽

        #endregion

        #region 初始化
        public DataFunction DataFunction;
        public DataIQWaveform DataIQWaveform;
        public DataFrequencySpectrum DataFrequencySpectrum;
        private AsyncDataQueue<DataAndFreq> m_queue;
        private Transform Transform = new Transform();

        public DataProcessing()
        {
            IQDataResolution = new double[m_DataPacket / 4];//初始化分辨率变量
            IQDataResolutionOrg = new double[m_DataPacket / 4];//初始化分辨率变量
            m_queue = new AsyncDataQueue<DataAndFreq>();//初始化队列
            m_queue.TEvent += new THandler<DataAndFreq>(m_queue_TEvent);
            DataFrequencySpectrum = new DataFrequencySpectrum(this);//初始化频域数据处理类
            DataIQWaveform = new DataIQWaveform();//初始化时域数据处理类
            DataFunction = new DataFunction();//存储数据、时标检测类
        }

        public void ClearQueue()
        {
            m_queue.ClearQueue();
        }

        #endregion

        class FFT_Data
        {
            static Dictionary<short, FFT_Data> dict = new Dictionary<short, FFT_Data>();
            public bool[] isFill;
            public byte[] data;
            public byte[] RF_Gain;
            public byte[] Digit_Gain;
            static int lastPackLen = 0;

            public FFT_Data(byte[] t)
            {
                int pack_len = BitConverter.ToInt32(t, 66);
                data = new byte[57 * pack_len * 2];
                RF_Gain = new byte[5];
                Digit_Gain = new byte[5 * 12];
                isFill = new bool[6];
            }
            //FFT拼接
            public FFT_Data Add(byte[] t)
            {
                int pack_len = BitConverter.ToInt32(t, 66);
                if(lastPackLen != pack_len)//(重要)记录下上一次的PACK_LEN 以免变更分辨率后遇到和之前分辨率不一样而包序号一致的数据时导致的拼包显示错误
                {
                    lastPackLen = pack_len;
                    dict.Clear();
                    return null;
                }
                int channel = t[12];
                if(channel >= 0 && channel < 6)
                {
                    int srcOffset = 70, dstOffset = 10 * 2 * pack_len * channel;
                    short key = BitConverter.ToInt16(t, 8);
                    if (dict.ContainsKey(key) && (srcOffset + 2 * pack_len > t.Length || dstOffset + 2 * pack_len > data.Length))
                    {
                        dict.Remove(key);
                    }
                    else
                    {
                        if(channel == 5)
                            Array.Copy(t, 70, data, dstOffset, 7 * 2 * pack_len);
                        else
                            Array.Copy(t, 70, data, dstOffset, 10 * 2 * pack_len);
                        isFill[channel] = true;
                    }
                }
                foreach (var f in isFill)
                    if (!f)
                        return null;
                return this;
            }

            public static FFT_Data DataProcessFFT(byte[] t)
            {
                FFT_Data fft_data = null;
                int channel = t[12];
                short key = BitConverter.ToInt16(t, 8);
                if (dict.ContainsKey(key))
                {
                    if (channel >= 0 && channel < 6)
                    {
                        fft_data = dict[key].Add(t);
                        if (fft_data != null)
                            dict.Remove(key);
                    }
                    else
                        dict.Remove(key);
                }
                else
                {
                    if (channel >= 0 && channel < 6)
                    {
                        FFT_Data data = new FFT_Data(t);
                        data.Add(t);
                        dict[key] = data;
                    }
                }
                if (dict.Count > 2000)
                {
                    dict.Clear();
                }
                return fft_data;
            }
        }


        #region 解析数据

        int fft_extract = 0;

        enum DATA_TYPE
        {
            DT_WB_FFT = 0x10001260,
            DT_WB_DDC = 0x10001001,
            DT_NB_DDC = 0x10003001,
            DT_NB_DEMODULATE = 0x10003002,
            DT_WB_RESULT = 0x10005001,
            DT_WB_CX_RESULT = 0x10005002
        };

        private int NB_Filter = 0;
        public void Set_NB_Filter(int channel)
        {
            NB_Filter = channel;
        }

        private int dataType = 0;
        public void SetDataType(int dataType)
        {
            this.dataType = dataType;
        }

        public void IQ_Process(byte[] t)
        {
            if(dataType == 0 && BitConverter.ToInt32(t, 4) == (uint)DATA_TYPE.DT_NB_DDC) //前4字节是ID 屏蔽网络上的非命令包
            {
                int channel = BitConverter.ToInt32(t, 16);
                int DATA_LEN = 512 * 2 * 2;
                if (NB_Filter != channel)
                    return;
                int NB_DataMode = BitConverter.ToInt32(t, 92);
                byte[] fData = new byte[DATA_LEN];
                Array.Copy(t, 104, fData, 0, DATA_LEN);
                //switch ((DataAndFreq.NB_DATA_MODE)NB_DataMode)
                //{
                //    case DataAndFreq.NB_DATA_MODE.NB_MODE_IQ:
                //    case DataAndFreq.NB_DATA_MODE.NB_MODE_ISB:
                //    {
                //        Array.Copy(t, 104, fData, 0, DATA_LEN);
                //        break;
                //    }
                //    case DataAndFreq.NB_DATA_MODE.NB_MODE_AM:
                //    case DataAndFreq.NB_DATA_MODE.NB_MODE_USB:
                //    case DataAndFreq.NB_DATA_MODE.NB_MODE_CW:
                //    case DataAndFreq.NB_DATA_MODE.NB_MODE_LSB:
                //    {
                //        Array.Copy(t, 104, fData, 0, DATA_LEN / 2);
                //        break;
                //    }
                //}
                DataAndFreq nDataAndFreq = new DataAndFreq(t, fData, 1, null, null, NB_DataMode);
                m_queue.Enqueue(nDataAndFreq);
            }
            else if(dataType == 1 && BitConverter.ToInt32(t, 4) == (uint)DATA_TYPE.DT_WB_FFT)
            {
                FFT_Data fft_Data = FFT_Data.DataProcessFFT(t);
                byte[] fData = fft_Data?.data;
                if (fData != null && fft_extract++ == 1)
                {
                    fft_extract = 0;
                    DataAndFreq nDataAndFreq = new DataAndFreq(t, fData, 3, null, null);
                    m_queue.Enqueue(nDataAndFreq);
                }
            }
        }
        
        #endregion

        double[][] Window = new double[7][]
        {
            Hanning.HanningWindow(256 * 2),
            Hanning.HanningWindow(256 * 2 * 2),
            Hanning.HanningWindow(256 * 2 * 2 * 2),
            Hanning.HanningWindow(256 * 2 * 2 * 2 * 2),
            Hanning.HanningWindow(256 * 2 * 2 * 2 * 2 * 2),
            Hanning.HanningWindow(256 * 2 * 2 * 2 * 2 * 2 * 2),
            Hanning.HanningWindow(256 * 2 * 2 * 2 * 2 * 2 * 2 * 2)
        };

        #region 对队列里的数据进行处理（窄带）
        void m_queue_TEvent(DataAndFreq t)
        {
            int dataLength;
            double[] fftData;
            if (t.Type == 3)
            {
                dataLength = t.Data.Length / 2;
                fftData = new double[dataLength];
                for (int i = 0; i < dataLength; ++i)
                {
                    fftData[i] = (double)BitConverter.ToInt16(t.Data, 2 * i);//每2个字节IQ数据转换为一个32位有符号整数的数组LX
                }
                t.m_Data = fftData;
            }
            else
            {
                dataLength = t.Data.Length / 2;
                //(重要)保持多线程数据一致行所做的必要处理
                int mm_Resolution = m_Resolution, Resolution_length = mm_Resolution * 1024;

                if (Resolution_length != IQDataResolution.Length)
                {
                    ResolutionIndex = 0;
                    IQDataResolution = new double[Resolution_length];
                    IQDataResolutionOrg = new double[Resolution_length];
                }

                int BaseIndex = dataLength * ResolutionIndex;

                //for (int i = 0; i < dataLength; ++i)
                //{
                //    IQDataResolutionOrg[BaseIndex + i] = (double)BitConverter.ToInt16(t.Data, 2 * i);
                //}

                switch (t.NB_DataMode)
                {
                    case DataAndFreq.NB_DATA_MODE.NB_MODE_IQ:
                    case DataAndFreq.NB_DATA_MODE.NB_MODE_ISB:
                        {
                            for (int k = 0; k < dataLength; ++k)
                            {
                                IQDataResolutionOrg[BaseIndex + k] = (double)BitConverter.ToInt16(t.Data, 2 * k);
                            }
                            break;
                        }
                    case DataAndFreq.NB_DATA_MODE.NB_MODE_AM:
                    case DataAndFreq.NB_DATA_MODE.NB_MODE_USB:
                    case DataAndFreq.NB_DATA_MODE.NB_MODE_CW:
                        {
                            for (int k = dataLength / 2 - 1; k >= 0; k--)
                            {
                                IQDataResolutionOrg[2 * k] = (double)BitConverter.ToInt16(t.Data, 2 * k);
                            }
                            break;
                        }
                    case DataAndFreq.NB_DATA_MODE.NB_MODE_LSB:
                        {
                            for (int k = dataLength / 2 - 1; k >= 0; k--)
                            {
                                IQDataResolutionOrg[2 * k + 1] = (double)BitConverter.ToInt16(t.Data, 2 * k);
                            }
                            break;
                        }
                    default: break;
                }

                if (++ResolutionIndex < mm_Resolution)//凑包个数计数
                    return;
                ResolutionIndex = 0;

                //对不同分辨率(长度)的数据进行加窗处理
                int mm_Resolution_Index = (int)Math.Log(mm_Resolution, 2);
                for (int i = 0; i < IQDataResolution.Length / 2; i++)
                {
                    IQDataResolution[2 * i] = IQDataResolutionOrg[2 * i] * Window[mm_Resolution_Index][i];
                    IQDataResolution[2 * i + 1] = IQDataResolutionOrg[2 * i + 1] * Window[mm_Resolution_Index][i];
                }
                t.m_Data = IQDataResolution;

                ///分发至时域处理类
                DataIQWaveform.PushData(IQDataResolutionOrg);

                //进入此处必然处理下一包 需要为下一包申请指向新空间
                IQDataResolution = new double[Resolution_length];
                IQDataResolutionOrg = new double[Resolution_length];
            }
            ///分发至频域处理类
            DataFrequencySpectrum.PushData(t);
        }
        #endregion
    }
}
