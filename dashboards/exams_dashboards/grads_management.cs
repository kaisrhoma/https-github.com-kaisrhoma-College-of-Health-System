using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;



namespace college_of_health_sciences.dashboards.exams_dashboards
{
    public partial class grads_management : UserControl
    {

        private readonly string connectionString = @"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;";


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
            }
            comboBox_Year.Items.Add("1");
            comboBox_Year.Items.Add("2");
            comboBox_Year.Items.Add("3");
            comboBox_Year.Items.Add("4");
            printDocument1.PrintPage += printDocument1_PrintPage;
            printDocument1.BeginPrint += BeginPrint_Reset;
            comboBox_Year.SelectedIndex = 0;
            dataGridView2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            comboDepartment.SelectedIndexChanged += comboDepartmentOrYearChanged;
            comboYear4.SelectedIndexChanged += comboDepartmentOrYearChanged;
            LoadDepartments();

            comboYear4.Items.Clear();
            comboYear4.Items.Add(1);
            comboYear4.Items.Add(2);
            comboYear4.Items.Add(3);
            comboYear4.Items.Add(4);
            comboYear4.SelectedIndex = 0;

            dataGridViewGrades.CellValueChanged += dataGridViewGrades_CellValueChanged;
            dataGridViewGrades.CurrentCellDirtyStateChanged += dataGridViewGrades_CurrentCellDirtyStateChanged;
            //---
            comboExamRound.Items.Clear();

            comboExamRound.Items.Add("دور أول");
            comboExamRound.Items.Add("دور ثاني");

            comboExamRound.SelectedIndex = 0; // اختيار افتراضي: كل الأدوار
                                              //-----------4
            numericUpDownYear1.Maximum = 2100;    // أعلى سنة مسموح بها
            numericUpDownYear1.Value = DateTime.Now.Year;      // القيمة الافتراضية (مثلاً)
            numericUpDownYear1.Increment = 1;     // خطوة الزيادة/النقصان سنة واحدة
            numericUpDownYear1.ThousandsSeparator = false; // حسب رغبتك
            int startYear1 = (int)numericUpDownYear1.Value;

            comboBox2.Items.Add("دور أول");
            comboBox2.Items.Add("دور ثاني");
            comboBox2.SelectedIndex = 0;
       
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

        private void comboExamRound_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedRound = null;

            if (comboExamRound.SelectedIndex >= 0) // 0 تعني "كل الأدوار"
            {
                selectedRound = comboExamRound.SelectedItem.ToString();
            }
            if (comboCourse.SelectedValue != null && int.TryParse(comboCourse.SelectedValue.ToString(), out int selectedCourseId))
            {
                LoadStudents(selectedCourseId, selectedRound);
            }
            else
            {
                MessageBox.Show("الرجاء اختيار القسم و مادة قبل تحميل الطلاب.");
            }


        }

        private void comboCourse_CursorChanged(object sender, EventArgs e)
        {


        }

        private void comboYear4_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (comboDepartment.SelectedValue != null && comboYear4.SelectedItem != null &&
      int.TryParse(comboDepartment.SelectedValue.ToString(), out int deptId) &&
      int.TryParse(comboYear4.SelectedItem.ToString(), out int year))
            {
                LoadCourses(deptId, year);
            }
            string selectedRound = null;

            if (comboExamRound.SelectedIndex >= 0) // 0 تعني "كل الأدوار"
            {
                selectedRound = comboExamRound.SelectedItem.ToString();
            }

            if (comboCourse.SelectedValue != null && int.TryParse(comboCourse.SelectedValue.ToString(), out int selectedCourseId))
            {
                LoadStudents(selectedCourseId, selectedRound);
            }
        }

        private void comboDepartmentOrYearChanged(object sender, EventArgs e)
        {
            if (comboDepartment.SelectedValue != null && comboYear4.SelectedItem != null &&
         int.TryParse(comboDepartment.SelectedValue.ToString(), out int deptId) &&
         int.TryParse(comboYear4.SelectedItem.ToString(), out int year))
            {
                LoadCourses(deptId, year);
            }
        }




        private void LoadCourses(int departmentId, int year)
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
                  WHERE cd.department_id = @deptId
                  AND c.year_number = @year", conn);

                    da.SelectCommand.Parameters.AddWithValue("@deptId", departmentId);
                    da.SelectCommand.Parameters.AddWithValue("@year", year);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    comboCourse.DisplayMember = "course_name";
                    comboCourse.ValueMember = "course_id";

                    if (dt.Rows.Count > 0)
                    {
                        comboCourse.DataSource = dt;
                    }
                    else
                    {
                        comboCourse.DataSource = null;
                        comboCourse.Items.Clear();
                        comboCourse.Items.Add("لا توجد مواد");
                        comboCourse.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء تحميل المواد: " + ex.Message);
            }
        }
        private void LoadStudents(int courseId, string examRound)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"SELECT 
    s.university_number AS [رقم القيد],
    s.full_name AS [اسم الطالب],
    CAST(g.work_grade AS NVARCHAR) AS [درجة الأعمال],
    CAST(g.final_grade AS NVARCHAR) AS [درجة الامتحان النهائي],
    CAST(g.total_grade AS NVARCHAR) AS [المجموع الكلي],
    g.success_status AS [حالة الطالب],
    s.exam_round AS [الدور]
FROM Students s
INNER JOIN Registrations r 
    ON s.student_id = r.student_id
    AND r.course_id = @courseId
    AND r.status = N'مسجل'
    -- عرض الطلاب فقط للسنة الأكاديمية الأعلى في المادة
    AND r.academic_year_start = (
        SELECT MAX(r2.academic_year_start)
        FROM Registrations r2
        WHERE r2.course_id = @courseId
    )
LEFT JOIN Grades g 
    ON s.student_id = g.student_id 
    AND g.course_id = r.course_id
WHERE 1=1
";

                    if (!string.IsNullOrEmpty(examRound))
                    {
                        query += " AND s.exam_round = @examRound ";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@courseId", courseId);

                        if (!string.IsNullOrEmpty(examRound))
                        {
                            cmd.Parameters.AddWithValue("@examRound", examRound);
                        }

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dataGridViewGrades.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء تحميل الطلاب: " + ex.Message);
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

            if (comboDepartment.SelectedValue != null && comboYear4.SelectedItem != null)
            {
                int deptId = (int)comboDepartment.SelectedValue;
                int year = Convert.ToInt32(comboYear4.SelectedItem);
                LoadCourses(deptId, year);
            }
        }

        private void comboYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboDepartment.SelectedValue != null && comboYear4.SelectedItem != null)
            {
                int deptId = (int)comboDepartment.SelectedValue;
                int year = Convert.ToInt32(comboYear4.SelectedItem);
                LoadCourses(deptId, year);
            }

        }
      

        private void button1_Click(object sender, EventArgs e)
        {
            string exr = comboExamRound.Text;
            if (exr == "دور أول")
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
                    {
                        conn.Open();
                        using (SqlTransaction transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                int insertedCount = 0;
                                int updatedCount = 0;
                                int skippedCount = 0;

                                int courseId = Convert.ToInt32(comboCourse.SelectedValue);
                                string selectedRound = comboExamRound.SelectedItem?.ToString();

                                foreach (DataGridViewRow row in dataGridViewGrades.Rows)
                                {
                                    if (row.IsNewRow) continue;

                                    string uniNumber = row.Cells["رقم القيد"].Value?.ToString();
                                    string studentName = row.Cells["اسم الطالب"].Value?.ToString();

                                    string workStr = row.Cells["درجة الأعمال"].Value?.ToString().Trim();
                                    string finalStr = row.Cells["درجة الامتحان النهائي"].Value?.ToString().Trim();

                                    bool hasWork = !string.IsNullOrEmpty(workStr) && workStr != "لم ترصد";
                                    bool hasFinal = !string.IsNullOrEmpty(finalStr) && finalStr != "لم ترصد";

                                    // إذا كلاهما فارغ → تخطي
                                    if (!hasWork && !hasFinal)
                                    {
                                        skippedCount++;
                                        continue;
                                    }

                                    int wg = 0;
                                    int fg = 0;
                                    int? workGrade = null;
                                    int? finalGrade = null;

                                    if (hasWork)
                                    {
                                        if (!int.TryParse(workStr, out wg))
                                        {
                                            MessageBox.Show($"⚠️ قيمة غير صالحة في درجة الأعمال للطالب: {studentName}");
                                            continue;
                                        }
                                        if (wg < 0 || wg > 40)
                                        {
                                            MessageBox.Show($"⚠️ درجة الأعمال يجب أن تكون بين 0 و 40 للطالب: {studentName}");
                                            continue;
                                        }
                                        workGrade = wg;
                                    }

                                    if (hasFinal)
                                    {
                                        if (!int.TryParse(finalStr, out fg))
                                        {
                                            MessageBox.Show($"⚠️ قيمة غير صالحة في درجة النهائي للطالب: {studentName}");
                                            continue;
                                        }
                                        if (fg < 0 || fg > 60)
                                        {
                                            MessageBox.Show($"⚠️ درجة الامتحان النهائي يجب أن تكون بين 0 و 60 للطالب: {studentName}");
                                            continue;
                                        }
                                        finalGrade = fg;
                                    }

                                    // جلب student_id
                                    string getIdQuery = "SELECT student_id FROM Students WHERE university_number=@uni";
                                    int studentId = -1;
                                    using (SqlCommand cmdId = new SqlCommand(getIdQuery, conn, transaction))
                                    {
                                        cmdId.Parameters.AddWithValue("@uni", uniNumber);
                                        var res = cmdId.ExecuteScalar();
                                        if (res != null) studentId = Convert.ToInt32(res);
                                        else continue;
                                    }

                                    // التحقق من السجل الحالي
                                    string checkQuery = @"SELECT work_grade, final_grade FROM Grades WHERE student_id=@sid AND course_id=@cid";
                                    using (SqlCommand cmdCheck = new SqlCommand(checkQuery, conn, transaction))
                                    {
                                        cmdCheck.Parameters.AddWithValue("@sid", studentId);
                                        cmdCheck.Parameters.AddWithValue("@cid", courseId);

                                        using (SqlDataReader reader = cmdCheck.ExecuteReader())
                                        {
                                            if (reader.Read())
                                            {
                                                int dbWork = reader["work_grade"] == DBNull.Value ? -1 : Convert.ToInt32(reader["work_grade"]);
                                                int dbFinal = reader["final_grade"] == DBNull.Value ? -1 : Convert.ToInt32(reader["final_grade"]);

                                                // ===========================
                                                // الشرط الجديد: تخطي إذا كلاهما موجود مسبقًا
                                                // ===========================
                                                if (dbWork != -1 && dbFinal != -1)
                                                {
                                                    skippedCount++;
                                                    reader.Close();
                                                    continue; // تخطي الصف بالكامل
                                                }

                                                reader.Close();

                                                // الأعمال: لا يمكن تعديلها بعد الإدخال
                                                if (workGrade.HasValue)
                                                {
                                                    if (dbWork != -1 && workGrade != dbWork)
                                                    {
                                                        MessageBox.Show($"⚠️ لا يمكن تعديل درجة الأعمال بعد إدخالها للطالب: {studentName}");
                                                        workGrade = dbWork;
                                                    }
                                                    else if (dbWork == -1)
                                                    {
                                                        dbWork = workGrade.Value;
                                                    }
                                                }

                                                // النهائي: يمكن التعديل فقط إذا كانت القيمة فارغة
                                                if (finalGrade.HasValue)
                                                {
                                                    if (dbFinal != -1 && finalGrade != dbFinal)
                                                    {
                                                        MessageBox.Show($"⚠️ لا يمكن تعديل درجة النهائي بعد إدخالها للطالب: {studentName}");
                                                        finalGrade = dbFinal;
                                                    }
                                                    else if (dbFinal == -1)
                                                    {
                                                        dbFinal = finalGrade.Value;
                                                    }
                                                }

                                                int? newWork = workGrade.HasValue ? workGrade : (dbWork != -1 ? dbWork : (int?)null);
                                                int? newFinal = finalGrade.HasValue ? finalGrade : (dbFinal != -1 ? dbFinal : (int?)null);
                                                int total = (newWork ?? 0) + (newFinal ?? 0);
                                                string status = (newFinal.HasValue ? (total >= 60 ? "نجاح" : "راسب") : null);

                                                string updateQuery = @"
                                        UPDATE Grades
                                        SET work_grade=@w, final_grade=@f, total_grade=@t,
                                            success_status=CASE WHEN @s IS NOT NULL THEN @s ELSE success_status END
                                        WHERE student_id=@sid AND course_id=@cid";

                                                using (SqlCommand cmdUpdate = new SqlCommand(updateQuery, conn, transaction))
                                                {
                                                    cmdUpdate.Parameters.AddWithValue("@w", (object)newWork ?? DBNull.Value);
                                                    cmdUpdate.Parameters.AddWithValue("@f", (object)newFinal ?? DBNull.Value);
                                                    cmdUpdate.Parameters.AddWithValue("@t", total);
                                                    cmdUpdate.Parameters.AddWithValue("@s", (object)status ?? DBNull.Value);
                                                    cmdUpdate.Parameters.AddWithValue("@sid", studentId);
                                                    cmdUpdate.Parameters.AddWithValue("@cid", courseId);
                                                    cmdUpdate.ExecuteNonQuery();
                                                }

                                                // سجل التحديث
                                                string audit = "INSERT INTO Audit_Log (user_id, action, table_name, record_id) VALUES (@uid,'UPDATE','Grades',@rid)";
                                                using (SqlCommand cmdAudit = new SqlCommand(audit, conn, transaction))
                                                {
                                                    cmdAudit.Parameters.AddWithValue("@uid", Session.userID);
                                                    cmdAudit.Parameters.AddWithValue("@rid", studentId);
                                                    cmdAudit.ExecuteNonQuery();
                                                }

                                                updatedCount++;
                                            }
                                            else
                                            {
                                                reader.Close();

                                                int? newWork = workGrade; // يبقى NULL إذا لم يدخل
                                                int? newFinal = finalGrade; // يبقى NULL إذا لم يدخل
                                                int total = (newWork ?? 0) + (newFinal ?? 0);
                                                string status = (newFinal.HasValue ? (total >= 60 ? "نجاح" : "راسب") : null);

                                                string insertQuery = @"INSERT INTO Grades (student_id, course_id, work_grade, final_grade, total_grade, success_status)
                                                   VALUES (@sid,@cid,@w,@f,@t,@s)";
                                                using (SqlCommand cmdInsert = new SqlCommand(insertQuery, conn, transaction))
                                                {
                                                    cmdInsert.Parameters.AddWithValue("@sid", studentId);
                                                    cmdInsert.Parameters.AddWithValue("@cid", courseId);
                                                    cmdInsert.Parameters.AddWithValue("@w", (object)newWork ?? DBNull.Value);
                                                    cmdInsert.Parameters.AddWithValue("@f", (object)newFinal ?? DBNull.Value);
                                                    cmdInsert.Parameters.AddWithValue("@t", total);
                                                    cmdInsert.Parameters.AddWithValue("@s", (object)status ?? DBNull.Value);
                                                    cmdInsert.ExecuteNonQuery();
                                                }

                                                // سجل الإدخال
                                                string audit = "INSERT INTO Audit_Log (user_id, action, table_name, record_id) VALUES (@uid,'INSERT','Grades',@rid)";
                                                using (SqlCommand cmdAudit = new SqlCommand(audit, conn, transaction))
                                                {
                                                    cmdAudit.Parameters.AddWithValue("@uid", Session.userID);
                                                    cmdAudit.Parameters.AddWithValue("@rid", studentId);
                                                    cmdAudit.ExecuteNonQuery();
                                                }

                                                insertedCount++;
                                            }
                                        }
                                    }
                                }

                                transaction.Commit();
                                label11.Text = $"✅ تم الحفظ:\n📥 تم الإدخال: {insertedCount}\n✏️ تم التحديث: {updatedCount}\n⏭ تم التخطي: {skippedCount}";
                                LoadStudents(courseId, selectedRound);
                            }
                            catch (Exception exInner)
                            {
                                transaction.Rollback();
                                MessageBox.Show("❌ حدث خطأ، تم التراجع عن جميع العمليات:\n" + exInner.Message);
                            }
                        }
                    }
                }
                catch (Exception exOuter)
                {
                    MessageBox.Show("❌ خطأ أثناء الاتصال أو العملية:\n" + exOuter.Message);
                }
            }
            // دور ثاني
            else
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
                    {
                        conn.Open();
                        using (SqlTransaction transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                int insertedCount = 0;
                                int updatedCount = 0;
                                int skippedCount = 0;

                                int courseId = Convert.ToInt32(comboCourse.SelectedValue);
                                string selectedRound = comboExamRound.SelectedItem?.ToString();

                                foreach (DataGridViewRow row in dataGridViewGrades.Rows)
                                {
                                    if (row.IsNewRow) continue;

                                    string uniNumber = row.Cells["رقم القيد"].Value?.ToString();
                                    string studentName = row.Cells["اسم الطالب"].Value?.ToString();

                                    string workStr = row.Cells["درجة الأعمال"].Value?.ToString().Trim();
                                    string finalStr = row.Cells["درجة الامتحان النهائي"].Value?.ToString().Trim();

                                    bool hasWork = !string.IsNullOrEmpty(workStr) && workStr != "لم ترصد";
                                    bool hasFinal = !string.IsNullOrEmpty(finalStr) && finalStr != "لم ترصد";

                                    // إذا كلاهما فارغ → تخطي
                                    if (!hasWork && !hasFinal)
                                    {
                                        skippedCount++;
                                        continue;
                                    }

                                    int wg = 0;
                                    int fg = 0;
                                    int? workGrade = null;
                                    int? finalGrade = null;

                                    if (hasWork)
                                    {
                                        if (!int.TryParse(workStr, out wg))
                                        {
                                            MessageBox.Show($"⚠️ قيمة غير صالحة في درجة الأعمال للطالب: {studentName}");
                                            continue;
                                        }
                                        if (wg < 0 || wg > 40)
                                        {
                                            MessageBox.Show($"⚠️ درجة الأعمال يجب أن تكون بين 0 و 40 للطالب: {studentName}");
                                            continue;
                                        }
                                        workGrade = wg;
                                    }

                                    if (hasFinal)
                                    {
                                        if (!int.TryParse(finalStr, out fg))
                                        {
                                            MessageBox.Show($"⚠️ قيمة غير صالحة في درجة النهائي للطالب: {studentName}");
                                            continue;
                                        }
                                        if (fg < 0 || fg > 60)
                                        {
                                            MessageBox.Show($"⚠️ درجة الامتحان النهائي يجب أن تكون بين 0 و 60 للطالب: {studentName}");
                                            continue;
                                        }
                                        finalGrade = fg;
                                    }

                                    // جلب student_id
                                    string getIdQuery = "SELECT student_id FROM Students WHERE university_number=@uni";
                                    int studentId = -1;
                                    using (SqlCommand cmdId = new SqlCommand(getIdQuery, conn, transaction))
                                    {
                                        cmdId.Parameters.AddWithValue("@uni", uniNumber);
                                        var res = cmdId.ExecuteScalar();
                                        if (res != null) studentId = Convert.ToInt32(res);
                                        else continue;
                                    }

                                    // التحقق من السجل الحالي
                                    string checkQuery = @"SELECT work_grade, final_grade FROM Grades WHERE student_id=@sid AND course_id=@cid";
                                    using (SqlCommand cmdCheck = new SqlCommand(checkQuery, conn, transaction))
                                    {
                                        cmdCheck.Parameters.AddWithValue("@sid", studentId);
                                        cmdCheck.Parameters.AddWithValue("@cid", courseId);

                                        using (SqlDataReader reader = cmdCheck.ExecuteReader())
                                        {
                                            if (reader.Read())
                                            {
                                                int dbWork = reader["work_grade"] == DBNull.Value ? -1 : Convert.ToInt32(reader["work_grade"]);
                                                int dbFinal = reader["final_grade"] == DBNull.Value ? -1 : Convert.ToInt32(reader["final_grade"]);

                                          
                                                reader.Close();

                                                // الأعمال: لا يمكن تعديلها بعد الإدخال
                                                if (workGrade.HasValue)
                                                {
                                                  
                                                   if (dbWork == -1)
                                                    {
                                                        dbWork = workGrade.Value;
                                                    }
                                                }

                                                // النهائي: يمكن التعديل فقط إذا كانت القيمة فارغة
                                                if (finalGrade.HasValue)
                                                {
                                                  
                                                   if (dbFinal == -1)
                                                    {
                                                        dbFinal = finalGrade.Value;
                                                    }
                                                }

                                                int? newWork = workGrade.HasValue ? workGrade : (dbWork != -1 ? dbWork : (int?)null);
                                                int? newFinal = finalGrade.HasValue ? finalGrade : (dbFinal != -1 ? dbFinal : (int?)null);
                                                int total = (newWork ?? 0) + (newFinal ?? 0);
                                                string status = (newFinal.HasValue ? (total >= 60 ? "نجاح" : "راسب") : null);

                                                string updateQuery = @"
                                        UPDATE Grades
                                        SET work_grade=@w, final_grade=@f, total_grade=@t,
                                            success_status=CASE WHEN @s IS NOT NULL THEN @s ELSE success_status END
                                        WHERE student_id=@sid AND course_id=@cid";

                                                using (SqlCommand cmdUpdate = new SqlCommand(updateQuery, conn, transaction))
                                                {
                                                    cmdUpdate.Parameters.AddWithValue("@w", (object)newWork ?? DBNull.Value);
                                                    cmdUpdate.Parameters.AddWithValue("@f", (object)newFinal ?? DBNull.Value);
                                                    cmdUpdate.Parameters.AddWithValue("@t", total);
                                                    cmdUpdate.Parameters.AddWithValue("@s", (object)status ?? DBNull.Value);
                                                    cmdUpdate.Parameters.AddWithValue("@sid", studentId);
                                                    cmdUpdate.Parameters.AddWithValue("@cid", courseId);
                                                    cmdUpdate.ExecuteNonQuery();
                                                }

                                                // سجل التحديث
                                                string audit = "INSERT INTO Audit_Log (user_id, action, table_name, record_id) VALUES (@uid,'UPDATE','Grades',@rid)";
                                                using (SqlCommand cmdAudit = new SqlCommand(audit, conn, transaction))
                                                {
                                                    cmdAudit.Parameters.AddWithValue("@uid", Session.userID);
                                                    cmdAudit.Parameters.AddWithValue("@rid", studentId);
                                                    cmdAudit.ExecuteNonQuery();
                                                }

                                                updatedCount++;
                                            }
                                            else
                                            {
                                                reader.Close();

                                                int? newWork = workGrade; // يبقى NULL إذا لم يدخل
                                                int? newFinal = finalGrade; // يبقى NULL إذا لم يدخل
                                                int total = (newWork ?? 0) + (newFinal ?? 0);
                                                string status = (newFinal.HasValue ? (total >= 60 ? "نجاح" : "راسب") : null);

                                                string insertQuery = @"INSERT INTO Grades (student_id, course_id, work_grade, final_grade, total_grade, success_status)
                                                   VALUES (@sid,@cid,@w,@f,@t,@s)";
                                                using (SqlCommand cmdInsert = new SqlCommand(insertQuery, conn, transaction))
                                                {
                                                    cmdInsert.Parameters.AddWithValue("@sid", studentId);
                                                    cmdInsert.Parameters.AddWithValue("@cid", courseId);
                                                    cmdInsert.Parameters.AddWithValue("@w", (object)newWork ?? DBNull.Value);
                                                    cmdInsert.Parameters.AddWithValue("@f", (object)newFinal ?? DBNull.Value);
                                                    cmdInsert.Parameters.AddWithValue("@t", total);
                                                    cmdInsert.Parameters.AddWithValue("@s", (object)status ?? DBNull.Value);
                                                    cmdInsert.ExecuteNonQuery();
                                                }

                                                // سجل الإدخال
                                                string audit = "INSERT INTO Audit_Log (user_id, action, table_name, record_id) VALUES (@uid,'INSERT','Grades',@rid)";
                                                using (SqlCommand cmdAudit = new SqlCommand(audit, conn, transaction))
                                                {
                                                    cmdAudit.Parameters.AddWithValue("@uid", Session.userID);
                                                    cmdAudit.Parameters.AddWithValue("@rid", studentId);
                                                    cmdAudit.ExecuteNonQuery();
                                                }

                                                insertedCount++;
                                            }
                                        }
                                    }
                                }

                                transaction.Commit();
                                label11.Text = $"✅ تم الحفظ:\n📥 تم الإدخال: {insertedCount}\n✏️ تم التحديث: {updatedCount}\n⏭ تم التخطي: {skippedCount}";
                                LoadStudents(courseId, selectedRound);
                            }
                            catch (Exception exInner)
                            {
                                transaction.Rollback();
                                MessageBox.Show("❌ حدث خطأ، تم التراجع عن جميع العمليات:\n" + exInner.Message);
                            }
                        }
                    }
                }
                catch (Exception exOuter)
                {
                    MessageBox.Show("❌ خطأ أثناء الاتصال أو العملية:\n" + exOuter.Message);
                }



            }

        }
        private void button9_Click(object sender, EventArgs e)
        {
            if (comboExamRound.SelectedItem == null)
            {
                MessageBox.Show("الرجاء اختيار الدور قبل التحديث.");
                return;
            }

            string selectedRound = comboExamRound.SelectedItem.ToString();

            if (MessageBox.Show("هل أنت متأكد ؟", "تأكيد",
                                  MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // جلب آخر سنة أكاديمية من القاعدة
                            int currentAcademicYear;
                            using (SqlCommand cmdYear = new SqlCommand("SELECT MAX(academic_year_start) FROM Registrations", conn, transaction))
                            {
                                currentAcademicYear = Convert.ToInt32(cmdYear.ExecuteScalar());
                            }

                            if (selectedRound == "دور أول")
                            {
                                string query = @"
UPDATE s
SET exam_round = CASE 
    WHEN fc.fail_count = 0 THEN N'مكتمل'
    WHEN fc.fail_count >= 1 THEN N'دور ثاني'
    ELSE s.exam_round
END
FROM Students s
INNER JOIN (
    SELECT 
        s.student_id,
        COUNT(CASE WHEN g.total_grade < 60 THEN 1 END) AS fail_count
    FROM Students s
    INNER JOIN Registrations r ON s.student_id = r.student_id
    INNER JOIN Courses c ON r.course_id = c.course_id
    LEFT JOIN Grades g ON g.student_id = s.student_id AND g.course_id = r.course_id
    WHERE r.status = N'مسجل'
      AND r.academic_year_start = @academicYearStart
      AND s.exam_round = N'دور أول'
    GROUP BY s.student_id
) AS fc ON s.student_id = fc.student_id;
";

                                using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@academicYearStart", currentAcademicYear);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else if (selectedRound == "دور ثاني")
                            {
                                string query = @"
WITH CurrentYearFails AS (
    SELECT 
        s.student_id,
        s.current_year,
        COUNT(CASE WHEN g.total_grade < 60 THEN 1 END) AS current_year_fails
    FROM Students s
    INNER JOIN Registrations r ON s.student_id = r.student_id
    INNER JOIN Courses c ON r.course_id = c.course_id
    LEFT JOIN Grades g ON r.student_id = g.student_id AND r.course_id = g.course_id
    WHERE r.status = N'مسجل'
      AND r.academic_year_start = @academicYearStart
      AND s.exam_round = N'دور ثاني'
    GROUP BY s.student_id, s.current_year
)
UPDATE s
SET exam_round = CASE
    WHEN cf.current_year = 4 AND cf.current_year_fails >= 1 THEN N'إعادة سنة'
    WHEN cf.current_year_fails = 0 THEN N'مكتمل'
    WHEN cf.current_year_fails BETWEEN 1 AND 2 THEN N'مرحل'
    WHEN cf.current_year_fails >= 3 THEN N'إعادة سنة'
    ELSE s.exam_round
END
FROM Students s
INNER JOIN CurrentYearFails cf ON s.student_id = cf.student_id;";

                                using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@academicYearStart", currentAcademicYear);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit(); // ✅ نجاح كل العمليات
                            MessageBox.Show("تم تحديث حالة الطلاب بنجاح.");
                        }
                        catch (Exception exInner)
                        {
                            transaction.Rollback(); // ❌ التراجع عند الخطأ
                            MessageBox.Show("حدث خطأ، تم التراجع عن جميع العمليات:\n" + exInner.Message);
                        }
                    }
                }
            }
            catch (Exception exOuter)
            {
                MessageBox.Show("حدث خطأ أثناء الاتصال:\n" + exOuter.Message);
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
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // تخطي الصف الأول (العناوين)

                    using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
                    {
                        conn.Open();

                        int insertedCount = 0;
                        int updatedCount = 0;
                        int skippedCount = 0;
                        string selectedRound = comboExamRound.SelectedItem?.ToString();

                        foreach (var row in rows)
                        {
                            string universityNumber = row.Cell(1).GetString().Trim();
                            string studentName = row.Cell(2).GetString().Trim();
                            string courseCode = row.Cell(3).GetString().Trim(); // الكورس كود
                            var workCell = row.Cell(4);
                            var finalCell = row.Cell(5);

                            bool isWorkGradeValid = !(workCell.IsEmpty() || workCell.GetString().Trim() == "لم ترصد");
                            bool isFinalGradeValid = !(finalCell.IsEmpty() || finalCell.GetString().Trim() == "لم ترصد");

                            if (!isWorkGradeValid && !isFinalGradeValid)
                            {
                                skippedCount++;
                                continue;
                            }

                            int workGrade = 0;
                            int finalGrade = 0;

                            if (isWorkGradeValid && !int.TryParse(workCell.GetString().Trim(), out workGrade))
                            {
                                MessageBox.Show($"⚠️ قيمة غير صالحة في درجة الأعمال للطالب: {studentName} في الصف {row.RowNumber()}");
                                skippedCount++;
                                continue;
                            }

                            if (isFinalGradeValid && !int.TryParse(finalCell.GetString().Trim(), out finalGrade))
                            {
                                MessageBox.Show($"⚠️ قيمة غير صالحة في درجة الامتحان النهائي للطالب: {studentName} في الصف {row.RowNumber()}");
                                skippedCount++;
                                continue;
                            }

                            if (workGrade < 0 || workGrade > 40 || finalGrade < 0 || finalGrade > 60)
                            {
                                MessageBox.Show($"⚠️ درجات الطالب {studentName} غير ضمن النطاق المسموح في الصف {row.RowNumber()}");
                                skippedCount++;
                                continue;
                            }

                            int totalGrade = workGrade + finalGrade;

                            // الحصول على student_id
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
                                    MessageBox.Show($"لم يتم العثور على الطالب: {universityNumber} في الصف {row.RowNumber()}");
                                    skippedCount++;
                                    continue;
                                }
                            }


                            // الحصول على course_id من course_code
                            string courseIdQuery = "SELECT course_id FROM Courses WHERE course_code = @code";
                            int courseId = -1;
                            using (SqlCommand cmdCourseId = new SqlCommand(courseIdQuery, conn))
                            {
                                cmdCourseId.Parameters.AddWithValue("@code", courseCode);
                                var res = cmdCourseId.ExecuteScalar();
                                if (res != null)
                                    courseId = Convert.ToInt32(res);
                                else
                                {
                                    MessageBox.Show($"لم يتم العثور على المادة بالرمز: {courseCode} في الصف {row.RowNumber()}");
                                    skippedCount++;
                                    continue;
                                }
                            }


                            // التحقق من وجود سجل الدرجات
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
                                        int dbWorkGrade = reader["work_grade"] != DBNull.Value ? Convert.ToInt32(reader["work_grade"]) : -1;
                                        int dbFinalGrade = reader["final_grade"] != DBNull.Value ? Convert.ToInt32(reader["final_grade"]) : -1;

                                        bool allowUpdate = (dbWorkGrade == -1 || dbWorkGrade == 0) && (dbFinalGrade == -1 || dbFinalGrade == 0);

                                        if (allowUpdate)
                                        {
                                            reader.Close();
                                            string updateQuery = @"
                        UPDATE Grades 
                        SET work_grade = @workGrade,
                            final_grade = @finalGrade,
                            total_grade = @totalGrade,
                            success_status = CASE WHEN @totalGrade >= 60 THEN N'نجاح' ELSE N'راسب' END
                        WHERE student_id = @studentId AND course_id = @courseId";

                                            using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                                            {
                                                updateCmd.Parameters.AddWithValue("@studentId", studentId);
                                                updateCmd.Parameters.AddWithValue("@courseId", courseId);
                                                updateCmd.Parameters.AddWithValue("@workGrade", workGrade);
                                                updateCmd.Parameters.AddWithValue("@finalGrade", finalGrade);
                                                updateCmd.Parameters.AddWithValue("@totalGrade", totalGrade);
                                                updateCmd.ExecuteNonQuery();
                                            }

                                            using (SqlCommand auditCmd = new SqlCommand(@"
                        INSERT INTO Audit_Log (user_id, action, table_name, record_id)
                        VALUES (@userId, 'UPDATE', 'Grades', @recordId)", conn))
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
                            CASE WHEN @totalGrade >= 60 THEN N'نجاح' ELSE N'راسب' END)";
                                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                                        {
                                            insertCmd.Parameters.AddWithValue("@studentId", studentId);
                                            insertCmd.Parameters.AddWithValue("@courseId", courseId);
                                            insertCmd.Parameters.AddWithValue("@workGrade", workGrade);
                                            insertCmd.Parameters.AddWithValue("@finalGrade", finalGrade);
                                            insertCmd.Parameters.AddWithValue("@totalGrade", totalGrade);
                                            insertCmd.ExecuteNonQuery();
                                        }

                                        using (SqlCommand auditCmd = new SqlCommand(@"
                    INSERT INTO Audit_Log (user_id, action, table_name, record_id)
                    VALUES (@userId, 'INSERT', 'Grades', @recordId)", conn))
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

                        MessageBox.Show($"✅ تم الاستيراد من ملف Excel:\n📥 تم الإدخال: {insertedCount}\n✏️ تم التحديث: {updatedCount}\n⏭ تم التخطي: {skippedCount}");


                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("❌ خطأ أثناء استيراد البيانات:\n" + ex.Message);
            }



        }




        private void comboBoxCourse_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboCourse.SelectedValue != null && int.TryParse(comboCourse.SelectedValue.ToString(), out int courseId))
            {
                string examRound = comboExamRound.SelectedItem?.ToString() ?? ""; // قيمة الدور من الكمبو بوكس الآخر
                LoadStudents(courseId, examRound);
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
    g.work_grade AS [درجة الأعمال],
    g.final_grade AS [الدرجة النهائية],
    g.total_grade AS [المجموع الكلي],
    g.success_status AS [الحالة],
    g.grade_id
FROM Students s
INNER JOIN Registrations r ON s.student_id = r.student_id
INNER JOIN Courses c ON r.course_id = c.course_id
LEFT JOIN Grades g ON s.student_id = g.student_id AND c.course_id = g.course_id
WHERE s.university_number = @universityNumber AND r.status = N'مسجل'
ORDER BY r.year_number, c.course_name;";


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

                        dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;


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

            if (MessageBox.Show("هل أنت متأكد ؟", "تأكيد",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            int updatedCount = 0;
                            int skippedCount = 0;

                            foreach (DataGridViewRow row in dataGridView2.Rows)
                            {
                                if (row.IsNewRow) continue;

                                if (!int.TryParse(row.Cells["grade_id"].Value?.ToString(), out int gradeId))
                                    continue;

                                string workGradeStr = row.Cells["درجة الأعمال"].Value?.ToString();
                                string finalGradeStr = row.Cells["الدرجة النهائية"].Value?.ToString();

                                bool workGradeHasValue = !string.IsNullOrWhiteSpace(workGradeStr);
                                bool finalGradeHasValue = !string.IsNullOrWhiteSpace(finalGradeStr);

                                int workGrade = 0;
                                int finalGrade = 0;

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

                                // جلب بيانات الطالب الحالية
                                string selectQuery = "SELECT work_grade, final_grade, student_id FROM Grades WHERE grade_id = @gradeId";
                                int dbWorkGrade = 0;
                                int dbFinalGrade = 0;
                                int studentId = -1;

                                using (SqlCommand selectCmd = new SqlCommand(selectQuery, conn, transaction))
                                {
                                    selectCmd.Parameters.AddWithValue("@gradeId", gradeId);
                                    using (SqlDataReader reader = selectCmd.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            dbWorkGrade = reader["work_grade"] != DBNull.Value ? Convert.ToInt32(reader["work_grade"]) : 0;
                                            dbFinalGrade = reader["final_grade"] != DBNull.Value ? Convert.ToInt32(reader["final_grade"]) : 0;
                                            studentId = Convert.ToInt32(reader["student_id"]);
                                        }
                                        else
                                        {
                                            skippedCount++;
                                            continue;
                                        }
                                    }
                                }

                                int newWorkGrade = workGradeHasValue ? workGrade : dbWorkGrade;
                                int newFinalGrade = finalGradeHasValue ? finalGrade : dbFinalGrade;
                                int totalGrade = newWorkGrade + newFinalGrade;

                                // تحديث الدرجات
                                string updateQuery = @"
UPDATE Grades
SET work_grade = @workGrade,
    final_grade = @finalGrade,
    total_grade = @totalGrade,
    success_status = CASE WHEN @totalGrade >= 60 THEN N'نجاح' ELSE N'راسب' END
WHERE grade_id = @gradeId";

                                using (SqlCommand cmd = new SqlCommand(updateQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@workGrade", newWorkGrade);
                                    cmd.Parameters.AddWithValue("@finalGrade", newFinalGrade);
                                    cmd.Parameters.AddWithValue("@totalGrade", totalGrade);
                                    cmd.Parameters.AddWithValue("@gradeId", gradeId);

                                    int rowsAffected = cmd.ExecuteNonQuery();
                                    if (rowsAffected > 0)
                                    {
                                        // سجل التعديل
                                        string auditQuery = @"
INSERT INTO Audit_Log (user_id, action, table_name, record_id)
VALUES (@userId, 'UPDATE', 'Grades', @recordId)";
                                        using (SqlCommand auditCmd = new SqlCommand(auditQuery, conn, transaction))
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
                            } // نهاية اللوب

                            transaction.Commit(); // ✅ كل شيء نجح
                            MessageBox.Show($"✅ تم حفظ التعديلات بنجاح.\nعدد التحديثات: {updatedCount}\nعدد السجلات التي لم يتم تحديثها: {skippedCount}");
                        }
                        catch (Exception exInner)
                        {
                            transaction.Rollback(); // ❌ أي خطأ → تراجع عن كل العمليات
                            MessageBox.Show("❌ حدث خطأ، تم التراجع عن جميع التعديلات: " + exInner.Message);
                        }
                    }
                }
            }
            catch (Exception exOuter)
            {
                MessageBox.Show("❌ خطأ أثناء الاتصال بقاعدة البيانات: " + exOuter.Message);
            }

            // تحديث العرض حسب وجود رقم الجامعة
            if (txtUniversityNumber.Text == "") { button4_Click(null, null); }
            if (txtUniversityNumber.Text != "") { button3_Click(null, null); }
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }
        //--------------------------------------------------------------------------------------------------------------------------3
        private void BeginPrint_Reset(object sender, PrintEventArgs e)
        {
            currentPageIndex = 0;
            currentRowIndex = 0;
        }

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
        private int currentRowIndex = 0;  // تتبع الصف الحالي أثناء الطباعة

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

            if (dt.Columns.Contains("رقم المجموعة") && firstRow["رقم المجموعة"] != DBNull.Value && !string.IsNullOrEmpty(firstRow["رقم المجموعة"].ToString()))
            {
                group = firstRow["رقم المجموعة"].ToString();
            }

            string instructor = firstRow["اسم الأستاذ"]?.ToString() ?? "غير معروف";
            string failedCount = dt.Rows.Count.ToString();

            string[] infoTitles = { "اسم الأستاذ", "السنة الدراسية", "اسم المادة" };
            string[] infoValues = { instructor, year, courseName };

            string[] infoTitles2 = { "رقم المادة", "رقم المجموعة", "عدد الطلاب" };
            string[] infoValues2 = { courseId, group, failedCount };

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
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                    tableX += columnWidths[i];
                }
                tableY += rowHeightStudents;
            }

            // --- ترقيم الصفحة في الأسفل ---
            currentPageIndex++;
            Font pageNumberFont = new Font("Arial", 10, FontStyle.Regular);
            string pageNumberText = $"الصفحة {currentPageIndex}";
            float pageNumberX = x + tableWidth / 2;
            float pageNumberY = e.MarginBounds.Bottom + 10;
            StringFormat pageNumberFormat = new StringFormat { Alignment = StringAlignment.Center };

            e.Graphics.DrawString(pageNumberText, pageNumberFont, brush, pageNumberX, pageNumberY, pageNumberFormat);


            e.HasMorePages = currentPageIndex < pages.Count;
        }




        private void button5_Click(object sender, EventArgs e)
        {
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
INNER JOIN Registrations r ON r.student_id = s.student_id AND r.course_id = c.course_id
INNER JOIN Course_Classroom cc ON r.course_classroom_id = cc.id
LEFT JOIN Course_Instructor ci ON c.course_id = ci.course_id
LEFT JOIN Instructors i ON ci.instructor_id = i.instructor_id
WHERE c.year_number = @year
  AND c.course_id = @courseId
  AND g.success_status = N'راسب'
ORDER BY c.course_id, cc.group_number, s.university_number;
";


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


            currentPageIndex = 0;


            PrintPreviewDialog previewDialog = new PrintPreviewDialog();

            previewDialog.Document = printDocument1;

            previewDialog.ShowDialog();

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
            //    if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            //    if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            //    if (isHandlingCellValueChanged) return;  // لمنع التكرار المتداخل
            //    string exr = comboExamRound.Text;
            //    if (exr == "دور أول")
            //    {
            //        try
            //        {
            //            isHandlingCellValueChanged = true;

            //            var row = dataGridViewGrades.Rows[e.RowIndex];

            //            int workGrade = 0, finalGrade = 0;

            //            if (int.TryParse(row.Cells["درجة الأعمال"].Value?.ToString(), out int wg))
            //                workGrade = wg;

            //            if (int.TryParse(row.Cells["درجة الامتحان النهائي"].Value?.ToString(), out int fg))
            //                finalGrade = fg;

            //            int total = workGrade + finalGrade;

            //            row.Cells["المجموع الكلي"].Value = total;
            //        }
            //        catch (Exception ex)
            //        {
            //            MessageBox.Show("حدث خطأ: " + ex.Message);
            //        }
            //        finally
            //        {
            //            isHandlingCellValueChanged = false;
            //        }
            //    }
            //    else if (exr == "دور ثاني")
            //    {
            //        try
            //        {
            //            isHandlingCellValueChanged = true;
            //            var row = dataGridViewGrades.Rows[e.RowIndex];
            //            int finalGrade = 0;
            //            if (int.TryParse(row.Cells["درجة الامتحان النهائي"].Value?.ToString(), out int fg))
            //                finalGrade = fg;
            //            row.Cells["المجموع الكلي"].Value = finalGrade;
            //        }
            //        catch (Exception ex)
            //        {
            //            MessageBox.Show("حدث خطأ: " + ex.Message);
            //        }
            //        finally
            //        {
            //            isHandlingCellValueChanged = false;
            //        }
            //    }
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (isHandlingCellValueChanged) return;  // لمنع التكرار المتداخل

            string exr = comboExamRound.Text;

            try
            {
                isHandlingCellValueChanged = true;
                var row = dataGridViewGrades.Rows[e.RowIndex];

                if (exr == "دور أول")
                {
                    // الدور الأول كما كان سابقًا
                    int workGrade = 0, finalGrade = 0;

                    if (int.TryParse(row.Cells["درجة الأعمال"].Value?.ToString(), out int wg))
                        workGrade = wg;

                    if (int.TryParse(row.Cells["درجة الامتحان النهائي"].Value?.ToString(), out int fg))
                        finalGrade = fg;

                    int total = workGrade + finalGrade;
                    row.Cells["المجموع الكلي"].Value = total;
                }
                else if (exr == "دور ثاني")
                {
                    // الدور الثاني: إذا تم تعديل "المجموع الكلي"، احسب الدرجات الفرعية تلقائيًا
                    if (e.ColumnIndex == row.Cells["المجموع الكلي"].ColumnIndex)
                    {
                        if (int.TryParse(row.Cells["المجموع الكلي"].Value?.ToString(), out int total))
                        {
                            // 40% أعمال السنة و60% امتحان نهائي
                            int finalGrade = (int)Math.Round(total * 0.6);
                            int workGrade = total - finalGrade; // لضمان أن المجموع يبقى صحيحًا

                            row.Cells["درجة الامتحان النهائي"].Value = finalGrade;
                            row.Cells["درجة الأعمال"].Value = workGrade;
                        }
                    }
                }
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




        //--------------------4----------------------------------------------------------------------------------------------------------------------
        private void button6_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("الرجاء إدخال رقم القيد للطالب.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBox2.SelectedItem == null)
            {
                MessageBox.Show("الرجاء اختيار الدور قبل التحديث.");
                return;
            }

            string selectedRound = comboBox2.SelectedItem.ToString();
            int academicYearStart = Convert.ToInt32(numericUpDownYear1.Value);
            string uniNumber = textBox1.Text.Trim();

            if (MessageBox.Show("هل أنت متأكد ؟", "تأكيد",
                                  MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // جلب student_id للطالب من رقم القيد
                            int studentId;
                            using (SqlCommand cmdStudent = new SqlCommand(
                                "SELECT student_id FROM Students WHERE university_number=@uniNumber", conn, transaction))
                            {
                                cmdStudent.Parameters.AddWithValue("@uniNumber", uniNumber);
                                var res = cmdStudent.ExecuteScalar();
                                if (res == null)
                                {
                                    MessageBox.Show("⚠️ رقم القيد غير موجود.");
                                    return;
                                }
                                studentId = Convert.ToInt32(res);
                            }

                            if (selectedRound == "دور أول")
                            {
                                string query = @"
UPDATE s
SET exam_round = CASE 
    WHEN fc.fail_count = 0 THEN N'مكتمل'
    WHEN fc.fail_count >= 1 THEN N'دور ثاني'
    ELSE s.exam_round
END
FROM Students s
INNER JOIN (
    SELECT 
        s.student_id,
        COUNT(CASE WHEN g.total_grade < 60 THEN 1 END) AS fail_count
    FROM Students s
    INNER JOIN Registrations r ON s.student_id = r.student_id
    INNER JOIN Courses c ON r.course_id = c.course_id
    LEFT JOIN Grades g ON g.student_id = s.student_id AND g.course_id = r.course_id
    WHERE r.status = N'مسجل'
      AND r.academic_year_start = @academicYearStart
      AND s.student_id = @studentId
      AND s.exam_round = N'دور أول'
    GROUP BY s.student_id
) AS fc ON s.student_id = fc.student_id;
";

                                using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@academicYearStart", academicYearStart);
                                    cmd.Parameters.AddWithValue("@studentId", studentId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else if (selectedRound == "دور ثاني")
                            {
                                string query = @"
WITH CurrentYearFails AS (
    SELECT 
        s.student_id,
        s.current_year,
        COUNT(CASE WHEN g.total_grade < 60 THEN 1 END) AS current_year_fails
    FROM Students s
    INNER JOIN Registrations r ON s.student_id = r.student_id
    INNER JOIN Courses c ON r.course_id = c.course_id
    LEFT JOIN Grades g ON r.student_id = g.student_id AND r.course_id = g.course_id
    WHERE r.status = N'مسجل'
      AND r.academic_year_start = @academicYearStart
      AND s.student_id = @studentId
      AND s.exam_round = N'دور ثاني'
    GROUP BY s.student_id, s.current_year
)
UPDATE s
SET exam_round = CASE
    WHEN cf.current_year = 4 AND cf.current_year_fails >= 1 THEN N'إعادة سنة'
    WHEN cf.current_year_fails = 0 THEN N'مكتمل'
    WHEN cf.current_year_fails BETWEEN 1 AND 2 THEN N'مرحل'
    WHEN cf.current_year_fails >= 3 THEN N'إعادة سنة'
    ELSE s.exam_round
END
FROM Students s
INNER JOIN CurrentYearFails cf ON s.student_id = cf.student_id;
";

                                using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@academicYearStart", academicYearStart);
                                    cmd.Parameters.AddWithValue("@studentId", studentId);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            MessageBox.Show("تم تحديث حالة الطالب بنجاح.");
                        }
                        catch (Exception exInner)
                        {
                            transaction.Rollback();
                            MessageBox.Show("حدث خطأ، تم التراجع عن العملية:\n" + exInner.Message);
                        }
                    }
                }
            }
            catch (Exception exOuter)
            {
                MessageBox.Show("حدث خطأ أثناء الاتصال:\n" + exOuter.Message);
            }

        }

        private void button10_Click(object sender, EventArgs e)
        {
            int selectedYear = (int)numericUpDownYear1.Value;
            string universityNumber = textBox1.Text.Trim();
            string a = comboBox2.Text;

            try
            {
                //                if (a == "دور أول")
                //                {
                //                    using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
                //                    {
                //                        conn.Open();

                //                        string query = @"
                //SELECT             
                //    ROW_NUMBER() OVER (ORDER BY s.student_id) AS رقم,
                //    s.student_id,
                //    s.university_number AS [رقم القيد],
                //    s.full_name AS [اسم الطالب],
                //    c.course_name AS [اسم المادة],
                //    c.year_number AS [السنة الدراسية للمادة],
                //    CONCAT(r.academic_year_start,'-',r.academic_year_start + 1) AS [العام الجامعي],
                //    g.work_grade AS [أعمال السنة],
                //    g.final_grade AS [الامتحان النهائي],
                //    g.total_grade AS [المجموع],
                //    g.success_status AS [الحالة],
                //    s.exam_round AS [الدور]
                //FROM Students s
                //JOIN Registrations r ON s.student_id = r.student_id
                //JOIN Courses c ON r.course_id = c.course_id
                //LEFT JOIN Grades g ON r.student_id = g.student_id AND r.course_id = g.course_id
                //WHERE r.academic_year_start = @year
                //AND s.university_number LIKE '%' + @uniNumber + '%'
                //AND s.exam_round = @examRound
                //ORDER BY s.student_id;
                //";

                //                        using (SqlCommand cmd = new SqlCommand(query, conn))
                //                        {
                //                            cmd.Parameters.AddWithValue("@year", selectedYear);
                //                            cmd.Parameters.AddWithValue("@uniNumber", universityNumber);
                //                            cmd.Parameters.AddWithValue("@examRound", a);

                //                            DataTable dt = new DataTable();
                //                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                //                            da.Fill(dt);

                //                            dataGridView1.DataSource = dt;
                //                            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                //                            if (dataGridView1.Columns.Contains("student_id"))
                //                            {
                //                                dataGridView1.Columns["student_id"].Visible = false;
                //                            }

                //                        }
                //                    }
                //                }
                //                else if (a == "دور ثاني")
                //                {

                //                }
                using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
                {
                    conn.Open();

                    string query = @"
SELECT             
    ROW_NUMBER() OVER (ORDER BY s.student_id) AS رقم,
    s.student_id,
    s.university_number AS [رقم القيد],
    s.full_name AS [اسم الطالب],
    c.course_name AS [اسم المادة],
    c.year_number AS [السنة الدراسية للمادة],
    CONCAT(r.academic_year_start,'-',r.academic_year_start + 1) AS [العام الجامعي],
    g.work_grade AS [أعمال السنة],
    g.final_grade AS [الامتحان النهائي],
    g.total_grade AS [المجموع],
    g.success_status AS [الحالة],
    s.exam_round AS [الدور]
FROM Students s
JOIN Registrations r ON s.student_id = r.student_id
JOIN Courses c ON r.course_id = c.course_id
LEFT JOIN Grades g ON r.student_id = g.student_id AND r.course_id = g.course_id
WHERE r.academic_year_start = @year
AND s.university_number LIKE '%' + @uniNumber + '%'
AND s.exam_round = @examRound
";

                    // إضافة شرط فقط للدور الثاني لإظهار المواد الراسبة
                    if (a == "دور ثاني")
                    {
                        query += " AND g.total_grade < 60";
                    }

                    query += " ORDER BY s.student_id;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@year", selectedYear);
                        cmd.Parameters.AddWithValue("@uniNumber", universityNumber);
                        cmd.Parameters.AddWithValue("@examRound", a);

                        DataTable dt = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        dataGridView1.DataSource = dt;
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                        if (dataGridView1.Columns.Contains("student_id"))
                        {
                            dataGridView1.Columns["student_id"].Visible = false;
                        }
                    }

                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء البحث: " + ex.Message);
            } 

        }

        private void button11_Click(object sender, EventArgs e)
        {
            string a = comboBox2.Text;
            if (a == "دور أول")
            {


                try
                {
                    using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
                    {
                        conn.Open();
                        using (SqlTransaction transaction = conn.BeginTransaction())
                        {
                            int insertedCount = 0, updatedCount = 0, skippedCount = 0;

                            foreach (DataGridViewRow row in dataGridView1.Rows)
                            {
                                if (row.IsNewRow) continue;

                                int studentId = Convert.ToInt32(row.Cells["student_id"].Value);
                                string courseName = row.Cells["اسم المادة"].Value?.ToString();

                                if (string.IsNullOrEmpty(courseName)) { skippedCount++; continue; }

                                int courseId;
                                using (SqlCommand cmdCourse = new SqlCommand("SELECT course_id FROM Courses WHERE course_name=@cname", conn, transaction))
                                {
                                    cmdCourse.Parameters.AddWithValue("@cname", courseName);
                                    courseId = Convert.ToInt32(cmdCourse.ExecuteScalar());
                                }

                                string workStr = row.Cells["أعمال السنة"].Value?.ToString().Trim();
                                string finalStr = row.Cells["الامتحان النهائي"].Value?.ToString().Trim();

                                bool workEmpty = string.IsNullOrEmpty(workStr);
                                bool finalEmpty = string.IsNullOrEmpty(finalStr);

                                // ====================================================
                                // تخطي الصف إذا كانت كلتا الدرجتين فارغة
                                // ====================================================
                                if (workEmpty && finalEmpty)
                                {
                                    skippedCount++;
                                    continue;
                                }

                                // جلب القيم القديمة من قاعدة البيانات
                                int dbWork = -1, dbFinal = -1;
                                using (SqlCommand cmdCheckOld = new SqlCommand(
                                    "SELECT work_grade, final_grade FROM Grades WHERE student_id=@sid AND course_id=@cid", conn, transaction))
                                {
                                    cmdCheckOld.Parameters.AddWithValue("@sid", studentId);
                                    cmdCheckOld.Parameters.AddWithValue("@cid", courseId);
                                    using (SqlDataReader reader = cmdCheckOld.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            dbWork = reader["work_grade"] != DBNull.Value ? Convert.ToInt32(reader["work_grade"]) : -1;
                                            dbFinal = reader["final_grade"] != DBNull.Value ? Convert.ToInt32(reader["final_grade"]) : -1;
                                        }
                                    }
                                }

                                // ====================================================
                                // شرط تخطي الصف إذا كانت كلتا الدرجتين موجودتين مسبقًا
                                // ====================================================
                                if (dbWork != -1 && dbFinal != -1)
                                {
                                    skippedCount++;
                                    continue;
                                }

                                int? workGrade = null;
                                int? finalGrade = null;

                                // ====================================================
                                // التحقق من Work
                                // ====================================================
                                if (!workEmpty)
                                {
                                    if (!int.TryParse(workStr, out int wg) || wg < 0 || wg > 40)
                                    {
                                        MessageBox.Show($"⚠️ أعمال السنة يجب أن تكون بين 0 و 40 للطالب {row.Cells["اسم الطالب"].Value}");
                                        skippedCount++;
                                        continue;
                                    }
                                    workGrade = wg;
                                }

                                // ====================================================
                                // التحقق من Final
                                // ====================================================
                                if (!finalEmpty)
                                {
                                    if (!int.TryParse(finalStr, out int fg) || fg < 0 || fg > 60)
                                    {
                                        MessageBox.Show($"⚠️ الامتحان النهائي يجب أن يكون بين 0 و 60 للطالب {row.Cells["اسم الطالب"].Value}");
                                        skippedCount++;
                                        continue;
                                    }
                                    finalGrade = fg;
                                }

                                // حساب المجموع فقط (الحالة تحسب عند الحفظ)
                                int? totalGrade = null;
                                if (workGrade.HasValue || finalGrade.HasValue)
                                    totalGrade = (workGrade ?? 0) + (finalGrade ?? 0);

                                // تحقق من وجود الصف في Grades
                                int count;
                                using (SqlCommand cmdCheck = new SqlCommand("SELECT COUNT(*) FROM Grades WHERE student_id=@sid AND course_id=@cid", conn, transaction))
                                {
                                    cmdCheck.Parameters.AddWithValue("@sid", studentId);
                                    cmdCheck.Parameters.AddWithValue("@cid", courseId);
                                    count = (int)cmdCheck.ExecuteScalar();
                                }

                                if (count > 0)
                                {
                                    string updateQuery = @"
UPDATE Grades
SET work_grade=@cw, final_grade=@fe, total_grade=@total, success_status=CASE WHEN (@cw + ISNULL(@fe,0)) >= 60 THEN N'نجاح' ELSE N'راسب' END
WHERE student_id=@sid AND course_id=@cid";

                                    using (SqlCommand cmdUpdate = new SqlCommand(updateQuery, conn, transaction))
                                    {
                                        cmdUpdate.Parameters.AddWithValue("@cw", (object)workGrade ?? DBNull.Value);
                                        cmdUpdate.Parameters.AddWithValue("@fe", (object)finalGrade ?? DBNull.Value);
                                        cmdUpdate.Parameters.AddWithValue("@total", (object)totalGrade ?? DBNull.Value);
                                        cmdUpdate.Parameters.AddWithValue("@sid", studentId);
                                        cmdUpdate.Parameters.AddWithValue("@cid", courseId);
                                        cmdUpdate.ExecuteNonQuery();
                                    }
                                    updatedCount++;
                                }
                                else
                                {
                                    string insertQuery = @"
INSERT INTO Grades(student_id, course_id, work_grade, final_grade, total_grade, success_status)
VALUES(@sid,@cid,@cw,@fe,@total, CASE WHEN (@cw + ISNULL(@fe,0)) >= 60 THEN N'نجاح' ELSE N'راسب' END)";

                                    using (SqlCommand cmdInsert = new SqlCommand(insertQuery, conn, transaction))
                                    {
                                        cmdInsert.Parameters.AddWithValue("@sid", studentId);
                                        cmdInsert.Parameters.AddWithValue("@cid", courseId);
                                        cmdInsert.Parameters.AddWithValue("@cw", (object)workGrade ?? DBNull.Value);
                                        cmdInsert.Parameters.AddWithValue("@fe", (object)finalGrade ?? DBNull.Value);
                                        cmdInsert.Parameters.AddWithValue("@total", (object)totalGrade ?? DBNull.Value);
                                        cmdInsert.ExecuteNonQuery();
                                    }
                                    insertedCount++;
                                }
                            }

                            transaction.Commit();
                            MessageBox.Show($"✅ تم الحفظ:\n📥 تم الإدخال: {insertedCount}\n✏️ تم التحديث: {updatedCount}\n⏭ تم التخطي: {skippedCount}");
                            button10_Click(null, null); // إعادة تحميل البيانات
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("❌ خطأ أثناء العملية: " + ex.Message);
                }
            }
            //==================================================================================================================================
            else
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
                    {
                        conn.Open();
                        using (SqlTransaction transaction = conn.BeginTransaction())
                        {
                            int insertedCount = 0, updatedCount = 0, skippedCount = 0;

                            foreach (DataGridViewRow row in dataGridView1.Rows)
                            {
                                if (row.IsNewRow) continue;

                                int studentId = Convert.ToInt32(row.Cells["student_id"].Value);
                                string courseName = row.Cells["اسم المادة"].Value?.ToString();

                                if (string.IsNullOrEmpty(courseName)) { skippedCount++; continue; }

                                int courseId;
                                using (SqlCommand cmdCourse = new SqlCommand("SELECT course_id FROM Courses WHERE course_name=@cname", conn, transaction))
                                {
                                    cmdCourse.Parameters.AddWithValue("@cname", courseName);
                                    courseId = Convert.ToInt32(cmdCourse.ExecuteScalar());
                                }

                                string workStr = row.Cells["أعمال السنة"].Value?.ToString().Trim();
                                string finalStr = row.Cells["الامتحان النهائي"].Value?.ToString().Trim();

                                bool workEmpty = string.IsNullOrEmpty(workStr);
                                bool finalEmpty = string.IsNullOrEmpty(finalStr);

                                // ====================================================
                                // تخطي الصف إذا كانت كلتا الدرجتين فارغة
                                // ====================================================
                                if (workEmpty && finalEmpty)
                                {
                                    skippedCount++;
                                    continue;
                                }

                                // جلب القيم القديمة من قاعدة البيانات
                                int dbWork = -1, dbFinal = -1;
                                using (SqlCommand cmdCheckOld = new SqlCommand(
                                    "SELECT work_grade, final_grade FROM Grades WHERE student_id=@sid AND course_id=@cid", conn, transaction))
                                {
                                    cmdCheckOld.Parameters.AddWithValue("@sid", studentId);
                                    cmdCheckOld.Parameters.AddWithValue("@cid", courseId);
                                    using (SqlDataReader reader = cmdCheckOld.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            dbWork = reader["work_grade"] != DBNull.Value ? Convert.ToInt32(reader["work_grade"]) : -1;
                                            dbFinal = reader["final_grade"] != DBNull.Value ? Convert.ToInt32(reader["final_grade"]) : -1;
                                        }
                                    }
                                }

                              

                                int? workGrade = null;
                                int? finalGrade = null;

                                // ====================================================
                                // التحقق من Work
                                // ====================================================
                                if (!workEmpty)
                                {
                                    if (!int.TryParse(workStr, out int wg) || wg < 0 || wg > 40)
                                    {
                                        MessageBox.Show($"⚠️ أعمال السنة يجب أن تكون بين 0 و 40 للطالب {row.Cells["اسم الطالب"].Value}");
                                        skippedCount++;
                                        continue;
                                    }
                                    workGrade = wg;
                                }

                                // ====================================================
                                // التحقق من Final
                                // ====================================================
                                if (!finalEmpty)
                                {
                                    if (!int.TryParse(finalStr, out int fg) || fg < 0 || fg > 60)
                                    {
                                        MessageBox.Show($"⚠️ الامتحان النهائي يجب أن يكون بين 0 و 60 للطالب {row.Cells["اسم الطالب"].Value}");
                                        skippedCount++;
                                        continue;
                                    }
                                    finalGrade = fg;
                                }

                                // حساب المجموع فقط (الحالة تحسب عند الحفظ)
                                int? totalGrade = null;
                                if (workGrade.HasValue || finalGrade.HasValue)
                                    totalGrade = (workGrade ?? 0) + (finalGrade ?? 0);

                                // تحقق من وجود الصف في Grades
                                int count;
                                using (SqlCommand cmdCheck = new SqlCommand("SELECT COUNT(*) FROM Grades WHERE student_id=@sid AND course_id=@cid", conn, transaction))
                                {
                                    cmdCheck.Parameters.AddWithValue("@sid", studentId);
                                    cmdCheck.Parameters.AddWithValue("@cid", courseId);
                                    count = (int)cmdCheck.ExecuteScalar();
                                }

                                if (count > 0)
                                {
                                    string updateQuery = @"
UPDATE Grades
SET work_grade=@cw, final_grade=@fe, total_grade=@total, success_status=CASE WHEN (@cw + ISNULL(@fe,0)) >= 60 THEN N'نجاح' ELSE N'راسب' END
WHERE student_id=@sid AND course_id=@cid";

                                    using (SqlCommand cmdUpdate = new SqlCommand(updateQuery, conn, transaction))
                                    {
                                        cmdUpdate.Parameters.AddWithValue("@cw", (object)workGrade ?? DBNull.Value);
                                        cmdUpdate.Parameters.AddWithValue("@fe", (object)finalGrade ?? DBNull.Value);
                                        cmdUpdate.Parameters.AddWithValue("@total", (object)totalGrade ?? DBNull.Value);
                                        cmdUpdate.Parameters.AddWithValue("@sid", studentId);
                                        cmdUpdate.Parameters.AddWithValue("@cid", courseId);
                                        cmdUpdate.ExecuteNonQuery();
                                    }
                                    updatedCount++;
                                }
                                else
                                {
                                    string insertQuery = @"
INSERT INTO Grades(student_id, course_id, work_grade, final_grade, total_grade, success_status)
VALUES(@sid,@cid,@cw,@fe,@total, CASE WHEN (@cw + ISNULL(@fe,0)) >= 60 THEN N'نجاح' ELSE N'راسب' END)";

                                    using (SqlCommand cmdInsert = new SqlCommand(insertQuery, conn, transaction))
                                    {
                                        cmdInsert.Parameters.AddWithValue("@sid", studentId);
                                        cmdInsert.Parameters.AddWithValue("@cid", courseId);
                                        cmdInsert.Parameters.AddWithValue("@cw", (object)workGrade ?? DBNull.Value);
                                        cmdInsert.Parameters.AddWithValue("@fe", (object)finalGrade ?? DBNull.Value);
                                        cmdInsert.Parameters.AddWithValue("@total", (object)totalGrade ?? DBNull.Value);
                                        cmdInsert.ExecuteNonQuery();
                                    }
                                    insertedCount++;
                                }
                            }

                            transaction.Commit();
                            MessageBox.Show($"✅ تم الحفظ:\n📥 تم الإدخال: {insertedCount}\n✏️ تم التحديث: {updatedCount}\n⏭ تم التخطي: {skippedCount}");
                            button10_Click(null, null); // إعادة تحميل البيانات
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("❌ خطأ أثناء العملية: " + ex.Message);
                }
            }

        }


        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            //var row = dataGridView1.Rows[e.RowIndex];
            //if (row.IsNewRow) return;

            //string workStr = row.Cells["أعمال السنة"].Value?.ToString();
            //string finalStr = row.Cells["الامتحان النهائي"].Value?.ToString();

            //bool workEmpty = string.IsNullOrEmpty(workStr);
            //bool finalEmpty = string.IsNullOrEmpty(finalStr);

            //if (workEmpty && finalEmpty)
            //{
            //    row.Cells["المجموع"].Value = DBNull.Value;
            //    return;
            //}

            //int work = 0, finalGrade = 0;
            //if (!workEmpty && int.TryParse(workStr, out int wg)) work = wg;
            //if (!finalEmpty && int.TryParse(finalStr, out int fg)) finalGrade = fg;

            //int total = work + finalGrade;
            //row.Cells["المجموع"].Value = total;

            //// حذف حساب الحالة من DataGridView
            //row.Cells["الحالة"].Value = "";
            var row = dataGridView1.Rows[e.RowIndex];
            if (row.IsNewRow) return;

            string exr = comboBox2.Text;

            if (exr == "دور أول")
            {
                // حساب المجموع من أعمال السنة + الامتحان النهائي
                string workStr = row.Cells["أعمال السنة"].Value?.ToString();
                string finalStr = row.Cells["الامتحان النهائي"].Value?.ToString();

                bool workEmpty = string.IsNullOrEmpty(workStr);
                bool finalEmpty = string.IsNullOrEmpty(finalStr);

                if (workEmpty && finalEmpty)
                {
                    row.Cells["المجموع"].Value = DBNull.Value;
                    return;
                }

                int work = 0, finalGrade = 0;
                if (!workEmpty && int.TryParse(workStr, out int wg)) work = wg;
                if (!finalEmpty && int.TryParse(finalStr, out int fg)) finalGrade = fg;

                row.Cells["المجموع"].Value = work + finalGrade;
            }
            else if (exr == "دور ثاني")
            {
                // إذا تم تعديل المجموع فقط، احسب الأعمال والنهائي تلقائيًا
                string totalStr = row.Cells["المجموع"].Value?.ToString();
                if (int.TryParse(totalStr, out int total))
                {
                    int finalGrade = (int)Math.Round(total * 0.6);  // 60% امتحان نهائي
                    int work = total - finalGrade;                 // 40% أعمال السنة

                    row.Cells["الامتحان النهائي"].Value = finalGrade;
                    row.Cells["أعمال السنة"].Value = work;
                }
            }

            // اترك عمود الحالة فارغ
            row.Cells["الحالة"].Value = "";

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            bool isApproved = checkBox1.Checked; // true لو معتمد، false لو مش معتمد

            using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE Months SET is_approved = @isApproved WHERE month_id = 1", conn))
                {
                    cmd.Parameters.AddWithValue("@isApproved", isApproved);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

}
