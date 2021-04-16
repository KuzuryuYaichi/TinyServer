using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Common.ComponentCs.Log;
using Common.ComponentCs;
using XD_DBDW_Server.DataAlgorithm;

namespace XD_DBDW_Server
{
    public class FileProcessing
    {
        private AsyncDataQueue<byte[]> m_queue;
        public delegate void IQ_Process(byte[] t);
        private DataProcessing m_DataProcessing;
        
        Form1 form;

        IQ_Process p_IQ_Process;

        public FileProcessing(DataProcessing m_DataProcessing, Form1 form)
        {
            this.m_DataProcessing = m_DataProcessing;
            p_IQ_Process += m_DataProcessing.IQ_Process;
            m_queue = new AsyncDataQueue<byte[]>();//初始化队列
            m_queue.TEvent += new THandler<byte[]>(m_queue_TEvent);
            this.form = form;
        }

        #region UDP底层数据传输

        public udpRecv udpRecvNB, udpRecvFFT, udpRecvOrder;
        public int udpRecvInit(string LocalIP, int LocalPort)//窄带宽带数据接收与处理
        {
            udpRecvNB = new udpRecv(LocalIP, 6543);
            udpRecvNB.passParameter += new XD_DBDW_Server.udpRecv.PassInformationr(udpRecv_passParameter_NB);
            udpRecvNB.Start();

            udpRecvFFT = new udpRecv(LocalIP, 8765);
            udpRecvFFT.passParameter += new XD_DBDW_Server.udpRecv.PassInformationr(udpRecv_passParameter_FFT);
            udpRecvFFT.Start();

            udpRecvOrder = new udpRecv(LocalIP, 9876);
            udpRecvOrder.passParameter += new XD_DBDW_Server.udpRecv.PassInformationr(udpRecv_passParameter_Order);
            udpRecvOrder.Start();

            return 0;
        }

        #endregion

        #region 解析数据

        void udpRecv_passParameter_NB(udpRecv sender, byte[] t)//委托事件，udpRecv类传递的数据
        {
            if (t[0] == 0x66 && t[1] == 0x66 && t[2] == 0x66 && t[3] == 0x66)
            {
                m_queue.Enqueue(t);
            }
        }

        void udpRecv_passParameter_FFT(udpRecv sender, byte[] t)//委托事件，udpRecv类传递的数据
        {
            if (t[0] == 0x66 && t[1] == 0x66 && t[2] == 0x66 && t[3] == 0x66)
            {
                m_queue.Enqueue(t);
            }
        }

        void udpRecv_passParameter_Order(udpRecv sender, byte[] t)//委托事件，udpRecv类传递的数据
        {
            if (t[0] == 0x44 && t[1] == 0x44 && t[2] == 0x44 && t[3] == 0x44 && BitConverter.ToInt32(t, 12) == 0x1000FFFF)
            {
                switch (BitConverter.ToInt32(t, 20))
                {
                    case 0x10000002:
                        {
                            string result = "自检成功!";
                            StringBuilder str = new StringBuilder();
                            ushort status = BitConverter.ToUInt16(t, 26);
                            for (int i = 0; i < 5; i++)
                            {
                                switch (i)
                                {
                                    case 0:
                                        str.Append(" 射频AGC_MGC设置");
                                        break;
                                    case 1:
                                        str.Append(" 射频增益设置");
                                        break;
                                    case 2:
                                        str.Append(" 射频状态查询");
                                        break;
                                    case 3:
                                        str.Append(" PCIE工作");
                                        break;
                                    case 4:
                                        str.Append(" 时钟工作");
                                        break;
                                }
                                if ((status & (1 << i)) != 0)
                                {
                                    str.Append("成功");
                                }
                                else
                                {
                                    str.Append("失败");
                                    result = "自检失败";
                                } 
                            }
                            float temp = BitConverter.ToUInt16(t, 24);
                            str.Append( "设备温度: " + (temp * 503.975f / 4096 - 273.15f) + " ℃");
                            form.SetReturnStatus(result + str.ToString());
                            break;
                        }
                    case 0x10000101:
                        {
                            form.SetReturnStatus("窄带频点配置成功");
                            break;
                        }
                    case 0x10000102:
                        {
                            form.SetReturnStatus("窄带带宽配置成功");
                            break;
                        }
                    case 0x10000103:
                        {
                            form.SetReturnStatus("射频衰减控制成功");
                            break;
                        }
                    case 0x10000302:
                        {
                            form.SetReturnStatus("窄带解调模式配置成功");
                            break;
                        }
                    case 0x10000303:
                        {
                            form.SetReturnStatus("CW拍频值设置成功");
                            break;
                        }
                    case 0x10000ccc:
                        {
                            form.SetReturnStatus("24db开关控制成功");
                            break;
                        }
                    case 0x10000eee:
                        {
                            short temp = (short)((t[26] << 8) | t[27]);
                            form.SetReturnStatus("射频衰减值: " + t[24] + "dBm | 是否馈电: " + ((t[25] == 1) ? "是" : "否") + " | 射频温度: " + ((double)temp * 0.03125) + " ℃");
                            break;
                        }
                    case 0x10000fff:
                        {
                            form.SetReturnStatus("射频增益模式: " + ((t[24] == 1) ? "AGC" : "MGC"));
                            break;
                        }
                    case 0x10000202:
                        {
                            form.SetReturnStatus("FFT点数配置成功");
                            break;
                        }
                    case 0x10000ddd:
                        {
                            form.SetReturnStatus("平滑次数配置成功");
                            break;
                        }
                    default: break;
                }
            }
        }

        void m_queue_TEvent(byte[] t)
        {
            p_IQ_Process(t);
        }

        #endregion
    }
}
