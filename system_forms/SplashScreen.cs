using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace college_of_health_sciences.system_forms
{
    public partial class SplashScreen : Form
    {
        public SplashScreen()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true; // GIF من Resources
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void SplashScreen_Load(object sender, EventArgs e)
        {
            Timer timer = new Timer();
            timer.Interval = 5000; // مدة العرض 2 ثانية
            timer.Tick += (s, ev) =>
            {
                timer.Stop();
                this.Close(); // يغلق الـ Splash بعد 2 ثانية
            };
            timer.Start();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
