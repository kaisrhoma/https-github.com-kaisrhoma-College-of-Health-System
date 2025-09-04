using Microsoft.VisualBasic;
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

namespace college_of_health_sciences.system_forms
{
    public partial class equation : Form
    {
        private int _studentId;
        private string _studentName;
        public equation(int studentId,string fullName)
        {
            InitializeComponent();
            _studentId = studentId;
            _studentName = fullName;
            this.Text = "معادلة مواد - " + fullName;
            label2.Text = fullName;
            FillDepartmentsAndYears(studentId);
            LoadStudentCourses("مسجل");
        }

        int GENERAL_DEPARTMENT_ID;
        public void datagridviewstyle(DataGridView datagrid)
        {
            datagrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            datagrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            datagrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }


        // نفترض أن لديك DataTable يحتوي على الأقسام
        private void FillDepartmentsAndYears(int studentId)
        {
            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    // 1️⃣ جلب جميع الأقسام
                    SqlCommand cmdDepartments = new SqlCommand("SELECT department_id, dep_name FROM Departments", con);
                    DataTable dtDepartments = new DataTable();
                    new SqlDataAdapter(cmdDepartments).Fill(dtDepartments);

                    comboBox2.DisplayMember = "dep_name";
                    comboBox2.ValueMember = "department_id";
                    comboBox2.DataSource = dtDepartments;

                    // جلب قسم الطالب لتعيينه كـ SelectedValue
                    SqlCommand cmdStudentDept = new SqlCommand("SELECT department_id, current_year FROM Students WHERE student_id = @studentId", con);
                    cmdStudentDept.Parameters.AddWithValue("@studentId", studentId);
                    using (SqlDataReader reader = cmdStudentDept.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int studentDeptId = reader.GetInt32(0);
                            int studentYear = reader.GetInt32(1);

                            comboBox2.SelectedValue = studentDeptId;

                            // 2️⃣ ملء comboBox1 بالسنوات
                            // ملء comboBox1 بالسنوات باستخدام DataSource
                            DataTable dtYears = new DataTable();
                            dtYears.Columns.Add("Year", typeof(int));
                            for (int y = 1; y <= 4; y++)
                            {
                                dtYears.Rows.Add(y);
                            }
                            comboBox1.DataSource = dtYears;
                            comboBox1.DisplayMember = "Year";
                            comboBox1.ValueMember = "Year";
                            comboBox1.SelectedValue = studentYear; // تعيين السنة الحالية للطالب

                        }
                    }
                    int month3;

                    using (SqlCommand cmddate = new SqlCommand("SELECT month_number FROM Months WHERE month_id = 1 ", con))
                    {
                        month3 = Convert.ToInt32(cmddate.ExecuteScalar());
                    }

                    int academicYearStart = DateTime.Now.Month >= month3 ? DateTime.Now.Year : DateTime.Now.Year - 1;
                    numericUpDown3.Value = academicYearStart;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء ملء القوائم: " + ex.Message);
            }
        }

        private bool checkNulls(SqlConnection con)
        {
            string query = @"
        SELECT COUNT(*) 
        FROM Grades 
        WHERE student_id = @studentId AND success_status IS NULL";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@studentId", _studentId);
                int failedCount = Convert.ToInt32(cmd.ExecuteScalar());

                if (failedCount > 0)
                {
                    MessageBox.Show("⚠ هناك درجات لم يتم إدخالها بعد !");
                    return false;
                }
            }

            return true;
        }

        private void LoadStudentCourses(string status)
        {
            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    
                    string q = @"
                SELECT r.registration_id,r.academic_year_start,c.course_id, c.course_name, c.course_code,c.year_number,
                       g.work_grade, g.final_grade, g.total_grade, g.success_status,
                       st.description
                FROM Registrations r
                JOIN Courses c ON c.course_id = r.course_id
                LEFT JOIN Grades g ON g.course_id = r.course_id AND g.student_id = r.student_id
                JOIN Students s ON s.student_id = r.student_id
                JOIN Status st ON st.status_id = s.status_id
                WHERE r.student_id = @studentId 
                AND r.status = @st

";


                    SqlCommand cmd = new SqlCommand(q, con);
                    cmd.Parameters.AddWithValue("@studentId", _studentId);
                    cmd.Parameters.AddWithValue("@st", status);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dataGridView1.DataSource = dt;

                    // إضافة CheckBox للحذف
                    if (!dataGridView1.Columns.Contains("Select"))
                    {
                        DataGridViewCheckBoxColumn chk = new DataGridViewCheckBoxColumn();
                        chk.Name = "Select";
                        chk.HeaderText = "تحديد";
                        dataGridView1.Columns.Insert(0, chk);
                    }
                    datagridviewstyle(dataGridView1); 
                    dataGridView1.Columns["registration_id"].Visible = false;
                    dataGridView1.Columns["course_id"].Visible = false;
                    dataGridView1.Columns["Select"].Width = 50;
                    dataGridView1.Columns["academic_year_start"].HeaderText = "العام الجامعي";
                    dataGridView1.Columns["course_name"].HeaderText = "اسم المادة";
                    dataGridView1.Columns["course_name"].ReadOnly = true;
                    dataGridView1.Columns["course_code"].HeaderText = "رمز المادة";
                    dataGridView1.Columns["course_code"].ReadOnly = true;
                    dataGridView1.Columns["year_number"].HeaderText = "السنة";
                    dataGridView1.Columns["year_number"].ReadOnly = true;
                    dataGridView1.Columns["work_grade"].HeaderText = "درجة الأعمال";
                    dataGridView1.Columns["final_grade"].HeaderText = "درجة الامتحان النهائي";
                    dataGridView1.Columns["total_grade"].HeaderText = "المجموع";
                    dataGridView1.Columns["success_status"].HeaderText = "الحالة";
                    dataGridView1.Columns["success_status"].ReadOnly = true;
                    dataGridView1.Columns["description"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // التحقق من اختيار السنة والقسم
                if (comboBox1.SelectedValue == null || comboBox2.SelectedValue == null)
                {
                    MessageBox.Show("⚠ يرجى اختيار السنة والقسم أولاً.");
                    return;
                }

                int newYear = Convert.ToInt32(comboBox1.SelectedValue);
                int newDeptId = Convert.ToInt32(comboBox2.SelectedValue);

                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(@"
UPDATE Students
SET current_year = @newYear,
    department_id = @newDeptId
WHERE student_id = @studentId", con))
                    {
                        cmd.Parameters.AddWithValue("@newYear", newYear);
                        cmd.Parameters.AddWithValue("@newDeptId", newDeptId);
                        cmd.Parameters.AddWithValue("@studentId", _studentId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("✅ تم تحديث السنة والقسم بنجاح.");
                        }
                        else
                        {
                            MessageBox.Show("⚠ لم يتم العثور على الطالب المحدد.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ: " + ex.Message);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            conn.DatabaseConnection db = new conn.DatabaseConnection();
            using (SqlConnection con = db.OpenConnection())
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    bool selected = row.Cells["Select"].Value != null && (bool)row.Cells["Select"].Value;
                    if (selected)
                    {
                        int regId = Convert.ToInt32(row.Cells["registration_id"].Value);

                        string deleteQuery = @"
                    DELETE FROM Grades WHERE student_id = @studentId AND course_id = (
                        SELECT course_id FROM Registrations WHERE registration_id = @regId
                    );
                    DELETE FROM Registrations WHERE registration_id = @regId;";

                        SqlCommand cmd = new SqlCommand(deleteQuery, con);
                        cmd.Parameters.AddWithValue("@studentId", _studentId);
                        cmd.Parameters.AddWithValue("@regId", regId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            LoadStudentCourses("مسجل");
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || dataGridView1.Rows[e.RowIndex].IsNewRow) return;

                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                object workObj = row.Cells["work_grade"].Value ?? 0;
                object finalObj = row.Cells["final_grade"].Value ?? 0;

                int work = 0, final = 0;
                int.TryParse(workObj.ToString(), out work);
                int.TryParse(finalObj.ToString(), out final);

                int total = work + final;
                row.Cells["total_grade"].Value = total;

                // 🔴 مافيش تغيير في success_status هنا
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في الحساب: " + ex.Message);
            }
        }


        private void button13_Click(object sender, EventArgs e)
        {
            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    
                        // رسالة تأكيد عادية
                        var confirmSave = MessageBox.Show(
                            "هل تريد حفظ الدرجات وتحديث العام الجامعي؟",
                            "تأكيد",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (confirmSave != DialogResult.Yes)
                            return;
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.IsNewRow) continue;

                        int work = 0, final = 0;

                        // التحقق من قيم العمل
                        if (!int.TryParse(row.Cells["work_grade"].Value?.ToString(), out work) || work < 0 || work > 40)
                        {
                            MessageBox.Show("⚠ درجة الأعمال يجب أن تكون بين 0 و 40");
                            row.Cells["work_grade"].Selected = true;
                            return;
                        }

                        if (!int.TryParse(row.Cells["final_grade"].Value?.ToString(), out final) || final < 0 || final > 60)
                        {
                            MessageBox.Show("⚠ درجة النهائي يجب أن تكون بين 0 و 60");
                            row.Cells["final_grade"].Selected = true;
                            return;
                        }

                        int total = work + final;
                        row.Cells["total_grade"].Value = total;

                        // تحديد النجاح أو الرسوب
                        string status = total >= 60 ? "نجاح" : "راسب";
                        row.Cells["success_status"].Value = status;

                        // قراءة قيمة العام الجامعي (قد تكون null)
                        object academicYearValue = row.Cells["academic_year_start"].Value;
                        object dbAcademicYear;

                        if (academicYearValue == null || string.IsNullOrWhiteSpace(academicYearValue.ToString()))
                        {
                            // رسالة تأكيد خاصة بالـ NULL
                            var confirmNull = MessageBox.Show(
                                "⚠ لم يتم إدخال العام الجامعي.\nهل تريد حفظه كـ NULL؟",
                                "تأكيد الحفظ كـ NULL",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning);

                            if (confirmNull != DialogResult.Yes)
                                return; // المستخدم رفض الحفظ كـ NULL

                            dbAcademicYear = DBNull.Value;
                        }
                        else
                        {
                            dbAcademicYear = Convert.ToInt32(academicYearValue);
                        }

                            using (SqlCommand cmd = new SqlCommand(@"
 -- تحديث أو إدخال الدرجات
 IF EXISTS (SELECT 1 FROM Grades WHERE student_id = @studentId AND course_id = @courseId)
 BEGIN
     UPDATE Grades
     SET work_grade = @work, final_grade = @final,
         total_grade = @total, success_status = @status
     WHERE student_id = @studentId AND course_id = @courseId
 END
 ELSE
 BEGIN
     INSERT INTO Grades(student_id, course_id, work_grade, final_grade, total_grade, success_status)
     VALUES (@studentId, @courseId, @work, @final, @total, @status)
 END;

 -- تحديث العام الجامعي في الـ Registrations
 UPDATE Registrations
 SET academic_year_start = @academicYearStart
 WHERE student_id = @studentId AND course_id = @courseId;
", con))
                            {
                                cmd.Parameters.AddWithValue("@work", work);
                                cmd.Parameters.AddWithValue("@final", final);
                                cmd.Parameters.AddWithValue("@total", total);
                                cmd.Parameters.AddWithValue("@status", status);
                                cmd.Parameters.AddWithValue("@studentId", _studentId);
                                cmd.Parameters.AddWithValue("@courseId", row.Cells["course_id"].Value);
                                cmd.Parameters.AddWithValue("@academicYearStart", dbAcademicYear);

                                cmd.ExecuteNonQuery();
                            }


                    }

                    MessageBox.Show("✅ تم الحفظ وتحديث الحالات بنجاح");
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error " + ex.Message);
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



        private int GetOrCreateGroup(SqlConnection con, int courseId, int? academicYearStart, int departmentid)
        {
            int groupId = 0;

            // ✅ جلب كل المجموعات المرتبطة بالمادة
            SqlCommand getGroupsCmd = new SqlCommand(@"
        SELECT cc.id, cc.capacity, cc.group_number
        FROM Course_Classroom cc
        WHERE cc.course_id = @courseId AND cc.department_id = @departmentid
        ORDER BY cc.group_number;", con);
            getGroupsCmd.Parameters.AddWithValue("@courseId", courseId);
            getGroupsCmd.Parameters.AddWithValue("@departmentid", departmentid);

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
            (course_id, classroom_id, group_number, capacity, start_time, end_time, lecture_day, instructor_id, department_id)
            OUTPUT INSERTED.id 
            VALUES (@courseId, @classroomId, @groupNumber, 80, '09:00:00', '12:00:00',6, @instructorId, @departmentid)", con))
                {
                    cmd.Parameters.AddWithValue("@courseId", courseId);
                    cmd.Parameters.AddWithValue("@classroomId", classroomId);
                    cmd.Parameters.AddWithValue("@groupNumber", nextGroupNumber);
                    cmd.Parameters.AddWithValue("@instructorId", instructorId);
                    cmd.Parameters.AddWithValue("@departmentid", departmentid);

                    groupId = (int)cmd.ExecuteScalar();
                }
            }

            return groupId;
        }


        private void DownloadCoursesForStudent(SqlConnection con, int studentId, int year, int deptId, int academicYear)
        {
            // جلب المواد للسنة والقسم
            DataTable courses = new DataTable();
            using (SqlCommand cmd = new SqlCommand(@"
        SELECT c.course_id, c.course_name
        FROM Courses c
        JOIN Course_Department cd ON cd.course_id = c.course_id
        WHERE c.year_number = @year AND cd.department_id = @dept", con))
            {
                cmd.Parameters.AddWithValue("@year", year);
                cmd.Parameters.AddWithValue("@dept", deptId);
                new SqlDataAdapter(cmd).Fill(courses);
            }

            foreach (DataRow row in courses.Rows)
            {
                int courseId = Convert.ToInt32(row["course_id"]);
                int groupId = GetOrCreateGroup(con, courseId, academicYear,deptId);

                using (SqlCommand cmd = new SqlCommand(@"
            IF NOT EXISTS (SELECT 1 FROM Registrations WHERE student_id=@studentId AND course_id=@courseId)
            INSERT INTO Registrations (student_id, course_id, year_number, status, course_classroom_id, academic_year_start)
            VALUES (@studentId, @courseId, @year, N'مسجل', @groupId, @academicYear)", con))
                {
                    cmd.Parameters.AddWithValue("@studentId", studentId);
                    cmd.Parameters.AddWithValue("@courseId", courseId);
                    cmd.Parameters.AddWithValue("@year", year);
                    cmd.Parameters.AddWithValue("@groupId", groupId);
                    cmd.Parameters.AddWithValue("@academicYear", academicYear);
                    cmd.ExecuteNonQuery();
                }
            }
        }

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


        private string GetStudentExamRound(SqlConnection con, int studentId)
        {
            string query = @"
        SELECT COUNT(*) 
        FROM Grades 
        WHERE student_id = @studentId AND success_status = N'راسب'";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@studentId", studentId);
                int failedCount = Convert.ToInt32(cmd.ExecuteScalar());

                if (failedCount == 0)
                    return "مكتمل";
                else if (failedCount == 1 || failedCount == 2)
                    return "مرحل";
                else
                    return "إعادة سنة";
            }
        }

        // 2️⃣ مرحل: تحديث المواد الراسبة، تصفير الدرجات، ترقية للسنة الثانية + تنزيل مواد جديدة
        private void PromoteRepeaterStudent(SqlConnection con, int studentId, int deptId, int academicYear,int cuYear)
        {
            // جلب المواد الراسبة في السنة الأولى
            string failQuery = @"
        SELECT r.course_id
        FROM Registrations r
        JOIN Grades g ON r.course_id = g.course_id AND r.student_id = g.student_id
        WHERE r.student_id = @studentId AND g.success_status = N'راسب'";
            DataTable dtFail = new DataTable();
            using (SqlCommand cmd = new SqlCommand(failQuery, con))
            {
                cmd.Parameters.AddWithValue("@studentId", studentId);
                new SqlDataAdapter(cmd).Fill(dtFail);
            }
            // تحديث المواد الراسبة: تصفير الدرجات وتحديث السنة الجامعية والمجموعات
            foreach (DataRow fail in dtFail.Rows)
            {
                int courseId = Convert.ToInt32(fail["course_id"]);
                int groupId = GetOrCreateGroup(con, courseId, academicYear, deptId);

                using (SqlCommand cmdUpdate = new SqlCommand(@"
            UPDATE Registrations 
            SET academic_year_start = @academicYear, course_classroom_id = @groupId
            WHERE student_id = @studentId AND course_id = @courseId;

            UPDATE Grades
            SET final_grade = NULL, work_grade = NULL, total_grade = NULL, success_status = NULL
            WHERE student_id = @studentId AND course_id = @courseId;", con))
                {
                    cmdUpdate.Parameters.AddWithValue("@studentId", studentId);
                    cmdUpdate.Parameters.AddWithValue("@courseId", courseId);
                    cmdUpdate.Parameters.AddWithValue("@academicYear", academicYear);
                    cmdUpdate.Parameters.AddWithValue("@groupId", groupId);
                    cmdUpdate.ExecuteNonQuery();
                }
            }
            DownloadCoursesForStudent(con, studentId, cuYear, deptId, academicYear);
        }


        private void RepeatStudent(SqlConnection con, int studentId, int academicYear,int cuYear,int depId)
        {
            // جلب المواد الراسبة
            string failQuery = @"
        SELECT r.course_id
        FROM Registrations r
        JOIN Grades g ON r.course_id = g.course_id AND r.student_id = g.student_id
        WHERE r.student_id = @studentId AND g.success_status = N'راسب'";
            DataTable dtFail = new DataTable();
            using (SqlCommand cmd = new SqlCommand(failQuery, con))
            {
                cmd.Parameters.AddWithValue("@studentId", studentId);
                new SqlDataAdapter(cmd).Fill(dtFail);
            }
            foreach (DataRow fail in dtFail.Rows)
            {
                int courseId = Convert.ToInt32(fail["course_id"]);
                int groupId = GetOrCreateGroup(con, courseId, academicYear,depId);

                using (SqlCommand cmdUpdate = new SqlCommand(@"
            UPDATE Registrations 
            SET academic_year_start = @academicYear, course_classroom_id = @groupId
            WHERE student_id = @studentId AND course_id = @courseId;

            UPDATE Grades
            SET final_grade = NULL, work_grade = NULL, total_grade = NULL, success_status = NULL
            WHERE student_id = @studentId AND course_id = @courseId;", con))
                {
                    cmdUpdate.Parameters.AddWithValue("@studentId", studentId);
                    cmdUpdate.Parameters.AddWithValue("@courseId", courseId);
                    cmdUpdate.Parameters.AddWithValue("@academicYear", academicYear);
                    cmdUpdate.Parameters.AddWithValue("@groupId", groupId);
                    cmdUpdate.ExecuteNonQuery();
                }
            }

            if (cuYear == 2)
            {
                // تغيير قسم الطالب إلى العام
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Students SET department_id = @generalDept,current_year = current_year - 1 WHERE student_id = @studentId", con))
                {
                    cmd.Parameters.AddWithValue("@generalDept", GENERAL_DEPARTMENT_ID);
                    cmd.Parameters.AddWithValue("@studentId", studentId);
                    cmd.ExecuteNonQuery();
                }
            } else if (cuYear > 2) 
                {
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Students SET current_year = current_year - 1 WHERE student_id = @studentId", con))
                {
                    cmd.Parameters.AddWithValue("@studentId", studentId);
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void PromoteStudent()
        {

            int academicYear = (int)numericUpDown3.Value;

            using (SqlConnection con = new conn.DatabaseConnection().OpenConnection())
            {
                if (!checkNulls(con))
                    return;
                GENERAL_DEPARTMENT_ID = GetGeneralDepartmentId(con);

                string q = @"SELECT department_id, current_year 
                     FROM Students 
                     WHERE student_id = @studentId";
                // ✅ نحسب exam_round من الدرجات الفعلية
                string examRound = GetStudentExamRound(con, _studentId);
                int deptId = 0;
                int currentYear = 0;
                using (SqlCommand cmd = new SqlCommand(q, con))
                {
                    cmd.Parameters.AddWithValue("@studentId", _studentId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            deptId = reader.GetInt32(0);
                            currentYear = reader.GetInt32(1);
                        }
                    }
                            // 🚀 ترقية حسب الحالة
                            switch (examRound)
                            {
                                case "مكتمل":
                                    DownloadCoursesForStudent(con, _studentId, currentYear, deptId, academicYear);
                                    break;

                                case "مرحل":
                                    PromoteRepeaterStudent(con,_studentId, deptId, academicYear,currentYear);
                                    break;

                                case "إعادة سنة":
                                    RepeatStudent(con, _studentId, academicYear,currentYear,deptId);
                                    break;
                            }
             
                }
                using (SqlCommand cmd = new SqlCommand("UPDATE Students SET status_id = @statusid  WHERE student_id = @studentId", con))
                {
                    int statusidst = GetStatusId(con, "مستمر");
                    cmd.Parameters.AddWithValue("@statusid", statusidst);
                    cmd.Parameters.AddWithValue("@studentid", _studentId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ تمت ترقية الطالب بنجاح.");
            }
        }

        public bool CheckConstrains()
        {
            try
            {
                conn.DatabaseConnection dbCheck = new conn.DatabaseConnection();
                using (SqlConnection con = dbCheck.OpenConnection())
                {
                    int monthStart = 0;
                    bool isApproved = false;

                    using (SqlCommand cmdMonth = new SqlCommand(
                        "SELECT month_number, is_approved FROM Months WHERE month_id = @id", con))
                    {
                        cmdMonth.Parameters.AddWithValue("@id", 1);
                        DataTable dt = new DataTable();
                        new SqlDataAdapter(cmdMonth).Fill(dt);
                        monthStart = Convert.ToInt32(dt.Rows[0]["month_number"]);
                    }

                    // التحقق من بداية السنة
                    if (DateTime.Now.Month < monthStart)
                    {
                        MessageBox.Show("لا يمكن الترقية قبل بداية السنة الدراسية الجديدة.");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ: " + ex.Message);
                return false;
            }

            // إدخال رمز التحقق
            string input = Interaction.InputBox("تأكيد الترقية", "ادخل الرمز للتأكيد", "");
            string confirmCode = "2025";

            if (input != confirmCode)
            {
                MessageBox.Show("رمز خاطئ، يرجى إعادة المحاولة.");
                return false;
            }

            return true;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (!CheckConstrains())
                return;
            int academicYearStart = (int)numericUpDown3.Value;
            DialogResult dr = MessageBox.Show(
            "AcademicYearStart = " + academicYearStart + "\n\nهل تريد الاستمرار في الترقية؟",
            "تأكيد الترقية",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

            if (dr == DialogResult.No)
            {
                return; // يوقف العملية إذا اخترت لا
            }
            PromoteStudent();
            LoadStudentCourses("مسجل");
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            string status = comboBox3.SelectedItem.ToString();
            LoadStudentCourses(status); 
        }

        private void equation_Load(object sender, EventArgs e)
        {
            comboBox3.SelectedIndexChanged -= comboBox3_SelectedIndexChanged;
            comboBox3.Items.Clear(); // تنظيف أي عناصر موجودة
            comboBox3.Items.Add("مسجل");
            comboBox3.Items.Add("سابق");

            // تعيين القيمة الافتراضية (اختياري)
            comboBox3.SelectedIndex = 0;
            comboBox3.SelectedIndexChanged += comboBox3_SelectedIndexChanged;
        }
    }
}
