using college_of_health_sciences.moduls;
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

        private void SearchStudent()
        {
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                conn.DatabaseConnection db2 = new conn.DatabaseConnection();
                SqlConnection con2 = db2.OpenConnection();

                string q2 = "SELECT s.student_id, s.university_number,s.full_name,d.dep_name,s.current_year,t.description,s.gender,s.birth_date,s.nationality,s.exam_round FROM Students s JOIN " +
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
                            default:
                                MessageBox.Show("شكل الإدخال يجب ان يكون مثل سنة أولى");
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

                    if (dataGridView2.Columns.Contains("yearText"))
                        dataGridView2.Columns.Remove("yearText");
                    DataGridViewComboBoxColumn comboYear = new DataGridViewComboBoxColumn();
                    comboYear.Name = "comboyear";
                    comboYear.HeaderText = "السنة";
                    comboYear.DataPropertyName = "yearText";
                    comboYear.Items.Add("سنة أولى");
                    comboYear.Items.Add("سنة ثانية");
                    comboYear.Items.Add("سنة ثالثة");
                    comboYear.Items.Add("سنة رابعة");
                    dataGridView2.Columns.Add(comboYear);

                    if (dataGridView2.Columns.Contains("ExamRoundText"))
                        dataGridView2.Columns.Remove("ExamRoundText");
                    DataGridViewComboBoxColumn comboRound = new DataGridViewComboBoxColumn();
                    comboRound.Name = "comboround";
                    comboRound.HeaderText = "الدور";
                    comboRound.DataPropertyName = "ExamRoundText";
                    comboRound.Items.Add("الأول");
                    comboRound.Items.Add("الثاني");
                    dataGridView2.Columns.Add(comboRound);

                    if (dataGridView2.Columns.Contains("GenderText"))
                        dataGridView2.Columns.Remove("GenderText");
                    DataGridViewComboBoxColumn comboGender = new DataGridViewComboBoxColumn();
                    comboGender.Name = "combogender";
                    comboGender.HeaderText = "الجنس";
                    comboGender.DataPropertyName = "GenderText";
                    comboGender.Items.Add("أنثى");
                    comboGender.Items.Add("ذكر");
                    dataGridView2.Columns.Add(comboGender);

                    if (dataGridView2.Columns.Contains("birth_date"))
                        dataGridView2.Columns.Remove("birth_date");

                    CalendarColumn columnDate = new CalendarColumn();
                    columnDate.HeaderText = "تاريخ الميلاد";
                    columnDate.Name = "columndate";
                    columnDate.DataPropertyName = "birth_date";
                    dataGridView2.Columns.Add(columnDate);

                    if (dataGridView2.Columns.Contains("dep_name"))
                        dataGridView2.Columns.Remove("dep_name");
                    DataGridViewComboBoxColumn columnDepartment = new DataGridViewComboBoxColumn();
                    columnDepartment.Name = "columndepartment";
                    columnDepartment.HeaderText = "القسم";
                    columnDepartment.DataPropertyName = "dep_name";
                    columnDepartment.Items.Add("قسم الرياضيات");
                    columnDepartment.Items.Add("قسم الكيمياء");
                    columnDepartment.Items.Add("قسم الفيزياء");
                    dataGridView2.Columns.Add(columnDepartment);

                    // باقي التنسيق
                    dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView2.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dataGridView2.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dataGridView2.Columns["full_name"].HeaderText = "الإسم";
                    dataGridView2.Columns["university_number"].HeaderText = "الرقم الجامعي";
                    dataGridView2.Columns["description"].HeaderText = "الحالة";
                    dataGridView2.Columns["description"].ReadOnly = true;
                    dataGridView2.Columns["student_id"].Visible = false;
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


        // جلب بيانات الطالب حسب رقم القيد و السماح بتعديلها وعرضها في الجدول 
        private void button2_Click(object sender, EventArgs e)
        {
            dataGridView2.DataSource = null;
            SearchStudent();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button2_Click(null, null);
                e.SuppressKeyPress = true;
            }
        }



        public void UpdateStudentFromGrid(DataGridView dataGridView)
        {
            if (dataGridView.Rows.Count == 0 || dataGridView.Rows[0].IsNewRow)
            {
                MessageBox.Show("لا توجد بيانات لتحديثها.");
                return;
            }

            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    DataGridViewRow row = dataGridView.Rows[0];

                    bool sturound = true;
                    bool stugender = true;
                    int currentYear = 1;

                    //اخد البيانات من الجدول لاضافتها الى متفيرات
                    if (row.Cells["combogender"].FormattedValue.ToString() == "أنثى")
                        stugender = false;
                    if (row.Cells["comboround"].FormattedValue.ToString() == "الأول")
                        sturound = false;
                    switch (row.Cells["comboyear"].FormattedValue.ToString())
                    {
                        case "سنة ثانية": currentYear = 2; break;
                        case "سنة ثالثة": currentYear = 3; break;
                        case "سنة رابعة": currentYear = 4; break;
                        default: currentYear = 1; break;
                    }
                    string universitynumber = row.Cells["university_number"].Value?.ToString() ?? "";
                    string depName = row.Cells["columndepartment"].FormattedValue.ToString();
                    string statusDescription = row.Cells["description"].FormattedValue.ToString();
                    string fullName = row.Cells["full_name"].Value?.ToString() ?? "";
                    string studentid = row.Cells["student_id"].Value?.ToString() ?? "";
                    string nationality = row.Cells["nationality"].Value?.ToString() ?? "";
                    DateTime birthDate;

                    if (!DateTime.TryParse(row.Cells["columndate"].FormattedValue?.ToString(), out birthDate))
                    {
                        MessageBox.Show("تاريخ الميلاد غير صالح.");
                        return;
                    }

                    // جلب معرف القسم
                    int departmentId = GetIdFromName("Departments", "department_id", "dep_name", depName, con);

                    // جلب معرف الحالة
                    int statusId = GetIdFromName("Status", "status_id", "description", statusDescription, con);

                    string updateQuery = @"
                        UPDATE Students SET
                            full_name = @full_name,
                            university_number = @university_number,
                            department_id = @department_id,
                            current_year = @current_year,
                            status_id = @status_id,
                            birth_date = @birth_date,
                            nationality = @nationality,
                            gender = @gender,
                            exam_round = @exam_round
                        WHERE student_id = @student_id";

                    //ادخال البارمترات الى الاستعلام
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@university_number", universitynumber);
                        cmd.Parameters.AddWithValue("@full_name", fullName);
                        cmd.Parameters.AddWithValue("@student_id", studentid);
                        cmd.Parameters.AddWithValue("@department_id", departmentId);
                        cmd.Parameters.AddWithValue("@current_year", currentYear);
                        cmd.Parameters.AddWithValue("@status_id", statusId);
                        cmd.Parameters.AddWithValue("@birth_date", birthDate);
                        cmd.Parameters.AddWithValue("@nationality", nationality);
                        cmd.Parameters.AddWithValue("@exam_round", sturound);
                        cmd.Parameters.AddWithValue("@gender", stugender);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        MessageBox.Show(rowsAffected > 0 ? "تم التحديث بنجاح." : "لم يتم التحديث.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }



        //جلب المعرف بالاسم
        private int GetIdFromName(string tableName, string idColumn, string nameColumn, string nameValue, SqlConnection con)
        {
            string query = $"SELECT {idColumn} FROM {tableName} WHERE {nameColumn} = @name";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@name", nameValue);
                object result = cmd.ExecuteScalar();
                if (result == null)
                {
                    throw new Exception($"لم يتم العثور على القيمة '{nameValue}' في الجدول {tableName}.");
                }
                return Convert.ToInt32(result);
            }
        }


        //زر حفظ التعديلات
        private void button4_Click(object sender, EventArgs e)
        {
            UpdateStudentFromGrid(dataGridView2);
            button2_Click(null, null);
        }
    }
}