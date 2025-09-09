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
        private readonly string connectionString = @"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;";
        public exams_home()
        {

            InitializeComponent();

            LoadDashboardStatistics();
            UpdateChartPassedFailed();
            UpdateChartStudentStatus();
            UpdateChartStudentsPerDepartment();
        }
        private void LoadDashboardStatistics()
        {
       
            using (SqlConnection conn = new SqlConnection(connectionString))
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

                // 🔹 جلب آخر سنة أكاديمية موجودة
                SqlCommand cmdYear = new SqlCommand(@"
            SELECT MAX(academic_year_start) 
            FROM Registrations
        ", con);

                object result = cmdYear.ExecuteScalar();

                if (result == DBNull.Value)
                {
                    MessageBox.Show("⚠️ لا توجد بيانات للسنة الأكاديمية.");
                    return;
                }

                int lastYear = Convert.ToInt32(result);

                // 🔹 حساب عدد الطلاب الناجحين والراسبين
                SqlCommand cmd = new SqlCommand(@"
            SELECT 
                SUM(CASE WHEN PassedAll = 1 THEN 1 ELSE 0 END) AS Passed,
                SUM(CASE WHEN PassedAll = 0 THEN 1 ELSE 0 END) AS Failed
            FROM (
                SELECT g.student_id,
                       MIN(CASE WHEN g.total_grade >= 60 THEN 1 ELSE 0 END) AS PassedAll
                FROM Grades g
                INNER JOIN Registrations r ON g.student_id = r.student_id
                   AND g.course_id = r.course_id
                WHERE r.academic_year_start = @lastYear
                  AND g.total_grade IS NOT NULL
                GROUP BY g.student_id
                HAVING COUNT(*) = SUM(CASE WHEN g.total_grade IS NOT NULL THEN 1 ELSE 0 END)
            ) AS StudentResults;
        ", con);

                cmd.Parameters.AddWithValue("@lastYear", lastYear);

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                int passed = dt.Rows[0]["Passed"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["Passed"]) : 0;
                int failed = dt.Rows[0]["Failed"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["Failed"]) : 0;

                // 🔹 مسح أي بيانات سابقة
                chart3.Series.Clear();
                chart3.Titles.Clear();
                chart3.ChartAreas.Clear();

                // 🔹 إنشاء ChartArea دائمًا حتى لو لا يوجد طلاب
                var chartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea("Default");
                chart3.ChartAreas.Add(chartArea);
                chart3.Dock = DockStyle.Fill;

                chartArea.AxisX.Title = "الحالة";
                chartArea.AxisY.Title = "عدد الطلبة";
                chartArea.AxisX.IntervalAutoMode = System.Windows.Forms.DataVisualization.Charting.IntervalAutoMode.VariableCount;
                chartArea.AxisY.IsStartedFromZero = true;
                chartArea.RecalculateAxesScale();

                // 🔹 التحقق إذا لا يوجد طلاب
                if (passed == 0 && failed == 0)
                {
                    chart3.Titles.Add($"⚠️ لا توجد بيانات للطلاب للسنة الأكاديمية {lastYear}");
                }
                else
                {
                    // 🔹 إنشاء السلسلة وإضافة بيانات
                    var series = new System.Windows.Forms.DataVisualization.Charting.Series
                    {
                        Name = "الطلاب",
                        IsVisibleInLegend = true,
                        ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column,
                        IsValueShownAsLabel = true
                    };

                    var pointPassed = series.Points.AddXY("ناجح", passed);
                    series.Points[0].Color = Color.Green;

                    var pointFailed = series.Points.AddXY("راسب", failed);
                    series.Points[1].Color = Color.Red;

                    chart3.Series.Add(series);
                    chart3.Titles.Add($"نسبة النجاح والرسوب للسنة الأكاديمية {lastYear}");
                }

                chart3.Invalidate(); // تحديث الرسم
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


        private void UpdateChartStudentStatus()
        {
            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                // 🔹 جلب عدد الطلاب حسب الحالة، حتى لو لم يوجد طلاب لبعض الحالات
                SqlCommand cmd = new SqlCommand(@"
            SELECT st.status_id, st.description, COUNT(s.student_id) AS StudentCount
            FROM Status st
            LEFT JOIN Students s ON s.status_id = st.status_id
            GROUP BY st.status_id, st.description
            ORDER BY st.status_id;
        ", con);

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                // 🔹 مسح أي بيانات سابقة
                chart2.Series.Clear();
                chart2.Titles.Clear();
                chart2.ChartAreas.Clear();

                // 🔹 إنشاء ChartArea جديد
                var chartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea("Default");
                chart2.ChartAreas.Add(chartArea);
                chart2.Dock = DockStyle.Fill;

                chartArea.AxisX.Title = "الحالة";
                chartArea.AxisY.Title = "عدد الطلاب";
                chartArea.AxisX.Interval = 1;
                chartArea.AxisX.LabelStyle.Angle = -90;
                chartArea.AxisX.LabelStyle.Font = new Font("Tahoma", 8, FontStyle.Bold);
                chartArea.AxisX.MajorGrid.Enabled = false;
                chartArea.AxisY.MajorGrid.Enabled = true;
                chartArea.AxisY.Minimum = 0;

                if (dt.Rows.Count == 0)
                {
                    chart2.Titles.Add("⚠️ لا توجد بيانات للطلاب.");
                }
                else
                {
                    // 🔹 إيجاد أعلى قيمة
                    double maxValue = dt.AsEnumerable().Max(r => Convert.ToInt32(r["StudentCount"]));
                    double yMax = Math.Ceiling(maxValue * 1.2); // زيادة 20% فوق الأعلى

                    chartArea.AxisY.Maximum = yMax;
                    chartArea.AxisY.Interval = 20; // تقسيم واضح
                    chartArea.AxisY.IntervalAutoMode = System.Windows.Forms.DataVisualization.Charting.IntervalAutoMode.FixedCount;

                    // 🔹 إنشاء السلسلة
                    var series = new System.Windows.Forms.DataVisualization.Charting.Series
                    {
                        Name = "الحالة",
                        IsVisibleInLegend = true,
                        ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column,
                        IsValueShownAsLabel = true
                    };

                    // 🔹 إضافة البيانات لكل حالة مع اللون والرقم فوق العمود
                    foreach (DataRow row in dt.Rows)
                    {
                        string statusDesc = row["description"].ToString();
                        int count = Convert.ToInt32(row["StudentCount"]);

                        int pointIndex = series.Points.AddXY(statusDesc, count);

                        // ألوان مخصصة لكل حالة
                        switch (Convert.ToInt32(row["status_id"]))
                        {
                            case 1: series.Points[pointIndex].Color = Color.Blue; break;      // مسجل
                            case 2: series.Points[pointIndex].Color = Color.Orange; break;    // مؤجل
                            case 3: series.Points[pointIndex].Color = Color.Red; break;       // مستبعد
                            case 4: series.Points[pointIndex].Color = Color.Green; break;     // خريج
                            default: series.Points[pointIndex].Color = Color.Gray; break;
                        }

                        // عرض الرقم فوق العمود
                        series.Points[pointIndex].Label = count.ToString();
                        series.Points[pointIndex]["LabelStyle"] = "Top";
                        series.Points[pointIndex].LabelForeColor = Color.Black;
                    }

                    chart2.Series.Add(series);
                    chart2.Titles.Add("عدد الطلاب حسب الحالة");
                }

                chart2.Invalidate(); // تحديث الرسم
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


        private void UpdateChartStudentsPerDepartment()
        {
            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                // 🔹 جلب عدد الطلاب في كل قسم
                SqlCommand cmd = new SqlCommand(@"
            SELECT d.dep_name, COUNT(s.student_id) AS StudentCount
            FROM Departments d
            LEFT JOIN Students s ON s.department_id = d.department_id
            GROUP BY d.dep_name
            ORDER BY d.dep_name;
        ", con);

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                // 🔹 مسح أي بيانات سابقة
                chart4.Series.Clear();
                chart4.Titles.Clear();
                chart4.ChartAreas.Clear();

                // 🔹 إنشاء ChartArea جديد
                var chartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea("Default");
                chart4.ChartAreas.Add(chartArea);
                chart4.Dock = DockStyle.Fill;

                // 🔹 إعدادات محاور X و Y
                chartArea.AxisX.Title = "القسم";
                chartArea.AxisY.Title = "عدد الطلاب";
                chartArea.AxisX.Interval = 1;
                chartArea.AxisX.LabelStyle.Angle = -30;
                chartArea.AxisX.LabelStyle.Font = new Font("Tahoma", 8, FontStyle.Bold);
                chartArea.AxisX.MajorGrid.Enabled = false;
                chartArea.AxisY.MajorGrid.Enabled = true;
                chartArea.AxisY.Minimum = 0;

                if (dt.Rows.Count == 0)
                {
                    chart4.Titles.Add("⚠️ لا توجد بيانات للطلاب.");
                }
                else
                {
                    // 🔹 إيجاد أعلى قيمة في الأعمدة
                    double maxValue = dt.AsEnumerable().Max(r => Convert.ToInt32(r["StudentCount"]));

                    // 🔹 زيادة 20% فوق أعلى قيمة لإعطاء مساحة للأرقام
                    double yMax = Math.Ceiling(maxValue * 1.2);

                    chartArea.AxisY.Maximum = yMax;

                    // 🔹 ضبط Interval ثابت لتقسيم المحور Y (مثلاً كل 20)
                    chartArea.AxisY.Interval = 20;
                    chartArea.AxisY.IntervalAutoMode = System.Windows.Forms.DataVisualization.Charting.IntervalAutoMode.FixedCount;

                    // 🔹 إنشاء السلسلة
                    var series = new System.Windows.Forms.DataVisualization.Charting.Series
                    {
                        Name = "الاقسام",
                        IsVisibleInLegend = true,
                        ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column,
                        IsValueShownAsLabel = true
                    };

                    // 🔹 مصفوفة ألوان متنوعة لكل قسم
                    Color[] colors = new Color[]
                    {
                Color.CornflowerBlue, Color.Orange, Color.Green, Color.Red, Color.Purple,
                Color.DarkCyan, Color.Gold, Color.Magenta, Color.SaddleBrown, Color.Teal
                    };
                    int colorIndex = 0;

                    // 🔹 إضافة البيانات لكل قسم مع اللون والرقم فوق العمود
                    foreach (DataRow row in dt.Rows)
                    {
                        string depName = row["dep_name"].ToString();
                        int count = Convert.ToInt32(row["StudentCount"]);

                        int pointIndex = series.Points.AddXY(depName, count);

                        // تعيين لون مختلف لكل قسم
                        series.Points[pointIndex].Color = colors[colorIndex % colors.Length];
                        colorIndex++;

                        // عرض الرقم فوق العمود
                        series.Points[pointIndex].Label = count.ToString();
                        series.Points[pointIndex]["LabelStyle"] = "Top";
                        series.Points[pointIndex].LabelForeColor = Color.Black;
                    }

                    chart4.Series.Add(series);
                    chart4.Titles.Add("عدد الطلاب لكل قسم");
                }

                chart4.Invalidate();
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

        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void chart3_Click(object sender, EventArgs e)
        {

        }
    }
}
