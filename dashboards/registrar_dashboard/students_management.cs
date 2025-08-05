using college_of_health_sciences.moduls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public bool checkTextBoxes(string elementText,TextBox element,bool error)
        {
            bool hasError = error;

            if (string.IsNullOrEmpty(elementText))
            {
                errorProvider1.SetError(element, "يرجى ملئ الحقل قبل الحفظ");
                hasError = true;
            }
            else
            {
                errorProvider1.SetError(element, "");
            }
            return hasError;
        }

        public bool checkComboBoxes(ComboBox element, bool error)
        {
            bool hasError = error;

            if (element.SelectedItem == null)
            {
                errorProvider1.SetError(element, "يرجى ملئ الحقل قبل الحفظ");
                hasError = true;
            }
            else
            {
                errorProvider1.SetError(element, "");
            }
            return hasError;
        }

        public bool CheckNullFields()
        {
            bool hasError = false;
            hasError = checkTextBoxes(textBox2.Text,textBox2,hasError);
            hasError = checkTextBoxes(textBox3.Text,textBox3, hasError);
            hasError = checkTextBoxes(textBox5.Text,textBox5, hasError);
            hasError = checkComboBoxes(comboBox3,hasError);
            hasError = checkComboBoxes(comboBox4,hasError);
            hasError = checkComboBoxes(comboBox5,hasError);

            if (!dateTimePicker1.Checked)
            {
                errorProvider1.SetError(dateTimePicker1, "يرجى ملئ الحقل قبل الحفظ");
                hasError = true;
            }
            else
            {
                errorProvider1.SetError(dateTimePicker1, "");
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

            return !hasError; // إذا لا يوجد خطأ -> true
        }


        //اضافة طالب جديد الى القاعدة في التاب الأول
        private void button7_Click(object sender, EventArgs e)
        {
            if (CheckNullFields())
            {
                string fullName = textBox2.Text.Trim();
                Regex regexName = new Regex(@"^[\p{L}\s]+$");
                if (!regexName.IsMatch(fullName))
                {
                    label1.ForeColor = Color.Red;
                    label1.Text = "⚠ الاسم يجب أن يحتوي على حروف فقط.!";
                    return;
                }

                string uni = textBox3.Text.Trim();
                if (!uni.All(char.IsDigit))
                {
                    label1.ForeColor = Color.Red;
                    label1.Text = "⚠ الرقم الجامعي يجب أن يحتوي على أرقام فقط.!";
                    return;
                }

                bool st_gender = false;
                if (radioButton1.Checked) st_gender = true;

                conn.DatabaseConnection db = new conn.DatabaseConnection();
                SqlConnection con = db.OpenConnection();

                // ✅ أولاً: التحقق من أن الرقم الجامعي غير موجود بالفعل
                string uniNum = textBox3.Text.Trim();
                SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Students WHERE university_number = @uni", con);
                checkCmd.Parameters.AddWithValue("@uni", uniNum);
                int exists = (int)checkCmd.ExecuteScalar();
                
                if (exists > 0)
                {
                    label1.ForeColor = Color.Red;
                    label1.Text = "⚠ الرقم الجامعي موجود مسبقًا!";
                    db.CloseConnection();
                    return; // وقف العملية
                }

                string q = "INSERT INTO Students (university_number, full_name, college, department_id, current_year, status_id, documents_path, gender, birth_date, nationality, exam_round) " +
                           "VALUES (@university_number, @full_name, N'كلية العلوم الصحية', @department_id, @current_year, @status_id, NULL, @gender, @birth_date, @nationality,N'دور أول')";

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

            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    string q = "select * from Departments";
                    SqlDataAdapter da = new SqlDataAdapter(q, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    comboBox3.DataSource = new BindingSource(dt, null);
                    comboBox3.DisplayMember = "dep_name";
                    comboBox3.ValueMember = "department_id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There is an Error : " + ex.Message);
            }


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

            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    string q = "select * from Status";
                    SqlDataAdapter da = new SqlDataAdapter(q, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    comboBox5.DataSource = new BindingSource(dt, null);
                    comboBox5.DisplayMember = "description";
                    comboBox5.ValueMember = "status_id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There is an Error : " + ex.Message);
            }
            
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

        public void datagridviewstyle(DataGridView datagrid)
        {
            datagrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            datagrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            datagrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void setColumnReadOnly(String column ,DataGridView dgrid)
        {
            dgrid.Columns[column].ReadOnly = true;
        }

        private void setColumnHeaderText(String column, DataGridView dgrid, string text)
        {
            dgrid.Columns[column].HeaderText = text;
        }
        public void setColumnComboBoxsyncwithDB(DataGridView data_grid, string removed_column, string newc_name, string newc_header, string property_name, string query, string displaymemper, string displayvalue)
        {
            // حذف العمود القديم إن وجد
            if (data_grid.Columns.Contains(removed_column))
                data_grid.Columns.Remove(removed_column);

            DataGridViewComboBoxColumn combo = new DataGridViewComboBoxColumn
            {
                Name = newc_name,
                HeaderText = newc_header,
                DataPropertyName = property_name
            };

            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    combo.DataSource = dt;
                    combo.DisplayMember = displaymemper;
                    combo.ValueMember = displayvalue;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There is an Error : " + ex.Message);
            }

            data_grid.Columns.Add(combo); // ✅ الإضافة في النهاية
        }


        public void setColumnComboBox(DataGridView data_grid, string removed_column, string newc_name, string newc_header, string property_name, List<string> arry)
        {
            if (data_grid.Columns.Contains(removed_column))
                data_grid.Columns.Remove(removed_column);
            DataGridViewComboBoxColumn combo = new DataGridViewComboBoxColumn
            {
                Name = newc_name,
                HeaderText = newc_header,
                DataPropertyName = property_name
            };
            if (arry != null && arry.Count > 0)
                combo.Items.AddRange(arry.ToArray());
            data_grid.Columns.Add(combo);
        }


        private void SearchStudent()
        {
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                conn.DatabaseConnection db2 = new conn.DatabaseConnection();
                SqlConnection con2 = db2.OpenConnection();

                string q2 = "SELECT s.student_id, s.university_number,s.full_name,d.dep_name AS dname,s.department_id,s.current_year,t.description,s.gender,s.birth_date,s.nationality,s.exam_round FROM Students s JOIN " +
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

                    dataGridView2.DataSource = dt;

                    // إخفاء الأعمدة الأصلية
                    dataGridView2.Columns["gender"].Visible = false;
                    dataGridView2.Columns["current_year"].Visible = false;


                    // عرض الأعمدة النصية بدلاً منها
                    dataGridView2.Columns["GenderText"].HeaderText = "الجنس";
                    dataGridView2.Columns["exam_round"].HeaderText = "الدور";
                    dataGridView2.Columns["yearText"].HeaderText = "السنة";

                    setColumnComboBox(dataGridView2, "yearText", "comboyear", "السنة", "yearText", new List<string> { "سنة أولى", "سنة ثانية", "سنة ثالثة", "سنة رابعة" });
                    setColumnComboBox(dataGridView2, "exam_round", "comboround", "الدور", "exam_round", new List<string> { "دور أول", "دور ثاني", "إعادة سنة" , "مرحل" });
                    setColumnComboBox(dataGridView2, "GenderText", "combogender", "الجنس", "GenderText", new List<string> { "أنثى", "ذكر" });
                    setColumnComboBoxsyncwithDB(dataGridView2, "department_id", "columndepartment", "القسم", "department_id","select * from Departments", "dep_name", "department_id");


                    if (dataGridView2.Columns.Contains("birth_date"))
                        dataGridView2.Columns.Remove("birth_date");
                    CalendarColumn columnDate = new CalendarColumn();
                    columnDate.HeaderText = "تاريخ الميلاد";
                    columnDate.Name = "columndate";
                    columnDate.DataPropertyName = "birth_date";
                    dataGridView2.Columns.Add(columnDate);

                    // باقي التنسيق
                    datagridviewstyle(dataGridView2);
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
            string studentidforcheck = dataGridView2.Rows[0].Cells["student_id"].Value.ToString();
 
            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    DataGridViewRow row = dataGridView.Rows[0];

                    
                    bool stugender = true;
                    int currentYear = 1;

                    //اخد البيانات من الجدول لاضافتها الى متفيرات
                    if (row.Cells["combogender"].FormattedValue.ToString() == "أنثى")
                        stugender = false;
                    switch (row.Cells["comboyear"].FormattedValue.ToString())
                    {
                        case "سنة ثانية": currentYear = 2; break;
                        case "سنة ثالثة": currentYear = 3; break;
                        case "سنة رابعة": currentYear = 4; break;
                        default: currentYear = 1; break;
                    }
                    string universitynumber = row.Cells["university_number"].Value?.ToString() ?? "";
                    string sturound = row.Cells["comboround"].FormattedValue.ToString();
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

                    // جلب القسم الحالي الموجود في قاعدة البيانات
                    SqlCommand getOldDeptCmd = new SqlCommand(
                        "SELECT department_id FROM Students WHERE student_id = @student_id", con);
                    getOldDeptCmd.Parameters.AddWithValue("@student_id", studentidforcheck);

                    int oldDeptId = Convert.ToInt32(getOldDeptCmd.ExecuteScalar());

                    // إذا اختلف القسم الجديد عن القديم → تحقق من وجود مواد
                    if (oldDeptId != departmentId)
                    {
                        SqlCommand regCheckCmd = new SqlCommand(
                            "SELECT COUNT(*) FROM Registrations WHERE student_id = @student_id", con);
                        regCheckCmd.Parameters.AddWithValue("@student_id", studentidforcheck);

                        int regCount = (int)regCheckCmd.ExecuteScalar();

                        if (regCount > 0)
                        {
                            MessageBox.Show("غير مسموح بتغيير قسم طالب مسجل بمواد.\nلحذف المواد أو تحويل الطالب، استخدم واجهة التحويل والترحيل.");
                            return;  // منع عملية التحديث
                        }
                    }


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

        private void button5_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("يجب إدخال الرقم الجامعي أولا");
                return;
            }
            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using(SqlConnection con = db.OpenConnection())
                {
                    string q = "SELECT " +
                        " s.university_number," +
                        "s.full_name," +
                        "d.dep_name," +
                        "s.current_year," +
                        "t.description," +
                        "s.gender," +
                        "s.birth_date," +
                        "s.nationality," +
                        "s.exam_round " +
                        "FROM Students s " +
                        "JOIN Departments d ON s.department_id = d.department_id " +
                        "JOIN Status t ON s.status_id = t.status_id " +
                        "WHERE university_number = @university_number";
                    using (SqlCommand cmd = new SqlCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@university_number",textBox1.Text.Trim());
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

                        dataGridView1.Columns["gender"].Visible = false;
                        dataGridView1.Columns["current_year"].Visible = false;

                        datagridviewstyle(dataGridView1);
                        setColumnReadOnly("university_number", dataGridView1);
                        setColumnReadOnly("full_name", dataGridView1);
                        setColumnReadOnly("dep_name", dataGridView1);
                        setColumnReadOnly("yearText", dataGridView1);
                        setColumnReadOnly("GenderText", dataGridView1);
                        setColumnReadOnly("birth_date", dataGridView1);
                        setColumnReadOnly("nationality", dataGridView1);
                        setColumnReadOnly("exam_round", dataGridView1);

                        setColumnHeaderText("university_number", dataGridView1, "الرقم الجامعي");
                        setColumnHeaderText("full_name", dataGridView1, "الإسم");
                        setColumnHeaderText("dep_name", dataGridView1, "القسم");
                        setColumnHeaderText("yearText", dataGridView1, "السنة");
                        setColumnHeaderText("GenderText", dataGridView1, "الجنس");
                        setColumnHeaderText("birth_date", dataGridView1, "تاريخ الميلاد");
                        setColumnHeaderText("nationality", dataGridView1, "الجنسية");
                        setColumnHeaderText("exam_round", dataGridView1, "الدور");

                        setColumnComboBox(dataGridView1, "description", "student_status", "الحالة الدراسية", "description", new List<string> { "مستمر", "مؤجل", "مستبعد", "خريج"});
                    }
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show("there is error : " + ex.Message);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button5_Click(null, null);
                e.SuppressKeyPress = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            UpdateStudentStatus(dataGridView1);
            button5_Click(null,null);
        }


        public void UpdateStudentStatus(DataGridView dataGridView)
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

                    string universitynumber = row.Cells["university_number"].Value?.ToString() ?? "";
                    string statusDescription = row.Cells["student_status"].FormattedValue.ToString();
                    int statusId = GetIdFromName("Status", "status_id", "description", statusDescription, con);
                    string updateQuery = @"
                        UPDATE Students SET
                            status_id = @status_id
                        WHERE university_number = @university_number";

                    //ادخال البارمترات الى الاستعلام
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@university_number", universitynumber);
                        cmd.Parameters.AddWithValue("@status_id", statusId);

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
    }
}