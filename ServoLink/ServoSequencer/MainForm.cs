using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServoSequencer
{
    public partial class MainForm : Form
    {
        
        public MainForm()
        {
            InitializeComponent();
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        private void MainForm_Load(object sender, EventArgs e)
        {
            AllocConsole();
            for (int i = 0; i < 18; i++)
            {
                var c = new ServoView();
                fpanel.Controls.Add(c);
                c.ValueChanged += ServoValueChanged;
                c.Name = string.Format("Servo {0}", i);
                c.Group = i % 3 == 0 ? "A" : i % 3 == 1 ? "B" : "C";
                c.GroupColor = i % 3 == 0 ? Color.Gold : i % 3 == 1 ? Color.GreenYellow : Color.LightSkyBlue;
                c.Min = 1000;
                c.Max = 2000;
                c.Value = 1500;
                c.IsEnabled = true;
                c.IsInverted = (i % 6) / 3 > 0;
                c.UpdateState();
            }
        }


        private void ServoValueChanged(object sender, EventArgs e)
        {
            var view = sender as ServoView;
            Console.WriteLine("{0}-{1}: {2}", view.Group, view.Name, view.Value);
        }
    }
}
