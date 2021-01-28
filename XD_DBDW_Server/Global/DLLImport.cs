using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SWDirectCollect_Channel8
{
    public class DLLImport
    {
        #region 初始化及千兆网设置
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "InitialzeDevice", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int InitialzeDevice(string DevIP, ushort DevCCPort, ushort DevFCPort, string LocalIP);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "FreeDevice", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int FreeDevice();

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "LocalNetConfig_IP", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int LocalNetConfig_IP(string ip);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DevNetConfig_IP", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int DevNetConfig_IP(string ip);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DevNetConfig_CCPort", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int DevNetConfig_CCPort(ushort CCPort);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DevNetConfig_FCPort", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int DevNetConfig_FCPort(ushort FCPort);
        #endregion

        #region FPGA程序加载
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DevCC_FPGALoad", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int DevCC_FPGALoad(int DevID, string FilePatht);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DevRC_FPGALoad", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int DevRC_FPGALoad(int FlashID, string FilePatht);
        #endregion

        #region

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "RFGainState", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int RFGainState(int DevId, int UpLimit, int DownLimit, int DischargeConst);
        #endregion

        #region 设备状态


        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DigitaGainMode", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int DigitaGainMode(int DevID, int Mode);

        #endregion

        #region 温度信息
        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_HEARTBEAT_RESULT
        {
            public int DevID;//   设备编号
            public int CC_FPGA_State;
            public int CC_AD_State;
            public int CC_Aurora_State;
            public int RF_Voltage_State;
            public int RF_Current_State;
            public float RF_Temperature_State;
            public int RF_Module_State;
            public float CC_Temperature_State;
            public int RC_Net_State;
            public int RC_Aurora_State;
        }


        public delegate int Func(float t);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "RegisterTemperatureCallback", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static void RegisterTemperatureCallback(Func func);
        #endregion

        #region 射频版本V1.8

        #region 版本控制
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "CtrlVersion", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int CtrlVersion(int Version = 2);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "CtrlType", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int CtrlType(int Type = 0);

        //控制程序版本：
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DevDriveID", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int DevDriveID(int ID = 0);
        #endregion

        #region 射频前端控制

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "RFFreqBand", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int RFFreqBand(int RF_DevID, int Channel, float StartFreq, float EndFreq);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "CSFreqBand", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int CSFreqBand(int RF_DevID, float StartFreq, float EndFreq);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "RFAllGainValue", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int RFAllGainValue(int RF_DevID, int Value);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "RFAllWokeMode", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int RFAllWokeMode(int RF_DevID, int Mode);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "RFParamConfig", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int RFParamConfig(int RF_DevID, int Channel, int GainValue, int WorkMode);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "CSAllParamConfig", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int CSAllParamConfig(int RF_DevID, int GainVlaue, int WorkMode);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "CSGainValue", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int CSGainValue(int RF_DevID, int Value);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "CSWorkMode", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int CSWorkMode(int RF_DevID, int Mode);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "GSWorkMode", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int GSWorkMode(int RF_DevID, int Mode);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "RFPowerSwitch", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int RFPowerSwitch(int RF_DevID, int Channel, int PowerSwitch);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "CSPowerSwitch", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int CSPowerSwitch(int RF_DevID, int PowerSwitch);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "GSPowerSwitch", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int GSPowerSwitch(int RF_DevID, int PowerSwitch);


        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "WSWholeStatus", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int WSWholeStatus(int RF_DevID, int GainValue, int RF_WorkMode, int CS_WorkMode, int GS_WorkMode);


        #endregion

        #region 射频前端查询

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_CS_STATUS_RESULT
        {
            public int FreqBandStart;//10KHz
            public int FreqBandEnd;//10KHz
            public int CS_GainValue;
            public int CS_WorkMode;//0校准信号关 1校准信号开
            public int CS_PowerStatus;//0 关闭  1 电源打开
            public int CS_WorkingVoltage;//mv
            public int CS_WorkingCurrent;//mA
            public float CS_Temperature;//摄氏度
        }
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "CSGetDevState", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int CSGetDevState(int RF_DevID, ref DEV_CS_STATUS_RESULT Reslut);

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_GS_STATUS_RESULT
        {
            public int GS_WorkMode; // 0 校正状态  1 天线状态
            public int GS_PowerStatus;//0 关闭  1 电源打开
            public int GS_WorkingVoltage;
            public int GS_WorkingCurrent;
            public float GS_Temperature;
        }
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "GSGetDevState", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GSGetDevState(int RF_DevID, ref DEV_GS_STATUS_RESULT Reslut);

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_RF_IDENTITY_RESULT
        {
            public float RF_BoardVer;   //载板固件版本码  如V10.01 (b1010 0001)
            public float RF_Ver;        //固件版本号  如V10.01 (b1010 0001)
            public int RF_MaxWorkingFreq;  //最大正常工作时钟频率码,MHz
            public int RF_ComName;    //公司名称码  00000000为五十七所，00000001为重庆会凌，00000010为成都中亚，00000011为成都赛狄，00000100位天津754
            public int RF_ModuleType; //模块类型码
            public int RF_BoughtIndex;//模块申购编号码
            public int RF_Addr;       //模块地址码
        }
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "RFGetDevIdentity", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int RFGetDevIdentity(int RF_DevID, int Channel, ref DEV_RF_IDENTITY_RESULT Reslut);

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_CS_IDENTITY_RESULT
        {
            public float CS_BoardVer;   //载板固件版本码  如V10.01 (b1010 0001)
            public float CS_Ver;        //固件版本号  如V10.01 (b1010 0001)
            public int CS_MaxWorkingFreq;  //最大正常工作时钟频率码,MHz
            public int CS_ComName;    //公司名称码  00000000为五十七所，00000001为重庆会凌，00000010为成都中亚，00000011为成都赛狄，00000100位天津754
            public int CS_ModuleType; //模块类型码
            public int CS_BoughtIndex;//模块申购编号码
        }
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "CSGetDevIdentity", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int CSGetDevIdentity(int RF_DevID, ref DEV_CS_IDENTITY_RESULT Reslut);

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_GS_IDENTITY_RESULT
        {
            public float GS_BoardVer;   //载板固件版本码  如V10.01 (b1010 0001)
            public float GS_Ver;        //固件版本号  如V10.01 (b1010 0001)
            public int GS_MaxWorkingFreq;  //最大正常工作时钟频率码,MHz
            public int GS_ComName;    //公司名称码  00000000为五十七所，00000001为重庆会凌，00000010为成都中亚，00000011为成都赛狄，00000100位天津754
            public int GS_ModuleType; //模块类型码
            public int GS_BoughtIndex;//模块申购编号码
        }
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "GSGetDevIdentity", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GSGetDevIdentity(int RF_DevID, ref DEV_GS_IDENTITY_RESULT Reslut);
        #endregion

        #endregion

        #region 万兆网数据传输

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DataDestIP_B", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int DataDestIP_B(int BoardID, int NetID, int connectionId, string ip, ushort disPort, ushort sourcePort);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DataLinkEnable_B", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int DataLinkEnable_B(int BoardID, int NetID, int connectionId, int en);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DataReset_X_B", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int DataReset_X_B(int boardNum, int enable, int dataSource);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DataSourceIP_B", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int DataSourceIP_B(int BoardID, int NetID, string ip, int PacketLen = 8512, int Intervalin = 10, int SlicesClock = 10);//0305LX

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DataAllEnable", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int DataAllEnable(int Enable);
        #endregion

        #region JGZC系列功能

        //01采集板自检200107LX
        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_FPGA_CHECKSELF_RESULT
        {
            public int Status1; //1 FPGA加载正常， 0 FPGA加载失败
            public float Tepmerature; //采集板温度
        }
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DevCC_FPGACheckSelf", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int DevCC_FPGACheckSelf(ref DEV_FPGA_CHECKSELF_RESULT Reslut);

        //传输板自检200107LX
        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_RC_CHECKSELF_RESULT
        {
            public int DevRCVice;//传输板从板状态     1 成功       0 失败                      Status[0]
            public int DevRCNet1;//传输板主板1号万兆网口状态     1 成功       0 失败           Status[1]
            public int DevRCNet2;//传输板主板2号万兆网口状态     1 成功       0 失败           Status[2]
            public int DevRCNet3;//传输板主板3号万兆网口状态     1 成功       0 失败           Status[3]
            public int DevRCNet4;//传输板主板4号万兆网口状态     1 成功       0 失败           Status[4]
            public int DevRCFPGA1;//传输板主板1号FPGA程序加载情况     1 成功       0 失败      Status[5]
            public int DevRCFPGA2;//传输板主板2号FPGA程序加载情况     1 成功       0 失败      Status[6]
        }
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DevRC_CheckSelf", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int DevRC_CheckSelf(ref DEV_RC_CHECKSELF_RESULT Reslut);

        //02射频增益模式控制 200102LX
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "RFGainMode", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int RFGainMode(int Mode);

        //03射频增益模式查询 200102LX
        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_DevRF_GetGainMode_RESULT
        {
            public int Mode; //1 AGC， 0 MGC
        }
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "GetRFGainMode", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int GetRFGainMode(ref DEV_DevRF_GetGainMode_RESULT Reslut);

        //04射频增益值控制指令 200102LX
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "RFGainValue", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int RFGainValue(int Value);

        //05射频工作模式指令 200102LX
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "RFWokeMode", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int RFWokeMode(int Mode);

        //06射频状态信息回读 200102LX
        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_RF_STATUS_RESULT
        {
            public int RF_GainValue;
            public int RF_WorkMode; //0常规 1低噪声
            //public int RF_PowerStatus; //0 关闭  1 电源打开
            //public int RF_WorkingVoltage;	//mv
            //public int RF_WorkingCurrent;  //mA
            //public int RF_Channel;
            public float RF_Temperature;    //摄氏度 45.7 == 4570
        }
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "RFGetDevState", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int RFGetDevState(ref DEV_RF_STATUS_RESULT Reslut);

        //07窄带196路中心频点配置，带宽配置 200102LX
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "NBDDCFreqBand", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int NBDDCFreqBand(int Index, int BandWidth, double Freq);

        //08宽带60路中心频点配置 200102LX
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "WBDDCFreqBand", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int WBDDCFreqBand(int Index, double Freq);

        //09射频AGC参数控制 200102LX
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "RFGainParameter", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int RFGainParameter(int UpLimit, int DownLimit, int HoldTime);

        //10数字AGC参数控制 200102LX
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DigitalGainParameter", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int DigitalGainParameter(int UpLimit, int DownLimit, int HoldTime);

        //11时标切换200102LX
        public struct DEV_TIMEDEV_RESULT
        {
            public int Mode; //0 B码， 1 串码
        }
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "SelectTimeDev", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int SelectTimeDev(int Mode, ref DEV_TIMEDEV_RESULT Reslut);

        //12 GPS/BD类型控制指令200102LX
        public struct DEV_GPSBD_RESULT
        {
            public int Mode; //1 FPGA加载正常， 0 FPGA加载失败
        }
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "SelectGPSBD", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int SelectGPSBD(int Mode, ref DEV_GPSBD_RESULT Reslut);

        //13数字增益24dB开关控制200102LX
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DigitalGainSwitch", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int DigitalGainSwitch(int WorkSwitch);

        //14系统时钟状态查询200102LX
        public struct DEV_CLKStatus_RESULT
        {
            public int Status; //1 有外部时钟输入， 0 无外部时钟输入
        }
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DevCC_SampleClockCheckSelf", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int DevCC_SampleClockCheckSelf(ref DEV_CLKStatus_RESULT Reslut);

        //15Aurora接口channel_up连接状态200102LX
        public struct DEV_CCAURORA_CHECKSELF_RESULT
        {
            public int AuroraX1; //1 正常， 0 异常
            public int AuroraX4_1; //1 正常， 0 异常
            public int AuroraX4_2; //1 正常， 0 异常
        }
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "AuroraStatus", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int AuroraStatus(ref DEV_CCAURORA_CHECKSELF_RESULT Reslut);

        //16FPGA版本号查询采集板200102LX
        public struct DEV_GetFPGAVersion_CHECKSELF_RESULT
        {
            //int DevID;//板卡号
            public int manufacturers;//厂家固定为754
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public int[] sampleK7_Type;//采集板K7设备类型
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public int[] sampleK7_integer;//采集板K7版本号整数
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public int[] sampleK7_decimal;//采集板K7版本号小数


            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public int[] sampleV7_Type;//采集板V7设备类型
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public int[] sampleV7_integer;//采集板V7版本号整数
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public int[] sampleV7_decimal;//采集板V7版本号小数

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] netK7_Type;//传输板K7设备类型
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] netK7_integer;//传输板K7版本号整数
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] netK7_decimal;//传输板V7版本号小数

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] netF1_Type;//传输板K7设备类型
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] netF1_integer;//传输板F1版本号整数
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] netF1_decimal;//传输板F1版本号小数

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] netF2_Type;//传输板K7设备类型
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] netF2_integer;//传输板F2版本号整数
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] netF2_decimal;//传输板F2版本号小数
        }
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "GetFPGAVersion", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int GetFPGAVersion(ref DEV_GetFPGAVersion_CHECKSELF_RESULT Reslut);

        //16FPGA版本号查询传输板200102LX
        public struct DEV_GetFPGAVersionFC_CHECKSELF_RESULT
        {
            //int DevID;//板卡号
            public int manufacturers;//厂家固定为754
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public int[] sampleK7_Type;//采集板K7设备类型
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public int[] sampleK7_integer;//采集板K7版本号整数
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public int[] sampleK7_decimal;//采集板K7版本号小数


            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public int[] sampleV7_Type;//采集板V7设备类型
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public int[] sampleV7_integer;//采集板V7版本号整数
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public int[] sampleV7_decimal;//采集板V7版本号小数

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] netK7_Type;//传输板K7设备类型
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] netK7_integer;//传输板K7版本号整数
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] netK7_decimal;//传输板V7版本号小数

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] netF1_Type;//传输板K7设备类型
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] netF1_integer;//传输板F1版本号整数
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] netF1_decimal;//传输板F1版本号小数

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] netF2_Type;//传输板K7设备类型
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] netF2_integer;//传输板F2版本号整数
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] netF2_decimal;//传输板F2版本号小数
        }
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "GetFPGAVersionFC", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int GetFPGAVersionFC(ref DEV_GetFPGAVersionFC_CHECKSELF_RESULT Reslut);


        //17数字增益模式控制200102LX
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DigitalGainMode", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int DigitalGainMode(int Mode);

        //18数字增益模式查询200102LX
        public struct DEV_GetDigitalGainMode_RESULT
        {
            public int Mode; //1 数字AGC， 0 数字MGC        
        }
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "GetCCGainMode", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int GetCCGainMode(ref DEV_GetDigitalGainMode_RESULT Reslut);

        //19数字MGC衰减值控制200102LX
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DigitalGainValue", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int DigitalGainValue(int Value);

        //21窄带DDC时标精度200102LX
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "NBDDCAccuracy", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int NBDDCAccuracy(int Accuracy);

        //22FFT时标精度200102LX
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "FFTAccuracy", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int FFTAccuracy(int Accuracy);

        //23JGZCAGC增益值查询
        public struct DEV_GetDigitalGainValueJGZC_RESULT
        {

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
            public int[] DigitalGain; //数字增益数组

        }

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "GetDigitalGainValueJGZC", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int GetDigitalGainValueJGZC(int DevID, ref DEV_GetDigitalGainValueJGZC_RESULT Reslut);

        //03远程复位指令
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "ResetDev", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int ResetDev(int DevID = 0);

        //06开机状态查询
        public struct DEV_CHECKSELF_RESULT
        {
            public int DevID;
            public int Status; //1 正常， 0 异常
        }
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "DevCC_DevPowerCheckSelf", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int DevCC_DevPowerCheckSelf(int DevID, ref DEV_CHECKSELF_RESULT Reslut);

        //08射频通信状态查询
        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "CommunityStatus", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int CommunityStatus(int DevID, ref DEV_CHECKSELF_RESULT Reslut);

        //13AGC增益值查询
        public struct DEV_GetDigitalGainValue_RESULT
        {
            public int DevID;//板卡号

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public int[] RFChannelGain; //射频通道增益数组

            public float PathGain;//链路增益

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32 * 30)]//0424修改AGC查询数组大小JGZC
            public int[] DigitalGain; //数字AGC增益数组
        }

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "GetDigitalGainValue", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static int GetDigitalGainValue(int DevID, ref DEV_GetDigitalGainValue_RESULT Reslut);

        #endregion

        #region AD过载报警

        public delegate int Func1(_DEV_ADOverloadAlarm_RESULT m_struct);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "RegisterADOverloadCallback", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static void RegisterADOverloadCallback(Func1 ADOverload);


        public delegate int Func2(_DEV_HEARTBEAT_RESULT m_struct);

        [DllImport("SWDC_DevDrive_Ctrl.dll", EntryPoint = "RegisterHeartBeatCallback", CharSet = CharSet.Ansi,
CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public extern static void RegisterHeartBeatCallback(Func2 HeartBeatLoad);
        #endregion

    }

    public struct _DEV_ADOverloadAlarm_RESULT
    {
        public int DevID;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public int[] alarm; //1 正常， 0 异常

    }
    public struct _DEV_HEARTBEAT_RESULT
    {
        public int DevID;//   设备编号
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public int[] CC_Switch_State;//数字接收机开机状态
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public int[] CC_Clock_State; //系统时钟状态
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public float[] CC_Temperature_State;//数字接收机温度值
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public int[] CC_FPGA_State;//FPGA芯片状态
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public int[] RC_Net_State;//传输板状态
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public int[] RF_Module_State;//射频通道模块状态
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public int[] GS_WorkMode; // 0 校正状态  1 天线状态
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public int[] GS_Status;//0 关闭  1 电源打开
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public float[] RF_Temperature_State;//射频机箱温度
    }
}
