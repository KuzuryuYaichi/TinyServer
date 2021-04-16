using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Diagnostics;
using Common.ComponentCs.Log;
using Common.ComponentCs;
using System.IO;
using System.Windows.Forms;


namespace XD_DBDW_Server
{
    public class TimeInfo
    {
        private int[] MONTH_DAYS = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        public int year;
        public int month;
        public int day_offset;
        public int hour;
        public int minute;
        public int second;
        public uint millisecond;
        public uint microsecond;
        public BD_GPS satelliteInfo;
        
        public class BD_GPS
        {
            public int year;
            public int month;
            public int day;
            public int hour;
            public int minute;
            public int second;
            public int percentileSecond;
            public int time_state;
            public BD_GPS(byte[] t)
            {
                if(t[1] == 1)//数据类型
                {
                    this.year = 2000 + (t[16] - 0x30) * 10 + (t[17] - 0x30);
                    this.month = (t[18] - 0x30) * 10 + (t[19] - 0x30);
                    this.day = (t[20] - 0x30) * 10 + (t[21] - 0x30);
                    this.hour = (t[22] - 0x30) * 10 + (t[23] - 0x30);//修改去掉小时里的+8
                    this.minute = (t[24] - 0x30) * 10 + (t[25] - 0x30);
                    this.second = (t[26] - 0x30) * 10 + (t[27] - 0x30);
                    this.percentileSecond = (t[28] >> 4) * 10 + (t[29] - 0x30);
                    this.time_state = t[29] & 0x03;
                }
                else
                {
                    this.year = 2000 + (t[36] - 0x30) * 10 + (t[37] - 0x30);
                    this.month = (t[38] - 0x30) * 10 + (t[39] - 0x30);
                    this.day = (t[40] - 0x30) * 10 + (t[41] - 0x30);
                    this.hour = (t[42] - 0x30) * 10 + (t[43] - 0x30);//修改去掉小时里的+8
                    this.minute = (t[44] - 0x30) * 10 + (t[45] - 0x30);
                    this.second = (t[46] - 0x30) * 10 + (t[47] - 0x30);
                    this.percentileSecond = (t[48] >> 4) * 10 + (t[49] - 0x30);
                    this.time_state = t[49] & 0x03;
                }

                if(this.year > 0 &&
                    this.month >= 1 && this.month <= 12 &&
                    this.day > 0 && this.day <= 31 &&
                    this.hour >= 0 && this.hour <= 24 &&
                    this.minute >= 0 && this.minute <= 60 &&
                    this.second >= 0 && this.second <= 60)
                {
                    int AddSecond = t[1] == 2 ? 2 : 1;
                    DateTime dt = new DateTime(year, month, day, hour, minute, second).AddSeconds(AddSecond);
                    this.year = dt.Year;
                    this.month = dt.Month;
                    this.day = dt.Day;
                    this.hour = dt.Hour;
                    this.minute = dt.Minute;
                    this.second = dt.Second;
                }
            }
        }
        public TimeInfo(byte[] t)
        {
            this.year = BitConverter.ToInt16(t, 20);
            this.month = BitConverter.ToInt16(t, 22);
            this.day_offset = BitConverter.ToInt16(t, 26);
            this.hour = BitConverter.ToInt16(t, 28);
            this.minute = BitConverter.ToInt16(t, 30);
            this.second = BitConverter.ToInt16(t, 32);
            this.microsecond = BitConverter.ToUInt32(t, 36);
            this.millisecond = microsecond / 1000;
            this.microsecond %= 1000;
        }
    }

    public class udpRecv
    {
        public delegate void PassInformationr(udpRecv sender, byte[] t);
        public event PassInformationr passParameter;//IQ数据的委托事件LX

        public delegate void PassTime(TimeInfo timeInfo, int Type);
        public event PassTime passTime;//时标的委托事件LX

        /// <summary>
        /// 线程安全的队列
        /// </summary>
        AsyncDataQueue<byte[]> m_queue;
        AsyncDataQueue<byte[]> m_queueSend;

        /// <summary>
        /// 接收数据个数
        /// </summary>
        public UInt64 RevCount { get; set; }
        /// <summary>
        /// 丢包数
        /// </summary>
        public int LostCount { get; set; }
        /// <summary>
        /// 数据包序号
        /// </summary>
        private Int64 m_LastPckIndex;
        /// <summary>
        ///  最大包索引值
        /// </summary>
        private long MaxPackIndex = 4294967296;

        public int[] noisecheckresult = new int[196];

        public udpRecv(string LocalIP, int LocalPort)
        {
            udpInit(LocalIP, LocalPort);
            m_queue = new AsyncDataQueue<byte[]>();//开启一个线程安全的队列LX
            m_queue.TEvent += new THandler<byte[]>(m_queue_TEvent);

            m_queueSend = new AsyncDataQueue<byte[]>();//开启一个线程安全的队列LX
            m_queueSend.TEvent += new THandler<byte[]>(m_queue_Send_TEvent);
        }

        #region 网络连接
        Socket m_RecvSocket;

        public void udpInit(string LocalIP, int LocalPort)
        {
            //实例化一个socket
            m_RecvSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //允许套接字绑定正在使用的地址
            m_RecvSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //超时设置
            m_RecvSocket.ReceiveTimeout = 1000;
            //接收缓存区大小设置
            m_RecvSocket.ReceiveBufferSize = 1024 * 1024 * 1024;
            //绑定本地IP，单网卡可省略此步骤
            IPEndPoint LocalIPEndPoint = new IPEndPoint(IPAddress.Any, LocalPort);
            try
            {
                m_RecvSocket.Bind(LocalIPEndPoint);
            }
            catch (Exception)
            {
                MessageBox.Show("万兆网本地IP地址错误，绑定失败，请重新绑定");
                return;
            }
        }

        /// <summary>
        /// 启动数据接收，异步套接字
        /// </summary>
        /// <returns></returns>
        //数据接收标志,防止重复开启异步套接字
        bool m_bReceiveData = false;

        public bool Start()//启动宽带窄带数据接收
        {
            if (m_bReceiveData == true)
            {
                return false;
            }
            m_bReceiveData = true;

            byte[] RecvBuffer = new byte[64 * 1024];//设置接收缓冲区大小
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();//初始化异步套接字
            args.SetBuffer(RecvBuffer, 0, RecvBuffer.Length);//设置要用于异步套接字方法的数据缓冲区
            args.Completed += new EventHandler<SocketAsyncEventArgs>(args_Completed);//完成异步操作的事件

            bool rec = m_RecvSocket.ReceiveAsync(args);//开始一个异步请求以便从连接的 System.Net.Sockets.Socket 对象中接收数据
            //如果 I/O 操作挂起，将返回 true 如果 I/O 操作同步完成，将返回 false
            if (rec == false)
            {
                args_Completed(m_RecvSocket, args);
            }
            return rec;
        }

        //窄带宽带数据移交给处理函数
        void args_Completed(object sender, SocketAsyncEventArgs e)
        {
            int nRecvBytes = e.BytesTransferred;
            if (nRecvBytes > 0)
            {
                byte[] RecvData = e.Buffer;
                RecvDataProc(RecvData, nRecvBytes); //移交给处理函数
            }
            if (m_bReceiveData == false)
            {
                return;
            }
            try
            {
                bool rec = m_RecvSocket.ReceiveAsync(e);
                if (rec == false)
                {
                    args_Completed(m_RecvSocket, e);
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 停止数据接收
        /// </summary>
        public void Stop()
        {
            m_bReceiveData = false;
            m_RecvSocket.Close();
        }

        public void ClearQueue()
        {
            m_queue.ClearQueue();
        }

        #endregion

        /// <summary>
        /// 接收数据处理
        /// </summary>
        /// <param name="revBytesBuf"></param>
        /// <param name="nRecvDatCnt"></param>
        private void RecvDataProc(byte[] revBytesBuf, int nRecvDatCnt)
        {
            byte[] t = new byte[nRecvDatCnt];
            Buffer.BlockCopy(revBytesBuf, 0, t, 0, nRecvDatCnt);
            m_queue.Enqueue(t);//数据进队列LX
        }

        void m_queue_TEvent(byte[] t)
        {
            if (t != null)
            {
                if (passParameter != null)
                {
                    passParameter(this, t);//执行委托事件
                }
                if(passTime != null)
                {
                    if(t[0] == 0x66 && t[1] == 0x66 && t[2] == 0x66 && t[3] == 0x66)
                        passTime(new TimeInfo(t), 3);
                }
            }
        }

        public void SendOrder(byte[] data)
        {
            m_queueSend.Enqueue(data);
        }

        void m_queue_Send_TEvent(byte[] t)
        {
            if (m_RecvSocket != null)
            {
                m_RecvSocket.SendTo(t, new IPEndPoint(IPAddress.Parse("192.168.1.155"), 9876));
            }
        }
    }
}
