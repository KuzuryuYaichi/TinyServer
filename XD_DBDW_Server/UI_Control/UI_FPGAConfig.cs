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
    public partial class UI_FPGAConfig : DevExpress.XtraEditors.XtraUserControl
    {
        public delegate void PassIPEndPoint(IPEndPoint t);
        public event PassIPEndPoint passIPEndPoint;
        private XmlNode LocalIPNode;
        private XmlNode DevIPNode;
        private XmlNode CC_PortNode;
        private XmlNode RC_PortNode;
        private XmlNode Net10G_LocalIPNode;
        private XmlNode Net10G_LocalPortNode;
        private XmlNode Net10G_GroupIPNode;

        public UI_FPGAConfig()
        {
            InitializeComponent();

            comboBox6.Text = "1";
            comboBox1.Text = "1";

            WriteXml();

            string hostName = Dns.GetHostName();
            IPAddress[] localNetIP = Dns.GetHostAddresses(hostName);
            
            string[] LocalIP = new string[localNetIP.Length];
            if (localNetIP != null && localNetIP.Length > 0)
            {
                for (int i = 0; i < localNetIP.Length; i++)
                {
                    LocalIP[i] = localNetIP[i].ToString();
                    comboBox5.Items.Add(LocalIP[i]);
                    comboBox2.Items.Add(LocalIP[i]);
                }
            }
        }

        private void WriteXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"Config/NetConfig.xml");
            XmlNode configNode = doc.SelectSingleNode("config");
            LocalIPNode = configNode.SelectSingleNode("LocalIP");
            DevIPNode = configNode.SelectSingleNode("DevIP");
            CC_PortNode = configNode.SelectSingleNode("CC_Port");
            RC_PortNode = configNode.SelectSingleNode("RC_Port");
            Net10G_LocalIPNode = configNode.SelectSingleNode("Net10G_LocalIP");
            Net10G_LocalPortNode = configNode.SelectSingleNode("Net10G_LocalPort");
            Net10G_GroupIPNode = configNode.SelectSingleNode("Net10G_GroupIP");
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "BIN文件(*.bin)|*.bin";
            ofd.Multiselect = false;
            ofd.Title = "请选择采集板FPGA程序文件";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textEdit1.Text = ofd.FileName;
            }
        }

        private void simpleButton5_Click(object sender, EventArgs e)
        {
            splashScreenManager1.ShowWaitForm();
            string FilePath = textEdit1.Text;
            int FlashID = comboBox6.SelectedIndex + 1;
            int rc = DLLImport.DevRC_FPGALoad(FlashID, FilePath);
            splashScreenManager1.CloseWaitForm();
            if (rc != 0)
            {
                MessageBox.Show("错误{0}", rc.ToString());
            }
            else
            {
                MessageBox.Show("加载完成");
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            splashScreenManager1.ShowWaitForm();
            string FilePath = textEdit1.Text;
            int FlashID = comboBox1.SelectedIndex + 1;
            int rc = DLLImport.DevCC_FPGALoad(FlashID, FilePath);
            splashScreenManager1.CloseWaitForm();
            if (rc != 0)
            {
                MessageBox.Show("错误{0}", rc.ToString());
            }
            else
            {
                MessageBox.Show("加载完成");
            }
        }

        private void simpleButton7_Click(object sender, EventArgs e)
        {
            string LocalCtrlIP = comboBox5.Text;
            DLLImport.LocalNetConfig_IP(LocalCtrlIP);

            LocalIPNode.InnerText = LocalCtrlIP;

            MessageBox.Show("请重启程序");
            WindowApp.RestartApplication();
        }

        private void simpleButton6_Click(object sender, EventArgs e)
        {
            string DevNetIP = textEdit2.Text;
            DLLImport.DevNetConfig_IP(DevNetIP);
            DevIPNode.InnerText = DevNetIP;
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            UInt16 CC_Port = Convert.ToUInt16(textEdit3.Text);
            DLLImport.DevNetConfig_CCPort(CC_Port);
            CC_PortNode.InnerText = CC_Port.ToString();
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            UInt16 RC_Port = Convert.ToUInt16(textEdit4.Text);
            DLLImport.DevNetConfig_FCPort(RC_Port);
            RC_PortNode.InnerText = RC_Port.ToString();
        }

        private void simpleButton8_Click(object sender, EventArgs e)
        {
            IPAddress ip = IPAddress.Parse(comboBox2.Text);
            int port = Convert.ToInt32(textEdit5.Text);
            IPEndPoint LocalIPEndPoint = new IPEndPoint(ip, port);
            Net10G_LocalIPNode.InnerText = ip.ToString();
            Net10G_LocalPortNode.InnerText = port.ToString();
            if (passIPEndPoint != null)
            {
                passIPEndPoint(LocalIPEndPoint);
            }
        }

        private void simpleButton9_Click(object sender, EventArgs e)
        {
            int DevID = comboBox3.SelectedIndex;
            DLLImport.ResetDev(DevID);
        }

        //16FPGA版本号查询
        private void button30_Click(object sender, EventArgs e)
        {
            DLLImport.DEV_GetFPGAVersion_CHECKSELF_RESULT Reslut1 = new DLLImport.DEV_GetFPGAVersion_CHECKSELF_RESULT();
            DLLImport.DEV_GetFPGAVersionFC_CHECKSELF_RESULT Reslut2 = new DLLImport.DEV_GetFPGAVersionFC_CHECKSELF_RESULT();

            int ret1 = DLLImport.GetFPGAVersion(ref Reslut1);
            int ret2 = DLLImport.GetFPGAVersionFC(ref Reslut2);

            richTextBox11.Text = "\r\n\r\nFPGA版本号查询结果："

                           + "\r\n\r\n传输板主板："
                           + "\r\n\r\n" + "V_" + Reslut2.netK7_Type[0] + "_" + Reslut2.manufacturers + "_netK7_" + Reslut2.netK7_integer[0] + "." + Reslut2.netK7_decimal[0]
                           + "\r\n\r\n" + "V_" + Reslut2.netF1_Type[0] + "_" + Reslut2.manufacturers + "_netF1_" + Reslut2.netF1_integer[0] + "." + Reslut2.netF1_decimal[0]
                           + "\r\n\r\n" + "V_" + Reslut2.netF2_Type[0] + "_" + Reslut2.manufacturers + "_netF2_" + Reslut2.netF2_integer[0] + "." + Reslut2.netF2_decimal[0]

                           + "\r\n\r\n采集板1："
                           + "\r\n\r\n" + "V_" + Reslut1.sampleK7_Type[0] + "_" + Reslut1.manufacturers + "_sampleK7_" + Reslut1.sampleK7_integer[0] + "." + Reslut1.sampleK7_decimal[0]
                           + "\r\n\r\n" + "V_" + Reslut1.sampleV7_Type[0] + "_" + Reslut1.manufacturers + "_sampleV7_" + Reslut1.sampleV7_integer[0] + "." + Reslut1.sampleV7_decimal[0];
        }

    }
}
