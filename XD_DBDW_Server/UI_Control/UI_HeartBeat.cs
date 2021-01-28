using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Net;
using System.Xml;

namespace XD_DBDW_Server
{
    public partial class UI_HeartBeat : UserControl
    {
        public UI_HeartBeat()
        {
            InitializeComponent();
        }


        #region 15 心跳包
        delegate int DelegateUpDateHeartBeatStatus(_DEV_HEARTBEAT_RESULT Result);

        public int HeartBeat(_DEV_HEARTBEAT_RESULT Result)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new DelegateUpDateHeartBeatStatus(HeartBeat), Result);
                return 1;
            }
            richTextBox1.Text = "\r\n" + "开机状态："
             + "\r\n" + Result.CC_Switch_State[0]
             + "\t" + Result.CC_Switch_State[1]
             + "\t" + Result.CC_Switch_State[2]
             + "\t" + Result.CC_Switch_State[3]
             + "\t" + Result.CC_Switch_State[4]
             + "\r\n" + "系统时钟状态："
             + "\r\n" + Result.CC_Clock_State[0]
             + "\t" + Result.CC_Clock_State[1]
             + "\t" + Result.CC_Clock_State[2]
             + "\t" + Result.CC_Clock_State[3]
             + "\t" + Result.CC_Clock_State[4]
             + "\r\n" + "数字接收机温度："
             + "\r\n" + Result.CC_Temperature_State[0]
             + "\t" + Result.CC_Temperature_State[1]
             + "\t" + Result.CC_Temperature_State[2]
             + "\t" + Result.CC_Temperature_State[3]
             + "\t" + Result.CC_Temperature_State[4]
             + "\r\n" + "FPGA芯片状态："
             + "\r\n" + Result.CC_FPGA_State[0]
             + "\t" + Result.CC_FPGA_State[1]
             + "\t" + Result.CC_FPGA_State[2]
             + "\t" + Result.CC_FPGA_State[3]
             + "\t" + Result.CC_FPGA_State[4]
             + "\r\n" + "传输板状态："
             + "\r\n" + Result.RC_Net_State[0]
             + "\t" + Result.RC_Net_State[1]
             + "\t" + Result.RC_Net_State[2]
             + "\t" + Result.RC_Net_State[3]
             + "\t" + Result.RC_Net_State[4]
             + "\r\n" + "射频通道模块状态："
             + "\r\n" + Result.RF_Module_State[0]
             + "\t" + Result.RF_Module_State[1]
             + "\t" + Result.RF_Module_State[2]
             + "\t" + Result.RF_Module_State[3]
             + "\t" + Result.RF_Module_State[4]
             + "\r\n" + "校正源开关状态："
             + "\r\n" + Result.GS_WorkMode[0]
             + "\t" + Result.GS_WorkMode[1]
             + "\t" + Result.GS_WorkMode[2]
             + "\t" + Result.GS_WorkMode[3]
             + "\t" + Result.GS_WorkMode[4]
             + "\r\n" + "校正源模块："
             + "\r\n" + Result.GS_Status[0]
             + "\t" + Result.GS_Status[1]
             + "\t" + Result.GS_Status[2]
             + "\t" + Result.GS_Status[3]
             + "\t" + Result.GS_Status[4]
             + "\r\n" + "射频机箱温度："
             + "\r\n" + Result.RF_Temperature_State[0]
             + "\t" + Result.RF_Temperature_State[1]
             + "\t" + Result.RF_Temperature_State[2]
             + "\t" + Result.RF_Temperature_State[3]
             + "\t" + Result.RF_Temperature_State[4];
            return 1;
        }
        #endregion

    }
}
