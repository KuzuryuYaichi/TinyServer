using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace XD_DBDW_Server
{
    public class UI_Control
    {
        private Transform Transform = new Transform();
        private XmlProcessing XmlProcessing = new XmlProcessing();
        private string LocalIP;
        private string DevIP;
        private ushort CC_Port;
        private ushort RC_Port;
        private int m_BandIndex = 0;
        public UI_Control()
        {
            ReadXml();
        }
        private void ReadXml()
        {
            LocalIP = XmlProcessing.ReadCtrlLocalIP();
            DevIP = XmlProcessing.ReadCtrlDevIP();
            CC_Port = Convert.ToUInt16(XmlProcessing.ReadCC_Port());
            RC_Port = Convert.ToUInt16(XmlProcessing.ReadRC_Port());
            int DevID = Convert.ToInt32(XmlProcessing.ReadDevID());
            switch (DevID)
            {
                case 0:
                    m_BandIndex = 66;
                    break;
                case 2:
                    m_BandIndex = 58;//190729增加一个子带
                    break;
                case 3:
                    m_BandIndex = 58;//190729增加一个子带
                    break;
                case 5:
                    m_BandIndex = 204;//200303JGZC项目LX
                    break;
                default:
                    break;
            }
        }

        //01采集板自检200107LX
        public string DevSelfCheck()
        {
            StringBuilder sbInfo = new StringBuilder();
            DLLImport.DEV_FPGA_CHECKSELF_RESULT CC_Result = new DLLImport.DEV_FPGA_CHECKSELF_RESULT();
            DLLImport.DEV_RC_CHECKSELF_RESULT RC_Result = new DLLImport.DEV_RC_CHECKSELF_RESULT();
            DLLImport.DevRC_CheckSelf(ref RC_Result);
            sbInfo.AppendFormat("\r\n万兆网1号光口状态：{1}\r\n万兆网2号光口状态：{2}"
                                + "\r\n万兆网3号光口状态：{3}\r\n万兆网4号光口状态：{4}"
                                + "\r\n传输板FPGA1加载状态：{5}\r\n传输板FPGA2加载状态：{6}",
                                RC_Result.DevRCVice,
                                RC_Result.DevRCNet1, RC_Result.DevRCNet2, RC_Result.DevRCNet3, RC_Result.DevRCNet4,
                                RC_Result.DevRCFPGA1, RC_Result.DevRCFPGA2);
            int res = 0;
            for (int i = 0; i < 1; i++)
            {
                res = DLLImport.DevCC_FPGACheckSelf(ref CC_Result);
                sbInfo.AppendFormat("\r\n采集板FPGA1加载状态：{0}\r\n采集板温度：{1}", CC_Result.Status1, CC_Result.Tepmerature);
            }
            string Result = sbInfo.ToString();
            return Result;
        }

        public int DevNetCtrlInit()
        {
            int res = DLLImport.InitialzeDevice(DevIP, CC_Port, RC_Port, LocalIP);
            return res;
        }

        //JGZC接收机万兆网口ip初始化
        public int DevNetDataSourceInit()
        {
            int BoardID = 0;
            int NetID = 0;
            BoardID = 0;
            NetID = 3;
            string ip = "10.168.4.103";
            int res = DLLImport.DataSourceIP_B(BoardID, NetID, ip);
            if (res != 0)
                return res;
            Thread.Sleep(1);
            return 0;
        }
        public int DevNetDataDestInit()
        {
            int BoardID = 0;
            int NetID = 0;
            int ConnectID = 0;
            for (int i = 0; i < m_BandIndex; i++)
            {
                Transform.Transform_TryBand(i + 1, ref BoardID, ref NetID, ref ConnectID);
                string ip = "239.1.1." + (i + 1);
                ushort disPort = 400;
                ushort srcPort = Convert.ToUInt16(2300 + i + 1);
                int res = DLLImport.DataDestIP_B(BoardID, NetID, ConnectID, ip, disPort, srcPort);
                if (res != 0)
                    return res;
                Thread.Sleep(1);
            }
            DLLImport.DataAllEnable(1);
            return 0;
        }
        public int DataLinkEnable()
        {
            int BoardID = 0;
            int NetID = 3;
            int Enable = 1;
            for (int i = 0; i < 204; i++)
            {
                int ConnectID = i;
                int res = DLLImport.DataLinkEnable_B(BoardID, NetID, ConnectID, Enable);
                if (res != 0)
                    return res;
                Thread.Sleep(1);
            }
            return 0;
        }
    }
}
