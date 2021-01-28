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
        public int millisecond;
        public int microsecond;
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
            this.year = 2000 + ((t[7] >> 2) & 0x3F);
            if ((year % 4 ==0 && year % 100 != 0) || year % 400 == 0)
            {
                this.MONTH_DAYS[1] = 29;
            }
            this.day_offset = ((t[7] & 0x3) << 7) | ((t[8] >> 1) & 0x7F);
            for (this.month = 0; day_offset > 0; ++this.month)
            {
                if (day_offset - MONTH_DAYS[this.month] > 0)
                    day_offset -= MONTH_DAYS[this.month];
                else
                {
                    ++this.month;
                    break;
                }
            }
            this.hour = ((t[8] & 0x1) << 4) | (t[9] >> 4) & 0xF;
            this.minute = ((t[9] & 0xF) << 2)| ((t[10] >> 6) & 0x3);
            this.second = (t[10] & 0x3F);
            this.satelliteInfo = new BD_GPS(t);
            int nanosecond = ((t[11] << 24) + (t[12] << 16) + (t[13] << 8) + t[14]) / 192;
            this.millisecond = nanosecond / 1000;//修改
            this.microsecond = nanosecond % 1000;

            if(this.year > 0 &&
                    this.month >= 1 && this.month <= 12 &&
                    this.day_offset > 0 && this.day_offset <= 31 &&
                    this.hour >= 0 && this.hour <= 24 &&
                    this.minute >= 0 && this.minute <= 60 &&
                    this.second >= 0 && this.second <= 60)
            {
                int AddSecond = t[1] == 2 ? 2:1;
                DateTime dt = new DateTime(year, month, day_offset, hour, minute, second).AddSeconds(AddSecond);
                this.year = dt.Year;
                this.month = dt.Month;
                this.day_offset = dt.Day;
                this.hour = dt.Hour;
                this.minute = dt.Minute;
                this.second = dt.Second;
            }
        }
    }

    public class udpServer
    {
        public delegate void PassInformationr(udpServer sender, byte[] t);
        public event PassInformationr passParameter;//IQ数据的委托事件LX

        public delegate void PassTime(TimeInfo timeInfo, int Type);
        public event PassTime passTime;//时标的委托事件LX

        /// <summary>
        /// 线程安全的队列
        /// </summary>
        AsyncDataQueue<byte[]> m_queue;

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

        public udpServer(string LocalIP, int LocalPort)
        {
            udpInit(LocalIP, LocalPort);
            m_queue = new AsyncDataQueue<byte[]>();//开启一个线程安全的队列LX
            m_queue.TEvent += new THandler<byte[]>(m_queue_TEvent);
        }
        #region 网络连接
        Socket m_RecvSocket;
        public void udpInit(string LocalIP,string[] GroupIP,int GroupPort)
        {
            //实例化一个socket
            m_RecvSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //允许套接字绑定正在使用的地址
            m_RecvSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //组播生存周期
            m_RecvSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
            //超时设置
            m_RecvSocket.ReceiveTimeout = 1000;
            //接收缓存区大小设置
            m_RecvSocket.ReceiveBufferSize = 1024 * 1024 * 1024;
            //绑定本地IP，单网卡可省略此步骤
            IPEndPoint LocalIPEndPoint = new IPEndPoint(IPAddress.Any, GroupPort);
            try
            {
                m_RecvSocket.Bind(LocalIPEndPoint);
            }
            catch (Exception)
            {
                MessageBox.Show("万兆网本地IP地址错误，绑定失败，请重新绑定");
                return;
            }
            //加入组播组
            m_RecvSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(GroupIP[0])));
            m_RecvSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(GroupIP[1])));
            m_RecvSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(GroupIP[2])));
            m_RecvSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(GroupIP[3])));
            m_RecvSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(GroupIP[4])));
            //日志类，写日志到指定路径
            string info = string.Format("分配{0}:{1}成功;接收包计数{2}", GroupIP, GroupPort, RevCount);//200306LX
            LogUtil.WriteAndAuthLogFile(info);        
        }

        public void udpInit(string LocalIP, int LocalPort)
        {
            //实例化一个socket
            m_RecvSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //允许套接字绑定正在使用的地址
            m_RecvSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //组播生存周期
            m_RecvSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
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

            byte[] RecvBuffer = new byte[64 * 1024];//设置接收缓冲区大小LX
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();//初始化异步套接字LX
            args.SetBuffer(RecvBuffer, 0, RecvBuffer.Length);//设置要用于异步套接字方法的数据缓冲区LX
            args.Completed += new EventHandler<SocketAsyncEventArgs>(args_Completed);//完成异步操作的事件LX

            bool rec = m_RecvSocket.ReceiveAsync(args);//开始一个异步请求以便从连接的 System.Net.Sockets.Socket 对象中接收数据LX
            //如果 I/O 操作挂起，将返回 true
            //如果 I/O 操作同步完成，将返回 false
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
                //移交给处理函数
                RecvDataProc(RecvData, nRecvBytes);
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
            #region 丢包计数
            byte[] Index = new byte[4];
            Array.Copy(t, 3, Index, 0, 4);
            Array.Reverse(Index);
            UInt32 curIndex = BitConverter.ToUInt32(Index, 0);
            if (RevCount == 0)
            {
                m_LastPckIndex = curIndex;
            }
            else // 丢包计数
            {
                Int64 lastIndex = m_LastPckIndex;
                if (lastIndex == MaxPackIndex)
                {
                    lastIndex = -1;
                }
                lastIndex++;
                if (lastIndex != curIndex)
                {
                    LostCount++;
                    StringBuilder sbInfo = new StringBuilder();
                    sbInfo.AppendFormat("接收数据包总数{0};丢包数{1};丢包序号{2};", RevCount, LostCount, curIndex);
                    LogUtil.WriteAndAuthLogFile(sbInfo.ToString(), ELogType.Normal, "丢包检测日志");
                }
            }
            m_LastPckIndex = curIndex;
            RevCount++;
            #endregion

            #region 判断底噪是否正确200117LX
            byte[] noise = new byte[1024];

            Array.Copy(t, 32, noise, 0, 1024);
            for (int i = 0; i < 196; i++)
            {
                if (noise[i] != 0 || noise[i] != 1 || noise[i] != 0xFF)//判断底噪是否为0、+1、-1 
                {
                    noisecheckresult[i] = 1;//检查结果数组赋值 0：正常；1：异常
                }
                else
                {
                    noisecheckresult[i] = 0;//检查结果数组赋值 0：正常；1：异常
                }
            }
            #endregion

            if (t != null)
            {
                if (passParameter != null)
                {
                    passParameter(this, t);//执行委托事件
                }
                TimeInfo timeInfo = new TimeInfo(t);
                if (passTime != null)
                {
                    passTime(timeInfo, t[1]);
                }
            }
        }
    }
}
