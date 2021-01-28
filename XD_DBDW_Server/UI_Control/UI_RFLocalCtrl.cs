using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using DevExpress.XtraEditors;

namespace XD_DBDW_Server
{
    public partial class UI_RFLocalCtrl : DevExpress.XtraEditors.XtraUserControl
    {
        public class NBDDC//定义窄带频点带宽类
        {
            public int NBDDCIndex;//窄带子带序号
            public double[] NBDDCFreq = new double[196];//196路中心频点数组
            public int NBDDCBandWidthindex;//千兆网DLL下发的中心频点序号
            public double[] NBDDCBandWidth = new double[196];//196路带宽数组
        }

        public class WBDDC//定义宽带频点类
        {
            public int WBDDCIndex;//宽带子带序号
            public int WBDDCBandWidthindex;//196路中心频点数组序号
            public double[] WBDDCFreq = new double[60];//60路中心频点数组
        }

        DataProcessing m_DataProcessing;
        XmlProcessing XmlProcessing = new XmlProcessing();
        public NBDDC nbddc = new NBDDC();//实例化窄带频点带宽类
        public WBDDC wbddc = new WBDDC();//实例化宽带频点类

        public UI_RFLocalCtrl(DataProcessing m_DataProcessing)
        {
            InitializeComponent();
            this.m_DataProcessing = m_DataProcessing;
            int Version = Convert.ToInt32(XmlProcessing.ReadRFCtrlVersion());
            DLLImport.CtrlVersion(Version);
            int CtrlType = Convert.ToInt32(XmlProcessing.ReadRFRFCtrlType());
            DLLImport.CtrlType(CtrlType);
            int ID = Convert.ToInt32(XmlProcessing.ReadDevID());
            DLLImport.DevDriveID(ID);

            DLLImport.RFGainValue(0);
            DLLImport.DigitalGainSwitch(0);
            DLLImport.RFWokeMode(0);
            for (int i = 0; i < 196; i++)
            {
                nbddc.NBDDCFreq[i] = 1.70;//窄带中心频点初始化
                nbddc.NBDDCBandWidth[i] = 0.05;//窄带带宽初始化
            }

            for (int i = 0; i < 60; i++)
            {
                wbddc.WBDDCFreq[i] = 1.75 + i * 0.5;//宽带中心频点初始化
            }

            this.passCtrlBack_RF += new UI_RFLocalCtrl.PassCtrlBack_RF(this.m_DataProcessing.passCtrlBac_RF);//射频控制模式
        }

        #region 信息传递
        /// <summary>
        /// 窄带配置
        /// </summary>
        /// <param name="t"></param>
        public delegate void NBChangedCallBack(int BandNum, double NBDDCBandwidth, double NBDDCFreq);
        public event NBChangedCallBack nbChangedCallBack;

        /// <summary>
        /// 宽带配置
        /// </summary>
        /// <param name="t"></param>
        public delegate void WBChangedCallBack(int BandNum, double WBDDCFreq);
        public event WBChangedCallBack wbChangedCallBack;


        /// <summary>
        /// 控制反馈
        /// </summary>
        /// <param name="t"></param>
        public delegate void PassCtrlBack(int t);
        public event PassCtrlBack passCtrlBack;

        public delegate void PassCtrlBack_RF(int t);
        public event PassCtrlBack_RF passCtrlBack_RF;

        /// <summary>
        /// 通道状态查询
        /// </summary>
        /// <param name="t"></param>
        public delegate void PassRFDevState(DLLImport.DEV_RF_STATUS_RESULT t);
        public event PassRFDevState passRFDevState;

        /// <summary>
        /// 校正源状态查询
        /// </summary>
        /// <param name="t"></param>
        public delegate void PassCSDevState(DLLImport.DEV_CS_STATUS_RESULT t);
        public event PassCSDevState passCSDevState;

        /// <summary>
        /// 测向开关状态查询
        /// </summary>
        /// <param name="t"></param>
        public delegate void PassGSDevState(DLLImport.DEV_GS_STATUS_RESULT t);
        public event PassGSDevState passGSDevState;

        /// <summary>
        /// 通道身份查询
        /// </summary>
        /// <param name="t"></param>
        public delegate void PassRFDevIdentity(DLLImport.DEV_RF_IDENTITY_RESULT t);
        public event PassRFDevIdentity passRFDevIdentity;

        /// <summary>
        /// 校正源身份查询
        /// </summary>
        /// <param name="t"></param>
        public delegate void PassCSDevIdentity(DLLImport.DEV_CS_IDENTITY_RESULT t);
        public event PassCSDevIdentity passCSDevIdentity;

        /// <summary>
        /// 测向开关身份查询
        /// </summary>
        /// <param name="t"></param>
        public delegate void PassGSDevIdentity(DLLImport.DEV_GS_IDENTITY_RESULT t);
        public event PassGSDevIdentity passGSDevIdentity;

        /// <summary>
        /// 数字增益开关
        /// </summary>
        /// <param name="t"></param>
        public delegate void PassDigitaGainMode(int t);
        public event PassDigitaGainMode passDigitaGainMode;

        /// <summary>
        /// 接收机增益模式查询
        /// </summary>
        /// <param name="t"></param>
        public delegate void PassRFGainModeState(DLLImport.DEV_DevRF_GetGainMode_RESULT t);
        public event PassRFGainModeState passRFGainModeState;

        /// <summary>
        /// 开机状态查询
        /// </summary>
        /// <param name="t"></param>
        public delegate void PassDevPowerCheckSelf(DLLImport.DEV_CHECKSELF_RESULT t);
        public event PassDevPowerCheckSelf passDevPowerCheckSelf;

        /// <summary>
        /// 系统时钟状态查询
        /// </summary>
        /// <param name="t"></param>
        public delegate void PassSampleClockCheckSelf(DLLImport.DEV_CLKStatus_RESULT t);
        public event PassSampleClockCheckSelf passSampleClockCheckSelf;

        /// <summary>
        /// 射频通信状态查询
        /// </summary>
        /// <param name="t"></param>
        public delegate void PassCommunityStatus(DLLImport.DEV_CHECKSELF_RESULT t);
        public event PassCommunityStatus passCommunityStatus;

        /// <summary>
        /// 采集板aurora状态查询
        /// </summary>
        /// <param name="t"></param>
        public delegate void PassAuroraStatus(DLLImport.DEV_CCAURORA_CHECKSELF_RESULT t);
        public event PassAuroraStatus passAuroraStatus;

        /// <summary>
        /// 时统模式查询
        /// </summary>
        /// <param name="t"></param>
        public delegate void PassTimeDev(DLLImport.DEV_TIMEDEV_RESULT t);
        public event PassTimeDev passTimeDev;
        
        /// <summary>
        /// GPS/BD模式查询
        /// </summary>
        /// <param name="t"></param>
        public delegate void PassGPSBD(DLLImport.DEV_GPSBD_RESULT t);
        public event PassGPSBD passGPSBD;

        /// <summary>
        /// 数字增益模式查询
        /// </summary>
        /// <param name="t"></param>
        public delegate void PassGetCCGainMode(DLLImport.DEV_GetDigitalGainMode_RESULT t);
        public event PassGetCCGainMode passGetCCGainMode;

        /// <summary>
        /// 窄带带宽中心频点委托200117LX
        /// </summary>
        /// <param name="t"></param>
        public delegate void PassNBDDCBandWidthFreq(UI_RFLocalCtrl.NBDDC t);
        public event PassNBDDCBandWidthFreq passnbddcBandWidthFreq;
        #endregion
        
        //#region 射频控制
        //private void simpleButton13_Click(object sender, EventArgs e)
        //{
        //    int RF_DevID = Convert.ToInt32(comboBox11.Text);
        //    int GSWorkMode = comboBox13.SelectedIndex;
        //    int ret = DLLImport.GSWorkMode(RF_DevID, GSWorkMode);
        //    if (passCtrlBack != null)
        //    {
        //        passCtrlBack(ret);
        //    }
        //}

        //private void simpleButton4_Click(object sender, EventArgs e)
        //{
        //    int RF_DevID = Convert.ToInt32(comboBox11.Text);
        //    int RFWorkMode = comboBox1.SelectedIndex;
        //    int ret = DLLImport.RFAllWokeMode(RF_DevID, RFWorkMode);
        //    if (passCtrlBack != null)
        //    {
        //        passCtrlBack(ret);
        //    }
        //}


        ////private void simpleButton5_Click(object sender, EventArgs e)
        ////{
        ////    int RF_DevID = Convert.ToInt32(comboBox11.Text);
        ////    int RFGainValue = comboBox5.SelectedIndex;
        ////    int ret = DLLImport.RFAllGainValue(RF_DevID,RFGainValue);
        ////    if (passCtrlBack != null)
        ////    {
        ////        passCtrlBack(ret);
        ////    }
        ////}

        //private void simpleButton2_Click(object sender, EventArgs e)
        //{
        //    int RF_DevID = Convert.ToInt32(comboBox11.Text);
        //    int CSWorkMode = comboBox3.SelectedIndex;
        //    int ret = DLLImport.CSWorkMode(RF_DevID, CSWorkMode);
        //    if (passCtrlBack != null)
        //    {
        //        passCtrlBack(ret);
        //    }
        //}

        //private void simpleButton1_Click(object sender, EventArgs e)
        //{
        //    int RF_DevID = Convert.ToInt32(comboBox11.Text);
        //    int CSGainValue = comboBox2.SelectedIndex;
        //    int ret = DLLImport.CSGainValue(RF_DevID, CSGainValue);
        //    if (passCtrlBack != null)
        //    {
        //        passCtrlBack(ret);
        //    }
        //}

        //private void simpleButton22_Click(object sender, EventArgs e)
        //{
        //    int RF_DevID = Convert.ToInt32(comboBox11.Text);
        //    int GSWorkMode = comboBox13.SelectedIndex;
        //    int RFWorkMode = comboBox1.SelectedIndex;
        //    int RFGainValue = comboBox5.SelectedIndex;
        //    int CSWorkMode = comboBox3.SelectedIndex;
        //    int ret = DLLImport.WSWholeStatus(RF_DevID, RFGainValue, RFWorkMode, CSWorkMode, GSWorkMode);
        //    if (passCtrlBack != null)
        //    {
        //        passCtrlBack(ret);
        //    }
        //}

        //#endregion

        /*#region 射频查询

        private void simpleButton18_Click(object sender, EventArgs e)
        {
            int RF_DevID = Convert.ToInt32(comboBox11.Text);
            DLLImport.DEV_CS_STATUS_RESULT Result = new DLLImport.DEV_CS_STATUS_RESULT();
            int res = DLLImport.CSGetDevState(RF_DevID, ref Result);
            if (passCSDevState != null)
            {
                passCSDevState(Result);
            }
        }

        private void simpleButton17_Click(object sender, EventArgs e)
        {
            int RF_DevID = Convert.ToInt32(comboBox11.Text);
            DLLImport.DEV_GS_STATUS_RESULT Result = new DLLImport.DEV_GS_STATUS_RESULT();
            int res = DLLImport.GSGetDevState(RF_DevID, ref Result);
            if (passGSDevState != null)
            {
                passGSDevState(Result);
            }
        }

        private void simpleButton14_Click(object sender, EventArgs e)
        {
            int RF_DevID = Convert.ToInt32(comboBox11.Text);
            int RF_Channel = Convert.ToInt32(comboBox10.Text);
            DLLImport.DEV_RF_IDENTITY_RESULT Result = new DLLImport.DEV_RF_IDENTITY_RESULT();
            int res = DLLImport.RFGetDevIdentity(RF_DevID, RF_Channel, ref Result);
            if (passRFDevIdentity != null)
            {
                passRFDevIdentity(Result);
            }
        }

        private void simpleButton15_Click(object sender, EventArgs e)
        {
            int RF_DevID = Convert.ToInt32(comboBox11.Text);
            DLLImport.DEV_CS_IDENTITY_RESULT Result = new DLLImport.DEV_CS_IDENTITY_RESULT();
            int res = DLLImport.CSGetDevIdentity(RF_DevID, ref Result);
            if (passCSDevIdentity != null)
            {
                passCSDevIdentity(Result);
            }
        }

        private void simpleButton16_Click(object sender, EventArgs e)
        {
            int RF_DevID = Convert.ToInt32(comboBox11.Text);
            DLLImport.DEV_GS_IDENTITY_RESULT Result = new DLLImport.DEV_GS_IDENTITY_RESULT();
            int res = DLLImport.GSGetDevIdentity(RF_DevID, ref Result);
            if (passGSDevIdentity != null)
            {
                passGSDevIdentity(Result);
            }
        }
        #endregion*/

        /*#region 电源控制
        private void simpleButton7_Click(object sender, EventArgs e)
        {
            int RF_DevID = Convert.ToInt32(comboBox11.Text);
            int RF_Channel = Convert.ToInt32(comboBox10.Text);
            int RFPowerSwitch = comboBox7.SelectedIndex;
            int ret = DLLImport.RFPowerSwitch(RF_DevID, RF_Channel, RFPowerSwitch);
            if (passCtrlBack != null)
            {
                passCtrlBack(ret);
            }
        }

        private void simpleButton8_Click(object sender, EventArgs e)
        {
            int RF_DevID = Convert.ToInt32(comboBox11.Text);
            int CSPowerSwitch = comboBox8.SelectedIndex;
            int ret = DLLImport.CSPowerSwitch(RF_DevID, CSPowerSwitch);
            if (passCtrlBack != null)
            {
                passCtrlBack(ret);
            }
        }

        private void simpleButton9_Click(object sender, EventArgs e)
        {
            int RF_DevID = Convert.ToInt32(comboBox11.Text);
            int GSPowerSwitch = comboBox9.SelectedIndex;
            int ret = DLLImport.GSPowerSwitch(RF_DevID, GSPowerSwitch);
            if (passCtrlBack != null)
            {
                passCtrlBack(ret);
            }
        }
        #endregion

        private void simpleButton11_Click(object sender, EventArgs e)
        {
            int RF_DevID = Convert.ToInt32(comboBox11.Text);
            int RF_Channel = Convert.ToInt32(comboBox10.Text);
            float StartFreq = Convert.ToSingle(textBox2.Text);
            float StopFreq = Convert.ToSingle(textBox3.Text);

            int ret = DLLImport.RFFreqBand(RF_DevID, RF_Channel, StartFreq, StopFreq);
            if (passCtrlBack != null)
            {
                passCtrlBack(ret);
            }
        }

        private void simpleButton23_Click(object sender, EventArgs e)
        {
            int RF_DevID = Convert.ToInt32(comboBox11.Text);
            float StartFreq = Convert.ToSingle(textBox5.Text);
            float StopFreq = Convert.ToSingle(textBox4.Text);

            int ret = DLLImport.CSFreqBand(RF_DevID, StartFreq, StopFreq);
            if (passCtrlBack != null)
            {
                passCtrlBack(ret);
            }
        }

        private void simpleButton4_Click_1(object sender, EventArgs e)
        {
            int type = comboBox6.SelectedIndex;

            int ret = DLLImport.CtrlType(type);
            if (passCtrlBack != null)
            {
                passCtrlBack(ret);
            }
        }

        ////03远程复位指令
        //private void button18_Click(object sender, EventArgs e)
        //{
        //    int DevID = comboBox20.SelectedIndex;
        //    int ret = DLLImport.ResetDev(DevID);

        //    if (passCtrlBack != null)
        //    {
        //        passCtrlBack(ret);
        //    }
        //}*/

        #region JGZC新增功能

        //02射频增益模式控制 200102LX
        private void simpleButton12_Click(object sender, EventArgs e)
        {
            int GainMode = comboBox12.SelectedIndex;
            int ret = DLLImport.RFGainMode(GainMode);
            if (passCtrlBack != null)
            {
                passCtrlBack(ret);
            }
        }

        //03射频增益模式查询 200102LX
        private void simpleButton21_Click(object sender, EventArgs e)
        {
            DLLImport.DEV_DevRF_GetGainMode_RESULT Result = new DLLImport.DEV_DevRF_GetGainMode_RESULT();
            DLLImport.GetRFGainMode(ref Result);
            if (passRFGainModeState != null)
            {
                passRFGainModeState(Result);
            }
        }

        //04射频增益值控制指令 200102LX
        private void simpleButton6_Click(object sender, EventArgs e)
        {
            int Value = comboBox5.SelectedIndex;
            int ret = DLLImport.RFGainValue(Value);
            if (passCtrlBack != null)
            {
                passCtrlBack(ret);
            }
        }

        //05射频工作模式指令 200102LX
        private void simpleButton3_Click(object sender, EventArgs e)
        {
            int Mode = comboBox3.SelectedIndex;
            if (passCtrlBack_RF != null)
            {
                passCtrlBack_RF(Mode);
            }
            int ret = DLLImport.RFWokeMode(Mode);
            if (passCtrlBack != null)
            {
                passCtrlBack(ret);
            }
            m_DataProcessing.LowNoise = Mode == 1 ? true : false;
        }

        //06射频状态信息回读 200102LX
        private void simpleButton19_Click(object sender, EventArgs e)
        {
            DLLImport.DEV_RF_STATUS_RESULT Result = new DLLImport.DEV_RF_STATUS_RESULT();
            int res = DLLImport.RFGetDevState(ref Result);
            if (passRFDevState != null)
            {
                passRFDevState(Result);
            }
        }

        public delegate void SaveEvent();
        public event SaveEvent StartSave;
        public event SaveEvent StopSave;

        //07窄带196路中心频点配置，带宽配置 200102LX
        private void button6_Click(object sender, EventArgs e)
        {
            nbddc.NBDDCIndex = Convert.ToInt32(textBox7.Text);//选定子带号
            nbddc.NBDDCBandWidthindex = comboBox29.SelectedIndex;//带宽序号0~13（千兆网DLL下发）
            nbddc.NBDDCFreq[nbddc.NBDDCIndex] = (comboBox4.SelectedIndex + 1) + Convert.ToDouble(comboBox14.SelectedIndex) / 10 + Convert.ToDouble(comboBox1.SelectedIndex) / 100;//给选定子带的中心频点数组赋值
            nbddc.NBDDCBandWidth[nbddc.NBDDCIndex] = Convert.ToDouble(comboBox29.Text);//给选定子带的带宽数组赋值
            int ret = DLLImport.NBDDCFreqBand(nbddc.NBDDCIndex, nbddc.NBDDCBandWidthindex, nbddc.NBDDCFreq[nbddc.NBDDCIndex] * 10);

            if (passCtrlBack != null)
            {
                passCtrlBack(ret);
            }

            if (nbChangedCallBack != null)
            {
                nbChangedCallBack(nbddc.NBDDCIndex, nbddc.NBDDCBandWidth[nbddc.NBDDCIndex], nbddc.NBDDCFreq[nbddc.NBDDCIndex]);
            }

        }

        //08宽带60路中心频点配置 200102LX
        private void button7_Click(object sender, EventArgs e)
        {
            wbddc.WBDDCIndex = Convert.ToInt32(textBox9.Text);//选定子带号
            wbddc.WBDDCFreq[wbddc.WBDDCIndex] = (comboBox16.SelectedIndex + 1) + Convert.ToDouble(comboBox21.SelectedIndex)/10 + Convert.ToDouble(comboBox2.SelectedIndex) / 100;
            int ret = DLLImport.WBDDCFreqBand(wbddc.WBDDCIndex, wbddc.WBDDCFreq[wbddc.WBDDCIndex] * 10);

            if (passCtrlBack != null)
            {
                passCtrlBack(ret);
            }

            if (wbChangedCallBack != null)
            {
                wbChangedCallBack(wbddc.WBDDCIndex, wbddc.WBDDCFreq[wbddc.WBDDCIndex]);
            }
        }

        //09射频AGC参数控制 200102LX
        private void button1_Click(object sender, EventArgs e)
        {
            int UpLimit = comboBox17.SelectedIndex;
            int DownLimit = comboBox19.SelectedIndex;
            int HoldTime = comboBox18.SelectedIndex;

            int ret = DLLImport.RFGainParameter(UpLimit, DownLimit, HoldTime);

            if (passCtrlBack != null)
            {
                passCtrlBack(ret);
            }
        }

        //10数字AGC参数控制 200102LX
        private void button20_Click(object sender, EventArgs e)
        {
            int UpLimit = comboBox30.SelectedIndex;
            int DownLimit = comboBox31.SelectedIndex;
            int HoldTime = comboBox34.SelectedIndex;

            int ret = DLLImport.DigitalGainParameter(UpLimit, DownLimit, HoldTime);

            if (passCtrlBack != null)
            {
                passCtrlBack(ret);
            }
        }

        //11 时标切换200102LX
        private void button161_Click(object sender, EventArgs e)
        {
            int Mode = comboBox62.SelectedIndex;
            DLLImport.DEV_TIMEDEV_RESULT Result = new DLLImport.DEV_TIMEDEV_RESULT();

            int ret = DLLImport.SelectTimeDev(Mode, ref Result);
            if (passTimeDev != null)
            {
                passTimeDev(Result);

                if (Result.Mode == Mode)
                {
                    passCtrlBack(0);
                }
            }
        }
        //12 GPS/BD类型控制指令200102LX
        private void button4_Click(object sender, EventArgs e)
        {
            int TimeMode = comboBox6.SelectedIndex;
            DLLImport.DEV_GPSBD_RESULT Result = new DLLImport.DEV_GPSBD_RESULT();

            int ret = DLLImport.SelectGPSBD(TimeMode, ref Result);
            if(passGPSBD != null)
            {
                passGPSBD(Result);
                if(Result.Mode == TimeMode)
                {
                    passCtrlBack(0);
                }
            }
        }

        //13数字增益24dB开关控制200102LX
        private void simpleButton20_Click(object sender, EventArgs e)
        {
            int ret = 0;
            if (simpleButton20.Text == "24dB增\r\n益开启")
            {
                simpleButton20.Text = "24dB增\r\n益关闭";
                ret = DLLImport.DigitalGainSwitch(1);
                this.m_DataProcessing.DigitialGain_24db = true;
            }
            else if (simpleButton20.Text == "24dB增\r\n益关闭")
            {
                simpleButton20.Text = "24dB增\r\n益开启";
                ret = DLLImport.DigitalGainSwitch(0);
                this.m_DataProcessing.DigitialGain_24db = false;
            }
            if (passCtrlBack != null)
            {
                passCtrlBack(ret);
            }
        }

        //14系统时钟状态查询200102LX
        private void button3_Click(object sender, EventArgs e)
        {
            DLLImport.DEV_CLKStatus_RESULT Result = new DLLImport.DEV_CLKStatus_RESULT();
            int ret = DLLImport.DevCC_SampleClockCheckSelf(ref Result);

            if (passSampleClockCheckSelf != null)
            {
                passSampleClockCheckSelf(Result);
            }
        }

        //15Aurora接口channel_up连接状态 200102LX
        private void button5_Click(object sender, EventArgs e)
        {
            DLLImport.DEV_CCAURORA_CHECKSELF_RESULT Result = new DLLImport.DEV_CCAURORA_CHECKSELF_RESULT();
            int ret = DLLImport.AuroraStatus(ref Result);

            if (passAuroraStatus != null)
            {
                passAuroraStatus(Result);
            }
        }

        //17数字增益模式控制 200102LX
        private void button21_Click(object sender, EventArgs e)
        {
            int Mode = comboBox22.SelectedIndex;

            if (Mode == 1)
            {
                int Value = 31;
                int ret1 = DLLImport.DigitalGainMode(Mode);
                Thread.Sleep(10);
                int ret2 = DLLImport.DigitalGainValue(Value);
                int ret = ret1 & ret2;
                if (passCtrlBack != null)
                {
                    passCtrlBack(ret);
                }
            }

            if (Mode == 0)
            {
                int ret = DLLImport.DigitalGainMode(Mode);
                if (passCtrlBack != null)
                {
                    passCtrlBack(ret);
                }
            }

        }

        //18数字增益模式查询 200102LX
        private void button2_Click(object sender, EventArgs e)
        {
            DLLImport.DEV_GetDigitalGainMode_RESULT Result = new DLLImport.DEV_GetDigitalGainMode_RESULT();
            int ret = DLLImport.GetCCGainMode(ref Result);

            if (passGetCCGainMode != null)
            {
                passGetCCGainMode(Result);
            }
        }

        //19数字MGC衰减值控制 200102LX
        private void button22_Click(object sender, EventArgs e)
        {
            int Value = Convert.ToInt32(textBox29.Text);
            int ret = DLLImport.DigitalGainValue(Value);

            if (passCtrlBack != null)
            {
                passCtrlBack(ret);
            }
        }
        
        #endregion   

        //21窄带DDC时标精度
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            int Accuracy = Convert.ToInt32(textBox1.Text);
            int ret = DLLImport.NBDDCAccuracy(Accuracy);

            if (passCtrlBack != null)
            {
                passCtrlBack(ret);
            }
        }

        //22FFT时标精度
        private void simpleButton2_Click(object sender, EventArgs e)
        {
            int Accuracy = Convert.ToInt32(textBox2.Text);
            int ret = DLLImport.FFTAccuracy(Accuracy);

            if (passCtrlBack != null)
            {
                passCtrlBack(ret);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 196; i++)
            {
                nbddc.NBDDCIndex = i;//选定子带号
                nbddc.NBDDCBandWidthindex = comboBox10.SelectedIndex; ;//带宽序号0~13（千兆网DLL下发）
                nbddc.NBDDCFreq[nbddc.NBDDCIndex] = i * 0.1 + (comboBox9.SelectedIndex + 1) + Convert.ToDouble(comboBox8.SelectedIndex) / 10 + Convert.ToDouble(comboBox7.SelectedIndex) / 100;//给选定子带的中心频点数组赋值
                nbddc.NBDDCBandWidth[nbddc.NBDDCIndex] = Convert.ToDouble(comboBox10.Text);//给选定子带的带宽数组赋值
                int ret = DLLImport.NBDDCFreqBand(nbddc.NBDDCIndex, nbddc.NBDDCBandWidthindex, nbddc.NBDDCFreq[nbddc.NBDDCIndex] * 10);
                //StartSave();
                //Thread.Sleep(1000);
                //StopSave();
                if (passCtrlBack != null)
                {
                    passCtrlBack(ret);
                }
                if (nbChangedCallBack != null)
                {
                    nbChangedCallBack(nbddc.NBDDCIndex, nbddc.NBDDCBandWidth[nbddc.NBDDCIndex], nbddc.NBDDCFreq[nbddc.NBDDCIndex]);
                }
            }
        }
    }
}
