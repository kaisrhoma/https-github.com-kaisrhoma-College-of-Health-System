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

namespace college_of_health_sciences.dashboards.exams_dashboards
{
    public partial class grads_management : UserControl
    {

        private readonly string connectionString = @"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;";
       // SqlConnection conn = new SqlConnection("Data Source=.;Initial Catalog=Cohs_DB;Integrated Security=True");


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
                comboBox_Year.Items.Add("1");
                comboBox_Year.Items.Add("2");
                comboBox_Year.Items.Add("3");
                comboBox_Year.Items.Add("4");

                comboBox_Year.SelectedIndex = 0;
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

            int selectedYear;
            if (!int.TryParse(comboBox_Year.SelectedItem.ToString(), out selectedYear))
            {
                MessageBox.Show("السنة الدراسية غير صالحة.");
                return;
            }

            int courseId = Convert.ToInt32(comboBox_Course.SelectedValue);

            string connectionString = @"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
            SELECT 
                s.student_id, 
                s.university_number, 
                s.full_name, 
                d.dep_name, 
                g.final_grade, 
                g.success_status,
                c.course_name, 
                c.course_id,
                i.full_name AS instructor_name
            FROM Grades g
            INNER JOIN Students s ON g.student_id = s.student_id
            INNER JOIN Departments d ON s.department_id = d.department_id
            INNER JOIN Courses c ON g.course_id = c.course_id
            INNER JOIN Course_Instructor ci ON c.course_id = ci.course_id
            INNER JOIN Instructors i ON ci.instructor_id = i.instructor_id
            WHERE g.course_id = @courseId 
              AND c.year_number = @year
              AND g.success_status = N'رسوب'
        ";

                //    string query = @"
                //SELECT s.student_id, s.full_name, s.university_number, g.final_grade, g.success_status
                //FROM Grades g
                //INNER JOIN Students s ON g.student_id = s.student_id
                //INNER JOIN Courses c ON g.course_id = c.course_id
                //WHERE g.course_id = @courseId 
                //  AND c.year_number = @year
                //  AND g.success_status = N'رسوب'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@courseId", courseId);
                    cmd.Parameters.AddWithValue("@year", selectedYear);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dataGridView3.DataSource = dt;

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("لا يوجد طلاب راسبين في هذه المادة والسنة الدراسية.");
                    }
                }
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (dataGridView3.Rows.Count == 0)
            {
                MessageBox.Show("لا يوجد بيانات للطباعة.");
                return;
            }

            PrintDocument printDoc = new PrintDocument();
            printDoc.DefaultPageSettings.Landscape = false;
            printDoc.PrintPage += PrintDoc_PrintPage;

            PrintPreviewDialog preview = new PrintPreviewDialog();
            preview.Document = printDoc;
            preview.ShowDialog();
        }

        private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            int marginLeft = e.MarginBounds.Left;
            int marginTop = e.MarginBounds.Top;
            int pageWidth = e.MarginBounds.Width;

            Font titleFont = new Font("Arial", 16, FontStyle.Bold);
            Font headerFont = new Font("Arial", 12, FontStyle.Bold);
            Font cellFont = new Font("Arial", 11);
            Brush blackBrush = Brushes.Black;

            StringFormat rightAlign = new StringFormat()
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.DirectionRightToLeft
            };

            StringFormat centerAlign = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.DirectionRightToLeft
            };

            int currentY = marginTop;

            // --- رأس الصفحة ---
            string[] headerLines = new string[]
            {
        "دولة ليبيا",
        "وزارة التعليم",
        "جامعة غريان",
        "كلية العلوم الصحية",
        $"التاريخ: {DateTime.Now.ToString("yyyy/MM/dd")}"
            };

            foreach (var line in headerLines)
            {
                g.DrawString(line, titleFont, blackBrush, new RectangleF(marginLeft, currentY, pageWidth, 30), centerAlign);
                currentY += 30;
            }

            currentY += 10; // مسافة بعد الرأس

            // --- عنوان الكشف ---
            string reportTitle = "كشف الطلاب الراسبين";
            g.DrawString(reportTitle, titleFont, blackBrush, new RectangleF(marginLeft, currentY, pageWidth, 30), centerAlign);
            currentY += 40;

            // --- معلومات المادة (يمكن تعديل المتغيرات حسب بياناتك) ---
            // افترض أن هذه البيانات مأخوذة من مكان ما (مثلاً من اختيار الكمبوكس)
            string courseName = "مقدمة في الحاسوب"; // اسم المادة
            string courseId = "43";                  // رقم المادة
            string instructorName = "أ. محمد علي";  // اسم المحاضر
            int studentsCount = dataGridView3.Rows.Count;

            // عرض معلومات المادة في جدول صغير
            int infoColWidth = pageWidth / 4;
            int infoRowHeight = 30;

            // مستطيلات كل عمود
            Rectangle rectCourseNameTitle = new Rectangle(marginLeft + infoColWidth * 0, currentY, infoColWidth, infoRowHeight);
            Rectangle rectCourseNameValue = new Rectangle(marginLeft + infoColWidth * 0, currentY + infoRowHeight, infoColWidth, infoRowHeight);

            Rectangle rectCourseIdTitle = new Rectangle(marginLeft + infoColWidth * 1, currentY, infoColWidth, infoRowHeight);
            Rectangle rectCourseIdValue = new Rectangle(marginLeft + infoColWidth * 1, currentY + infoRowHeight, infoColWidth, infoRowHeight);

            Rectangle rectInstructorTitle = new Rectangle(marginLeft + infoColWidth * 2, currentY, infoColWidth, infoRowHeight);
            Rectangle rectInstructorValue = new Rectangle(marginLeft + infoColWidth * 2, currentY + infoRowHeight, infoColWidth, infoRowHeight);

            Rectangle rectCountTitle = new Rectangle(marginLeft + infoColWidth * 3, currentY, infoColWidth, infoRowHeight);
            Rectangle rectCountValue = new Rectangle(marginLeft + infoColWidth * 3, currentY + infoRowHeight, infoColWidth, infoRowHeight);

            // رسم رؤوس الأعمدة
            g.FillRectangle(Brushes.LightGray, rectCourseNameTitle);
            g.FillRectangle(Brushes.LightGray, rectCourseIdTitle);
            g.FillRectangle(Brushes.LightGray, rectInstructorTitle);
            g.FillRectangle(Brushes.LightGray, rectCountTitle);

            g.DrawRectangle(Pens.Black, rectCourseNameTitle);
            g.DrawRectangle(Pens.Black, rectCourseIdTitle);
            g.DrawRectangle(Pens.Black, rectInstructorTitle);
            g.DrawRectangle(Pens.Black, rectCountTitle);

            g.DrawString("اسم المادة", headerFont, blackBrush, rectCourseNameTitle, centerAlign);
            g.DrawString("رقم المادة", headerFont, blackBrush, rectCourseIdTitle, centerAlign);
            g.DrawString("اسم المحاضر", headerFont, blackBrush, rectInstructorTitle, centerAlign);
            g.DrawString("عدد الطلاب", headerFont, blackBrush, rectCountTitle, centerAlign);

            // رسم قيم الأعمدة
            g.DrawRectangle(Pens.Black, rectCourseNameValue);
            g.DrawRectangle(Pens.Black, rectCourseIdValue);
            g.DrawRectangle(Pens.Black, rectInstructorValue);
            g.DrawRectangle(Pens.Black, rectCountValue);

            g.DrawString(courseName, cellFont, blackBrush, rectCourseNameValue, centerAlign);
            g.DrawString(courseId, cellFont, blackBrush, rectCourseIdValue, centerAlign);
            g.DrawString(instructorName, cellFont, blackBrush, rectInstructorValue, centerAlign);
            g.DrawString(studentsCount.ToString(), cellFont, blackBrush, rectCountValue, centerAlign);

            currentY += infoRowHeight * 2 + 20; // اضافة مسافة بعد معلومات المادة

            // --- جدول الطلاب الراسبين ---

            // تعريف أعمدة الجدول
            int colWidth_Id = 80;
            int colWidth_Name = 200;
            int colWidth_Department = 150;
            int colWidth_Grade = 70;
            int colWidth_Result = 70;

            // رسم رؤوس الأعمدة
            Rectangle rId = new Rectangle(marginLeft, currentY, colWidth_Id, infoRowHeight);
            Rectangle rName = new Rectangle(marginLeft + colWidth_Id, currentY, colWidth_Name, infoRowHeight);
            Rectangle rDept = new Rectangle(marginLeft + colWidth_Id + colWidth_Name, currentY, colWidth_Department, infoRowHeight);
            Rectangle rGrade = new Rectangle(marginLeft + colWidth_Id + colWidth_Name + colWidth_Department, currentY, colWidth_Grade, infoRowHeight);
            Rectangle rResult = new Rectangle(marginLeft + colWidth_Id + colWidth_Name + colWidth_Department + colWidth_Grade, currentY, colWidth_Result, infoRowHeight);

            // خلفية الرؤوس
            g.FillRectangle(Brushes.LightGray, rId);
            g.FillRectangle(Brushes.LightGray, rName);
            g.FillRectangle(Brushes.LightGray, rDept);
            g.FillRectangle(Brushes.LightGray, rGrade);
            g.FillRectangle(Brushes.LightGray, rResult);

            // حدود الرؤوس
            g.DrawRectangle(Pens.Black, rId);
            g.DrawRectangle(Pens.Black, rName);
            g.DrawRectangle(Pens.Black, rDept);
            g.DrawRectangle(Pens.Black, rGrade);
            g.DrawRectangle(Pens.Black, rResult);

            // نص الرؤوس (محاذاة يمين أو وسط حسب المطلوب)
            g.DrawString("رقم القيد", headerFont, blackBrush, rId, rightAlign);
            g.DrawString("اسم الطالب", headerFont, blackBrush, rName, rightAlign);
            g.DrawString("القسم", headerFont, blackBrush, rDept, rightAlign);
            g.DrawString("الدرجة", headerFont, blackBrush, rGrade, rightAlign);
            g.DrawString("النتيجة", headerFont, blackBrush, rResult, rightAlign);

            currentY += infoRowHeight;

            int rowHeight = 30;

            // رسم بيانات الطلاب
            foreach (DataGridViewRow row in dataGridView3.Rows)
            {
                if (row.IsNewRow) continue;

                string studentId = row.Cells["student_id"].Value?.ToString() ?? "";
                string studentName = row.Cells["full_name"].Value?.ToString() ?? "";
                string department = row.Cells["dep_name"]?.ToString() ?? ""; // تأكد أن لديك هذا الحقل في DataGridView أو اجلبه من DB
                string grade = row.Cells["final_grade"].Value?.ToString() ?? "";
                string result = row.Cells["success_status"].Value?.ToString() ?? "";

                Rectangle rrId = new Rectangle(marginLeft, currentY, colWidth_Id, rowHeight);
                Rectangle rrName = new Rectangle(marginLeft + colWidth_Id, currentY, colWidth_Name, rowHeight);
                Rectangle rrDept = new Rectangle(marginLeft + colWidth_Id + colWidth_Name, currentY, colWidth_Department, rowHeight);
                Rectangle rrGrade = new Rectangle(marginLeft + colWidth_Id + colWidth_Name + colWidth_Department, currentY, colWidth_Grade, rowHeight);
                Rectangle rrResult = new Rectangle(marginLeft + colWidth_Id + colWidth_Name + colWidth_Department + colWidth_Grade, currentY, colWidth_Result, rowHeight);

                // رسم الحدود
                g.DrawRectangle(Pens.Black, rrId);
                g.DrawRectangle(Pens.Black, rrName);
                g.DrawRectangle(Pens.Black, rrDept);
                g.DrawRectangle(Pens.Black, rrGrade);
                g.DrawRectangle(Pens.Black, rrResult);

                // طباعة النصوص
                g.DrawString(studentId, cellFont, blackBrush, rrId, rightAlign);
                g.DrawString(studentName, cellFont, blackBrush, rrName, rightAlign);
                g.DrawString(department, cellFont, blackBrush, rrDept, rightAlign);
                g.DrawString(grade, cellFont, blackBrush, rrGrade, rightAlign);
                g.DrawString(result, cellFont, blackBrush, rrResult, rightAlign);

                currentY += rowHeight;

                // تحقق من تجاوز الصفحة
                if (currentY + rowHeight > e.MarginBounds.Bottom)
                {
                    e.HasMorePages = true;
                    return;
                }
            }

            e.HasMorePages = false;
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
    }
}
