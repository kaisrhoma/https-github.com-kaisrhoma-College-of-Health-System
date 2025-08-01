using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;


namespace college_of_health_sciences.dashboards.exams_dashboards
{
    public partial class grads_management : UserControl
    {

        private readonly string connectionString = @"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;";
        // SqlConnection conn = new SqlConnection("Data Source=.;Initial Catalog=Cohs_DB;Integrated Security=True");

        private PrintDocument printDocument1 = new PrintDocument();
        private DataTable reportData;
        public grads_management()
        {
            InitializeComponent();
            this.Load += new System.EventHandler(this.grads_management_Load);
            LoadDepartments();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT course_id, course_name FROM Courses";
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

              
                comboBox_Year.Items.Add("1");
                comboBox_Year.Items.Add("2");
                comboBox_Year.Items.Add("3");
                comboBox_Year.Items.Add("4");
               // printDocument1.PrintPage += printDocument1_PrintPage;
                comboBox_Year.SelectedIndex = 0;
                dataGridView2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;


                dataGridViewGrades.CellValueChanged += dataGridViewGrades_CellValueChanged;
                dataGridViewGrades.CurrentCellDirtyStateChanged += dataGridViewGrades_CurrentCellDirtyStateChanged;
            }

        }

        private void LoadDepartments()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter("SELECT department_id, dep_name FROM Departments", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    comboDepartment.DisplayMember = "dep_name";
                    comboDepartment.ValueMember = "department_id";
                    comboDepartment.DataSource = dt;

                    dataGridViewGrades.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridViewGrades.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء تحميل الأقسام: " + ex.Message);
            }
        }


        private void LoadCourses(int departmentId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(
                        @"SELECT c.course_id, c.course_name 
                  FROM Courses c
                  INNER JOIN Course_Department cd ON c.course_id = cd.course_id
                  WHERE cd.department_id = @deptId", conn);

                    da.SelectCommand.Parameters.AddWithValue("@deptId", departmentId);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    comboCourse.DisplayMember = "course_name";
                    comboCourse.ValueMember = "course_id";
                    comboCourse.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء تحميل المواد: " + ex.Message);
            }
        }


        private void LoadStudents(int courseId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
                {
                    conn.Open();

                    string query = @"
            SELECT 
                s.university_number AS [رقم القيد],
                s.full_name AS [اسم الطالب],
                CAST(g.work_grade AS NVARCHAR) AS [درجة الأعمال],
                CAST(g.final_grade AS NVARCHAR) AS [درجة الامتحان النهائي],
                CAST(g.total_grade AS NVARCHAR) AS [المجموع الكلي]
            FROM Students s
            INNER JOIN Registrations r ON s.student_id = r.student_id
            LEFT JOIN Grades g ON s.student_id = g.student_id AND g.course_id = r.course_id
            WHERE r.course_id = @courseId 
              AND r.status = N'مسجل'
              AND (
                  g.student_id IS NULL 
                  OR (g.work_grade IS NULL AND g.final_grade IS NULL)
              )
            ORDER BY s.student_id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@courseId", courseId);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // تنظيف المصدر القديم قبل التعيين
                        dataGridViewGrades.DataSource = null;
                        dataGridViewGrades.Rows.Clear();

                        // استبدال NULL بـ "لم ترصد"
                        foreach (DataRow row in dt.Rows)
                        {
                            if (row["درجة الأعمال"] == DBNull.Value || string.IsNullOrWhiteSpace(row["درجة الأعمال"].ToString()))
                                row["درجة الأعمال"] = "لم ترصد";

                            if (row["درجة الامتحان النهائي"] == DBNull.Value || string.IsNullOrWhiteSpace(row["درجة الامتحان النهائي"].ToString()))
                                row["درجة الامتحان النهائي"] = "لم ترصد";

                            if (row["المجموع الكلي"] == DBNull.Value || string.IsNullOrWhiteSpace(row["المجموع الكلي"].ToString()))
                                row["المجموع الكلي"] = "لم ترصد";
                        }

                        dataGridViewGrades.DataSource = dt;

                        // ضبط خصائص الأعمدة (القراءة، الحجم)
                        dataGridViewGrades.Columns["رقم القيد"].ReadOnly = true;
                        dataGridViewGrades.Columns["اسم الطالب"].ReadOnly = true;
                        dataGridViewGrades.Columns["المجموع الكلي"].ReadOnly = true;

                        dataGridViewGrades.Columns["درجة الأعمال"].ReadOnly = false;
                        dataGridViewGrades.Columns["درجة الامتحان النهائي"].ReadOnly = false;

                        dataGridViewGrades.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        dataGridViewGrades.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء تحميل الطلاب: " + ex.Message);
            }
        }






        private void grads_management_Load(object sender, EventArgs e)
        {
          
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void comboBoxDepartments_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (comboDepartment.SelectedValue != null && int.TryParse(comboDepartment.SelectedValue.ToString(), out int departmentId))
            {
                LoadCourses(departmentId);
            }



        }

        private void button1_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
            //    {
            //        conn.Open();
            //        int updatedCount = 0, skippedCount = 0;

            //        foreach (DataGridViewRow row in dataGridViewGrades.Rows)
            //        {
            //            if (row.IsNewRow) continue;

            //            // استخراج رقم القيد بدلاً من رقم_الجامعية
            //            string universityNumber = row.Cells["رقم القيد"].Value?.ToString();

            //            if (string.IsNullOrEmpty(universityNumber))
            //            {
            //                skippedCount++;
            //                continue;
            //            }

            //            // استخراج student_id من قاعدة البيانات بناءً على رقم القيد
            //            int studentId = -1;
            //            string getIdQuery = "SELECT student_id FROM Students WHERE university_number = @uniNum";
            //            using (SqlCommand getIdCmd = new SqlCommand(getIdQuery, conn))
            //            {
            //                getIdCmd.Parameters.AddWithValue("@uniNum", universityNumber);
            //                object result = getIdCmd.ExecuteScalar();
            //                if (result == null)
            //                {
            //                    skippedCount++;
            //                    continue;
            //                }
            //                studentId = Convert.ToInt32(result);
            //            }

            //            int courseId = Convert.ToInt32(comboCourse.SelectedValue);

            //            // محاولة قراءة الدرجات، إذا كانت فارغة يتم اعتبارها 0
            //            int.TryParse(row.Cells["درجة الأعمال"].Value?.ToString(), out int workGrade);
            //            int.TryParse(row.Cells["درجة الامتحان النهائي"].Value?.ToString(), out int finalGrade);
            //            int totalGrade = workGrade + finalGrade;

            //            // تحقق من وجود سجل سابق
            //            string checkQuery = "SELECT COUNT(*) FROM Grades WHERE student_id = @studentId AND course_id = @courseId";
            //            using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
            //            {
            //                checkCmd.Parameters.AddWithValue("@studentId", studentId);
            //                checkCmd.Parameters.AddWithValue("@courseId", courseId);
            //                int exists = (int)checkCmd.ExecuteScalar();

            //                if (exists > 0)
            //                {
            //                    // تحديث البيانات
            //                    string updateQuery = @"
            //                UPDATE Grades 
            //                SET work_grade = @workGrade,
            //                    final_grade = @finalGrade,
            //                    total_grade = @totalGrade,
            //                    success_status = CASE WHEN @totalGrade >= 50 THEN N'نجاح' ELSE N'رسوب' END
            //                WHERE student_id = @studentId AND course_id = @courseId";

            //                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
            //                    {
            //                        updateCmd.Parameters.AddWithValue("@workGrade", workGrade);
            //                        updateCmd.Parameters.AddWithValue("@finalGrade", finalGrade);
            //                        updateCmd.Parameters.AddWithValue("@totalGrade", totalGrade);
            //                        updateCmd.Parameters.AddWithValue("@studentId", studentId);
            //                        updateCmd.Parameters.AddWithValue("@courseId", courseId);
            //                        updateCmd.ExecuteNonQuery();
            //                    }

            //                    // تسجيل في سجل العمليات
            //                    string auditQuery = @"
            //                INSERT INTO Audit_Log (user_id, action, table_name, record_id)
            //                VALUES (@userId, 'UPDATE', 'Grades', @recordId)";
            //                    using (SqlCommand auditCmd = new SqlCommand(auditQuery, conn))
            //                    {
            //                        auditCmd.Parameters.AddWithValue("@userId", Session.userID);
            //                        auditCmd.Parameters.AddWithValue("@recordId", studentId);
            //                        auditCmd.ExecuteNonQuery();
            //                    }

            //                    updatedCount++;
            //                }
            //                else
            //                {
            //                    // إنشاء سجل جديد
            //                    string insertQuery = @"
            //                INSERT INTO Grades (student_id, course_id, work_grade, final_grade, total_grade, success_status)
            //                VALUES (@studentId, @courseId, @workGrade, @finalGrade, @totalGrade, 
            //                CASE WHEN @totalGrade >= 50 THEN N'نجاح' ELSE N'رسوب' END)";
            //                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
            //                    {
            //                        insertCmd.Parameters.AddWithValue("@studentId", studentId);
            //                        insertCmd.Parameters.AddWithValue("@courseId", courseId);
            //                        insertCmd.Parameters.AddWithValue("@workGrade", workGrade);
            //                        insertCmd.Parameters.AddWithValue("@finalGrade", finalGrade);
            //                        insertCmd.Parameters.AddWithValue("@totalGrade", totalGrade);
            //                        insertCmd.ExecuteNonQuery();
            //                    }

            //                    updatedCount++;
            //                }

            //            }
            //        }

            //        MessageBox.Show($"✅ تم حفظ الدرجات بنجاح:\n📥 تم تحديث/إدخال: {updatedCount}\n⏭ تم تخطي: {skippedCount}");
            //    }

            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("❌ خطأ أثناء حفظ الدرجات:\n" + ex.Message);
            //}

            try
            {
                using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
                {
                    conn.Open();
                    int insertedCount = 0;
                    int updatedCount = 0;
                    int skippedCount = 0;

                    foreach (DataGridViewRow row in dataGridViewGrades.Rows)
                    {
                        if (row.IsNewRow) continue;

                        string universityNumber = row.Cells["رقم القيد"].Value?.ToString();
                        string studentName = row.Cells["اسم الطالب"].Value?.ToString();
                        int courseId = Convert.ToInt32(comboCourse.SelectedValue);

                        string workGradeStr = row.Cells["درجة الأعمال"].Value?.ToString().Trim();
                        string finalGradeStr = row.Cells["درجة الامتحان النهائي"].Value?.ToString().Trim();

                        // إذا كانت كلتا الدرجتين غير مدخلتين أو "لم ترصد" لا نحفظ شيء
                        bool isWorkGradeValid = !(string.IsNullOrEmpty(workGradeStr) || workGradeStr == "لم ترصد");
                        bool isFinalGradeValid = !(string.IsNullOrEmpty(finalGradeStr) || finalGradeStr == "لم ترصد");

                        if (!isWorkGradeValid && !isFinalGradeValid)
                        {
                            skippedCount++;
                            continue; // لا تدخل أو تحدث سجل إذا لم تدخل أي درجة
                        }

                        int workGrade = 0;
                        int finalGrade = 0;

                        if (isWorkGradeValid && !int.TryParse(workGradeStr, out workGrade))
                        {
                            MessageBox.Show($"⚠️ قيمة غير صالحة في درجة الأعمال للطالب: {studentName}");
                            continue;
                        }

                        if (isFinalGradeValid && !int.TryParse(finalGradeStr, out finalGrade))
                        {
                            MessageBox.Show($"⚠️ قيمة غير صالحة في درجة الامتحان النهائي للطالب: {studentName}");
                            continue;
                        }

                        // تحقق من صحة الدرجات إذا كانت مدخلة
                        if (isWorkGradeValid && (workGrade < 0 || workGrade > 40))
                        {
                            MessageBox.Show($"⚠️ درجة الأعمال يجب أن تكون بين 0 و 40 للطالب: {studentName}");
                            continue;
                        }
                        if (isFinalGradeValid && (finalGrade < 0 || finalGrade > 60))
                        {
                            MessageBox.Show($"⚠️ درجة الامتحان النهائي يجب أن تكون بين 0 و 60 للطالب: {studentName}");
                            continue;
                        }

                        int totalGrade = workGrade + finalGrade;

                        // جلب student_id حسب رقم القيد
                        string studentIdQuery = "SELECT student_id FROM Students WHERE university_number = @uniNumber";
                        int studentId = -1;
                        using (SqlCommand cmdStudentId = new SqlCommand(studentIdQuery, conn))
                        {
                            cmdStudentId.Parameters.AddWithValue("@uniNumber", universityNumber);
                            var res = cmdStudentId.ExecuteScalar();
                            if (res != null)
                                studentId = Convert.ToInt32(res);
                            else
                                continue; // إذا لم يوجد الطالب، تخطى
                        }

                        // تحقق هل السجل موجود وماذا قيم الدرجات
                        string checkGradesQuery = @"
                SELECT work_grade, final_grade FROM Grades 
                WHERE student_id = @studentId AND course_id = @courseId";

                        using (SqlCommand checkGradesCmd = new SqlCommand(checkGradesQuery, conn))
                        {
                            checkGradesCmd.Parameters.AddWithValue("@studentId", studentId);
                            checkGradesCmd.Parameters.AddWithValue("@courseId", courseId);

                            using (SqlDataReader reader = checkGradesCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    // سجل موجود
                                    object dbWorkGradeObj = reader["work_grade"];
                                    object dbFinalGradeObj = reader["final_grade"];

                                    int dbWorkGrade = (dbWorkGradeObj == DBNull.Value) ? -1 : Convert.ToInt32(dbWorkGradeObj);
                                    int dbFinalGrade = (dbFinalGradeObj == DBNull.Value) ? -1 : Convert.ToInt32(dbFinalGradeObj);

                                    // يسمح بالتحديث فقط إذا الدرجات القديمة NULL أو -1 (أي غير موجودة فعلياً)
                                    bool allowUpdate = (dbWorkGrade == -1 || dbWorkGrade == 0) && (dbFinalGrade == -1 || dbFinalGrade == 0);

                                    if (allowUpdate)
                                    {
                                        // تحديث السجل
                                        string updateQuery = @"
                                UPDATE Grades 
                                SET work_grade = @workGrade,
                                    final_grade = @finalGrade,
                                    total_grade = @totalGrade,
                                    success_status = CASE WHEN @totalGrade >= 50 THEN N'نجاح' ELSE N'رسوب' END
                                WHERE student_id = @studentId AND course_id = @courseId";

                                        reader.Close(); // يجب إغلاق القارئ قبل تنفيذ أمر آخر

                                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                                        {
                                            updateCmd.Parameters.AddWithValue("@studentId", studentId);
                                            updateCmd.Parameters.AddWithValue("@courseId", courseId);
                                            updateCmd.Parameters.AddWithValue("@workGrade", workGrade);
                                            updateCmd.Parameters.AddWithValue("@finalGrade", finalGrade);
                                            updateCmd.Parameters.AddWithValue("@totalGrade", totalGrade);
                                            updateCmd.ExecuteNonQuery();
                                        }

                                        // سجل التحديث في Audit_Log
                                        string auditQuery = @"
                                INSERT INTO Audit_Log (user_id, action, table_name, record_id)
                                VALUES (@userId, 'UPDATE', 'Grades', @recordId)";
                                        using (SqlCommand auditCmd = new SqlCommand(auditQuery, conn))
                                        {
                                            auditCmd.Parameters.AddWithValue("@userId", Session.userID);
                                            auditCmd.Parameters.AddWithValue("@recordId", studentId);
                                            auditCmd.ExecuteNonQuery();
                                        }

                                        updatedCount++;
                                    }
                                    else
                                    {
                                        // درجات موجودة وغير صفرية، لا نسمح بالتعديل
                                        skippedCount++;
                                    }
                                }
                                else
                                {
                                    // سجل غير موجود => إدخال جديد فقط إذا تم إدخال درجات صحيحة
                                    reader.Close();

                                    string insertQuery = @"
                            INSERT INTO Grades (student_id, course_id, work_grade, final_grade, total_grade, success_status)
                            VALUES (@studentId, @courseId, @workGrade, @finalGrade, @totalGrade,
                                    CASE WHEN @totalGrade >= 50 THEN N'نجاح' ELSE N'رسوب' END)";

                                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                                    {
                                        insertCmd.Parameters.AddWithValue("@studentId", studentId);
                                        insertCmd.Parameters.AddWithValue("@courseId", courseId);
                                        insertCmd.Parameters.AddWithValue("@workGrade", workGrade);
                                        insertCmd.Parameters.AddWithValue("@finalGrade", finalGrade);
                                        insertCmd.Parameters.AddWithValue("@totalGrade", totalGrade);
                                        insertCmd.ExecuteNonQuery();
                                    }

                                    // سجل الإدخال في Audit_Log
                                    string auditQuery = @"
                            INSERT INTO Audit_Log (user_id, action, table_name, record_id)
                            VALUES (@userId, 'INSERT', 'Grades', @recordId)";
                                    using (SqlCommand auditCmd = new SqlCommand(auditQuery, conn))
                                    {
                                        auditCmd.Parameters.AddWithValue("@userId", Session.userID);
                                        auditCmd.Parameters.AddWithValue("@recordId", studentId);
                                        auditCmd.ExecuteNonQuery();
                                    }

                                    insertedCount++;
                                }
                            }
                        }
                    }

                    MessageBox.Show($"✅ تم الحفظ:\n📥 تم الإدخال: {insertedCount}\n✏️ تم التحديث: {updatedCount}\n⏭ تم التخطي: {skippedCount}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ خطأ أثناء الحفظ:\n" + ex.Message);
            }

        }

        private void comboBoxCourse_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboCourse.SelectedValue != null && int.TryParse(comboCourse.SelectedValue.ToString(), out int courseId))
            {
                LoadStudents(courseId);
            }
        }
        //-------------------------------------------------------------------------------------------------2

       
        private void button3_Click(object sender, EventArgs e)
        {
            string universityNumber = txtUniversityNumber.Text.Trim();

            if (string.IsNullOrEmpty(universityNumber))
            {
                MessageBox.Show("من فضلك أدخل رقم الجامعة للبحث.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // استعلام لجلب بيانات الطالب مع المواد التي يسجلها والسنة، والدرجات (إن وجدت)
                    string query = @"
                SELECT 
                    s.university_number AS [رقم الجامعة],
                    s.full_name AS [اسم الطالب],
                    c.course_name AS [اسم المادة],
                    r.year_number AS [السنة الدراسية],
                    ISNULL(g.work_grade, 0) AS [درجة الأعمال],
                    ISNULL(g.final_grade, 0) AS [الدرجة النهائية],
                    ISNULL(g.total_grade, 0) AS [المجموع الكلي],
                    ISNULL(g.success_status, N'غير محدد') AS [الحالة],
                    g.grade_id
                FROM Students s
                INNER JOIN Registrations r ON s.student_id = r.student_id
                INNER JOIN Courses c ON r.course_id = c.course_id
                LEFT JOIN Grades g ON s.student_id = g.student_id AND c.course_id = g.course_id
                WHERE s.university_number = @universityNumber AND r.status = N'مسجل'
                ORDER BY r.year_number, c.course_name";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@universityNumber", universityNumber);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count == 0)
                        {
                            MessageBox.Show("لا يوجد بيانات لهذا الرقم الجامعي أو الطالب غير مسجل في أي مادة.");
                            dataGridView2.DataSource = null;
                            return;
                        }

                        dataGridView2.DataSource = dt;

                        // الأعمدة غير قابلة للتعديل إلا درجات الأعمال والنهائية فقط
                        foreach (DataGridViewColumn col in dataGridView2.Columns)
                        {
                            if (col.Name == "درجة الأعمال" || col.Name == "الدرجة النهائية")
                                col.ReadOnly = false;
                            else
                                col.ReadOnly = true;
                        }

                        // إخفاء عمود grade_id لكن نحتاجه للتحديث
                        if (dataGridView2.Columns.Contains("grade_id"))
                            dataGridView2.Columns["grade_id"].Visible = false;

                        dataGridViewGrades.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء البحث: " + ex.Message);
            }


        }



        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
               SELECT 
    s.university_number AS [رقم الجامعة],
    s.full_name AS [اسم الطالب],
    c.course_name AS [اسم المادة],
    r.year_number AS [السنة الدراسية],
    g.work_grade AS [درجة الأعمال],          -- بدون ISNULL
    g.final_grade AS [الدرجة النهائية],       -- بدون ISNULL
    g.total_grade AS [المجموع الكلي],          -- بدون ISNULL
    g.success_status AS [الحالة],              -- بدون ISNULL
    g.grade_id
FROM Students s
INNER JOIN Registrations r ON s.student_id = r.student_id
INNER JOIN Courses c ON r.course_id = c.course_id
LEFT JOIN Grades g ON s.student_id = g.student_id AND c.course_id = g.course_id
WHERE r.status = N'مسجل'
ORDER BY s.university_number, r.year_number, c.course_name;
";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dataGridView2.DataSource = dt;

                    foreach (DataGridViewColumn col in dataGridView2.Columns)
                    {
                        if (col.Name == "درجة الأعمال" || col.Name == "الدرجة النهائية")
                            col.ReadOnly = false;
                        else
                            col.ReadOnly = true;
                    }

                    if (dataGridView2.Columns.Contains("grade_id"))
                        dataGridView2.Columns["grade_id"].Visible = false;

                    dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء تحميل البيانات: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    int updatedCount = 0;
                    int skippedCount = 0;

                    foreach (DataGridViewRow row in dataGridView2.Rows)
                    {
                        if (row.IsNewRow) continue;

                        if (!int.TryParse(row.Cells["grade_id"].Value?.ToString(), out int gradeId))
                            continue; // لو grade_id غير موجود أو غير صالح، تخطي

                        // القيم التي قد تكون null أو فارغة
                        string workGradeStr = row.Cells["درجة الأعمال"].Value?.ToString();
                        string finalGradeStr = row.Cells["الدرجة النهائية"].Value?.ToString();

                        bool workGradeHasValue = !string.IsNullOrWhiteSpace(workGradeStr);
                        bool finalGradeHasValue = !string.IsNullOrWhiteSpace(finalGradeStr);

                        int workGrade = 0;
                        int finalGrade = 0;

                        // فقط إذا كانت القيم موجودة، نقوم بتحويلها، وإلا نتركها بدون تغيير
                        if (workGradeHasValue)
                        {
                            if (!int.TryParse(workGradeStr, out workGrade))
                            {
                                MessageBox.Show($"قيمة درجة الأعمال غير صحيحة في الصف رقم {row.Index + 1}");
                                continue;
                            }
                            if (workGrade < 0 || workGrade > 40)
                            {
                                MessageBox.Show($"درجة الأعمال يجب أن تكون بين 0 و 40 في الصف رقم {row.Index + 1}");
                                continue;
                            }
                        }

                        if (finalGradeHasValue)
                        {
                            if (!int.TryParse(finalGradeStr, out finalGrade))
                            {
                                MessageBox.Show($"قيمة الدرجة النهائية غير صحيحة في الصف رقم {row.Index + 1}");
                                continue;
                            }
                            if (finalGrade < 0 || finalGrade > 60)
                            {
                                MessageBox.Show($"الدرجة النهائية يجب أن تكون بين 0 و 60 في الصف رقم {row.Index + 1}");
                                continue;
                            }
                        }

                        // اذا لم يُدخل أي قيمة للدرجات نتخطى التحديث لهذا الصف
                        if (!workGradeHasValue && !finalGradeHasValue)
                        {
                            skippedCount++;
                            continue;
                        }

                        // نحتاج معرفة القيم الحالية في قاعدة البيانات للدرجات لنحدّث فقط القيم المعطاة
                        string selectQuery = "SELECT work_grade, final_grade FROM Grades WHERE grade_id = @gradeId";

                        int dbWorkGrade = 0;
                        int dbFinalGrade = 0;

                        using (SqlCommand selectCmd = new SqlCommand(selectQuery, conn))
                        {
                            selectCmd.Parameters.AddWithValue("@gradeId", gradeId);
                            using (SqlDataReader reader = selectCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    dbWorkGrade = reader["work_grade"] != DBNull.Value ? Convert.ToInt32(reader["work_grade"]) : 0;
                                    dbFinalGrade = reader["final_grade"] != DBNull.Value ? Convert.ToInt32(reader["final_grade"]) : 0;
                                }
                                else
                                {
                                    // إذا لم نجد السجل، نتخطى
                                    skippedCount++;
                                    continue;
                                }
                            }
                        }

                        // تحديد القيم التي سيتم تحديثها: إذا القيمة جديدة موجودة نستخدمها، وإلا نحتفظ بالقيمة القديمة
                        int newWorkGrade = workGradeHasValue ? workGrade : dbWorkGrade;
                        int newFinalGrade = finalGradeHasValue ? finalGrade : dbFinalGrade;

                        int totalGrade = newWorkGrade + newFinalGrade;

                        string updateQuery = @"
                UPDATE Grades
                SET work_grade = @workGrade,
                    final_grade = @finalGrade,
                    total_grade = @totalGrade,
                    success_status = CASE WHEN @totalGrade >= 50 THEN N'نجاح' ELSE N'رسوب' END
                WHERE grade_id = @gradeId";

                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@workGrade", newWorkGrade);
                            cmd.Parameters.AddWithValue("@finalGrade", newFinalGrade);
                            cmd.Parameters.AddWithValue("@totalGrade", totalGrade);
                            cmd.Parameters.AddWithValue("@gradeId", gradeId);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                // تسجيل التعديل
                                string auditQuery = @"
                        INSERT INTO Audit_Log (user_id, action, table_name, record_id)
                        VALUES (@userId, 'UPDATE', 'Grades', @recordId)";
                                using (SqlCommand auditCmd = new SqlCommand(auditQuery, conn))
                                {
                                    auditCmd.Parameters.AddWithValue("@userId", Session.userID);
                                    auditCmd.Parameters.AddWithValue("@recordId", gradeId);
                                    auditCmd.ExecuteNonQuery();
                                }
                                updatedCount++;
                            }
                            else
                            {
                                skippedCount++;
                            }
                        }
                    }

                    MessageBox.Show($"✅ تم حفظ التعديلات بنجاح.\nعدد التحديثات: {updatedCount}\nعدد السجلات التي لم يتم تحديثها: {skippedCount}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ خطأ أثناء حفظ التعديلات: " + ex.Message);
            }

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }
        //--------------------------------------------------------------------------------------------------------------------------3

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)

        {
            if (comboBox_Year.SelectedItem == null) return;

            int yearNumber;
            if (!int.TryParse(comboBox_Year.SelectedItem.ToString(), out yearNumber))
                return;

            using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;")) 

            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT course_id, course_name FROM Courses WHERE year_number = @year", conn);
                cmd.Parameters.AddWithValue("@year", yearNumber);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                comboBox_Course.DisplayMember = "course_name";
                comboBox_Course.ValueMember = "course_id";
                comboBox_Course.DataSource = dt;
            }


        }
        private int currentPageIndex = 0;
        private List<DataTable> pages = new List<DataTable>();



        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (pages == null || pages.Count == 0 || currentPageIndex >= pages.Count)
            {
                e.HasMorePages = false;
                return;
            }

            DataTable dt = pages[currentPageIndex];
            DataRow firstRow = dt.Rows[0];

            Font titleFont = new Font("Arial", 14, FontStyle.Bold);
            Font headerFont = new Font("Arial", 12, FontStyle.Bold);
            Font textFont = new Font("Arial", 11);
            Brush brush = Brushes.Black;

            int x = 50;
            int y = 50;
            int tableWidth = 680;

            // --- رأس الصفحة ---
            StringFormat centerFormat = new StringFormat { Alignment = StringAlignment.Center };
            e.Graphics.DrawString("دولة ليبيا", titleFont, brush, x + tableWidth / 2, y, centerFormat); y += 30;
            e.Graphics.DrawString("وزارة التعليم", titleFont, brush, x + tableWidth / 2, y, centerFormat); y += 30;
            e.Graphics.DrawString("جامعة غريان", titleFont, brush, x + tableWidth / 2, y, centerFormat); y += 30;
            e.Graphics.DrawString("كلية العلوم الصحية", titleFont, brush, x + tableWidth / 2, y, centerFormat); y += 30;
            e.Graphics.DrawString("التاريخ: " + DateTime.Now.ToString("yyyy/MM/dd"), textFont, brush, x + tableWidth / 2, y, centerFormat); y += 40;

            // --- بيانات المادة في جدول 2 صفوف و3 أعمدة ---
            int colWidth = tableWidth / 3;
            int rowHeight = 30;

            string courseName = firstRow["اسم المادة"].ToString();
            string courseId = firstRow["رقم المادة"].ToString();
            string year = firstRow["السنة الدراسية"].ToString();
            string group = firstRow["رقم المجموعة"].ToString();
            string instructor = firstRow["اسم الأستاذ"]?.ToString() ?? "غير معروف";
            string failedCount = dt.Rows.Count.ToString();

            string[] infoTitles = { "اسم الأستاذ", "السنة الدراسية", "اسم المادة" };
            string[] infoValues = { instructor, year, courseName };

            string[] infoTitles2 = { "رقم المادة", "رقم المجموعة", "عدد الطلاب" };
            string[] infoValues2 = { failedCount, group, courseId };


            // الصف الأول
            for (int i = 0; i < 3; i++)
            {
                int colX = x + i * colWidth;
                Rectangle rectTitle = new Rectangle(colX, y, colWidth, rowHeight);
                Rectangle rectValue = new Rectangle(colX, y + rowHeight, colWidth, rowHeight);

                e.Graphics.DrawRectangle(Pens.Black, rectTitle);
                e.Graphics.DrawString(infoTitles[i], headerFont, brush, rectTitle, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                e.Graphics.DrawRectangle(Pens.Black, rectValue);
                e.Graphics.DrawString(infoValues[i], textFont, brush, rectValue, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }

            // الصف الثاني
            y += 2 * rowHeight;
            for (int i = 0; i < 3; i++)
            {
                int colX = x + i * colWidth;
                Rectangle rectTitle = new Rectangle(colX, y, colWidth, rowHeight);
                Rectangle rectValue = new Rectangle(colX, y + rowHeight, colWidth, rowHeight);

                e.Graphics.DrawRectangle(Pens.Black, rectTitle);
                e.Graphics.DrawString(infoTitles2[i], headerFont, brush, rectTitle, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                e.Graphics.DrawRectangle(Pens.Black, rectValue);
                e.Graphics.DrawString(infoValues2[i], textFont, brush, rectValue, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }

            y += 2 * rowHeight + 20;

            // --- جدول الطلاب ---
            string[] headers = { "النتيجة", "الدرجة", "القسم", "الرقم الجامعي", "اسم الطالب" };
            int[] columnWidths = { 80, 80, 150, 100, 270 }; // المجموع = 680
            int rowHeightStudents = 30;

            int tableX = x;
            int tableY = y;

            // رؤوس الأعمدة (يمين لليسار)
            for (int i = 0; i < headers.Length; i++)
            {
                Rectangle rect = new Rectangle(tableX, tableY, columnWidths[i], rowHeightStudents);
                e.Graphics.DrawRectangle(Pens.Black, rect);
                e.Graphics.DrawString(headers[i], headerFont, brush,
                    new RectangleF(rect.X, rect.Y, rect.Width, rect.Height),
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                tableX += columnWidths[i];
            }
            tableY += rowHeightStudents;

            int pageHeightLimit = e.MarginBounds.Bottom - 50;

            // بيانات الطلاب
            foreach (DataRow row in dt.Rows)
            {
                if (tableY + rowHeightStudents > pageHeightLimit)
                {

                    e.HasMorePages = true;
                    currentPageIndex++;
                    return;
                }

                tableX = x;
                string[] values =
                {
            row["النتيجة"].ToString(),
            row["الدرجة"].ToString(),
            row["القسم"].ToString(),
            row["الرقم الجامعي"].ToString(),
            row["اسم الطالب"].ToString()
        };

                for (int i = 0; i < values.Length; i++)
                {
                    Rectangle rect = new Rectangle(tableX, tableY, columnWidths[i], rowHeightStudents);
                    e.Graphics.DrawRectangle(Pens.Black, rect);
                    e.Graphics.DrawString(values[i], textFont, brush,
                        new RectangleF(rect.X, rect.Y, rect.Width, rect.Height),
                        new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center });
                    tableX += columnWidths[i];
                }
                tableY += rowHeightStudents;
            }

            currentPageIndex++;
            e.HasMorePages = currentPageIndex < pages.Count;
        }



        private void button5_Click(object sender, EventArgs e)

        {
            //    if (comboBox_Year.SelectedItem == null)
            //    {
            //        MessageBox.Show("يرجى اختيار السنة الدراسية.");
            //        return;
            //    }

            //    if (comboBox_Course.SelectedValue == null)
            //    {
            //        MessageBox.Show("يرجى اختيار المادة.");
            //        return;
            //    }

            //    int selectedYear;
            //    if (!int.TryParse(comboBox_Year.SelectedItem.ToString(), out selectedYear))
            //    {
            //        MessageBox.Show("السنة الدراسية غير صالحة.");
            //        return;
            //    }

            //    int courseId = Convert.ToInt32(comboBox_Course.SelectedValue);

            //    string connectionString = @"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;";
            //    using (SqlConnection conn = new SqlConnection(connectionString))
            //    {
            //        conn.Open();
            //        string query = @"
            //    SELECT 
            //        s.student_id, 
            //        s.university_number, 
            //        s.full_name, 
            //        d.dep_name, 
            //        g.final_grade, 
            //        g.success_status,
            //        c.course_name, 
            //        c.course_id,
            //        i.full_name AS instructor_name
            //    FROM Grades g
            //    INNER JOIN Students s ON g.student_id = s.student_id
            //    INNER JOIN Departments d ON s.department_id = d.department_id
            //    INNER JOIN Courses c ON g.course_id = c.course_id
            //    INNER JOIN Course_Instructor ci ON c.course_id = ci.course_id
            //    INNER JOIN Instructors i ON ci.instructor_id = i.instructor_id
            //    WHERE g.course_id = @courseId 
            //      AND c.year_number = @year
            //      AND g.success_status = N'رسوب'
            //";

            //    string query = @"
            //SELECT s.student_id, s.full_name, s.university_number, g.final_grade, g.success_status
            //FROM Grades g
            //INNER JOIN Students s ON g.student_id = s.student_id
            //INNER JOIN Courses c ON g.course_id = c.course_id
            //WHERE g.course_id = @courseId 
            //  AND c.year_number = @year
            //  AND g.success_status = N'رسوب'";

            //    using (SqlCommand cmd = new SqlCommand(query, conn))
            //    {
            //        cmd.Parameters.AddWithValue("@courseId", courseId);
            //        cmd.Parameters.AddWithValue("@year", selectedYear);

            //        SqlDataAdapter da = new SqlDataAdapter(cmd);
            //        DataTable dt = new DataTable();
            //        da.Fill(dt);
            //        dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            //        dataGridView3.DataSource = dt;

            //        if (dt.Rows.Count == 0)
            //        {
            //            MessageBox.Show("لا يوجد طلاب راسبين في هذه المادة والسنة الدراسية.");
            //        }
            //    }
            //}
            // التحقق من اختيار السنة والمادة
            if (comboBox_Year.SelectedItem == null)
            {
                MessageBox.Show("يرجى اختيار السنة الدراسية.");
                return;
            }

            if (comboBox_Course.SelectedValue == null)
            {
                MessageBox.Show("يرجى اختيار المادة.");
                return;
            }

            // استخراج القيم
            int selectedYear;
            if (!int.TryParse(comboBox_Year.SelectedItem.ToString(), out selectedYear))
            {
                MessageBox.Show("السنة الدراسية غير صالحة.");
                return;
            }

            int courseId = Convert.ToInt32(comboBox_Course.SelectedValue);

            // نص الاتصال بقاعدة البيانات
            string connectionString = @"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;";

         
            string query = @"
SELECT 
    c.course_name AS 'اسم المادة',
    c.course_id AS 'رقم المادة',
    c.year_number AS 'السنة الدراسية',
    cc.group_number AS 'رقم المجموعة',
    i.full_name AS 'اسم الأستاذ',
    s.full_name AS 'اسم الطالب',
    s.university_number AS 'الرقم الجامعي',
    d.dep_name AS 'القسم',
    g.final_grade AS 'الدرجة',
    g.success_status AS 'النتيجة'
FROM Grades g
INNER JOIN Students s ON g.student_id = s.student_id
INNER JOIN Courses c ON g.course_id = c.course_id
INNER JOIN Departments d ON s.department_id = d.department_id
LEFT JOIN Course_Classroom cc ON c.course_id = cc.course_id
LEFT JOIN Course_Instructor ci ON c.course_id = ci.course_id
LEFT JOIN Instructors i ON ci.instructor_id = i.instructor_id
WHERE c.year_number = @year
  AND c.course_id = @courseId
  AND g.success_status = N'رسوب'
ORDER BY c.course_id, cc.group_number, s.university_number;";


            // تنفيذ الاستعلام
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@courseId", courseId);
                cmd.Parameters.AddWithValue("@year", selectedYear);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView3.DataSource = dt;

                // حفظ البيانات لاستخدامها لاحقًا في الطباعة إن وجدت
                reportData = dt;
                PreparePagesByCourse(reportData);

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("لا يوجد طلاب راسبين في هذه المادة والسنة الدراسية.");
                }
            }
        }
        private void PreparePagesByCourse(DataTable data)
        {
            pages.Clear();
            var grouped = data.AsEnumerable()
                .GroupBy(r => r["رقم المادة"].ToString());

            foreach (var group in grouped)
            {
                DataTable dtPage = data.Clone();
                foreach (var row in group)
                    dtPage.ImportRow(row);
                pages.Add(dtPage);
            }

            currentPageIndex = 0;
        }
        private void button7_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dataGridView3.DataSource;
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات للطباعة.");
                return;
            }

            PreparePagesByCourse(dt);

            if (pages == null || pages.Count == 0)
            {
                MessageBox.Show("لا توجد صفحات للطباعة.");
                return;
            }




            //printDocument1.PrintPage -= printDocument1_PrintPage;

            printDocument1.PrintPage += printDocument1_PrintPage;
            currentPageIndex = 0;
            printDocument1.Print();
            //PrintPreviewDialog preview = new PrintPreviewDialog();
            ////PrintPreviewDialog previewDialog = new PrintPreviewDialog();
          
            //preview.Document = printDocument1;

            //preview.ShowDialog();

        }



        private void button6_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
            {
                conn.Open();

                foreach (DataGridViewRow row in dataGridView3.Rows)
                {
                    if (row.IsNewRow) continue;

                    string univNo = row.Cells["university_number"].Value.ToString();

                    SqlCommand cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM Grades g
                JOIN Students s ON g.student_id = s.student_id
                WHERE s.university_number = @univNo AND g.success_status = 'راسب'", conn);

                    cmd.Parameters.AddWithValue("@univNo", univNo);

                    int failCount = (int)cmd.ExecuteScalar();

                    string result = failCount <= 2 ? $"مرحّل ({failCount} مواد)." : "يعيد السنة.";
                    MessageBox.Show($"الطالب {univNo}: {result}");
                }
            }
        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void comboBox_Course_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dataGridViewGrades_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridViewGrades.IsCurrentCellDirty)
            {
                dataGridViewGrades.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }
        private bool isHandlingCellValueChanged = false;
        private void dataGridViewGrades_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (isHandlingCellValueChanged) return;  // لمنع التكرار المتداخل

            try
            {
                isHandlingCellValueChanged = true;

                var row = dataGridViewGrades.Rows[e.RowIndex];

                int workGrade = 0, finalGrade = 0;

                if (int.TryParse(row.Cells["درجة الأعمال"].Value?.ToString(), out int wg))
                    workGrade = wg;

                if (int.TryParse(row.Cells["درجة الامتحان النهائي"].Value?.ToString(), out int fg))
                    finalGrade = fg;

                int total = workGrade + finalGrade;

                row.Cells["المجموع الكلي"].Value = total;
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ: " + ex.Message);
            }
            finally
            {
                isHandlingCellValueChanged = false;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Excel Files|*.xlsx;*.xls";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            string filePath = ofd.FileName;

            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // تخطي العنوان

                    using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
                    {
                        conn.Open();

                        int insertedCount = 0;
                        int updatedCount = 0;
                        int skippedCount = 0;

                        foreach (var row in rows)
                        {
                            string universityNumber = row.Cell(1).GetString().Trim();
                            string studentName = row.Cell(2).GetString().Trim();
                            int courseId;

                            if (!int.TryParse(row.Cell(3).GetString().Trim(), out courseId))
                            {
                                MessageBox.Show($"رمز المادة غير صالح في الصف رقم {row.RowNumber()}");
                                skippedCount++;
                                continue;
                            }

                            // تحقق من وجود المادة في جدول Courses
                            string checkCourseQuery = "SELECT COUNT(*) FROM Courses WHERE course_id = @courseId";
                            using (SqlCommand cmdCheckCourse = new SqlCommand(checkCourseQuery, conn))
                            {
                                cmdCheckCourse.Parameters.AddWithValue("@courseId", courseId);
                                int count = (int)cmdCheckCourse.ExecuteScalar();
                                if (count == 0)
                                {
                                    MessageBox.Show($"رمز المادة {courseId} غير موجود في قاعدة البيانات. الصف {row.RowNumber()}");
                                    skippedCount++;
                                    continue; // تخطي الصف
                                }
                            }

                            var workCell = row.Cell(4);
                            var finalCell = row.Cell(5);

                            bool isWorkGradeValid = !(workCell.IsEmpty() || workCell.GetString().Trim() == "لم ترصد");
                            bool isFinalGradeValid = !(finalCell.IsEmpty() || finalCell.GetString().Trim() == "لم ترصد");

                            int workGrade = 0;
                            int finalGrade = 0;

                            if (isWorkGradeValid)
                            {
                                if (!workCell.TryGetValue<int>(out workGrade))
                                {
                                    string workGradeStr = workCell.GetString().Trim();
                                    if (!int.TryParse(workGradeStr, out workGrade))
                                    {
                                        MessageBox.Show($"⚠️ قيمة غير صالحة في درجة الأعمال للطالب: {studentName} في الصف {row.RowNumber()}");
                                        skippedCount++;
                                        continue;
                                    }
                                }

                                if (workGrade < 0 || workGrade > 40)
                                {
                                    MessageBox.Show($"⚠️ درجة الأعمال يجب أن تكون بين 0 و 40 للطالب: {studentName} في الصف {row.RowNumber()}");
                                    skippedCount++;
                                    continue;
                                }
                            }

                            if (isFinalGradeValid)
                            {
                                if (!finalCell.TryGetValue<int>(out finalGrade))
                                {
                                    string finalGradeStr = finalCell.GetString().Trim();
                                    if (!int.TryParse(finalGradeStr, out finalGrade))
                                    {
                                        MessageBox.Show($"⚠️ قيمة غير صالحة في درجة الامتحان النهائي للطالب: {studentName} في الصف {row.RowNumber()}");
                                        skippedCount++;
                                        continue;
                                    }
                                }

                                if (finalGrade < 0 || finalGrade > 60)
                                {
                                    MessageBox.Show($"⚠️ درجة الامتحان النهائي يجب أن تكون بين 0 و 60 للطالب: {studentName} في الصف {row.RowNumber()}");
                                    skippedCount++;
                                    continue;
                                }
                            }

                            if (!isWorkGradeValid && !isFinalGradeValid)
                            {
                                skippedCount++;
                                continue;
                            }

                            int totalGrade = workGrade + finalGrade;

                            // جلب student_id حسب رقم القيد
                            string studentIdQuery = "SELECT student_id FROM Students WHERE university_number = @uniNumber";
                            int studentId = -1;
                            using (SqlCommand cmdStudentId = new SqlCommand(studentIdQuery, conn))
                            {
                                cmdStudentId.Parameters.AddWithValue("@uniNumber", universityNumber);
                                var res = cmdStudentId.ExecuteScalar();
                                if (res != null)
                                    studentId = Convert.ToInt32(res);
                                else
                                {
                                    MessageBox.Show($"لم يتم العثور على الطالب برقم الجامعة: {universityNumber} في الصف {row.RowNumber()}");
                                    skippedCount++;
                                    continue;
                                }
                            }

                            // تحقق هل السجل موجود وماذا قيم الدرجات
                            string checkGradesQuery = @"
                    SELECT work_grade, final_grade FROM Grades 
                    WHERE student_id = @studentId AND course_id = @courseId";

                            using (SqlCommand checkGradesCmd = new SqlCommand(checkGradesQuery, conn))
                            {
                                checkGradesCmd.Parameters.AddWithValue("@studentId", studentId);
                                checkGradesCmd.Parameters.AddWithValue("@courseId", courseId);

                                using (SqlDataReader reader = checkGradesCmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        object dbWorkGradeObj = reader["work_grade"];
                                        object dbFinalGradeObj = reader["final_grade"];

                                        int dbWorkGrade = (dbWorkGradeObj == DBNull.Value) ? -1 : Convert.ToInt32(dbWorkGradeObj);
                                        int dbFinalGrade = (dbFinalGradeObj == DBNull.Value) ? -1 : Convert.ToInt32(dbFinalGradeObj);

                                        bool allowUpdate = (dbWorkGrade == -1 || dbWorkGrade == 0) && (dbFinalGrade == -1 || dbFinalGrade == 0);

                                        if (allowUpdate)
                                        {
                                            string updateQuery = @"
                                    UPDATE Grades 
                                    SET work_grade = @workGrade,
                                        final_grade = @finalGrade,
                                        total_grade = @totalGrade,
                                        success_status = CASE WHEN @totalGrade >= 50 THEN N'نجاح' ELSE N'رسوب' END
                                    WHERE student_id = @studentId AND course_id = @courseId";

                                            reader.Close();

                                            using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                                            {
                                                updateCmd.Parameters.AddWithValue("@studentId", studentId);
                                                updateCmd.Parameters.AddWithValue("@courseId", courseId);
                                                updateCmd.Parameters.AddWithValue("@workGrade", workGrade);
                                                updateCmd.Parameters.AddWithValue("@finalGrade", finalGrade);
                                                updateCmd.Parameters.AddWithValue("@totalGrade", totalGrade);
                                                updateCmd.ExecuteNonQuery();
                                            }

                                            // تسجيل التعديل في Audit_Log
                                            string auditQuery = @"
                                    INSERT INTO Audit_Log (user_id, action, table_name, record_id)
                                    VALUES (@userId, 'UPDATE', 'Grades', @recordId)";
                                            using (SqlCommand auditCmd = new SqlCommand(auditQuery, conn))
                                            {
                                                auditCmd.Parameters.AddWithValue("@userId", Session.userID);
                                                auditCmd.Parameters.AddWithValue("@recordId", studentId);
                                                auditCmd.ExecuteNonQuery();
                                            }

                                            updatedCount++;
                                        }
                                        else
                                        {
                                            skippedCount++;
                                            reader.Close();
                                        }
                                    }
                                    else
                                    {
                                        reader.Close();

                                        string insertQuery = @"
                                INSERT INTO Grades (student_id, course_id, work_grade, final_grade, total_grade, success_status)
                                VALUES (@studentId, @courseId, @workGrade, @finalGrade, @totalGrade,
                                        CASE WHEN @totalGrade >= 50 THEN N'نجاح' ELSE N'رسوب' END)";

                                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                                        {
                                            insertCmd.Parameters.AddWithValue("@studentId", studentId);
                                            insertCmd.Parameters.AddWithValue("@courseId", courseId);
                                            insertCmd.Parameters.AddWithValue("@workGrade", workGrade);
                                            insertCmd.Parameters.AddWithValue("@finalGrade", finalGrade);
                                            insertCmd.Parameters.AddWithValue("@totalGrade", totalGrade);
                                            insertCmd.ExecuteNonQuery();
                                        }

                                        // تسجيل الإدخال في Audit_Log
                                        string auditQuery = @"
                                INSERT INTO Audit_Log (user_id, action, table_name, record_id)
                                VALUES (@userId, 'INSERT', 'Grades', @recordId)";
                                        using (SqlCommand auditCmd = new SqlCommand(auditQuery, conn))
                                        {
                                            auditCmd.Parameters.AddWithValue("@userId", Session.userID);
                                            auditCmd.Parameters.AddWithValue("@recordId", studentId);
                                            auditCmd.ExecuteNonQuery();
                                        }

                                        insertedCount++;
                                    }
                                }
                            }
                        }

                        MessageBox.Show($"✅ تم الحفظ من ملف الإكسل:\n📥 تم الإدخال: {insertedCount}\n✏️ تم التحديث: {updatedCount}\n⏭ تم التخطي: {skippedCount}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ خطأ أثناء استيراد البيانات من ملف الإكسل:\n" + ex.Message);
            }


        }


    }
}
