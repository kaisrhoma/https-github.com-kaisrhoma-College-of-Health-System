using college_of_health_sciences.dashboards.admin;
using college_of_health_sciences.dashboards.registrar_dashboard;
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
            label2.Text = Session.Username;
            
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

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void admin_form_Load(object sender, EventArgs e)
        {
           
        }

        private void button7_Click(object sender, EventArgs e)
        {
            login_form login = new login_form();
            login.Show();
            this.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            panel4.Controls.Clear();
            registrar_edit_profile redit = new registrar_edit_profile();
            redit.Dock = DockStyle.Fill;
            panel4.Controls.Add(redit);
        }
    }
}