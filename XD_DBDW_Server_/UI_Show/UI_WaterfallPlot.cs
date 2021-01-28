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
    public partial class UI_WaterfallPlot : DevExpress.XtraEditors.XtraUserControl
    {
        public UI_WaterfallPlot()
        {
            InitializeComponent();
            InitScope();
        }

        public void PushDataAttribute(PowerAndFreq t)
        {
            double tScope1XAxisMin = t.StartFreq * 1000 - 70;
            double tScope1XAxisMax = (t.StartFreq + 0.5) * 1000 + 70 - 640 / t.Power.Length;

            try
            {
                waterfall1.BeginInvoke(new Action(() =>
                {
                    waterfall1.YAxis.Max.Tick.Value = tScope1XAxisMax;
                    waterfall1.YAxis.MajorTicks.Step = 640 / t.Power.Length;
                    waterfall1.YAxis.Min.Tick.Value = tScope1XAxisMin;
                }));
            }
            catch (Exception)
            {
            }
           

            PushData(t.Power);
        }


        /// <summary>   
        /// 初始化频谱显示控件
        /// </summary>
        private void InitScope()
        {
            #region 瀑布图
            this.waterfall1.Location = new Point(0, 0);
            this.waterfall1.Size = new System.Drawing.Size(1100, 200);
            this.waterfall1.Name = "waterfall1";
            this.waterfall1.Title.Visible = true;
            this.waterfall1.Title.Font.Size = 9;
            this.waterfall1.Title.ViewSize.AutoSize = true;
            this.waterfall1.Title.Text = "  ";
            this.waterfall1.RefreshInterval = ((uint)(100u));
            this.waterfall1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waterfall1.Zooming.MouseWheelEnabled = true;
            this.waterfall1.Zooming.Mode = DisplayZoomMode.XAxis;

            this.waterfall1.Vertical = true;  
            this.waterfall1.Trails.Font = new Vcl.VclFont(System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255))))), "Arial", 1, 0, true, 11, 0);
            this.waterfall1.Trails.Color = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            waterfall1.Highlighting.MouseHitPoint.PointLabel.Text = "频率：%YMHz，时间：%X，幅度：%Z";

            // x坐标轴设置
            this.waterfall1.XAxis.AxisLabel.Visible = true;
            this.waterfall1.XAxis.AxisLabel.Text = "时间";
            this.waterfall1.XAxis.AxisLabel.Font.Size = 9;
            this.waterfall1.XAxis.Max.Value = 1024D;
            this.waterfall1.XAxis.Max.Tick.Value = 1024D;
            this.waterfall1.XAxis.Max.Tick.AutoScale = false;
            this.waterfall1.XAxis.Max.Range.Low.Value = -1000D;
            this.waterfall1.XAxis.Max.Range.Low.Enabled = false;
            this.waterfall1.XAxis.Max.Range.High.Value = 1000D;
            this.waterfall1.XAxis.Max.Range.High.Enabled = false;
            this.waterfall1.XAxis.Max.DataValue = 1024D;
            this.waterfall1.XAxis.Min.DataValue = 0D;
            this.waterfall1.XAxis.Min.Range.High.Enabled = false;
            this.waterfall1.XAxis.Min.Range.High.Value = 1000D;
            this.waterfall1.XAxis.Min.Range.Low.Enabled = false;
            this.waterfall1.XAxis.Min.Range.Low.Value = -1000D;
            this.waterfall1.XAxis.Min.Tick.Value = 0D;
            this.waterfall1.XAxis.Min.Value = 0D;
            this.waterfall1.XAxis.Min.Tick.AutoScale = false;
            this.waterfall1.XAxis.Samples = 140;   //纵坐标线数
            this.waterfall1.XAxis.TrackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.waterfall1.XAxis.ViewSize.Size = ((uint)(100u));
            //this.waterfall1.XAxis.Reversed = true;
            //this.waterfall1.XAxis.Zooming.MouseWheelEnabled = true;


            // Y坐标轴设置
            this.waterfall1.YAxis.AxisLabel.Text = "频率";
            //this.waterfall1.YAxis.Max.AutoScale = false;
            this.waterfall1.YAxis.Max.Range.High.Enabled = false;
            this.waterfall1.YAxis.Max.Range.Low.Enabled = false;
            //this.waterfall1.YAxis.Min.AutoScale = false;
            this.waterfall1.YAxis.Min.Range.High.Enabled = false;
            this.waterfall1.YAxis.Min.Range.Low.Enabled = false;
            waterfall1.YAxis.MajorTicks.Mode = MajorTicksMode.Auto;
            waterfall1.YAxis.MajorTicks.StartFrom.Mode = MajorTicksMode.Override;
            waterfall1.YAxis.Min.Tick.AutoScale = false;
            waterfall1.YAxis.Max.Tick.AutoScale = false;
            waterfall1.YAxis.MajorTicks.Step = Math.Round((waterfall1.YAxis.Max.Tick.Value - waterfall1.YAxis.Min.Tick.Value) / 10, 3);


            // z坐标轴设置
            this.waterfall1.Levels.ViewSize.Size = ((uint)(100u));
            this.waterfall1.Levels.TrackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.waterfall1.Levels.LevelLabel.Text = "幅度（dBm）";
            this.waterfall1.Levels.LevelLabel.Font = new Vcl.VclFont(System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64))))), "Arial", 1, 0, true, 13, 1);
            this.waterfall1.Levels.Background.Color = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.waterfall1.Levels.Axis.Color = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.waterfall1.Levels.Axis.Font = new Vcl.VclFont(System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64))))), "Arial", 1, 0, true, 11, 0);
            this.waterfall1.Levels.Axis.Max = 0D;
            this.waterfall1.Levels.Axis.Min = -160D;
            this.waterfall1.Levels.Axis.Autoscale = false;

            // 悬停读数设计
            //this.waterfall1.Highlighting.CursorLinks.Color = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            //this.waterfall1.Highlighting.Cursors.Color = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            //this.waterfall1.Highlighting.Labels.Color = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            //this.waterfall1.Highlighting.Labels.HiglightFromMouse = false;
            //this.waterfall1.Highlighting.Labels.HiglightLegendButton = false;
            //this.waterfall1.Highlighting.Markers.Color = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            //this.waterfall1.Highlighting.MouseHitPoint.Color = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            //this.waterfall1.Highlighting.MouseHitPoint.PointLabel.Font = new Vcl.VclFont(System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255))))), "Arial", 1, 0, true, 11, 0);
            //this.waterfall1.Highlighting.MouseHitPoint.PointLabel.Text = "频率：%YMHz，频次：%Z，幅度：%XdBm";
            //this.waterfall1.Highlighting.MouseHitPoint.Enabled = false;

            this.waterfall1.Zooming.HoldOnZoom = false;
            this.waterfall1.Zooming.Mode = Mitov.PlotLab.DisplayZoomMode.Both;
            this.waterfall1.Zooming.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            //scope1.XAxis.Max.DataValue = 1024;
            waterfall1.YAxis.Max.Tick.Value = 1024;
            //scope1.XAxis.Min.DataValue = 0;
            waterfall1.YAxis.Min.Tick.Value = 0;
            //waterfall1.XAxis.Max.Tick.Value = 1000;
            //waterfall1.XAxis.Min.Tick.Value = 100;
            #endregion

        }


        // 更新scope委托
        private delegate void PushDataHandler_(double[] data);

        // 更新瀑布图数据
        private void PushData(double[] data)
        {
            if (this.waterfall1.InvokeRequired)
            {
                PushDataHandler_ handler = new PushDataHandler_(PushData);

                this.waterfall1.Invoke(handler, new object[] { data });
            }
            else
            {
                try
                {
                    this.waterfall1.Data.AddData(data);
                }
                catch (System.Exception ex)
                {

                }
            }
        }

        public void clearData()
        {
            this.waterfall1.Data.Clear();
        }
    }
}
