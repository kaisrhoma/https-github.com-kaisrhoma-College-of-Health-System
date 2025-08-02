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
    public partial class statements_reports : UserControl
    {
        private PrintDocument printDocument1 = new PrintDocument();
        private DataTable reportData;
        private PrintDocument printDocument2 = new PrintDocument();
        private PrintDocument printDocument3 = new PrintDocument();

        private List<string> pageSummaries = new List<string>();
        private string studentName = "", universityNumber = "";

        //3
        private List<DataTable> subjectPages = new List<DataTable>();
        private int currentPrintIndex = 0;
        private string selectedYear = "";
        private string selectedDepartment = "";
        private int currentRowIndex = 0;

        private PrintDocument printDocument12 = new PrintDocument();


        public statements_reports()
        {
            InitializeComponent();
            comboBox_Year2.Items.Add("1");
            comboBox_Year2.Items.Add("2");
            comboBox_Year2.Items.Add("3");
            comboBox_Year2.Items.Add("4");

            comboBox_Year2.SelectedIndex = 0;
            dataGridViewGrades.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            printDocument1.PrintPage += printDocument1_PrintPage;
            printDocument2.PrintPage += printDocument2_PrintPage;
            printDocument3.PrintPage += printDocument3_PrintPage;

            //3
            comboBox_Year.Items.AddRange(new object[] { 1, 2, 3, 4 });

            comboBox_Year.SelectedIndex = 0;
            FillDepartmentComboBox();
            printDocument12.PrintPage += printDocument12_PrintPage;
        }
        private int currentPageIndex = 0;
        private List<DataTable> pages = new List<DataTable>();




        private int currentSubjectIndex = 0;
        private int currentStudentIndex = 0;



        private int currentPageInSubject = 1;


        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (pages == null || pages.Count == 0 || currentSubjectIndex >= pages.Count)
            {
                e.HasMorePages = false;
                // إعادة تعيين المؤشرات للطباعة القادمة
                currentSubjectIndex = 0;
                currentStudentIndex = 0;
                currentPageInSubject = 1;
                return;
            }

            DataTable dt = pages[currentSubjectIndex];
            if (dt.Rows.Count == 0)
            {
                currentSubjectIndex++;
                currentStudentIndex = 0;
                currentPageInSubject = 1;
                e.HasMorePages = currentSubjectIndex < pages.Count;
                return;
            }

            // الخطوط والألوان
            Font titleFont = new Font("Arial", 14, FontStyle.Bold);
            Font headerFont = new Font("Arial", 12, FontStyle.Bold);
            Font textFont = new Font("Arial", 11);
            Brush brush = Brushes.Black;

            int x = 50;
            int y = 50;
            int tableWidth = 680;
            StringFormat centerFormat = new StringFormat { Alignment = StringAlignment.Center };

            // رسم رأس التقرير
            e.Graphics.DrawString("دولة ليبيا", titleFont, brush, x + tableWidth / 2, y, centerFormat); y += 30;
            e.Graphics.DrawString("وزارة التعليم", titleFont, brush, x + tableWidth / 2, y, centerFormat); y += 30;
            e.Graphics.DrawString("جامعة غريان", titleFont, brush, x + tableWidth / 2, y, centerFormat); y += 30;
            e.Graphics.DrawString("كلية العلوم الصحية", titleFont, brush, x + tableWidth / 2, y, centerFormat); y += 30;
            e.Graphics.DrawString("التاريخ: " + DateTime.Now.ToString("yyyy/MM/dd"), textFont, brush, x + tableWidth / 2, y, centerFormat); y += 40;

            // بيانات المادة
            int colWidth = tableWidth / 3;
            int rowHeight = 30;

            DataRow firstRow = dt.Rows[0];

            string courseName = firstRow["اسم المادة"].ToString();
            string courseId = firstRow["رقم المادة"].ToString();
            string year = firstRow["السنة الدراسية"].ToString();
            string group = dt.Columns.Contains("رقم المجموعة") && !string.IsNullOrEmpty(firstRow["رقم المجموعة"]?.ToString())
                  ? firstRow["رقم المجموعة"].ToString()
                  : "1";

            string instructor = string.IsNullOrEmpty(firstRow["اسم الأستاذ"]?.ToString()) ? "غير معروف" : firstRow["اسم الأستاذ"].ToString();
            string failedCount = dt.Rows.Count.ToString();

            string[] infoTitles = { "اسم الأستاذ", "السنة الدراسية", "اسم المادة" };
            string[] infoValues = { instructor, year, courseName };

            string[] infoTitles2 = { "رقم المادة", "رقم المجموعة", "عدد الطلاب" };
            string[] infoValues2 = { courseId, group, failedCount };

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

            // جدول الطلاب
            int numberingColumnWidth = 50;
            int studentNameWidth = 270 - numberingColumnWidth;
            string[] headers = { "رقم", "اسم الطالب", "الرقم الجامعي", "القسم", "الدرجة", "النتيجة" };
            int[] columnWidths = { numberingColumnWidth, studentNameWidth, 100, 150, 80, 80 };

            int rowHeightStudents = 30;
            int tableX = x;
            int tableY = y;

            int totalWidth = columnWidths.Sum();
            int tableRight = tableX + totalWidth;

            int colXDraw;

            using (Pen thickPen = new Pen(Color.Black, 2))
            {
                colXDraw = tableRight;
                for (int i = 0; i < headers.Length; i++)
                {
                    colXDraw -= columnWidths[i];
                    Rectangle rect = new Rectangle(colXDraw, tableY, columnWidths[i], rowHeightStudents);
                    e.Graphics.DrawRectangle(thickPen, rect);
                    e.Graphics.DrawString(headers[i], headerFont, brush, rect,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }
            }

            tableY += rowHeightStudents;

            int studentNumbering = currentStudentIndex + 1;

            // طباعة الطلاب حتى انتهاء الصفحة أو انتهاء البيانات
            while (currentStudentIndex < dt.Rows.Count)
            {
                if (tableY + rowHeightStudents > e.MarginBounds.Bottom - 50)
                {
                    // انتهت الصفحة، نعطي إشارة للطباعة صفحة أخرى بنفس المادة
                    e.HasMorePages = true;
                    currentPageInSubject++; // زيادة رقم الصفحة داخل المادة
                    return;
                }

                DataRow row = dt.Rows[currentStudentIndex];
                colXDraw = tableRight;

                string[] values =
                {
            studentNumbering.ToString(),
            row["اسم الطالب"].ToString(),
            row["الرقم الجامعي"].ToString(),
            row["القسم"].ToString(),
            row["الدرجة"].ToString(),
            row["النتيجة"].ToString()
        };

                using (Pen pen = new Pen(Color.Black, 1))
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        colXDraw -= columnWidths[i];
                        Rectangle rect = new Rectangle(colXDraw, tableY, columnWidths[i], rowHeightStudents);
                        e.Graphics.DrawRectangle(pen, rect);
                        e.Graphics.DrawString(values[i], textFont, brush, rect,
                            new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center });
                    }
                }

                tableY += rowHeightStudents;
                currentStudentIndex++;
                studentNumbering++;
                // طباعة ترقيم الصفحة داخل المادة (نطرح 1 لأننا زدنا currentPageInSubject قبل الخروج)
                string pageText = $"صفحة {currentPageInSubject}";
                e.Graphics.DrawString(pageText, textFont, brush, x + tableWidth / 2, e.MarginBounds.Bottom + 10, centerFormat);
            }

            // انتهينا من طباعة كل طلاب المادة الحالية
            currentSubjectIndex++;
            currentStudentIndex = 0;
            currentPageInSubject = 1; // إعادة تعيين رقم صفحة المادة الجديدة

   
          

            // هل هناك مواد أخرى للطباعة؟
            e.HasMorePages = currentSubjectIndex < pages.Count;

            if (!e.HasMorePages)
            {
                // إعادة تعيين المؤشرات عند انتهاء الطباعة
                currentSubjectIndex = 0;
                currentStudentIndex = 0;
                currentPageInSubject = 1;
            }
        }












        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (comboBox_Year2.SelectedItem == null)
            {
                MessageBox.Show("يرجى اختيار السنة الدراسية.");
                return;
            }

            int selectedYear = Convert.ToInt32(comboBox_Year2.SelectedItem);

            string query = @"
SELECT 
    c.course_name AS 'اسم المادة',
    c.course_id AS 'رقم المادة',
    c.year_number AS 'السنة الدراسية',
    i.full_name AS 'اسم الأستاذ',
    s.full_name AS 'اسم الطالب',
    s.university_number AS 'الرقم الجامعي',
    d.dep_name AS 'القسم',
    g.total_grade AS 'الدرجة',
    g.success_status AS 'النتيجة'
FROM Grades g
INNER JOIN Students s ON g.student_id = s.student_id
INNER JOIN Courses c ON g.course_id = c.course_id
INNER JOIN Departments d ON s.department_id = d.department_id
LEFT JOIN Course_Instructor ci ON c.course_id = ci.course_id
LEFT JOIN Instructors i ON ci.instructor_id = i.instructor_id
WHERE c.year_number = @year
ORDER BY c.course_id,s.university_number;";


            using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@year", selectedYear);

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                dataGridViewGrades.DataSource = dt;
                dataGridViewGrades.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                // احفظ البيانات لطباعتها لاحقاً
                reportData = (DataTable)dataGridViewGrades.DataSource;

                //// اربط دالة الطباعة بالـ PrintDocument
                //printDocument1.PrintPage -= printDocument1_PrintPage; // لتجنب التكرار عند الطباعة أكثر من مرة
                //printDocument1.PrintPage += printDocument1_PrintPage;



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

        private void button1_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dataGridViewGrades.DataSource;
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

            //printDocument1.PrintPage -= printDocument1_PrintPage;
            //printDocument1.PrintPage += printDocument1_PrintPage;



            PrintPreviewDialog previewDialog = new PrintPreviewDialog();

            previewDialog.Document = printDocument1;

            previewDialog.ShowDialog();

            // printDocument1.Print();




        }
        //--------------------------------------------------------------------------------------------------------------------2
        private void PrepareStudentReportPages(DataTable dt)
        {
            pages.Clear();
            pageSummaries.Clear();

            var groupedByYear = dt.AsEnumerable().GroupBy(r => r.Field<int>("السنة"));

            foreach (var group in groupedByYear)
            {
                DataTable page = dt.Clone();
                foreach (var row in group)
                {
                    page.ImportRow(row);
                }

                pages.Add(page);
            }

            if (dt.Rows.Count > 0)
            {
                studentName = dt.Rows[0]["اسم_الطالب"].ToString();
                universityNumber = dt.Rows[0]["الرقم_الجامعي"].ToString();
            }

            currentPageIndex = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string uniNumber = txtUniversityNumber.Text.Trim();
            if (string.IsNullOrEmpty(uniNumber))
            {
                MessageBox.Show("يرجى إدخال الرقم الجامعي.");
                return;
            }

            string query = @"
    SELECT 
    s.full_name AS اسم_الطالب,
    s.university_number AS الرقم_الجامعي,
    c.year_number AS السنة,
    c.course_id AS رقم_المادة,
    c.course_name AS المادة,
    c.units AS الوحدات,
    g.total_grade AS الدرجة
FROM Grades g
INNER JOIN Students s ON g.student_id = s.student_id
INNER JOIN Courses c ON g.course_id = c.course_id
WHERE s.university_number = @university_number
ORDER BY c.year_number, c.course_name;";

            using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@university_number", uniNumber);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                reportData = new DataTable();
                da.Fill(reportData);
            }

            if (reportData.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات لهذا الرقم الجامعي.");
                return;
            }

            dataGridViewReport.DataSource = reportData;
            dataGridViewReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // حساب المعدلات
            CalculateAndDisplayAverages(reportData);
        }

        private void CalculateAndDisplayAverages(DataTable dt)
        {
            var groupedByYear = dt.AsEnumerable()
           .GroupBy(r => r.Field<int>("السنة"));

            double totalWeightedGrades = 0;
            int totalUnits = 0;

            string averagesText = "";

            foreach (var yearGroup in groupedByYear)
            {
                int year = yearGroup.Key;
                double sumWeightedGrades = 0;
                int sumUnits = 0;

                foreach (var row in yearGroup)
                {
                    int grade = 0;
                    int units = 0;

                    // قراءة آمنة دون رمي استثناء
                    if (row["الدرجة"] != DBNull.Value && row["الوحدات"] != DBNull.Value)
                    {
                        try
                        {
                            grade = Convert.ToInt32(row["الدرجة"]);
                            units = Convert.ToInt32(row["الوحدات"]);
                        }
                        catch
                        {
                            // تجاهل الصف إذا كان التحويل غير ممكن
                            continue;
                        }

                        sumWeightedGrades += grade * units;
                        sumUnits += units;
                    }
                }

                double yearAverage = sumUnits == 0 ? 0 : sumWeightedGrades / sumUnits;
                averagesText += $"معدل السنة {year}: {yearAverage:F2}\n";

                totalWeightedGrades += sumWeightedGrades;
                totalUnits += sumUnits;
            }

            double cumulativeAverage = totalUnits == 0 ? 0 : totalWeightedGrades / totalUnits;
            averagesText += $"المعدل التراكمي: {cumulativeAverage:F2}";

          
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (reportData == null || reportData.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات للطباعة.");
                return;
            }

            PrepareStudentReportPages(reportData);
            currentPageIndex = 0;

            //// ✅ استخدم printDocument2 بدلًا من printDocument1
            PrintPreviewDialog preview = new PrintPreviewDialog();
            preview.Document = printDocument2;
            preview.ShowDialog();

            // أو لطباعة مباشرة:
            //printDocument2.Print();
        }



        private void printDocument2_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (currentPageIndex >= pages.Count)
            {
                e.HasMorePages = false;
                return;
            }

            DataTable dt = pages[currentPageIndex];
            Font headerFont = new Font("Arial", 18, FontStyle.Bold);
            Font subHeaderFont = new Font("Arial", 12, FontStyle.Bold);
            Font textFont = new Font("Arial", 10);
            Brush brush = Brushes.Black;
            int margin = 50;
            int y = margin;
            int pageWidth = e.PageBounds.Width - 2 * margin;
            int pageHeight = e.PageBounds.Height;
            int x = margin;

            StringFormat centerFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.DirectionRightToLeft
            };

            // عنوان الكلية والتقرير
            e.Graphics.DrawString("جامعة غريان", headerFont, brush, new Rectangle(x, y, pageWidth, 30), centerFormat);
            y += 35;
            e.Graphics.DrawString("كلية العلوم الصحية", headerFont, brush, new Rectangle(x, y, pageWidth, 30), centerFormat);
            y += 35;
            e.Graphics.DrawString("كشف التخرج", subHeaderFont, brush, new Rectangle(x, y, pageWidth, 30), centerFormat);
            y += 50;

            // جدول معلومات الطالب
            string[] infoHeaders = { "اسم الطالب", "رقم القيد", "تاريخ الطباعة" };
            string[] infoValues = { studentName, universityNumber, DateTime.Now.ToString("yyyy/MM/dd") };
            int infoColWidth = pageWidth / 3;
            int infoRowHeight = 25;

            for (int i = 0; i < 3; i++)
            {
                int colIndex = 2 - i;
                Rectangle rectHeader = new Rectangle(x + i * infoColWidth, y, infoColWidth, infoRowHeight);
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, 230, 250)), rectHeader);
                e.Graphics.DrawRectangle(Pens.Black, rectHeader);
                e.Graphics.DrawString(infoHeaders[colIndex], subHeaderFont, brush, rectHeader, centerFormat);

                Rectangle rectValue = new Rectangle(x + i * infoColWidth, y + infoRowHeight, infoColWidth, infoRowHeight);
                e.Graphics.DrawRectangle(Pens.Black, rectValue);
                e.Graphics.DrawString(infoValues[colIndex], textFont, brush, rectValue, centerFormat);
            }

            y += infoRowHeight * 2 + 20;

            // جدول الدرجات (معكوس: نبدأ من المادة يمينًا)
            string[] gradeHeaders = { "المادة", "رمز المادة", "الدرجة", "عدد الوحدات", "عدد النقاط", "نتيجة المادة", "ملاحظة" };
            int gradeColCount = gradeHeaders.Length;
            int gradeColWidth = pageWidth / gradeColCount;
            int gradeRowHeight = 25;

            for (int i = 0; i < gradeColCount; i++)
            {
                int colIndex = gradeColCount - 1 - i; // لعكس الاتجاه
                Rectangle rect = new Rectangle(x + i * gradeColWidth, y, gradeColWidth, gradeRowHeight);
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, 230, 250)), rect);
                e.Graphics.DrawRectangle(Pens.Black, rect);
                e.Graphics.DrawString(gradeHeaders[colIndex], subHeaderFont, brush, rect, centerFormat);
            }

            y += gradeRowHeight;

            double sumPoints = 0;
            int sumUnits = 0;
            int completedUnits = 0;

            foreach (DataRow row in dt.Rows)
            {
                string subject = row["المادة"].ToString();
                string code = row["رقم_المادة"].ToString();
                int grade = Convert.ToInt32(row["الدرجة"]);
                int units = Convert.ToInt32(row["الوحدات"]);
                int points = grade * units;
                string result = grade >= 50 ? "ناجح" : "راسب";
                string note = "";

                sumPoints += points;
                sumUnits += units;
                if (grade >= 50) completedUnits += units;

                string[] values = { subject, code, grade.ToString(), units.ToString(), points.ToString(), result, note };

                for (int i = 0; i < values.Length; i++)
                {
                    int colIndex = values.Length - 1 - i; // لعكس الاتجاه
                    Rectangle rect = new Rectangle(x + i * gradeColWidth, y, gradeColWidth, gradeRowHeight);
                    e.Graphics.DrawRectangle(Pens.Black, rect);
                    e.Graphics.DrawString(values[colIndex], textFont, brush, rect, centerFormat);
                }

                y += gradeRowHeight;
            }

            y += 20;

            // جدول الملخص
            double totalPoints = 0;
            int totalUnits = 0;

            foreach (DataTable page in pages)
            {
                foreach (DataRow row in page.Rows)
                {
                    int g = Convert.ToInt32(row["الدرجة"]);
                    int u = Convert.ToInt32(row["الوحدات"]);
                    totalPoints += g * u;
                    totalUnits += u;
                }
            }

            double semesterGPA = sumUnits == 0 ? 0 : sumPoints / sumUnits;
            double cumulativeGPA = totalUnits == 0 ? 0 : totalPoints / totalUnits;

            string[] summaryHeaders = currentPageIndex == pages.Count - 1
                ? new string[] { "الوحدات المسجلة", "الوحدات المنجزة", "إجمالي النقاط", "المعدل السنوي", "المعدل التراكمي" }
                : new string[] { "الوحدات المسجلة", "الوحدات المنجزة", "إجمالي النقاط", "المعدل السنوي" };

            string[] summaryValues = currentPageIndex == pages.Count - 1
                ? new string[] { sumUnits.ToString(), completedUnits.ToString(), ((int)sumPoints).ToString(), semesterGPA.ToString("F2"), cumulativeGPA.ToString("F2") }
                : new string[] { sumUnits.ToString(), completedUnits.ToString(), ((int)sumPoints).ToString(), semesterGPA.ToString("F2") };

            int summaryColCount = summaryHeaders.Length;
            int summaryColWidth = pageWidth / summaryColCount;

            for (int i = 0; i < summaryColCount; i++)
            {
                int colIndex = summaryColCount - 1 - i;
                Rectangle rectHeader = new Rectangle(x + i * summaryColWidth, y, summaryColWidth, gradeRowHeight);
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, 230, 250)), rectHeader);
                e.Graphics.DrawRectangle(Pens.Black, rectHeader);
                e.Graphics.DrawString(summaryHeaders[colIndex], subHeaderFont, brush, rectHeader, centerFormat);

                Rectangle rectValue = new Rectangle(x + i * summaryColWidth, y + gradeRowHeight, summaryColWidth, gradeRowHeight);
                e.Graphics.DrawRectangle(Pens.Black, rectValue);
                e.Graphics.DrawString(summaryValues[colIndex], textFont, brush, rectValue, centerFormat);
            }

            // **اجعل التوقيعات في أسفل الصفحة مهما كان المحتوى**

            int signHeight = 50;
            int signY = pageHeight - margin - signHeight; // مكان التوقيعات في أسفل الصفحة

            string[] signatures = { "عميد الكلية", "قسم الدراسة والامتحانات", "القسم العلمي" };
            int signCount = signatures.Length;
            int signColWidth = pageWidth / signCount;

            Pen dottedPen = new Pen(Color.Black);
            dottedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

            for (int i = 0; i < signCount; i++)
            {
                int posX = x + i * signColWidth;
                // نص التوقيع فوق الخط
                Rectangle rectSignText = new Rectangle(posX, signY, signColWidth, gradeRowHeight);
                e.Graphics.DrawString(signatures[i], textFont, brush, rectSignText, centerFormat);

                // خط التوقيع المنقط تحت النص
                int lineY = signY + gradeRowHeight + 5;
                e.Graphics.DrawLine(dottedPen, posX + 10, lineY, posX + signColWidth - 10, lineY);
            }

            currentPageIndex++;
            e.HasMorePages = currentPageIndex < pages.Count;
        }



        //--------------------------------------------------------------------------------------3
        private void printDocument3_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (currentPageIndex >= pages.Count)
            {
                e.HasMorePages = false;
                return;
            }

            DataTable dt = pages[currentPageIndex];
            Font headerFont = new Font("Arial", 18, FontStyle.Bold);
            Font subHeaderFont = new Font("Arial", 12, FontStyle.Bold);
            Font textFont = new Font("Arial", 10);
            Brush brush = Brushes.Black;
            int margin = 50;
            int y = margin;
            int pageWidth = e.PageBounds.Width - 2 * margin;
            int pageHeight = e.PageBounds.Height;
            int x = margin;

            StringFormat centerFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.DirectionRightToLeft
            };

            // عنوان الكلية والتقرير
            e.Graphics.DrawString("جامعة غريان", headerFont, brush, new Rectangle(x, y, pageWidth, 30), centerFormat);
            y += 35;
            e.Graphics.DrawString("كلية العلوم الصحية", headerFont, brush, new Rectangle(x, y, pageWidth, 30), centerFormat);
            y += 35;
            e.Graphics.DrawString("كشف درجات", subHeaderFont, brush, new Rectangle(x, y, pageWidth, 30), centerFormat);
            y += 50;

            // جدول معلومات الطالب
            string[] infoHeaders = { "اسم الطالب", "رقم القيد", "تاريخ الطباعة" };
            string[] infoValues = { studentName, universityNumber, DateTime.Now.ToString("yyyy/MM/dd") };
            int infoColWidth = pageWidth / 3;
            int infoRowHeight = 25;

            for (int i = 0; i < 3; i++)
            {
                int colIndex = 2 - i;
                Rectangle rectHeader = new Rectangle(x + i * infoColWidth, y, infoColWidth, infoRowHeight);
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, 230, 250)), rectHeader);
                e.Graphics.DrawRectangle(Pens.Black, rectHeader);
                e.Graphics.DrawString(infoHeaders[colIndex], subHeaderFont, brush, rectHeader, centerFormat);

                Rectangle rectValue = new Rectangle(x + i * infoColWidth, y + infoRowHeight, infoColWidth, infoRowHeight);
                e.Graphics.DrawRectangle(Pens.Black, rectValue);
                e.Graphics.DrawString(infoValues[colIndex], textFont, brush, rectValue, centerFormat);
            }

            y += infoRowHeight * 2 + 20;

            // جدول الدرجات (معكوس: نبدأ من المادة يمينًا)
            string[] gradeHeaders = { "المادة", "رمز المادة", "الدرجة", "عدد الوحدات", "عدد النقاط", "نتيجة المادة", "ملاحظة" };
            int gradeColCount = gradeHeaders.Length;
            int gradeColWidth = pageWidth / gradeColCount;
            int gradeRowHeight = 25;

            for (int i = 0; i < gradeColCount; i++)
            {
                int colIndex = gradeColCount - 1 - i; // لعكس الاتجاه
                Rectangle rect = new Rectangle(x + i * gradeColWidth, y, gradeColWidth, gradeRowHeight);
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, 230, 250)), rect);
                e.Graphics.DrawRectangle(Pens.Black, rect);
                e.Graphics.DrawString(gradeHeaders[colIndex], subHeaderFont, brush, rect, centerFormat);
            }

            y += gradeRowHeight;

            double sumPoints = 0;
            int sumUnits = 0;
            int completedUnits = 0;

            foreach (DataRow row in dt.Rows)
            {
                string subject = row["المادة"].ToString();
                string code = row["رقم_المادة"].ToString();
                int grade = Convert.ToInt32(row["الدرجة"]);
                int units = Convert.ToInt32(row["الوحدات"]);
                int points = grade * units;
                string result = grade >= 50 ? "ناجح" : "راسب";
                string note = "";

                sumPoints += points;
                sumUnits += units;
                if (grade >= 50) completedUnits += units;

                string[] values = { subject, code, grade.ToString(), units.ToString(), points.ToString(), result, note };

                for (int i = 0; i < values.Length; i++)
                {
                    int colIndex = values.Length - 1 - i; // لعكس الاتجاه
                    Rectangle rect = new Rectangle(x + i * gradeColWidth, y, gradeColWidth, gradeRowHeight);
                    e.Graphics.DrawRectangle(Pens.Black, rect);
                    e.Graphics.DrawString(values[colIndex], textFont, brush, rect, centerFormat);
                }

                y += gradeRowHeight;
            }

            y += 20;

            // جدول الملخص
            double totalPoints = 0;
            int totalUnits = 0;

            foreach (DataTable page in pages)
            {
                foreach (DataRow row in page.Rows)
                {
                    int g = Convert.ToInt32(row["الدرجة"]);
                    int u = Convert.ToInt32(row["الوحدات"]);
                    totalPoints += g * u;
                    totalUnits += u;
                }
            }

            double semesterGPA = sumUnits == 0 ? 0 : sumPoints / sumUnits;
            double cumulativeGPA = totalUnits == 0 ? 0 : totalPoints / totalUnits;

            string[] summaryHeaders = currentPageIndex == pages.Count - 1
                ? new string[] { "الوحدات المسجلة", "الوحدات المنجزة", "إجمالي النقاط", "المعدل السنوي", "المعدل التراكمي" }
                : new string[] { "الوحدات المسجلة", "الوحدات المنجزة", "إجمالي النقاط", "المعدل السنوي" };

            string[] summaryValues = currentPageIndex == pages.Count - 1
                ? new string[] { sumUnits.ToString(), completedUnits.ToString(), ((int)sumPoints).ToString(), semesterGPA.ToString("F2"), cumulativeGPA.ToString("F2") }
                : new string[] { sumUnits.ToString(), completedUnits.ToString(), ((int)sumPoints).ToString(), semesterGPA.ToString("F2") };

            int summaryColCount = summaryHeaders.Length;
            int summaryColWidth = pageWidth / summaryColCount;

            for (int i = 0; i < summaryColCount; i++)
            {
                int colIndex = summaryColCount - 1 - i;
                Rectangle rectHeader = new Rectangle(x + i * summaryColWidth, y, summaryColWidth, gradeRowHeight);
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, 230, 250)), rectHeader);
                e.Graphics.DrawRectangle(Pens.Black, rectHeader);
                e.Graphics.DrawString(summaryHeaders[colIndex], subHeaderFont, brush, rectHeader, centerFormat);

                Rectangle rectValue = new Rectangle(x + i * summaryColWidth, y + gradeRowHeight, summaryColWidth, gradeRowHeight);
                e.Graphics.DrawRectangle(Pens.Black, rectValue);
                e.Graphics.DrawString(summaryValues[colIndex], textFont, brush, rectValue, centerFormat);
            }

            // **اجعل التوقيعات في أسفل الصفحة مهما كان المحتوى**

            int signHeight = 50;
            int signY = pageHeight - margin - signHeight; // مكان التوقيعات في أسفل الصفحة

            string[] signatures = { "عميد الكلية", "قسم الدراسة والامتحانات", "القسم العلمي" };
            int signCount = signatures.Length;
            int signColWidth = pageWidth / signCount;

            Pen dottedPen = new Pen(Color.Black);
            dottedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

            for (int i = 0; i < signCount; i++)
            {
                int posX = x + i * signColWidth;
                // نص التوقيع فوق الخط
                Rectangle rectSignText = new Rectangle(posX, signY, signColWidth, gradeRowHeight);
                e.Graphics.DrawString(signatures[i], textFont, brush, rectSignText, centerFormat);

                // خط التوقيع المنقط تحت النص
                int lineY = signY + gradeRowHeight + 5;
                e.Graphics.DrawLine(dottedPen, posX + 10, lineY, posX + signColWidth - 10, lineY);
            }

            currentPageIndex++;
            e.HasMorePages = currentPageIndex < pages.Count;
        }
        private void PrepareStudentReportPages1(DataTable dt)
        {
            pages.Clear();
            pageSummaries.Clear();

            var groupedByYear = dt.AsEnumerable().GroupBy(r => r.Field<int>("السنة"));

            foreach (var group in groupedByYear)
            {
                DataTable page = dt.Clone();
                foreach (var row in group)
                {
                    page.ImportRow(row);
                }

                pages.Add(page);
            }

            if (dt.Rows.Count > 0)
            {
                studentName = dt.Rows[0]["اسم_الطالب"].ToString();
                universityNumber = dt.Rows[0]["الرقم_الجامعي"].ToString();
            }

            currentPageIndex = 0;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            string uniNumber = txtUniversityNumber3.Text.Trim();
            if (string.IsNullOrEmpty(uniNumber))
            {
                MessageBox.Show("يرجى إدخال الرقم الجامعي.");
                return;
            }

            string query = @"
    SELECT 
    s.full_name AS اسم_الطالب,
    s.university_number AS الرقم_الجامعي,
    c.year_number AS السنة,
    c.course_id AS رقم_المادة,
    c.course_name AS المادة,
    c.units AS الوحدات,
    g.total_grade AS الدرجة
FROM Grades g
INNER JOIN Students s ON g.student_id = s.student_id
INNER JOIN Courses c ON g.course_id = c.course_id
WHERE s.university_number = @university_number
ORDER BY c.year_number, c.course_name;";

            using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@university_number", uniNumber);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                reportData = new DataTable();
                da.Fill(reportData);
            }

            if (reportData.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات لهذا الرقم الجامعي.");
                return;
            }

            dataGridView1.DataSource = reportData;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // حساب المعدلات
            CalculateAndDisplayAverages3(reportData);
        }

        private void CalculateAndDisplayAverages3(DataTable dt)
        {
            var groupedByYear = dt.AsEnumerable()
           .GroupBy(r => r.Field<int>("السنة"));

            double totalWeightedGrades = 0;
            int totalUnits = 0;

            string averagesText = "";

            foreach (var yearGroup in groupedByYear)
            {
                int year = yearGroup.Key;
                double sumWeightedGrades = 0;
                int sumUnits = 0;

                foreach (var row in yearGroup)
                {
                    int grade = 0;
                    int units = 0;

                    // قراءة آمنة دون رمي استثناء
                    if (row["الدرجة"] != DBNull.Value && row["الوحدات"] != DBNull.Value)
                    {
                        try
                        {
                            grade = Convert.ToInt32(row["الدرجة"]);
                            units = Convert.ToInt32(row["الوحدات"]);
                        }
                        catch
                        {
                            // تجاهل الصف إذا كان التحويل غير ممكن
                            continue;
                        }

                        sumWeightedGrades += grade * units;
                        sumUnits += units;
                    }
                }

                double yearAverage = sumUnits == 0 ? 0 : sumWeightedGrades / sumUnits;
                averagesText += $"معدل السنة {year}: {yearAverage:F2}\n";

                totalWeightedGrades += sumWeightedGrades;
                totalUnits += sumUnits;
            }

            double cumulativeAverage = totalUnits == 0 ? 0 : totalWeightedGrades / totalUnits;
            averagesText += $"المعدل التراكمي: {cumulativeAverage:F2}";

           
        }



        private void button3_Click(object sender, EventArgs e)
        {
            if (reportData == null || reportData.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات للطباعة.");
                return;
            }

            PrepareStudentReportPages1(reportData);
            currentPageIndex = 0;

            //// ✅ استخدم printDocument2 بدلًا من printDocument1
            //PrintPreviewDialog preview = new PrintPreviewDialog();
            //preview.Document = printDocument2;
            //preview.ShowDialog();

            // أو لطباعة مباشرة:
            printDocument3.Print();
        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        //**************************************************************************************************************
        //طبته
        private void button6_Click(object sender, EventArgs e)
        {

            selectedYear = comboBox_Year.SelectedItem?.ToString();
            if (!int.TryParse(selectedYear, out int yearNumber))
            {
                MessageBox.Show("السنة غير صالحة.");
                return;
            }

            if (comboBox_Department.SelectedValue == null)
            {
                MessageBox.Show("يرجى اختيار القسم.");
                return;
            }

            int departmentId = (int)comboBox_Department.SelectedValue; // استخدم ID

            subjectPages.Clear();
            currentPrintIndex = 0;
            currentRowIndex = 0;  // تهيئة مؤشر الصف للطباعة
            // تعديل رقم المجموعه
            string query = @"
        SELECT 
            s.full_name AS اسم_الطالب,
            s.university_number AS الرقم_الجامعي,
            c.course_name AS المادة,
            c.course_id AS رمز_المادة,
            c.units AS الوحدات,
            g.total_grade AS الدرجة,
            d.dep_name AS القسم,
            i.full_name AS الأستاذ
        FROM Grades g
        INNER JOIN Students s ON g.student_id = s.student_id
        INNER JOIN Courses c ON g.course_id = c.course_id
        INNER JOIN Course_Department cd ON cd.course_id = c.course_id
        INNER JOIN Departments d ON d.department_id = cd.department_id
        LEFT JOIN Instructors i ON i.instructor_id = d.head_id
        WHERE c.year_number = @year AND cd.department_id = @deptId
        ORDER BY c.course_name, s.full_name";

            using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@year", yearNumber);
                cmd.Parameters.AddWithValue("@deptId", departmentId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable allData = new DataTable();
                da.Fill(allData);

                var groupedSubjects = allData.AsEnumerable()
                    .GroupBy(r => r["المادة"].ToString() + "|" + r["رمز_المادة"].ToString());

                foreach (var group in groupedSubjects)
                {
                    var distinctRows = group
                        .GroupBy(r => new
                        {
                            StudentName = r["اسم_الطالب"].ToString(),
                            UniversityNumber = r["الرقم_الجامعي"].ToString()
                        })
                        .Select(g => g.First());

                    DataTable dt = allData.Clone();
                    foreach (var row in distinctRows)
                        dt.ImportRow(row);
                    subjectPages.Add(dt);
                }

            }

            if (subjectPages.Count == 0)
            {
                MessageBox.Show("لا توجد نتائج للعرض.");
                return;
            }

            // ربط حدث تهيئة الطباعة إن لم يكن مرتبطاً مسبقاً


            PrintPreviewDialog previewDialog = new PrintPreviewDialog();
            previewDialog.Document = printDocument12;
            previewDialog.WindowState = FormWindowState.Maximized;
            previewDialog.ShowDialog();

            // بدلاً من المعاينة، يمكن استخدام هذا للطباعة مباشرة:
            // printDocument12.Print();
        }




        private void FillDepartmentComboBox()
        {
            string query = "SELECT department_id, dep_name FROM Departments";

            using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                Dictionary<int, string> departmentDict = new Dictionary<int, string>();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    departmentDict.Add(id, name);
                }

                comboBox_Department.DataSource = new BindingSource(departmentDict, null);
                comboBox_Department.DisplayMember = "Value"; // اسم القسم للعرض
                comboBox_Department.ValueMember = "Key";     // المعرف للتعامل الداخلي
            }
        }

        private int currentPageNumber = 1;   // رقم الصفحة العام

        private void printDocument12_PrintPage(object sender, PrintPageEventArgs e)
        {
            // تحقق من انتهاء كل المواد
            if (currentPrintIndex >= subjectPages.Count)
            {
                e.HasMorePages = false;
                currentRowIndex = 0;
                currentPrintIndex = 0;
                currentPageNumber = 1;
                return;
            }

            DataTable dt = subjectPages[currentPrintIndex];

            // إذا المادة فارغة انتقل للمادة التالية
            if (dt.Rows.Count == 0)
            {
                currentPrintIndex++;
                currentRowIndex = 0;
                e.HasMorePages = currentPrintIndex < subjectPages.Count;
                return;
            }

            // الخطوط والفرشات
            Font headerFont = new Font("Arial", 16, FontStyle.Bold);
            Font subHeaderFont = new Font("Arial", 12, FontStyle.Bold);
            Font textFont = new Font("Arial", 11, FontStyle.Bold);
            Brush brush = Brushes.Black;

            int extraWidth = 50;
            int usableWidth = e.MarginBounds.Width + extraWidth;
            int leftMargin = e.MarginBounds.Left - (extraWidth / 2);
            int topMargin = 50;
            int lineHeight = (int)textFont.GetHeight(e.Graphics) + 8;

            StringFormat rightAlign = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center
            };
            StringFormat centerAlign = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            // رسم الرأس (عنوان الكلية، المادة، القسم، الأستاذ، السنة الدراسية)
            string courseName = dt.Rows[0]["المادة"].ToString();
            string departmentName = dt.Rows[0]["القسم"]?.ToString() ?? "";
            string teacherName = dt.Rows[0]["الأستاذ"]?.ToString() ?? "";

            int currentYear = DateTime.Now.Year;
            int previousYear = DateTime.Now.Month >= 7 ? currentYear : currentYear - 1;
            int nextYear = previousYear + 1;
            string academicYear = $"{previousYear}-{nextYear}";

            e.Graphics.DrawString("كلية العلوم الصحية", headerFont, brush, leftMargin + usableWidth / 2, topMargin, centerAlign);
            e.Graphics.DrawString("نتائج المواد", subHeaderFont, brush, leftMargin + usableWidth / 2, topMargin + 40, centerAlign);
            e.Graphics.DrawString($"السنة الدراسية: {academicYear}", subHeaderFont, brush, leftMargin + usableWidth / 2, topMargin + 70, centerAlign);

            int headerY = topMargin + 110;
            int sectionWidth = usableWidth / 3;

            Rectangle rectSubject = new Rectangle(leftMargin + 2 * sectionWidth, headerY, sectionWidth, lineHeight);
            Rectangle rectDepartment = new Rectangle(leftMargin + sectionWidth, headerY, sectionWidth, lineHeight);
            Rectangle rectTeacher = new Rectangle(leftMargin, headerY, sectionWidth, lineHeight);

            using (Pen thickPen = new Pen(Color.Black, 3))
            {
                e.Graphics.DrawRectangle(thickPen, rectSubject);
                e.Graphics.DrawRectangle(thickPen, rectDepartment);
                e.Graphics.DrawRectangle(thickPen, rectTeacher);
            }

            e.Graphics.DrawString($"المادة: {courseName}", textFont, brush, rectSubject, rightAlign);
            e.Graphics.DrawString($"القسم: {departmentName}", textFont, brush, rectDepartment, rightAlign);
            e.Graphics.DrawString($"الأستاذ: {teacherName}", textFont, brush, rectTeacher, rightAlign);

            // أعمدة الجدول
            int numberingColumnWidth = 50;
            int studentNameWidth = (int)(usableWidth * 0.4) - numberingColumnWidth;

            int[] columnWidths = {
        numberingColumnWidth,           // ر.م
        studentNameWidth,               // اسم الطالب
        (int)(usableWidth * 0.25),     // الرقم الجامعي
        (int)(usableWidth * 0.20),     // الدرجة
        (int)(usableWidth * 0.15)      // الحالة
    };

            string[] headers = { "ر.م", "اسم الطالب", "الرقم الجامعي", "الدرجة", "الحالة" };

            int totalWidth = columnWidths.Sum();
            int tableRight = leftMargin + totalWidth;

            int y = headerY + lineHeight + 20;
            int colX = tableRight - columnWidths[0];

            // رسم رؤوس الأعمدة
            using (Pen thickPen = new Pen(Color.Black, 3))
            {
                for (int i = 0; i < headers.Length; i++)
                {
                    Rectangle rect = new Rectangle(colX, y, columnWidths[i], lineHeight);
                    e.Graphics.DrawRectangle(thickPen, rect);
                    e.Graphics.DrawString(headers[i], textFont, brush, rect, rightAlign);
                    if (i + 1 < headers.Length)
                        colX -= columnWidths[i + 1];
                }
            }

            y += lineHeight;

            int studentNumbering = currentRowIndex + 1;

            // طباعة الصفوف من currentRowIndex وحتى نهاية الجدول أو امتلاء الصفحة
            while (currentRowIndex < dt.Rows.Count)
            {
                if (y + lineHeight > e.MarginBounds.Bottom - 40)
                {
                    // انتهت مساحة الصفحة، إرجع HasMorePages = true للطباعة في صفحة جديدة
                    e.HasMorePages = true;
                    return;
                }

                DataRow row = dt.Rows[currentRowIndex];

                colX = tableRight - columnWidths[0];

                string numberingStr = studentNumbering.ToString();
                string studentName = row["اسم_الطالب"].ToString();
                string studentNumber = row["الرقم_الجامعي"].ToString();
                string grade = row["الدرجة"].ToString();

                string status = "غير معروف";
                int gradeValue = 0;
                if (int.TryParse(grade, out gradeValue))
                {
                    status = gradeValue > 50 ? "ناجح" : "راسب";
                }

                string[] values = { numberingStr, studentName, studentNumber, grade, status };

                using (Pen thickPen = new Pen(Color.Black, 3))
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        Rectangle rect = new Rectangle(colX, y, columnWidths[i], lineHeight);
                        e.Graphics.DrawRectangle(thickPen, rect);
                        e.Graphics.DrawString(values[i], textFont, brush, rect, rightAlign);
                        if (i + 1 < values.Length)
                            colX -= columnWidths[i + 1];
                    }
                }

                y += lineHeight;
                currentRowIndex++;
                studentNumbering++;
            }

            // رسم رقم الصفحة بوضوح وبخط أكبر وأسفل الصفحة في المنتصف
            using (Font pageFont = new Font("Tahoma", 12, FontStyle.Bold))
            {
                string pageText = $"الصفحة {currentPageNumber}";
                RectangleF pageRect = new RectangleF(
                    e.MarginBounds.Left,
                    e.MarginBounds.Bottom + 10,
                    e.MarginBounds.Width,
                    pageFont.Height);

                StringFormat pageFormat = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                e.Graphics.DrawString(pageText, pageFont, Brushes.Black, pageRect, pageFormat);
            }

            // انتهينا من طباعة هذه المادة بالكامل
            currentPrintIndex++;
            currentRowIndex = 0;

            // زيادة رقم الصفحة
            currentPageNumber++;

            // تحديد استمرار الطباعة
            e.HasMorePages = currentPrintIndex < subjectPages.Count;

            // لو انتهت الطباعة، إعادة تعيين المؤشرات للطباعة من جديد في المرة القادمة (اختياري)
            if (!e.HasMorePages)
            {
                currentPrintIndex = 0;
                currentRowIndex = 0;
                currentPageNumber = 1;
            }
        }
    }
}