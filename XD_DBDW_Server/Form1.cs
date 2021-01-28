using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraBars.Docking2010.Views;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Navigation;
using System.Threading;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml;
using System.Globalization;

namespace XD_DBDW_Server
{
    public partial class Form1 : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        XtraUserControl FrequencySpectrumUserControl;
        XtraUserControl IQWaveformUserControl;
        XtraUserControl WaterfallPlotUserControl;

        private XmlProcessing m_XmlProcessing = new XmlProcessing();
        private WindowApp WindowApp = new WindowApp();
        private Transform Transform = new Transform();
        private UI_Control m_UI_Control;
        private UI_RFLocalCtrl m_UI_RFLocalCtrl;
        private UI_NetConfigData m_UI_NetConfigData;
        private UI_FPGAConfig m_UI_FPGAConfig;
        private UI_AGC m_UI_AGC;
        private UI_FrequencySpectrum m_UI_FrequencySpectrum;
        private UI_IQWaveform m_UI_IQWaveform;
        private UI_WaterfallPlot m_UI_WaterfallPlot;
        private DataProcessing m_DataProcessing;
        private string m_NetLocalIP;
        private int m_NetLocalPort;
        private int m_DevID = 0;
        public string m_NetGroupIP;
        public int band;
        public string[] UdpMulticastGroup = { "239.1.1.200", "239.1.1.201", "239.1.1.202", "239.1.1.203", "239.1.1.204" };
        #region 初始化

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            WriteXml();

            #region 网络初始化
            try
            {
                NetInit();
                m_DataProcessing = new DataProcessing();//初始化数据处理类
                m_DataProcessing.udpRecvInit(m_NetLocalIP, string.Format(m_NetGroupIP, 1), m_NetLocalPort);//进数据处理类
            }
            catch (Exception e)
            {

            }
            #endregion

            #region 委托传递数据
            m_DataProcessing.udpRecv.passTime += new udpRecv.PassTime(udpRecv_passTime);//时标信息的委托
            m_DataProcessing.DataFrequencySpectrum.passPowerAndFreq += new DataFrequencySpectrum.PassPowerAndFreq(DataFrequencySpectrum_passPowerAndFreq);//频域数据的委托
            m_DataProcessing.DataIQWaveform.passIQData += new DataIQWaveform.PassIQData(DataIQWaveform_passIQData);//IQ数据的委托
            #endregion

            #region UI
            FrequencySpectrumUserControl = CreateUserControl("FrequencySpectrum");
            IQWaveformUserControl = CreateUserControl("IQWaveform");
            WaterfallPlotUserControl = CreateUserControl("WaterfallPlot");

            #region RFControl
            m_UI_RFLocalCtrl = new UI_RFLocalCtrl(m_DataProcessing);
            dockPanel1_Container.Controls.Add(m_UI_RFLocalCtrl);
            this.m_UI_RFLocalCtrl.Dock = DockStyle.Fill;
            #endregion

            #region NetConfigData
            m_UI_NetConfigData = new UI_NetConfigData();
            NetConfigData.Controls.Add(m_UI_NetConfigData);
            this.m_UI_NetConfigData.Dock = DockStyle.Fill;
            #endregion

            #region UI_FPGAConfig
            m_UI_FPGAConfig = new UI_FPGAConfig();
            FPGAConfig.Controls.Add(m_UI_FPGAConfig);
            this.m_UI_FPGAConfig.Dock = DockStyle.Fill;
            #endregion

            //0822LX增加AGC查询选项卡
            #region UI_AGC
            m_UI_AGC = new UI_AGC();
            dockPanel3.Controls.Add(m_UI_AGC);
            #endregion

            #region UI_FrequencySpectrum
            m_UI_FrequencySpectrum = new UI_FrequencySpectrum();
            this.FrequencySpectrumUserControl.Controls.Clear();
            this.FrequencySpectrumUserControl.Controls.Add(m_UI_FrequencySpectrum);
            this.m_UI_FrequencySpectrum.Dock = DockStyle.Fill;
            #endregion

            accordionControl.SelectedElement = FrequencySpectrumAccordionControlElement;

            #endregion

            #region 射频控制委托
            m_UI_RFLocalCtrl.nbChangedCallBack += new UI_RFLocalCtrl.NBChangedCallBack(m_UI_RFLocalCtrl_nbChangedCallBack);
            m_UI_RFLocalCtrl.wbChangedCallBack += new UI_RFLocalCtrl.WBChangedCallBack(m_UI_RFLocalCtrl_wbChangedCallBack);
            m_UI_RFLocalCtrl.passCtrlBack += new UI_RFLocalCtrl.PassCtrlBack(m_UI_RFLocalCtrl_passCtrlBack);
            m_UI_RFLocalCtrl.passCSDevIdentity += new UI_RFLocalCtrl.PassCSDevIdentity(m_UI_RFLocalCtrl_passCSDevIdentity);
            m_UI_RFLocalCtrl.passCSDevState += new UI_RFLocalCtrl.PassCSDevState(m_UI_RFLocalCtrl_passCSDevState);
            m_UI_RFLocalCtrl.passGSDevIdentity += new UI_RFLocalCtrl.PassGSDevIdentity(m_UI_RFLocalCtrl_passGSDevIdentity);
            m_UI_RFLocalCtrl.passGSDevState += new UI_RFLocalCtrl.PassGSDevState(m_UI_RFLocalCtrl_passGSDevState);
            m_UI_RFLocalCtrl.passRFDevIdentity += new UI_RFLocalCtrl.PassRFDevIdentity(m_UI_RFLocalCtrl_passRFDevIdentity);
            m_UI_RFLocalCtrl.passRFDevState += new UI_RFLocalCtrl.PassRFDevState(m_UI_RFLocalCtrl_passRFDevState);
            m_UI_RFLocalCtrl.passRFGainModeState += new UI_RFLocalCtrl.PassRFGainModeState(m_UI_RFLocalCtrl_passRFGainModeState);
            m_UI_RFLocalCtrl.passDevPowerCheckSelf += new UI_RFLocalCtrl.PassDevPowerCheckSelf(m_UI_RFLocalCtrl_passDevPowerCheckSelf);
            m_UI_RFLocalCtrl.passSampleClockCheckSelf += new UI_RFLocalCtrl.PassSampleClockCheckSelf(m_UI_RFLocalCtrl_passSampleClockCheckSelf);
            m_UI_RFLocalCtrl.passCommunityStatus += new UI_RFLocalCtrl.PassCommunityStatus(m_UI_RFLocalCtrl_passCommunityStatus);
            m_UI_RFLocalCtrl.passAuroraStatus += new UI_RFLocalCtrl.PassAuroraStatus(m_UI_RFLocalCtrl_passAuroraStatus);
            m_UI_RFLocalCtrl.passTimeDev += new UI_RFLocalCtrl.PassTimeDev(m_UI_RFLocalCtrl_passTimeDev);
            m_UI_RFLocalCtrl.passGPSBD += new UI_RFLocalCtrl.PassGPSBD(m_UI_RFLocalCtrl_passGPSBD);
            m_UI_RFLocalCtrl.passGetCCGainMode += new UI_RFLocalCtrl.PassGetCCGainMode(m_UI_RFLocalCtrl_passGetCCGainMode);
            m_UI_RFLocalCtrl.StartSave += new UI_RFLocalCtrl.SaveEvent(m_UI_RFLocalCtrl_passStartSaveEvent);
            m_UI_RFLocalCtrl.StopSave += new UI_RFLocalCtrl.SaveEvent(m_UI_RFLocalCtrl_passStopSaveEvent);
          
            #endregion

            #region 万兆网本地IP传递

            m_UI_FPGAConfig.passIPEndPoint += new UI_FPGAConfig.PassIPEndPoint(m_UI_FPGAConfig_passIPEndPoint);

            #endregion
        }

        void m_UI_RFLocalCtrl_passStartSaveEvent()
        {
            m_DataProcessing.DataFunction.StartSave();
        }

        void m_UI_RFLocalCtrl_passStopSaveEvent()
        {
            m_DataProcessing.DataFunction.StopSave();
        }

        private void MyAsyncCallback(IAsyncResult ar)
        {

        }

        UInt16 datatimeIndex = 0;

        //B码、GPS/BD时间显示
        void udpRecv_passTime(TimeInfo timeInfo, int type)
        {
            int MAX_TIME;
            switch(type)
            {
                case 1: MAX_TIME = 10; break;
                case 2: MAX_TIME = 500; break;
                case 3: MAX_TIME = 1000; break;
                default: MAX_TIME = 1000; break;
            }
            if (++datatimeIndex > MAX_TIME)
            {
                datatimeIndex = 0;
                if (this.IsHandleCreated)
                    this.BeginInvoke(new Action(() =>
                    {
                        DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo();//时间格式化
                        string nanosecond = timeInfo.millisecond.ToString("d3") + "毫秒" + timeInfo.microsecond.ToString("d3") + "微秒";

                        switch (timeInfo.satelliteInfo.time_state)
                        {
                            //B码=0、GPS/BD=0
                            case 0:
                                barStaticItem3.Caption =
                                "B码：" + System.DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒", dtFormat)
                                + " | BD/GPS时间：" + System.DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒", dtFormat);
                                break;

                            //B码=0、GPS/BD=1
                            case 1:
                                barStaticItem3.Caption =
                                "B码：" + System.DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒", dtFormat) + nanosecond
                                + " | BD/GPS时间：" + timeInfo.satelliteInfo.year.ToString() + "年"
                                + timeInfo.satelliteInfo.month.ToString() + "月"
                                + timeInfo.satelliteInfo.day.ToString() + "日"
                                + timeInfo.satelliteInfo.hour.ToString() + "时"
                                + timeInfo.satelliteInfo.minute.ToString("d2") + "分"
                                + timeInfo.satelliteInfo.second.ToString("d2") + "秒"
                                + nanosecond;
                                break;
                            //B码=1、GPS/BD=0
                            case 2:
                                barStaticItem3.Caption =
                                "B码：" + timeInfo.year.ToString() + "年"
                                + timeInfo.month.ToString() + "月"
                                + timeInfo.day_offset.ToString() + "日"
                                + timeInfo.hour.ToString() + "时"
                                + timeInfo.minute.ToString("d2") + "分"
                                + timeInfo.second.ToString("d2") + "秒"
                                + nanosecond
                                + " | BD/GPS时间：" + System.DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒", dtFormat) + nanosecond;
                                break;

                            //B码=1、GPS/BD=1
                            case 3:
                                barStaticItem3.Caption =
                                "B码：" + timeInfo.year.ToString() + "年"
                                + timeInfo.month.ToString() + "月"
                                + timeInfo.day_offset.ToString() + "日"
                                + timeInfo.hour.ToString() + "时"
                                + timeInfo.minute.ToString("d2") + "分"
                                + timeInfo.second.ToString("d2") + "秒"
                                + nanosecond
                                + " | BD/GPS时间：" + timeInfo.satelliteInfo.year.ToString() + "年"
                                + timeInfo.satelliteInfo.month.ToString() + "月"
                                + timeInfo.satelliteInfo.day.ToString() + "日"
                                + timeInfo.satelliteInfo.hour.ToString() + "时"
                                + timeInfo.satelliteInfo.minute.ToString("d2") + "分"
                                + timeInfo.satelliteInfo.second.ToString("d2") + "秒"
                                + nanosecond;
                                break;
                            default:
                                break;
                        }
                    }));
            }
        }

        void m_UI_FPGAConfig_passIPEndPoint(System.Net.IPEndPoint t)
        {
            m_NetLocalIP = t.Address.ToString();
            m_NetLocalPort = t.Port;
        }

        #endregion

        #region 千兆、万兆网络初始化

        void NetInit()
        {
            m_UI_Control = new UI_Control();

            int res = m_UI_Control.DevNetCtrlInit();
            if (res != 0)
            {
                memoEdit1.Text += "\r\n控制网络初始化失败！";
                return;
            }
            else
            {
                memoEdit1.Text += "\r\n控制网络初始化成功！";
            }
            res = m_UI_Control.DevNetDataSourceInit();
            if (res != 0)
            {
                memoEdit1.Text += "\r\n万兆网源端初始化失败！";
            }
            else
            {
                memoEdit1.Text += "\r\n万兆网源端初始化成功！";
            }

            res = m_UI_Control.DataLinkEnable();
            if (res != 0)
            {
                memoEdit1.Text += "\r\n万兆网数据使能开启失败！";
            }
            else
            {
                memoEdit1.Text += "\r\n万兆网数据使能开启成功！";
            }

            res = m_UI_Control.DevNetDataDestInit();
            if (res != 0)
            {
                memoEdit1.Text += "\r\n万兆网目的端初始化失败！";
            }
            else
            {
                memoEdit1.Text += "\r\n万兆网目的端初始化成功！";
            }
        }

        #endregion

        #region 创建XtraUserControl
        XtraUserControl CreateUserControl(string text)
        {
            XtraUserControl result = new XtraUserControl();
            result.Name = text.ToLower() + "UserControl";
            result.Text = text;
            LabelControl label = new LabelControl();
            label.Parent = result;
            label.Appearance.Font = new Font("Tahoma", 25.25F);
            label.Appearance.ForeColor = Color.Gray;
            label.Dock = System.Windows.Forms.DockStyle.Fill;
            label.AutoSizeMode = LabelAutoSizeMode.None;
            label.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            label.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            label.Text = text;
            return result;
        }
        #endregion

        #region accordionControl属性更改事件

        void accordionControl_SelectedElementChanged(object sender, SelectedElementChangedEventArgs e)
        {
            if (e.Element == null) return;
            XtraUserControl userControl = new XtraUserControl();
            switch (e.Element.Text)
            {
                case "FrequencySpectrum":
                    userControl = FrequencySpectrumUserControl;
                    break;
                case "IQWaveform":
                    userControl = IQWaveformUserControl;
                    break;
                case "WaterfallPlot":
                    userControl = WaterfallPlotUserControl;
                    break;
                default:
                    break;
            }
            tabbedView.AddDocument(userControl);
            tabbedView.ActivateDocument(userControl);
        }
        #endregion

        #region barSubItemNavigation按键效果
        void barButtonNavigation_ItemClick(object sender, ItemClickEventArgs e)
        {
            int barItemIndex = barSubItemNavigation.ItemLinks.IndexOf(e.Link);
            accordionControl.SelectedElement = mainAccordionGroup.Elements[barItemIndex];
        }

        #endregion

        #region tabbedView关闭事件
        void tabbedView_DocumentClosed(object sender, DocumentEventArgs e)
        {
            RecreateUserControls(e);
            SetAccordionSelectedElement(e);
        }
        void SetAccordionSelectedElement(DocumentEventArgs e)
        {
            if (tabbedView.Documents.Count != 0)
            {
                if (e.Document.Caption == "FrequencySpectrum") accordionControl.SelectedElement = IQWaveformAccordionControlElement;
                else accordionControl.SelectedElement = FrequencySpectrumAccordionControlElement;
            }
            else
            {
                accordionControl.SelectedElement = null;
            }
        }
        void RecreateUserControls(DocumentEventArgs e)
        {
            if (e.Document.Caption == "FrequencySpectrum") FrequencySpectrumUserControl = CreateUserControl("FrequencySpectrum");
            else IQWaveformUserControl = CreateUserControl("IQWaveform");
        }
        #endregion

        #region dockPane视图按钮

        private void barButtonItem9_ItemClick(object sender, ItemClickEventArgs e)
        {
            dockPanel.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
        }

        private void barButtonItem10_ItemClick(object sender, ItemClickEventArgs e)
        {
            Export.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
        }

        private void barButtonItem11_ItemClick(object sender, ItemClickEventArgs e)
        {
            FPGAConfig.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
        }

        private void barButtonItem12_ItemClick(object sender, ItemClickEventArgs e)
        {
            RFControl.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
        }

        private void barButtonItem13_ItemClick(object sender, ItemClickEventArgs e)
        {
            NetConfigData.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
        }
        #endregion

        #region accordionControl视图按键
        private void FrequencySpectrumAccordionControlElement_Click(object sender, EventArgs e)
        {
            m_UI_FrequencySpectrum = new UI_FrequencySpectrum();
            this.FrequencySpectrumUserControl.Controls.Clear();
            this.FrequencySpectrumUserControl.Controls.Add(m_UI_FrequencySpectrum);
            this.m_UI_FrequencySpectrum.Dock = DockStyle.Fill;
        }

        private void IQWaveformAccordionControlElement_Click(object sender, EventArgs e)
        {
            m_UI_IQWaveform = new UI_IQWaveform();
            this.IQWaveformUserControl.Controls.Clear();
            this.IQWaveformUserControl.Controls.Add(m_UI_IQWaveform);
            this.m_UI_IQWaveform.Dock = DockStyle.Fill;
        }
        private void accordionControlElement1_Click(object sender, EventArgs e)
        {
            m_UI_WaterfallPlot = new UI_WaterfallPlot();
            this.WaterfallPlotUserControl.Controls.Clear();
            this.WaterfallPlotUserControl.Controls.Add(m_UI_WaterfallPlot);
            this.m_UI_WaterfallPlot.Dock = DockStyle.Fill;
        }

        #endregion

        #region 功能按键

        private void barButtonItem14_ItemClick(object sender, ItemClickEventArgs e)
        {
            WindowApp.RestartApplication();
        }

        //01采集板自检200107LX
        private void barButtonItem5_ItemClick(object sender, ItemClickEventArgs e)
        {
            string stfo = m_UI_Control.DevSelfCheck();
            memoEdit1.Text = "";
            memoEdit1.Text += stfo;

        }
        private void barButtonItem7_ItemClick(object sender, ItemClickEventArgs e)
        {
            barHeaderItem1.Caption = string.Format("RecvPackets：{0}", m_DataProcessing.udpRecv.RevCount);
            barHeaderItem2.Caption = string.Format("LossPackets：{0}", m_DataProcessing.udpRecv.LostCount);
        }

        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (barButtonItem1.Caption == "开始保存")
            {
                barButtonItem1.Caption = "正在保存";
                m_DataProcessing.DataFunction.StartSave();

            }
            else if (barButtonItem1.Caption == "正在保存")
            {
                barButtonItem1.Caption = "开始保存";
                m_DataProcessing.DataFunction.StopSave();
            }
        }

        private void barButtonItem19_ItemClick(object sender, ItemClickEventArgs e)
        {
            m_DataProcessing.DataFrequencySpectrum.StartSave();
        }

        private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        {
            m_DataProcessing.DataFunction.StartCheckTime();
            m_DataProcessing.DataFunction.passDeleTimeSign += new DataFunction.DeleTimeSign(DataFunction_passDeleTimeSign);
        }

        void DataFunction_passDeleTimeSign(DataTime datatime)
        {
            memoEdit1.Text = "";
            memoEdit1.Text += "时标检测：" + datatime.year.ToString() + "年" + datatime.month.ToString() + "月"
                            + datatime.day.ToString() + "日" + datatime.hour.ToString() + "时" + datatime.minute.ToString() + "分"
                            + datatime.second.ToString() + "秒" + datatime.millisecond.ToString() + "毫秒" + datatime.microsecond.ToString() + "微秒";
        }
        
        #endregion

        static string[] ZHAIDAI_CH = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10",
                        "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28",
                        "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", 
                        "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", 
                        "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80", "81", "82",
                        "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "100",
                        "101", "102", "103", "104", "105", "106", "107", "108", "109", "110", "111", "112", "113", "114", "115", 
                        "116", "117", "118", "119", "120", "121", "122", "123", "124", "125", "126", "127", "128", "129", "130",
                        "131", "132", "133", "134", "135", "136", "137", "138", "139", "140", "141", "142", "143", "144", "145",
                        "146", "147", "148", "149", "150", "151", "152", "153", "154", "155", "156", "157", "158", "159", "160", 
                        "161", "162", "163", "164", "165", "166", "167", "168", "169", "170", "171", "172", "173", "174", "175",
                        "176", "177", "178", "179", "180", "181", "182", "183", "184", "185", "186", "187", "188", "189", "190",
                        "191", "192", "193", "194", "195", "196" };

        static string[] KUANDAI_CH = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10",
                        "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28",
                        "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", 
                        "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60" };

        static string[] FFT_CH = new string[] { "1", "2", "3", "4", "5" };

        ////数据类型选择
        private void repositoryItemComboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            string ip = "";
            int index = repositoryItemComboBox3.Items.IndexOf(barEditItem3.EditValue);
            repositoryItemComboBox3.Items.Clear();
            switch (((DevExpress.XtraEditors.ComboBoxEdit)sender).SelectedIndex)
            {
                case 0:
                    if (index < 0) index = 0;
                    else if (index > 195) index = 195;
                    this.repositoryItemComboBox3.Items.AddRange(ZHAIDAI_CH);
                    band = index + 1;
                    ip = string.Format(m_NetGroupIP, band);//根据选择的子带号（路数）更改组播IP
                    m_DataProcessing.Selected_NBBandWidth_Freq(index, m_UI_RFLocalCtrl.nbddc.NBDDCBandWidth[index], m_UI_RFLocalCtrl.nbddc.NBDDCFreq[index]);
                    
            m_DataProcessing.udpRecvDestroy();//销毁之前的socketLX
            m_DataProcessing.udpRecv.ClearQueue();
            m_DataProcessing.ClearQueue();
            m_DataProcessing.DataIQWaveform.ClearQueue();
            m_DataProcessing.DataFunction.ClearQueue();
            m_DataProcessing.DataFrequencySpectrum.ClearQueue();
            m_DataProcessing.udpRecvInit(m_NetLocalIP, ip, m_NetLocalPort);//根据选择的路数创建socketLX
            m_DataProcessing.udpRecv.passTime += new udpRecv.PassTime(udpRecv_passTime);//时标信息的委托LX
                    break;
                case 1:
                    if (index < 0) index = 0;
                    else if (index > 59) index = 59;
                    this.repositoryItemComboBox3.Items.AddRange(KUANDAI_CH);
                    band = index / 20 + 197;
                    ip = string.Format(m_NetGroupIP, band);//根据选择的路数更改组播IP
                    m_DataProcessing.Selected_WBBandWidth_Freq(index, m_UI_RFLocalCtrl.wbddc.WBDDCFreq[index]);//更改宽带频点
                    m_DataProcessing.SelectedBand(index + 1);//选择宽带数据包中的子带
                   
                    
            m_DataProcessing.udpRecvDestroy();//销毁之前的socketLX
            m_DataProcessing.udpRecv.ClearQueue();
            m_DataProcessing.ClearQueue();
            m_DataProcessing.DataIQWaveform.ClearQueue();
            m_DataProcessing.DataFunction.ClearQueue();
            m_DataProcessing.DataFrequencySpectrum.ClearQueue();
            m_DataProcessing.udpRecvInit(m_NetLocalIP, ip, m_NetLocalPort);//根据选择的路数创建socketLX
            m_DataProcessing.udpRecv.passTime += new udpRecv.PassTime(udpRecv_passTime);//时标信息的委托LX
                    break;
                case 2:
                    if (index < 0) index = 0;
                    else if (index > 4) index = 4;
                    this.repositoryItemComboBox3.Items.AddRange(FFT_CH);
                    band = index + 200;
                    ip = string.Format(m_NetGroupIP, band);//根据选择的路数更改组播IP
                    m_DataProcessing.Selected_FFTBandWidth_Freq(band - 200);//更改FFT频点
                   
                    
            m_DataProcessing.udpRecvDestroy();//销毁之前的socketLX
            m_DataProcessing.udpRecv.ClearQueue();
            m_DataProcessing.ClearQueue();
            m_DataProcessing.DataIQWaveform.ClearQueue();
            m_DataProcessing.DataFunction.ClearQueue();
            m_DataProcessing.DataFrequencySpectrum.ClearQueue();
            m_DataProcessing.udpRecvInit(m_NetLocalIP, UdpMulticastGroup, m_NetLocalPort);//根据选择的路数创建socketLX
            m_DataProcessing.udpRecv.passTime += new udpRecv.PassTime(udpRecv_passTime);//时标信息的委托LX
                    break;
                default:
                    break;
            }

        }

        ////路数选择
        private void repositoryItemComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            string ip = "";
            int index = ((DevExpress.XtraEditors.ComboBoxEdit)sender).SelectedIndex;
            switch (repositoryItemComboBox4.Items.IndexOf(barEditItem4.EditValue))
            {
                case 0:
                    if (index < 0) index = 0;
                    else if (index > 195) index = 195;
                    band = index + 1;//获取界面所选择的子带号
                    ip = string.Format(m_NetGroupIP, band);//根据选择的子带号（路数）更改组播IP
                    m_DataProcessing.Selected_NBBandWidth_Freq(index, m_UI_RFLocalCtrl.nbddc.NBDDCBandWidth[index], m_UI_RFLocalCtrl.nbddc.NBDDCFreq[index]);
                             m_DataProcessing.udpRecvDestroy();//销毁之前的socketLX
            m_DataProcessing.udpRecv.ClearQueue();
            m_DataProcessing.ClearQueue();
            m_DataProcessing.DataIQWaveform.ClearQueue();
            m_DataProcessing.DataFunction.ClearQueue();
            m_DataProcessing.DataFrequencySpectrum.ClearQueue();
            m_DataProcessing.udpRecvInit(m_NetLocalIP, ip, m_NetLocalPort);//根据选择的路数创建socketLX
            m_DataProcessing.udpRecv.passTime += new udpRecv.PassTime(udpRecv_passTime);//时标信息的委托LX
                    break;
                case 1:
                    if (index < 0) index = 0;
                    else if (index > 59) index = 59;
                    band = index / 20 + 197;
                    ip = string.Format(m_NetGroupIP, band);//根据选择的路数更改组播IP
                    m_DataProcessing.Selected_WBBandWidth_Freq(index, m_UI_RFLocalCtrl.wbddc.WBDDCFreq[index]);//更改宽带频点
                    m_DataProcessing.SelectedBand(index + 1);//选择宽带数据包中的子带
                            m_DataProcessing.udpRecvDestroy();//销毁之前的socketLX
            m_DataProcessing.udpRecv.ClearQueue();
            m_DataProcessing.ClearQueue();
            m_DataProcessing.DataIQWaveform.ClearQueue();
            m_DataProcessing.DataFunction.ClearQueue();
            m_DataProcessing.DataFrequencySpectrum.ClearQueue();
            m_DataProcessing.udpRecvInit(m_NetLocalIP, ip, m_NetLocalPort);//根据选择的路数创建socketLX
            m_DataProcessing.udpRecv.passTime += new udpRecv.PassTime(udpRecv_passTime);//时标信息的委托LX
                    break;
                case 2:
                    if (index < 0) index = 0;
                    else if (index > 4) index = 4;
                    band = index + 200;
                    ip = string.Format(m_NetGroupIP, band);//根据选择的路数更改组播IP
                    m_DataProcessing.Selected_FFTBandWidth_Freq(band - 200);//更改FFT频点
                            m_DataProcessing.udpRecvDestroy();//销毁之前的socketLX
            m_DataProcessing.udpRecv.ClearQueue();
            m_DataProcessing.ClearQueue();
            m_DataProcessing.DataIQWaveform.ClearQueue();
            m_DataProcessing.DataFunction.ClearQueue();
            m_DataProcessing.DataFrequencySpectrum.ClearQueue();
            m_DataProcessing.udpRecvInit(m_NetLocalIP, UdpMulticastGroup, m_NetLocalPort);//根据选择的路数创建socketLX
            m_DataProcessing.udpRecv.passTime += new udpRecv.PassTime(udpRecv_passTime);//时标信息的委托LX
                    break;
                default:
                    break;
            }
   
        }

        //平滑数
        private void repositoryItemComboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_DataProcessing.udpRecv.ClearQueue();
            m_DataProcessing.ClearQueue();
            m_DataProcessing.DataFrequencySpectrum.ClearQueue();
            if (((DevExpress.XtraEditors.ComboBoxEdit)sender).Text == null)
                return;
            int Num = Convert.ToInt32(((DevExpress.XtraEditors.ComboBoxEdit)sender).Text);
            m_DataProcessing.DataFrequencySpectrum.SelectSmoothNum(Num);
        }

        //分辨率
        private void repositoryItemComboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_DataProcessing.ClearQueue();
            m_DataProcessing.DataFrequencySpectrum.ClearQueue();
            if (((DevExpress.XtraEditors.ComboBoxEdit)sender).Text == null)
                return;
            int Num = ((DevExpress.XtraEditors.ComboBoxEdit)sender).SelectedIndex;
            int Resolution = (int)Math.Pow(2, Num);
            m_DataProcessing.SelectedResolution(Resolution);
        }

        #region 数据传输至UI显示层
        void DataIQWaveform_passIQData(IQData t)
        {
            if (m_UI_IQWaveform != null && t != null)
            {
                m_UI_IQWaveform.PushDataAttribute(t);
            }
        }

        void DataFrequencySpectrum_passPowerAndFreq(PowerAndFreq t)
        {
            if (m_UI_FrequencySpectrum != null && t != null)
            {
                m_UI_FrequencySpectrum.PushDataAttribute(t);
            }
            if (m_UI_WaterfallPlot != null && t != null)
            {
                m_UI_WaterfallPlot.PushDataAttribute(t);
            }
        }

        #endregion

        #region Xml
        //初始化设备类型、千兆网设置、万兆网设置200117LX
        private void WriteXml()
        {
            m_NetLocalIP = m_XmlProcessing.ReadNet10G_LocalIP();//初始化万兆网IP
            m_NetLocalPort = Convert.ToInt32(m_XmlProcessing.ReadNet10G_LocalPort());//初始化万兆网端口
            m_NetGroupIP = m_XmlProcessing.ReadNet10G_GroupIP();//初始化组播IP

            int DevID = Convert.ToInt32(m_XmlProcessing.ReadDevID());//初始化设备类型
            m_DevID = DevID;
            switch (m_DevID)
            {
                case 2:
                    repositoryItemComboBox3.Items.Clear();
                    this.repositoryItemComboBox3.Items.AddRange(new object[] { "1","2","3","4","5","6","7","8","9","10",
            "11","12","13","14", "15","16","17","18","19","20","21","22","23","24","25","26","27","28","29","30",
            "31","32","33","34","35","36","37","38","39","40","41","42","43","44","45","46","47","48","49","50",
            "51","52","53","54","55","56","57","58"});
                    repositoryItemComboBox4.Items.Clear();
                    this.repositoryItemComboBox4.Items.AddRange(new object[] { "0", "1", "2", "3", "4", "5", "6", "7", "8" });
                    repositoryItemComboBox5.Items.Clear();
                    this.repositoryItemComboBox5.Items.AddRange(new object[] {"2500Hz","1250Hz","625Hz","312.5Hz","156.25Hz",
            "78.125Hz","39.0625Hz","19.53125Hz"});
                    this.repositoryItemComboBox5.NullText = "2500Hz";
                    barStaticItem5.Caption = "8通道测试程序";
                    break;
                case 3:
                    repositoryItemComboBox3.Items.Clear();
                    this.repositoryItemComboBox3.Items.AddRange(new object[] { "1","2","3","4","5","6","7","8","9","10",
            "11","12","13","14", "15","16","17","18","19","20","21","22","23","24","25","26","27","28","29","30",
            "31","32","33","34","35","36","37","38","39","40","41","42","43","44","45","46","47","48","49","50",
            "51","52","53","54","55","56","57","58"});
                    repositoryItemComboBox4.Items.Clear();
                    this.repositoryItemComboBox4.Items.AddRange(new object[] { "0","1", "2", "3", "4", "5", "6", "7", 
                        "8","9","10", "11", "12", "13", "14", "15", "16", "17", "18" });
                    repositoryItemComboBox5.Items.Clear();
                    this.repositoryItemComboBox5.Items.AddRange(new object[] {"1250Hz","625Hz","312.5Hz","156.25Hz",
            "78.125Hz","39.0625Hz","19.53125Hz"});
                    barStaticItem5.Caption = "18通道测试程序";

                    break;
                case 0:
                    repositoryItemComboBox3.Items.Clear();
                    this.repositoryItemComboBox3.Items.AddRange(new object[] { "1","2","3","4","5","6","7","8","9","10",
            "11","12","13","14", "15","16","17","18","19","20","21","22","23","24","25","26","27","28","29","30",
            "31","32","33","34","35","36","37","38","39","40","41","42","43","44","45","46","47","48","49","50",
            "51","52","53","54","55","56","57","58","59","60","61","62","63","64","65","66"});
                    repositoryItemComboBox4.Items.Clear();
                    this.repositoryItemComboBox4.Items.AddRange(new object[] {"0", "1", "2", "3", "4", "5", "6", "7", 
                        "8","9","10", "11", "12", "13", "14", "15", "16", "17", "18","19","20" });
                    repositoryItemComboBox5.Items.Clear();
                    this.repositoryItemComboBox5.Items.AddRange(new object[] {"1250Hz","625Hz","312.5Hz","156.25Hz",
            "78.125Hz","39.0625Hz","19.53125Hz"});
                    barStaticItem5.Caption = "20通道测试程序";

                    break;

                case 5:
                    repositoryItemComboBox3.Items.Clear();
                    this.repositoryItemComboBox3.Items.AddRange(new object[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10",
                        "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28",
                        "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", 
                        "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", 
                        "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80", "81", "82",
                        "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "100",
                        "101", "102", "103", "104", "105", "106", "107", "108", "109", "110", "111", "112", "113", "114", "115", 
                        "116", "117", "118", "119", "120", "121", "122", "123", "124", "125", "126", "127", "128", "129", "130",
                        "131", "132", "133", "134", "135", "136", "137", "138", "139", "140", "141", "142", "143", "144", "145",
                        "146", "147", "148", "149", "150", "151", "152", "153", "154", "155", "156", "157", "158", "159", "160", 
                        "161", "162", "163", "164", "165", "166", "167", "168", "169", "170", "171", "172", "173", "174", "175",
                        "176", "177", "178", "179", "180", "181", "182", "183", "184", "185", "186", "187", "188", "189", "190",
                        "191", "192", "193", "194", "195", "196" });
                    repositoryItemComboBox4.Items.Clear();
                    this.repositoryItemComboBox4.Items.AddRange(new object[] {"窄带数据", "宽带数据", "FFT数据"});
                    repositoryItemComboBox5.Items.Clear();
                    this.repositoryItemComboBox5.Items.AddRange(new object[] {"1250Hz","625Hz","312.5Hz","156.25Hz",
            "78.125Hz","39.0625Hz","19.53125Hz"});
                    barStaticItem5.Caption = "JGZC测试程序";

                    break;
                default:
                    break;
            }
        }

        //读Xml文件，选择数据包格式
        //private void XmlSelectDataPacket()
        //{
        //    int DevID = Convert.ToInt32(m_XmlProcessing.ReadDevID());
        //    switch (DevID)
        //    {
        //        case 0:
        //            m_DataProcessing.SelectDataPacket(2048, 20);
        //            break;
        //        case 2:
        //            m_DataProcessing.SelectDataPacket(4096, 8);
        //            break;
        //        case 3:
        //            m_DataProcessing.SelectDataPacket(2048, 18);
        //            break;
        //        default:
        //            break;
        //    }
        //}
        #endregion

        #region 性能测试
        private TestDLLImport.Func TestFunc;


        private void barButtonItem3_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (barButtonItem3.Caption == "开始性能测试")
            {
                barButtonItem3.Caption = "正在性能测试";
                repositoryItemComboBox3.ReadOnly = true;

                //TestDLLImport.CreateRCtrl();
                TestFunc = new TestDLLImport.Func(GetTestInformation);
                //TestDLLImport.RegisterTestInformationCallback(TestFunc);

                m_DataProcessing.SelectedFlagTesting(true);
            }
            else if (barButtonItem3.Caption == "正在性能测试")
            {
                barButtonItem3.Caption = "开始性能测试";
                repositoryItemComboBox3.ReadOnly = false;

                //TestDLLImport.DestroyRCtrl();

                m_DataProcessing.SelectedFlagTesting(false);
            }
        }

        delegate int DelegatePerformanceTest(TestDLLImport.TestInformation t);
        private int m_TestBand = 0;
        private int GetTestInformation(TestDLLImport.TestInformation t)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new DelegatePerformanceTest(GetTestInformation), t);
                return 1;
            }
            int band = 0;
            Transform.Transform_FreqToBand(t.Freq, ref band);
            if (m_TestBand != band)
            {
                m_TestBand = band;
                string ip = string.Format(m_NetGroupIP, m_TestBand);
                m_DataProcessing.udpRecvDestroy();
                m_DataProcessing.udpRecvInit(m_NetLocalIP, ip, m_NetLocalPort);
                m_DataProcessing.udpRecv.passTime += new udpRecv.PassTime(udpRecv_passTime);
            }
            m_DataProcessing.Test_DataProcessing.SelectDataType(t.type);
            return 1;
        }


        #endregion

        #region 射频控制反馈结果

        void m_UI_RFLocalCtrl_passRFDevState(DLLImport.DEV_RF_STATUS_RESULT Reslut)
        {
            memoEdit1.Text = "";
            memoEdit1.Text += "\r\n\r\n射频状态信息回读："
               + "\r\n\r\n衰减值：  " + Reslut.RF_GainValue + "(dBm)"
               + "\r\n\r\n工作模式：" + Reslut.RF_WorkMode + "(0常规，1低噪声)"
               + "\r\n\r\n工作温度：" + Reslut.RF_Temperature + "℃";
        }

        void m_UI_RFLocalCtrl_passRFDevIdentity(DLLImport.DEV_RF_IDENTITY_RESULT Reslut)
        {
            memoEdit1.Text = "";

            memoEdit1.Text += "\r\n\r\n预处理模块身份查询结果："
               + "\r\n\r\n查询通道为：                " + Reslut.RF_Addr
               + "\r\n\r\n载板固件版本码为：          " + Reslut.RF_BoardVer
               + "\r\n\r\n固件版本号为：              " + Reslut.RF_Ver
               + "\r\n\r\n最大正常工作时钟频率码为：  " + Reslut.RF_MaxWorkingFreq
               + "\r\n\r\n公司名称码：                " + Reslut.RF_ComName
               + "\r\n\r\n模块类型码为：              " + Reslut.RF_ModuleType
               + "\r\n\r\n模块申购编号码为：          " + Reslut.RF_BoughtIndex;
        }

        void m_UI_RFLocalCtrl_passCSDevState(DLLImport.DEV_CS_STATUS_RESULT Reslut)
        {
            memoEdit1.Text = "";

            memoEdit1.Text += "\r\n\r\n校正源模块查询结果："
               + "\r\n\r\n起始频率为：" + Reslut.FreqBandStart + "(10KHz)"
               + "\r\n\r\n终止频率为：" + Reslut.FreqBandEnd + "(10KHz)"
               + "\r\n\r\n衰减值为：  " + Reslut.CS_GainValue + "(dBm)"
               + "\r\n\r\n工作模式为：" + Reslut.CS_WorkMode + "(校正源输出关闭，1校正源输出开启)"
               + "\r\n\r\n电源开关为：" + Reslut.CS_PowerStatus + "(0关闭，1开启)"
               + "\r\n\r\n工作电压为：" + Reslut.CS_WorkingVoltage + "(mV)"
               + "\r\n\r\n工作电流为：" + Reslut.CS_WorkingCurrent + "(mA)"
               + "\r\n\r\n工作温度为：" + Reslut.CS_Temperature + "(℃)";
        }

        void m_UI_RFLocalCtrl_passGSDevState(DLLImport.DEV_GS_STATUS_RESULT Reslut)
        {
            memoEdit1.Text = "";

            memoEdit1.Text += "\r\n\r\n测向开关模块查询结果："
               + "\r\n\r\n工作模式为：" + Reslut.GS_WorkMode + "(0校正状态，1天线状态)"
               + "\r\n\r\n电源开关为：" + Reslut.GS_PowerStatus + "(0关闭，1开启)"
               + "\r\n\r\n工作电压为：" + Reslut.GS_WorkingVoltage + "(mV)"
               + "\r\n\r\n工作电流为：" + Reslut.GS_WorkingCurrent + "(mA)"
               + "\r\n\r\n工作温度为：" + Reslut.GS_Temperature + "(℃)";
        }

        void m_UI_RFLocalCtrl_passGSDevIdentity(DLLImport.DEV_GS_IDENTITY_RESULT Reslut)
        {
            memoEdit1.Text = "";

            memoEdit1.Text += "\r\n\r\n测向开关模块身份查询结果："
               + "\r\n\r\n载板固件版本码为：          " + Reslut.GS_BoardVer
               + "\r\n\r\n固件版本号为：              " + Reslut.GS_Ver
               + "\r\n\r\n最大正常工作时钟频率码为：  " + Reslut.GS_MaxWorkingFreq
               + "\r\n\r\n公司名称码：                " + Reslut.GS_ComName
               + "\r\n\r\n模块类型码为：              " + Reslut.GS_ModuleType
               + "\r\n\r\n模块申购编号码为：          " + Reslut.GS_BoughtIndex;
        }


        void m_UI_RFLocalCtrl_passCSDevIdentity(DLLImport.DEV_CS_IDENTITY_RESULT Reslut)
        {
            memoEdit1.Text = "";

            memoEdit1.Text += "\r\n\r\n校正源模块身份查询结果："
               + "\r\n\r\n载板固件版本码为：          " + Reslut.CS_BoardVer
               + "\r\n\r\n固件版本号为：              " + Reslut.CS_Ver
               + "\r\n\r\n最大正常工作时钟频率码为：  " + Reslut.CS_MaxWorkingFreq
               + "\r\n\r\n公司名称码：                " + Reslut.CS_ComName
               + "\r\n\r\n模块类型码为：              " + Reslut.CS_ModuleType
               + "\r\n\r\n模块申购编号码为：          " + Reslut.CS_BoughtIndex;
        }

        void m_UI_RFLocalCtrl_passRFGainModeState(DLLImport.DEV_DevRF_GetGainMode_RESULT Result)
        {
            memoEdit1.Text = "";

            memoEdit1.Text += "\r\n\r\n射频增益模式查询结果：（0为MGC模式，1为AGC模式）"
               + "\r\n\r\n增益模式为：" + Result.Mode;
        }


        //06开机状态查询
        void m_UI_RFLocalCtrl_passDevPowerCheckSelf(DLLImport.DEV_CHECKSELF_RESULT Reslut)
        {
            memoEdit1.Text = "";
            memoEdit1.Text += "\r\n\r\n开机状态查询结果："
                           + "\r\n\r\n状态为：" + Reslut.Status + "(1 正常， 0 异常)";
        }

        //07系统时钟状态查询
        void m_UI_RFLocalCtrl_passSampleClockCheckSelf(DLLImport.DEV_CLKStatus_RESULT Reslut)
        {
            memoEdit1.Text = "";
            memoEdit1.Text += "\r\n\r\n系统时钟状态查询结果："
                           + "\r\n\r\n状态为：" + Reslut.Status + "(1 正常， 0 异常)";
        }

        //08射频通信状态查询
        void m_UI_RFLocalCtrl_passCommunityStatus(DLLImport.DEV_CHECKSELF_RESULT Reslut)
        {
            memoEdit1.Text = "";
            memoEdit1.Text += "\r\n\r\n射频通信状态查询结果："
                           + "\r\n\r\n查询板卡为：" + Reslut.DevID
                           + "\r\n\r\n状态为：" + Reslut.Status + "(1 正常， 0 异常)";
        }

        //09采集板aurora状态查询
        void m_UI_RFLocalCtrl_passAuroraStatus(DLLImport.DEV_CCAURORA_CHECKSELF_RESULT Reslut)
        {
            memoEdit1.Text = "";
            memoEdit1.Text += "\r\n\r\n采集板aurora状态查询结果："
                           + "\r\n\r\nAuroraX1状态为：" + Reslut.AuroraX1 + "(1 正常， 0 异常)"
                           + "\r\n\r\nAuroraX4_1状态为：" + Reslut.AuroraX4_1 + "(1 正常， 0 异常)"
                           + "\r\n\r\nAuroraX4_2状态为：" + Reslut.AuroraX4_2 + "(1 正常， 0 异常)";
        }

        //11时标切换200102LX
        void m_UI_RFLocalCtrl_passTimeDev(DLLImport.DEV_TIMEDEV_RESULT Reslut)
        {
            memoEdit1.Text = "";
            memoEdit1.Text += "\r\n\r\n时统模式查询结果："
                           + "\r\n\r\n当前时统模式为：" + Reslut.Mode + "(0 B码， 1 串码)";
        }

        //12GPS / BD类型控制指令200102LX
        void m_UI_RFLocalCtrl_passGPSBD(DLLImport.DEV_GPSBD_RESULT Reslut)
        {
            memoEdit1.Text = "";
            memoEdit1.Text += "\r\n\r\nGPS/BD模式查询结果："
                           + "\r\n\r\nGPS/BD模式为：" + Reslut.Mode + "(0 GPS， 1 BD)";
            barStaticItem3.Caption = Reslut.Mode + "时间:";
        }

        //18数字增益模式查询200102LX
        void m_UI_RFLocalCtrl_passGetCCGainMode(DLLImport.DEV_GetDigitalGainMode_RESULT Reslut)
        {
            memoEdit1.Text = "";
            memoEdit1.Text += "\r\n\r\n数字增益模式查询结果："
                           + "\r\n\r\n数字增益模式为：" + Reslut.Mode + "(0 MGC， 1 AGC)";
        }

        void m_UI_RFLocalCtrl_nbChangedCallBack(int BandNum, double NBDDCBandwidth, double NBDDCFreq)
        {
            if(m_DataProcessing != null)
                m_DataProcessing.Selected_NBBandWidth_Freq(BandNum, NBDDCBandwidth, NBDDCFreq);
        }

        void m_UI_RFLocalCtrl_wbChangedCallBack(int BandNum, double WBDDCFreq)
        {
            if (m_DataProcessing != null)
                m_DataProcessing.Selected_WBBandWidth_Freq(BandNum, WBDDCFreq);
        }

        void m_UI_RFLocalCtrl_passCtrlBack(int t)
        {
            m_DataProcessing.udpRecv.ClearQueue();
            m_DataProcessing.ClearQueue();
            m_DataProcessing.DataIQWaveform.ClearQueue();
            m_DataProcessing.DataFunction.ClearQueue();
            m_DataProcessing.DataFrequencySpectrum.ClearQueue();
            string Back = null;
            switch (t)
            {
                case 0:
                    Back = "射频控制成功";
                    break;
                case -2:
                    Back = "射频控制失败";
                    break;
                case -10:
                    Back = "射频控制未收到反馈指令";
                    break;
                case -100:
                    Back = "射频控制参数设置错误";
                    break;
                case -101:
                    Back = "射频控制网络错误，下发指令失败";
                    break;
                case -1:
                    Back = "网络未初始化，句柄未生成";
                    break;
                default:
                    break;
            }
            barStaticItem2.Caption = Back;
        }


        #endregion

        #region AD过载处理函数
        delegate int DelegateUpDateADOverloadStatus(_DEV_ADOverloadAlarm_RESULT Result);

        public int ADOverload(_DEV_ADOverloadAlarm_RESULT Result)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new DelegateUpDateADOverloadStatus(ADOverload), Result);
                return 1;
            }

            if (Result.DevID == 1)
            {
                richTextBox1.Text = Result.alarm[0]
           + "--" + Result.alarm[1]
           + "--" + Result.alarm[2]
           + "--" + Result.alarm[3]
           + "--" + Result.alarm[4]
           + "\r\n" + Result.alarm[5]
           + "--" + Result.alarm[6]
           + "--" + Result.alarm[7]
           + "--" + Result.alarm[8]
           + "--" + Result.alarm[9]
           + "\r\n" + Result.alarm[10]
           + "--" + Result.alarm[11]
           + "--" + Result.alarm[12]
           + "--" + Result.alarm[13]
           + "--" + Result.alarm[14]
           + "\r\n" + Result.alarm[15]
           + "--" + Result.alarm[16]
           + "--" + Result.alarm[17]
           + "--" + Result.alarm[18]
           + "--" + Result.alarm[19];
            }

            /*else if (Result.DevID == 2)
            {
                richTextBox2.Text = Result.alarm[0]
           + "--" + Result.alarm[1]
           + "--" + Result.alarm[2]
           + "--" + Result.alarm[3]
           + "--" + Result.alarm[4]
           + "\r\n" + Result.alarm[5]
           + "--" + Result.alarm[6]
           + "--" + Result.alarm[7]
           + "--" + Result.alarm[8]
           + "--" + Result.alarm[9]
           + "\r\n" + Result.alarm[10]
           + "--" + Result.alarm[11]
           + "--" + Result.alarm[12]
           + "--" + Result.alarm[13]
           + "--" + Result.alarm[14]
           + "\r\n" + Result.alarm[15]
           + "--" + Result.alarm[16]
           + "--" + Result.alarm[17]
           + "--" + Result.alarm[18]
           + "--" + Result.alarm[19];
            }

            else if (Result.DevID == 3)
            {
                richTextBox3.Text = Result.alarm[0]
           + "--" + Result.alarm[1]
           + "--" + Result.alarm[2]
           + "--" + Result.alarm[3]
           + "--" + Result.alarm[4]
           + "\r\n" + Result.alarm[5]
           + "--" + Result.alarm[6]
           + "--" + Result.alarm[7]
           + "--" + Result.alarm[8]
           + "--" + Result.alarm[9]
           + "\r\n" + Result.alarm[10]
           + "--" + Result.alarm[11]
           + "--" + Result.alarm[12]
           + "--" + Result.alarm[13]
           + "--" + Result.alarm[14]
           + "\r\n" + Result.alarm[15]
           + "--" + Result.alarm[16]
           + "--" + Result.alarm[17]
           + "--" + Result.alarm[18]
           + "--" + Result.alarm[19];
            }

            else if (Result.DevID == 4)
            {
                richTextBox4.Text = Result.alarm[0]
           + "--" + Result.alarm[1]
           + "--" + Result.alarm[2]
           + "--" + Result.alarm[3]
           + "--" + Result.alarm[4]
           + "\r\n" + Result.alarm[5]
           + "--" + Result.alarm[6]
           + "--" + Result.alarm[7]
           + "--" + Result.alarm[8]
           + "--" + Result.alarm[9]
           + "\r\n" + Result.alarm[10]
           + "--" + Result.alarm[11]
           + "--" + Result.alarm[12]
           + "--" + Result.alarm[13]
           + "--" + Result.alarm[14]
           + "\r\n" + Result.alarm[15]
           + "--" + Result.alarm[16]
           + "--" + Result.alarm[17]
           + "--" + Result.alarm[18]
           + "--" + Result.alarm[19];
            }

            else if (Result.DevID == 5)
            {
                richTextBox5.Text = Result.alarm[0]
           + "--" + Result.alarm[1]
           + "--" + Result.alarm[2]
           + "--" + Result.alarm[3]
           + "--" + Result.alarm[4]
           + "\r\n" + Result.alarm[5]
           + "--" + Result.alarm[6]
           + "--" + Result.alarm[7]
           + "--" + Result.alarm[8]
           + "--" + Result.alarm[9]
           + "\r\n" + Result.alarm[10]
           + "--" + Result.alarm[11]
           + "--" + Result.alarm[12]
           + "--" + Result.alarm[13]
           + "--" + Result.alarm[14]
           + "\r\n" + Result.alarm[15]
           + "--" + Result.alarm[16]
           + "--" + Result.alarm[17]
           + "--" + Result.alarm[18]
           + "--" + Result.alarm[19];
            }*/

            return 1;
        }


        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}