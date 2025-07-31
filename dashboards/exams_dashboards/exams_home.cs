using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace college_of_health_sciences.dashboards.exams_dashboards
{
    public partial class exams_home : UserControl
    {
        public exams_home()
        {
            InitializeComponent();
            LoadDashboardStatistics();
        }
        private void LoadDashboardStatistics()
        {
            string connStr = @"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // نص الاستعلامات
                string query = @"
            SELECT 
                (SELECT COUNT(*) FROM Instructors) AS TotalInstructors,
                (SELECT COUNT(*) FROM Departments) AS TotalDepartments,
                (SELECT COUNT(*) FROM Students) AS TotalStudents,
                (SELECT COUNT(*) FROM Courses) AS TotalCourses,
                (SELECT COUNT(*) FROM Students WHERE status_id = 4) AS GraduatedStudents,
                (SELECT COUNT(*) FROM Students WHERE current_year = 1) AS NewStudents;
        ";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    labelInstructors.Text = reader["TotalInstructors"].ToString();    
                    labelDepartments.Text = reader["TotalDepartments"].ToString();
                    Students.Text = reader["TotalStudents"].ToString();
                    labelCourses1.Text = reader["TotalCourses"].ToString();
                    labelGraduated.Text = reader["GraduatedStudents"].ToString();
                    labelNewStudents.Text = reader["NewStudents"].ToString();
                }

                reader.Close();
            }
        }


        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
