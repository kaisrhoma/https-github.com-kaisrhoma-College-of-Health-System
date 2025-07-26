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

namespace college_of_health_sciences.system_forms
{
    public partial class registerar_form : Form
    {
        public registerar_form()
        {
            InitializeComponent();
            button1_Click(null,null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel4.Controls.Clear();
            registrar_home rghome = new registrar_home();
            rghome.Dock = DockStyle.Fill;
            panel4.Controls.Add(rghome);
        }

        private void button2_Click(object sender, EventArgs e)
        { 
            panel4.Controls.Clear();
            students_management smanagement = new students_management();
            smanagement.Dock = DockStyle.Fill;
            panel4.Controls.Add(smanagement);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            panel4.Controls.Clear();
            certificates_reports c_reports = new certificates_reports();
            c_reports.Dock = DockStyle.Fill;
            panel4.Controls.Add(c_reports);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            panel4.Controls.Clear();
            transfer_deportation trdp = new transfer_deportation();
            trdp.Dock = DockStyle.Fill;
            panel4.Controls.Add(trdp);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            panel4.Controls.Clear();
            registrar_edit_profile redit = new registrar_edit_profile();
            redit.Dock = DockStyle.Fill;
            panel4.Controls.Add(redit);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.Close();
            (new login_form()).ShowDialog();
        }
    }
}
