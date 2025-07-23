using college_of_health_sciences.dashboards.admin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace college_of_health_sciences
{
    public partial class admin_form : Form
    {
        public admin_form()
        {
            InitializeComponent();
            button1_Click(null, null);
        }

        private void button5_Click(object sender, EventArgs e)
        {


        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void chart2_Click(object sender, EventArgs e)
        {

        }

        private void chart3_Click(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel4.Controls.Clear();
            admin_home adhome = new admin_home();
            adhome.Dock = DockStyle.Fill;
            panel4.Controls.Add(adhome);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            panel4.Controls.Clear();
            users_management users_manage = new users_management();
            users_manage.Dock = DockStyle.Fill; 
            panel4.Controls.Add(users_manage);
        }
    }
}