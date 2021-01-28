namespace XD_DBDW_Server
{
    partial class UI_AGC
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label88 = new System.Windows.Forms.Label();
            this.richTextBox9 = new System.Windows.Forms.RichTextBox();
            this.button28 = new System.Windows.Forms.Button();
            this.comboBox39 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label88
            // 
            this.label88.AutoSize = true;
            this.label88.Location = new System.Drawing.Point(20, 48);
            this.label88.Name = "label88";
            this.label88.Size = new System.Drawing.Size(71, 12);
            this.label88.TabIndex = 146;
            this.label88.Text = "数字AGC增益";
            // 
            // richTextBox9
            // 
            this.richTextBox9.Location = new System.Drawing.Point(22, 63);
            this.richTextBox9.Name = "richTextBox9";
            this.richTextBox9.Size = new System.Drawing.Size(370, 171);
            this.richTextBox9.TabIndex = 145;
            this.richTextBox9.Text = "";
            // 
            // button28
            // 
            this.button28.Location = new System.Drawing.Point(93, 11);
            this.button28.Name = "button28";
            this.button28.Size = new System.Drawing.Size(111, 23);
            this.button28.TabIndex = 142;
            this.button28.Text = "查询AGC增益值";
            this.button28.UseVisualStyleBackColor = true;
            this.button28.Click += new System.EventHandler(this.button28_Click);
            // 
            // comboBox39
            // 
            this.comboBox39.FormattingEnabled = true;
            this.comboBox39.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5"});
            this.comboBox39.Location = new System.Drawing.Point(22, 13);
            this.comboBox39.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox39.Name = "comboBox39";
            this.comboBox39.Size = new System.Drawing.Size(52, 20);
            this.comboBox39.TabIndex = 141;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(223, 11);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(111, 23);
            this.button1.TabIndex = 149;
            this.button1.Text = "清空";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // UI_AGC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label88);
            this.Controls.Add(this.richTextBox9);
            this.Controls.Add(this.button28);
            this.Controls.Add(this.comboBox39);
            this.Name = "UI_AGC";
            this.Size = new System.Drawing.Size(958, 267);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label88;
        private System.Windows.Forms.RichTextBox richTextBox9;
        private System.Windows.Forms.Button button28;
        private System.Windows.Forms.ComboBox comboBox39;
        private System.Windows.Forms.Button button1;

    }
}
