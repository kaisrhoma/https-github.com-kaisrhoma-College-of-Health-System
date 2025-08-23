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
        SqlConnection con = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=Cohs_DB;Integrated Security=True");
        public exams_home()
        {
     
            InitializeComponent();
            // في الـ Constructor أو عند تحميل الـ UserControl/Form
            chart3.Series.Clear();      // مسح جميع السلاسل
            chart3.Titles.Clear();      // مسح العناوين
            chart3.ChartAreas[0].AxisX.Title = "";  // إزالة عنوان المحور X
            chart3.ChartAreas[0].AxisY.Title = "";  // إزالة عنوان المحور Y

            LoadDashboardStatistics();
            UpdateChartPassedFailed();
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
    (SELECT COUNT(*) FROM Students WHERE current_year = 1 AND exam_round = 'دور أول') AS NewStudents;

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
        private void UpdateChartPassedFailed()
        {
            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                // جلب آخر سنة أكاديمية موجودة
                SqlCommand cmdYear = new SqlCommand(@"
            SELECT MAX(academic_year_start) 
            FROM Registrations
        ", con);

                object result = cmdYear.ExecuteScalar();

                if (result == DBNull.Value)
                {
                    MessageBox.Show("لا توجد بيانات للسنة الأكاديمية.");
                    return;
                }

                int lastYear = Convert.ToInt32(result);

                // جلب عدد الناجحين والراسبين بناءً على total_grade >= 60
                SqlCommand cmd = new SqlCommand(@"
      SELECT 
        SUM(CASE WHEN PassedAll = 1 THEN 1 ELSE 0 END) AS Passed,
        SUM(CASE WHEN PassedAll = 0 THEN 1 ELSE 0 END) AS Failed
    FROM (
        SELECT g.student_id,
               MIN(CASE WHEN g.total_grade >= 60 THEN 1 ELSE 0 END) AS PassedAll
        FROM Grades g
        INNER JOIN Registrations r ON g.student_id = r.student_id
        WHERE r.academic_year_start = @lastYear
        GROUP BY g.student_id
    ) AS StudentResults
        ", con);

                cmd.Parameters.AddWithValue("@lastYear", lastYear);

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                int passed = 0;
                int failed = 0;

                if (dt.Rows.Count > 0 && dt.Rows[0]["Passed"] != DBNull.Value)
                    passed = Convert.ToInt32(dt.Rows[0]["Passed"]);

                if (dt.Rows.Count > 0 && dt.Rows[0]["Failed"] != DBNull.Value)
                    failed = Convert.ToInt32(dt.Rows[0]["Failed"]);

                // مسح البيانات السابقة
                chart3.Series.Clear();
                chart3.Titles.Clear();

                // التأكد من وجود ChartArea
                if (chart3.ChartAreas.Count == 0)
                    chart3.ChartAreas.Add(new System.Windows.Forms.DataVisualization.Charting.ChartArea("Default"));

                // إعداد العنوان والمحاور
                chart3.Titles.Add($"نسبة النجاح والرسوب للسنة الأكاديمية {lastYear}");
                chart3.ChartAreas[0].AxisX.Title = "الحالة";
                chart3.ChartAreas[0].AxisY.Title = "عدد الطلبة";

                // إنشاء السلسلة
                var series = new System.Windows.Forms.DataVisualization.Charting.Series
                {
                    Name = "Students",
                    IsVisibleInLegend = true,
                    ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column,
                    IsValueShownAsLabel = true
                };

                // ألوان مختلفة لكل عمود
                series.Points.AddXY("ناجح", passed);
                series.Points[0].Color = Color.Green;

                series.Points.AddXY("راسب", failed);
                series.Points[1].Color = Color.Red;

                chart3.Series.Add(series);
                chart3.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في تحديث الرسم: " + ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }


        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
