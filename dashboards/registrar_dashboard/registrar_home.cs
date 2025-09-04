using DocumentFormat.OpenXml.Bibliography;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace college_of_health_sciences.dashboards.registrar_dashboard
{
    public partial class registrar_home : UserControl
    {
        SqlConnection con = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=Cohs_DB;Integrated Security=True");
        public registrar_home()
        {
            InitializeComponent();
            UpdateChartStudentsPerDepartment();
            LoadDashboardStatistics();
            UpdateChartStudentNationality();
            UpdateChartMaleFemale();
        }
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {


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
    (SELECT COUNT(*) FROM Departments) AS [عدد الأقسام],
    (SELECT COUNT(*) FROM Students) AS [عدد الطلاب الكلي],
    (SELECT COUNT(*) FROM Instructors) AS [عدد الدكاترة],
    (SELECT COUNT(*) FROM Students WHERE status_id = 2) AS [الطلاب موقفين القيد],
    (
        SELECT COUNT(DISTINCT s.student_id)  -- هنا نستخدم DISTINCT
        FROM Students s
        INNER JOIN Registrations r ON s.student_id = r.student_id
        WHERE r.academic_year_start = (
            SELECT MAX(academic_year_start) 
            FROM Registrations r2 
            WHERE r2.student_id = s.student_id
        )
        AND r.status = 'مسجل'
    ) AS [الطلاب في العام الجامعي الحالي],
    (SELECT COUNT(*) FROM Students WHERE status_id = 4) AS [عدد الخريجين];



        ";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    labelInstructors.Text = reader["عدد الدكاترة"].ToString();
                    labelDepartments.Text = reader["عدد الأقسام"].ToString();
                    Students.Text = reader["عدد الطلاب الكلي"].ToString();
                    labelCourses1.Text = reader["الطلاب موقفين القيد"].ToString();
                    labelGraduated.Text = reader["عدد الخريجين"].ToString();
                    labelNewStudents1.Text = reader["الطلاب في العام الجامعي الحالي"].ToString();
                }

                reader.Close();
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
        private void UpdateChartStudentNationality()
        {
            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                SqlCommand cmd = new SqlCommand(@"
            SELECT nationality, COUNT(*) AS StudentCount
            FROM Students
            WHERE nationality IS NOT NULL AND nationality <> 'غير محدد'
            GROUP BY nationality
            ORDER BY nationality;
        ", con);

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                chart2.Series.Clear();
                chart2.Titles.Clear();
                chart2.ChartAreas.Clear();

                var chartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea("Default");
                chart2.ChartAreas.Add(chartArea);
                chart2.Dock = DockStyle.Fill;

                chartArea.AxisX.Title = "الجنسية";
                chartArea.AxisY.Title = "عدد الطلاب";
                chartArea.AxisX.Interval = 1;
                chartArea.AxisX.LabelStyle.Angle = -45;
                chartArea.AxisX.LabelStyle.Font = new Font("Tahoma", 8, FontStyle.Bold);
                chartArea.AxisX.MajorGrid.Enabled = false;
                chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
                chartArea.AxisY.Minimum = 0;

                if (dt.Rows.Count == 0)
                {
                    chart2.Titles.Add("⚠️ لا توجد بيانات للطلاب.");
                }
                else
                {
                    double maxValue = dt.AsEnumerable().Max(r => Convert.ToInt32(r["StudentCount"]));
                    chartArea.AxisY.Maximum = Math.Ceiling(maxValue * 1.2);
                    chartArea.AxisY.Interval = Math.Ceiling(maxValue / 10.0);

                    var series = new System.Windows.Forms.DataVisualization.Charting.Series
                    {
                        Name = "الطلاب حسب الجنسية",
                        IsVisibleInLegend = true,
                        ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column,
                        IsValueShownAsLabel = true
                    };

                    Color[] colors = new Color[] {
                Color.CornflowerBlue, Color.Orange, Color.Green, Color.Red, Color.Purple, Color.Tomato,
                Color.MediumSeaGreen, Color.Gold, Color.SkyBlue, Color.Violet
            };

                    int colorIndex = 0;

                    foreach (DataRow row in dt.Rows)
                    {
                        string nationality = row["nationality"].ToString();
                        int count = Convert.ToInt32(row["StudentCount"]);
                        int pointIndex = series.Points.AddXY(nationality, count);

                        series.Points[pointIndex].Color = colors[colorIndex % colors.Length];
                        series.Points[pointIndex].Label = count.ToString();
                        series.Points[pointIndex]["LabelStyle"] = "Top";
                        series.Points[pointIndex].LabelForeColor = Color.Black;

                        colorIndex++;
                    }

                    chart2.Series.Add(series);
                    chart2.Titles.Add("عدد الطلاب حسب الجنسية");
                }

                chart2.Invalidate();
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
        private void UpdateChartMaleFemale()
        {
            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                // 🔹 جلب عدد الطلاب الذكور والإناث من جدول Students
                SqlCommand cmd = new SqlCommand(@"
            SELECT 
                SUM(CASE WHEN gender = 1 THEN 1 ELSE 0 END) AS MaleCount,
                SUM(CASE WHEN gender = 0 THEN 1 ELSE 0 END) AS FemaleCount
            FROM Students
            WHERE gender IS NOT NULL
        ", con);

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                int maleCount = dt.Rows[0]["MaleCount"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["MaleCount"]) : 0;
                int femaleCount = dt.Rows[0]["FemaleCount"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["FemaleCount"]) : 0;

                // 🔹 مسح أي بيانات سابقة
                chart3.Series.Clear();
                chart3.Titles.Clear();

                // 🔹 إنشاء ChartArea إذا لم يوجد
                if (chart3.ChartAreas.Count == 0)
                    chart3.ChartAreas.Add(new System.Windows.Forms.DataVisualization.Charting.ChartArea("Default"));

                var chartArea = chart3.ChartAreas[0];
                chartArea.AxisX.Title = "الجنس";
                chartArea.AxisY.Title = "عدد الطلاب";
                chart3.Dock = DockStyle.Fill;
                chartArea.AxisX.Interval = 1;
                chartArea.AxisY.IsStartedFromZero = true;

                // 🔹 التحقق إذا لا يوجد طلاب
                if (maleCount == 0 && femaleCount == 0)
                {
                    chart3.Titles.Add("⚠️ لا توجد بيانات للطلاب.");
                }
                else
                {
                    // 🔹 إنشاء السلسلة وإضافة البيانات
                    var series = new System.Windows.Forms.DataVisualization.Charting.Series
                    {
                        Name = "الطلاب",
                        IsVisibleInLegend = true,
                        ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column,
                        IsValueShownAsLabel = true
                    };

                    int pointMale = series.Points.AddXY("ذكر", maleCount);
                    series.Points[pointMale].Color = Color.Blue;

                    int pointFemale = series.Points.AddXY("أنثى", femaleCount);
                    series.Points[pointFemale].Color = Color.Pink;

                    chart3.Series.Add(series);
                    chart3.Titles.Add("عدد الطلاب حسب الجنس");
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


        private void chart3_Click(object sender, EventArgs e)
        {

        }

        private void chart2_Click(object sender, EventArgs e)
        {

        }

        private void chart4_Click(object sender, EventArgs e)
        {

        }
    }
}
