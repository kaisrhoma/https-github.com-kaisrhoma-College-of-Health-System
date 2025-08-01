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
using System.Windows.Forms;

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
        string studentName = "";


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
            int colmnw = pagew / 4;
            for(int i = 0 ; i < 4; i++)
            {
                int colindex = 4 - i;
               Rectangle rect = new Rectangle(x + i * colmnw, y, colmnw,30);
               
               e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, 230, 250)), rect);
               e.Graphics.DrawRectangle(Pens.Black,rect);
                e.Graphics.DrawString("قيس ميلود",textfont,brush,rect,format);
            }
            

        }
    }
}
