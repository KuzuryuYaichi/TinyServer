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
    public partial class UI_AGC : UserControl
    {
        public UI_AGC()
        {
            InitializeComponent();
        }
        private XmlProcessing m_XmlProcessing = new XmlProcessing();

        //13AGC增益值查询
        private void button28_Click(object sender, EventArgs e)
        {
            int DevID = comboBox39.SelectedIndex + 1;
            //int Channel = comboBox41.SelectedIndex + 1;
            DLLImport.DEV_GetDigitalGainValueJGZC_RESULT Reslut = new DLLImport.DEV_GetDigitalGainValueJGZC_RESULT();
            int ret =DLLImport.GetDigitalGainValueJGZC(DevID, ref Reslut);
            //int DevType = Convert.ToInt32(m_XmlProcessing.ReadDevID());
            //0427JGZC
            for (int i = 0; i < 60; i++)
            {
                string text = string.Format("子带{0}数字增益为：{1}",i, Reslut.DigitalGain[i].ToString());
                richTextBox9.AppendText(text+"\r\n");                                      
            }
            
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //richTextBox7.Text = " ";
            //richTextBox8.Text = " ";
            richTextBox9.Text = " ";
        }
    }
}

