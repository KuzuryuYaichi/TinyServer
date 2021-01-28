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
using Common.ComponentCs.Log;
using Common.ComponentCs;

namespace XD_DBDW_Server
{
    public partial class UI_IQWaveform : DevExpress.XtraEditors.XtraUserControl
    {
        public delegate void PushDataHandler(IQData data);

        public UI_IQWaveform()
        {
            InitializeComponent();
            InitScope();
        }

        public void PushDataAttribute(IQData t)
        {
            double tScope1YAxisMin = -t.IData.Max() * 3;
            double tScope1YAxisMax = t.IData.Max() * 3;
            try
            {
                scope1.BeginInvoke(new Action(() =>
                {
                    scope1.YAxis.Max.Tick.Value = tScope1YAxisMax;
                    scope1.YAxis.Min.Tick.Value = tScope1YAxisMin;
                }));
            }
            catch (Exception)
            {

            }
            PushFrequencyData(t);
            //PushFrequencyQData(t);--lyh
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
            scope1.YAxis.Max.DataValue = 1000;
            scope1.YAxis.Max.Value = 1000;
            scope1.YAxis.Min.DataValue = -1000;
            scope1.YAxis.Min.Value = -1000;
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
        private void PushFrequencyData(IQData data)
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
                    scope1.Channels[0].Data.SetYData(data.IData);
                    scope1.Channels[1].Data.SetYData(data.QData);
                }
            }
            catch (Exception e)
            {
                //LogUtil.WriteAndAuthLogFile(e.ToString(), ELogType.Error, "系统日志");
            }
        }

        private void PushFrequencyQData(IQData data)
        {
            try
            {
                if (scope1.InvokeRequired)
                {
                    PushDataHandler handler = new PushDataHandler(PushFrequencyQData);
                    scope1.Invoke(handler, new object[] { data });
                }
                else
                {
                    //scope1.Channels[0].Data.SetYData(data.IData);
                    scope1.Channels[1].Data.SetYData(data.QData);
                }
            }
            catch (Exception e)
            {
                //LogUtil.WriteAndAuthLogFile(e.ToString(), ELogType.Error, "系统日志");
            }
        }
    }
}
