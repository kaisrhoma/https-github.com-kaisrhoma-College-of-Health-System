using college_of_health_sciences.moduls;
using DocumentFormat.OpenXml.Bibliography;
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
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            comboBox3.SelectedItem = null;
            comboBox4.SelectedItem = null;
            int index = comboBox1.Items.IndexOf("ليبي");
            if (index >= 0)
                comboBox1.SelectedIndex = index;
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
            hasError = checkComboBoxes(comboBox1, hasError);
            hasError = checkComboBoxes(comboBox3,hasError);
            hasError = checkComboBoxes(comboBox4,hasError);

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

        public void DownloadCoursesForStudent(int studentId, int selectedYear, int departmentId)
        {
            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    int? academicYearStart = (selectedYear == 1 ? (int?)numericUpDown1.Value : null);
                    int totalRegistered = 0;
                    List<string> fullCourses = new List<string>();

                    // 1️⃣ تنزيل مواد السنة الأولى دائماً للقسم العام
                    int generalDepartmentId = GetGeneralDepartmentId(con); // دالة لجلب ID القسم العام
                    RegisterCoursesForYear(studentId, 1, generalDepartmentId,
                        selectedYear == 1 ? academicYearStart : null,
                        con, ref totalRegistered, fullCourses, selectedYear > 1);

                    // 2️⃣ تنزيل المواد لبقية السنوات (من 2 وحتى السنة المختارة -1)
                    for (int y = 2; y < selectedYear; y++)
                    {
                        RegisterCoursesForYear(studentId, y, departmentId, null, con, ref totalRegistered, fullCourses, true);
                    }

                    // 3️⃣ رسالة النتيجة
                    if (totalRegistered == 0)
                        MessageBox.Show("No courses were registered.");
                    else if (fullCourses.Count > 0)
                        MessageBox.Show("Student registered except for the following courses:\n" + string.Join("\n", fullCourses));
                    else
                        MessageBox.Show("All courses registered successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // دالة تجيب أول دكتور مرتبط بالمادة
        private int GetFirstInstructorForCourse(SqlConnection con, int courseId)
        {
            using (SqlCommand cmd = new SqlCommand(@"
        SELECT TOP 1 instructor_id 
        FROM Course_Instructor 
        WHERE course_id = @courseId 
        ORDER BY instructor_id", con))
            {
                cmd.Parameters.AddWithValue("@courseId", courseId);
                object result = cmd.ExecuteScalar();
                if (result != null)
                    return Convert.ToInt32(result);
                else
                    throw new Exception($"لا يوجد دكتور مرتبط بالمادة {courseId}");
            }
        }

        // دالة تجيب أول قاعة موجودة
        private int GetFirstClassroom(SqlConnection con)
        {
            using (SqlCommand cmd = new SqlCommand(@"
        SELECT TOP 1 classroom_id 
        FROM Classrooms 
        ORDER BY classroom_id", con))
            {
                object result = cmd.ExecuteScalar();
                if (result != null)
                    return Convert.ToInt32(result);
                else
                    throw new Exception("لا توجد قاعات متاحة في جدول Classrooms");
            }
        }


        private int GetOrCreateGroup(SqlConnection con, int courseId, int? academicYearStart)
        {
            int groupId = 0;

            // ✅ جلب كل المجموعات المرتبطة بالمادة
            SqlCommand getGroupsCmd = new SqlCommand(@"
        SELECT cc.id, cc.capacity, cc.group_number
        FROM Course_Classroom cc
        WHERE cc.course_id = @courseId
        ORDER BY cc.group_number;", con);
            getGroupsCmd.Parameters.AddWithValue("@courseId", courseId);

            DataTable groups = new DataTable();
            new SqlDataAdapter(getGroupsCmd).Fill(groups);

            foreach (DataRow group in groups.Rows)
            {
                int isgroupId = Convert.ToInt32(group["id"]);
                int capacity = Convert.ToInt32(group["capacity"]);

                SqlCommand countCmd = new SqlCommand(@"
            SELECT COUNT(*) 
            FROM Registrations 
            WHERE course_classroom_id = @groupId 
              AND academic_year_start = @academicYearStart", con);

                countCmd.Parameters.AddWithValue("@groupId", isgroupId);
                countCmd.Parameters.AddWithValue("@academicYearStart",
                    academicYearStart.HasValue ? (object)academicYearStart.Value : DBNull.Value);

                int currentCount = (int)countCmd.ExecuteScalar();

                if (currentCount < capacity)
                {
                    groupId = isgroupId;
                    break;
                }
            }

            if (groupId == 0)
            {
                // جلب أول دكتور مرتبط بالمادة
                int instructorId = GetFirstInstructorForCourse(con, courseId);

                // جلب أول قاعة متاحة
                int classroomId = GetFirstClassroom(con);

                // إنشاء مجموعة جديدة برقم أكبر من آخر مجموعة
                int nextGroupNumber = 1;
                if (groups.Rows.Count > 0)
                    nextGroupNumber = Convert.ToInt32(groups.Rows[groups.Rows.Count - 1]["group_number"]) + 1;

                using (SqlCommand cmd = new SqlCommand(@"
            INSERT INTO Course_Classroom
            (course_id, classroom_id, group_number, capacity, start_time, end_time, lecture_day, instructor_id)
            OUTPUT INSERTED.id 
            VALUES (@courseId, @classroomId, @groupNumber, 80, '09:00:00', '12:00:00',6, @instructorId)", con))
                {
                    cmd.Parameters.AddWithValue("@courseId", courseId);
                    cmd.Parameters.AddWithValue("@classroomId", classroomId);
                    cmd.Parameters.AddWithValue("@groupNumber", nextGroupNumber);
                    cmd.Parameters.AddWithValue("@instructorId", instructorId);

                    groupId = (int)cmd.ExecuteScalar();
                }
            }

            return groupId;
        }


        // دالة مساعدة لتسجيل المواد
        private void RegisterCoursesForYear(int studentId, int year, int deptId, int? academicYearStart,
                                            SqlConnection con, ref int totalRegistered, List<string> fullCourses, bool forceNull)
        {
            SqlCommand coursesCmd = new SqlCommand(@"
        SELECT c.course_id, c.course_name
        FROM Courses c
        JOIN Course_Department cd ON cd.course_id = c.course_id
        WHERE c.year_number = @year AND cd.department_id = @dept", con);
            coursesCmd.Parameters.AddWithValue("@year", year);
            coursesCmd.Parameters.AddWithValue("@dept", deptId);

            DataTable courses = new DataTable();
            new SqlDataAdapter(coursesCmd).Fill(courses);

            foreach (DataRow courseRow in courses.Rows)
            {
                int courseId = Convert.ToInt32(courseRow["course_id"]);
                string courseName = courseRow["course_name"].ToString();
                int? classroomId = null;

                // إذا السنة الأولى والسنة المختارة = 1 => البحث عن Classroom متاح
                if (year == 1 && !forceNull)
                {
                    classroomId = GetOrCreateGroup(con, courseId, academicYearStart);
                }

                SqlCommand insertCmd = new SqlCommand(@"
            IF NOT EXISTS (
                SELECT 1 FROM Registrations 
                WHERE student_id = @studentId AND course_id = @courseId
            )
            INSERT INTO Registrations
            (student_id, course_id, year_number, status, course_classroom_id, academic_year_start)
            VALUES
            (@studentId, @courseId, @year, N'مسجل', @classroomId, @academicYearStart)", con);

                insertCmd.Parameters.AddWithValue("@studentId", studentId);
                insertCmd.Parameters.AddWithValue("@courseId", courseId);
                insertCmd.Parameters.AddWithValue("@year", year);
                insertCmd.Parameters.AddWithValue("@classroomId", classroomId.HasValue ? (object)classroomId.Value : DBNull.Value);
                insertCmd.Parameters.AddWithValue("@academicYearStart", academicYearStart.HasValue ? (object)academicYearStart.Value : DBNull.Value);

                int affected = insertCmd.ExecuteNonQuery();
                if (affected > 0) totalRegistered++;
                else if (classroomId == null && year == 1 && !forceNull) fullCourses.Add(courseName);
            }
        }

        // دالة مساعدة لجلب ID القسم العام
        private int GetGeneralDepartmentId(SqlConnection con)
        {
            SqlCommand cmd = new SqlCommand("SELECT department_id FROM Departments WHERE dep_name = N'عام'", con);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }


        private int GetStatusId(SqlConnection con, string statusDescription)
        {
            SqlCommand cmd = new SqlCommand("SELECT status_id FROM Status WHERE description = @desc", con);
            cmd.Parameters.AddWithValue("@desc", statusDescription);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }


        //اضافة طالب جديد الى القاعدة في التاب الأول
        private void button7_Click(object sender, EventArgs e)
        {
            if (!CheckNullFields())
            {
                label1.ForeColor = Color.Red;
                label1.Text = "يرجى ملئ الحقول !";
                return;
            }

            string fullName = textBox2.Text.Trim();
            if (!Regex.IsMatch(fullName, @"^[\p{L}\s]+$"))
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

            bool st_gender = radioButton1.Checked;
            int selectedYear = Convert.ToInt32(comboBox4.SelectedValue);
            int departmentId = Convert.ToInt32(comboBox3.SelectedValue);

            conn.DatabaseConnection db = new conn.DatabaseConnection();
            using (SqlConnection con = db.OpenConnection())
            {
                // تحقق من الرقم الجامعي
                SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Students WHERE university_number = @uni", con);
                checkCmd.Parameters.Add("@uni", SqlDbType.NVarChar).Value = uni;
                int exists = (int)checkCmd.ExecuteScalar();
                if (exists > 0)
                {
                    label1.ForeColor = Color.Red;
                    label1.Text = "⚠ الرقم الجامعي موجود مسبقًا!";
                    return;
                }

                // إدراج الطالب
                string q = @"INSERT INTO Students 
                     (university_number, full_name, college, department_id, current_year, status_id, gender, birth_date, nationality, exam_round) 
                     VALUES (@university_number, @full_name, N'كلية العلوم الصحية', @department_id, @current_year, @status_id, @gender, @birth_date, @nationality, N'دور أول')";

                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.Add("@university_number", SqlDbType.NVarChar).Value = uni;
                cmd.Parameters.Add("@full_name", SqlDbType.NVarChar).Value = fullName;
                cmd.Parameters.Add("@department_id", SqlDbType.Int).Value = departmentId;
                cmd.Parameters.Add("@current_year", SqlDbType.Int).Value = selectedYear;
                int statusId = (selectedYear == 1 ? GetStatusId(con, "مستمر") : GetStatusId(con, "محول"));
                cmd.Parameters.Add("@status_id", SqlDbType.Int).Value = statusId;
                cmd.Parameters.Add("@gender", SqlDbType.Bit).Value = st_gender;
                cmd.Parameters.Add("@birth_date", SqlDbType.Date).Value = dateTimePicker1.Value.Date;
                cmd.Parameters.Add("@nationality", SqlDbType.NVarChar).Value = comboBox1.SelectedItem.ToString();

                try
                {
                    cmd.ExecuteNonQuery();

                    // جلب المعرف الجديد للطالب
                    SqlCommand getIdCmd = new SqlCommand("SELECT TOP 1 student_id FROM Students WHERE university_number = @uni ORDER BY student_id DESC", con);
                    getIdCmd.Parameters.Add("@uni", SqlDbType.NVarChar).Value = uni;
                    int studentId = (int)getIdCmd.ExecuteScalar();

                    label1.ForeColor = Color.Green;
                    label1.Text = "تمت إضافة الطالب بنجاح";
                    SetFieldsEmpty();

                    // تنزيل المواد مباشرة
                    DownloadCoursesForStudent(studentId, selectedYear, departmentId);
                }
                catch (Exception ex)
                {
                    label1.ForeColor = Color.Red;
                    label1.Text = "خطأ: " + ex.Message;
                }
            }
        }







        private void students_management_Load(object sender, EventArgs e)
        {
            label1.Text = "";
            textBox2.Focus();

            string[] nationalities = new string[]
    {
        "أفغاني", "ألباني", "جزائري", "أمريكي ساموا", "أندوري", "أنغولي", "أنتيغوا وبربودا",
        "أرجنتيني", "أرميني", "أسترالي", "نمساوي", "أذربيجاني", "باهامي", "بحريني",
        "بنغلاديشي", "باربادوسي", "بيلاروسي", "بلجيكي", "بليزي", "بيني", "بوتاني", "بوليفي",
        "البوسنة والهرسك", "بوتسواني", "برازيلي", "بروتغالي", "بروني", "بلغاري", "بوركيني",
        "بوروندي", "كمبودي", "كاميروني", "كندي", "الرأس الأخضر", "جمهورية إفريقيا الوسطى",
        "تشادي", "تشيلي", "صيني", "كولومبي", "قبرصي", "كوكوني", "كوبي", "تشيكي", "ديموقراطي الكونغو",
        "الدنماركي", "جيبوتي", "دومينيكا", "دومينيكاني", "تيموري", "الإكوادوري", "مصري", "السلفادوري",
        "غيني الاستوائي", "إريتري", "إستوني", "إثيوبي", "فيجي", "فنلندي", "فرنسي", "الغابوني",
        "غامبي", "جورجي", "ألماني", "غانا", "يوناني", "جرينادا", "غواتيمالي", "غيني", "غيني بيساو",
        "غيانا", "هايتي", "هondوراسي", "هنغاري", "أيسلندي", "هندي", "إندونيسي", "إيراني", "عراقي",
        "أيرلندي", "إيطالي", "جامايكي", "ياباني", "أردني", "قزاخستان", "كينيا", "كيريباتي",
        "كوريا الشمالية", "كوريا الجنوبية", "كويت", "قيرغيزستان", "لاوسي", "لاتفيا", "لبناني", "ليسوتو",
        "ليبيريا", "ليبي", "ليختنشتاين", "لتواني", "لوكسمبورغ", "مدغشقر", "مالاوي", "ماليزيا",
        "جزر المالديف", "مالي", "مالطا", "جزر مارشال", "موريتانيا", "موريشيوس", "المكسيك",
        "ولايات ميكرونيسيا المتحدة", "مولدوفا", "موناكو", "منغوليا", "الجبل الأسود", "المغربي", "موزمبيق",
        "ميانمار", "ناميبيا", "ناورو", "نيبال", "هولندي", "نيوزيلندا", "نيكاراغوا", "النيجيري",
        "النيجر", "النرويج", "عماني", "باكستان", "بالاو", "بنما", "بابوا غينيا الجديدة", "باراغواي",
        "بيرو", "الفلبين", "بولندا", "البرتغال", "قطر", "روماني", "روسي", "رواندي", "سانت كيتس ونيفيس",
        "سانت لوسيا", "سانت فنسنت والغرينادين", "ساموا", "سان مارينو", "ساو تومي وبرينسيبي", "السعودية",
        "السنغافوري", "سلوفاكي", "سلوفيني", "جزر سليمان", "الصومال", "جنوب أفريقيا", "جنوب السودان",
        "إسباني", "سريلانكي", "السودان", "سوري", "سورينام", "سوازيلاند", "السويدي", "سويسري",
        "سوريالي", "طاجيكستان", "تنزانيا", "تايلاندي", "توغو", "تونسي", "تركيا", "تركمانستان", "توفالو",
        "أوغندي", "أوكراني", "الإماراتي", "بريطاني", "أمريكي", "أوروغواي", "أوزبكستان", "فانواتو",
        "فنزويلا", "فيتنامي", "اليمني", "زامبي", "زيمبابوي"
    };

            // ترتيب أبجدي
            Array.Sort(nationalities, StringComparer.CurrentCulture);
            comboBox1.Items.AddRange(nationalities);

            // تعيين "ليبي" كافتراضي
            int index = comboBox1.Items.IndexOf("ليبي");
            if (index >= 0)
                comboBox1.SelectedIndex = index;

            // تفعيل البحث أثناء الكتابة
            comboBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBox1.AutoCompleteSource = AutoCompleteSource.ListItems;

            comboBox1.DropDownStyle = ComboBoxStyle.DropDown; // يسمح بالكتابة
            comboBox1.DropDownHeight = 1; // يجعل القائمة شبه مخفية
            comboBox1.IntegralHeight = false; // منع تغيير الحجم التلقائي
            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    int month3;

                    using (SqlCommand cmddate = new SqlCommand("SELECT month_number FROM Months WHERE month_id = 1 ", con))
                    {
                        month3 = Convert.ToInt32(cmddate.ExecuteScalar());
                    }

                    int academicYearStart = DateTime.Now.Month >= month3 ? DateTime.Now.Year : DateTime.Now.Year - 1;
                    numericUpDown1.Value = academicYearStart;
                    string q = "select * from Departments";
                    SqlDataAdapter da = new SqlDataAdapter(q, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    comboBox3.DataSource = dt;
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
            comboBox4.SelectedIndexChanged -= comboBox4_SelectedIndexChanged;
            comboBox4.DataSource = new BindingSource(study_year, null);
            comboBox4.DisplayMember = "Value";
            comboBox4.ValueMember = "Key";
            comboBox4.SelectedIndex = -1;
            comboBox4.SelectedIndexChanged += comboBox4_SelectedIndexChanged;
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
                        setColumnComboBox(dataGridView1, "description", "student_status", "الحالة الدراسية", "description", new List<string> { "مستمر", "مؤجل", "مستبعد", "خريج","محول"});
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


        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4.SelectedValue == null) return;

            int selectedYear = Convert.ToInt32(comboBox4.SelectedValue);

            if (selectedYear == 1)
            {
                // السنة الأولى => اختر "عام" تلقائيًا
                foreach (DataRowView row in comboBox3.Items)
                {
                    if (row["dep_name"].ToString().Equals("عام", StringComparison.OrdinalIgnoreCase))
                    {
                        comboBox3.SelectedValue = row["department_id"];
                        comboBox3.Enabled = false; // منع التغيير
                        break;
                    }
                }
            }
            else
            {
                comboBox3.Enabled = true;
            }
        }



    }
}