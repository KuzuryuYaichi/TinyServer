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

    public class IQDataAndFreq
    {
        public IQDataAndFreq(double StartFreq, double StopFreq, int Type, int[] RF_Gain, int[] Digital_Gain)
        {
            this.StartFreq = StartFreq;//起始频率
            this.StopFreq = StopFreq;//终止频率
            this.Type = Type;
            this.RF_Gain = RF_Gain;//射频增益
            this.Digital_Gain = Digital_Gain;//数字增益
        }
        public int Type;
        
        public int[] RF_Gain;
        public int[] Digital_Gain;
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
        private double[] m_data;
        public double[] Data
        {
            get { return m_data; }
            set { m_data = value; }
        }
    }

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

    public class DataAndFreq
    {
        public int Type;
        public int[] RF_Gain;
        public int[] Digital_Gain;
        private byte[] m_data;
        public byte[] Data
        {
            get { return m_data; }
            set { m_data = value; }
        }
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
