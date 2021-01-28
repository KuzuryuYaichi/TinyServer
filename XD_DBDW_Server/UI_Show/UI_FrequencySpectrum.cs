using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Mitov.PlotLab;

namespace XD_DBDW_Server
{
    public partial class UI_FrequencySpectrum : DevExpress.XtraEditors.XtraUserControl
    {

        public delegate void PushDataHandler(double[] data);
        string Mark_max;

        public UI_FrequencySpectrum()
        {
            InitializeComponent();
            InitScope();
        }

        //频域显示参数LX
        public void PushDataAttribute(PowerAndFreq t)
        {
            double tScope1XAxisMin = (double)(t.StartFreq);
            double tScope1XAxisMax = (double)(t.StopFreq);
            try
            {
                scope1.BeginInvoke(new Action(() =>
                {
                    scope1.XAxis.Max.Tick.Value = tScope1XAxisMax;
                    scope1.XAxis.MajorTicks.Step = 0.640 / t.Power.Length;
                    scope1.XAxis.Min.Tick.Value = tScope1XAxisMin;
                }));
            }
            catch (Exception)
            {

            }
            PushFrequencyData(t.Power);
            Mark_max = t.Power.Max().ToString("0.00");
        }

        /// <summary>   
        /// 初始化频谱显示控件
        /// </summary>
        private void InitScope()
        {
            scope1.Zooming.Mode = DisplayZoomMode.XAxis;
            scope1.XAxis.MajorTicks.Mode = MajorTicksMode.Auto;
            scope1.XAxis.Min.Tick.AutoScale = false;
            scope1.XAxis.Max.Tick.AutoScale = false;
            scope1.XAxis.AxisLabel.Text = " MHz";

            scope1.XAxis.MajorTicks.StartFrom.Mode = MajorTicksMode.Override;

            // 隐藏网站panel -----------------
            Panel panel = new System.Windows.Forms.Panel();
            panel.SuspendLayout();
            panel.BackColor = System.Drawing.Color.Black;
            panel.Location = new System.Drawing.Point(0, 0);
            panel.Size = new System.Drawing.Size(584, 15);
            panel.ResumeLayout(false);
            panel.PerformLayout();
            scope1.Controls.Add(panel);

            //----------------------------------------------------------------------//

            scope1.Title.Text = "";
            scope1.Labels[0].Visible = false;
            scope1.YAxis.Max.DataValue = 20;
            scope1.YAxis.Max.Value = 20;
            scope1.YAxis.Min.DataValue = -140;
            scope1.YAxis.Min.Value = -140;
            scope1.YAxis.AutoScaling.Enabled = false;
            scope1.YAxis.AxisLabel.Text = " dBm";
            scope1.YAxis.Reversed = false;//有小到大（升）

            ///坐标精度
            scope1.YAxis.Format.FixedPrecision = true;//默认设置
            scope1.YAxis.Format.Precision = 2;//精度2位
            scope1.YAxis.Format.PrecisionMode = Mitov.PlotLab.PrecisionMode.General;//一般
            ///网格刻线
            scope1.YAxis.DataView.Lines.Visible = true;



            scope1.Hold = false;

            scope1.RefreshInterval = ((uint)(100u));

        }

        /// <summary>
        /// 更新频谱数据
        /// </summary>
        /// <param name="data"></param>
        private void PushFrequencyData(double[] data)
        {
            try
            {
                if (scope1.InvokeRequired)
                {
                    PushDataHandler handler = new PushDataHandler(PushFrequencyData);
                    scope1.Invoke(handler, new object[] { data });
                }
                else
                {
                    scope1.Channels[0].Data.SetYData(data);
                }
            }
            catch (Exception e)
            {
                //LogUtil.WriteAndAuthLogFile(e.ToString(), ELogType.Error, "系统日志");

            }
        }
        //增加PEAK功能
        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox1.AppendText((Mark_max) + " dBm");
        }
    }
}
