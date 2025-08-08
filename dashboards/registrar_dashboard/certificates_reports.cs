using college_of_health_sciences;
using college_of_health_sciences.moduls;
using college_of_health_sciences.system_forms;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace college_of_health_sciences.dashboards.registrar_dashboard
{
    public partial class certificates_reports : UserControl
    {
        public certificates_reports()
        {
            InitializeComponent();
        }

        PrintDocument printDocument = new PrintDocument();
        DataTable printTable;
        DataTable supjectTable;
        string studentName = "";
        int stuid;


        public void datagridviewstyle(DataGridView datagrid)
        {
            datagrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            datagrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            datagrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }


        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            System.Drawing.Font headerFont = new System.Drawing.Font("Arial", 14, FontStyle.Bold);
            System.Drawing.Font bodyFont = new System.Drawing.Font("Arial", 10);
            int startX = 50;
            int startY = 50;
            int offsetY = 0;

            // عنوان الطالب
            e.Graphics.DrawString("كشف درجات الطالب", headerFont, Brushes.Black, startX, startY + offsetY);
            offsetY += 40;
            e.Graphics.DrawString("اسم الطالب: " + studentName, bodyFont, Brushes.Black, startX, startY + offsetY);
            offsetY += 30;

            // رؤوس الأعمدة
            e.Graphics.DrawString("المادة", bodyFont, Brushes.Black, startX, startY + offsetY);
            e.Graphics.DrawString("الوحدات", bodyFont, Brushes.Black, startX + 200, startY + offsetY);
            e.Graphics.DrawString("الدرجة", bodyFont, Brushes.Black, startX + 300, startY + offsetY);
            e.Graphics.DrawString("النتيجة", bodyFont, Brushes.Black, startX + 400, startY + offsetY);
            offsetY += 25;

            // سطر تحت العنوان
            e.Graphics.DrawLine(Pens.Black, startX, startY + offsetY, startX + 500, startY + offsetY);
            offsetY += 10;

            // طباعة الصفوف
            foreach (DataRow row in printTable.Rows)
            {
                string course = row["اسم_الماده"].ToString();
                string units = row["الوحدات"].ToString();
                string grade = row["الدرجة"].ToString();
                string status = row["النتيجة"].ToString();

                e.Graphics.DrawString(course, bodyFont, Brushes.Black, startX, startY + offsetY);
                e.Graphics.DrawString(units, bodyFont, Brushes.Black, startX + 200, startY + offsetY);
                e.Graphics.DrawString(grade, bodyFont, Brushes.Black, startX + 300, startY + offsetY);
                e.Graphics.DrawString(status, bodyFont, Brushes.Black, startX + 400, startY + offsetY);

                offsetY += 25;

                // في حال كانت الصفحة ممتلئة
                if (startY + offsetY > e.MarginBounds.Bottom)
                {
                    e.HasMorePages = true;
                    return;
                }
            }

            e.HasMorePages = false;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(txtSearch.Text))
            {
                MessageBox.Show("يرجى إدخال الرقم الجامعي");
                return;  
            }
            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using(SqlConnection con = db.OpenConnection() )
                {
                    
                    string q = @"
                               SELECT 
                                   s.full_name AS الإسم,
                                   s.university_number AS الرقم_الجامعي,
                                   s.college AS الكلية,
                                   s.current_year AS السنة,
                                   s.nationality AS الجنسية,
                                   d.dep_name AS القسم,
                                   c.course_id AS رقم_الماده,
                                   c.course_name AS اسم_الماده,
                                   c.units AS الوحدات,
                                   g.total_grade AS الدرجة,
                                   g.success_status AS النتيجة
                               FROM Students s
                               JOIN Departments d ON s.department_id = d.department_id
                               JOIN Grades g ON s.student_id = g.student_id
                               JOIN Courses c ON g.course_id = c.course_id
                               WHERE s.university_number = @university_number";
                    using (SqlCommand cmd = new SqlCommand(q,con))
                    {
                        cmd.Parameters.AddWithValue("@university_number",txtSearch.Text.Trim());
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        printTable = dt;

                        if (dt.Rows.Count > 0)
                        {
                            studentName = dt.Rows[0]["الإسم"].ToString();
                        }


                        dataGridView2.DataSource = dt;

                        datagridviewstyle(dataGridView2);
                        dataGridView2.Columns["الإسم"].Visible = false;
                        dataGridView2.Columns["الرقم_الجامعي"].Visible = false;
                        dataGridView2.Columns["الكلية"].Visible = false;
                        dataGridView2.Columns["السنة"].Visible = false;
                        dataGridView2.Columns["القسم"].Visible = false;
                        dataGridView2.Columns["الجنسية"].Visible = false;

                        dataGridView2.Columns["اسم_الماده"].ReadOnly = true;
                        dataGridView2.Columns["رقم_الماده"].ReadOnly = true;
                        dataGridView2.Columns["الوحدات"].ReadOnly = true;
                        dataGridView2.Columns["الدرجة"].ReadOnly = true;
                        dataGridView2.Columns["النتيجة"].ReadOnly = true;

                        
                        if (dataGridView2.Rows.Count == 0 || dataGridView2.Rows[0].IsNewRow)
                        {
                            MessageBox.Show("لايوجد طالب بهذا الرقم او ان الطالب ليس لديه مواد مكتمله بعد");
                        }
                    }
                }
            } 
            catch (Exception ex)
            {
                MessageBox.Show("There is Error : " + ex.Message);
            }
        }

        private void certificates_reports_Load(object sender, EventArgs e)
        {
            textBox2.Focus();
            var study_year = new Dictionary<int, string>()
            {
                {1, "سنة أولى"},
                {2, "سنة ثانية"},
                {3, "سنة ثالثة"},
                {4, "سنة رابعة"}
            };

            comboBox1.DataSource = new BindingSource(study_year, null);
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";

            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using(SqlConnection con = db.OpenConnection())
                {
                    string q = "select * from Departments";
                    SqlDataAdapter da = new SqlDataAdapter(q,con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    
                    comboBox2.DataSource = new BindingSource(dt, null);
                    comboBox2.DisplayMember = "dep_name";
                    comboBox2.ValueMember = "department_id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There is an Error : " + ex.Message);
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(tabControl1.SelectedTab == tabPage1)
            {
                textBox2.Focus();
            } else if (tabControl1.SelectedTab == tabPage2)
            {
                txtSearch.Focus();
            }
            else
            {
                textBox1.Focus();
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                button2_Click(null, null);
                e.SuppressKeyPress = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (printTable == null || printTable.Rows.Count == 0)
            {
                MessageBox.Show("لا يوجد بيانات للطباعة.");
                return;
            }

            PrintPreviewDialog preview = new PrintPreviewDialog();
            printDocument.PrintPage += PrintDocument_PrintPage;
            preview.Document = printDocument;
            preview.ShowDialog();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("يرجى إدخال الرقم الجامعي اولا");
                return;
            }

            try
            {       
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using(SqlConnection con = db.OpenConnection())
                {

                    int month;   // تم تعريف المتغير هنا

                    using (SqlCommand checkCmd = new SqlCommand("SELECT month_number FROM Months WHERE month_id = 1 ", con))
                    {
                        month = Convert.ToInt32(checkCmd.ExecuteScalar());
                    }

                    int ac_year = DateTime.Now.Month >= month ? DateTime.Now.Year : DateTime.Now.Year - 1;

                    string q = @"
                               SELECT
                                   s.full_name AS الإسم,
                                   s.university_number AS الرقم_الجامعي,
                                   s.college AS الكلية,
                                   s.current_year AS السنة,
                                   d.dep_name AS القسم,
                                   
                                   c.course_id AS رقم_المادة,
                                   c.course_name AS اسم_المادة,
                                   c.units AS الوحدات,

                                   CASE cc.lecture_date
                                        WHEN 1 THEN N'الأحد'
                                        WHEN 2 THEN N'الإثنين'
                                        WHEN 3 THEN N'الثلاثاء'
                                        WHEN 4 THEN N'الأربعاء'
                                        WHEN 5 THEN N'الخميس'
                                        WHEN 6 THEN N'الجمعة'
                                        WHEN 7 THEN N'السبت'
                                   END AS يوم,

                                   cc.start_time AS من_الساعة,
                                   cc.end_time AS الى_الساعة, 
                                   cc.group_number AS المجموعة,
                                   cl.room_name AS القاعة,
                               
                                   i.full_name AS الدكتور
                               
                               FROM Students s
                               JOIN Departments d ON s.department_id = d.department_id
                               JOIN Registrations r ON s.student_id = r.student_id
                               JOIN Courses c ON r.course_id = c.course_id
                               JOIN Course_Classroom cc ON r.course_classroom_id = cc.id
                               JOIN Classrooms cl ON cc.classroom_id = cl.classroom_id
                               JOIN Course_Instructor ci ON c.course_id = ci.course_id
                               JOIN Instructors i ON ci.instructor_id = i.instructor_id
                               
                               WHERE s.university_number = @university_number 
                               AND s.status_id = '1'
                               AND r.academic_year_start = @academic_year_start
                               ";


                    using (SqlCommand cmd = new SqlCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@university_number", textBox1.Text.Trim());
                        cmd.Parameters.AddWithValue("@academic_year_start", ac_year);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        printTable = dt;

                        if (dt.Rows.Count > 0)
                        {
                            studentName = dt.Rows[0]["الإسم"].ToString();
                        }

                        dataGridView1.DataSource = dt;

                        datagridviewstyle(dataGridView1);
                        dataGridView1.Columns["الإسم"].Visible = false;
                        dataGridView1.Columns["الرقم_الجامعي"].Visible = false;
                        dataGridView1.Columns["الكلية"].Visible = false;
                        dataGridView1.Columns["السنة"].Visible = false;
                        dataGridView1.Columns["القسم"].Visible = false;


                        dataGridView1.Columns["اسم_المادة"].ReadOnly = true;
                        dataGridView1.Columns["رقم_المادة"].ReadOnly = true;
                        dataGridView1.Columns["الوحدات"].ReadOnly = true;
                        dataGridView1.Columns["المجموعة"].ReadOnly = true;
                        dataGridView1.Columns["القاعة"].ReadOnly = true;
                        dataGridView1.Columns["الدكتور"].ReadOnly = true;


                        if (dataGridView1.Rows.Count == 0 || dataGridView1.Rows[0].IsNewRow)
                        {
                            MessageBox.Show("لايوجد طالب بهذا الرقم او ان الطالب لم يقم بتنزيل مواد هذا العام بعد او ان الطالب قيده متوقف");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There is an error in :" + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (printTable == null || printTable.Rows.Count == 0)
            {
                MessageBox.Show("لا يوجد بيانات للطباعة.");
                return;
            }
            PrintPreviewDialog preview = new PrintPreviewDialog();
            printDocument1.PrintPage += printDocument1_PrintPage;
            preview.Document = printDocument1;
            preview.ShowDialog();
        }

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            string stno = printTable.Rows[0]["الرقم_الجامعي"].ToString();
            string styear = printTable.Rows[0]["السنة"].ToString();
            string stdep = printTable.Rows[0]["القسم"].ToString();
            string cuyear = DateTime.Now.Month >= 9 ? DateTime.Now.Year.ToString() : (DateTime.Now.Year - 1).ToString(); ;

            System.Drawing.Font headerfont = new System.Drawing.Font("Arial", 18, FontStyle.Bold);
            System.Drawing.Font subheader = new System.Drawing.Font("Arial",14, FontStyle.Bold);
            System.Drawing.Font textfont = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
            Brush brush = Brushes.Black;
            int margin = 50;
            int x = 50;
            int y = 50;
            int pageh = e.PageBounds.Height;
            int pagew = e.PageBounds.Width - 2 * margin;

            StringFormat format = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.DirectionRightToLeft
            };

            e.Graphics.DrawString("جامعة غريان",headerfont, brush, new Rectangle(x, y, pagew, 30), format); y += 35;
            e.Graphics.DrawString("كلية العلوم الصحية", headerfont,brush, new Rectangle(x, y, pagew, 30), format); y += 35 + x;

            int colmnw = pagew / 5;
            int colmnh = 30;
            string[] colheaders = {"العام الجامعي", "القسم", "السنة", "الإسم", "الرقم_الجامعي" };
            string[] colvalues = {cuyear,stdep,styear,studentName,stno };


            for (int i = 0 ; i < 5; i++)
            {
               int colindex = 4 - i;
               Rectangle rect = new Rectangle(x + i * colmnw, y, colmnw, colmnh);
               Rectangle rectv = new Rectangle(x + i * colmnw, y+colmnh, colmnw, colmnh);

               e.Graphics.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(220, 230, 250)), rect);
               e.Graphics.DrawRectangle(Pens.Black,rect);
               e.Graphics.DrawRectangle(Pens.Black, rectv);
               e.Graphics.DrawString(colheaders[i],textfont,brush,rect,format);
               e.Graphics.DrawString(colvalues[i], textfont, brush,rectv, format);
            }
            y += colmnh + 60;

            int dheaderw = pagew / 6;
            int dheaderh = 30;
            string[] cheaders = { "قاعة", "يوم", "م", "وحدة", "المادة", "رقم المادة" };
            for (int i = 0; i < cheaders.Length; i++)
            {
                Rectangle recth = new Rectangle(x + i * dheaderw, y, dheaderw, dheaderh);
                e.Graphics.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(220, 230, 250)), recth);
                e.Graphics.DrawRectangle(Pens.Black, recth);
                e.Graphics.DrawString(cheaders[i], textfont, brush, recth, format);
            }

            string[] davalues = { "القاعة", "يوم", "المجموعة", "الوحدات", "اسم_المادة", "رقم_المادة" };
            y += colmnh;
            StringFormat newformat = new StringFormat();
            newformat.Alignment = StringAlignment.Center;
            newformat.LineAlignment = StringAlignment.Center;
            newformat.FormatFlags = StringFormatFlags.LineLimit; // يدعم الأسطر المتعددة
            foreach (DataRow row2 in printTable.Rows)
            {
                for (int i = 0; i < cheaders.Length; i++)
                {
                    Rectangle rh = new Rectangle(x + i * dheaderw, y, dheaderw, dheaderh + 30);
                    e.Graphics.DrawRectangle(Pens.Black, rh);

                    string text = "";

                    if (i == 4)
                    {
                        // عمود اسم المادة والدكتور في سطرين
                        string t1 = row2["اسم_المادة"].ToString();
                        string t2 = row2["الدكتور"].ToString();
                        text = t1 + "\n" + t2;
                    }
                    else
                    {
                        // الأعمدة الأخرى
                        text = row2[davalues[i]].ToString();
                    }

                    e.Graphics.DrawString(text, textfont, brush, rh, newformat);
                }

                y += dheaderh + 30;
            }
        
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if(radioButton2.Checked)
            {
                panel2.Visible = false;

                panel3.Visible = true;
                dataGridView5.DataSource = null;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                panel2.Visible = true;
                panel3.Visible = false;
                dataGridView5.DataSource = null;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {

            int year = Convert.ToInt32(comboBox1.SelectedValue);
            int dept = Convert.ToInt32(comboBox2.SelectedValue);

            conn.DatabaseConnection db = new conn.DatabaseConnection();
            using (SqlConnection con = db.OpenConnection())
            {
                // جلب الطلاب الذين ليس لديهم أي تسجيل في جدول Registrations
                string query = @"
                               SELECT s.student_id , s.university_number AS الرقم_الجامعي,
                                      s.full_name AS الإسم, d.dep_name AS القسم,
                                      s.current_year AS السنة, s.gender AS الجنس, s.exam_round AS الدور
                               FROM Students s
                               JOIN Departments d ON s.department_id = d.department_id
                               WHERE s.department_id = @dept
                                     AND s.current_year = @year
                                     AND s.status_id = '1'
                                     AND NOT EXISTS (
                                            SELECT 1 FROM Registrations r WHERE r.student_id = s.student_id
                                     )";


                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.SelectCommand.Parameters.AddWithValue("@dept", dept);
                da.SelectCommand.Parameters.AddWithValue("@year", year);

                DataTable dt = new DataTable();
                da.Fill(dt);

                // أضف أعمدة نصية للعرض بدل تعديل الأعمدة الأصلية
                if (!dt.Columns.Contains("GenderText"))
                    dt.Columns.Add("GenderText", typeof(string));
                if (!dt.Columns.Contains("yearText"))
                    dt.Columns.Add("yearText", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    bool genderBool = Convert.ToBoolean(row["الجنس"]);
                    row["GenderText"] = genderBool ? "ذكر" : "أنثى";

                    switch (row["السنة"].ToString())
                    {
                        case "1":
                            row["yearText"] = "سنة أولى";
                            break;
                        case "2":
                            row["yearText"] = "سنة ثانية";
                            break;
                        case "3":
                            row["yearText"] = "سنة ثالثة";
                            break;
                        case "4":
                            row["yearText"] = "سنة رابعة";
                            break;
                        default:
                            MessageBox.Show("شكل الإدخال يجب ان يكون مثل سنة أولى");
                            break;
                    }

                }

                dataGridView5.DataSource = dt;
                // إخفاء الأعمدة الأصلية
                dataGridView5.Columns["الجنس"].Visible = false;
                dataGridView5.Columns["السنة"].Visible = false;


                // عرض الأعمدة النصية بدلاً منها
                dataGridView5.Columns["GenderText"].HeaderText = "الجنس";
                dataGridView5.Columns["yearText"].HeaderText = "السنة";


                // باقي التنسيق
                datagridviewstyle(dataGridView5);
                dataGridView5.Columns["student_id"].Visible = false;

            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridView5.DataSource = null;
            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    string q = "SELECT " +
                        "c.course_id AS رمز_المادة" +
                        " , c.course_name AS اسم_المادة" +
                        " FROM Courses c " +
                        "JOIN Course_Department cd ON cd.course_id = c.course_id " +
                        "WHERE c.year_number = @YearNumber AND cd.department_id = @DepartmentID";

                    SqlCommand cmd = new SqlCommand(q, con);
                    cmd.Parameters.AddWithValue("@YearNumber", comboBox1.SelectedValue);
                    cmd.Parameters.AddWithValue("@DepartmentID", comboBox2.SelectedIndex + 1);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    supjectTable = dt;
                    dataGridView4.DataSource = supjectTable;
                    datagridviewstyle(dataGridView4);
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There is an Error : " + ex.Message);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridView5.DataSource = null; 
            if(comboBox2.SelectedItem != null)
            {
                comboBox2_SelectedIndexChanged(null,null);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            dataGridView5.DataSource = null;
            SearchStudent();
        }
        private void SearchStudent()
        {
            if (!string.IsNullOrEmpty(textBox3.Text))
            {
                conn.DatabaseConnection db2 = new conn.DatabaseConnection();
                SqlConnection con2 = db2.OpenConnection();

                string q2 = "SELECT s.student_id, s.university_number,s.full_name,d.dep_name AS القسم,s.current_year,t.description,s.gender,s.nationality,s.exam_round FROM Students s JOIN " +
                    "Departments d ON s.department_id = d.department_id JOIN Status t ON s.status_id = t.status_id WHERE university_number = @university_number";

                try
                {
                    SqlCommand cmd = new SqlCommand(q2, con2);
                    cmd.Parameters.AddWithValue("@university_number", textBox3.Text.Trim());

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // أضف أعمدة نصية للعرض بدل تعديل الأعمدة الأصلية
                    if (!dt.Columns.Contains("GenderText"))
                        dt.Columns.Add("GenderText", typeof(string));
                    if (!dt.Columns.Contains("yearText"))
                        dt.Columns.Add("yearText", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        bool genderBool = Convert.ToBoolean(row["gender"]);
                        row["GenderText"] = genderBool ? "ذكر" : "أنثى";

                        switch (row["current_year"].ToString())
                        {
                            case "1":
                                row["yearText"] = "سنة أولى";
                                break;
                            case "2":
                                row["yearText"] = "سنة ثانية";
                                break;
                            case "3":
                                row["yearText"] = "سنة ثالثة";
                                break;
                            case "4":
                                row["yearText"] = "سنة رابعة";
                                break;
                            default:
                                MessageBox.Show("شكل الإدخال يجب ان يكون مثل سنة أولى");
                                break;
                        }

                    }

                    dataGridView5.DataSource = dt;

                    // إخفاء الأعمدة الأصلية
                    dataGridView5.Columns["gender"].Visible = false;
                    dataGridView5.Columns["current_year"].Visible = false;


                    // عرض الأعمدة النصية بدلاً منها
                    dataGridView5.Columns["GenderText"].HeaderText = "الجنس";
                    dataGridView5.Columns["exam_round"].HeaderText = "الدور";
                    dataGridView5.Columns["yearText"].HeaderText = "السنة";


                    // باقي التنسيق
                    datagridviewstyle(dataGridView5);
                    dataGridView5.Columns["full_name"].HeaderText = "الإسم";
                    dataGridView5.Columns["university_number"].HeaderText = "الرقم الجامعي";
                    dataGridView5.Columns["description"].HeaderText = "الحالة";
                    dataGridView5.Columns["description"].ReadOnly = true;
                    dataGridView5.Columns["student_id"].Visible = false;
                    dataGridView5.Columns["nationality"].HeaderText = "الجنسية";


                }
                catch (Exception ex)
                {
                    MessageBox.Show("حدث خطأ أثناء جلب البيانات: " + ex.Message);

                }
                finally
                {
                    db2.CloseConnection();
                }
            }
            else
            {
                MessageBox.Show("يرجى إدخال رقم القيد أولاً.");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if(radioButton1.Checked)
                downloadForOneStudents();
            else
                downloadForMultiStudents();

        }
        public void downloadForOneStudents()
        {
            
            if (dataGridView5 == null || dataGridView5.Rows.Count == 0)
            {
                MessageBox.Show("لا يوجد بيانات لتخزينها، يرجى البحث عن الطالب قبل التخزين.");
                return;
            }
            if(comboBox1.SelectedValue.ToString() != dataGridView5.Rows[0].Cells["current_year"].Value.ToString())
            {
                MessageBox.Show("المواد اللتي تقوم بتنزيلها لا تناسب السنة الحالية للطالب");
                return;
            }
            if (dataGridView5.Rows[0].Cells["description"].Value.ToString() != "مستمر")
            {
                MessageBox.Show("لا يمكن تنزيل مواد لطالب غير مستمر");
                return;
            }

            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    int studentId = Convert.ToInt32(dataGridView5.Rows[0].Cells["student_id"].Value);
                    int year = Convert.ToInt32(comboBox1.SelectedValue);
                    int dept = Convert.ToInt32(comboBox2.SelectedValue);

                    // ✅ تحقق أن الطالب من القسم المحدد
                    SqlCommand checkCmd = new SqlCommand(@"
            SELECT COUNT(*) 
            FROM Students 
            WHERE student_id = @studentId AND department_id = @departmentId", con);

                    checkCmd.Parameters.AddWithValue("@studentId", studentId);
                    checkCmd.Parameters.AddWithValue("@departmentId", dept);

                    int count = (int)checkCmd.ExecuteScalar();

                    if (count == 0)
                    {
                        MessageBox.Show("خطأ: الطالب لا ينتمي إلى القسم المحدد، لا يمكن تنزيل المواد.");
                        return;
                    }

                    // ✅ جلب المواد الخاصة بالسنة والقسم
                    SqlCommand coursesCmd = new SqlCommand(@"
                                                        SELECT c.course_id, c.course_name
                                                        FROM Courses c
                                                        JOIN Course_Department cd ON cd.course_id = c.course_id
                                                        WHERE c.year_number = @year AND cd.department_id = @dept", con);

                    coursesCmd.Parameters.AddWithValue("@year", year);
                    coursesCmd.Parameters.AddWithValue("@dept", dept);

                    SqlDataAdapter adapter = new SqlDataAdapter(coursesCmd);
                    DataTable courses = new DataTable();
                    adapter.Fill(courses);

                    List<string> موادممتلئة = new List<string>();
                    int عددالموادالمسجلة = 0;

                    foreach (DataRow row in courses.Rows)
                    {
                        int courseId = Convert.ToInt32(row["course_id"]);
                        string courseName = row["course_name"].ToString();

                        // ✅ جلب كل المجموعات المرتبطة بالمادة
                        SqlCommand getGroupsCmd = new SqlCommand(@"
                                                                SELECT cc.id,
                                                                cc.capacity,       
                                                                cc.group_number
                                                                FROM Course_Classroom cc
                                                                WHERE cc.course_id = @courseId
                                                                ORDER BY cc.group_number;
                                                                ", con); // أو حسب cc.id إن أردت

                        getGroupsCmd.Parameters.AddWithValue("@courseId", courseId);

                        SqlDataAdapter groupAdapter = new SqlDataAdapter(getGroupsCmd);
                        DataTable groups = new DataTable();
                        groupAdapter.Fill(groups);

                        bool تمت_الإضافة = false;

                        foreach (DataRow group in groups.Rows)
                        {
                            int groupId = Convert.ToInt32(group["id"]);
                            int capacity = Convert.ToInt32(group["capacity"]);

                            SqlCommand countCmd = new SqlCommand(@"
                                                              SELECT COUNT(*) 
                                                              FROM Registrations 
                                                              WHERE course_classroom_id = @groupId", con);
                            countCmd.Parameters.AddWithValue("@groupId", groupId);

                            int currentCount = (int)countCmd.ExecuteScalar();

                            if (currentCount < capacity)
                            {
                                int month2; 

                                using (SqlCommand cmddate = new SqlCommand("SELECT month_number FROM Months WHERE month_id = 1 ", con))
                                {
                                    month2 = Convert.ToInt32(cmddate.ExecuteScalar());
                                }

                                int academicYearStart = DateTime.Now.Month >= month2 ? DateTime.Now.Year : DateTime.Now.Year - 1;
                                

                                SqlCommand insertCmd = new SqlCommand(@"
                                IF NOT EXISTS (
                                    SELECT 1 FROM Registrations 
                                    WHERE student_id = @studentId AND course_id = @courseId AND academic_year_start = @academicYearStart
                                )
                                INSERT INTO Registrations 
                                (student_id, course_id, year_number, status, course_classroom_id, academic_year_start)
                                VALUES 
                                (@studentId, @courseId, @year, N'مسجل', @groupId, @academicYearStart)", con);

                                insertCmd.Parameters.AddWithValue("@studentId", studentId);
                                insertCmd.Parameters.AddWithValue("@courseId", courseId);
                                insertCmd.Parameters.AddWithValue("@year", year);
                                insertCmd.Parameters.AddWithValue("@groupId", groupId);
                                insertCmd.Parameters.AddWithValue("@academicYearStart", academicYearStart);


                                int affected = insertCmd.ExecuteNonQuery();
                                if (affected > 0)
                                {
                                    عددالموادالمسجلة++;
                                    تمت_الإضافة = true;
                                }

                                break; // سجل الطالب، لا داعي لتجربة بقية المجموعات
                            }
                        }

                        if (!تمت_الإضافة)
                        {
                            موادممتلئة.Add(courseName);
                        }
                    }

                    // ✅ عرض النتيجة النهائية
                    if (عددالموادالمسجلة == 0)
                    {
                        MessageBox.Show("لم يتم تسجيل أي مادة. الطالب قد يكون مسجلاً مسبقًا أو لا توجد مقاعد متاحة.");
                    }
                    else if (موادممتلئة.Count > 0)
                    {
                        MessageBox.Show("تم تسجيل الطالب، باستثناء المواد التالية التي لم يتوفر بها مقاعد:\n" +
                                        string.Join("\n", موادممتلئة));
                    }
                    else
                    {
                        MessageBox.Show("تم تسجيل جميع المواد بنجاح.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ: " + ex.Message);
            }
        }



        public void downloadForMultiStudents()
        {
            if (dataGridView5 == null || dataGridView5.Rows.Count == 0)
            {
                MessageBox.Show("لا يوجد بيانات لتخزينها.");
                return;
            }

            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    int year = Convert.ToInt32(comboBox1.SelectedValue);
                    int dept = Convert.ToInt32(comboBox2.SelectedValue);

                    

                    // ✅ جلب المواد الخاصة بالسنة والقسم
                    SqlCommand coursesCmd = new SqlCommand(@"
                                                           SELECT c.course_id, c.course_name
                                                           FROM Courses c
                                                           JOIN Course_Department cd ON cd.course_id = c.course_id
                                                           WHERE c.year_number = @year AND cd.department_id = @dept", con);

                    coursesCmd.Parameters.AddWithValue("@year", year);
                    coursesCmd.Parameters.AddWithValue("@dept", dept);

                    SqlDataAdapter adapter = new SqlDataAdapter(coursesCmd);
                    DataTable courses = new DataTable();
                    adapter.Fill(courses);
                    Dictionary<string, List<string>> downlodedcourses = new Dictionary<string, List<string>>();
                    Dictionary<string,List<string>> fullcourses = new Dictionary<string,List<string>>();
                    int countfullcourses = 0;
                    

                    foreach (DataRow row in courses.Rows)
                    {
                        int counRegisteredCourses = 0;
                        int courseId = Convert.ToInt32(row["course_id"]);
                        string courseName = row["course_name"].ToString();

                        // ✅ جلب كل المجموعات المرتبطة بالمادة
                        SqlCommand getGroupsCmd = new SqlCommand(@"
                                                                SELECT cc.id,
                                                                cc.capacity,       
                                                                cc.group_number
                                                                FROM Course_Classroom cc
                                                                WHERE cc.course_id = @courseId
                                                                ORDER BY cc.group_number;", con); 

                        getGroupsCmd.Parameters.AddWithValue("@courseId", courseId);

                        SqlDataAdapter groupAdapter = new SqlDataAdapter(getGroupsCmd);
                        DataTable groups = new DataTable();
                        groupAdapter.Fill(groups);

                        bool addcourse = true;
                        foreach (DataGridViewRow strow in dataGridView5.Rows)
                        {
                            if(!addcourse)
                            {
                                break;
                            }
                            addcourse = false;
                            if (strow.IsNewRow) continue;
                            if (strow.Cells["student_id"].Value.ToString() == null) continue;
                            int studentId = Convert.ToInt32(strow.Cells["student_id"].Value);

                            foreach (DataRow group in groups.Rows)
                            {
                            int groupId = Convert.ToInt32(group["id"]);
                            int capacity = Convert.ToInt32(group["capacity"]);

                            SqlCommand countCmd = new SqlCommand(@"
                                                              SELECT COUNT(*) 
                                                              FROM Registrations 
                                                              WHERE course_classroom_id = @groupId", con);
                            countCmd.Parameters.AddWithValue("@groupId", groupId);

                            int currentCount = (int)countCmd.ExecuteScalar();

                            

                                if (currentCount < capacity)
                                {
                                    int month3;

                                    using (SqlCommand cmddate = new SqlCommand("SELECT month_number FROM Months WHERE month_id = 1 ", con))
                                    {
                                        month3 = Convert.ToInt32(cmddate.ExecuteScalar());
                                    }

                                    int academicYearStart = DateTime.Now.Month >= month3 ? DateTime.Now.Year : DateTime.Now.Year - 1;
                                   

                                    SqlCommand insertCmd = new SqlCommand(@"
                                    IF NOT EXISTS (
                                        SELECT 1 FROM Registrations 
                                        WHERE student_id = @studentId AND course_id = @courseId AND academic_year_start = @academicYearStart
                                    )
                                    INSERT INTO Registrations 
                                    (student_id, course_id, year_number, status, course_classroom_id, academic_year_start)
                                    VALUES 
                                    (@studentId, @courseId, @year, N'مسجل', @groupId, @academicYearStart)", con);

                                    insertCmd.Parameters.AddWithValue("@studentId", studentId);
                                    insertCmd.Parameters.AddWithValue("@courseId", courseId);
                                    insertCmd.Parameters.AddWithValue("@year", year);
                                    insertCmd.Parameters.AddWithValue("@groupId", groupId);
                                    insertCmd.Parameters.AddWithValue("@academicYearStart", academicYearStart);

                                    int affected = insertCmd.ExecuteNonQuery();
                                    if (affected > 0)
                                    {
                                        counRegisteredCourses ++;
                                        // أول مرة
                                        if (!downlodedcourses.ContainsKey(courseName))
                                            downlodedcourses[courseName] = new List<string>();
                                        downlodedcourses[courseName].Add(strow.Cells["الإسم"]?.Value.ToString() ?? "");

                                        addcourse = true;
                                    }

                                    break; // سجل الطالب، لا داعي لتجربة بقية المجموعات
                                }
                            }
                        }

                        if (counRegisteredCourses < dataGridView5.Rows.Count)
                        {
                            for (int i = counRegisteredCourses ; i < dataGridView5.Rows.Count; i++)
                            {
                                if (dataGridView5.Rows[i].IsNewRow)continue;
                                if (dataGridView5.Rows[i].Cells["الإسم"].Value.ToString() == null) continue;
                                if (!fullcourses.ContainsKey(courseName))
                                    fullcourses[courseName] = new List<string>();
                                fullcourses[courseName].Add(dataGridView5.Rows[i].Cells["الإسم"].Value.ToString());
                            }
                            countfullcourses ++;
                        }

                    }

                    // ✅ عرض النتيجة النهائية
                    if (downlodedcourses.Count == 0)
                    {
                        MessageBox.Show("لم يتم تسجيل أي مادة للطلاب. لا توجد مقاعد متاحة.");
                    }
                    else if (downlodedcourses.Count > 0 && fullcourses.Count > 0)
                    {
                        string result = "";

                        foreach (var prcourse in downlodedcourses)
                        {
                            result += $"Course: {prcourse.Key}\n";

                            foreach (var student in prcourse.Value)
                            {
                                result += $"- {student}\n";
                            }

                            result += "------------------------\n";
                        }
                        string rt = "";

                        foreach (var fucourse in fullcourses)
                        {
                            rt += $"Course: {fucourse.Key}\n";

                            foreach (var fstudent in fucourse.Value)
                            {
                                rt += $"- {fstudent}\n";
                            }

                            rt += "------------------------\n";
                        }

                        MessageBox.Show("تم تنزيل الموالد الاتية لكل من : \n" + result + "\n لم يتم تنزيل المواد الاتية لكل من : \n" + rt );

                    }
                    else
                    {
                        MessageBox.Show("تم تسجيل جميع المواد لكل الطلبة بنجاح.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ: " + ex.Message);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button6_Click(null, null);
                e.SuppressKeyPress = true;
            }
        }
    }
}