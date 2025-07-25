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
    public partial class grads_management : UserControl
    {

        private readonly string connectionString = @"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;";


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

                comboBox1.DataSource = dt;
                comboBox1.DisplayMember = "course_name";
                comboBox1.ValueMember = "course_id";
                comboBox1.SelectedIndex = -1; // لا شيء محدد افتراضياً
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading departments: " + ex.Message);
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
                MessageBox.Show("Error loading courses: " + ex.Message);
            }
        }

        private void LoadStudents(int courseId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
                {
                    conn.Open();

                    // جلب الطلاب المسجلين في المقرر مع درجاتهم (إن وجدت)
                    string query = @"
                SELECT 
                    s.student_id, 
                    s.full_name, 
                    ISNULL(g.final_grade, NULL) AS final_grade
                FROM Students s
                INNER JOIN Registrations r ON s.student_id = r.student_id
                LEFT JOIN Grades g ON s.student_id = g.student_id AND g.course_id = r.course_id
                WHERE r.course_id = @courseId AND r.status = 'مسجل'
                ORDER BY s.full_name";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@courseId", courseId);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dataGridViewGrades.DataSource = dt;

                        // اجعل أعمدة ID واسم الطالب غير قابلة للتعديل
                        dataGridViewGrades.Columns["student_id"].ReadOnly = true;
                        dataGridViewGrades.Columns["full_name"].ReadOnly = true;

                        // العمود final_grade قابل للتحرير فقط
                        dataGridViewGrades.Columns["final_grade"].ReadOnly = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading students: " + ex.Message);
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

            try
            {
                using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
                {
                    conn.Open();
                    int insertedCount = 0;
                    int skippedCount = 0;

                    foreach (DataGridViewRow row in dataGridViewGrades.Rows)
                    {
                        if (row.IsNewRow) continue;

                        int studentId = Convert.ToInt32(row.Cells["student_id"].Value);
                        int courseId = Convert.ToInt32(comboCourse.SelectedValue);
                        double finalGrade;

                        if (!double.TryParse(row.Cells["final_grade"].Value?.ToString(), out finalGrade))
                        {
                            MessageBox.Show($"الدرجة غير صالحة للطالب {row.Cells["full_name"].Value}");
                            return;
                        }

                        // تحقق هل الدرجة NULL (غير مدخلة مسبقاً)
                        string checkQuery = "SELECT final_grade FROM Grades WHERE student_id = @studentId AND course_id = @courseId";
                        object existingGradeObj;

                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@studentId", studentId);
                            checkCmd.Parameters.AddWithValue("@courseId", courseId);
                            existingGradeObj = checkCmd.ExecuteScalar();
                        }

                        if (existingGradeObj == DBNull.Value || existingGradeObj == null)
                        {
                            // الدرجة غير مدخلة مسبقًا، أدخلها
                            string insertQuery = @"
                    UPDATE Grades 
                    SET final_grade = @finalGrade, 
                        success_status = CASE WHEN @finalGrade >= 50 THEN N'نجاح' ELSE N'رسوب' END
                    WHERE student_id = @studentId AND course_id = @courseId";

                            // لكن اذا الصف موجود مسبقا في جدول الدرجات ولكن الدرجة فارغة (NULL) نستخدم UPDATE وليس INSERT
                            // لذلك ننفذ تحديث بدل إدخال جديد، لكن إذا لم يكن الصف موجود، نحتاج لإضافة الصف!

                            // إذا كنت متأكد ان الصف موجود مع null فقط نستخدم UPDATE
                            // وإلا تحتاج منطق إضافي لإنشاء صف جديد (INSERT) لو الصف غير موجود

                            using (SqlCommand updateCmd = new SqlCommand(insertQuery, conn))
                            {
                                updateCmd.Parameters.AddWithValue("@studentId", studentId);
                                updateCmd.Parameters.AddWithValue("@courseId", courseId);
                                updateCmd.Parameters.AddWithValue("@finalGrade", finalGrade);
                                updateCmd.ExecuteNonQuery();
                            }

                            // سجل العملية في Audit_Log
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
                        else
                        {
                            // الدرجة موجودة مسبقًا، لا تسمح بالتعديل
                            skippedCount++;
                        }
                    }

                    MessageBox.Show($"تم إدخال الدرجات بنجاح.\nتمت إضافة {insertedCount} طالب.\nتم تجاهل {skippedCount} طالب (لأن لديهم درجات بالفعل ولا يمكن تعديلها).");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء حفظ الدرجات: " + ex.Message);
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

        private void LoadAllGrades()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT g.grade_id, s.university_number, s.full_name, g.final_grade
                FROM Grades g
                INNER JOIN Students s ON g.student_id = s.student_id
                WHERE g.course_id = @courseId";

                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                adapter.SelectCommand.Parameters.AddWithValue("@courseId", comboCourse.SelectedValue);

                DataTable dt = new DataTable();
                adapter.Fill(dt);

                dataGridView2.DataSource = dt;
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {


            string universityNumber = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(universityNumber))
            {
                MessageBox.Show("من فضلك أدخل رقم الجامعة للبحث.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
    SELECT 
        g.grade_id,
        s.student_id,
        s.university_number, 
        s.full_name, 
        ISNULL(CAST(g.final_grade AS NVARCHAR(10)), N'غير مرصودة') AS final_grade
    FROM Students s
    LEFT JOIN Grades g ON s.student_id = g.student_id AND g.course_id = @courseId
    WHERE s.university_number = @universityNumber";

                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                adapter.SelectCommand.Parameters.AddWithValue("@universityNumber", universityNumber);
                adapter.SelectCommand.Parameters.AddWithValue("@courseId", comboBox1.SelectedValue); // تأكد من اسم الكومبو

                DataTable dt = new DataTable();
                adapter.Fill(dt);

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("لا يوجد طالب بهذا الرقم.");
                }

                dataGridView2.DataSource = dt;

                // لتسهيل التعديل: تحويل "غير مرصودة" إلى نص فارغ في الخلية عند تحريرها
                dataGridView2.CellBeginEdit += (s, eventArgs) =>
                {
                    if (dataGridView2.Columns[eventArgs.ColumnIndex].Name == "final_grade")
                    {
                        var val = dataGridView2.Rows[eventArgs.RowIndex].Cells[eventArgs.ColumnIndex].Value?.ToString();
                        if (val == "غير مرصودة")
                            dataGridView2.Rows[eventArgs.RowIndex].Cells[eventArgs.ColumnIndex].Value = "";
                    }
                };

                // لمنع المستخدم من تعديل بيانات أخرى غير الدرجة فقط
                foreach (DataGridViewColumn col in dataGridView2.Columns)
                {
                    if (col.Name != "final_grade")
                        col.ReadOnly = true;
                }
            }


        }



        private void button4_Click(object sender, EventArgs e)
        {
            LoadAllGrades();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
                {
                    conn.Open();

                    foreach (DataGridViewRow row in dataGridView2.Rows)
                    {
                        if (row.IsNewRow) continue;

                        int gradeId = Convert.ToInt32(row.Cells["grade_id"].Value);
                        double finalGrade;

                        if (!double.TryParse(row.Cells["final_grade"].Value?.ToString(), out finalGrade))
                        {
                            MessageBox.Show($"الدرجة غير صالحة للطالب {row.Cells["full_name"].Value}");
                            return;
                        }

                        // تحديث الدرجة والنجاح/الرسوب
                        string updateQuery = @"
                    UPDATE Grades
                    SET final_grade = @finalGrade,
                        success_status = CASE WHEN @finalGrade >= 50 THEN N'نجاح' ELSE N'رسوب' END
                    WHERE grade_id = @gradeId";

                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@finalGrade", finalGrade);
                            cmd.Parameters.AddWithValue("@gradeId", gradeId);
                            cmd.ExecuteNonQuery();
                        }

                        // تسجيل التعديل في سجل العمليات
                        string auditQuery = @"
                    INSERT INTO Audit_Log (user_id, action, table_name, record_id)
                    VALUES (@userId, 'UPDATE', 'Grades', @recordId)";

                        using (SqlCommand auditCmd = new SqlCommand(auditQuery, conn))
                        {
                            auditCmd.Parameters.AddWithValue("@userId", Session.userID);
                            auditCmd.Parameters.AddWithValue("@recordId", gradeId);
                            auditCmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("تم حفظ التعديلات بنجاح.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء الحفظ: " + ex.Message);
            }
        }
    }
}
