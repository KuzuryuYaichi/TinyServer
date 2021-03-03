using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraBars.Docking2010.Views;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Navigation;
using System.Globalization;
using System.IO;

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
        private UI_FrequencySpectrum m_UI_FrequencySpectrum;
        private UI_IQWaveform m_UI_IQWaveform;
        private UI_WaterfallPlot m_UI_WaterfallPlot;
        private DataProcessing m_DataProcessing;
        private FileProcessing m_FileProcessing;

        private string m_NetLocalIP;
        private int m_NetLocalPort;
        public string m_NetGroupIP;
        public int band;
        public string[] UdpMulticastGroup = { "239.1.1.200", "239.1.1.201", "239.1.1.202", "239.1.1.203", "239.1.1.204" };

        #region 初始化
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            WriteXml();
            DataAlgorithm.Filter.LoadFilterPara();

            this.barToggleSwitchItem3.CheckedChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.BarToggleSwitchItem3_CheckedChanged);
            this.barToggleSwitchItem2.CheckedChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.BarToggleSwitchItem2_CheckedChanged);
            this.barToggleSwitchItem4.CheckedChanged += new DevExpress.XtraBars.ItemClickEventHandler(this.BarToggleSwitchItem4_CheckedChanged);

            #region 网络初始化
            try
            {
                m_DataProcessing = new DataProcessing();//初始化数据处理类
                m_FileProcessing = new FileProcessing(m_DataProcessing, this);
                m_FileProcessing.udpRecvInit(m_NetLocalIP, m_NetLocalPort);//进数据处理类
            }
            catch (Exception ex)
            {

            }
            #endregion

            #region 委托传递数据
            m_FileProcessing.udpRecvNB.passTime += new udpRecv.PassTime(udpRecv_passTime);//时标信息的委托
            m_FileProcessing.udpRecvFFT.passTime += new udpRecv.PassTime(udpRecv_passTime);//时标信息的委托
            m_DataProcessing.DataFrequencySpectrum.passPowerAndFreq += new DataFrequencySpectrum.PassPowerAndFreq(DataFrequencySpectrum_passPowerAndFreq);//频域数据的委托
            m_DataProcessing.DataIQWaveform.passIQData += new DataIQWaveform.PassIQData(DataIQWaveform_passIQData);//IQ数据的委托
            #endregion

            #region UI
            FrequencySpectrumUserControl = CreateUserControl("FrequencySpectrum");
            IQWaveformUserControl = CreateUserControl("IQWaveform");
            WaterfallPlotUserControl = CreateUserControl("WaterfallPlot");

            #region UI_FrequencySpectrum
            m_UI_FrequencySpectrum = new UI_FrequencySpectrum();
            this.FrequencySpectrumUserControl.Controls.Clear();
            this.FrequencySpectrumUserControl.Controls.Add(m_UI_FrequencySpectrum);
            this.m_UI_FrequencySpectrum.Dock = DockStyle.Fill;
            #endregion

            //accordionControl.SelectedElement = FrequencySpectrumAccordionControlElement;

            #endregion
        }

        public void SetReturnStatus(string text)
        {
            if (this.IsHandleCreated)
                this.BeginInvoke(new Action(() =>
                {
                    barStaticItem2.Caption = text;
                }));
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

                        //switch (timeInfo.satelliteInfo.time_state)
                        //{
                        //    //B码=0、GPS/BD=0
                        //    case 0:
                        //        barStaticItem3.Caption =
                        //        "B码：" + System.DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒", dtFormat)
                        //        + " | BD/GPS时间：" + System.DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒", dtFormat);
                        //        break;

                        //    //B码=0、GPS/BD=1
                        //    case 1:
                        //        barStaticItem3.Caption =
                        //        "B码：" + System.DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒", dtFormat) + nanosecond
                        //        + " | BD/GPS时间：" + timeInfo.satelliteInfo.year.ToString() + "年"
                        //        + timeInfo.satelliteInfo.month.ToString() + "月"
                        //        + timeInfo.satelliteInfo.day.ToString() + "日"
                        //        + timeInfo.satelliteInfo.hour.ToString() + "时"
                        //        + timeInfo.satelliteInfo.minute.ToString("d2") + "分"
                        //        + timeInfo.satelliteInfo.second.ToString("d2") + "秒"
                        //        + nanosecond;
                        //        break;
                        //    //B码=1、GPS/BD=0
                        //    case 2:
                        barStaticItem3.Caption =
                        "B码：" + timeInfo.year.ToString() + "年"
                        + timeInfo.month.ToString() + "月"
                        + timeInfo.day_offset.ToString() + "日"
                        + timeInfo.hour.ToString() + "时"
                        + timeInfo.minute.ToString("d2") + "分"
                        + timeInfo.second.ToString("d2") + "秒"
                        + nanosecond;
                        //+ " | BD/GPS时间：" + System.DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒", dtFormat) + nanosecond;
                        //        break;

                        //    //B码=1、GPS/BD=1
                        //    case 3:
                        //        barStaticItem3.Caption =
                        //        "B码：" + timeInfo.year.ToString() + "年"
                        //        + timeInfo.month.ToString() + "月"
                        //        + timeInfo.day_offset.ToString() + "日"
                        //        + timeInfo.hour.ToString() + "时"
                        //        + timeInfo.minute.ToString("d2") + "分"
                        //        + timeInfo.second.ToString("d2") + "秒"
                        //        + nanosecond
                        //        + " | BD/GPS时间：" + timeInfo.satelliteInfo.year.ToString() + "年"
                        //        + timeInfo.satelliteInfo.month.ToString() + "月"
                        //        + timeInfo.satelliteInfo.day.ToString() + "日"
                        //        + timeInfo.satelliteInfo.hour.ToString() + "时"
                        //        + timeInfo.satelliteInfo.minute.ToString("d2") + "分"
                        //        + timeInfo.satelliteInfo.second.ToString("d2") + "秒"
                        //        + nanosecond;
                        //        break;
                        //    default:
                        //        break;
                        //}
                    }));
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
                //if (e.Document.Caption == "FrequencySpectrum") accordionControl.SelectedElement = IQWaveformAccordionControlElement;
                //else accordionControl.SelectedElement = FrequencySpectrumAccordionControlElement;
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

        private void barButtonItem19_ItemClick(object sender, ItemClickEventArgs e)
        {
            m_DataProcessing.DataFrequencySpectrum.StartSave();
        }

        //private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        //{
        //    m_DataProcessing.DataFunction.StartCheckTime();
        //    m_DataProcessing.DataFunction.passDeleTimeSign += new DataFunction.DeleTimeSign(DataFunction_passDeleTimeSign);
        //}

        //void DataFunction_passDeleTimeSign(DataTime datatime)
        //{
        //    memoEdit1.Text = "";
        //    memoEdit1.Text += "时标检测：" + datatime.year.ToString() + "年" + datatime.month.ToString() + "月"
        //                    + datatime.day.ToString() + "日" + datatime.hour.ToString() + "时" + datatime.minute.ToString() + "分"
        //                    + datatime.second.ToString() + "秒" + datatime.millisecond.ToString() + "毫秒" + datatime.microsecond.ToString() + "微秒";
        //}
        
        #endregion

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
            m_NetLocalIP = m_XmlProcessing.Read_LocalIP();//初始化万兆网IP
            m_NetLocalPort = Convert.ToInt32(m_XmlProcessing.Read_LocalPort());//初始化万兆网端口

            //string[] bandItem = new string[16];
            //for (int i = 0; i < 16; ++i)
            //{
            //    bandItem[i] = (4 * i + 1) + "-" + (4 * i + 4);
            //}

            string[] bandItem = new string[64];
            for (int i = 0; i < 64; ++i)
            {
                bandItem[i] = "" + (i + 1);
            }

            repositoryItemComboBox3.Items.Clear();
            this.repositoryItemComboBox3.Items.AddRange(bandItem);
            repositoryItemComboBox4.Items.Clear();
            this.repositoryItemComboBox4.Items.AddRange(new object[] { "0", "1", "2", "3", "4", "5", "6", "7", "8" });
            repositoryItemComboBox5.Items.Clear();
            this.repositoryItemComboBox5.Items.AddRange(new object[] {"1250Hz","625Hz","312.5Hz","156.25Hz", "2500Hz"});
            this.repositoryItemComboBox5.NullText = "2500Hz";
            this.repositoryItemComboBox11.Items.AddRange(new object[] { "无", "Rect", "Hanning", "Hamming", "Blackman", "Gaussion" });
            this.repositoryItemComboBox11.NullText = "无";
            
            barStaticItem5.Caption = "XD_DBDW测试程序";
        }
        #endregion

        private void FrequencySpectrumBtn_ItemClick(object sender, ItemClickEventArgs e)
        {
            XtraUserControl userControl = FrequencySpectrumUserControl;
            tabbedView.AddDocument(userControl);
            tabbedView.ActivateDocument(userControl);

            m_UI_FrequencySpectrum = new UI_FrequencySpectrum();
            this.FrequencySpectrumUserControl.Controls.Clear();
            this.FrequencySpectrumUserControl.Controls.Add(m_UI_FrequencySpectrum);
            this.m_UI_FrequencySpectrum.Dock = DockStyle.Fill;
        }

        private void IQWaveformBtn_ItemClick(object sender, ItemClickEventArgs e)
        {
            XtraUserControl userControl = IQWaveformUserControl;
            tabbedView.AddDocument(userControl);
            tabbedView.ActivateDocument(userControl);

            m_UI_IQWaveform = new UI_IQWaveform();
            this.IQWaveformUserControl.Controls.Clear();
            this.IQWaveformUserControl.Controls.Add(m_UI_IQWaveform);
            this.m_UI_IQWaveform.Dock = DockStyle.Fill;
        }

        private void WaterFallBtn_ItemClick(object sender, ItemClickEventArgs e)
        {
            XtraUserControl userControl = WaterfallPlotUserControl;
            tabbedView.AddDocument(userControl);
            tabbedView.ActivateDocument(userControl);

            m_UI_WaterfallPlot = new UI_WaterfallPlot();
            this.WaterfallPlotUserControl.Controls.Clear();
            this.WaterfallPlotUserControl.Controls.Add(m_UI_WaterfallPlot);
            this.m_UI_WaterfallPlot.Dock = DockStyle.Fill;
        }

        private void tmp_Click(object sender, EventArgs e)
        {
            byte[] test_data = new byte[4 * 512];
            string path = (string)((AccordionControlElement)sender).Tag;
            FileStream fs = new FileStream(path, FileMode.Open);
            fs.Read(test_data, 0, (int)test_data.Length);
            fs.Close();
            int type = 1;
            //m_DataProcessing.FilePass(type, test_data);
        }

        private void OpenFileBtn_ItemClick(object sender, ItemClickEventArgs e)
        {
            XtraFolderBrowserDialog OpenFolder = new XtraFolderBrowserDialog();
            OpenFolder.ShowNewFolderButton = false;
            OpenFolder.UseParentFormIcon = true;
            OpenFolder.ShowDragDropConfirmation = true;
            if (OpenFolder.ShowDialog() == DialogResult.OK)
            {
                string path = OpenFolder.SelectedPath;
                DirectoryInfo fi = new DirectoryInfo(path);
                if (!fi.Exists)
                    return;
                mainAccordionGroup.Elements.Clear();
                List<AccordionControlElement> ac = new List<AccordionControlElement>();
                foreach (FileInfo file in fi.GetFiles())
                {
                    if (file.Extension == ".dat")
                    {
                        AccordionControlElement tmp = new AccordionControlElement(DevExpress.XtraBars.Navigation.ElementStyle.Item);
                        tmp.Text = file.Name;
                        tmp.Tag = file.FullName;
                        tmp.Click += new System.EventHandler(this.tmp_Click); ;
                        ac.Add(tmp);
                    }
                }
                AccordionControlElement[] acArray = ac.ToArray();
                mainAccordionGroup.Elements.AddRange(ac.ToArray());
            }
        }

        //自检
        private void Ctrl_SelfCheck()
        {
            OrderController oc = new OrderController();
            uint ichan = 0;
            uint success = 1;
            m_FileProcessing.udpRecvOrder.SendOrder(oc.pack(ichan, 0x10000002, success));
        }

        //自检
        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            Ctrl_SelfCheck();
        }

        //射频MGC-AGC开关
        private void Ctrl_RF_AGC_MGC_Switch(bool checkeStatus)
        {
            if (barEditItem10.EditValue != null)
            {
                Ctrl_RF_Desc(Decimal.ToUInt32((decimal)barEditItem10.EditValue), checkeStatus);
            }
            //barEditItem10.Enabled = !checkeStatus;
            //barButtonItem15.Enabled = !checkeStatus;
        }

        private void BarToggleSwitchItem2_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            bool checkeStatus = ((DevExpress.XtraBars.BarToggleSwitchItem)sender).Checked;
            Ctrl_RF_AGC_MGC_Switch(checkeStatus);
        }

        //射频衰减设置
        private void Ctrl_RF_Desc(uint value, bool checkedStatus)
        {
            OrderController oc = new OrderController();
            uint ichan = 0;
            uint Checked = (uint)(checkedStatus ? 1 : 0);
            m_FileProcessing.udpRecvOrder.SendOrder(oc.pack(ichan, 0x10000103, Checked, value));
        }

        private void BarToggleSwitchItem4_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            bool checkeStatus = ((DevExpress.XtraBars.BarToggleSwitchItem)sender).Checked;
            Is_Record(checkeStatus);
        }

        private void Is_Record(bool checkedStatus)
        {
            OrderController oc = new OrderController();
            uint ichan = 0;
            uint Checked = (uint)(checkedStatus ? 1 : 0);
            m_FileProcessing.udpRecvOrder.SendOrder(oc.pack(ichan, 0x10000bbb, Checked));
        }

        private void barButtonItem15_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (barEditItem10.EditValue != null)
            {
                bool checkeStatus = ((DevExpress.XtraBars.BarToggleSwitchItem)barToggleSwitchItem2).Checked;
                Ctrl_RF_Desc(Decimal.ToUInt32((decimal)barEditItem10.EditValue), checkeStatus);
            }
        }

        //射频状态查询
        private void Ctrl_CheckRfStatus()
        {
            OrderController oc = new OrderController();
            uint ichan = 0;
            m_FileProcessing.udpRecvOrder.SendOrder(oc.pack(ichan, 0x10000eee, 0));
        }

        private void barButtonItem3_ItemClick(object sender, ItemClickEventArgs e)
        {
            Ctrl_CheckRfStatus();
        }

        //射频增益模式查询
        private void Ctrl_CheckRFGainMode()
        {
            OrderController oc = new OrderController();
            uint ichan = 0;
            m_FileProcessing.udpRecvOrder.SendOrder(oc.pack(ichan, 0x10000fff, 0));
        }

        private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        {
            Ctrl_CheckRFGainMode();
        }

        //射频AGC参数设置
        private void barButtonItem19_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            OrderController oc = new OrderController();
            if (barEditItem12.EditValue != null && barEditItem13.EditValue != null && barEditItem15.EditValue != null && barEditItem16.EditValue != null)
            {
                uint a = decimal.ToUInt32((decimal)barEditItem12.EditValue);
                uint b = (uint)(decimal.ToInt32((decimal)barEditItem13.EditValue) + 6);
                uint c = decimal.ToUInt32((decimal)barEditItem15.EditValue);
                uint d = decimal.ToUInt32((decimal)barEditItem16.EditValue);

                byte[] dd = new byte[3];
                dd[0] = (byte)(((a & 0xF) << 4) | (b & 0xF));
                dd[1] = (byte)(((c & 0xF) << 4) | (d & 0xF));
                dd[2] = (byte)(d & 0xFF);
                uint ichan = 0;
                uint data = (uint)((dd[2] << 16) | (dd[1] << 8) | dd[0]);
                m_FileProcessing.udpRecvOrder.SendOrder(oc.pack(ichan, 0x10000bbb, data));
            }
        }

        //窄带通道选择
        private void repositoryItemComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            int select = ((DevExpress.XtraEditors.ComboBoxEdit)sender).SelectedIndex;
            //int[] channels = new int[4];
            //channels[0] = select * 4 + 0;
            //channels[1] = select * 4 + 1;
            //channels[2] = select * 4 + 2;
            //channels[3] = select * 4 + 3;
            int channel = select;
            m_DataProcessing.Set_NB_Filter(channel);
        }

        //窗函数
        private void repositoryItemComboBox11_SelectedIndexChanged(object sender, EventArgs e)
        {
            OrderController oc = new OrderController();
            uint ichan = 0;
            int select = ((DevExpress.XtraEditors.ComboBoxEdit)sender).SelectedIndex;
            m_FileProcessing.udpRecvOrder.SendOrder(oc.pack(ichan, 0x10000002, (uint)select));
        }

        //平滑数
        private void repositoryItemComboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            OrderController oc = new OrderController();
            uint ichan = 0;
            int select = ((DevExpress.XtraEditors.ComboBoxEdit)sender).SelectedIndex;
            m_FileProcessing.udpRecvOrder.SendOrder(oc.pack(ichan, 0x10000203, (uint)select));
        }

        //分辨率
        private void repositoryItemComboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            OrderController oc = new OrderController();
            uint ichan = 0;
            int select = ((DevExpress.XtraEditors.ComboBoxEdit)sender).SelectedIndex;
            m_FileProcessing.udpRecvOrder.SendOrder(oc.pack(ichan, 0x10000202, (uint)select));
        }

        //24dB开关
        private void BarToggleSwitchItem3_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OrderController oc = new OrderController();
            uint ichan = 0;
            bool checkeStatus = ((DevExpress.XtraBars.BarToggleSwitchItem)sender).Checked;
            m_FileProcessing.udpRecvOrder.SendOrder(oc.pack(ichan, 0x10000ccc, (uint)(checkeStatus ? 1 : 0)));
        }

        //数据类型选择
        private void RepositoryItemComboBox12_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            int select = ((DevExpress.XtraEditors.ComboBoxEdit)sender).SelectedIndex;
            m_DataProcessing.SetDataType(select);
        }

        private void barButtonItem20_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (barEditItem14.EditValue != null)
            {
                uint ichan = Decimal.ToUInt32((decimal)barEditItem14.EditValue) - 1;
                if (barEditItemFreq.EditValue != null)
                {
                    OrderController oc = new OrderController();
                    uint freq = Decimal.ToUInt32((decimal)barEditItemFreq.EditValue * 1000000);
                    m_FileProcessing.udpRecvOrder.SendOrder(oc.pack(ichan, 0x10000101, freq));
                }
                if (barEditItemBandWidth.EditValue != null)
                {
                    OrderController oc = new OrderController();
                    uint bandwidth = UInt32.Parse((string)barEditItemBandWidth.EditValue);
                    m_FileProcessing.udpRecvOrder.SendOrder(oc.pack(ichan, 0x10000102, bandwidth));
                }
                if (barEditItemMode.EditValue != null)
                {
                    OrderController oc = new OrderController();
                    uint mode = (uint)repositoryItemComboBox14.Items.IndexOf(barEditItemMode.EditValue);
                    m_FileProcessing.udpRecvOrder.SendOrder(oc.pack(ichan, 0x10000302, mode));
                }
            }
        }

        private void barButtonItem21_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (barEditItem14.EditValue != null)
            {
                uint ichan = Decimal.ToUInt32((decimal)barEditItem14.EditValue) - 1;
                if (barEditItemCW.EditValue != null)
                {
                    OrderController oc = new OrderController();
                    uint CW_Value = decimal.ToUInt32((decimal)barEditItemCW.EditValue);
                    m_FileProcessing.udpRecvOrder.SendOrder(oc.pack(ichan, 0x10000303, CW_Value));
                }
            }
        }

        //平滑次数
        private void repositoryItemComboBox17_SelectedIndexChanged(object sender, EventArgs e)
        {
            OrderController oc = new OrderController();
            uint ichan = 0;
            int select = ((DevExpress.XtraEditors.ComboBoxEdit)sender).SelectedIndex;
            m_FileProcessing.udpRecvOrder.SendOrder(oc.pack(ichan, 0x10000ddd, (uint)select));
        }
    }
}