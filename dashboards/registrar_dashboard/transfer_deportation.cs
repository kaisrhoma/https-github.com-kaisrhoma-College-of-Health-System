using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Office.Word;
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

namespace college_of_health_sciences.dashboards.registrar_dashboard
{
    public partial class transfer_deportation : UserControl
    {
        public transfer_deportation()
        {
            InitializeComponent();
        }

        public void datagridviewstyle(DataGridView datagrid)
        {
            datagrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            datagrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            datagrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void SearchStudent()
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                conn.DatabaseConnection db2 = new conn.DatabaseConnection();
                SqlConnection con2 = db2.OpenConnection();

                string q2 = "SELECT s.student_id, s.university_number AS الرقم_الجامعي,s.full_name الإسم,d.dep_name AS القسم,s.current_year,t.description AS الحالة,s.gender,s.nationality AS الجنسية,s.exam_round AS الدور FROM Students s JOIN " +
                    "Departments d ON s.department_id = d.department_id JOIN Status t ON s.status_id = t.status_id WHERE university_number = @university_number";

                try
                {
                    SqlCommand cmd = new SqlCommand(q2, con2);
                    cmd.Parameters.AddWithValue("@university_number", textBox1.Text.Trim());

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

                    dataGridView1.DataSource = dt;

                    // إخفاء الأعمدة الأصلية
                    dataGridView1.Columns["gender"].Visible = false;
                    dataGridView1.Columns["current_year"].Visible = false;


                    // عرض الأعمدة النصية بدلاً منها
                    dataGridView1.Columns["GenderText"].HeaderText = "الجنس";
                    dataGridView1.Columns["yearText"].HeaderText = "السنة";


                    // باقي التنسيق
                    datagridviewstyle(dataGridView1);
                    dataGridView1.Columns["student_id"].Visible = false;


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
        private void button5_Click(object sender, EventArgs e)
        {
            SearchStudent();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button5_Click(null, null);
                e.SuppressKeyPress = true;
            }
        }

        private void transfer_deportation_Load(object sender, EventArgs e)
        {
            textBox1.Focus();
            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    string q = "select * from Departments";
                    SqlDataAdapter da = new SqlDataAdapter(q, con);
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

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0 || dataGridView1.Rows[0].IsNewRow)
            {
                MessageBox.Show("يرجى البحث عن الطالب اولا");
                return;
            }
            int stu_id = Convert.ToInt32(dataGridView1.Rows[0].Cells["student_id"].Value.ToString());
            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    if (!isStudentHaveRegistrations(stu_id))
                    {
                        string updateQuery = @"
                    UPDATE Students SET
                    department_id = @department_id 
                    WHERE student_id = @student_id";
                        using (SqlCommand cmdnoreg = new SqlCommand(updateQuery, con))
                        {
                            cmdnoreg.Parameters.AddWithValue("@student_id", stu_id);
                            cmdnoreg.Parameters.AddWithValue("@department_id", comboBox2.SelectedValue);
                            int rowsAffected = cmdnoreg.ExecuteNonQuery();
                            MessageBox.Show(rowsAffected > 0 ? "تم تغيير القسم بنجاح." : "لم يتم تغيير القسم.");
                            button5_Click(null, null);
                        }
                    }
                    else
                    {
                        DialogResult answer = MessageBox.Show(
                            "هل انت متأكد من تبدل القسم للطالب ؟\nللعلم سيتم مسح كل المواد في القسم السابق\nوالتي غير موجودة في القسم الحالي\nوبما في ذلك الدرجات\n\nللتأكيد اضغط :\n- نعم\nللتراجع اضغط :\n- لا",
                            "تأكيد التحويل",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation,
                            MessageBoxDefaultButton.Button2,
                            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                        );

                        if (answer == DialogResult.Yes)
                        {
                            string updateq = @"
                        -- حذف الدرجات للمواد التي لا تنتمي للقسم الجديد
                        DELETE g
                        FROM Grades g
                        WHERE g.student_id = @studentId
                        AND g.course_id NOT IN (
                            SELECT course_id
                            FROM Course_Department
                            WHERE department_id = @newDeptId
                        );

                        -- حذف التسجيلات للمواد التي لا تنتمي للقسم الجديد
                        DELETE r
                        FROM Registrations r
                        WHERE r.student_id = @studentId
                        AND r.course_id NOT IN (
                            SELECT course_id
                            FROM Course_Department
                            WHERE department_id = @newDeptId
                        );

                        -- تحديث القسم للطالب
                        UPDATE Students
                        SET department_id = @newDeptId
                        WHERE student_id = @studentId;
                    ";

                            using (SqlCommand cmdupdate = new SqlCommand(updateq, con))
                            {
                                cmdupdate.Parameters.AddWithValue("@studentId", stu_id);
                                cmdupdate.Parameters.AddWithValue("@newDeptId", comboBox2.SelectedValue);
                                int rows = cmdupdate.ExecuteNonQuery(); // تنفذ الاستعلامات
                                MessageBox.Show("تم التحويل بنجاح");
                                button5_Click(null, null);
                            }
                        }
                        else
                        {
                            MessageBox.Show("لم يتم التحويل");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There is an Error : " + ex.Message);
            }
        }


        public bool isStudentHaveRegistrations(int id)
        {
            int value = 0;
            try
            {
                conn.DatabaseConnection db1 = new conn.DatabaseConnection();
                using (SqlConnection con1 = db1.OpenConnection())
                {
                    using (SqlCommand cmd1 = new SqlCommand("SELECT COUNT(*) FROM Registrations WHERE student_id = @student_id",con1))
                    {
                        cmd1.Parameters.AddWithValue("@student_id", id);
                        value = Convert.ToInt32(cmd1.ExecuteScalar());
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("There is an Error : " + ex.Message);
            }
            if (value > 0)
            return true;
            else return false;
        }
    }
}
