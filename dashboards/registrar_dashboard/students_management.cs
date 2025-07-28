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
    public partial class students_management : UserControl
    {

        

        public students_management()
        {
            InitializeComponent();

        }

        public void SetFieldsEmpty()
        {
            textBox2.Text = "";
            textBox3.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            comboBox3.SelectedItem = null;
            comboBox4.SelectedItem = null;
            comboBox5.SelectedItem = null;
            dateTimePicker1.Value = DateTime.Now;
        }


        public bool CheckNullFields()
        {
            bool hasError = false;

            if (string.IsNullOrEmpty(textBox2.Text))
            {
                errorProvider1.SetError(textBox2, "يرجى ملئ الحقل قبل الحفظ");
                hasError = true;
            }
            else
            {
                errorProvider1.SetError(textBox2, "");
            }

            if (string.IsNullOrEmpty(textBox3.Text))
            {
                errorProvider1.SetError(textBox3, "يرجى ملئ الحقل قبل الحفظ");
                hasError = true;
            }
            else
            {
                errorProvider1.SetError(textBox3, "");
            }

            if (!dateTimePicker1.Checked)
            {
                errorProvider1.SetError(dateTimePicker1, "يرجى ملئ الحقل قبل الحفظ");
                hasError = true;
            }
            else
            {
                errorProvider1.SetError(dateTimePicker1, "");
            }

            if (string.IsNullOrEmpty(textBox5.Text))
            {
                errorProvider1.SetError(textBox5, "يرجى ملئ الحقل قبل الحفظ");
                hasError = true;
            }
            else
            {
                errorProvider1.SetError(textBox5, "");
            }

            if (!radioButton1.Checked && !radioButton2.Checked)
            {
                errorProvider1.SetError(radioButton1, "يرجى اختيار أحد الخيارات");
                hasError = true;
            }
            else
            {
                errorProvider1.SetError(radioButton1, "");
            }

            if (comboBox3.SelectedItem == null)
            {
                errorProvider1.SetError(comboBox3, "يرجى ملئ الحقل قبل الحفظ");
                hasError = true;
            }
            else
            {
                errorProvider1.SetError(comboBox3, "");
            }

            if (comboBox4.SelectedItem == null)
            {
                errorProvider1.SetError(comboBox4, "يرجى ملئ الحقل قبل الحفظ");
                hasError = true;
            }
            else
            {
                errorProvider1.SetError(comboBox4, "");
            }

            if (comboBox5.SelectedItem == null)
            {
                errorProvider1.SetError(comboBox5, "يرجى ملئ الحقل قبل الحفظ");
                hasError = true;
            }
            else
            {
                errorProvider1.SetError(comboBox5, "");
            }

            return !hasError; // إذا لا يوجد خطأ -> true
        }


        //اضافة طالب جديد الى القاعدة في التاب الأول
        private void button7_Click(object sender, EventArgs e)
        {
            if (CheckNullFields())
            {
                bool st_gender = false;
                if (radioButton1.Checked) st_gender = true;
                else st_gender = true;

                conn.DatabaseConnection db = new conn.DatabaseConnection();
                SqlConnection con = db.OpenConnection();

                string q = "INSERT INTO Students (university_number, full_name, college, department_id, current_year, status_id, documents_path, gender, birth_date, nationality, exam_round) " +
                           "VALUES (@university_number, @full_name, N'كلية العلوم الصحية', @department_id, @current_year, @status_id, NULL, @gender, @birth_date, @nationality,0)";

                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@university_number", textBox3.Text.Trim());
                cmd.Parameters.AddWithValue("@full_name", textBox2.Text.Trim());
                cmd.Parameters.AddWithValue("@department_id", comboBox3.SelectedValue);
                cmd.Parameters.AddWithValue("@current_year", comboBox4.SelectedValue);
                cmd.Parameters.AddWithValue("@status_id", comboBox5.SelectedValue);
                cmd.Parameters.AddWithValue("@gender", st_gender);
                cmd.Parameters.AddWithValue("@birth_date", dateTimePicker1.Value);
                cmd.Parameters.AddWithValue("@nationality", textBox5.Text.Trim());

                try
                {
                    cmd.ExecuteNonQuery();
                    label1.ForeColor = Color.Green;
                    label1.Text = "تمت إضافة الطالب بنجاح";
                    SetFieldsEmpty();
                }
                catch (Exception ex)
                {
                    label1.ForeColor = Color.Red;
                    label1.Text = "خطأ: " + ex.Message;
                }
                finally
                {
                    db.CloseConnection();
                }
            }
            else
            {
                label1.ForeColor = Color.Red;
                label1.Text = "يرجى ملئ الحقول !";
            }
        }




        private void students_management_Load(object sender, EventArgs e)
        {
            label1.Text = "";
            textBox2.Focus();

            var departments = new Dictionary<int, string>()
            {
                {1, "قسم الرياضيات"},
                {2, "قسم الكيمياء"},
                {3, "قسم الفيزياء"}
            };

            comboBox3.DataSource = new BindingSource(departments, null);
            comboBox3.DisplayMember = "Value";
            comboBox3.ValueMember = "Key";


            var study_year = new Dictionary<int, string>()
            {
                {1, "1"},
                {2, "2"},
                {3, "3"},
                {4, "4"}
            };

            comboBox4.DataSource = new BindingSource(study_year, null);
            comboBox4.DisplayMember = "Value";
            comboBox4.ValueMember = "Key";


            var study_status = new Dictionary<int, string>()
            {
                {1, "مستمر"},
                {2, "مؤجل"},
                {3, "مستبعد"},
                {4, "خريج"}
            };

            comboBox5.DataSource = new BindingSource(study_status, null);
            comboBox5.DisplayMember = "Value";
            comboBox5.ValueMember = "Key";
        }


        // تحط الركيز علي العناصر في التاب الحالي
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage1) 
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

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) 
            {
                textBox3.Focus();
                e.SuppressKeyPress = true;
            }
        }


        private void comboBox5_SelectionChangeCommitted(object sender, EventArgs e)
        {
            textBox5.Focus();
        }

        private void textBox5_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                button7_Click(null, null);
                e.SuppressKeyPress = true;
            }
        }


        // جلب بيانات الطالب حسب رقم القيد و السماح بتعديلها وعرضها في الجدول 
        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                conn.DatabaseConnection db2 = new conn.DatabaseConnection();
                SqlConnection con2 = db2.OpenConnection();

                string q2 = "SELECT s.university_number,s.full_name,d.dep_name,s.current_year,t.description,s.gender,s.birth_date,s.nationality,s.exam_round FROM Students s JOIN " +
                    "Departments d ON s.department_id = d.department_id JOIN Status t ON s.status_id = t.status_id WHERE university_number = @university_number";

                try
                {
                    SqlCommand cmd = new SqlCommand(q2, con2);
                    cmd.Parameters.AddWithValue("@university_number", txtSearch.Text.Trim());

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
                            default : MessageBox.Show("شكل الإدخال يجب ان يكون مثل سنة أولى");
                                break;
                        }

                    }

                    dataGridView2.DataSource = dt;

                    // إخفاء الأعمدة الأصلية
                    dataGridView2.Columns["gender"].Visible = false;
                    dataGridView2.Columns["exam_round"].Visible = false;
                    dataGridView2.Columns["current_year"].Visible = false;

                    // عرض الأعمدة النصية بدلاً منها
                    dataGridView2.Columns["GenderText"].HeaderText = "الجنس";
                    dataGridView2.Columns["ExamRoundText"].HeaderText = "الدور";
                    dataGridView2.Columns["yearText"].HeaderText = "السنة";

                    // باقي التنسيق
                    dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView2.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dataGridView2.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dataGridView2.Columns["full_name"].HeaderText = "الإسم";
                    dataGridView2.Columns["university_number"].HeaderText = "الرقم الجامعي";
                    dataGridView2.Columns["dep_name"].HeaderText = "القسم";
                    dataGridView2.Columns["current_year"].HeaderText = "السنة";
                    dataGridView2.Columns["description"].HeaderText = "الحالة";
                    dataGridView2.Columns["birth_date"].HeaderText = "تاريخ الميلاد";
                    dataGridView2.Columns["nationality"].HeaderText = "الجنسية";

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


        private void button4_Click(object sender, EventArgs e)
        {

        }
    }
}
