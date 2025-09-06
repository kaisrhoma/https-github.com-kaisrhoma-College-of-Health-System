using college_of_health_sciences.system_forms;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Spreadsheet;
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
                    numericUpDown5.Value = academicYearStart;
                    numericUpDown6.Value = academicYearStart;
                    // ضبط القيمة الافتراضية للـ numericUpDown
                    numericUpDown1.Value = academicYearStart - 1;
                    numericUpDown2.Value = academicYearStart;
                    numericUpDown3.Value = academicYearStart;
                    numericUpDown4.Value = academicYearStart - 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

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

                    comboBox3.DataSource = new BindingSource(dt, null);
                    comboBox3.DisplayMember = "dep_name";
                    comboBox3.ValueMember = "department_id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There is an Error : " + ex.Message);
            }

            try
            {
                //نملأ الكومبوبوكس بالقيم من 1 إلى12
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
                MessageBox.Show("يرجى البحث عن الطالب أولا");
                return;
            }

            int studentId = Convert.ToInt32(dataGridView1.Rows[0].Cells["student_id"].Value.ToString());
            int newDeptId = Convert.ToInt32(comboBox2.SelectedValue);
            int newYear = 2; // إعادة الطالب للسنة الثانية
            int academicYear = (int)numericUpDown5.Value; // العام الجامعي
            int generalDeptId;

            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    generalDeptId = GetGeneralDepartmentId(con);
                    if (newDeptId == generalDeptId)
                    {
                        MessageBox.Show("⚠ لا يمكن تحويل الطالب إلى القسم العام مباشرة.");
                        return;
                    }
                    string oldst = dataGridView1.Rows[0].Cells["الحالة"].Value.ToString();
                    if(oldst != "مستمر")
                    {
                        MessageBox.Show("⚠ لايمكن تحويل طالب غير مستمر من قسم لقسم");
                        return;
                    }
                    string depold = dataGridView1.Rows[0].Cells["القسم"].Value.ToString();
                    int depoldid = 1;
                    using (SqlCommand cmd = new SqlCommand("SELECT department_id FROM Departments WHERE dep_name = @od ", con))
                    {
                        cmd.Parameters.AddWithValue("@od", depold);
                        depoldid = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                        if (depoldid == newDeptId)
                        {
                            MessageBox.Show("⚠ لايمكن تحويل الطالب لنفس القسم الموجود به حاليا");
                            return;
                        }

                    // التحقق من وجود تسجيلات
                    if (!isStudentHaveRegistrations(studentId))
                    {
                        MessageBox.Show("لايملك الطالب تسجيلات يرجى تحويل الطالب الى حالة محول \nثم فتحه في نافذة المعادة للمزيد من التفاصيل");
                        return;
                    }

                    // إذا يوجد تسجيلات للطالب
                    DialogResult answer = MessageBox.Show(
                        "هل انت متأكد من تبديل القسم للطالب؟\nسيتم التعامل مع المواد كالآتي:\n" +
                        "- المواد المشتركة (الناجحة) ستبقى.\n" +
                        "- المواد غير المشتركة مع النجاح → تتحول إلى 'سابق' إذا ليست سنة أولى أو القسم عام.\n" +
                        "- المواد غير المشتركة (راسب/NULL) → سيتم حذفها إذا ليست سنة أولى أو القسم عام.\n\n" +
                        "للتأكيد اضغط: نعم\nللتراجع اضغط: لا",
                        "تأكيد التحويل",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation,
                        MessageBoxDefaultButton.Button2,
                        MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                    );

                    if (answer != DialogResult.Yes) return;

                    DialogResult dr = MessageBox.Show(
            "AcademicYearStart = " + academicYear + "\n\nهل تريد الاستمرار في الترقية؟",
            "تأكيد الترقية",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

                    if (dr == DialogResult.No)
                    {
                        return; // يوقف العملية إذا اخترت لا
                    }

                    // 1️⃣ تحويل المواد الناجحة وغير المشتركة إلى 'سابق'
                    string updateStatusQuery = @"
-- 1. المواد التي نجح فيها الطالب من السنة الثانية فما فوق
SELECT g.student_id, g.course_id
INTO #PassedSubjects   -- # تعني جدول مؤقت
FROM Grades g
JOIN Courses c ON g.course_id = c.course_id
WHERE g.success_status = N'نجاح'
  AND c.year_number >= 2
  AND g.student_id = @StudentID;

-- 2. مواد القسم الجديد من السنة الثانية فما فوق
SELECT cd.course_id
INTO #NewDeptSubjects   -- برضه مؤقت
FROM Course_Department cd
JOIN Courses c ON cd.course_id = c.course_id
WHERE cd.department_id = @NewDeptID
  AND c.year_number >= 2;


---- 3. تحديث المواد المشتركة بين القسمين إلى مسجل
UPDATE r
SET r.status = N'مسجل'
FROM Registrations r
JOIN #PassedSubjects p ON r.student_id = p.student_id AND r.course_id = p.course_id
JOIN #NewDeptSubjects n ON p.course_id = n.course_id
WHERE r.student_id = @StudentID;
";

                    using (SqlCommand cmdStatus = new SqlCommand(updateStatusQuery, con))
                    {
                        cmdStatus.Parameters.AddWithValue("@StudentID", studentId);
                        cmdStatus.Parameters.AddWithValue("@NewDeptID", newDeptId);
                        cmdStatus.ExecuteNonQuery();
                    }


                    string qPrev = @"
-- 1. المواد التي نجح فيها الطالب من السنة الثانية فما فوق
SELECT g.student_id, g.course_id
INTO #PassedSubjects   -- # تعني جدول مؤقت
FROM Grades g
JOIN Courses c ON g.course_id = c.course_id
WHERE g.success_status = N'نجاح'
  AND c.year_number >= 2
  AND g.student_id = @StudentID;

-- 2. مواد القسم الجديد من السنة الثانية فما فوق
SELECT cd.course_id
INTO #NewDeptSubjects   -- برضه مؤقت
FROM Course_Department cd
JOIN Courses c ON cd.course_id = c.course_id
WHERE cd.department_id = @NewDeptID
  AND c.year_number >= 2;

-- 4. تحديث المواد الناجح فيها الطالب بس مش موجودة في القسم الجديد إلى سابق
UPDATE r
SET r.status = N'سابق'
FROM Registrations r
JOIN #PassedSubjects p ON r.student_id = p.student_id AND r.course_id = p.course_id
WHERE r.student_id = @StudentID
  AND NOT EXISTS (
      SELECT 1 FROM #NewDeptSubjects n WHERE n.course_id = r.course_id
  );
";
                    using (SqlCommand cmdStatus = new SqlCommand(qPrev, con))
                    {
                        cmdStatus.Parameters.AddWithValue("@StudentID", studentId);
                        cmdStatus.Parameters.AddWithValue("@NewDeptID", newDeptId);
                        cmdStatus.ExecuteNonQuery();
                    }


                    string qRegDelete = @"
-- 1. المواد التي نجح فيها الطالب من السنة الثانية فما فوق
SELECT g.student_id, g.course_id
INTO #PassedSubjects   -- # تعني جدول مؤقت
FROM Grades g
JOIN Courses c ON g.course_id = c.course_id
WHERE g.success_status = N'نجاح'
  AND c.year_number >= 2
  AND g.student_id = @StudentID;


-- 5. حذف أي مادة راسب أو NULL (سواء في القسم القديم أو الجديد)
DELETE r
FROM Registrations r
LEFT JOIN Grades g 
    ON g.course_id = r.course_id 
   AND g.student_id = r.student_id
WHERE r.student_id = @StudentID
  AND NOT EXISTS (
      SELECT 1 
      FROM #PassedSubjects p 
      WHERE p.course_id = r.course_id
  )
  AND (g.success_status = N'راسب' OR g.success_status IS NULL)
  AND r.year_number >= 2 ;
";
                    using (SqlCommand cmdStatus = new SqlCommand(qRegDelete, con))
                    {
                        cmdStatus.Parameters.AddWithValue("@StudentID", studentId);
                        cmdStatus.ExecuteNonQuery();
                    }


                    string qGradDelete = @"
-- 1. المواد التي نجح فيها الطالب من السنة الثانية فما فوق
SELECT g.student_id, g.course_id
INTO #PassedSubjects   -- # تعني جدول مؤقت
FROM Grades g
JOIN Courses c ON g.course_id = c.course_id
WHERE g.success_status = N'نجاح'
  AND c.year_number >= 2
  AND g.student_id = @StudentID;

-- حذف الدرجات من جدول الدرجات اللتي تكون اما null او راسب

DELETE g
FROM Grades g
WHERE g.student_id = @StudentID
  AND NOT EXISTS (
      SELECT 1 
      FROM #PassedSubjects p
      WHERE p.course_id = g.course_id
  )
  AND (g.success_status = N'راسب' OR g.success_status IS NULL);
";
                    using (SqlCommand cmdStatus = new SqlCommand(qGradDelete, con))
                    {
                        cmdStatus.Parameters.AddWithValue("@StudentID", studentId);
                        cmdStatus.ExecuteNonQuery();
                    }
                    // 3️⃣ تحديث القسم والسنة للطالب
                    string updateStudentQuery = @"
                UPDATE Students
                SET department_id = @newDeptId,
                    current_year = @newYear
                WHERE student_id = @studentId";

                    using (SqlCommand cmdUpdate = new SqlCommand(updateStudentQuery, con))
                    {
                        cmdUpdate.Parameters.AddWithValue("@studentId", studentId);
                        cmdUpdate.Parameters.AddWithValue("@newDeptId", newDeptId);
                        cmdUpdate.Parameters.AddWithValue("@newYear", newYear);
                        cmdUpdate.ExecuteNonQuery();
                    }

                    // 4️⃣ تنزيل مواد السنة الجديدة
                    DownloadCoursesForStudent(con, studentId, newYear, newDeptId, academicYear);

                    MessageBox.Show("✅ تم تحويل الطالب بنجاح.");
                    button5_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ: " + ex.Message);
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

        public void updateStudentsStatus()
        {
            //        // التحقق من بداية السنة الدراسية الجديدة
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

            try
            {
                int academicYear = (int)numericUpDown6.Value;

                using (SqlConnection con = new conn.DatabaseConnection().OpenConnection())
                {
                    int univNumb = Convert.ToInt32(textBox2.Text.Trim());
                    GENERAL_DEPARTMENT_ID = GetGeneralDepartmentId(con);
                    // جلب طلاب السنة الأولى الذين exam_round ليست دور أول/دور ثاني
                    string q = @"
            SELECT student_id, current_year, department_id, exam_round ,status_id,full_name
            FROM Students
            WHERE university_number = @univNumb 
            ";
                    DataTable dtStudent = new DataTable();
                    int stId = GetStatusId(con, "مستمر");
                    using (SqlCommand cmd = new SqlCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@univNumb", univNumb);
                        new SqlDataAdapter(cmd).Fill(dtStudent);
                    }

                    // التحقق من أي طالب في القسم العام
                    int deptId = 1;
                    int studentId = 0;
                    string examRound = "";
                    int currentY = 0;
                    if (dtStudent.Rows.Count > 0) // تحقق من وجود صفوف
                    {
                        DataRow row = dtStudent.Rows[0]; // الصف الأول (أو الصف الذي تريده)

                        if (stId != Convert.ToInt32(row["status_id"]))
                        {
                            MessageBox.Show("لايمكن ترقية طالب غير مستمر!");
                            return;
                        }

                        deptId = Convert.ToInt32(row["department_id"]);
                        if (deptId == GENERAL_DEPARTMENT_ID)
                        {
                            MessageBox.Show("لايمكن ترقية طالب سنة اولى من هنا يمكنك استخدام واجهة المعادلة وتحديد القسم والسنة");
                            return;
                        }

                        studentId = Convert.ToInt32(row["student_id"]);
                        examRound = row["exam_round"].ToString();
                        currentY = Convert.ToInt32(row["current_year"]);
                        string name = row["full_name"].ToString();
                        int academicYearStart = (int)numericUpDown6.Value;
                        DialogResult dr = MessageBox.Show(
                        "AcademicYearStart = " + academicYearStart + "\n\nهل تريد الاستمرار في ترقية " + name + "؟",
                        "تأكيد الترقية",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                        if (dr == DialogResult.No)
                        {
                            return; // يوقف العملية إذا اخترت لا
                        }
                    }
                    else
                    {
                        MessageBox.Show("يبدو ان الطالب غير موجود !");
                        return;
                    }

                    // البدء بالترقية حسب الحالة

                    if (currentY == 1)
                    {
                        MessageBox.Show("لا يمكن ترقية طالب سنة اولى كطالب فردي");
                        return;
                    }

                    switch (examRound)
                    {
                        case "مكتمل":
                            PromoteCompleteStudent(con, studentId, deptId, academicYear);
                            break;

                        case "مرحل":
                            PromoteRepeaterStudent(con, studentId, deptId, academicYear);
                            break;

                        case "إعادة سنة":
                            RepeatStudentTowThreeFour(con, studentId, academicYear, deptId);
                            break;
                        default : MessageBox.Show("لم يتم احتساب درجات الطالب بعد!");
                            break;
                    }


                    // إعادة exam_round إلى دور أول
                    using (SqlCommand cmdRound = new SqlCommand(
                        "UPDATE Students SET exam_round = N'دور أول' WHERE student_id = @studentId ", con))
                    {
                        cmdRound.Parameters.AddWithValue("@studentId", studentId);
                        cmdRound.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("تمت ترقية الطالب بنجاح.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء الترقية: " + ex.Message);
            }

            textBox2.Text = "";
        }


        private void button7_Click(object sender, EventArgs e)
        {
            updateStudentsStatus();
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
                        label1.ForeColor = System.Drawing.Color.Green;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                label1.Text = "لا يمكن الكتابة في هذا المجلد، اختر مجلداً آخر";
                label1.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception )
            {
                label1.Text = "خطأ في النسخ الاحتياطي: ";
                label1.ForeColor = System.Drawing.Color.Red;
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

        public void updateStudentsdep()
        {
            if (comboBox3.SelectedValue == null)
            {
                MessageBox.Show("يرجى تحديد القسم الجديد.");
                return;
            }

            // الطريقة الأسهل
            if (comboBox3.Text == "عام")
            {
                MessageBox.Show("لا يمكن التحديث للقسم العام.");
                return;
            }

            // أو باستخدام GetItemText
            //if (comboBox3.GetItemText(comboBox3.SelectedItem) == "عام")
            //{
            //    MessageBox.Show("لا يمكن التحديث للقسم العام.");
            //    return;
            //}


            int newDepartmentId = Convert.ToInt32(comboBox3.SelectedValue);
            List<int> selectedStudentIds = new List<int>();

            if (!dataGridView2.Columns.Contains("Select"))
            {
                MessageBox.Show("عمود التحديد غير موجود في الجدول!");
                return;
            }

            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (row.IsNewRow) continue;

                // التأكد من وجود قيمة في عمود Select وتحويلها بأمان
                bool isSelected = false;
                object selectValue = row.Cells["Select"].Value;
                if (selectValue != null && selectValue != DBNull.Value)
                {
                    bool.TryParse(selectValue.ToString(), out isSelected);
                }

                // التأكد من وجود student_id وعدم احتوائها DBNull قبل الإضافة
                object studentIdObj = row.Cells["student_id"].Value;
                if (isSelected && studentIdObj != null && studentIdObj != DBNull.Value)
                {
                    selectedStudentIds.Add(Convert.ToInt32(studentIdObj));
                }
            }

            if (selectedStudentIds.Count == 0)
            {
                MessageBox.Show("يرجى اختيار طالب واحد على الأقل.");
                return;
            }

            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    foreach (int studentId in selectedStudentIds)
                    {
                        using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE Students
                    SET department_id = @newDept
                    WHERE student_id = @studentId", con))
                        {
                            cmd.Parameters.AddWithValue("@newDept", newDepartmentId);
                            cmd.Parameters.AddWithValue("@studentId", studentId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("تم تحديث القسم للطلاب المحددين بنجاح.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ: " + ex.Message);
            }
        }



        public void getStudentsYearOne()
        {
            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    // SQL لجلب طلاب السنة الأولى، القسم العام، والحالة مستمر
                    string query = @"
                                  SELECT s.student_id, 
                                         s.university_number AS [الرقم الجامعي], 
                                         s.full_name AS [الاسم], 
                                         s.current_year AS [السنة الحالية], 
                                         st.description AS [الحالة],
                                         d.dep_name AS [القسم]
                                  FROM Students s
                                  JOIN Departments d ON s.department_id = d.department_id
                                  JOIN Status st ON s.status_id = st.status_id
                                  WHERE s.current_year = 1
                                    AND st.description = N'مستمر'
                                    AND s.exam_round NOT IN (N'دور أول', N'دور ثاني')";

                    // مستمر

                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        dataGridView2.DataSource = null;
                        MessageBox.Show("لا يوجد طلاب سنة أولى مستمرين في القسم العام.");
                    }
                    else
                    {
                        dataGridView2.DataSource = dt;
                        datagridviewstyle(dataGridView2);
                        dataGridView2.Columns["student_id"].Visible = false;
                        // افترض أن dt تم ربطه بـ DataGridView بالفعل
                        if (!dataGridView2.Columns.Contains("Select"))
                        {
                            DataGridViewCheckBoxColumn chk = new DataGridViewCheckBoxColumn();
                            chk.Name = "Select";
                            chk.HeaderText = "اختر";
                            chk.Width = 50;
                            chk.ReadOnly = false;
                            dataGridView2.Columns.Insert(0, chk); // لإظهار العمود أولًا
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ: " + ex.Message);
            }
        }
        private void button8_Click(object sender, EventArgs e)
        {
            getStudentsYearOne();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            updateStudentsdep();
            getStudentsYearOne();
        }


        private int GENERAL_DEPARTMENT_ID ; // رقم القسم العام

        private void PromoteFirstYearStudents()
        {
            int academicYear = (int)numericUpDown2.Value;

            using (SqlConnection con = new conn.DatabaseConnection().OpenConnection())
            {
                GENERAL_DEPARTMENT_ID = GetGeneralDepartmentId(con);
                // جلب طلاب السنة الأولى الذين exam_round ليست دور أول/دور ثاني
                string q = @"
            SELECT student_id, department_id, exam_round
            FROM Students
            WHERE current_year = 1 AND exam_round NOT IN (N'دور أول', N'دور ثاني')";

                DataTable dtStudents = new DataTable();
                new SqlDataAdapter(q, con).Fill(dtStudents);

                // التحقق من أي طالب في القسم العام
                foreach (DataRow student in dtStudents.Rows)
                {
                    int deptId = Convert.ToInt32(student["department_id"]);
                    if (deptId == GENERAL_DEPARTMENT_ID)
                    {
                        MessageBox.Show("يوجد طالب في القسم العام، يرجى تغييره قبل الترقية.");
                        return;
                    }
                }

                // البدء بالترقية حسب الحالة
                foreach (DataRow student in dtStudents.Rows)
                {
                    int studentId = Convert.ToInt32(student["student_id"]);
                    string examRound = student["exam_round"].ToString();
                    int deptId = Convert.ToInt32(student["department_id"]);

                    switch (examRound)
                    {
                        case "مكتمل":
                            PromoteCompleteStudent(con, studentId, deptId, academicYear);
                            break;

                        case "مرحل":
                            PromoteRepeaterStudent(con, studentId, deptId, academicYear);
                            break;

                        case "إعادة سنة":
                            RepeatStudent(con, studentId, academicYear,deptId);
                            break;
                    }

                    // إعادة exam_round إلى دور أول
                    using (SqlCommand cmdRound = new SqlCommand(
                        "UPDATE Students SET exam_round = N'دور أول' WHERE student_id = @studentId", con))
                    {
                        cmdRound.Parameters.AddWithValue("@studentId", studentId);
                        cmdRound.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("تمت ترقية طلاب السنة الأولى بنجاح.");
                dataGridView2.DataSource = null;
            }
        }
        private void ClearPassedCoursesClassrooms(SqlConnection con, int studentId)
        {
            // أولاً: نجيب آخر عام جامعي للطالب (من جدول Registrations أو الدرجات)
            int lastYear;
            using (SqlCommand cmd = new SqlCommand(@"
        SELECT MAX(academic_year_start)
        FROM Registrations
        WHERE student_id = @studentId", con))
            {
                cmd.Parameters.AddWithValue("@studentId", studentId);
                object result = cmd.ExecuteScalar();
                if (result == DBNull.Value) return; // ما عنداش تنزيلات سابقة
                lastYear = Convert.ToInt32(result);
            }

            // ثانياً: نخلي course_classroom_id = NULL للمواد اللي نجح فيها
            using (SqlCommand cmd = new SqlCommand(@"
        UPDATE r
        SET r.course_classroom_id = NULL
        FROM Registrations r
        INNER JOIN Grades g ON g.student_id = r.student_id AND g.course_id = r.course_id
        WHERE r.student_id = @studentId
          AND r.academic_year_start = @lastYear
          AND g.success_status = N'نجاح'", con)) // استبدل 'نجاح' بالقيمة اللي تستعملها عندك
            {
                cmd.Parameters.AddWithValue("@studentId", studentId);
                cmd.Parameters.AddWithValue("@lastYear",lastYear);
                cmd.ExecuteNonQuery();
            }
        }

        private int GetStatusId(SqlConnection con, string statusDescription)
        {
            SqlCommand cmd = new SqlCommand("SELECT status_id FROM Status WHERE description = @desc", con);
            cmd.Parameters.AddWithValue("@desc", statusDescription);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // ---------------------- الدوال المساعدة ----------------------

        // 1️⃣ مكتمل: ترقية السنة + تنزيل مواد السنة الثانية
        private void PromoteCompleteStudent(SqlConnection con, int studentId, int deptId, int academicYear)
        {
            ClearPassedCoursesClassrooms(con, studentId);
            //تحقق من سنة رابعة يعيد السنة
            int newYear;
            using (SqlCommand cmd = new SqlCommand(@"
            SELECT current_year
            FROM Students
            WHERE student_id = @studentid", con))
            {
                cmd.Parameters.AddWithValue("@studentid", studentId);
                newYear = Convert.ToInt32(cmd.ExecuteScalar());
            }
            if (newYear == 4)
            {
                using (SqlCommand cmd = new SqlCommand("UPDATE Students SET status_id = @statusid  WHERE student_id = @studentId", con))
                {
                    int statusidst = GetStatusId(con,"خريج");
                    cmd.Parameters.AddWithValue("@statusid",statusidst);
                    cmd.Parameters.AddWithValue("@studentid", studentId);
                    cmd.ExecuteNonQuery();
                }
                return;
            }
            // ترقية الطالب لسنة أعلى
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE Students SET current_year = current_year + 1 OUTPUT INSERTED.current_year WHERE student_id = @studentId", con))
            {
                cmd.Parameters.AddWithValue("@studentId", studentId);
                newYear = (int)cmd.ExecuteScalar(); // هنا تجيب القيمة الجديدة مباشرة
            }
            // تنزيل المواد للسنة الجديدة (ديناميكية)
            DownloadCoursesForStudent(con, studentId, newYear, deptId, academicYear);
        }


        // 2️⃣ مرحل: تحديث المواد الراسبة، تصفير الدرجات، ترقية للسنة الثانية + تنزيل مواد جديدة
        private void PromoteRepeaterStudent(SqlConnection con, int studentId, int deptId, int academicYear)
        {
            // جلب المواد الراسبة في السنة الأولى
            string failQuery = @"
        SELECT r.course_id
        FROM Registrations r
        JOIN Grades g ON r.course_id = g.course_id AND r.student_id = g.student_id
        WHERE r.student_id = @studentId AND g.success_status = N'راسب'
";
            DataTable dtFail = new DataTable();
            using (SqlCommand cmd = new SqlCommand(failQuery, con))
            {
                cmd.Parameters.AddWithValue("@studentId", studentId);
                new SqlDataAdapter(cmd).Fill(dtFail);
            }
            ClearPassedCoursesClassrooms(con,studentId);
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

            //تحقق من سنة رابعة يعيد السنة 
            int newYear;
            using (SqlCommand cmd = new SqlCommand(@"
            SELECT current_year
            FROM Students
            WHERE student_id = @studentid", con))
            {
                cmd.Parameters.AddWithValue("@studentid", studentId);
                newYear = Convert.ToInt32(cmd.ExecuteScalar());
            }
            if (newYear == 4)
            {
                return;
            }

            // ترقية الطالب لسنة أعلى
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE Students SET current_year = current_year + 1 OUTPUT INSERTED.current_year WHERE student_id = @studentId", con))
            {
                cmd.Parameters.AddWithValue("@studentId", studentId);
                newYear = (int)cmd.ExecuteScalar(); // هنا تجيب القيمة الجديدة مباشرة
            }
            // تنزيل المواد للسنة الجديدة (ديناميكية)
            DownloadCoursesForStudent(con, studentId, newYear, deptId, academicYear);
        }



        // 3️⃣ إعادة سنة: تحديث المواد الراسبة فقط، لا ترقية، تغيير القسم إلى العام
        private void RepeatStudent(SqlConnection con, int studentId, int academicYear, int depId)
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
            ClearPassedCoursesClassrooms(con, studentId);
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

            // تغيير قسم الطالب إلى العام
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE Students SET department_id = @generalDept WHERE student_id = @studentId", con))
            {
                cmd.Parameters.AddWithValue("@generalDept", GENERAL_DEPARTMENT_ID);
                cmd.Parameters.AddWithValue("@studentId", studentId);
                cmd.ExecuteNonQuery();
            }
        }


        private void RepeatStudentTowThreeFour(SqlConnection con, int studentId, int academicYear,int depId)
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
            ClearPassedCoursesClassrooms(con, studentId);
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
        }

        // ---------------------- دوال مساعدة ----------------------
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

        // تنزيل مواد الطالب لسنة معينة
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
                int groupId = GetOrCreateGroup(con, courseId,academicYear,deptId);

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

        //تحقق من الطلبة اللذين لم يتم إدخال درجاتهم
        private int GetGeneralDepartmentId(SqlConnection con)
        {
            SqlCommand cmd = new SqlCommand("SELECT department_id FROM Departments WHERE dep_name = N'عام'", con);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        //تحقق من الطلبة اللذين لم يتم إدخال درجاتهم
        private bool HasUnfinishedStudents(SqlConnection con, int year)
        {
            using (SqlCommand cmd = new SqlCommand(@"
        SELECT COUNT(*) 
        FROM Students 
        WHERE current_year = @year
          AND (exam_round = N'دور أول' OR exam_round = N'دور ثاني')", con))
            {
                cmd.Parameters.AddWithValue("@year", year);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private bool HasPromotableStudents(SqlConnection con, int year)
        {
            using (SqlCommand cmd = new SqlCommand(@"
        SELECT COUNT(*) 
        FROM Students 
        WHERE current_year = @year
          AND (exam_round = N'مكتمل' OR exam_round = N'مرحل' OR exam_round = N'إعادة سنة')", con))
            {
                cmd.Parameters.AddWithValue("@year", year);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (!CheckConstrains())
                return;
            int academicYearStart = (int)numericUpDown2.Value;
            DialogResult dr = MessageBox.Show(
            "AcademicYearStart = " + academicYearStart + "\n\nهل تريد الاستمرار في الترقية؟",
            "تأكيد الترقية",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

            if (dr == DialogResult.No)
            {
                return ; // يوقف العملية إذا اخترت لا
            }
            try
            {
                int year = 1;
                conn.DatabaseConnection dbchekfirst = new conn.DatabaseConnection();
                using (SqlConnection con = dbchekfirst.OpenConnection()) // فتح الاتصال
                {
                    // تحقق من الطلبة الذين لم تُدخل درجاتهم بعد
                    if (HasUnfinishedStudents(con, year))
                    {
                        MessageBox.Show($"⚠ هناك طلبة لم يتم إدخال درجاتهم بعد في السنة {year}، لا يمكن الترقية.");
                        return;
                    }


                    // تحقق من وجود طلبة قابلين للترقية
                    if (!HasPromotableStudents(con, year))
                    {
                        MessageBox.Show($"⚠ ليس هناك طلاب في هذه السنة {year} للترقية.");
                        return;
                    }
                }

                // استدعاء دالة الترقية الخاصة بالطلاب
                PromoteFirstYearStudents();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error : " + ex.Message);
            }
        }

        private void PromoteFirstSecondThiredYearStudents()
        {
            int academicYear = (int)numericUpDown3.Value;

            using (SqlConnection con = new conn.DatabaseConnection().OpenConnection())
            {
                GENERAL_DEPARTMENT_ID = GetGeneralDepartmentId(con);
                // جلب طلاب السنة الأولى الذين exam_round ليست دور أول/دور ثاني
                string q = @"
            SELECT student_id, department_id, exam_round
            FROM Students
            WHERE current_year NOT IN (1) AND exam_round NOT IN (N'دور أول', N'دور ثاني')";

                DataTable dtStudents = new DataTable();
                new SqlDataAdapter(q, con).Fill(dtStudents);

                // التحقق من أي طالب في القسم العام
                foreach (DataRow student in dtStudents.Rows)
                {
                    int deptId = Convert.ToInt32(student["department_id"]);
                    if (deptId == GENERAL_DEPARTMENT_ID)
                    {
                        MessageBox.Show("يوجد طالب في القسم العام، يرجى تغييره قبل الترقية.");
                        return;
                    }
                }

                // البدء بالترقية حسب الحالة
                foreach (DataRow student in dtStudents.Rows)
                {
                    int studentId = Convert.ToInt32(student["student_id"]);
                    string examRound = student["exam_round"].ToString();
                    int deptId = Convert.ToInt32(student["department_id"]);

                    switch (examRound)
                    {
                        case "مكتمل":
                            PromoteCompleteStudent(con, studentId, deptId, academicYear);
                            break;

                        case "مرحل":
                            PromoteRepeaterStudent(con, studentId, deptId, academicYear);
                            break;

                        case "إعادة سنة":
                            RepeatStudentTowThreeFour(con, studentId, academicYear,deptId);
                            break;
                    }

                    // إعادة exam_round إلى دور أول
                    using (SqlCommand cmdRound = new SqlCommand(
                        "UPDATE Students SET exam_round = N'دور أول' WHERE student_id = @studentId", con))
                    {
                        cmdRound.Parameters.AddWithValue("@studentId", studentId);
                        cmdRound.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("تمت ترقية الطلاب بنجاح.");
                dataGridView3.DataSource = null;
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
                        isApproved = Convert.ToBoolean(dt.Rows[0]["is_approved"]);
                        monthStart = Convert.ToInt32(dt.Rows[0]["month_number"]);
                    }

                    // التحقق من الاعتماد
                    if (!isApproved)
                    {
                        MessageBox.Show("لم يتم اعتماد الدرجات بعد.");
                        return false;
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
                return ; // يوقف العملية إذا اخترت لا
            }


            try
            {
                conn.DatabaseConnection dbchekfirst = new conn.DatabaseConnection();
                using (SqlConnection con = dbchekfirst.OpenConnection()) // فتح الاتصال
                {
                    string q = @"
            SELECT COUNT(*)
            FROM Students
            WHERE current_year NOT IN (1) AND exam_round NOT IN (N'دور أول', N'دور ثاني')";


                    using (SqlCommand cmd = new SqlCommand(q, con))
                    {
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        if(count > 0)
                        {
                            MessageBox.Show("⚠ ليس هناك طلاب للترقية ربما لم يتم ادخال درجاتهم او ان لم يتم احتسابها");
                            return ;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error : " + ex.Message);
            }

            PromoteFirstSecondThiredYearStudents();
        }
        public void getStudentsYearOneTowThree()
        {
            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    // SQL لجلب طلاب السنة الأولى، القسم العام، والحالة مستمر
                    string query = @"
                                  SELECT s.student_id, 
                                         s.university_number AS [الرقم الجامعي], 
                                         s.full_name AS [الاسم], 
                                         s.current_year AS [السنة الحالية], 
                                         st.description AS [الحالة],
                                         d.dep_name AS [القسم]
                                  FROM Students s
                                  JOIN Departments d ON s.department_id = d.department_id
                                  JOIN Status st ON s.status_id = st.status_id
                                  WHERE s.current_year NOT IN (1)
                                    AND st.description = N'مستمر'
                                    AND s.exam_round NOT IN (N'دور أول', N'دور ثاني')";

                    // مستمر

                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        dataGridView3.DataSource = null;
                        MessageBox.Show("لا يوجد طلاب للترقية .او لم يتم إدخال درجاتهم بعد.");
                    }
                    else
                    {
                        dataGridView3.DataSource = dt;
                        datagridviewstyle(dataGridView3);
                        dataGridView3.Columns["student_id"].Visible = false;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ: " + ex.Message);
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            getStudentsYearOneTowThree();
        }

        private void LoadTransferredStudents()
        {
            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    string q = @"
                SELECT s.student_id, s.full_name,s.current_year, d.dep_name, st.description
                FROM Students s
                JOIN Departments d ON d.department_id = s.department_id
                JOIN Status st ON st.status_id = s.status_id
                WHERE st.description = N'محول'";

                    SqlDataAdapter da = new SqlDataAdapter(q, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dataGridView4.DataSource = dt;
                    
                    // إضافة زر فتح فورم equation
                    if (!dataGridView4.Columns.Contains("OpenEquation"))
                    {
                        DataGridViewButtonColumn btn = new DataGridViewButtonColumn();
                        btn.Name = "OpenEquation";
                        btn.HeaderText = "أنقر";
                        btn.Text = "معادلة";
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.Width = 100;
                        btn.UseColumnTextForButtonValue = true;

                        // ضبط الألوان
                        btn.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(0, 109, 148);   // لون الخلفية
                        btn.DefaultCellStyle.ForeColor = System.Drawing.Color.White;                   // لون النص
                        btn.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(0, 90, 120); // عند التحديد
                        btn.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;

                        dataGridView4.Columns.Insert(0, btn);
                        datagridviewstyle(dataGridView4);
                        dataGridView4.Columns["OpenEquation"].Width = 100;
                        dataGridView4.Columns["student_id"].Visible = false;
                        dataGridView4.Columns["full_name"].HeaderText = "الإسم";
                        dataGridView4.Columns["current_year"].HeaderText = "السنة";
                        dataGridView4.Columns["dep_name"].HeaderText = "القسم";
                        dataGridView4.Columns["description"].HeaderText = "الحالة";
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ: " + ex.Message);
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            LoadTransferredStudents();
        }

        private void dataGridView4_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridView4.Columns[e.ColumnIndex].Name == "OpenEquation")
            {
                DataGridViewRow row = dataGridView4.Rows[e.RowIndex];

                // ✅ تحقق أن الصف ليس فارغ
                if (row.Cells["student_id"].Value != null &&
                    !string.IsNullOrWhiteSpace(row.Cells["student_id"].Value.ToString()))
                {
                    int studentId = Convert.ToInt32(row.Cells["student_id"].Value);
                    string fullName = row.Cells["full_name"].Value?.ToString();

                    equation eqForm = new equation(studentId, fullName);
                    eqForm.ShowDialog();
                }
            }

        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                updateStudentsStatus();
                e.SuppressKeyPress = true;
            }
        }
    }
}