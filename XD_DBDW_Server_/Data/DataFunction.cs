using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Common.ComponentCs.Log;
using Common.ComponentCs;

namespace XD_DBDW_Server
{
    public class DataFunction
    {
        /// <summary>
        /// 数据存储路径
        /// </summary>
        private string path;
        /// <summary>
        /// 数据流
        /// </summary>
        private FileStream fs;

        /// <summary>
        /// 数据存储标志位
        /// </summary>
        private bool m_FlagSave = false;
        private bool m_FlagTime = false;
        private bool m_FlagTimeSign = false;

        private UInt32 m_time_count = 0;
        private UInt32 m_time_count1 = 0;

        private double m_StartFreq = 0f;
        private double m_StopFreq = 0f;
        private int m_channel = 0;

        private AsyncDataQueue<DataAndTime> m_queue;
        public delegate void DeleTimeSign(DataTime t);
        public event DeleTimeSign passDeleTimeSign;

        public DataFunction()
        {
            m_queue = new AsyncDataQueue<DataAndTime>();
            m_queue.TEvent += new THandler<DataAndTime>(m_queue_TEvent);
        }

        public void ClearQueue()
        {
            m_queue.ClearQueue();
        }

        bool m_FlagSaveLast = false;

        public void PushChannelData(byte[] t, double StartFreq, int channel, DataTime datatime, TimeInfo timeInfo)
        {
            m_channel = channel;
            m_StartFreq = StartFreq;
            m_StopFreq = m_StartFreq + 0.5;//每个子带的带宽LX

            /*记录文件初始化*/
            if (m_FlagSaveLast == false && m_FlagSave == true)
            {
                string time = timeInfo.year.ToString("d") + "_" + timeInfo.month.ToString("d") + "_" + timeInfo.day_offset.ToString("d") + "_" + timeInfo.hour.ToString("d") + "_" + timeInfo.minute.ToString("d") + "_" + timeInfo.second.ToString("d") + "_" + timeInfo.millisecond.ToString("d") + "_" + timeInfo.microsecond.ToString("d");
                path = @"\...\...\RecvData\";
                DirectoryInfo fi = new DirectoryInfo(path);
                if (!fi.Exists)
                    fi.Create();
                fs = new FileStream(path + @"\" + time + ".dat", FileMode.OpenOrCreate | FileMode.Append);
                m_FlagSaveLast = true;
            }
            else if (m_FlagSaveLast == true && m_FlagSave == false)
            {
                fs.Close();
                m_FlagSaveLast = false;
            }
            if (m_FlagSave == true)
            {
                fs.Write(t, 0, (int)t.Length);
            }

            if (m_FlagTime == true)
            {
                m_time_count++;
                DataAndTime nDataAndTime = new DataAndTime();
                nDataAndTime.Time = datatime;
                nDataAndTime.Data = new byte[(int)t.Length];
                Buffer.BlockCopy(t, 0, nDataAndTime.Data, 0, (int)t.Length);
                m_queue.Enqueue(nDataAndTime);
                if (m_time_count >= 1250)
                {
                    m_time_count = 0;
                    m_FlagTime = false;
                }
            }
            /////////////////////////////////////////////////////////////////
        }

        #region 存储原始IQ数据
        public void StartSave()
        {
            if (m_FlagSave == true)
                return;m_FlagSave = true;
        }

        public void StopSave()
        {
            if (m_FlagSave == false)
                return;
             m_FlagSave = false;        
        }

        #endregion

        #region 时标检测
        //读取队列数据写入时标文件，
        void m_queue_TEvent(DataAndTime t)
        {
            if (m_FlagTimeSign == true)
            {
                Pro_CheckTime(t);
            }
            m_time_count1++;
            fs.Write(t.Data, 0, (int)(t.Data.Length));
            if (m_time_count1 == 1250)
            {
                m_time_count1 = 0;
                fs.Close();
            }
        }
        //
        private void Pro_CheckTime(DataAndTime t)
        {
            short[] IData = new short[512];
            int[] DataDifference = new int[512];

            IData[0] = BitConverter.ToInt16(t.Data, 0);

            for (int i = 2; i < 512; i++)
            {
                IData[i] = BitConverter.ToInt16(t.Data, 4 * i);
                DataDifference[i] = Math.Abs(IData[i] - IData[i - 1]);
                if (DataDifference[i] > 10000 && DataDifference[i - 1] > 6000 && DataDifference[i - 2] > 6000)
                {
                    if (passDeleTimeSign != null)
                    {
                        passDeleTimeSign(t.Time);
                    }
                    m_FlagTimeSign = false;
                    break;
                }
            }


        }
        //创建时标文件存储路径，由“时标检测”按钮控制
        public void StartCheckTime()
        {
            m_FlagTime = true;
            m_FlagTimeSign = true;

            path = @"\...\...\RecvData\new" + DateTime.Now.Date.ToString("yyyy-MM-dd");
            DirectoryInfo fi = new DirectoryInfo(path);
            if (!fi.Exists)
                fi.Create();
            fs = new FileStream(path + "\\" + DateTime.Now.ToString("MM-dd-HH-mm") + "--" + "时标检测" + ".dat", FileMode.OpenOrCreate | FileMode.Append);
        }

        #endregion
    }
}
