namespace ServoSequencer
{
    partial class ServoView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cb = new System.Windows.Forms.CheckBox();
            this.cbInv = new System.Windows.Forms.CheckBox();
            this.posBar = new System.Windows.Forms.TrackBar();
            this.group = new System.Windows.Forms.ComboBox();
            this.pos = new System.Windows.Forms.NumericUpDown();
            this.btMin = new System.Windows.Forms.Button();
            this.btMax = new System.Windows.Forms.Button();
            this.btMid = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.posBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pos)).BeginInit();
            this.SuspendLayout();
            // 
            // cb
            // 
            this.cb.AutoSize = true;
            this.cb.BackColor = System.Drawing.Color.Transparent;
            this.cb.Font = new System.Drawing.Font("Arial Narrow", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cb.Location = new System.Drawing.Point(7, 4);
            this.cb.Name = "cb";
            this.cb.Size = new System.Drawing.Size(62, 20);
            this.cb.TabIndex = 1;
            this.cb.Text = "Servo #";
            this.cb.UseVisualStyleBackColor = false;
            this.cb.CheckedChanged += new System.EventHandler(this.cb_CheckedChanged);
            // 
            // cbInv
            // 
            this.cbInv.AutoSize = true;
            this.cbInv.BackColor = System.Drawing.Color.Transparent;
            this.cbInv.Font = new System.Drawing.Font("Arial Narrow", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbInv.Location = new System.Drawing.Point(86, 4);
            this.cbInv.Name = "cbInv";
            this.cbInv.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cbInv.Size = new System.Drawing.Size(61, 20);
            this.cbInv.TabIndex = 8;
            this.cbInv.Text = "Inverted";
            this.cbInv.UseVisualStyleBackColor = false;
            this.cbInv.CheckedChanged += new System.EventHandler(this.cbInv_CheckedChanged);
            // 
            // posBar
            // 
            this.posBar.AutoSize = false;
            this.posBar.BackColor = System.Drawing.SystemColors.Control;
            this.posBar.LargeChange = 2;
            this.posBar.Location = new System.Drawing.Point(7, 47);
            this.posBar.Name = "posBar";
            this.posBar.Size = new System.Drawing.Size(140, 23);
            this.posBar.TabIndex = 4;
            this.posBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.posBar.ValueChanged += new System.EventHandler(this.posBar_ValueChanged);
            // 
            // group
            // 
            this.group.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.group.Font = new System.Drawing.Font("Arial Narrow", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.group.FormattingEnabled = true;
            this.group.Location = new System.Drawing.Point(75, 24);
            this.group.Name = "group";
            this.group.Size = new System.Drawing.Size(72, 21);
            this.group.TabIndex = 3;
            // 
            // pos
            // 
            this.pos.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pos.Font = new System.Drawing.Font("Arial Narrow", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.pos.Location = new System.Drawing.Point(7, 25);
            this.pos.Name = "pos";
            this.pos.Size = new System.Drawing.Size(63, 20);
            this.pos.TabIndex = 2;
            this.pos.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.pos.ValueChanged += new System.EventHandler(this.pos_ValueChanged);
            // 
            // btMin
            // 
            this.btMin.BackColor = System.Drawing.Color.Gray;
            this.btMin.Font = new System.Drawing.Font("Arial Narrow", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btMin.ForeColor = System.Drawing.Color.White;
            this.btMin.Location = new System.Drawing.Point(7, 71);
            this.btMin.Margin = new System.Windows.Forms.Padding(1);
            this.btMin.Name = "btMin";
            this.btMin.Size = new System.Drawing.Size(37, 23);
            this.btMin.TabIndex = 5;
            this.btMin.Text = "min";
            this.btMin.UseVisualStyleBackColor = false;
            this.btMin.Click += new System.EventHandler(this.btMin_Click);
            // 
            // btMax
            // 
            this.btMax.BackColor = System.Drawing.Color.Gray;
            this.btMax.Font = new System.Drawing.Font("Arial Narrow", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btMax.ForeColor = System.Drawing.Color.White;
            this.btMax.Location = new System.Drawing.Point(110, 71);
            this.btMax.Margin = new System.Windows.Forms.Padding(1);
            this.btMax.Name = "btMax";
            this.btMax.Size = new System.Drawing.Size(37, 23);
            this.btMax.TabIndex = 7;
            this.btMax.Text = "max";
            this.btMax.UseVisualStyleBackColor = false;
            this.btMax.Click += new System.EventHandler(this.btMax_Click);
            // 
            // btMid
            // 
            this.btMid.BackColor = System.Drawing.Color.Gray;
            this.btMid.FlatAppearance.BorderSize = 0;
            this.btMid.Font = new System.Drawing.Font("Arial Narrow", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btMid.ForeColor = System.Drawing.Color.White;
            this.btMid.Location = new System.Drawing.Point(49, 71);
            this.btMid.Margin = new System.Windows.Forms.Padding(1);
            this.btMid.Name = "btMid";
            this.btMid.Size = new System.Drawing.Size(59, 23);
            this.btMid.TabIndex = 6;
            this.btMid.Text = "center";
            this.btMid.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btMid.UseVisualStyleBackColor = false;
            this.btMid.Click += new System.EventHandler(this.btMid_Click);
            // 
            // ServoView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.cb);
            this.Controls.Add(this.cbInv);
            this.Controls.Add(this.posBar);
            this.Controls.Add(this.btMid);
            this.Controls.Add(this.group);
            this.Controls.Add(this.btMax);
            this.Controls.Add(this.pos);
            this.Controls.Add(this.btMin);
            this.DoubleBuffered = true;
            this.Name = "ServoView";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.Size = new System.Drawing.Size(154, 100);
            ((System.ComponentModel.ISupportInitialize)(this.posBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pos)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cb;
        private System.Windows.Forms.CheckBox cbInv;
        private System.Windows.Forms.TrackBar posBar;
        private System.Windows.Forms.ComboBox group;
        private System.Windows.Forms.NumericUpDown pos;
        private System.Windows.Forms.Button btMin;
        private System.Windows.Forms.Button btMax;
        private System.Windows.Forms.Button btMid;

    }
}
