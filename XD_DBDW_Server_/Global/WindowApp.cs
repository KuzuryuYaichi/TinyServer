using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace XD_DBDW_Server
{
    public class WindowApp
    {
        /// <summary>
        /// 重启程序
        /// </summary>
        public static void RestartApplication()
        {
            Application.ExitThread();
            Application.Exit();
            Application.Restart();
            Process.GetCurrentProcess().Kill();
        }
    }

    public class Transform
    {
        private XmlProcessing XmlProcessing = new XmlProcessing();
        private int m_DevID = 0;
        public Transform()
        {

        }

        public bool Transform_TryBand(int BandID, ref int BoardID, ref int NetID, ref int ConnectID)
        {
            switch (m_DevID)
            {
                case 2:
                    TryBand_8(BandID, ref BoardID, ref NetID, ref ConnectID);
                    break;
                case 3:
                    TryBand_18(BandID, ref BoardID, ref NetID, ref ConnectID);
                    break;
                case 0:
                    TryBand_20(BandID, ref BoardID, ref NetID, ref ConnectID);
                    break;
                case 5:
                    TryBand_JGZC(BandID, ref BoardID, ref NetID, ref ConnectID);
                    break;
                default:
                    break;
            }
            return true;

        }

        public bool Transform_ToBand(ref int BandID, int BoardID, int NetID, int ConnectID)
        {
            int NetDevID = 0;
            if (BoardID == 1)
            {
                NetDevID = NetID;
            }
            else
            {
                NetDevID = NetID + 4;
            }

            switch (NetDevID)
            {
                case 1:
                    BandID = ConnectID + 1;
                    break;
                case 2:
                    BandID = ConnectID + 17;
                    break;
                case 3:
                    BandID = ConnectID + 27;
                    break;
                case 4:
                    BandID = ConnectID + 47;
                    break;
                default:
                    break;
            }
            return true;
        }

        //起始频率解析方式
        public bool Transform_ToFrequency(int BandID, ref double StartFrequency)
        {
            switch (m_DevID)
            {
                case 2:
                    ToFrequency8(BandID, ref StartFrequency);
                    break;
                case 3:
                    ToFrequency18(BandID, ref StartFrequency);
                    break;
                case 0:
                    ToFrequency20(BandID, ref StartFrequency);
                    break;
                default:
                    break;
            }
            return true;
        }

        //起始频率解析方式
        public bool Transform_ToFrequencyNBDDC(int BandID, ref double StartFrequency)
        {
            switch (m_DevID)
            {
                case 2:
                    ToFrequency8(BandID, ref StartFrequency);
                    break;
                case 3:
                    ToFrequency18(BandID, ref StartFrequency);
                    break;
                case 0:
                    ToFrequency20(BandID, ref StartFrequency);
                    break;
                default:
                    break;
            }
            return true;
        }

        public bool Transform_FreqToBand(int Freq, ref int Band)
        {
            int r = Convert.ToInt32((Freq - 1.5e6) / 500e3);
            if (r >= 26)
            {
                Band = r + 8;
            }
            else
            {
                Band = r;
            }
            return true;
        }

        public DataTime Transform_DataTime(byte[] t)//时间戳解析方式LX
        {
            DataTime datatime = new DataTime();
            datatime.year = Convert.ToUInt16((t[0] >> 1) & 0xff);
            datatime.month = Convert.ToUInt16(((t[0] & 0x01) << 1) + ((t[1] >> 5) & 0xff));
            datatime.day = Convert.ToUInt16(t[1] & 0x1f);
            datatime.hour = Convert.ToUInt16((t[2] >> 4) & 0xff);
            datatime.minute = Convert.ToUInt16(((t[2] & 0x0f) << 2) + ((t[3] >> 6) & 0xff));
            datatime.second = Convert.ToUInt16(t[3] & 0x3f);

            byte[] Nanosecond = new byte[4];
            Array.Copy(t, 4, Nanosecond, 0, 4);
            Array.Reverse(Nanosecond);
            UInt32 nanosecond = BitConverter.ToUInt32(Nanosecond, 0) * 3;
            datatime.millisecond = Convert.ToUInt32(nanosecond / 1000);
            datatime.microsecond = Convert.ToUInt32(nanosecond % 1000);
            return datatime;
        }

        private bool TryBand_20(int BandID, ref int BoardID, ref int NetID, ref int ConnectID)
        {
            if (BandID < 1 || BandID > 66)
                return false;

            if (BandID > 0 && BandID <= 16)
            {
                BoardID = 1;
                NetID = 1;
                ConnectID = BandID - 1;
            }
            else if (BandID > 16 && BandID <= 26)
            {
                BoardID = 1;
                NetID = 3;
                ConnectID = BandID - 17;
            }
            else if (BandID > 26 && BandID <= 46)
            {
                BoardID = 0;
                NetID = 1;
                ConnectID = BandID - 27;
            }
            else if (BandID > 46 && BandID <= 66)
            {
                BoardID = 0;
                NetID = 3;
                ConnectID = BandID - 47;
            }
            return true;
        }

        private bool TryBand_8(int BandID, ref int BoardID, ref int NetID, ref int ConnectID)
        {
            if (BandID < 1 || BandID > 58)//190729增加一个子带
                return false;

            if (BandID > 0 && BandID <= 8)
            {
                BoardID = 1;
                NetID = 1;
                ConnectID = BandID - 1;
            }
            else if (BandID > 8 && BandID <= 16)
            {
                BoardID = 1;
                NetID = 3;
                ConnectID = BandID - 9;
            }
            else if (BandID > 16 && BandID <= 28)
            {
                BoardID = 1;
                NetID = 1;
                ConnectID = BandID - 9;
            }
            else if (BandID > 28 && BandID <= 58)//190729增加一个子带
            {
                BoardID = 1;
                NetID = 3;
                ConnectID = BandID - 21;
            }
            return true;
        }

        private bool TryBand_18(int BandID, ref int BoardID, ref int NetID, ref int ConnectID)
        {
            if (BandID < 1 || BandID > 58)//190729增加一个子带
                return false;

            if (BandID > 0 && BandID <= 16)
            {
                BoardID = 1;
                NetID = 1;
                ConnectID = BandID - 1;
            }
            else if (BandID > 16 && BandID <= 28)
            {
                BoardID = 1;
                NetID = 3;
                ConnectID = BandID - 17;
            }
            else if (BandID > 28 && BandID <= 43)//190801更改18T子带与光口对应关系
            {
                BoardID = 0;
                NetID = 1;
                ConnectID = BandID - 29;
            }
            else if (BandID > 43 && BandID <= 58)//190729增加一个子带
            {
                BoardID = 0;
                NetID = 3;
                ConnectID = BandID - 44;
            }
            return true;
        }
 
        //200303JGZC子带对应方式LX
        private bool TryBand_JGZC(int BandID, ref int BoardID, ref int NetID, ref int ConnectID)
        {
            if (BandID < 1 || BandID > 204)
                return false;

            if (BandID > 0 && BandID <= 204)
            {
                BoardID = 0;
                NetID = 3;
                ConnectID = BandID - 1;
            }
            return true;
        }

        //起始频率解析方式20T
        private bool ToFrequency20(int BandID, ref double StartFrequency)
        {
            BandID = BandID + 1;
            if (BandID < 1 || BandID > 66)
                return false;

            if (BandID <= 26)
            {
                StartFrequency = (1.5 + 0.5 * (BandID - 1));
                return true;
            }
            else
            {
                StartFrequency = (10.5 + 0.5 * (BandID - 27));
                return true;
            }
        }
        //起始频率解析方式8T
        private bool ToFrequency8(int BandID, ref double StartFrequency)
        {
            BandID = BandID + 1;
            if (BandID < 1 || BandID > 58)//190729增加一个子带
                return false;

            if (BandID > 0 && BandID <= 8)
            {
                StartFrequency = (1.5 + 0.5 * (BandID - 1));
            }
            else if (BandID > 8 && BandID <= 20)
            {
                StartFrequency = (9.5 + 0.5 * (BandID - 9));
            }
            else if (BandID > 20 && BandID <= 28)
            {
                StartFrequency = (5.5 + 0.5 * (BandID - 21));
            }
            else if (BandID > 28 && BandID <= 58)//190729增加一个子带
            {
                StartFrequency = (15.5 + 0.5 * (BandID - 29));
            }
            return true;
        }
        //起始频率解析方式18T
        private bool ToFrequency18(int BandID, ref double StartFrequency)
        {
            BandID = BandID + 1;
            if (BandID < 1 || BandID > 58)//190729增加一个子带
                return false;

            StartFrequency = (1.5 + 0.5 * (BandID - 1));
            return true;
        }
    }
}
