using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ServoSequencer
{
    public partial class ServoView : UserControl
    {
        private static List<ServoView> _servoViews = new List<ServoView>(); 

        public delegate void ValueChangedEventHandler(object sender, EventArgs e);

        public event ValueChangedEventHandler ValueChanged;
        public event ValueChangedEventHandler GroupValueChanged;

        public bool IsEnabled
        {
            get { return cb.Checked; }
            set { cb.Checked = true; }
        }

        public bool IsInverted
        {
            get { return cbInv.Checked; }
            set { cbInv.Checked = value; }
        }

        public int Min
        {
            get { return posBar.Minimum; }
            set { pos.Minimum = posBar.Minimum = value; }
        }

        public int Max
        {
            get { return posBar.Maximum; }
            set { pos.Maximum = posBar.Maximum = value; }
        }

        public int Value
        {
            get { return posBar.Value; }
            set { posBar.Value = value; }
        }

        private int Mid
        {
            get { return Min + (Max - Min) / 2; }
        }

        public string Group
        {
            get { return group.Text; }
            set { group.Text = value; }
        }

        public Color GroupColor { get; set; }

        public string Name
        {
            get { return cb.Text; }
            set { cb.Text = value; }
        }

        public ServoView()
        {
            GroupColor = Color.LightGray;
            InitializeComponent();
            UpdateState();
            _servoViews.Add(this);
        }

        public void UpdateState()
        {
            pos.Enabled = group.Enabled = cbInv.Enabled = posBar.Enabled = IsEnabled;
            btMin.Enabled = IsEnabled && posBar.Value != Min;
            btMax.Enabled = IsEnabled && posBar.Value != Max;
            btMid.Enabled = IsEnabled && posBar.Value != Mid;
            posBar.BackColor = BackColor = IsEnabled ? GroupColor : SystemColors.Control;

            btMin.ForeColor = btMid.ForeColor = btMax.ForeColor = IsEnabled ? GroupColor : Color.LightGray;
            cbInv.ForeColor = IsInverted ? Color.Black : GroupColor;

            Invalidate();
        }

        private void cb_CheckedChanged(object sender, System.EventArgs e)
        {
            UpdateState();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            e.Graphics.DrawRectangle(new Pen(IsEnabled ? Color.DarkBlue : GroupColor, 1), 0, 0, Width - 1, Height - 1);
        }

        private void posBar_ValueChanged(object sender, System.EventArgs e)
        {
            pos.Value = posBar.Value;
            UpdateState();

            var groupViews = GetServosFromSameGroup();
            if (groupViews.Length > 0)
            {
                foreach (var view in groupViews)
                {
                    view.Value = Value;
                }
                var ghandler = GroupValueChanged;
                if (ghandler != null)
                {
                    ghandler(this, e);
                }
            }
            

            var handler = ValueChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void pos_ValueChanged(object sender, System.EventArgs e)
        {
            posBar.Value = (int) pos.Value;
        }

        private void btMid_Click(object sender, System.EventArgs e)
        {
            posBar.Value = Mid;
        }

        private void btMin_Click(object sender, System.EventArgs e)
        {
            posBar.Value = Min;
        }

        private void btMax_Click(object sender, System.EventArgs e)
        {
            posBar.Value = Max;
        }

        private void cbInv_CheckedChanged(object sender, EventArgs e)
        {
            UpdateState();
        }

        private ServoView[] GetServosFromSameGroup()
        {
            return _servoViews.Where(s => s.IsEnabled && s.Group == Group && s.Name != Name).ToArray();
        }
    }
}
