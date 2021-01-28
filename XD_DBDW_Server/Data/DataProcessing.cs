using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.ComponentCs.Log;
using Common.ComponentCs;


namespace XD_DBDW_Server
{
    public class DataProcessing
    {
        public bool DigitialGain_24db = false;
        public bool LowNoise = false;
        private XmlProcessing XmlProcessing = new XmlProcessing();
        private int m_DataPacket = 4096;
        private int m_ChannelCount = 20;
        private int RF_WorkMode = 0;//初始化值，0常规，1低噪声

        #region UDP底层数据传输
        public udpServer udpServer;

        public int udpServerInit(string LocalIP, int LocalPort)//窄带宽带数据接收与处理
        {
            udpServer = new udpServer(LocalIP, LocalPort);//建立udp连接
            udpServer.passParameter += new XD_DBDW_Server.udpServer.PassInformationr(udpServer_passParameter);//委托
            udpServer.Start();//开始接收数据
            return 0;
        }

        public void udpServerDestroy()
        {
            if(udpServer != null)
            {
                udpServer.Stop();
            }
        }
        #endregion

        #region 通道、分辨率、是否进行性能测试选择、确定当前子带的中心频点和带宽

        // 选择数据包中的子带
        private int m_PacketBandNum = 1;
        public void SelectedBand(int PacketBandNum)
        {
            if (0 < PacketBandNum && PacketBandNum <= 20) 
            {
                m_PacketBandNum = PacketBandNum;
            }
            else if (20 < PacketBandNum && PacketBandNum <= 40)
            {
                m_PacketBandNum = PacketBandNum - 20;
            }
            else if (40 < PacketBandNum && PacketBandNum <= 60)
            {
                m_PacketBandNum = PacketBandNum - 40;
            }
        }

        /// 选择分辨率
        private int ResolutionIndex = 0;
        private int m_Resolution = 1;
        private double[] IQDataResolution, IQDataResolutionOrg;

        public void SelectedResolution(int Resolution)//上位机界面选择分辨率
        {
            m_Resolution = Resolution;
        }

        private int band = 0;//初始化子带号
        private double nbddcfreq = 1.70;//初始化窄带中心频点
        public double nbddcbandwidth = 0.05;//初始化窄带带宽
        private double wbddcfreq = 1.75;//初始化宽带中心频点

        ///选择当前窄带子带的中心频点和带宽LX
        public void Selected_NBBandWidth_Freq(int BandNum, double NBDDCBandwidth, double NBDDCFreq)
        {
            band = BandNum;
            nbddcfreq = NBDDCFreq;
            nbddcbandwidth = NBDDCBandwidth;
        }

        //选择当前宽带子带的中心频点LX
        public void Selected_WBBandWidth_Freq(int BandNum, double WBDDCFreq)
        {
            band = BandNum;
            wbddcfreq = WBDDCFreq;
        }

        //选择当前FFT子带的中心频点LX
        public void Selected_FFTBandWidth_Freq(int BandNum)
        {
            band = BandNum;
        }

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

        Dictionary<int, FFT_Data> dict = new Dictionary<int, FFT_Data>();

        class FFT_Data
        {
            public int count;
            public byte[] data;
            public byte[] RF_Gain;
            public byte[] Digit_Gain;
            public FFT_Data()
            {
                data = new byte[5 * 38400- 38400/4];//修改
                RF_Gain = new byte[5];
                Digit_Gain = new byte[5 * 12];
                count = 0;
            }
            //FFT拼接
            public FFT_Data Add(byte[] t)
            { 
                int channel = t[2];
                RF_Gain[channel] = t[15];
                if(channel < 4)
                {
                    //数字增益拼接
                    for (int i = 0; i < 12; i++)
                    {
                        Digit_Gain[channel * 12 + i] = t[i + 16];
                    }
                    //数据拼接 
                    Array.Copy(t, 96, data, 38400 * channel, 38400);  
                }
                else if(channel == 4)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        Digit_Gain[channel * 12 + i] = t[i + 16];
                    }

                    Array.Copy(t, 96, data, 38400 * channel, 38400 * 3 / 4);
                }
                ++count;
                return count == 5 ? this : null;
            }
        }
        FFT_Data DataProcessFFT(byte[] t)
        {
            FFT_Data fft_data = null;
            int key = (t[3] << 24) | (t[4] << 16) | (t[5] << 8) | t[6];
            if(dict.ContainsKey(key))
            {
                if(t[2] >= 0 && t[2] < 5)
                {
                    fft_data = dict[key].Add(t);
                    if(fft_data != null)
                        dict.Remove(key);
                }
                else
                    dict.Remove(key);
            }
            else
            {
                if (t[2] >= 0 && t[2] < 5)
                {
                    FFT_Data data = new FFT_Data();
                    data.Add(t);
                    dict[key] = data;
                }
            }

            if(dict.Count > 20)
            {
                //int rmKey = dict.ElementAt(dict.Count - 1).Key;
                //dict.Remove(rmKey);
                dict.Clear();
            }
            return fft_data;
        }

        #region 射频控制模式反馈结果       
        public void passCtrlBac_RF(int RF_Mode)
        {
            RF_WorkMode = RF_Mode;//0常规，1低噪声
        }
        #endregion

        #region 解析数据
        
        void udpServer_passParameter(udpServer sender, byte[] t)//委托事件，udpServer类传递的数据LX
        {
            double StartFreq = 0d;//初始化起始频率LX
            double StopFreq = 0d;//初始化终止频率LX
            int Header_DataType = Convert.ToInt32(t[1]);//数据包帧头中的数据类型LX
            
            switch (Header_DataType)
            {
                #region 窄带数据

                case 1://200305LXtemp
                        m_DataPacket = 2048;//每路IQ数据长度LX
                        m_ChannelCount = 1;//每包中的通道数（路数）LX
                        byte[][] ChannelData = new byte[m_ChannelCount][];//存放IQ数据的二维数组LX
                        byte[] Data = new byte[m_DataPacket];
                        for (int i = 0; i < m_ChannelCount; i++)
                        {
                            ChannelData[i] = new byte[m_DataPacket];
                            Buffer.BlockCopy(t, 32 + m_DataPacket * i, ChannelData[i], 0, m_DataPacket);//接收到的数据分通道放入二维数组
                        }
                        if (m_PacketBandNum != 0)
                        {
                            Buffer.BlockCopy(ChannelData[m_ChannelCount - 1], 0, Data, 0, m_DataPacket);//从二位数组中取出选择的一路数据
                        }
                        else
                        {
                            int[][] ChannelIQData = new int[m_DataPacket / 2][];//定义IQ数据二维数组
                            short[] IQData = new short[m_DataPacket / 2];
                            byte[] Temp = new byte[2];
                            for (int i = 0; i < m_DataPacket / 2; i++)
                            {
                                ChannelIQData[i] = new int[20];
                                for (int j = 0; j < m_ChannelCount; j++)
                                {
                                    ChannelIQData[i][j] = (int)BitConverter.ToInt16(ChannelData[j], 2 * i);
                                }
                            }
                            for (int k = 0; k < m_DataPacket / 2; k++)
                            {
                                IQData[k] = (short)ChannelIQData[k].Average();
                                Temp = BitConverter.GetBytes(IQData[k]);
                                Buffer.BlockCopy(Temp, 0, Data, 2 * k, 2);
                            }
                        }
                        byte[] time = new byte[8];//定义时标数组timeLX
                        Buffer.BlockCopy(t, 6, time, 0, 8);//从帧头中取出时标，存放在time数组中LX
                        DataTime datatime = Transform.Transform_DataTime(time);//解析时间戳
                        
                        StartFreq = nbddcfreq - nbddcbandwidth/2000;//窄带起始频率
                        StopFreq = nbddcfreq + nbddcbandwidth/2000;//窄带终止频率
                        DataFunction.PushChannelData(Data, StartFreq, m_PacketBandNum, datatime, new TimeInfo(t));//时标检测

                        DataAndFreq nDataAndFreq = new DataAndFreq();//定义结构体
                        nDataAndFreq.StartFreq = StartFreq;//起始频率
                        nDataAndFreq.StopFreq = StopFreq;//终止频率

                        nDataAndFreq.Data = new byte[m_DataPacket];//定义数据数组
                        Buffer.BlockCopy(Data, 0, nDataAndFreq.Data, 0, m_DataPacket);//数组赋值
                        nDataAndFreq.Type = Header_DataType;

                        m_queue.Enqueue(nDataAndFreq);
                        break;

                #endregion 

                #region 宽带数据
                case 2:                                
                        m_DataPacket = 2048;//每路IQ数据长度LX
                        m_ChannelCount = 20;//每包中的通道数（路数）LX//20200224test
                        ChannelData = new byte[m_ChannelCount][];//存放IQ数据的二维数组LX
                        Data = new byte[m_DataPacket];
                        byte[] Data_temp = new byte[m_DataPacket];
                        for (int i = 0; i < m_ChannelCount; i++)
                        {
                            ChannelData[i] = new byte[m_DataPacket];
                            Buffer.BlockCopy(t, 96 + m_DataPacket * i, ChannelData[i], 0, m_DataPacket);//接收到的数据分通道放入二维数组
                        }
                        if (m_PacketBandNum != 0)
                        {
                            Buffer.BlockCopy(ChannelData[m_PacketBandNum - 1], 0, Data, 0, m_DataPacket);//从二位数组中取出选择的一路数据
                        }
                        else
                        {
                            int[][] ChannelIQData = new int[m_DataPacket / 2][];//定义IQ数据二维数组
                            short[] IQData = new short[m_DataPacket / 2];
                            byte[] Temp = new byte[2];
                            for (int i = 0; i < m_DataPacket / 2; i++)
                            {
                                ChannelIQData[i] = new int[20];
                                for (int j = 0; j < m_ChannelCount; j++)
                                {
                                    ChannelIQData[i][j] = (int)BitConverter.ToInt16(ChannelData[j], 2 * i);
                                }
                            }
                            for (int k = 0; k < m_DataPacket / 2; k++)
                            {
                                IQData[k] = (short)ChannelIQData[k].Average();
                                Temp = BitConverter.GetBytes(IQData[k]);
                                Buffer.BlockCopy(Temp, 0, Data, 2 * k, 2);
                            }
                        }

                        time = new byte[8];//定义时标数组timeLX
                        Buffer.BlockCopy(t, 6, time, 0, 8);//从帧头中取出时标，存放在time数组中LX
                        datatime = Transform.Transform_DataTime(time);//解析时间戳
                        StartFreq = wbddcfreq - 500.0/2000;//宽带起始频率LX
                        StopFreq = wbddcfreq + 500.0/2000;//宽带终止频率LX
                        DataFunction.PushChannelData(Data, StartFreq, m_PacketBandNum, datatime, new TimeInfo(t));//时标检测
                        
                        
                        nDataAndFreq = new DataAndFreq();//定义结构体（起始频率、数据）
                        nDataAndFreq.StartFreq = StartFreq;//起始频率
                        nDataAndFreq.StopFreq = StopFreq;//终止频率
                        nDataAndFreq.Data = new byte[m_DataPacket];//定义数据数组
                        Buffer.BlockCopy(Data, 0, nDataAndFreq.Data, 0, m_DataPacket);//数据数组赋值
                        nDataAndFreq.Type = Header_DataType;

                    
                        nDataAndFreq.RF_Gain = new int[1];//定义射频增益
                        int RF_Gain = Convert.ToInt32(t[15]);
                        if (RF_WorkMode == 0)
                        {
                            nDataAndFreq.RF_Gain[0] = (RF_Gain - 15);//RF_WorkMode：0--常规射频增益
                        }
                        else
                            nDataAndFreq.RF_Gain[0] = (RF_Gain - 25);//RF_WorkMode：1--低噪射频增益
                           
                        nDataAndFreq.Digital_Gain = new int[m_ChannelCount];//定义数字增益
                        for(int i = 0; i < m_ChannelCount;i++)
                        {
                            int Digital_Gain = Convert.ToInt32(t[16+i]);
                            nDataAndFreq.Digital_Gain[i] = (-Digital_Gain);//数字增益赋值
                        }
                        
                        m_queue.Enqueue(nDataAndFreq);//结构体进队列

                        break;
                #endregion

                #region FFT数据
                case 3:
                        FFT_Data fft_Data = DataProcessFFT(t);
                        byte[] fData = fft_Data != null ? fft_Data.data : null;
                        if (fData != null && fft_extract++ == 10)
                        {
                            fft_extract = 0;

                            time = new byte[8];//定义时标数组timeLX
                            Buffer.BlockCopy(t, 6, time, 0, 8);//从帧头中取出时标，存放在time数组中LX
                            datatime = Transform.Transform_DataTime(time);//解析时间戳
                            StartFreq = band * 6 + 1.5;//FFT起始频率
                            StopFreq = band * 6 + 6 + 1.5;//FFT终止频率

                            //Transform.Transform_ToFrequency(band, ref StartFreq);//按照频段对应关系确定该子带对应的起始频率LX
                            DataFunction.PushChannelData(fData, StartFreq, m_PacketBandNum, datatime, new TimeInfo(t));

                            nDataAndFreq = new DataAndFreq();//定义结构体
                            nDataAndFreq.StartFreq = 1.5;//起始频率
                            nDataAndFreq.StopFreq = 30;//终止频率(修改)

                            nDataAndFreq.Data = fData;//定义数据数组
                            nDataAndFreq.Type = Header_DataType;

                            nDataAndFreq.RF_Gain = new int[5];//定义射频增益
                            for (int i = 0; i < 5; i++)
                            {
                                int RF_Gain_FFT = Convert.ToInt32(fft_Data.RF_Gain[i]);
                                if (RF_WorkMode == 0)
                                {
                                    nDataAndFreq.RF_Gain[i] = (RF_Gain_FFT-15);//RF_WorkMode：0--常规射频增益
                                }
                                else
                                    nDataAndFreq.RF_Gain[i] = (RF_Gain_FFT-25);//RF_WorkMode：1--低噪射频增益    
                            }
                        
                            nDataAndFreq.Digital_Gain = new int[12 * 5-3];//定义数字增益
                            for (int j = 0; j < 12*5-3 ; j++)
                            {
                                int Digital_Gain_FFT = Convert.ToInt32(fft_Data.Digit_Gain[j]);
                                nDataAndFreq.Digital_Gain[j] = (-Digital_Gain_FFT);//数字增益赋值
                            }

                            m_queue.Enqueue(nDataAndFreq);
                        }
                        break;
                #endregion

                default:
                    break;
            }
        }
        #endregion

        int fft_extract = 0;

        double[][] Window = new double[7][]
        {
            Hanning.HanningWindow(512), 
            Hanning.HanningWindow(512 * 2), 
            Hanning.HanningWindow(512 * 2 * 2), 
            Hanning.HanningWindow(512 * 2 * 2 * 2), 
            Hanning.HanningWindow(512 * 2 * 2 * 2 * 2), 
            Hanning.HanningWindow(512 * 2 * 2 * 2 * 2 * 2),
            Hanning.HanningWindow(512 * 2 * 2 * 2 * 2 * 2 * 2)
        };

        #region 对队列里的数据进行处理（宽带窄带）
        void m_queue_TEvent(DataAndFreq t)
        {
            int dataLength;
            double[] fftData = null;
            IQDataAndFreq nIQDataAndFreq = null;
            if(t.Type == 3)
            {
                dataLength = t.Data.Length / 4;
                fftData = new double[dataLength];
                for (int i = 0; i < dataLength; ++i)
                {
                    fftData[i] = (double)BitConverter.ToInt32(t.Data, 4 * i);//每4个字节IQ数据转换为一个32位有符号整数的数组LX
                }
                nIQDataAndFreq  = new IQDataAndFreq(t.StartFreq, t.StopFreq, t.Type, t.RF_Gain, t.Digital_Gain);//构造结构体
                nIQDataAndFreq.Data = fftData;
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
                //if(t.Type == 1)
                //{
                //    for (int i = 0; i < dataLength; ++i)
                //    {
                //        IQDataResolutionOrg[BaseIndex + i] = (double)BitConverter.ToInt16(t.Data, 2 * i);//每两个字节IQ数据转换为一个16位有符号整数的数组LX
                //    }
                //}
                //else if (t.Type == 2)
                //{
                    for (int i = 0; i < dataLength / 2; ++i)
                    {
                        IQDataResolutionOrg[BaseIndex + 2 * i] = (double)BitConverter.ToInt16(t.Data, 4 * i + 2);
                        IQDataResolutionOrg[BaseIndex + 2 * i + 1] = (double)BitConverter.ToInt16(t.Data, 4 * i);
                    }
                //}
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

                nIQDataAndFreq = new IQDataAndFreq(t.StartFreq, t.StopFreq, t.Type, t.RF_Gain, t.Digital_Gain);//构造结构体
                nIQDataAndFreq.Data = IQDataResolution;

                ///分发至时域处理类
                DataIQWaveform.PushData(IQDataResolutionOrg);

                //进入此处必然处理下一包 需要为下一包申请指向新空间
                IQDataResolution = new double[Resolution_length];
                IQDataResolutionOrg = new double[Resolution_length];
            }
            
            ///分发至频域处理类
            DataFrequencySpectrum.PushData(nIQDataAndFreq);
        }
        #endregion
    }
}
