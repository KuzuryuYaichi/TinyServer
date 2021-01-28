using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Threading;
using System.Net;

namespace XD_DBDW_Server
{
    public partial class UI_NetConfigData : DevExpress.XtraEditors.XtraUserControl
    {
        private Transform Transform = new Transform();
        public UI_NetConfigData()
        {
            InitializeComponent();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            string IP = textEdit1.Text;
            int DevNetID = Convert.ToInt32(comboBox6.Text);
            int BoardID = 0;
            int NetID = 0;
            if (DevNetID <= 4)
            {
                BoardID = 1;
                NetID = DevNetID;
            }
            else
            {
                BoardID = 0;
                NetID = DevNetID - 4;
            }
            DLLImport.DataSourceIP_B(BoardID, NetID, IP);
            Thread.Sleep(1);
            int datasouce = comboBox2.SelectedIndex;
            DLLImport.DataReset_X_B(BoardID, 1, datasouce);
            Thread.Sleep(1);
            DLLImport.DataReset_X_B(BoardID, 0, datasouce);
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            //int BandID = Convert.ToInt32(comboBox3.Text);
            string IP = textEdit2.Text;
            UInt16 srcPort = Convert.ToUInt16(textEdit4.Text);
            UInt16 disPort = Convert.ToUInt16(textEdit3.Text);
            int NetID = Convert.ToInt32(comboBox5.Text);
            int BoardID = 0;
            if (NetID < 5)
            {
                BoardID = 1;
            }
            else
            {
                BoardID = 0;
                NetID = NetID - 4;
            }
            int ConnectID = Convert.ToInt32(textEdit5.Text);
            //Transform.Transform_TryBand(BandID, ref BoardID, ref NetID, ref ConnectID);
            DLLImport.DataDestIP_B(BoardID, NetID, ConnectID, IP, disPort, srcPort);
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            //int BandID = Convert.ToInt32(comboBox1.Text);
            int Enable = comboBox4.SelectedIndex;
            int NetID = Convert.ToInt32(comboBox5.Text);
            int BoardID = 0;
            //if (NetID < 5)
            //{
            //    BoardID = 1;
            //}
            //else
            //{
            //    BoardID = 0;
            //    NetID = NetID - 4;
            //}
            BoardID = 0;
            NetID = 3;

            int ConnectID = Convert.ToInt32(textEdit5.Text);
            //Transform.Transform_TryBand(BandID, ref BoardID, ref NetID, ref ConnectID);
            DLLImport.DataLinkEnable_B(BoardID, NetID, ConnectID, Enable);
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            int Enable = comboBox4.SelectedIndex;
            DLLImport.DataAllEnable(Enable);
        }


    }
}
