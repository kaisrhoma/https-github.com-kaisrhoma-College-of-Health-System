using college_of_health_sciences.moduls;
using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml;

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
            Font headerFont = new Font("Arial", 14, FontStyle.Bold);
            Font bodyFont = new Font("Arial", 10);
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
                {1, "سنة اولى"},
                {2, "سنة ثانية"},
                {3, "سنة ثالثة"},
                {4, "سنة رابعة"}
            };

            comboBox1.DataSource = new BindingSource(study_year, null);
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";

            var departments = new Dictionary<int, string>()
            {
                {1, "قسم الرياضيات"},
                {2, "قسم الكيمياء"},
                {3, "قسم الفيزياء"}
            };

            comboBox2.DataSource = new BindingSource(departments, null);
            comboBox2.DisplayMember = "Value";
            comboBox2.ValueMember = "Key";
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
                    string q = @"
                               SELECT
                                   s.full_name AS الإسم,
                                   s.university_number AS الرقم_الجامعي,
                                   s.college AS الكلية,
                                   s.current_year AS السنة,
                                   d.dep_name AS القسم,
                                   
                                   c.units AS الوحدات, 
                                   c.course_id AS رقم_المادة,
                                   c.course_name AS اسم_المادة,
                                   c.theory_hours AS الساعات_النظرية,
                                   c.practical_hours AS الساعات_العملية,
                                   c.credit_hrs AS مجموع_الساعات,
                               
                                   cc.schedule AS اليوم, 
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
                               ";


                    using (SqlCommand cmd = new SqlCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@university_number", textBox1.Text.Trim());
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
                        dataGridView1.Columns["الساعات_النظرية"].ReadOnly = true;
                        dataGridView1.Columns["الساعات_العملية"].ReadOnly = true;
                        dataGridView1.Columns["مجموع_الساعات"].ReadOnly = true;
                        dataGridView1.Columns["المجموعة"].ReadOnly = true;
                        dataGridView1.Columns["القاعة"].ReadOnly = true;
                        dataGridView1.Columns["اليوم"].ReadOnly = true;
                        dataGridView1.Columns["الدكتور"].ReadOnly = true;


                        if (dataGridView1.Rows.Count == 0 || dataGridView1.Rows[0].IsNewRow)
                        {
                            MessageBox.Show("لايوجد طالب بهذا الرقم او ان الطالب قيده متوقف");
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
            string cuyear =DateTime.Now.ToString("yyyy/MM/dd");

            Font headerfont = new Font("Arial", 18, FontStyle.Bold);
            Font subheader = new Font("Arial",14, FontStyle.Bold);
            Font textfont = new Font("Arial", 12, FontStyle.Bold);
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

               e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, 230, 250)), rect);
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
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, 230, 250)), recth);
                e.Graphics.DrawRectangle(Pens.Black, recth);
                e.Graphics.DrawString(cheaders[i], textfont, brush, recth, format);
            }

            string[] davalues = { "القاعة", "اليوم", "المجموعة", "الوحدات", "اسم_المادة", "رقم_المادة" };
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

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
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
                    if (!dt.Columns.Contains("ExamRoundText"))
                        dt.Columns.Add("ExamRoundText", typeof(string));
                    if (!dt.Columns.Contains("yearText"))
                        dt.Columns.Add("yearText", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        bool genderBool = Convert.ToBoolean(row["gender"]);
                        row["GenderText"] = genderBool ? "ذكر" : "أنثى";

                        bool roundBool = Convert.ToBoolean(row["exam_round"]);
                        row["ExamRoundText"] = roundBool ? "الثاني" : "الأول";

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
                    dataGridView5.Columns["exam_round"].Visible = false;
                    dataGridView5.Columns["current_year"].Visible = false;


                    // عرض الأعمدة النصية بدلاً منها
                    dataGridView5.Columns["GenderText"].HeaderText = "الجنس";
                    dataGridView5.Columns["ExamRoundText"].HeaderText = "الدور";
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
            if (dataGridView5 == null || dataGridView5.Rows.Count == 0)
            {
                MessageBox.Show("لا يوجد بيانات لتخزينها، يرجى البحث عن الطالب قبل التخزين.");
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

                    // ✅ تحقق أولاً أن الطالب من نفس القسم
                    SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Students WHERE student_id = @studentId AND department_id = @departmentId", con);
                    checkCmd.Parameters.AddWithValue("@studentId", studentId);
                    checkCmd.Parameters.AddWithValue("@departmentId", dept);

                    int count = (int)checkCmd.ExecuteScalar();

                    if (count == 0)
                    {
                        MessageBox.Show("خطأ: الطالب لا ينتمي إلى القسم المحدد، لا يمكن تنزيل المواد.");
                        return;
                    }

                    // ✅ استعلام تنزيل المواد
                    string q = @"
            INSERT INTO Registrations (student_id, course_id, year_number, status, course_classroom_id)
            SELECT @studentId, c.course_id, @yearNumber, N'مسجل',
                   (SELECT TOP 1 cc.id FROM Course_Classroom cc WHERE cc.course_id = c.course_id)
            FROM Courses c
            JOIN Course_Department cd ON cd.course_id = c.course_id
            WHERE c.year_number = @yearNumber AND cd.department_id = @departmentId
            AND NOT EXISTS (
                SELECT 1 FROM Registrations 
                WHERE student_id = @studentId AND course_id = c.course_id
            )";

                    SqlCommand cmd = new SqlCommand(q, con);
                    cmd.Parameters.AddWithValue("@studentId", studentId);
                    cmd.Parameters.AddWithValue("@yearNumber", year);
                    cmd.Parameters.AddWithValue("@departmentId", dept);

                    int affected = cmd.ExecuteNonQuery();

                    if (affected > 0)
                    {
                        MessageBox.Show("تم تنزيل المواد بنجاح.");
                    }
                    else
                    {
                        MessageBox.Show("لم يتم تنزيل أي مادة. ربما المواد موجودة مسبقًا.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ: " + ex.Message);
            }

        }
    }
}