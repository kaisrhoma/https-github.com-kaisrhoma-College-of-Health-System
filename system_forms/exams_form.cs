using college_of_health_sciences.dashboards.exams_dashboards;
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
    public partial class exams_form : Form
    {
        public exams_form()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            panel4.Controls.Clear();
            grads_management gradsm = new grads_management();
            gradsm.Dock = DockStyle.Fill;
            panel4.Controls.Add(gradsm);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            login_form login = new login_form();
            login.Show();
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            panel4.Controls.Clear();
            statements_reports stmt_reports = new statements_reports();
            stmt_reports.Dock = DockStyle.Fill;
            panel4.Controls.Add(stmt_reports);
        }
    }
}
