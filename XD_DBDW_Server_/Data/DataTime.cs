using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XD_DBDW_Server
{
    public class DataTime
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        private UInt16 m_year;
        public UInt16 year
        {
            get { return m_year; }
            set { m_year = value; }
        }
        private UInt16 m_month;
        public UInt16 month
        {
            get { return m_month; }
            set { m_month = value; }
        }
        private UInt16 m_day;
        public UInt16 day
        {
            get { return m_day; }
            set { m_day = value; }
        }
        private UInt16 m_hour;
        public UInt16 hour
        {
            get { return m_hour; }
            set { m_hour = value; }
        }
        private UInt16 m_minute;
        public UInt16 minute
        {
            get { return m_minute; }
            set { m_minute = value; }
        }
        private UInt16 m_second;
        public UInt16 second
        {
            get { return m_second; }
            set { m_second = value; }
        }
        private UInt32 m_millisecond;//修改
        public UInt32 millisecond
        {
            get { return m_millisecond; }
            set { m_millisecond = value; }
        }
        private UInt32 m_microsecond;
        public UInt32 microsecond
        {
            get { return m_microsecond; }
            set { m_microsecond = value; }
        }
        /// <summary>
        /// 时间戳纳秒计时器
        /// </summary> 
        /*
        private UInt32 m_nanosecond;
        public UInt32 nanosecond
        {
            get { return m_nanosecond; }
            set { m_nanosecond = value; }
        }
       */
    }

    public class DataAndTime
    {
        private DataTime m_DataTime;
        public DataTime Time
        {
            get { return m_DataTime; }
            set { m_DataTime = value; }
        }
        private byte[] m_data;
        public byte[] Data
        {
            get { return m_data; }
            set { m_data = value; }
        }
    }

    public class DataAndFreq
    {
        public enum NB_DATA_MODE
        {
            NB_MODE_IQ = 0,
            NB_MODE_AM,
            NB_MODE_ISB,
            NB_MODE_CW,
            NB_MODE_USB,
            NB_MODE_LSB
        };

        public DataAndFreq(byte[] t, byte[] Data, int Type, int[] RF_Gain, int[] Digital_Gain)
        {
            this.Type = Type;
            this.Data = Data;
            this.RF_Gain = RF_Gain;//射频增益
            this.Digital_Gain = Digital_Gain;//数字增益
            if (Type == 3)
            {
                this.StartFreq = 1.5;
                this.StopFreq = 30;
            }
            else if (Type == 1)
            {
                this.StartFreq = 1.5;
                this.StopFreq = 30;

                int freq = BitConverter.ToInt32(t, 40) / 1000000;
                int bandWidth = BitConverter.ToInt32(t, 44);
                switch (bandWidth)
                {
                    case 3200://9.6
                    case 6400:
                        {
                            this.StartFreq = freq - 0.0048;
                            this.StopFreq = freq + 0.0048;
                            break;
                        }
                    case 10000://19.2
                    case 15000:
                        {
                            this.StartFreq = freq - 0.0096;
                            this.StopFreq = freq + 0.0096;
                            break;
                        }
                    case 20000://38.4
                    case 25000:
                        {
                            this.StartFreq = freq - 0.0192;
                            this.StopFreq = freq + 0.0192;
                            break;
                        }
                    case 50000://76.8
                        {
                            this.StartFreq = freq - 0.0384;
                            this.StopFreq = freq + 0.0384;
                            break;
                        }
                    default: break;
                }
            }
        }

        public DataAndFreq(byte[] t, byte[] Data, int Type, int[] RF_Gain, int[] Digital_Gain, int NB_DataMode) : this(t, Data, Type, RF_Gain, Digital_Gain)
        {
            this.NB_DataMode = (NB_DATA_MODE)NB_DataMode;
        }

        public int Type;
        public NB_DATA_MODE NB_DataMode;
        public int[] RF_Gain;
        public int[] Digital_Gain;
        public byte[] Data;
        public double[] m_Data;

        private double m_StartFreq;
        public double StartFreq
        {
            get { return m_StartFreq; }
            set { m_StartFreq = value; }
        }
        private double m_StopFreq;
        public double StopFreq
        {
            get { return m_StopFreq; }
            set { m_StopFreq = value; }
        }
    }

    //public class IQDataAndFreq
    //{
    //    public IQDataAndFreq(byte[] t, int Type, int[] RF_Gain, int[] Digital_Gain)
    //    {
    //        this.Type = Type;
    //        this.RF_Gain = RF_Gain;//射频增益
    //        this.Digital_Gain = Digital_Gain;//数字增益
    //        if (Type == 3)
    //        {
    //            this.StartFreq = 1.5;//起始频率
    //            this.StopFreq = 30;//终止频率
    //        }
    //        else if(Type == 1)
    //        {
    //            this.StartFreq = StartFreq;//起始频率
    //            this.StopFreq = StopFreq;//终止频率
    //        }
    //    }

    //    public IQDataAndFreq(byte[] t, int Type, int[] RF_Gain, int[] Digital_Gain, int NB_DataMode): this(t, Type, RF_Gain, Digital_Gain)
    //    {
    //        this.NB_DataMode = NB_DataMode;
    //    }

    //    public int Type;
    //    public int NB_DataMode;
    //    public int[] RF_Gain;
    //    public int[] Digital_Gain;
    //    private double m_StartFreq;
    //    public double StartFreq
    //    {
    //        get { return m_StartFreq; }
    //        set { m_StartFreq = value; }
    //    }
    //    private double m_StopFreq;
    //    public double StopFreq
    //    {
    //        get { return m_StopFreq; }
    //        set { m_StopFreq = value; }
    //    }
    //    private double[] m_data;
    //    public double[] m_Data
    //    {
    //        get { return m_data; }
    //        set { m_data = value; }
    //    }
    //}

    public class PowerAndFreq
    {
        private double m_StartFreq;
        public double StartFreq
        {
            get { return m_StartFreq; }
            set { m_StartFreq = value; }
        }
        private double m_StopFreq;
        public double StopFreq
        {
            get { return m_StopFreq; }
            set { m_StopFreq = value; }
        }
        private double[] m_power;
        public double[] Power
        {
            get { return m_power; }
            set { m_power = value; }
        }
    }

    public class IQData
    {
        private double[] m_Idata;
        public double[] IData
        {
            get { return m_Idata; }
            set { m_Idata = value; }
        }
        private double[] m_Qdata;
        public double[] QData
        {
            get { return m_Qdata; }
            set { m_Qdata = value; }
        }
    }
}
