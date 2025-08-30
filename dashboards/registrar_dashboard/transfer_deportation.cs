using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Office.Word;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace college_of_health_sciences.dashboards.registrar_dashboard
{
    public partial class transfer_deportation : UserControl
    {
        SqlConnection con = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=Cohs_DB;Integrated Security=True");
        public transfer_deportation()
        {
            InitializeComponent();
        }
        public void datagridviewstyle(DataGridView datagrid)
        {
            datagrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            datagrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            datagrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        public class DownloadResult
        {
            public int StudentId { get; set; }                 // رقم الطالب
            public int RegisteredCourses { get; set; } = 0;   // عدد المواد التي تم تسجيلها
            public List<string> FullCourses { get; set; } = new List<string>(); // المواد التي لم يتم تسجيلها بسبب امتلاء المقاعد
            public string Error { get; set; } = "";           // أي خطأ حدث أثناء العملية

            // خصائص مساعدة للعرض السريع
            public bool AllCoursesFull => RegisteredCourses == 0 && FullCourses.Count > 0;
            public bool SomeCoursesFull => RegisteredCourses > 0 && FullCourses.Count > 0;
            public bool HasAnyRegistered => RegisteredCourses > 0;
        }



        private void SearchStudent()
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                conn.DatabaseConnection db2 = new conn.DatabaseConnection();
                SqlConnection con2 = db2.OpenConnection();

                string q2 = "SELECT s.student_id, s.university_number AS الرقم_الجامعي,s.full_name الإسم,d.dep_name AS القسم,s.current_year,t.description AS الحالة,s.gender,s.nationality AS الجنسية,s.exam_round AS الدور FROM Students s JOIN " +
                    "Departments d ON s.department_id = d.department_id JOIN Status t ON s.status_id = t.status_id WHERE university_number = @university_number";

                try
                {
                    SqlCommand cmd = new SqlCommand(q2, con2);
                    cmd.Parameters.AddWithValue("@university_number", textBox1.Text.Trim());

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

                    // إخفاء الأعمدة الأصلية
                    dataGridView1.Columns["gender"].Visible = false;
                    dataGridView1.Columns["current_year"].Visible = false;


                    // عرض الأعمدة النصية بدلاً منها
                    dataGridView1.Columns["GenderText"].HeaderText = "الجنس";
                    dataGridView1.Columns["yearText"].HeaderText = "السنة";


                    // باقي التنسيق
                    datagridviewstyle(dataGridView1);
                    dataGridView1.Columns["student_id"].Visible = false;


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
        private void button5_Click(object sender, EventArgs e)
        {
            SearchStudent();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button5_Click(null, null);
                e.SuppressKeyPress = true;
            }
        }

        private void transfer_deportation_Load(object sender, EventArgs e)
        {
            textBox1.Focus();
            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    string q = "select * from Departments";
                    SqlDataAdapter da = new SqlDataAdapter(q, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    comboBox2.DataSource = new BindingSource(dt, null);
                    comboBox2.DisplayMember = "dep_name";
                    comboBox2.ValueMember = "department_id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There is an Error : " + ex.Message);
            }

            try
            {
                // نملأ الكومبوبوكس بالقيم من 1 إلى 9
                for (int i = 1; i <= 12; i++)
                {
                    comboBox1.Items.Add(i);
                }

                // نجيب القيمة الافتراضية من جدول Months
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    SqlCommand cmd = new SqlCommand("SELECT TOP 1 month_number FROM Months", con);
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        int monthNumber = Convert.ToInt32(result);
                        comboBox1.SelectedItem = monthNumber; // تحديد القيمة الحالية
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في تحميل الأشهر: " + ex.Message);
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0 || dataGridView1.Rows[0].IsNewRow)
            {
                MessageBox.Show("يرجى البحث عن الطالب اولا");
                return;
            }
            int stu_id = Convert.ToInt32(dataGridView1.Rows[0].Cells["student_id"].Value.ToString());
            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    if (!isStudentHaveRegistrations(stu_id))
                    {
                        string updateQuery = @"
                    UPDATE Students SET
                    department_id = @department_id 
                    WHERE student_id = @student_id";
                        using (SqlCommand cmdnoreg = new SqlCommand(updateQuery, con))
                        {
                            cmdnoreg.Parameters.AddWithValue("@student_id", stu_id);
                            cmdnoreg.Parameters.AddWithValue("@department_id", comboBox2.SelectedValue);
                            int rowsAffected = cmdnoreg.ExecuteNonQuery();
                            MessageBox.Show(rowsAffected > 0 ? "تم تغيير القسم بنجاح." : "لم يتم تغيير القسم.");
                            button5_Click(null, null);
                        }
                    }
                    else
                    {
                        DialogResult answer = MessageBox.Show(
                            "هل انت متأكد من تبدل القسم للطالب ؟\nللعلم سيتم مسح كل المواد في القسم السابق\nوالتي غير موجودة في القسم الحالي\nوبما في ذلك الدرجات\n\nللتأكيد اضغط :\n- نعم\nللتراجع اضغط :\n- لا",
                            "تأكيد التحويل",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation,
                            MessageBoxDefaultButton.Button2,
                            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                        );

                        if (answer == DialogResult.Yes)
                        {
                            string updateq = @"
                        -- حذف الدرجات للمواد التي لا تنتمي للقسم الجديد
                        DELETE g
                        FROM Grades g
                        WHERE g.student_id = @studentId
                        AND g.course_id NOT IN (
                            SELECT course_id
                            FROM Course_Department
                            WHERE department_id = @newDeptId
                        );

                        -- حذف التسجيلات للمواد التي لا تنتمي للقسم الجديد
                        DELETE r
                        FROM Registrations r
                        WHERE r.student_id = @studentId
                        AND r.course_id NOT IN (
                            SELECT course_id
                            FROM Course_Department
                            WHERE department_id = @newDeptId
                        );

                        -- تحديث القسم للطالب
                        UPDATE Students
                        SET department_id = @newDeptId
                        WHERE student_id = @studentId;
                    ";

                            using (SqlCommand cmdupdate = new SqlCommand(updateq, con))
                            {
                                cmdupdate.Parameters.AddWithValue("@studentId", stu_id);
                                cmdupdate.Parameters.AddWithValue("@newDeptId", comboBox2.SelectedValue);
                                int rows = cmdupdate.ExecuteNonQuery(); // تنفذ الاستعلامات
                                MessageBox.Show("تم التحويل بنجاح");
                                button5_Click(null, null);
                            }
                        }
                        else
                        {
                            MessageBox.Show("لم يتم التحويل");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There is an Error : " + ex.Message);
            }
        }


        public bool isStudentHaveRegistrations(int id)
        {
            int value = 0;
            try
            {
                conn.DatabaseConnection db1 = new conn.DatabaseConnection();
                using (SqlConnection con1 = db1.OpenConnection())
                {
                    using (SqlCommand cmd1 = new SqlCommand("SELECT COUNT(*) FROM Registrations WHERE student_id = @student_id",con1))
                    {
                        cmd1.Parameters.AddWithValue("@student_id", id);
                        value = Convert.ToInt32(cmd1.ExecuteScalar());
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("There is an Error : " + ex.Message);
            }
            if (value > 0)
            return true;
            else return false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
        }


        public DownloadResult DownloadForOneStudent(int studentId, int year, int deptId, int academicYearStart)
        {
            DownloadResult result = new DownloadResult { StudentId = studentId };

            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    // تحقق أن الطالب من القسم
                    SqlCommand checkCmd = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM Students 
                WHERE student_id = @studentId AND department_id = @departmentId", con);

                    checkCmd.Parameters.AddWithValue("@studentId", studentId);
                    checkCmd.Parameters.AddWithValue("@departmentId", deptId);
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count == 0) return result;

                    // جلب المواد للسنة والقسم
                    SqlCommand coursesCmd = new SqlCommand(@"
                SELECT c.course_id, c.course_name
                FROM Courses c
                JOIN Course_Department cd ON cd.course_id = c.course_id
                WHERE c.year_number = @year AND cd.department_id = @dept", con);

                    coursesCmd.Parameters.AddWithValue("@year", year);
                    coursesCmd.Parameters.AddWithValue("@dept", deptId);

                    DataTable courses = new DataTable();
                    new SqlDataAdapter(coursesCmd).Fill(courses);

                    foreach (DataRow row in courses.Rows)
                    {
                        int courseId = Convert.ToInt32(row["course_id"]);
                        string courseName = row["course_name"].ToString();

                        // جلب مجموعات المادة
                        SqlCommand getGroupsCmd = new SqlCommand(@"
                    SELECT id, capacity, group_number
                    FROM Course_Classroom
                    WHERE course_id = @courseId
                    ORDER BY group_number", con);
                        getGroupsCmd.Parameters.AddWithValue("@courseId", courseId);

                        DataTable groups = new DataTable();
                        new SqlDataAdapter(getGroupsCmd).Fill(groups);

                        bool added = false;
                        foreach (DataRow group in groups.Rows)
                        {
                            int groupId = Convert.ToInt32(group["id"]);
                            int capacity = Convert.ToInt32(group["capacity"]);

                            SqlCommand countCmd = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM Registrations 
                        WHERE course_classroom_id = @groupId", con);
                            countCmd.Parameters.AddWithValue("@groupId", groupId);

                            int currentCount = (int)countCmd.ExecuteScalar();
                            if (currentCount < capacity)
                            {
                                SqlCommand insertCmd = new SqlCommand(@"
                            IF NOT EXISTS (
                                SELECT 1 FROM Registrations 
                                WHERE student_id = @studentId AND course_id = @courseId
                            )
                            INSERT INTO Registrations 
                            (student_id, course_id, year_number, status, course_classroom_id, academic_year_start)
                            VALUES 
                            (@studentId, @courseId, @year, N'مسجل', @groupId, @academicYearStart)", con);

                                insertCmd.Parameters.AddWithValue("@studentId", studentId);
                                insertCmd.Parameters.AddWithValue("@courseId", courseId);
                                insertCmd.Parameters.AddWithValue("@year", year);
                                insertCmd.Parameters.AddWithValue("@groupId", groupId);
                                insertCmd.Parameters.AddWithValue("@academicYearStart", academicYearStart);

                                int affected = insertCmd.ExecuteNonQuery();
                                if (affected > 0)
                                {
                                    result.RegisteredCourses++;
                                    added = true;
                                }
                                break;
                            }
                        }

                        if (!added)
                            result.FullCourses.Add(courseName);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            return result;
        }

        public void updateStudentsStatus(int updateNumber)
        {
            // التحقق من بداية السنة الدراسية الجديدة
            try
            {
                conn.DatabaseConnection dbCheck = new conn.DatabaseConnection();
                using (SqlConnection con = dbCheck.OpenConnection())
                {
                    int monthStart;
                    using (SqlCommand cmdMonth = new SqlCommand("SELECT month_number FROM Months WHERE month_id = 1", con))
                    {
                        monthStart = Convert.ToInt32(cmdMonth.ExecuteScalar());
                    }

                    if (DateTime.Now.Month < monthStart)
                    {
                        MessageBox.Show("لا يمكن الترقية قبل بداية السنة الدراسية الجديدة.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ: " + ex.Message);
                return;
            }

            // تأكيد الترقية
            string input = Interaction.InputBox("تأكيد الترقية", "ادخل الرمز للتأكيد", "الرمز هنا");
            if (input != "2025")
            {
                MessageBox.Show("رمز خاطئ، يرجى إعادة المحاولة.");
                return;
            }

            // اظهار شريط التقدم
            label1.Visible = true;
            progressBar1.Visible = true;
            progressBar1.Style = ProgressBarStyle.Marquee;
            button6.Enabled = false;
            Application.DoEvents();

            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    int monthStart;
                    using (SqlCommand cmdMonth = new SqlCommand("SELECT month_number FROM Months WHERE month_id = 1", con))
                    {
                        monthStart = Convert.ToInt32(cmdMonth.ExecuteScalar());
                    }

                    int academicYearStart = DateTime.Now.Month >= monthStart ? DateTime.Now.Year : DateTime.Now.Year - 1;
                    if(string.IsNullOrEmpty(textBox2.Text) && updateNumber == 3)
                    {
                        MessageBox.Show("يجب ادخال الرقم الجامعي اولا");
                        return;
                    }
                    int univNumb = Convert.ToInt32(textBox2.Text.Trim());
                    string queryUpdate = "";
                    if (updateNumber == 1)
                    {
                        queryUpdate = @"
                SELECT student_id, current_year, department_id, exam_round
                FROM Students
                WHERE status_id = '1'";
                    }
                    else if (updateNumber == 2)
                    {
                        queryUpdate = @"
                SELECT student_id, current_year, department_id, exam_round
                FROM Students
                WHERE status_id = '1'
                  AND current_year = 1";
                    }else
                    {
                        queryUpdate = @"
    SELECT student_id, current_year, department_id, exam_round
    FROM Students
    WHERE university_number = @univNumb 
      AND status_id = '1'";
                        using (SqlCommand checkistherestu = new SqlCommand("select count(*) from students where university_number = @universityNumb", con))
                        {
                            checkistherestu.Parameters.AddWithValue("@universityNumb", univNumb);
                            int effec = (int)checkistherestu.ExecuteScalar(); // هنا مضمون يرجع 0 أو أكثر
                            if (effec == 0)
                            {
                                MessageBox.Show("لا يوجد طالب بهذا الرقم الجامعي!");
                                progressBar1.Visible = false;
                                button6.Enabled = true;
                                label1.Visible = false;
                                return;
                            }
                        }


                    }

                    // جلب جميع الطلاب المستمرين حسب الحالة
                    SqlCommand cmdStudents = new SqlCommand(queryUpdate, con);
                    if (updateNumber == 3)
                        cmdStudents.Parameters.AddWithValue("@univNumb", univNumb);
                    DataTable dtStudents = new DataTable();
                    new SqlDataAdapter(cmdStudents).Fill(dtStudents);

                    int promoted = 0, failedDownload = 0, partialDownload = 0, fullSuccess = 0;

                    foreach (DataRow student in dtStudents.Rows)
                    {
                        int studentId = Convert.ToInt32(student["student_id"]);
                        int currentYear = Convert.ToInt32(student["current_year"]);
                        int deptId = Convert.ToInt32(student["department_id"]);
                        string examRound = student["exam_round"].ToString();

                        int newYear = currentYear;
                        bool shouldDownloadNewYear = false;

                        // الحالة 1: دور أول ناجح أو دور أول السنة الرابعة
                        if (examRound == "دور أول")
                        {
                            if (currentYear < 4)
                            {
                                newYear = currentYear + 1;
                                shouldDownloadNewYear = true;

                                // المواد الناجحة يتم وضع course_classroom_id = NULL
                                SqlCommand cmdPassCourses = new SqlCommand(@"
            UPDATE Registrations
            SET course_classroom_id = NULL
            WHERE student_id = @studentId
              AND academic_year_start = @lastAcademicYear
              AND course_id IN (
                  SELECT course_id FROM Grades
                  WHERE student_id = @studentId AND success_status = N'ناجح'
              )", con);

                                cmdPassCourses.Parameters.AddWithValue("@studentId", studentId);
                                cmdPassCourses.Parameters.AddWithValue("@lastAcademicYear", academicYearStart - 1);
                                cmdPassCourses.ExecuteNonQuery();
                            }
                            else // السنة الرابعة
                            {
                                SqlCommand cmdGraduate = new SqlCommand(@"
            UPDATE Students 
            SET status_id = '4', exam_round = N'دور أول' 
            WHERE student_id = @studentId;

            UPDATE Registrations
            SET course_classroom_id = NULL
            WHERE student_id = @studentId
              AND academic_year_start = @lastAcademicYear;
        ", con);

                                cmdGraduate.Parameters.AddWithValue("@studentId", studentId);
                                cmdGraduate.Parameters.AddWithValue("@lastAcademicYear", academicYearStart - 1);
                                cmdGraduate.ExecuteNonQuery();

                                continue; // لا حاجة لتنزيل مواد للسنة الخامسة
                            }
                        }
                        // الحالة 2: مرحل أو الحالة 3: إعادة سنة
                        else if (examRound == "مرحل" || examRound == "إعادة سنة")
                        {
                            newYear = currentYear + 1;
                            shouldDownloadNewYear = true;

                            // إعادة المواد الراسبة فقط
                            SqlCommand cmdFailCourses = new SqlCommand(@"
        SELECT r.course_id
        FROM Registrations r
        JOIN Grades g ON r.course_id = g.course_id AND r.student_id = g.student_id
        WHERE r.student_id = @studentId
        AND g.success_status = N'راسب'", con);
                            cmdFailCourses.Parameters.AddWithValue("@studentId", studentId);
                            DataTable dtFail = new DataTable();
                            new SqlDataAdapter(cmdFailCourses).Fill(dtFail);

                            foreach (DataRow fail in dtFail.Rows)
                            {
                                int courseId = Convert.ToInt32(fail["course_id"]);

                                // جلب مجموعة رقم 1 من Course_Classroom
                                SqlCommand cmdGroup1 = new SqlCommand(@"
            SELECT TOP 1 id FROM Course_Classroom 
            WHERE course_id = @courseId
            ORDER BY group_number", con);
                                cmdGroup1.Parameters.AddWithValue("@courseId", courseId);
                                int groupId = Convert.ToInt32(cmdGroup1.ExecuteScalar());

                                SqlCommand cmdReset = new SqlCommand(@"
            UPDATE Registrations 
            SET academic_year_start = @newYear, course_classroom_id = @groupId
            WHERE student_id = @studentId AND course_id = @courseId;

            UPDATE Grades
            SET final_grade = NULL, work_grade = NULL, total_grade = NULL, success_status = NULL
            WHERE student_id = @studentId AND course_id = @courseId;", con);

                                cmdReset.Parameters.AddWithValue("@studentId", studentId);
                                cmdReset.Parameters.AddWithValue("@courseId", courseId);
                                cmdReset.Parameters.AddWithValue("@newYear", academicYearStart);
                                cmdReset.Parameters.AddWithValue("@groupId", groupId);
                                cmdReset.ExecuteNonQuery();
                            }

                            if (examRound == "إعادة سنة")
                            {
                                SqlCommand cmdUpdateRound = new SqlCommand(@"
            UPDATE Students 
            SET exam_round = N'دور أول' 
            WHERE student_id = @studentId", con);
                                cmdUpdateRound.Parameters.AddWithValue("@studentId", studentId);
                                cmdUpdateRound.ExecuteNonQuery();

                                continue; // لا حاجة لتنزيل مواد جديدة للسنة الحالية
                            }
                        }


                        // تحديث السنة الدراسية للطالب
                        SqlCommand cmdUpdateYear = new SqlCommand(@"
                    UPDATE Students SET current_year = @newYear, exam_round = N'دور أول'
                    WHERE student_id = @studentId", con);
                        cmdUpdateYear.Parameters.AddWithValue("@newYear", newYear);
                        cmdUpdateYear.Parameters.AddWithValue("@studentId", studentId);
                        cmdUpdateYear.ExecuteNonQuery();
                        promoted++;

                        // تنزيل المواد للسنة الجديدة إذا لزم الأمر
                        if (shouldDownloadNewYear)
                        {
                            var result = DownloadForOneStudent(studentId, newYear, deptId, academicYearStart);
                            if (result.AllCoursesFull) failedDownload++;
                            else if (result.SomeCoursesFull) partialDownload++;
                            else if (result.HasAnyRegistered) fullSuccess++;
                        }
                    }

                    // عرض النتائج
                    MessageBox.Show($@"نتيجة الترقية:
                                    ✔️ عدد الطلاب الذين تمت ترقيتهم: {promoted}
                                    ✅ نجاح كامل (تم تسجيل جميع المواد): {fullSuccess}
                                    ⚠️ نجاح جزئي (تم تسجيل بعض المواد فقط): {partialDownload}
                                    ❌ فشل في تسجيل أي مادة: {failedDownload}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء الترقية: " + ex.Message);
            }

            progressBar1.Visible = false;
            button6.Enabled = true;
            label1.Visible = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            updateStudentsStatus(1);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            updateStudentsStatus(2);
        }
        private void button7_Click(object sender, EventArgs e)
        {
            updateStudentsStatus(3);
        }




        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Backup Files (*.bak)|*.bak";
                    sfd.FileName = "Cohs_DB_Backup_" + DateTime.Now.ToString("yyyy-MM-dd") + ".bak";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        string backupFile = sfd.FileName;

                        string sqlBackup = $"BACKUP DATABASE Cohs_DB TO DISK = '{backupFile}' WITH INIT, NAME = 'Cohs_DB Backup'";

                        using (SqlConnection con = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=master;Integrated Security=True"))
                        {
                            con.Open();
                            using (SqlCommand cmd = new SqlCommand(sqlBackup, con))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }

                        label1.Text = "تم حفظ النسخة الاحتياطية بنجاح";
                        label1.ForeColor = Color.Green;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                label1.Text = "لا يمكن الكتابة في هذا المجلد، اختر مجلداً آخر";
                label1.ForeColor = Color.Red;
            }
            catch (Exception )
            {
                label1.Text = "خطأ في النسخ الاحتياطي: ";
                label1.ForeColor = Color.Red;
            }
            finally
            {
                label1.Visible = true;
            }

        }

        

        private void button4_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("الرجاء اختيار شهر أولاً.");
                return;
            }

            try
            {
                int newMonth = Convert.ToInt32(comboBox1.SelectedItem);

                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    SqlCommand cmd = new SqlCommand("UPDATE Months SET month_number = @month", con);
                    cmd.Parameters.AddWithValue("@month", newMonth);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("تم تحديث الشهر بنجاح ✅");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء التحديث: " + ex.Message);
            }
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        
    }
}