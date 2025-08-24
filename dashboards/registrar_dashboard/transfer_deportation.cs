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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace college_of_health_sciences.dashboards.registrar_dashboard
{
    public partial class transfer_deportation : UserControl
    {
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
            public int StudentId { get; set; }
            public int RegisteredCourses { get; set; }
            public List<string> FullCourses { get; set; } = new List<string>();

            public bool HasAnyRegistered => RegisteredCourses > 0;
            public bool AllCoursesFull => FullCourses.Count > 0 && RegisteredCourses == 0;
            public bool SomeCoursesFull => FullCourses.Count > 0 && RegisteredCourses > 0;
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

        public DownloadResult DownloadForOneStudent(int studentId, int newYear, int departmentId)
        {
            DownloadResult result = new DownloadResult { StudentId = studentId };

            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    SqlCommand checkCmd = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM Students 
                    WHERE student_id = @studentId AND department_id = @departmentId", con);

                    checkCmd.Parameters.AddWithValue("@studentId", studentId);
                    checkCmd.Parameters.AddWithValue("@departmentId", departmentId);

                    int count = (int)checkCmd.ExecuteScalar();
                    if (count == 0)
                        return result;

                    SqlCommand coursesCmd = new SqlCommand(@"
                    SELECT c.course_id, c.course_name
                    FROM Courses c
                    JOIN Course_Department cd ON cd.course_id = c.course_id
                    WHERE c.year_number = @year AND cd.department_id = @dept", con);

                    coursesCmd.Parameters.AddWithValue("@year", newYear);
                    coursesCmd.Parameters.AddWithValue("@dept", departmentId);

                    SqlDataAdapter adapter = new SqlDataAdapter(coursesCmd);
                    DataTable courses = new DataTable();
                    adapter.Fill(courses);

                    foreach (DataRow row in courses.Rows)
                    {
                        int courseId = Convert.ToInt32(row["course_id"]);
                        string courseName = row["course_name"].ToString();

                        SqlCommand getGroupsCmd = new SqlCommand(@"
                     SELECT cc.id, cc.capacity, cc.group_number
                     FROM Course_Classroom cc
                     WHERE cc.course_id = @courseId
                     ORDER BY cc.group_number", con);

                        getGroupsCmd.Parameters.AddWithValue("@courseId", courseId);
                        SqlDataAdapter groupAdapter = new SqlDataAdapter(getGroupsCmd);
                        DataTable groups = new DataTable();
                        groupAdapter.Fill(groups);

                        bool registered = false;

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
                                int month2;
                                using (SqlCommand cmddate = new SqlCommand("SELECT month_number FROM Months WHERE month_id = 1", con))
                                {
                                    month2 = Convert.ToInt32(cmddate.ExecuteScalar());
                                }
                                int academicYearStart = DateTime.Now.Month >= month2 ? DateTime.Now.Year : DateTime.Now.Year - 1;

                                SqlCommand insertCmd = new SqlCommand(@"
                    IF NOT EXISTS (
                        SELECT 1 FROM Registrations 
                        WHERE student_id = @studentId AND course_id = @courseId AND academic_year_start = @academicYearStart
                    )
                    INSERT INTO Registrations 
                    (student_id, course_id, year_number, status, course_classroom_id, academic_year_start)
                    VALUES 
                    (@studentId, @courseId, @year, N'مسجل', @groupId, @academicYearStart)", con);

                                insertCmd.Parameters.AddWithValue("@studentId", studentId);
                                insertCmd.Parameters.AddWithValue("@courseId", courseId);
                                insertCmd.Parameters.AddWithValue("@year", newYear);
                                insertCmd.Parameters.AddWithValue("@groupId", groupId);
                                insertCmd.Parameters.AddWithValue("@academicYearStart", academicYearStart); 
                                int affected = insertCmd.ExecuteNonQuery();
                                if (affected > 0)
                                {
                                    result.RegisteredCourses++;
                                    registered = true;
                                    break;
                                }
                            }
                        }

                        if (!registered)
                        {
                            result.FullCourses.Add(courseName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message);
            }

            return result;
        }

        public void downloadForOneStudents(int st_update_id,int newYear,int dep_id)
        {
            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    int studentId = st_update_id;
                    int year = newYear;
                    int dept = dep_id;

                    // ✅ تحقق أن الطالب من القسم المحدد
                    SqlCommand checkCmd = new SqlCommand(@"
            SELECT COUNT(*) 
            FROM Students 
            WHERE student_id = @studentId AND department_id = @departmentId", con);

                    checkCmd.Parameters.AddWithValue("@studentId", studentId);
                    checkCmd.Parameters.AddWithValue("@departmentId", dept);

                    int count = (int)checkCmd.ExecuteScalar();

                    if (count == 0)
                    {
                        MessageBox.Show("خطأ: الطالب لا ينتمي إلى القسم المحدد، لا يمكن تنزيل المواد.");
                        return;
                    }

                    // ✅ جلب المواد الخاصة بالسنة والقسم
                    SqlCommand coursesCmd = new SqlCommand(@"
                                                        SELECT c.course_id, c.course_name
                                                        FROM Courses c
                                                        JOIN Course_Department cd ON cd.course_id = c.course_id
                                                        WHERE c.year_number = @year AND cd.department_id = @dept", con);

                    coursesCmd.Parameters.AddWithValue("@year", year);
                    coursesCmd.Parameters.AddWithValue("@dept", dept);

                    SqlDataAdapter adapter = new SqlDataAdapter(coursesCmd);
                    DataTable courses = new DataTable();
                    adapter.Fill(courses);

                    List<string> موادممتلئة = new List<string>();
                    int عددالموادالمسجلة = 0;

                    foreach (DataRow row in courses.Rows)
                    {
                        int courseId = Convert.ToInt32(row["course_id"]);
                        string courseName = row["course_name"].ToString();

                        // ✅ جلب كل المجموعات المرتبطة بالمادة
                        SqlCommand getGroupsCmd = new SqlCommand(@"
                                                                SELECT cc.id,
                                                                cc.capacity,       
                                                                cc.group_number
                                                                FROM Course_Classroom cc
                                                                WHERE cc.course_id = @courseId
                                                                ORDER BY cc.group_number;
                                                                ", con); // أو حسب cc.id إن أردت

                        getGroupsCmd.Parameters.AddWithValue("@courseId", courseId);

                        SqlDataAdapter groupAdapter = new SqlDataAdapter(getGroupsCmd);
                        DataTable groups = new DataTable();
                        groupAdapter.Fill(groups);

                        bool تمت_الإضافة = false;

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
                                int month2;

                                using (SqlCommand cmddate = new SqlCommand("SELECT month_number FROM Months WHERE month_id = 1 ", con))
                                {
                                    month2 = Convert.ToInt32(cmddate.ExecuteScalar());
                                }

                                int academicYearStart = DateTime.Now.Month >= month2 ? DateTime.Now.Year : DateTime.Now.Year - 1;


                                SqlCommand insertCmd = new SqlCommand(@"
                                IF NOT EXISTS (
                                    SELECT 1 FROM Registrations 
                                    WHERE student_id = @studentId AND course_id = @courseId AND academic_year_start = @academicYearStart
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
                                    عددالموادالمسجلة++;
                                    تمت_الإضافة = true;
                                }

                                break; // سجل الطالب، لا داعي لتجربة بقية المجموعات
                            }
                        }

                        if (!تمت_الإضافة)
                        {
                            موادممتلئة.Add(courseName);
                        }
                    }

                    // ✅ عرض النتيجة النهائية
                    if (عددالموادالمسجلة == 0)
                    {
                        MessageBox.Show("لم يتم تسجيل أي مادة. الطالب قد يكون مسجلاً مسبقًا أو لا توجد مقاعد متاحة.");
                    }
                    else if (موادممتلئة.Count > 0)
                    {
                        MessageBox.Show("تم تسجيل الطالب، باستثناء المواد التالية التي لم يتوفر بها مقاعد:\n" +
                                        string.Join("\n", موادممتلئة));
                    }
                    else
                    {
                        MessageBox.Show("تم تسجيل جميع المواد بنجاح.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ: " + ex.Message);
            }
        }


        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                conn.DatabaseConnection db3 = new conn.DatabaseConnection();
                using (SqlConnection con = db3.OpenConnection())
                {
                    int month2;
                    using (SqlCommand cmddate = new SqlCommand("SELECT month_number FROM Months WHERE month_id = 1", con))
                    {
                        month2 = Convert.ToInt32(cmddate.ExecuteScalar());
                    }
                    if (DateTime.Now.Month < month2)
                    {
                        MessageBox.Show("لايمكن الترقية قبل بداية السنة الدراسية الجديدة");
                        return;
                    }
                }
            }
            catch(SqlException ex)
            {
                MessageBox.Show("Error : " + ex.Message);
            }

            string value = Interaction.InputBox("تأكيد الترقية", "هل متأكد من الترقية ؟\nللتأكيد ادخل الرمز للتأكيد", "الرمز هنا");
            if (value != "2025")
                return;
            else MessageBox.Show("رمز خاطئ يرجى إعادة المحاولة");
            label1.Visible = true;
            progressBar1.Visible = true;
            progressBar1.Style = ProgressBarStyle.Marquee;
            button6.Enabled = false;
            Application.DoEvents(); // تحديث الواجهة قبل بدء المعالجة

            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    // جلب الطلاب الذين تنطبق عليهم الشروط
                    SqlCommand cmd = new SqlCommand(@"
                SELECT student_id, current_year, department_id
                FROM Students
                WHERE status_id = '1'
                AND exam_round = N'دور أول'", con);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable students = new DataTable();
                    adapter.Fill(students);
                    int promoted = 0;
                    int failedDownload = 0;
                    int partialDownload = 0;
                    int fullSuccess = 0;

                    foreach (DataRow row in students.Rows)
                    {
                        int studentId = Convert.ToInt32(row["student_id"]);
                        int currentYear = Convert.ToInt32(row["current_year"]);
                        int departmentId = Convert.ToInt32(row["department_id"]);

                        int newYear = (currentYear == 1) ? 2 : (currentYear == 2) ? 3 : (currentYear == 3) ? 4 : currentYear;

                        // Update student year
                        SqlCommand updateCmd = new SqlCommand(@"UPDATE Students SET current_year = @newYear WHERE student_id = @studentId", con);
                        updateCmd.Parameters.AddWithValue("@newYear", newYear);
                        updateCmd.Parameters.AddWithValue("@studentId", studentId);
                        updateCmd.ExecuteNonQuery();

                        // Download courses
                        var result = DownloadForOneStudent(studentId, newYear, departmentId);
                        promoted++;

                        if (result.AllCoursesFull)
                            failedDownload++;
                        else if (result.SomeCoursesFull)
                            partialDownload++;
                        else if (result.HasAnyRegistered)
                            fullSuccess++;
                    }
                    MessageBox.Show($@"نتيجة الترقية:
                                    ✔️ عدد الطلاب الذين تمت ترقيتهم: {promoted}
                                    ✅ نجاح كامل (تم تسجيل جميع المواد): {fullSuccess}
                                    ⚠️ نجاح جزئي (تم تسجيل بعض المواد فقط): {partialDownload}
                                    ❌ فشل في تسجيل أي مادة: {failedDownload}");



                    //foreach (DataRow row in students.Rows)
                    //{
                    //    int studentId = Convert.ToInt32(row["student_id"]);
                    //    int currentYear = Convert.ToInt32(row["current_year"]);
                    //    int departmentId = Convert.ToInt32(row["department_id"]);

                    //    int newYear;
                    //    if (currentYear == 1)
                    //        newYear = 2;
                    //    else if (currentYear == 2)
                    //        newYear = 3;
                    //    else if (currentYear == 3)
                    //        newYear = 4;
                    //    else
                    //        newYear = currentYear; // بدون تغيير


                    //    // ترقية الطالب في الجدول
                    //    SqlCommand updateCmd = new SqlCommand(@"
                    //UPDATE Students
                    //SET current_year = @newYear
                    //WHERE student_id = @studentId", con);

                    //    updateCmd.Parameters.AddWithValue("@newYear", newYear);
                    //    updateCmd.Parameters.AddWithValue("@studentId", studentId);
                    //    updateCmd.ExecuteNonQuery();

                    //    // ثم تنزيل المواد له
                    //    downloadForOneStudents(studentId, newYear, departmentId);
                    //}

                    //MessageBox.Show("تمت ترقية وتنزيل المواد لجميع طلاب الدور الاول بنجاح.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ: " + ex.Message);
            }

            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    // تحديد العام الجامعي الحالي
                    int month2;
                    using (SqlCommand cmddate = new SqlCommand("SELECT month_number FROM Months WHERE month_id = 1", con))
                    {
                        month2 = Convert.ToInt32(cmddate.ExecuteScalar());
                    }
                    int academicYearStart = DateTime.Now.Month >= month2 ? DateTime.Now.Year : DateTime.Now.Year - 1;

                    // جلب الطلاب الذين تنطبق عليهم الشروط
                    SqlCommand cmd = new SqlCommand(@"
                                                 SELECT student_id, current_year, department_id
                                                 FROM Students
                                                 WHERE status_id = '1'
                                                 AND exam_round = N'مرحل'", con);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable students = new DataTable();
                    adapter.Fill(students);

                    foreach (DataRow row in students.Rows)
                    {
                        int studentId = Convert.ToInt32(row["student_id"]);
                        int currentYear = Convert.ToInt32(row["current_year"]);
                        int departmentId = Convert.ToInt32(row["department_id"]);

                        int newYear;
                        if (currentYear == 1)
                            newYear = 2;
                        else if (currentYear == 2)
                            newYear = 3;
                        else if (currentYear == 3)
                            newYear = 4;
                        else
                            newYear = currentYear;

                        // ترقية الطالب
                        SqlCommand updateCmd = new SqlCommand(@"
                UPDATE Students
                SET current_year = @newYear,
                exam_round = 'دور أول'
                WHERE student_id = @studentId", con);

                        updateCmd.Parameters.AddWithValue("@newYear", newYear);
                        updateCmd.Parameters.AddWithValue("@studentId", studentId);
                        updateCmd.ExecuteNonQuery();

                        // تنزيل المواد الجديدة
                        downloadForOneStudents(studentId, newYear, departmentId);

                        // العام الجامعي السابق حيث توجد المواد الراسبة
                        // جلب المواد الراسبة
                        SqlCommand cmdcarry = new SqlCommand(@"
                                                         SELECT r.course_id
                                                         FROM Registrations r
                                                         JOIN Grades g ON r.course_id = g.course_id AND r.student_id = g.student_id
                                                         WHERE r.student_id = @student_id
                                                         AND r.academic_year_start = @academic_year_start 
                                                         AND g.success_status = N'رسوب'", con);

                        cmdcarry.Parameters.AddWithValue("@student_id", studentId);
                        cmdcarry.Parameters.AddWithValue("@academic_year_start", academicYearStart -1);

                        SqlDataAdapter ada = new SqlDataAdapter(cmdcarry);
                        DataTable carryCourses = new DataTable();
                        ada.Fill(carryCourses);

                        foreach (DataRow carry in carryCourses.Rows)
                        {
                            int courseId = Convert.ToInt32(carry["course_id"]);

                            SqlCommand cmdUpdatecarry = new SqlCommand(@"
                      UPDATE Registrations
                      SET academic_year_start = academic_year_start + 1
                      WHERE course_id = @course_id
                      AND student_id = @student_id;

                      UPDATE Grades
                      SET final_grade = NULL,
                        work_grade = NULL,
                        total_grade = NULL,
                        success_status = NULL
                      WHERE course_id = @course_id
                      AND student_id = @student_id;", con);

                            cmdUpdatecarry.Parameters.AddWithValue("@student_id", studentId);
                            cmdUpdatecarry.Parameters.AddWithValue("@course_id", courseId);
                            cmdUpdatecarry.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("تمت ترقية وتنزيل المواد ومعالجة الراسبين بنجاح.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ: " + ex.Message);
            }

            try
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                using (SqlConnection con = db.OpenConnection())
                {
                    int month2;
                    using (SqlCommand cmddate = new SqlCommand("SELECT month_number FROM Months WHERE month_id = 1", con))
                    {
                        month2 = Convert.ToInt32(cmddate.ExecuteScalar());
                    }
                    int academicYearStart = DateTime.Now.Month >= month2 ? DateTime.Now.Year : DateTime.Now.Year - 1;


                    // جلب الطلاب الذين تنطبق عليهم الشروط
                    SqlCommand cmd = new SqlCommand(@"
                SELECT student_id, current_year, department_id
                FROM Students
                WHERE status_id = '1'
                AND exam_round = N'إعادة سنة' ", con);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable students = new DataTable();
                    adapter.Fill(students);

                    foreach (DataRow row in students.Rows)
                    {
                        int studentId = Convert.ToInt32(row["student_id"]);
                        int currentYear = Convert.ToInt32(row["current_year"]);

                        int newYear = currentYear; // بدون تغيير


                        // ترقية الطالب في الجدول
                        SqlCommand updateCmd = new SqlCommand(@"
                        UPDATE Students
                        SET exam_round = N'دور أول'  
                        WHERE student_id = @studentId ", con);
                        updateCmd.Parameters.AddWithValue("@studentId", studentId);
                        updateCmd.ExecuteNonQuery();

                        SqlCommand prevCourses = new SqlCommand(@"
                                                         SELECT r.course_id
                                                         FROM Registrations r
                                                         JOIN Grades g ON r.course_id = g.course_id AND r.student_id = g.student_id
                                                         WHERE r.student_id = @student_id
                                                         AND r.academic_year_start = @academic_year_start
                                                         AND g.success_status = 'رسوب' ", con);

                        prevCourses.Parameters.AddWithValue("@student_id", studentId);
                        prevCourses.Parameters.AddWithValue("@academic_year_start", academicYearStart - 1);

                        SqlDataAdapter ada = new SqlDataAdapter(prevCourses);
                        DataTable previosCourses = new DataTable();
                        ada.Fill(previosCourses);

                        foreach (DataRow pcourse in previosCourses.Rows)
                        {
                            int courseId = Convert.ToInt32(pcourse["course_id"]);

                            SqlCommand cmdrefresh = new SqlCommand(@"
                      UPDATE Registrations
                      SET academic_year_start = academic_year_start + 1
                      WHERE course_id = @course_id
                      AND student_id = @student_id;

                      UPDATE Grades
                      SET final_grade = NULL,
                        work_grade = NULL,
                        total_grade = NULL,
                        success_status = NULL
                      WHERE course_id = @course_id
                      AND student_id = @student_id;", con);

                            cmdrefresh.Parameters.AddWithValue("@student_id", studentId);
                            cmdrefresh.Parameters.AddWithValue("@course_id", courseId);
                            cmdrefresh.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("تمت اعادة السنة للطلبة الراسبين بنجاح.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ: " + ex.Message);
            }

            progressBar1.Visible = false;
            button6.Enabled = true;
            label1.Visible = false;
        }
    }
}