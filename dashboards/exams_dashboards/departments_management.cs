using DocumentFormat.OpenXml.Bibliography;
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
        private int selectedCourseIdForRow = 0;   // لتخزين course_id للصف المحدد
        private int selectedCCId1 = 0;


        DataTable dtInstructors;
        public departments_management()
        {
            InitializeComponent();
            LoadInstructors();
            LoadYearComboBox();
            LoadTypeComboBox();
            InitializeControls();
            LoadDepartments1();
            LoadYears();
            LoadInstructors1();
            LoadYears1();
            dataGridViewDepartment.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewDepartment.MultiSelect = false;
        }
        public class ReservedPeriod
        {
            public TimeSpan Start { get; set; }
            public TimeSpan End { get; set; }
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
            string deptCode = textBox1.Text.Trim();
            int headId = Convert.ToInt32(comboBoxHead.SelectedValue);

            if (string.IsNullOrEmpty(deptName))
            {
                lblMessage.Text = "الرجاء إدخال اسم القسم";
                lblMessage.ForeColor = Color.Red;
                return;
            }

            try
            {
                con.Open();

                // التحقق إذا القسم موجود مسبقًا
                using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Departments WHERE dep_name = @name", con))
                {
                    checkCmd.Parameters.AddWithValue("@name", deptName);
                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        lblMessage.Text = "القسم موجود مسبقًا!";
                        lblMessage.ForeColor = Color.Red;
                        return;
                    }
                }

                // الحفظ
                using (SqlCommand insertCmd = new SqlCommand(
                    "INSERT INTO Departments (dep_name, head_id, department_code) VALUES (@name, @head, @d)", con))
                {
                    insertCmd.Parameters.AddWithValue("@name", deptName);
                    insertCmd.Parameters.AddWithValue("@head", headId);
                    insertCmd.Parameters.AddWithValue("@d", deptCode);

                    insertCmd.ExecuteNonQuery();
                }

                lblMessage.Text = "تم الحفظ بنجاح";
                lblMessage.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "خطأ: " + ex.Message;
                lblMessage.ForeColor = Color.Red;
            }
            finally
            {
                con.Close();
            }
            LoadDepartments();
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
                textBox2.Text = row.Cells["department_code"].Value?.ToString() ?? "";
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
                string query = @"SELECT d.department_id, d.dep_name, i.full_name, d.head_id , d.department_code
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
                dataGridView1.Columns["department_code"].HeaderText = "كود القسم";
                // الأعمدة الأخرى تملأ العرض
                dataGridView1.Columns["dep_name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView1.Columns["full_name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView1.Columns["department_code"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
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
            string deptCode = textBox2.Text.Trim(); // رمز القسم
            int headId = Convert.ToInt32(comboBoxHead2.SelectedValue);

            try
            {
                con.Open();

                // 🔹 جلب اسم القسم الحالي
                SqlCommand getNameCmd = new SqlCommand(
                    "SELECT dep_name FROM Departments WHERE department_id = @id", con);
                getNameCmd.Parameters.AddWithValue("@id", selectedDeptId);
                string currentDeptName = (string)getNameCmd.ExecuteScalar();

                // 🔹 إذا القسم الحالي اسمه "عام" → لا يسمح بتغيير الاسم
                if (currentDeptName == "عام" && deptName != "عام")
                {
                    MessageBox.Show("لا يمكن تغيير اسم القسم العام!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 🔹 التحقق إذا الاسم مكرر
                SqlCommand checkNameCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Departments WHERE dep_name = @name AND department_id <> @id", con);
                checkNameCmd.Parameters.AddWithValue("@name", deptName);
                checkNameCmd.Parameters.AddWithValue("@id", selectedDeptId);

                int nameCount = (int)checkNameCmd.ExecuteScalar();
                if (nameCount > 0)
                {
                    MessageBox.Show("هذا الاسم مستخدم مسبقًا، لا يمكن التعديل!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 🔹 التحقق إذا الرمز مكرر
                SqlCommand checkCodeCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Departments WHERE department_code = @code AND department_id <> @id", con);
                checkCodeCmd.Parameters.AddWithValue("@code", deptCode);
                checkCodeCmd.Parameters.AddWithValue("@id", selectedDeptId);

                int codeCount = (int)checkCodeCmd.ExecuteScalar();
                if (codeCount > 0)
                {
                    MessageBox.Show("رمز القسم مستخدم مسبقًا، لا يمكن التعديل!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 🔹 تنفيذ التعديل
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Departments SET dep_name = @name, department_code = @code, head_id = @head WHERE department_id = @id", con);
                cmd.Parameters.AddWithValue("@name", deptName);
                cmd.Parameters.AddWithValue("@code", deptCode);
                cmd.Parameters.AddWithValue("@head", headId);
                cmd.Parameters.AddWithValue("@id", selectedDeptId);

                cmd.ExecuteNonQuery();
                MessageBox.Show("تم التعديل بنجاح ✅");

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

            try
            {
                con.Open();

                // 🔹 جلب اسم القسم الحالي من قاعدة البيانات
                SqlCommand getNameCmd = new SqlCommand(
                    "SELECT dep_name FROM Departments WHERE department_id = @id", con);
                getNameCmd.Parameters.AddWithValue("@id", selectedDeptId);

                string currentDeptName = (string)getNameCmd.ExecuteScalar();

                // 🔹 إذا القسم اسمه "عام" → ممنوع الحذف
                if (currentDeptName == "عام")
                {
                    MessageBox.Show("⚠️ لا يمكن حذف القسم العام!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show("هل أنت متأكد من الحذف؟", "تأكيد",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM Departments WHERE department_id = @id", con);
                    cmd.Parameters.AddWithValue("@id", selectedDeptId);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("تم الحذف بنجاح");

                    LoadDepartments(); // إعادة تحميل
                    txtDeptName2.Clear();
                    comboBoxHead2.SelectedIndex = -1;
                    selectedDeptId = -1;
                }
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

        //المواد
        // دالة لملء ComboBox السنة
        private int selectedCourseId = 0;
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

            // التحقق من المدخلات
            if (string.IsNullOrEmpty(courseName) || string.IsNullOrEmpty(courseCode))
            {
                label48.Text = "⚠️ الرجاء إدخال اسم المادة ورمزها";
                label48.ForeColor = Color.Red;
                return;
            }

            try
            {
                con.Open();

                // التحقق من وجود المادة مسبقًا
                SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Courses WHERE course_code = @code", con);
                checkCmd.Parameters.AddWithValue("@code", courseCode);
                int count = (int)checkCmd.ExecuteScalar();
                if (count > 0)
                {
                    label48.Text = "⚠️ رمز المادة موجود مسبقًا، لا يمكن إضافته مرة أخرى";
                    label48.ForeColor = Color.Red;
                    return;
                }

                // إدخال المادة
                SqlCommand cmd = new SqlCommand(@"
        INSERT INTO Courses
        (course_name, course_code, theory_hours, practical_hours, credit_hrs, year_number, type, units)
        VALUES (@name, @code, @theory, @practical, @credit, @year, @type, @units)", con);

                cmd.Parameters.AddWithValue("@name", courseName);
                cmd.Parameters.AddWithValue("@code", courseCode);
                cmd.Parameters.AddWithValue("@theory", theoryHours);
                cmd.Parameters.AddWithValue("@practical", practicalHours);
                cmd.Parameters.AddWithValue("@credit", theoryHours + practicalHours);
                cmd.Parameters.AddWithValue("@year", yearNumber);
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@units", units);

                cmd.ExecuteNonQuery();

                label48.Text = "✅ تم إضافة المادة بنجاح";
                label48.ForeColor = Color.Green;

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
            // تحميل المواد لتحديث الجدول
            LoadCourses();


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
            if (e.RowIndex < 0 || e.RowIndex >= dataGridView7.Rows.Count)
                return;

            DataGridViewRow row = dataGridView7.Rows[e.RowIndex];

            // تعبئة الأدوات مع التحقق من DBNull
            txtCourseName.Text = row.Cells["course_name"].Value?.ToString() ?? "";
            txtCourseCode.Text = row.Cells["course_code"].Value?.ToString() ?? "";

            // السنة الدراسية
            if (row.Cells["year_number"].Value != DBNull.Value)
                comboBoxYear.SelectedItem = row.Cells["year_number"].Value.ToString();
            else
                comboBoxYear.SelectedIndex = 0;

            // النوع
            if (row.Cells["type"].Value != DBNull.Value)
                comboBoxType.SelectedItem = row.Cells["type"].Value.ToString();
            else
                comboBoxType.SelectedIndex = 0;

            // الساعات النظرية والعملية والوحدات
            numericUpDownTheory.Value = Convert.ToDecimal(row.Cells["credit_hrs"].Value ?? 0);
            numericUpDownPractical.Value = Convert.ToDecimal(row.Cells["practical_hours"].Value ?? 0);
            numericUpDownUnits.Value = Convert.ToDecimal(row.Cells["units"].Value ?? 0);

            // تخزين ID داخلي لتحديثه لاحقاً
            selectedCourseId = Convert.ToInt32(row.Cells["course_id"].Value);


        }

        private void button21_Click(object sender, EventArgs e)
        {
            // مثل زر التحديث: لازم يكون فيه عنصر مختار (selectedCourseId تم ضبطه من CellClick)
            if (selectedCourseId == 0)
            {
                MessageBox.Show("الرجاء اختيار مادة من الجدول أولاً.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("هل أنت متأكد من الحذف؟", "تأكيد الحذف",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                con.Open();
                using (SqlTransaction tran = con.BeginTransaction())
                {
                    using (SqlCommand cmd = new SqlCommand(@"
                DELETE FROM Course_Classroom  WHERE course_id = @id;
                DELETE FROM Course_Department WHERE course_id = @id;
                DELETE FROM Course_Instructor WHERE course_id = @id;
                DELETE FROM Courses           WHERE course_id = @id;", con, tran))
                    {
                        cmd.Parameters.AddWithValue("@id", selectedCourseId);
                        cmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                }


                label49.Text = "تم حذف المادة وجميع العلاقات المرتبطة بها بنجاح.";
                label49.ForeColor = Color.Red;
                // نفس سلوك زر التحديث بعد الإتمام
                selectedCourseId = 0;

                dataGridView7.ClearSelection();

            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء الحذف: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
            LoadCourses();


        }

        private void button20_Click(object sender, EventArgs e)
        {
            if (selectedCourseId == 0)
            {
                MessageBox.Show("الرجاء اختيار مادة لتعديلها.");
                return;
            }

            try
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(@"
            UPDATE Courses 
            SET course_name=@name, 
                course_code=@code,
                theory_hours=@theory,
                practical_hours=@practical,
                credit_hrs=@credit,
                year_number=@year, 
                type=@type, 
                units=@units 
            WHERE course_id=@id", con);

                cmd.Parameters.AddWithValue("@name", txtCourseName.Text.Trim());
                cmd.Parameters.AddWithValue("@code", txtCourseCode.Text.Trim());
                cmd.Parameters.AddWithValue("@theory", (int)numericUpDownTheory.Value);
                cmd.Parameters.AddWithValue("@practical", (int)numericUpDownPractical.Value);
                cmd.Parameters.AddWithValue("@credit", (int)(numericUpDownTheory.Value + numericUpDownPractical.Value));
                cmd.Parameters.AddWithValue("@year", Convert.ToInt32(comboBoxYear.SelectedItem));
                cmd.Parameters.AddWithValue("@type", comboBoxType.SelectedItem?.ToString());
                cmd.Parameters.AddWithValue("@units", (int)numericUpDownUnits.Value);
                cmd.Parameters.AddWithValue("@id", selectedCourseId);

                cmd.ExecuteNonQuery();

                label49.Text = "تم تعديل المادة بنجاح.";
                label49.ForeColor = Color.Green;


            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء التعديل: " + ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
            LoadCourses();


        }
        private void button25_Click(object sender, EventArgs e)
        {
            txtCourseName.Clear();
            txtCourseCode.Clear();
            comboBoxYear.SelectedIndex = 0;
            comboBoxType.SelectedIndex = 0;
            numericUpDownTheory.Value = 0;
            numericUpDownPractical.Value = 0;
            numericUpDownUnits.Value = 0;
            txtCourseCode.Focus();
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

            if (MessageBox.Show("هل أنت متأكد من الحذف؟", "تأكيد الحذف",
                          MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;
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
        private int hiddenInstructorId = 0;
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
                                               // تهيئة DataGridView
            InitializeDataGridView();
        }
        private void InitializeDataGridView()
        {
            dataGridViewInstructors.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewInstructors.MultiSelect = false;
            dataGridViewInstructors.ReadOnly = true;
            dataGridViewInstructors.ClearSelection();
            dataGridViewInstructors.CellClick += dataGridViewInstructors_CellClick;
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

                DataTable dtWithIndex = new DataTable();
                dtWithIndex.Columns.Add("الرقم", typeof(int));
                dtWithIndex.Columns.Add("الاسم", typeof(string));
                dtWithIndex.Columns.Add("التخصص", typeof(string));
                dtWithIndex.Columns.Add("الجنس", typeof(string));
                dtWithIndex.Columns.Add("تاريخ الميلاد", typeof(DateTime));
                dtWithIndex.Columns.Add("الدرجة العلمية", typeof(string));
                dtWithIndex.Columns.Add("instructor_id", typeof(int));

                int index = 1;
                foreach (DataRow row in dtInstructors.Rows)
                {
                    string genderStr = (bool)row["gender"] ? "ذكر" : "أنثى";
                    dtWithIndex.Rows.Add(index++, row["full_name"], row["specialization"], genderStr, row["birth_date"], row["academic_degree"], row["instructor_id"]);
                }

                dataGridViewInstructors.DataSource = dtWithIndex;
                dataGridViewInstructors.Columns["instructor_id"].Visible = false;
                dataGridViewInstructors.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                hiddenInstructorId = 0;
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
            if (hiddenInstructorId == 0)
            {
                label47.Text = "يرجى اختيار مدرس من القائمة أولاً";
                label47.ForeColor = Color.Red;
                return;
            }

            string name = textBoxName.Text.Trim();
            string specialization = textBoxSpecialization.Text.Trim();
            string degree = comboBoxDegree.SelectedItem?.ToString();
            bool gender = radioButtonMale.Checked;
            DateTime birthDate = dateTimePickerBirth.Value;

            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Instructors SET full_name=@name, specialization=@spec, gender=@gender, birth_date=@birth, academic_degree=@degree WHERE instructor_id=@id", con);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@spec", specialization);
                cmd.Parameters.AddWithValue("@gender", gender);
                cmd.Parameters.AddWithValue("@birth", birthDate);
                cmd.Parameters.AddWithValue("@degree", degree);
                cmd.Parameters.AddWithValue("@id", hiddenInstructorId);

                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                {
                    label47.Text = "تم تحديث المدرس بنجاح";
                    label47.ForeColor = Color.Green;

                }
                else
                {
                    label47.Text = "لم يتم التحديث";
                    label47.ForeColor = Color.Red;
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

            ShowAllInstructors();
        }

        private void button9_Click(object sender, EventArgs e)
        {

            if (hiddenInstructorId == 0)
            {
                label47.Text = "يرجى اختيار مدرس من القائمة أولاً";
                label47.ForeColor = Color.Red;
                return;
            }
            if (MessageBox.Show("هل أنت متأكد من الحذف؟", "تأكيد الحذف",
                          MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    // إزالة instructor_id من Course_Instructor
                    SqlCommand updateCourses = new SqlCommand(
                        "UPDATE Course_Instructor SET instructor_id=NULL WHERE instructor_id=@id", con, tran);
                    updateCourses.Parameters.AddWithValue("@id", hiddenInstructorId);
                    updateCourses.ExecuteNonQuery();

                    // تحديث Departments إذا كان رئيس قسم
                    SqlCommand updateDepartments = new SqlCommand(
                        "UPDATE Departments SET head_id=NULL WHERE head_id=@id", con, tran);
                    updateDepartments.Parameters.AddWithValue("@id", hiddenInstructorId);
                    updateDepartments.ExecuteNonQuery();

                    // حذف المدرس
                    SqlCommand delCmd = new SqlCommand(
                        "DELETE FROM Instructors WHERE instructor_id=@id", con, tran);
                    delCmd.Parameters.AddWithValue("@id", hiddenInstructorId);
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

                    hiddenInstructorId = 0;

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

        private void dataGridViewInstructors_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // تجاهل النقر على رؤوس الأعمدة أو الصف الجديد
            if (e.RowIndex < 0 || e.RowIndex >= dataGridViewInstructors.Rows.Count)
                return;

            DataGridViewRow row = dataGridViewInstructors.Rows[e.RowIndex];

            // تعبئة الأدوات مع التحقق من DBNull
            textBoxName.Text = row.Cells["الاسم"].Value?.ToString() ?? "";
            textBoxSpecialization.Text = row.Cells["التخصص"].Value?.ToString() ?? "";
            comboBoxDegree.SelectedItem = row.Cells["الدرجة العلمية"].Value?.ToString() ?? comboBoxDegree.Items[0].ToString();

            // التعامل مع الجنس
            if (row.Cells["الجنس"].Value != DBNull.Value && !string.IsNullOrEmpty(row.Cells["الجنس"].Value.ToString()))
            {
                bool gender = row.Cells["الجنس"].Value.ToString() == "ذكر";
                radioButtonMale.Checked = gender;
                radioButtonFemale.Checked = !gender;
            }
            else
            {
                // قيمة افتراضية
                radioButtonMale.Checked = true;
                radioButtonFemale.Checked = false;
            }

            // التعامل مع تاريخ الميلاد
            if (row.Cells["تاريخ الميلاد"].Value != DBNull.Value)
            {
                dateTimePickerBirth.Value = Convert.ToDateTime(row.Cells["تاريخ الميلاد"].Value);
            }
            else
            {
                dateTimePickerBirth.Value = DateTime.Today; // قيمة افتراضية
            }

            // التعامل مع ID
            if (row.Cells["instructor_id"].Value != DBNull.Value)
            {
                hiddenInstructorId = Convert.ToInt32(row.Cells["instructor_id"].Value);
            }
            else
            {
                hiddenInstructorId = 0;
            }

        }

        private void button24_Click(object sender, EventArgs e)
        {
            // إعادة تعيين TextBox و ComboBox و RadioButton
            textBoxName.Clear();
            textBoxSpecialization.Clear();
            comboBoxDegree.SelectedIndex = 0; // الدرجة العلمية الافتراضية
            radioButtonMale.Checked = true;   // الجنس الافتراضي ذكر
            radioButtonFemale.Checked = false;

            dateTimePickerBirth.Value = DateTime.Today; // تاريخ الميلاد الافتراضي

            hiddenInstructorId = 0; // إعادة ضبط ID للصف الجديد

            // مسح أي رسالة سابقة
            label38.Text = "";
            label47.Text = "";

            // إزالة أي تحديد من DataGridView
            dataGridViewInstructors.ClearSelection();
        }
        //ربط المواد بي القسم
        // تحميل السنوات في الكمبو الأول

        private void tabControl1_Click(object sender, EventArgs e)
        {
           
        }
        private void LoadYears()
        {

            if (comboBoxDepartment.Text == "عام") {
                comboBoxYear4.Items.Clear();
                for (int i = 1; i <= 1; i++) // حسب عدد سنوات الكلية
                    comboBoxYear4.Items.Add(i);
                if (comboBoxYear4.Items.Count > 0)
                { comboBoxYear4.SelectedIndex = 0; }// السنة الأولى افتراضي
            }
            else
            {
                comboBoxYear4.Items.Clear();
                for (int i = 2; i <= 4; i++) // حسب عدد سنوات الكلية
                    comboBoxYear4.Items.Add(i);
                if (comboBoxYear4.Items.Count > 0)
                { comboBoxYear4.SelectedIndex = 0; }// السنة الأولى افتراضي
            }
        }
        // تحميل الأقسام في الكمبو الثاني

        private void LoadDepartments1()
        {
            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                SqlDataAdapter da = new SqlDataAdapter("SELECT department_id, dep_name FROM Departments", con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                comboBoxDepartment.DataSource = dt;
                comboBoxDepartment.DisplayMember = "dep_name";
                comboBoxDepartment.ValueMember = "department_id";



            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }



        private void button7_Click(object sender, EventArgs e)
        {
            if (dataGridViewAvailable.CurrentRow == null || comboBoxDepartment.SelectedValue == null)
            {
                MessageBox.Show("⚠️ الرجاء اختيار مادة وقسم أولاً");
                return;
            }

            int courseId = Convert.ToInt32(dataGridViewAvailable.CurrentRow.Cells["course_id"].Value);
            int deptId = Convert.ToInt32(comboBoxDepartment.SelectedValue);

            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                SqlCommand cmd = new SqlCommand("INSERT INTO Course_Department(course_id, department_id) VALUES(@c, @d)", con);
                cmd.Parameters.AddWithValue("@c", courseId);
                cmd.Parameters.AddWithValue("@d", deptId);
                cmd.ExecuteNonQuery();


                label50.Text = "✅ تم ربط المادة بالقسم بنجاح.";
                label50.ForeColor = Color.Green;


            }
            catch (SqlException)
            {
                MessageBox.Show("⚠️ المادة مرتبطة بالفعل بهذا القسم.");
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }

            comboBoxYear4_SelectedIndexChanged(null, null);
            LoadDepartmentCourses();
        }

        private void comboBoxYear4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxYear4.SelectedItem == null)
                return;

            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                SqlCommand cmd = new SqlCommand(@"
           SELECT c.course_id, c.course_name AS [اسم المادة], c.course_code AS [رمز المادة]
   FROM Courses c
   WHERE c.year_number = @year
   AND (
       c.type = N'مشتركة'
       OR (
           c.type = N'غير مشتركة'
           AND NOT EXISTS (
               SELECT 1 FROM Course_Department cd WHERE cd.course_id = c.course_id
           )
       )
   )
", con);
                cmd.Parameters.AddWithValue("@year", comboBoxYear4.SelectedItem);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridViewAvailable.DataSource = dt;

                // إضافة عمود ترقيم
                if (!dataGridViewAvailable.Columns.Contains("ترقيم"))
                {
                    DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                    col.Name = "ترقيم";
                    col.HeaderText = "م";
                    col.Width = 50;
                    dataGridViewAvailable.Columns.Insert(0, col);
                }
                dataGridViewAvailable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                // تعبئة العمود بالترقيم
                for (int i = 0; i < dataGridViewAvailable.Rows.Count; i++)
                    dataGridViewAvailable.Rows[i].Cells["ترقيم"].Value = i + 1;

                // إخفاء العمود الأصلي
                if (dataGridViewAvailable.Columns.Contains("course_id"))
                    dataGridViewAvailable.Columns["course_id"].Visible = false;

                //if (dt.Rows.Count == 0)
                //    label50.Text = "⚠️ لا توجد مواد في هذه السنة.";
                //label50.ForeColor = Color.Red;

            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }
        private void LoadDepartmentCourses()
        {

            if (comboBoxDepartment.SelectedValue == null || comboBoxDepartment.SelectedValue is DataRowView)
                return;

            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                int deptId = Convert.ToInt32(comboBoxDepartment.SelectedValue);

                SqlCommand cmd = new SqlCommand(@"
            SELECT c.course_id, c.course_name AS [اسم المادة],cd.course_dep_code AS [رمز المادة]
            FROM Courses c
            INNER JOIN Course_Department cd ON cd.course_id = c.course_id
            WHERE cd.department_id = @deptId", con);
                cmd.Parameters.AddWithValue("@deptId", deptId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridViewDepartment.DataSource = dt;

                // إضافة عمود ترقيم
                if (!dataGridViewDepartment.Columns.Contains("ترقيم"))
                {
                    DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                    col.Name = "ترقيم";
                    col.HeaderText = "م";
                    col.Width = 50;
                    dataGridViewDepartment.Columns.Insert(0, col);
                }

                // تعبئة العمود بالترقيم
                for (int i = 0; i < dataGridViewDepartment.Rows.Count; i++)
                    dataGridViewDepartment.Rows[i].Cells["ترقيم"].Value = i + 1;

                // إخفاء العمود الأصلي
                if (dataGridViewDepartment.Columns.Contains("course_id"))
                    dataGridViewDepartment.Columns["course_id"].Visible = false;
                dataGridViewDepartment.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;


                if (dt.Rows.Count == 0)
                    label50.Text = "⚠️ لا توجد مواد في هذا القسم.";
                //  label50.ForeColor = Color.Red;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        private void comboBoxDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadYears();
            LoadDepartmentCourses();
        }



        private void button6_Click(object sender, EventArgs e)
        {  // الحصول على الصف الفعلي الذي ضغط عليه المستخدم
          
            if (dataGridViewDepartment.SelectedRows.Count == 0)
            {
                label50.Text = "⚠️ الرجاء اختيار مادة صحيحة للحذف.";
                label50.ForeColor = Color.Red;
                return;
            }
            if (MessageBox.Show("هل أنت متأكد من الحذف؟", "تأكيد الحذف",
                              MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;
            DataGridViewRow row = dataGridViewDepartment.SelectedRows[0];

            if (row.IsNewRow)
            {
                label50.Text = "⚠️ الرجاء اختيار مادة صحيحة للحذف.";
                label50.ForeColor = Color.Red;
                return;
            }

            int courseId = Convert.ToInt32(row.Cells["course_id"].Value);

            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                SqlCommand cmd = new SqlCommand("DELETE FROM Course_Department WHERE course_id = @c", con);
                cmd.Parameters.AddWithValue("@c", courseId);
                cmd.ExecuteNonQuery();

                label50.Text = "✅ تم حذف المادة من القسم بنجاح.";
                label50.ForeColor = Color.Green;

                LoadDepartmentCourses();
                comboBoxYear4_SelectedIndexChanged(null, null);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }

        }

        private void label50_Click(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {
        }
        //ادارة المحاضرات
        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox8.SelectedIndex < 0) return;

            int courseId = Convert.ToInt32(comboBox8.SelectedValue);

            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    string q = @"
SELECT i.instructor_id, i.full_name
FROM Instructors i
INNER JOIN Course_Instructor ci ON i.instructor_id = ci.instructor_id
WHERE ci.course_id = @course_id
";

                    SqlDataAdapter da = new SqlDataAdapter(q, con);
                    da.SelectCommand.Parameters.AddWithValue("@course_id", courseId);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    comboBox9.DataSource = dt;
                    comboBox9.DisplayMember = "full_name";
                    comboBox9.ValueMember = "instructor_id";
                    comboBox9.SelectedIndex = -1; // لتكون فارغة افتراضيًا
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في جلب الدكاترة: " + ex.Message);
            }

            if (comboBox8.SelectedValue == null || comboBox8.SelectedValue == DBNull.Value)
                return;

            try
            {
                conn.DatabaseConnection dbstudent = new conn.DatabaseConnection();
                using (SqlConnection con = dbstudent.OpenConnection())
                {
                    int month2;

                    using (SqlCommand cmddate = new SqlCommand("SELECT month_number FROM Months WHERE month_id = 1", con))
                    {
                        month2 = Convert.ToInt32(cmddate.ExecuteScalar());
                    }

                    string q = @"
                              SELECT COUNT(*) 
                              FROM Students s
                              WHERE s.status_id = 1
                                AND s.department_id = @department_id
                                AND s.current_year = @current_year - 1
                          ";


                    int academicYearStart = DateTime.Now.Month >= month2 ? DateTime.Now.Year : DateTime.Now.Year - 1;

                    using (SqlCommand cmddate2 = new SqlCommand(q, con))
                    {
                        int selectedCourse = Convert.ToInt32(comboBox6.SelectedValue);
                        cmddate2.Parameters.AddWithValue("@department_id", selectedCourse);
                        cmddate2.Parameters.AddWithValue("@current_year", Convert.ToInt32(comboBox7.SelectedValue));

                        // هنا لازم تستخدم ExecuteScalar مش ExecuteNonQuery عشان تسترجع العدد
                        int count = Convert.ToInt32(cmddate2.ExecuteScalar());
                        label53.Text = count.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message);
            }
        }



        private bool isLoading = false;
        private void departments_management_Load(object sender, EventArgs e)
        {
            isLoading = true;
            ddepartments();
            dyears();
            dgroups();
            ddays();
            dtimes();
            drooms();
            isLoading = false;
        }
        public void ddepartments()
        {
            try
            {
                conn.DatabaseConnection db2 = new conn.DatabaseConnection();
                using (SqlConnection con2 = db2.OpenConnection())
                {
                    string q2 = "select * from Departments";
                    SqlDataAdapter da2 = new SqlDataAdapter(q2, con2);
                    DataTable dt2 = new DataTable();
                    da2.Fill(dt2);

                    comboBox6.DataSource = dt2;
                    comboBox6.DisplayMember = "dep_name";
                    comboBox6.ValueMember = "department_id";
                    comboBox6.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There is an Error : " + ex.Message);
            }
        }

 
        public void dyears()
        {
            Dictionary<int, string> years;

            if (comboBox6.Text == "عام")
            {
                years = new Dictionary<int, string>() { { 1, "1" } };
            }
            else
            {
                years = new Dictionary<int, string>()
        {
            {2, "2"},
            {3, "3"},
            {4, "4"}
        };
            }

            comboBox7.DataSource = new BindingSource(years, null);
            comboBox7.DisplayMember = "Value";
            comboBox7.ValueMember = "Key";
            comboBox7.SelectedIndex = 0;
        }

        public void dgroups()
        {
            var group_num = new Dictionary<int, string>()
            {
                {1, "1"},
                {2, "2"},
                {3, "3"},
                {4, "4"},
                {5, "5"},
                {6, "6"},
                {7, "7"},
                {8, "8"},
                {9, "9"},
                {10, "10"},
                {11, "11"},
                {12, "12"},
                {13, "13"},
                {14, "14"},
                {15, "15"},
                {16, "16"},
                {17, "17"},
                {18, "18"}
            };
            comboBox10.DataSource = new BindingSource(group_num, null);
            comboBox10.DisplayMember = "Value";
            comboBox10.ValueMember = "Key";
        }

        public void dtimes()
        {
            comboBox12.Items.Clear();
            for (int hour = 0; hour < 24; hour++)
            {
                string timeText = new DateTime(1, 1, 1, hour, 0, 0).ToString("HH:mm");
                comboBox12.Items.Add(timeText);
            }

            comboBox12.SelectedIndex = -1;
            comboBox12.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox12.DrawItem += comboBox12_DrawItem;
        }


        // ==================== تعبئة الأيام ====================
        public void ddays()
        {
            var day = new Dictionary<int, string>()
    {
        {1, "الأحد"},
        {2, "الأثنين"},
        {3, "الثلاثاء"},
        {4, "الأربعاء"},
        {5, "الخميس"},
        {6, "الجمعة"},
        {7, "السبت"}
    };
            comboBox11.DataSource = new BindingSource(day, null);
            comboBox11.DisplayMember = "Value";
            comboBox11.ValueMember = "Key";
            comboBox11.SelectedIndex = -1;
        }

        // ==================== تعبئة القاعات ====================
        public void drooms()
        {
            try
            {
                conn.DatabaseConnection dbrooms1 = new conn.DatabaseConnection();
                using (SqlConnection conrooms1 = dbrooms1.OpenConnection())
                {
                    string qroom = "SELECT classroom_id, room_name FROM Classrooms";
                    SqlDataAdapter darooms = new SqlDataAdapter(qroom, conrooms1);
                    DataTable dtrooms = new DataTable();
                    darooms.Fill(dtrooms);

                    comboBox2.DataSource = dtrooms;
                    comboBox2.DisplayMember = "room_name";
                    comboBox2.ValueMember = "classroom_id";
                    comboBox2.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ: " + ex.Message);
            }
        }

        private void LoadCoursesInstractors()
        {
            if (comboBox6.SelectedValue == null || comboBox7.SelectedValue == null)
                return;

            try
            {
                int selectedDep = Convert.ToInt32(comboBox6.SelectedValue);
                int selectedYear = Convert.ToInt32(comboBox7.SelectedValue);

                conn.DatabaseConnection dbconnect = new conn.DatabaseConnection();
                using (SqlConnection con4 = dbconnect.OpenConnection())
                {
                    string q4 = @"
                SELECT c.course_name, c.course_id
                FROM Courses c
                JOIN Course_Department cd ON c.course_id = cd.course_id
                WHERE cd.department_id = @department_id 
                  AND c.year_number = @year_number
            ";

                    using (SqlCommand cmdconnect = new SqlCommand(q4, con4))
                    {
                        cmdconnect.Parameters.AddWithValue("@department_id", selectedDep);
                        cmdconnect.Parameters.AddWithValue("@year_number", selectedYear);

                        SqlDataAdapter daconn = new SqlDataAdapter(cmdconnect);
                        DataTable dtcon = new DataTable();
                        daconn.Fill(dtcon);

                        // 🔹 قم بإلغاء الحدث مؤقتًا لتجنب الحلقة
                        comboBox8.SelectedIndexChanged -= comboBox8_SelectedIndexChanged;

                        comboBox8.DataSource = dtcon;
                        comboBox8.DisplayMember = "course_name";
                        comboBox8.ValueMember = "course_id";
                        comboBox8.SelectedIndex = -1;

                        // 🔹 أعد تفعيل الحدث بعد ملء البيانات
                        comboBox8.SelectedIndexChanged += comboBox8_SelectedIndexChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("هناك خطأ: " + ex.Message);
            }
        }



        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            dyears();
            if (isLoading) return;
            LoadCoursesInstractors();
        }

        private void comboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            LoadCoursesInstractors();
        }



        public void datagridviewstyle(DataGridView datagrid)
        {
            datagrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            datagrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            datagrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        private void button18_Click(object sender, EventArgs e)
        {
            LoadCourseClassroom();
        }

        public void LoadCourseClassroom()
        {
            try
            {
                conn.DatabaseConnection dbshowcc = new conn.DatabaseConnection();

                using (SqlConnection concc = dbshowcc.OpenConnection()) // فتح الاتصال
                {
                 
                    string query = @"
SELECT 
    cc.id,
    c.course_id,
    c.year_number AS [السنة],
    c.course_name AS [اسم المادة],
    d.dep_name AS [القسم],
    cl.room_name AS [اسم القاعة],
    cc.group_number AS [المجموعة],
    cc.capacity AS [العدد],
    cc.start_time AS [وقت البداية],
    cc.end_time AS [وقت النهاية],
    CASE cc.lecture_day
        WHEN 1 THEN N'الأحد'
        WHEN 2 THEN N'الأثنين'
        WHEN 3 THEN N'الثلاثاء'
        WHEN 4 THEN N'الأربعاء'
        WHEN 5 THEN N'الخميس'
        WHEN 6 THEN N'الجمعة'
        WHEN 7 THEN N'السبت'
        ELSE N'غير محدد'
    END AS [اليوم],
    i.full_name AS [الدكتور]
FROM Course_Classroom cc
JOIN Courses c ON cc.course_id = c.course_id
JOIN Classrooms cl ON cc.classroom_id = cl.classroom_id
LEFT JOIN Instructors i ON i.instructor_id = cc.instructor_id
LEFT JOIN Course_Department cd ON cd.course_id = c.course_id
LEFT JOIN Departments d ON d.department_id = cd.department_id
";
                    if (dataGridView6.Columns["course_id"] != null)
                        dataGridView6.Columns["course_id"].Visible = false;



                    using (SqlCommand cmdcc = new SqlCommand(query, concc))
                    {
                        SqlDataAdapter dacc = new SqlDataAdapter(cmdcc);
                        DataTable dtcc = new DataTable();

                        dacc.Fill(dtcc); // تعبئة البيانات من الاستعلام

                        // إضافة عمود الترقيم في البداية
                        dtcc.Columns.Add("رقم", typeof(int)).SetOrdinal(0);

                        // تعبئة الترقيم
                        int counter = 1;
                        foreach (DataRow row in dtcc.Rows)
                        {
                            row["رقم"] = counter++;
                        }

                        // عرض النتيجة في DataGridView
                        dataGridView6.DataSource = dtcc;
                        datagridviewstyle(dataGridView6);

                        // إخفاء عمود id لأنه داخلي
                        if (dataGridView6.Columns["id"] != null)
                            dataGridView6.Columns["id"].Visible = false;

                        if (dataGridView6.Columns["course_id"] != null)
                            dataGridView6.Columns["course_id"].Visible = false;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected Error : " + ex.Message);
            }
        }

        int selectedCCId = -1;


        private void button17_Click(object sender, EventArgs e)
        {
            if (selectedCCId == -1)
            {
                MessageBox.Show("الرجاء اختيار سجل للحذف");
                return;
            }

            if (MessageBox.Show("هل أنت متأكد من الحذف؟", "تأكيد",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    conn.DatabaseConnection constring = new conn.DatabaseConnection();
                    using (SqlConnection condele = constring.OpenConnection())
                    {
                        using (SqlCommand cmddele = new SqlCommand("DELETE FROM Course_Classroom WHERE id = @id", condele))
                        {
                            cmddele.Parameters.AddWithValue("@id", selectedCCId);
                            int rowsAffected = cmddele.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("تم الحذف بنجاح");
                                LoadCourseClassroom(); // إعادة تحميل البيانات
                                selectedCCId = -1;     // إعادة التعيين
                            }
                            else
                            {
                                MessageBox.Show("لم يتم العثور على السجل المطلوب");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ: " + ex.Message);
                }
            }

        }
        private void ClearFields()
        {
            // إعادة تعيين الـ ComboBoxes
            comboBox8.Text = "";
            comboBox10.Text = "";
            comboBox11.Text = "";
            comboBox12.Text = "";
            comboBox9.Text = "";
            comboBox2.Text = "";
            comboBox6.Text = "";
            comboBox7.Text = "";

            // إعادة تعيين NumericUpDown
            numericUpDown1.Value = numericUpDown1.Minimum; // أو 0 حسب ما يناسبك

            // إعادة تعيين الصف المحدد
            selectedCCId = -1;

            // إزالة تحديد أي صف في DataGridView
            dataGridView6.ClearSelection();
        }

        private void dataGridView6_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView6.Rows[e.RowIndex];

                // حفظ الـ ID للصف المحدد
                object idValue = row.Cells["id"].Value;
                selectedCCId = (idValue != DBNull.Value) ? Convert.ToInt32(idValue) : 0;
                // تظليل الصف
                dataGridView6.Rows[e.RowIndex].Selected = true;
          
                // عرض البيانات في الحقول
                comboBox8.Text = row.Cells["اسم المادة"].Value.ToString();
                comboBox10.Text = row.Cells["المجموعة"].Value.ToString();
                comboBox11.Text = row.Cells["اليوم"].Value.ToString();
                comboBox12.Text = row.Cells["وقت البداية"].Value.ToString();
                comboBox9.Text = row.Cells["الدكتور"].Value.ToString();
                comboBox2.Text = row.Cells["اسم القاعة"].Value.ToString();
                comboBox6.Text = row.Cells["القسم"].Value.ToString();
                comboBox7.Text = row.Cells["السنة"].Value.ToString();
                object value = row.Cells["العدد"].Value;
                numericUpDown1.Value = (value != DBNull.Value) ? Convert.ToDecimal(value) : 0;
                UpdateReservedTimes();
             

            }
            if (e.RowIndex < 0) return;

            DataGridViewRow row1 = dataGridView6.Rows[e.RowIndex];

            // حفظ ID و course_id للصف المحدد
            selectedCCId1 = (row1.Cells["id"].Value != null && row1.Cells["id"].Value != DBNull.Value)
        ? Convert.ToInt32(row1.Cells["id"].Value)
        : 0;

            selectedCourseIdForRow = (row1.Cells["course_id"].Value != null && row1.Cells["course_id"].Value != DBNull.Value)
                ? Convert.ToInt32(row1.Cells["course_id"].Value)
                : 0;


            // تظليل الصف
            row1.Selected = true;

         
      
            UpdateStudentsCountLabel(selectedCCId1);

            // تعبئة ComboBox للمجموعات الأخرى لنفس المادة
            PopulateDestinationGroups(selectedCourseIdForRow, selectedCCId1);

        }

        private void button26_Click_1(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void button19_Click(object sender, EventArgs e)
        {
            try
            {
                conn.DatabaseConnection dbinsert = new conn.DatabaseConnection();
                using (SqlConnection conninsert = dbinsert.OpenConnection())
                {
                    // التحقق من اختيار الدكتور
                    if (comboBox9.SelectedValue == null)
                    {
                        MessageBox.Show("اختر الدكتور!");
                        return;
                    }
                    int instructorIdIn = Convert.ToInt32(comboBox9.SelectedValue);

                    // التحقق من اختيار القسم
                    if (comboBox6.SelectedValue == null)
                    {
                        MessageBox.Show("اختر القسم!");
                        return;
                    }
                    int depID = Convert.ToInt32(comboBox6.SelectedValue);

                    // التحقق من اختيار المادة
                    if (comboBox8.SelectedValue == null)
                    {
                        MessageBox.Show("اختر المادة!");
                        return;
                    }
                    int courseIdIn = Convert.ToInt32(comboBox8.SelectedValue);

                    // التحقق من اختيار القاعة
                    DataRowView drvRoom = comboBox2.SelectedItem as DataRowView;
                    if (drvRoom == null)
                    {
                        MessageBox.Show("اختر القاعة!");
                        return;
                    }
                    int classroomIdIn = Convert.ToInt32(drvRoom["classroom_id"]);

                    // التحقق من اختيار اليوم
                    if (comboBox11.SelectedItem == null)
                    {
                        MessageBox.Show("اختر اليوم!");
                        return;
                    }
                    var selectedDay = (KeyValuePair<int, string>)comboBox11.SelectedItem;
                    int lectureDayIn = selectedDay.Key;

                    // التحقق من اختيار المجموعة
                    if (comboBox10.SelectedValue == null)
                    {
                        MessageBox.Show("اختر المجموعة!");
                        return;
                    }
                    int groupNumIn = Convert.ToInt32(comboBox10.SelectedValue);

                    // التحقق من قيمة العدد (Capacity)
                    if (numericUpDown1.Value <= 0)
                    {
                        MessageBox.Show("حدد العدد بشكل صحيح!");
                        return;
                    }
                    int capacityIn = Convert.ToInt32(numericUpDown1.Value);

                    if (numericUpDown2.Value <= 0)
                    {
                        MessageBox.Show("حدد المدة بشكل صحيح!");
                        return;
                    }
                    int duration = Convert.ToInt32(numericUpDown2.Value);

                    // التحقق من اختيار وقت البداية
                    if (comboBox12.SelectedItem == null)
                    {
                        MessageBox.Show("اختر وقت البداية!");
                        return;
                    }
                    TimeSpan start = TimeSpan.Parse(comboBox12.SelectedItem.ToString());

                    // جلب عدد الوحدات من جدول Courses
                    int courseUnits = duration;

                    TimeSpan end = start.Add(TimeSpan.FromHours(courseUnits));

                    // 1️⃣ تحقق من تعارض القاعة (نفس اليوم + نفس القاعة)
                    string qCheckRoom = @"
                                       SELECT COUNT(*) 
                                       FROM Course_Classroom
                                       WHERE classroom_id = @classroom_id
                                         AND lecture_day = @lecture_day
                                         AND (
                                               (@start_time >= start_time AND @start_time < end_time) OR
                                               (@end_time > start_time AND @end_time <= end_time) OR
                                               (@start_time <= start_time AND @end_time >= end_time)
                                             )
                                       ";

                    using (SqlCommand cmdRoom = new SqlCommand(qCheckRoom, conninsert))
                    {
                        cmdRoom.Parameters.AddWithValue("@classroom_id", classroomIdIn);
                        cmdRoom.Parameters.AddWithValue("@lecture_day", lectureDayIn);
                        cmdRoom.Parameters.AddWithValue("@start_time", start);
                        cmdRoom.Parameters.AddWithValue("@end_time", end);

                        int existsRoom = (int)cmdRoom.ExecuteScalar();
                        if (existsRoom > 0)
                        {
                            MessageBox.Show("❌ القاعة محجوزة في هذا الوقت.");
                            return;
                        }
                    }

                    // 2️⃣ تحقق من تعارض الدكتور (نفس اليوم + أي قاعة) مع تفاصيل
                    string qCheckInstructor = @"
                                            SELECT TOP 1 
                                                c.course_name,
                                                cl.room_name,
                                                cc.start_time,
                                                cc.end_time
                                            FROM Course_Classroom cc
                                            JOIN Courses c ON cc.course_id = c.course_id
                                            JOIN Classrooms cl ON cc.classroom_id = cl.classroom_id
                                            WHERE cc.instructor_id = @instructor_id
                                              AND cc.lecture_day = @lecture_day
                                              AND (
                                                    (@start_time >= cc.start_time AND @start_time < cc.end_time) OR
                                                    (@end_time > cc.start_time AND @end_time <= cc.end_time) OR
                                                    (@start_time <= cc.start_time AND @end_time >= cc.end_time)
                                                  )
                                            ";

                    using (SqlCommand cmdInstructor = new SqlCommand(qCheckInstructor, conninsert))
                    {
                        cmdInstructor.Parameters.AddWithValue("@instructor_id", instructorIdIn);
                        cmdInstructor.Parameters.AddWithValue("@lecture_day", lectureDayIn);
                        cmdInstructor.Parameters.AddWithValue("@start_time", start);
                        cmdInstructor.Parameters.AddWithValue("@end_time", end);

                        using (SqlDataReader reader = cmdInstructor.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string courseName = reader["course_name"].ToString();
                                string roomName = reader["room_name"].ToString();
                                string startTime = reader["start_time"].ToString();
                                string endTime = reader["end_time"].ToString();

                                MessageBox.Show(
                                    $"❌ الدكتور لديه محاضرة أخرى في هذا الوقت:\n" +
                                    $"المادة: {courseName}\n" +
                                    $"القاعة: {roomName}\n" +
                                    $"من {startTime} إلى {endTime}"
                                );
                                return;
                            }
                        }
                    }


                    // 3️⃣ تحقق من المجموعة (نفس المادة + نفس المجموعة)
                    string qCheckGroup = @"
                                         SELECT COUNT(*) 
                                         FROM Course_Classroom
                                         WHERE course_id = @course_id
                                           AND group_number = @group_number
                                           AND department_id = @depid
                                         ";

                    using (SqlCommand cmdGroup = new SqlCommand(qCheckGroup, conninsert))
                    {
                        cmdGroup.Parameters.AddWithValue("@course_id", courseIdIn);
                        cmdGroup.Parameters.AddWithValue("@group_number", groupNumIn);
                        cmdGroup.Parameters.AddWithValue("@depid", depID);

                        int existsGroup = (int)cmdGroup.ExecuteScalar();
                        if (existsGroup > 0)
                        {
                            MessageBox.Show("❌ هذه المجموعة موجودة مسبقًا لهذه المادة_القسم، اختر مجموعة جديدة.");
                            return;
                        }
                    }


                    // 3️⃣ إدخال المحاضرة
                    string qinsert = @"
                    INSERT INTO Course_Classroom 
                    (course_id, classroom_id, group_number, capacity, start_time, end_time, lecture_day,instructor_id,department_id)
                    VALUES
                    (@course_id, @classroom_id, @group_number, @capacity, @start_time, @end_time, @lecture_day,@instructor_id,@depid)
                    ";
                    using (SqlCommand cmdInsert = new SqlCommand(qinsert, conninsert))
                    {
                        cmdInsert.Parameters.AddWithValue("@instructor_id", instructorIdIn);
                        cmdInsert.Parameters.AddWithValue("@course_id", courseIdIn);
                        cmdInsert.Parameters.AddWithValue("@classroom_id", classroomIdIn);
                        cmdInsert.Parameters.AddWithValue("@group_number", groupNumIn);
                        cmdInsert.Parameters.AddWithValue("@capacity", capacityIn);
                        cmdInsert.Parameters.AddWithValue("@start_time", start);
                        cmdInsert.Parameters.AddWithValue("@end_time", end);
                        cmdInsert.Parameters.AddWithValue("@lecture_day", lectureDayIn);
                        cmdInsert.Parameters.AddWithValue("@depid", depID);

                        cmdInsert.ExecuteNonQuery();
                    }

                    MessageBox.Show("✅ تم إضافة المحاضرة بنجاح!");
                    ClearFields();
                    LoadCourseClassroom();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private List<(TimeSpan Start, TimeSpan End, string CourseName, string RoomName, string InstructorName)> reservedTimes = new List<(TimeSpan, TimeSpan, string, string, string)>();
        private void LoadReservedTimes(int classroomId, int lectureDay, int ignoreCCId = 0)
        {
            reservedTimes.Clear();

            using (SqlConnection con = new conn.DatabaseConnection().OpenConnection())
            {
                string q = @"
    SELECT cc.start_time, cc.end_time, c.course_name, cl.room_name, i.full_name
    FROM Course_Classroom cc
    JOIN Courses c ON cc.course_id = c.course_id
    JOIN Classrooms cl ON cc.classroom_id = cl.classroom_id
    LEFT JOIN Instructors i ON i.instructor_id = cc.instructor_id
    WHERE cc.classroom_id = @classroom_id
      AND cc.lecture_day = @lecture_day";

                // إذا كان ignoreCCId > 0 الشرط لتجاهل الصف الحالي
                if (ignoreCCId > 0)
                    q += " AND cc.id <> @cc_id";

                using (SqlCommand cmd = new SqlCommand(q, con))
                {
                    cmd.Parameters.AddWithValue("@classroom_id", classroomId);
                    cmd.Parameters.AddWithValue("@lecture_day", lectureDay);

                    if (ignoreCCId > 0)
                        cmd.Parameters.AddWithValue("@cc_id", ignoreCCId);

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            TimeSpan start = (TimeSpan)rdr["start_time"];
                            TimeSpan end = (TimeSpan)rdr["end_time"];
                            string courseName = rdr["course_name"].ToString();
                            string roomName = rdr["room_name"].ToString();
                            string instructorName = rdr["full_name"]?.ToString() ?? "";

                            reservedTimes.Add((start, end, courseName, roomName, instructorName));
                        }
                    }
                }
            }

            comboBox12.Invalidate(); // إعادة رسم لتوضيح الأوقات المحجوزة
        }








        // ==================== رسم ComboBox مع تظليل الأوقات المحجوزة ====================
        private void comboBox12_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            ComboBox cb = sender as ComboBox;
            string timeText = cb.Items[e.Index].ToString();
            TimeSpan itemTime = TimeSpan.Parse(timeText);

            int lectureDurationHours = (int)numericUpDown2.Value;
            
            // حساب وقت النهاية مع دوران 24 ساعة
            TimeSpan duration = TimeSpan.FromHours(lectureDurationHours);
            TimeSpan endTime = TimeSpan.FromHours((itemTime.TotalHours + duration.TotalHours) % 24);

            bool isReserved = reservedTimes.Any(r =>
            {
                if (r.Start < r.End) // محاضرة داخل اليوم
                    return itemTime < r.End && endTime > r.Start;
                else // محاضرة تمتد بعد منتصف الليل
                    return (itemTime < r.End || endTime > r.Start);
            });

            e.DrawBackground();
            using (Brush brush = new SolidBrush(isReserved ? Color.Red : e.ForeColor))
            {
                e.Graphics.DrawString(timeText, e.Font, brush, e.Bounds);
            }
            e.DrawFocusRectangle();
        }




        // ==================== تحقق عند اختيار وقت البداية ====================
        private void comboBox12_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox12.SelectedIndex < 0) return;

            TimeSpan startTime = TimeSpan.Parse(comboBox12.SelectedItem.ToString());

            int lectureDurationHours = 0;
            if (comboBox8.SelectedValue != null)
            {
                int courseId = Convert.ToInt32(comboBox8.SelectedValue);
                using (SqlConnection con = new conn.DatabaseConnection().OpenConnection())
                {
                    string qUnits = "SELECT units FROM Courses WHERE course_id = @course_id";
                    using (SqlCommand cmd = new SqlCommand(qUnits, con))
                    {
                        cmd.Parameters.AddWithValue("@course_id", courseId);
                        object result = cmd.ExecuteScalar();
                        if (result != null) lectureDurationHours = Convert.ToInt32(result);
                    }
                }
            }
            TimeSpan duration = TimeSpan.FromHours(lectureDurationHours);
            TimeSpan endTime = TimeSpan.FromHours((startTime.TotalHours + duration.TotalHours) % 24);


            var conflict = reservedTimes.FirstOrDefault(r =>
            {
                if (r.Start < r.End)
                    return startTime < r.End && endTime > r.Start;
                else
                    return (startTime < r.End || endTime > r.Start);
            });

            if (conflict != default)
            {
                MessageBox.Show($"⚠ الفترة المختارة تتعارض مع محاضرة:\nالمادة: {conflict.CourseName}\nالقاعة: {conflict.RoomName}\nالدكتور: {conflict.InstructorName}");
                comboBox12.SelectedIndex = -1;
            }
        }

        private void UpdateReservedTimes()
        {
            if (comboBox2.SelectedIndex >= 0 && comboBox11.SelectedIndex >= 0)
            {
                DataRowView drvRoom = comboBox2.SelectedItem as DataRowView;
                int classroomId = Convert.ToInt32(drvRoom["classroom_id"]);

                var selectedDay = (KeyValuePair<int, string>)comboBox11.SelectedItem;
                int lectureDay = selectedDay.Key;

                if (selectedCCId > 0)
                    LoadReservedTimes(classroomId, lectureDay, selectedCCId); // تعديل: تجاهل المحاضرة الحالية
                else
                    LoadReservedTimes(classroomId, lectureDay); // إضافة: أخذ كل الأوقات المحجوزة

                comboBox12.Invalidate(); // إعادة الرسم لتوضيح الأوقات المحجوزة
            }
        }




        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox11_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateReservedTimes();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateReservedTimes();
        }
        //************************
        // ==================== كود النقل المستقل ====================

        // دالة لإظهار/إخفاء عناصر النقل
     

        private void PopulateDestinationGroups(int courseId, int currentCcId)
        {
            // إزالة أي بيانات قديمة إذا كان courseId = 0
            if (courseId == 0)
            {
                comboBox1.DataSource = null;
                button4.Enabled = false;
                return;
            }

            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection con = new conn.DatabaseConnection().OpenConnection())
                using (SqlCommand cmd = new SqlCommand(@"
            SELECT 
                cc.id,
                N'مجموعة ' + CAST(cc.group_number AS NVARCHAR(10)) AS DisplayText
            FROM Course_Classroom cc
            WHERE cc.course_id = @course_id
              AND (@current_cc_id = 0 OR cc.id <> @current_cc_id)
            ORDER BY cc.group_number
        ", con))
                {
                    cmd.Parameters.AddWithValue("@course_id", courseId);
                    cmd.Parameters.AddWithValue("@current_cc_id", currentCcId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                // تعطيل الحدث مؤقتاً لتجنب التكرار
                comboBox1.SelectedIndexChanged -= comboBox1_SelectedIndexChanged;

                // إعداد ComboBox
                comboBox1.DataSource = dt;
                comboBox1.DisplayMember = "DisplayText";
                comboBox1.ValueMember = "id";

                comboBox1.DrawMode = DrawMode.Normal;
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBox1.BackColor = SystemColors.Window;
                comboBox1.ForeColor = SystemColors.WindowText;
                comboBox1.Font = new Font("Segoe UI", 11, FontStyle.Bold);

                // اختيار أول عنصر تلقائياً إذا كان موجود
                if (dt.Rows.Count > 0)
                    comboBox1.SelectedIndex = 0;
                else
                    comboBox1.SelectedIndex = -1;

                // إعادة تفعيل الحدث
                comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;

                // تفعيل زر النقل إذا كانت هناك عناصر
                button4.Enabled = dt.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في جلب المجموعات: " + ex.Message);
                comboBox1.DataSource = null;
                button4.Enabled = false;
            }
        }





        private void UpdateStudentsCountLabel(int ccId)
        {
            using (SqlConnection con = new conn.DatabaseConnection().OpenConnection())
            {
                int latestAcademicYear = 0;

                // أحدث عام جامعي للمجموعة
                using (SqlCommand cmdMaxYear = new SqlCommand(@"
            SELECT ISNULL(MAX(academic_year_start), 0)
            FROM Registrations
            WHERE course_classroom_id = @cc_id
        ", con))
                {
                    cmdMaxYear.Parameters.AddWithValue("@cc_id", ccId);
                    latestAcademicYear = Convert.ToInt32(cmdMaxYear.ExecuteScalar());
                }

                if (latestAcademicYear == 0)
                {
                    label63.Text = "0";
                    return;
                }

                // عدد الطلاب
                using (SqlCommand cmdCount = new SqlCommand(@"
            SELECT COUNT(*)
            FROM Registrations
            WHERE course_classroom_id = @cc_id
              AND academic_year_start = @ay
        ", con))
                {
                    cmdCount.Parameters.AddWithValue("@cc_id", ccId);
                    cmdCount.Parameters.AddWithValue("@ay", latestAcademicYear);

                    int count = Convert.ToInt32(cmdCount.ExecuteScalar());
                    label63.Text = count.ToString();
                }
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show("الرجاء اختيار مجموعة للنقل إليها.");
                return;
            }

            int destCcId = Convert.ToInt32(comboBox1.SelectedValue);
            int toMoveCount;
            int latestAcademicYear;

            using (SqlConnection con = new conn.DatabaseConnection().OpenConnection())
            using (SqlTransaction tx = con.BeginTransaction())
            {
                try
                {
                    // أحدث عام جامعي
                    using (SqlCommand cmdMaxYear = new SqlCommand(@"
                SELECT ISNULL(MAX(academic_year_start), 0)
                FROM Registrations
                WHERE course_classroom_id = @cc_id
            ", con, tx))
                    {
                        cmdMaxYear.Parameters.AddWithValue("@cc_id", selectedCCId1);
                        latestAcademicYear = Convert.ToInt32(cmdMaxYear.ExecuteScalar());
                    }

                    if (latestAcademicYear == 0)
                    {
                        MessageBox.Show("لا توجد تسجيلات مرتبطة بالمجموعة.");
                        tx.Rollback();
                        return;
                    }

                    // عدد الطلاب المراد نقلهم
                    using (SqlCommand cmdCount = new SqlCommand(@"
                SELECT COUNT(*)
                FROM Registrations
                WHERE course_classroom_id = @cc_id
                  AND academic_year_start = @ay
            ", con, tx))
                    {
                        cmdCount.Parameters.AddWithValue("@cc_id", selectedCCId1);
                        cmdCount.Parameters.AddWithValue("@ay", latestAcademicYear);
                        toMoveCount = Convert.ToInt32(cmdCount.ExecuteScalar());
                    }

                    if (toMoveCount <= 0)
                    {
                        MessageBox.Show("لا يوجد طلاب لنقلهم.");
                        tx.Rollback();
                        return;
                    }

                    if (MessageBox.Show(
                        $"سيتم نقل {toMoveCount} طالب/ة.\nهل تريد المتابعة؟",
                        "تأكيد النقل",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        tx.Rollback();
                        return;
                    }

                    // تحديث التسجيلات
                    using (SqlCommand cmdUpdate = new SqlCommand(@"
                UPDATE Registrations
                SET course_classroom_id = @dest_cc_id
                WHERE course_classroom_id = @src_cc_id
                  AND academic_year_start = @ay
            ", con, tx))
                    {
                        cmdUpdate.Parameters.AddWithValue("@dest_cc_id", destCcId);
                        cmdUpdate.Parameters.AddWithValue("@src_cc_id", selectedCCId1);
                        cmdUpdate.Parameters.AddWithValue("@ay", latestAcademicYear);
                        cmdUpdate.ExecuteNonQuery();
                    }

                    // الحصول على السعة وعدد الطلاب الحاليين في المجموعة المستهدفة
                    int destCapacity = 0;
                    int currentCount = 0;

                    using (SqlCommand cmdInfo = new SqlCommand(@"
                SELECT capacity,
                       (SELECT COUNT(*) FROM Registrations 
                        WHERE course_classroom_id = @dest_cc_id 
                          AND academic_year_start = @ay) AS currentCount
                FROM Course_Classroom
                WHERE id = @dest_cc_id
            ", con, tx))
                    {
                        cmdInfo.Parameters.AddWithValue("@dest_cc_id", destCcId);
                        cmdInfo.Parameters.AddWithValue("@ay", latestAcademicYear);
                        using (SqlDataReader dr = cmdInfo.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                destCapacity = Convert.ToInt32(dr["capacity"]);
                                currentCount = Convert.ToInt32(dr["currentCount"]);
                            }
                        }
                    }

                    // زيادة السعة فقط إذا كان العدد يتجاوز السعة
                    int additionalCapacity = 0;
                    if (currentCount > destCapacity)
                    {
                        additionalCapacity = currentCount - destCapacity;
                    }

                    if (additionalCapacity > 0)
                    {
                        using (SqlCommand cmdCap = new SqlCommand(@"
                    UPDATE Course_Classroom
                    SET capacity = capacity + @inc
                    WHERE id = @dest_cc_id
                ", con, tx))
                        {
                            cmdCap.Parameters.AddWithValue("@inc", additionalCapacity);
                            cmdCap.Parameters.AddWithValue("@dest_cc_id", destCcId);
                            cmdCap.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    MessageBox.Show("خطأ: " + ex.Message);
                    return;
                }
            }

            MessageBox.Show("✅ تم النقل بنجاح.");
            UpdateStudentsCountLabel(selectedCCId1);
            PopulateDestinationGroups(selectedCourseIdForRow, selectedCCId1);
            LoadCourseClassroom();
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        //************************
        //ربط المواد بي الاساتده
        private void LoadYears1()
        {
            comboBox5.Items.Clear();
            for (int i = 1; i <= 4; i++)
                comboBox5.Items.Add(i);
            if (comboBox5.Items.Count > 0)
                comboBox5.SelectedIndex = 0;
        }

        private void LoadInstructors1()
        {
            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                SqlDataAdapter da = new SqlDataAdapter("SELECT instructor_id, full_name FROM Instructors", con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                comboBox4.DataSource = dt;
                comboBox4.DisplayMember = "full_name";
                comboBox4.ValueMember = "instructor_id";
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }
        private void LoadInstructorCourses()
        {
            if (comboBox4.SelectedValue == null || comboBox4.SelectedValue is DataRowView)
                return;

            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                int instructorId = Convert.ToInt32(comboBox4.SelectedValue);

                SqlCommand cmd = new SqlCommand(@"
            SELECT 
    c.course_id, 
    c.course_name AS [اسم المادة], 
    cd.course_dep_code AS [رمزالمادة]
FROM Courses c
INNER JOIN Course_Department cd ON cd.course_id = c.course_id
INNER JOIN Course_Instructor ci ON ci.course_id = c.course_id
WHERE ci.instructor_id = @id
", con);
                cmd.Parameters.AddWithValue("@id", instructorId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView2.DataSource = dt;

                if (!dataGridView2.Columns.Contains("ترقيم"))
                {
                    DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                    col.Name = "ترقيم";
                    col.HeaderText = "م";
                    col.Width = 50;
                    dataGridView2.Columns.Insert(0, col);
                }
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                    dataGridView2.Rows[i].Cells["ترقيم"].Value = i + 1;

                if (dataGridView2.Columns.Contains("course_id"))
                    dataGridView2.Columns["course_id"].Visible = false;
                dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                if (dt.Rows.Count == 0)
                    label55.Text = "⚠️ لا توجد مواد مرتبطة بهذا الأستاذ.";
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        private void button27_Click(object sender, EventArgs e)
        {
            if (dataGridView3.CurrentRow == null || comboBox4.SelectedValue == null)
            {
                MessageBox.Show("⚠️ الرجاء اختيار مادة وأستاذ أولاً");
                return;
            }

            int courseId = Convert.ToInt32(dataGridView3.CurrentRow.Cells["course_id"].Value);
            int instructorId = Convert.ToInt32(comboBox4.SelectedValue);

            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                SqlCommand cmd = new SqlCommand("INSERT INTO Course_Instructor(course_id, instructor_id) VALUES(@c, @i)", con);
                cmd.Parameters.AddWithValue("@c", courseId);
                cmd.Parameters.AddWithValue("@i", instructorId);
                cmd.ExecuteNonQuery();

                label55.Text = "✅ تم ربط المادة بالأستاذ بنجاح.";
                label55.ForeColor = Color.Green;
            }
            catch (SqlException)
            {
                MessageBox.Show("⚠️ المادة مرتبطة بالفعل بهذا الأستاذ.");
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }

            comboBoxYear4_SelectedIndexChanged(null, null);
            LoadInstructorCourses();
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {

            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                SqlCommand cmd = new SqlCommand(@"
          SELECT 
    c.course_id, 
    c.course_name AS [اسم المادة], 
    cd.course_dep_code AS [رمزالمادة]
FROM Courses c
LEFT JOIN Course_Department cd ON cd.course_id = c.course_id
WHERE c.year_number = @year
", con);
                cmd.Parameters.AddWithValue("@year", comboBox5.SelectedItem);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView3.DataSource = dt;

                // عمود الترقيم
                if (!dataGridView3.Columns.Contains("ترقيم"))
                {
                    DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                    col.Name = "ترقيم";
                    col.HeaderText = "م";
                    col.Width = 20;
                    dataGridView3.Columns.Insert(0, col);
                }
                for (int i = 0; i < dataGridView3.Rows.Count; i++)
                    dataGridView3.Rows[i].Cells["ترقيم"].Value = i + 1;

                if (dataGridView3.Columns.Contains("course_id"))
                    dataGridView3.Columns["course_id"].Visible = false;
                dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadInstructorCourses();
        }



        private void button29_Click(object sender, EventArgs e)
        {


            // الحصول على الصف الفعلي الذي ضغط عليه المستخدم
            DataGridViewRow row = null;

            if (dataGridView2.SelectedRows.Count > 0)
                row = dataGridView2.SelectedRows[0];
            else if (dataGridView2.CurrentRow != null)
                row = dataGridView2.CurrentRow;

            if (row == null || row.IsNewRow || row.Cells["course_id"].Value == null || row.Cells["course_id"].Value == DBNull.Value)
            {
                label55.Text = "⚠️ الرجاء اختيار مادة صحيحة للحذف.";
                label55.ForeColor = Color.Red;
                return;
            }

            // التحقق من اختيار أستاذ
            if (comboBox4.SelectedValue == null || comboBox4.SelectedValue is DataRowView)
            {
                label55.Text = "⚠️ الرجاء اختيار أستاذ أولاً.";
                label55.ForeColor = Color.Red;
                return;
            }

            int courseId = Convert.ToInt32(row.Cells["course_id"].Value);
            int instructorId = Convert.ToInt32(comboBox4.SelectedValue);

            if (MessageBox.Show("هل أنت متأكد من الحذف؟", "تأكيد الحذف",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Course_Instructor WHERE course_id = @c AND instructor_id = @i", con);
                cmd.Parameters.AddWithValue("@c", courseId);
                cmd.Parameters.AddWithValue("@i", instructorId);

                int affected = cmd.ExecuteNonQuery();

                if (affected == 0)
                {
                    label55.Text = "ℹ️ لا يوجد ربط لهذا الأستاذ مع هذه المادة.";
                    label55.ForeColor = Color.DarkGoldenrod;
                }
                else
                {
                    label55.Text = "✅ تم حذف الربط بنجاح.";
                    label55.ForeColor = Color.Green;
                }
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }

            // تحديث الجداول بعد الحذف
            LoadInstructorCourses();
            comboBox5_SelectedIndexChanged(null, null);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if (selectedCCId == 0)
            {
                MessageBox.Show("اختر محاضرة من القائمة للتعديل!");
                return;
            }

            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection connUpdate = db.OpenConnection())
                {
                    // ==================== التحقق من الحقول ====================
                    // التحقق من اختيار القسم
                    if (comboBox6.SelectedValue == null)
                    {
                        MessageBox.Show("اختر القسم!");
                        return;
                    }
                    int depID = Convert.ToInt32(comboBox6.SelectedValue);

                    if (comboBox9.SelectedValue == null) { MessageBox.Show("اختر الدكتور!"); return; }
                    int instructorIdIn = Convert.ToInt32(comboBox9.SelectedValue);

                    if (comboBox8.SelectedValue == null) { MessageBox.Show("اختر المادة!"); return; }
                    int courseIdIn = Convert.ToInt32(comboBox8.SelectedValue);

                    DataRowView drvRoom = comboBox2.SelectedItem as DataRowView;
                    if (drvRoom == null) { MessageBox.Show("اختر القاعة!"); return; }
                    int classroomIdIn = Convert.ToInt32(drvRoom["classroom_id"]);

                    if (comboBox11.SelectedItem == null) { MessageBox.Show("اختر اليوم!"); return; }
                    var selectedDay = (KeyValuePair<int, string>)comboBox11.SelectedItem;
                    int lectureDayIn = selectedDay.Key;

                    if (comboBox10.SelectedValue == null) { MessageBox.Show("اختر المجموعة!"); return; }
                    int groupNumIn = Convert.ToInt32(comboBox10.SelectedValue);

                    if (numericUpDown1.Value <= 0) { MessageBox.Show("حدد العدد بشكل صحيح!"); return; }
                    int capacityIn = Convert.ToInt32(numericUpDown1.Value);

                    if (numericUpDown2.Value <= 0)
                    {
                        MessageBox.Show("حدد المدة بشكل صحيح!");
                        return;
                    }
                    int duration = Convert.ToInt32(numericUpDown2.Value);

                    if (comboBox12.SelectedItem == null) { MessageBox.Show("اختر وقت البداية!"); return; }
                    TimeSpan start = TimeSpan.Parse(comboBox12.SelectedItem.ToString());

                    // ==================== جلب مدة المحاضرة ====================
                    int courseUnits = 0;
                    string qUnits = "SELECT units FROM Courses WHERE course_id = @course_id";
                    using (SqlCommand cmdGetUnits = new SqlCommand(qUnits, connUpdate))
                    {
                        cmdGetUnits.Parameters.AddWithValue("@course_id", courseIdIn);
                        courseUnits = Convert.ToInt32(cmdGetUnits.ExecuteScalar());
                    }
                    TimeSpan end = start.Add(TimeSpan.FromHours(courseUnits));

                    // ==================== التحقق من تعارض القاعة ====================
                    string qCheckRoom = @"
                SELECT COUNT(*) 
                FROM Course_Classroom
                WHERE classroom_id = @classroom_id
                  AND lecture_day = @lecture_day
                  AND id <> @cc_id
                  AND (
                        (@start_time >= start_time AND @start_time < end_time) OR
                        (@end_time > start_time AND @end_time <= end_time) OR
                        (@start_time <= start_time AND @end_time >= end_time)
                      )";
                    using (SqlCommand cmdRoom = new SqlCommand(qCheckRoom, connUpdate))
                    {
                        cmdRoom.Parameters.AddWithValue("@classroom_id", classroomIdIn);
                        cmdRoom.Parameters.AddWithValue("@lecture_day", lectureDayIn);
                        cmdRoom.Parameters.AddWithValue("@start_time", start);
                        cmdRoom.Parameters.AddWithValue("@end_time", end);
                        cmdRoom.Parameters.AddWithValue("@cc_id", selectedCCId);

                        int existsRoom = (int)cmdRoom.ExecuteScalar();
                        if (existsRoom > 0) { MessageBox.Show("❌ القاعة محجوزة في هذا الوقت."); return; }
                    }

                    // ==================== التحقق من تعارض الدكتور ====================
                    string qCheckInstructor = @"
                SELECT TOP 1 
                    c.course_name, cl.room_name, cc.start_time, cc.end_time
                FROM Course_Classroom cc
                JOIN Courses c ON cc.course_id = c.course_id
                JOIN Classrooms cl ON cc.classroom_id = cl.classroom_id
                WHERE cc.instructor_id = @instructor_id
                  AND cc.lecture_day = @lecture_day
                  AND cc.id <> @cc_id
                  AND (
                        (@start_time >= cc.start_time AND @start_time < cc.end_time) OR
                        (@end_time > cc.start_time AND @end_time <= cc.end_time) OR
                        (@start_time <= cc.start_time AND @end_time >= cc.end_time)
                      )";
                    using (SqlCommand cmdInstructor = new SqlCommand(qCheckInstructor, connUpdate))
                    {
                        cmdInstructor.Parameters.AddWithValue("@instructor_id", instructorIdIn);
                        cmdInstructor.Parameters.AddWithValue("@lecture_day", lectureDayIn);
                        cmdInstructor.Parameters.AddWithValue("@start_time", start);
                        cmdInstructor.Parameters.AddWithValue("@end_time", end);
                        cmdInstructor.Parameters.AddWithValue("@cc_id", selectedCCId);

                        using (SqlDataReader reader = cmdInstructor.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string courseName = reader["course_name"].ToString();
                                string roomName = reader["room_name"].ToString();
                                string startTime = reader["start_time"].ToString();
                                string endTime = reader["end_time"].ToString();
                                MessageBox.Show(
                                    $"❌ الدكتور لديه محاضرة أخرى في هذا الوقت:\n" +
                                    $"المادة: {courseName}\n" +
                                    $"القاعة: {roomName}\n" +
                                    $"من {startTime} إلى {endTime}"
                                );
                                return;
                            }
                        }
                    }

                    // ==================== التحقق من المجموعة ====================
                    string qCheckGroup = @"
                SELECT COUNT(*) 
                FROM Course_Classroom
                WHERE course_id = @course_id
                  AND group_number = @group_number
                  AND department_id = @depID
                  AND id <> @cc_id";
                    using (SqlCommand cmdGroup = new SqlCommand(qCheckGroup, connUpdate))
                    {
                        cmdGroup.Parameters.AddWithValue("@course_id", courseIdIn);
                        cmdGroup.Parameters.AddWithValue("@group_number", groupNumIn);
                        cmdGroup.Parameters.AddWithValue("@cc_id", selectedCCId);
                        cmdGroup.Parameters.AddWithValue("@depID", depID);

                        int existsGroup = (int)cmdGroup.ExecuteScalar();
                        if (existsGroup > 0)
                        {
                            MessageBox.Show("❌ هذه المجموعة موجودة مسبقًا لهذه المادة، اختر مجموعة جديدة.");
                            return;
                        }
                    }

                    // ==================== تحديث المحاضرة ====================
                    string qUpdate = @"
                UPDATE Course_Classroom
                SET course_id = @course_id,
                    classroom_id = @classroom_id,
                    group_number = @group_number,
                    capacity = @capacity,
                    start_time = @start_time,
                    end_time = @end_time,
                    lecture_day = @lecture_day,
                    instructor_id = @instructor_id,
                    department_id = @depID
                WHERE id = @cc_id";
                    using (SqlCommand cmdUpdate = new SqlCommand(qUpdate, connUpdate))
                    {
                        cmdUpdate.Parameters.AddWithValue("@course_id", courseIdIn);
                        cmdUpdate.Parameters.AddWithValue("@classroom_id", classroomIdIn);
                        cmdUpdate.Parameters.AddWithValue("@group_number", groupNumIn);
                        cmdUpdate.Parameters.AddWithValue("@capacity", capacityIn);
                        cmdUpdate.Parameters.AddWithValue("@start_time", start);
                        cmdUpdate.Parameters.AddWithValue("@end_time", end);
                        cmdUpdate.Parameters.AddWithValue("@lecture_day", lectureDayIn);
                        cmdUpdate.Parameters.AddWithValue("@instructor_id", instructorIdIn);
                        cmdUpdate.Parameters.AddWithValue("@depID", depID);
                        cmdUpdate.Parameters.AddWithValue("@cc_id", selectedCCId);

                        cmdUpdate.ExecuteNonQuery();
                    }

                    MessageBox.Show("✅ تم تعديل المحاضرة بنجاح!");
                    ClearFields();
                    LoadCourseClassroom();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage4)
            {
                textBox7.Focus();

            }
            else if (tabControl1.SelectedTab == tabPage1)
            {
                txtDeptName.Focus();
            }
            else if (tabControl1.SelectedTab == tabPage2)
            {
                LoadDepartments1();
                comboBoxYear4_SelectedIndexChanged(null, null);
            }
            else if (tabControl1.SelectedTab == tabPage3)
            {
                textBoxName.Focus();

            }
            else if (tabControl1.SelectedTab == tabPage5)
            {

            }
            else if (tabControl1.SelectedTab == tabPage6)
            {
                txtCourseCode.Focus();
            }
        
            else if (tabControl1.SelectedTab == tabPage8)
            {
                LoadInstructorCourses();
                comboBox5_SelectedIndexChanged(null, null);
            }

            }
    }
}