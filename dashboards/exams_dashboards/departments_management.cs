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
        DataTable dt;
        SqlDataAdapter da;

        DataTable dtInstructors;
        public departments_management()
        {
            InitializeComponent();
            LoadInstructors();
            LoadYearComboBox();
            LoadTypeComboBox();
            InitializeControls();

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

                // الحصول على الـdepartment_id المحدد
                if (row.Cells["department_id"].Value != DBNull.Value)
                {
                    selectedDeptId = Convert.ToInt32(row.Cells["department_id"].Value);
                }
                else
                {
                    selectedDeptId = -1;
                }

                txtDeptName2.Text = row.Cells["dep_name"].Value?.ToString() ?? "";

                try
                {
                    con.Open();

                    // جلب كل الأساتذة بدون أي استثناء
                    SqlDataAdapter da = new SqlDataAdapter(
                        "SELECT instructor_id, full_name FROM Instructors", con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    comboBoxHead2.DataSource = dt;
                    comboBoxHead2.DisplayMember = "full_name";
                    comboBoxHead2.ValueMember = "instructor_id";

                    // تعيين الرئيس الحالي للقسم إن وجد
                    if (row.Cells["head_id"].Value != DBNull.Value)
                    {
                        comboBoxHead2.SelectedValue = row.Cells["head_id"].Value;
                    }
                    else
                    {
                        comboBoxHead2.SelectedIndex = -1; // لا يوجد رئيس حالياً
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
                        
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show("خطأ أثناء الحذف: " + ex.Message);
             
                }
                finally
                {
                    con.Close();
                }
                LoadCourses();
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

                foreach (DataGridViewRow row in dataGridView7.Rows)
                {
                    if (row.IsNewRow) continue; // تجاهل الصف الفارغ

                    int id = Convert.ToInt32(row.Cells["course_id"].Value);
                    string name = row.Cells["course_name"].Value?.ToString() ?? "";
                    int credit = Convert.ToInt32(row.Cells["credit_hrs"].Value ?? 0);
                    int year = Convert.ToInt32(row.Cells["year_number"].Value ?? 1);
                    string type = row.Cells["type"].Value?.ToString() ?? "";
                    int units = Convert.ToInt32(row.Cells["units"].Value ?? 0);
                    string code = row.Cells["course_code"].Value?.ToString() ?? "";

                    SqlCommand cmd = new SqlCommand(@"
            UPDATE Courses 
            SET course_name=@name, 
                credit_hrs=@credit, 
                year_number=@year, 
                type=@type, 
                units=@units, 
                course_code=@code 
            WHERE course_id=@id", con);

                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@credit", credit);
                    cmd.Parameters.AddWithValue("@year", year);
                    cmd.Parameters.AddWithValue("@type", type);
                    cmd.Parameters.AddWithValue("@units", units);
                    cmd.Parameters.AddWithValue("@code", code);
                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("تم تحديث جميع البيانات بنجاح!");
              
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء التحديث: " + ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
            LoadCourses();

        }
        //4 اداره القاعات
        private void button15_Click(object sender, EventArgs e)
        {
            string roomName = textBox7.Text.Trim();   // اسم القاعة
            string location = textBox8.Text.Trim();   // الموقع

            if (string.IsNullOrEmpty(roomName))
            {
                label26.Text = "يرجى إدخال اسم القاعة";
                label26.ForeColor = System.Drawing.Color.Red;
                return;
            }

            try
            {
                con.Open();

                // التحقق إذا الاسم موجود مسبقاً
                SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Classrooms WHERE room_name = @name", con);
                checkCmd.Parameters.AddWithValue("@name", roomName);

                int exists = (int)checkCmd.ExecuteScalar();

                if (exists > 0)
                {
                    label26.Text = "القاعة موجودة مسبقاً";
                    label26.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    // إضافة القاعة
                    SqlCommand insertCmd = new SqlCommand(
                        "INSERT INTO Classrooms (room_name, location) VALUES (@name, @location)", con);
                    insertCmd.Parameters.AddWithValue("@name", roomName);
                    insertCmd.Parameters.AddWithValue("@location", location);

                    int rows = insertCmd.ExecuteNonQuery();

                    if (rows > 0)
                    {
                        label26.Text = "تم الحفظ";
                        label26.ForeColor = System.Drawing.Color.Green;

                        // تفريغ الحقول
                        textBox7.Clear();
                        textBox8.Clear();
                    }
                    else
                    {
                        label26.Text = "فشل الحفظ";
                        label26.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
            catch (Exception ex)
            {
                label26.Text = "خطأ: " + ex.Message;
                label26.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                con.Close();
            }
        }
        //part2
        // 1️⃣ عرض القاعات
        private void showClassrooms()
        {
            try
            {
                con.Open();
                da = new SqlDataAdapter("SELECT classroom_id, room_name, location FROM Classrooms", con);
                SqlCommandBuilder cb = new SqlCommandBuilder(da);
                dt = new DataTable();
                da.Fill(dt);

                dataGridView5.DataSource = dt;

                // تعديل رؤوس الأعمدة للعربي
                dataGridView5.Columns["classroom_id"].HeaderText = "الرقم";
                dataGridView5.Columns["room_name"].HeaderText = "اسم القاعة";
                dataGridView5.Columns["location"].HeaderText = "الموقع";

                // إخفاء classroom_id لو تحب
                dataGridView5.Columns["classroom_id"].Visible = false;

                dataGridView5.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ: " + ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }
        private void button14_Click(object sender, EventArgs e)
        {
            showClassrooms();


        }
        // 2️⃣ تحديث التعديلات من DataGridView إلى قاعدة البيانات
        private void button12_Click(object sender, EventArgs e)
        {
            if (dataGridView5.Rows.Count == 0)
            {
                label10.Text = "لا توجد بيانات للتحديث";
                label10.ForeColor = Color.Red;
                return;
            }

            bool anySkipped = false; // متغير لتحديد إذا تم تخطي أي صف بسبب الاسم المكرر

            try
            {
                con.Open();

                foreach (DataGridViewRow row in dataGridView5.Rows)
                {
                    if (row.IsNewRow) continue;

                    int id = Convert.ToInt32(row.Cells["classroom_id"].Value);
                    string newName = row.Cells["room_name"].Value.ToString().Trim();
                    string location = row.Cells["location"].Value.ToString();

                    // التحقق إذا الاسم موجود مسبقاً في صف آخر
                    SqlCommand checkCmd = new SqlCommand(
                        "SELECT COUNT(*) FROM Classrooms WHERE room_name=@name AND classroom_id<>@id", con);
                    checkCmd.Parameters.AddWithValue("@name", newName);
                    checkCmd.Parameters.AddWithValue("@id", id);

                    int exists = (int)checkCmd.ExecuteScalar();

                    if (exists > 0)
                    {
                        anySkipped = true; // تم تخطي هذا الصف
                        continue; // تخطي هذا الصف وعدم تحديثه
                    }

                    // التحديث
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE Classrooms SET room_name=@name, location=@location WHERE classroom_id=@id", con);
                    cmd.Parameters.AddWithValue("@name", newName);
                    cmd.Parameters.AddWithValue("@location", location);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                // الرسالة النهائية
                if (anySkipped)
                {
                    label10.Text = "اسم القاعة موجود مسبقاً";
                    label10.ForeColor = Color.Red;
                }
                else
                {
                    label10.Text = "تم تحديث البيانات بنجاح";
                    label10.ForeColor = Color.Green;
                }
            }
            catch (Exception ex)
            {
                label10.Text = "خطأ: " + ex.Message;
                label10.ForeColor = Color.Red;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }

            // إعادة تحميل القاعات بعد التحديث
            showClassrooms();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (dataGridView5.SelectedCells.Count == 0)
            {
                label10.Text = "يرجى تحديد صف للحذف";
                label10.ForeColor = Color.Red;
                return;
            }

            int rowIndex = dataGridView5.SelectedCells[0].RowIndex;

            // تحقق من أن الصف ليس NewRow
            if (dataGridView5.Rows[rowIndex].IsNewRow)
            {
                label10.Text = "لا يمكن الحذف من الصف الفارغ";
                label10.ForeColor = Color.Red;
                return;
            }

            int classroomId = Convert.ToInt32(dataGridView5.Rows[rowIndex].Cells["classroom_id"].Value);

            try
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    // حذف من Course_Classroom
                    SqlCommand delCC = new SqlCommand("DELETE FROM Course_Classroom WHERE classroom_id=@id", con, tran);
                    delCC.Parameters.AddWithValue("@id", classroomId);
                    delCC.ExecuteNonQuery();

                    // حذف من Classrooms
                    SqlCommand delC = new SqlCommand("DELETE FROM Classrooms WHERE classroom_id=@id", con, tran);
                    delC.Parameters.AddWithValue("@id", classroomId);
                    int rows = delC.ExecuteNonQuery();

                    tran.Commit();

                    if (rows > 0)
                    {
                        label10.Text = "تم الحذف بنجاح";
                        label10.ForeColor = Color.Green;
                    }
                    else
                    {
                        label10.Text = "لم يتم العثور على القاعة";
                        label10.ForeColor = Color.Red;
                    }
                }
                catch (Exception ex1)
                {
                    tran.Rollback();
                    MessageBox.Show("خطأ أثناء الحذف: " + ex1.Message);
                }
            }
            catch (Exception ex2)
            {
                MessageBox.Show("خطأ: " + ex2.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }

            showClassrooms();
        }
        //ادارخ الدكاتره
        private void InitializeControls()
        {
            // تعبئة كومبو بوكس الدرجة العلمية
            comboBoxDegree.Items.Clear();
            comboBoxDegree.Items.Add("بكالوريوس");
            comboBoxDegree.Items.Add("ماجستير");
            comboBoxDegree.Items.Add("دكتوراه");
            comboBoxDegree.SelectedIndex = 0; // قيمة افتراضية

            // الراديو بوتون للنوع
            radioButtonMale.Checked = true; // افتراضي ذكر
            radioButtonFemale.Checked = false; // افتراضي أنثى غير محدد
        }
        private void button11_Click(object sender, EventArgs e)
        {
            string name = textBoxName.Text.Trim();
            string specialization = textBoxSpecialization.Text.Trim();
            string degree = comboBoxDegree.SelectedItem.ToString();
            DateTime birthDate = dateTimePickerBirth.Value;

            // تحديد الجنس من الراديو بوتون
            bool gender;
            if (radioButtonMale.Checked)
                gender = true; // ذكر
            else if (radioButtonFemale.Checked)
                gender = false; // أنثى
            else
            {
                label38.Text = "يرجى تحديد الجنس";
                label38.ForeColor = Color.Red;
                return;
            }

            if (string.IsNullOrEmpty(name))
            {
                label38.Text = "يرجى إدخال اسم المدرس";
                label38.ForeColor = Color.Red;
                return;
            }

            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Instructors (full_name, specialization, gender, birth_date, academic_degree) " +
                    "VALUES (@name, @spec, @gender, @birth, @degree)", con);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@spec", specialization);
                cmd.Parameters.AddWithValue("@gender", gender);
                cmd.Parameters.AddWithValue("@birth", birthDate);
                cmd.Parameters.AddWithValue("@degree", degree);

                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                {
                    label38.Text = "تم إضافة المدرس بنجاح";
                    label38.ForeColor = Color.Green;

                    textBoxName.Clear();
                    textBoxSpecialization.Clear();
                    comboBoxDegree.SelectedIndex = 0;
                    radioButtonMale.Checked = true; // إعادة التحديد الافتراضي للذكر
                }
                else
                {
                    label38.Text = "فشل إضافة المدرس";
                    label38.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ: " + ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }

        }
        // part2
        // 1️⃣ عرض جميع المدرسين
        // 1️⃣ عرض جميع المدرسين مع ترقيم وتغيير الهيدرز
        private void ShowAllInstructors()
        {
            try
            {
                con.Open();
                da = new SqlDataAdapter("SELECT * FROM Instructors", con);
                dtInstructors = new DataTable();
                da.Fill(dtInstructors);

                DataTable dtWithIndex = new DataTable();
                dtWithIndex.Columns.Add("الرقم", typeof(int));
                dtWithIndex.Columns.Add("الاسم", typeof(string));
                dtWithIndex.Columns.Add("التخصص", typeof(string));
                dtWithIndex.Columns.Add("الجنس", typeof(string));
                dtWithIndex.Columns.Add("تاريخ الميلاد", typeof(DateTime));
                dtWithIndex.Columns.Add("الدرجة العلمية", typeof(string));
                dtWithIndex.Columns.Add("instructor_id", typeof(int)); // مخفي

                int index = 1;
                foreach (DataRow row in dtInstructors.Rows)
                {
                    string genderStr = (bool)row["gender"] ? "ذكر" : "أنثى";
                    dtWithIndex.Rows.Add(index++, row["full_name"], row["specialization"], genderStr, row["birth_date"], row["academic_degree"], row["instructor_id"]);
                }

                dataGridViewInstructors.DataSource = dtWithIndex;
                dataGridViewInstructors.Columns["instructor_id"].Visible = false;
                dataGridViewInstructors.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridViewInstructors.Columns["الرقم"].Width = 50;
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ: " + ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            ShowAllInstructors();

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBoxSearchName.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                ShowAllInstructors();
                return;
            }

            try
            {
                con.Open();
                da = new SqlDataAdapter("SELECT * FROM Instructors WHERE full_name LIKE @name", con);
                da.SelectCommand.Parameters.AddWithValue("@name", "%" + searchText + "%");
                dtInstructors = new DataTable();
                da.Fill(dtInstructors);

                // إعادة نفس الترقيم والهيدرز
                DataTable dtWithIndex = new DataTable();
                dtWithIndex.Columns.Add("الرقم", typeof(int));
                dtWithIndex.Columns.Add("الاسم", typeof(string));
                dtWithIndex.Columns.Add("التخصص", typeof(string));
                dtWithIndex.Columns.Add("الجنس", typeof(string));
                dtWithIndex.Columns.Add("تاريخ الميلاد", typeof(DateTime));
                dtWithIndex.Columns.Add("الدرجة العلمية", typeof(string));
                dtWithIndex.Columns.Add("instructor_id", typeof(int)); // مخفي

                int index = 1;
                foreach (DataRow row in dtInstructors.Rows)
                {
                    string genderStr = (bool)row["gender"] ? "ذكر" : "أنثى";
                    dtWithIndex.Rows.Add(index++, row["full_name"], row["specialization"], genderStr, row["birth_date"], row["academic_degree"], row["instructor_id"]);
                }

                dataGridViewInstructors.DataSource = dtWithIndex;
                dataGridViewInstructors.Columns["instructor_id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ: " + ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (dataGridViewInstructors.Rows.Count == 0)
            {
                label47.Text = "لا توجد بيانات للتحديث";
                label47.ForeColor = Color.Red;
                return;
            }

            try
            {
                con.Open();
                foreach (DataGridViewRow row in dataGridViewInstructors.Rows)
                {
                    if (row.IsNewRow) continue;

                    int id = Convert.ToInt32(row.Cells["instructor_id"].Value);
                    string fullName = row.Cells["الاسم"].Value?.ToString() ?? "";
                    string specialization = row.Cells["التخصص"].Value?.ToString() ?? "";
                    string degree = row.Cells["الدرجة العلمية"].Value?.ToString() ?? "";
                    bool gender = row.Cells["الجنس"].Value?.ToString() == "ذكر";
                    DateTime birth = Convert.ToDateTime(row.Cells["تاريخ الميلاد"].Value);

                    SqlCommand cmd = new SqlCommand(
                        "UPDATE Instructors SET full_name=@name, specialization=@spec, gender=@gender, birth_date=@birth, academic_degree=@degree WHERE instructor_id=@id", con);
                    cmd.Parameters.AddWithValue("@name", fullName);
                    cmd.Parameters.AddWithValue("@spec", specialization);
                    cmd.Parameters.AddWithValue("@gender", gender);
                    cmd.Parameters.AddWithValue("@birth", birth);
                    cmd.Parameters.AddWithValue("@degree", degree);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                label47.Text = "تم تحديث بيانات المدرسين بنجاح";
                label47.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ: " + ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }

            ShowAllInstructors();
        }

        private void button9_Click(object sender, EventArgs e)
        {
       
    if (dataGridViewInstructors.SelectedCells.Count == 0)
            {
                label47.Text = "يرجى تحديد صف للحذف";
                label47.ForeColor = Color.Red;
                return;
            }

            int rowIndex = dataGridViewInstructors.SelectedCells[0].RowIndex;

            // تحقق من أن الصف ليس NewRow
            if (dataGridViewInstructors.Rows[rowIndex].IsNewRow)
            {
                label47.Text = "لا يمكن الحذف من الصف الفارغ";
                label47.ForeColor = Color.Red;
                return;
            }

            int instructorId = Convert.ToInt32(dataGridViewInstructors.Rows[rowIndex].Cells["instructor_id"].Value);

            try
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    // إزالة instructor_id من Course_Instructor للمواد التي يدرسها فقط
                    SqlCommand updateCourses = new SqlCommand(
                        "UPDATE Course_Instructor SET instructor_id=NULL WHERE instructor_id=@id", con, tran);
                    updateCourses.Parameters.AddWithValue("@id", instructorId);
                    updateCourses.ExecuteNonQuery();

                    // تحديث Departments إذا كان رئيس قسم
                    SqlCommand updateDepartments = new SqlCommand(
                        "UPDATE Departments SET head_id=NULL WHERE head_id=@id", con, tran);
                    updateDepartments.Parameters.AddWithValue("@id", instructorId);
                    updateDepartments.ExecuteNonQuery();

                    // حذف المدرس
                    SqlCommand delCmd = new SqlCommand(
                        "DELETE FROM Instructors WHERE instructor_id=@id", con, tran);
                    delCmd.Parameters.AddWithValue("@id", instructorId);
                    int rows = delCmd.ExecuteNonQuery();

                    tran.Commit();

                    if (rows > 0)
                    {
                        label47.Text = "تم الحذف بنجاح";
                        label47.ForeColor = Color.Green;
                    }
                    else
                    {
                        label47.Text = "لم يتم العثور على المدرس";
                        label47.ForeColor = Color.Red;
                    }
                }
                catch (Exception ex1)
                {
                    tran.Rollback();
                    label47.Text = "خطأ أثناء الحذف: " + ex1.Message;
                    label47.ForeColor = Color.Red;
                }
            }
            catch (Exception ex2)
            {
                MessageBox.Show("خطأ: " + ex2.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }

            // إعادة تحميل المدرسين بعد الحذف
            ShowAllInstructors();
        }
    }
}


