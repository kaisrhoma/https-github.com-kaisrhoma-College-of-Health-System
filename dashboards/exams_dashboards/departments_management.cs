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
            LoadDepartments1();
            LoadYears();
            dataGridViewDepartment.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewDepartment.MultiSelect = false;
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


        private void LoadYears()
        {
            comboBoxYear4.Items.Clear();
            for (int i = 1; i <= 4; i++) // حسب عدد سنوات الكلية
                comboBoxYear4.Items.Add(i);
            if (comboBoxYear4.Items.Count > 0)
                comboBoxYear4.SelectedIndex = 0; // السنة الأولى افتراضي
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
                // ComboBox لتحديد القسم الجديد للتحديث
                comboBox1.DataSource = dt.Copy();
                comboBox1.DisplayMember = "dep_name";
                comboBox1.ValueMember = "department_id";



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
            AND NOT EXISTS (
                SELECT 1 FROM Course_Department cd WHERE cd.course_id = c.course_id
            )", con);
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
            SELECT c.course_id, c.course_name AS [اسم المادة], c.course_code AS [رمز المادة]
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
            LoadDepartmentCourses();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridViewDepartment.CurrentRow == null)
            {
                label50.Text = "⚠️ الرجاء اختيار مادة لتحديث القسم.";
                label50.ForeColor = Color.Red;
                return;
            }

            if (comboBox1.SelectedValue == null || comboBox1.SelectedValue is DataRowView)
            {
                label50.Text = "⚠️ الرجاء اختيار قسم جديد ";
                label50.ForeColor = Color.Red;
                return;
            }

            int courseId = Convert.ToInt32(dataGridViewDepartment.CurrentRow.Cells["course_id"].Value);
            int newDeptId = Convert.ToInt32(comboBox1.SelectedValue);

            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                SqlCommand cmd = new SqlCommand("UPDATE Course_Department SET department_id = @newDept WHERE course_id = @courseId", con);
                cmd.Parameters.AddWithValue("@newDept", newDeptId);
                cmd.Parameters.AddWithValue("@courseId", courseId);
                cmd.ExecuteNonQuery();

                label50.Text = "✅ تم تغيير القسم للمادة: " + dataGridViewDepartment.CurrentRow.Cells["اسم المادة"].Value.ToString();
                label50.ForeColor = Color.Green;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }

            LoadDepartmentCourses();        // تحديث قائمة القسم الحالي
            comboBoxYear4_SelectedIndexChanged(null, null); // تحديث قائمة المواد غير المرتبطة
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
    }
}