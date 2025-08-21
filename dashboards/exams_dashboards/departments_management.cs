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
    public partial class departments_management : UserControl
    {
        SqlConnection con = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=Cohs_DB;Integrated Security=True");
        private DataTable dtCourses = new DataTable();
        private DataRow selectedCourse = null;
        private bool isSelecting = false;
        public departments_management()
        {
            InitializeComponent();
            LoadInstructors();
            LoadYearComboBox();
            LoadTypeComboBox();

        }
        private void LoadInstructors()
        {
            try
            {
                string query = "SELECT instructor_id, full_name FROM Instructors";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                comboBoxHead.DataSource = dt;
                comboBoxHead.DisplayMember = "full_name";   // عرض اسم الأستاذ
                comboBoxHead.ValueMember = "instructor_id"; // القيمة هي ID
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في تحميل الأساتذة: " + ex.Message);
            }
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string deptName = txtDeptName.Text.Trim();
            int headId = Convert.ToInt32(comboBoxHead.SelectedValue);

            if (string.IsNullOrEmpty(deptName))
            {
                lblMessage.Text = "الرجاء إدخال اسم القسم";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            try
            {
                con.Open();

                // التحقق إذا القسم موجود مسبقًا
                SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Departments WHERE dep_name = @name", con);
                checkCmd.Parameters.AddWithValue("@name", deptName);

                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    lblMessage.Text = "القسم موجود مسبقًا!";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    // الحفظ
                    SqlCommand insertCmd = new SqlCommand("INSERT INTO Departments (dep_name, head_id) VALUES (@name, @head)", con);
                    insertCmd.Parameters.AddWithValue("@name", deptName);
                    insertCmd.Parameters.AddWithValue("@head", headId);

                    insertCmd.ExecuteNonQuery();

                    lblMessage.Text = "تم الحفظ بنجاح";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "خطأ: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                con.Close();
            }

        }
        //2
        private void LoadInstructors2()
        {
            try
            {
                string query = "SELECT instructor_id, full_name FROM Instructors";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                comboBoxHead2.DataSource = dt;
                comboBoxHead2.DisplayMember = "full_name";
                comboBoxHead2.ValueMember = "instructor_id";
                comboBoxHead2.SelectedIndex = -1; // مبدئياً فارغ
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في تحميل الأساتذة: " + ex.Message);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            LoadDepartments();
            LoadInstructors2();
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }
        private int selectedDeptId = -1; // نخزن ID القسم
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // تحقق من أن العمود ليس DBNull
                if (row.Cells["department_id"].Value != DBNull.Value)
                {
                    selectedDeptId = Convert.ToInt32(row.Cells["department_id"].Value);
                }
                else
                {
                    selectedDeptId = -1;
                }

                txtDeptName2.Text = row.Cells["dep_name"].Value?.ToString() ?? "";

                // تحميل رئيس القسم فقط في الكمبو بوكس
                if (row.Cells["head_id"].Value != DBNull.Value)
                {
                    try
                    {
                        con.Open();

                        // جلب جميع الأساتذة
                        SqlDataAdapter da = new SqlDataAdapter(
                            "SELECT instructor_id, full_name FROM Instructors", con);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        comboBoxHead2.DataSource = dt;
                        comboBoxHead2.DisplayMember = "full_name";
                        comboBoxHead2.ValueMember = "instructor_id";

                        // تعيين رئيس القسم كاختيار افتراضي
                        if (row.Cells["head_id"].Value != DBNull.Value)
                        {
                            comboBoxHead2.SelectedValue = row.Cells["head_id"].Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("خطأ في تحميل رئيس القسم: " + ex.Message);
                    }
                    finally
                    {
                        con.Close();
                    }

                }
                else
                {
                    comboBoxHead2.DataSource = null;
                }
            }

        }
        private void LoadDepartments()
        {
            try
            {
                string query = @"SELECT d.department_id, d.dep_name, i.full_name, d.head_id 
                         FROM Departments d
                         LEFT JOIN Instructors i ON d.head_id = i.instructor_id";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // إضافة عمود ترقيم
                dt.Columns.Add("رقم", typeof(int));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["رقم"] = i + 1;
                }

                dataGridView1.DataSource = dt;

                // ترتيب الأعمدة: الرقم أولاً
                dataGridView1.Columns["رقم"].DisplayIndex = 0;

                // تصغير عمود الرقم ومنعه من التمدد
                dataGridView1.Columns["رقم"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridView1.Columns["رقم"].Width = 30;
                dataGridView1.Columns["رقم"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView1.Columns["رقم"].HeaderText = "م";

                // إخفاء الأعمدة المساعدة
                dataGridView1.Columns["department_id"].Visible = false;
                dataGridView1.Columns["head_id"].Visible = false;

                dataGridView1.Columns["dep_name"].HeaderText = "اسم القسم";
                dataGridView1.Columns["full_name"].HeaderText = "رئيس القسم";

                // الأعمدة الأخرى تملأ العرض
                dataGridView1.Columns["dep_name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView1.Columns["full_name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في تحميل الأقسام: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (selectedDeptId == -1)
            {
                MessageBox.Show("الرجاء اختيار قسم للتعديل");
                return;
            }

            string deptName = txtDeptName2.Text.Trim();
            int headId = Convert.ToInt32(comboBoxHead2.SelectedValue);

            try
            {
                con.Open();

                // التحقق إذا القسم موجود مسبقًا باسم آخر غير هذا القسم
                SqlCommand checkCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Departments WHERE dep_name = @name AND department_id <> @id", con);
                checkCmd.Parameters.AddWithValue("@name", deptName);
                checkCmd.Parameters.AddWithValue("@id", selectedDeptId);

                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    MessageBox.Show("هذا القسم موجود مسبقًا، لا يمكن التعديل بنفس الاسم!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // تنفيذ التعديل إذا الاسم غير موجود
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Departments SET dep_name = @name, head_id = @head WHERE department_id = @id", con);
                cmd.Parameters.AddWithValue("@name", deptName);
                cmd.Parameters.AddWithValue("@head", headId);
                cmd.Parameters.AddWithValue("@id", selectedDeptId);

                cmd.ExecuteNonQuery();
                MessageBox.Show("تم التعديل بنجاح");

                LoadDepartments(); // إعادة تحميل
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ: " + ex.Message);
            }
            finally
            {
                con.Close();
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (selectedDeptId == -1)
            {
                MessageBox.Show("الرجاء اختيار قسم للحذف");
                return;
            }

            if (MessageBox.Show("هل أنت متأكد من الحذف؟", "تأكيد",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM Departments WHERE department_id = @id", con);
                    cmd.Parameters.AddWithValue("@id", selectedDeptId);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("تم الحذف بنجاح");

                    LoadDepartments(); // إعادة تحميل
                    txtDeptName2.Clear();
                    comboBoxHead2.SelectedIndex = -1;
                    selectedDeptId = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ: " + ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }

        }
        //المواد
        // دالة لملء ComboBox السنة
        private void LoadYearComboBox()
        {
            comboBoxYear.Items.Clear();
            for (int i = 1; i <= 4; i++)
            {
                comboBoxYear.Items.Add(i);
            }
            comboBoxYear.SelectedIndex = 0; // افتراضي السنة 1
        }
        private void LoadTypeComboBox()
        {
            comboBoxType.Items.Clear();
            comboBoxType.Items.Add("مشتركة");
            comboBoxType.Items.Add("غير مشتركة");
            comboBoxType.SelectedIndex = 0; // افتراضي النوع "مشتركة"
        }

        private void button23_Click(object sender, EventArgs e)
        {
            string courseName = txtCourseName.Text.Trim();
            string courseCode = txtCourseCode.Text.Trim();
            int yearNumber = Convert.ToInt32(comboBoxYear.SelectedItem);
            string type = comboBoxType.SelectedItem?.ToString();
            int theoryHours = (int)numericUpDownTheory.Value;
            int practicalHours = (int)numericUpDownPractical.Value;
            int units = (int)numericUpDownUnits.Value;

            if (string.IsNullOrEmpty(courseName) || string.IsNullOrEmpty(courseCode))
            {
                MessageBox.Show("الرجاء إدخال اسم المادة ورمزها");
                return;
            }

            try
            {
                con.Open();

                // التحقق من وجود المادة مسبقاً
                SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Courses WHERE course_code = @code", con);
                checkCmd.Parameters.AddWithValue("@code", courseCode);
                int count = (int)checkCmd.ExecuteScalar();
                if (count > 0)
                {
                    MessageBox.Show("رمز المادة موجود مسبقًا، لا يمكن إضافته مرة أخرى", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // إدخال المادة
                SqlCommand cmd = new SqlCommand(@"INSERT INTO Courses
            (course_name, course_code, theory_hours, practical_hours, credit_hrs, year_number, type, units)
            VALUES (@name, @code, @theory, @practical, @credit, @year, @type, @units)", con);

                cmd.Parameters.AddWithValue("@name", courseName);
                cmd.Parameters.AddWithValue("@code", courseCode);
                cmd.Parameters.AddWithValue("@theory", theoryHours);
                cmd.Parameters.AddWithValue("@practical", practicalHours);
                cmd.Parameters.AddWithValue("@credit", theoryHours + practicalHours); // يمكن تعديل طريقة الحساب
                cmd.Parameters.AddWithValue("@year", yearNumber);
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@units", units);

                cmd.ExecuteNonQuery();
                MessageBox.Show("تم إضافة المادة بنجاح");

                // إعادة تعيين القيم بعد الإضافة
                txtCourseName.Clear();
                txtCourseCode.Clear();
                comboBoxYear.SelectedIndex = 0;
                comboBoxType.SelectedIndex = 0;
                numericUpDownTheory.Value = 0;
                numericUpDownPractical.Value = 0;
                numericUpDownUnits.Value = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء الحفظ: " + ex.Message);
            }
            finally
            {
                con.Close();
            }


        }
        // افترض أن لديك DataTable باسم dtCourses


        // تحميل جميع المواد من قاعدة البيانات
        private void LoadCourses()
        {
            try
            {
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Courses", con);
                dtCourses.Clear();
                da.Fill(dtCourses);
                dataGridView7.DataSource = dtCourses;

                // ترجمة الهيدر إلى العربية
                dataGridView7.Columns["course_id"].HeaderText = "رقم المادة";
                dataGridView7.Columns["course_name"].HeaderText = "اسم المادة";
                dataGridView7.Columns["credit_hrs"].HeaderText = "الساعات المعتمدة";
                dataGridView7.Columns["year_number"].HeaderText = "السنة الدراسية";
                dataGridView7.Columns["type"].HeaderText = "نوع المادة";
                dataGridView7.Columns["units"].HeaderText = "الوحدات";
                dataGridView7.Columns["course_code"].HeaderText = "رمز المادة";

                // ضبط حجم DataGridView
                dataGridView7.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView7.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                dataGridView7.AllowUserToAddRows = false; // منع الصف الفارغ الأخير
                dataGridView7.EditMode = DataGridViewEditMode.EditOnEnter; // تعديل الخلايا مباشرة
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في تحميل المواد: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }


        private void button22_Click(object sender, EventArgs e)
        {
            LoadCourses();

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            string filter = textBox6.Text.Trim();
            if (string.IsNullOrEmpty(filter))
            {
                dataGridView7.DataSource = dtCourses;
            }
            else
            {
                var filteredRows = dtCourses.AsEnumerable()
                                    .Where(r => r.Field<string>("course_name").Contains(filter));
                dataGridView7.DataSource = filteredRows.Any() ? filteredRows.CopyToDataTable() : null;
            }


        }

        private void dataGridView7_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                isSelecting = true;
                selectedCourse = ((DataRowView)dataGridView7.Rows[e.RowIndex].DataBoundItem).Row;
                textBox6.Text = selectedCourse["course_name"].ToString();
                isSelecting = false;
            }


        }

        private void button21_Click(object sender, EventArgs e)
        {
            if (dataGridView7.CurrentRow != null && dataGridView7.CurrentRow.Index >= 0)
            {
                try
                {
                    if (MessageBox.Show("هل أنت متأكد من الحذف؟", "تأكيد",
             MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        int courseId = Convert.ToInt32(dataGridView7.CurrentRow.Cells["course_id"].Value);

                        con.Open();

                        // حذف من Course_Classroom
                        SqlCommand cmdClassroom = new SqlCommand(
                            "DELETE FROM Course_Classroom WHERE course_id=@id", con);
                        cmdClassroom.Parameters.AddWithValue("@id", courseId);
                        cmdClassroom.ExecuteNonQuery();

                        // حذف من Course_Department
                        SqlCommand cmdDepartment = new SqlCommand(
                            "DELETE FROM Course_Department WHERE course_id=@id", con);
                        cmdDepartment.Parameters.AddWithValue("@id", courseId);
                        cmdDepartment.ExecuteNonQuery();

                        // حذف من Course_Instructor
                        SqlCommand cmdInstructor = new SqlCommand(
                            "DELETE FROM Course_Instructor WHERE course_id=@id", con);
                        cmdInstructor.Parameters.AddWithValue("@id", courseId);
                        cmdInstructor.ExecuteNonQuery();

                        // حذف المادة نفسها من Courses
                        SqlCommand cmdMain = new SqlCommand(
                            "DELETE FROM Courses WHERE course_id=@id", con);
                        cmdMain.Parameters.AddWithValue("@id", courseId);
                        cmdMain.ExecuteNonQuery();

                        con.Close();

                        // حذف الصف من DataGridView
                        dtCourses.Rows[dataGridView7.CurrentRow.Index].Delete();

                        MessageBox.Show("تم حذف المادة وجميع البيانات المرتبطة بها بنجاح!");
                        LoadCourses();
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show("خطأ أثناء الحذف: " + ex.Message);
                    con.Close();
                }
            }
            else
            {
                MessageBox.Show("الرجاء تحديد مادة أولاً للحذف.");
            }


        }

        private void button20_Click(object sender, EventArgs e)
        {
            try
            {
                con.Open();

                foreach (DataRow row in dtCourses.Rows)
                {
                    if (row.IsNull("course_id") || row.IsNull("course_name")) continue;

                    string sql = @"UPDATE Courses 
                                   SET course_name=@name, 
                                       credit_hrs=@credit, 
                                       year_number=@year, 
                                       type=@type, 
                                       units=@units, 
                                       course_code=@code 
                                   WHERE course_id=@id";

                    SqlCommand cmd = new SqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@name", row["course_name"]);
                    cmd.Parameters.AddWithValue("@credit", row["credit_hrs"]);
                    cmd.Parameters.AddWithValue("@year", row["year_number"]);
                    cmd.Parameters.AddWithValue("@type", row["type"]);
                    cmd.Parameters.AddWithValue("@units", row["units"]);
                    cmd.Parameters.AddWithValue("@code", row["course_code"]);
                    cmd.Parameters.AddWithValue("@id", row["course_id"]);

                    cmd.ExecuteNonQuery();
                }

                con.Close();
                MessageBox.Show("تم تحديث جميع البيانات بنجاح!");
                LoadCourses();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء التحديث: " + ex.Message);
                con.Close();
            }

        }
    }

}
