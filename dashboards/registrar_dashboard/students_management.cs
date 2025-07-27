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
                    MessageBox.Show("تمت إضافة الطالب بنجاح");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ: " + ex.Message);
                }
                finally
                {
                    db.CloseConnection();
                }
            }
            else
            {
                MessageBox.Show("يرجى ملئ الحقول");
            }
        



    }

        private void students_management_Load(object sender, EventArgs e)
        {
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
    }
}
