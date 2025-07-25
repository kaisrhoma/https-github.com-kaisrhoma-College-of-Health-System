﻿using college_of_health_sciences.dashboards.exams_dashboards;
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
            button1_Click(null, null);
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
            this.Close();
            login.Show();
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            panel4.Controls.Clear();
            statements_reports stmt_reports = new statements_reports();
            stmt_reports.Dock = DockStyle.Fill;
            panel4.Controls.Add(stmt_reports);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            panel4.Controls.Clear();
            administrative_operations admin_op = new administrative_operations();
            admin_op.Dock = DockStyle.Fill;
            panel4.Controls.Add(admin_op);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            panel4.Controls.Clear();
            exams_edit_profile  e_edit = new exams_edit_profile();
            e_edit.Dock = DockStyle.Fill;
            panel4.Controls.Add(e_edit);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel4.Controls.Clear();
            exams_home ehome = new exams_home();
            ehome.Dock = DockStyle.Fill;
            panel4.Controls.Add(ehome);
        }
    }
}
