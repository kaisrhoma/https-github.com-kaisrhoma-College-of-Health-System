using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
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
        private int currentRowIndex = 0;

        private PrintDocument printDocument12 = new PrintDocument();


        public statements_reports()
        {
            InitializeComponent();
            comboBox_Year2.Items.Add("1");
            comboBox_Year2.Items.Add("2");
            comboBox_Year2.Items.Add("3");
            comboBox_Year2.Items.Add("4");

            numericUpDownYear.Minimum = 1990;    // أقل سنة مسموح بها
            numericUpDownYear.Maximum = 2100;    // أعلى سنة مسموح بها
            numericUpDownYear.Value = DateTime.Now.Year;      // القيمة الافتراضية (مثلاً)
            numericUpDownYear.Increment = 1;     // خطوة الزيادة/النقصان سنة واحدة
            numericUpDownYear.ThousandsSeparator = false; // حسب رغبتك
            int startYear = (int)numericUpDownYear.Value;
            string academicYear = $"{startYear}-{startYear + 1}";



            comboBox_Year2.SelectedIndex = 0;
            dataGridViewGrades.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            printDocument1.PrintPage += printDocument1_PrintPage;
            printDocument2.PrintPage += printDocument2_PrintPage;
            printDocument3.PrintPage += printDocument3_PrintPage;
            printDocument12.PrintPage += printDocument12_PrintPage;

            printDocument1.BeginPrint += BeginPrint_Reset;
            printDocument2.BeginPrint += BeginPrint_Reset;
            printDocument3.BeginPrint += BeginPrint_Reset;
            printDocument12.BeginPrint += BeginPrint_Reset;
            //3
            comboBox_Year.Items.AddRange(new object[] { 1, 2, 3, 4 });

            comboBox_Year.SelectedIndex = 0;
            FillDepartmentComboBox();
            printDocument12.PrintPage += printDocument12_PrintPage;
            numericUpDownYear1.Minimum = 1990;    // أقل سنة مسموح بها
            numericUpDownYear1.Maximum = 2100;    // أعلى سنة مسموح بها
            numericUpDownYear1.Value = DateTime.Now.Year;      // القيمة الافتراضية (مثلاً)
            numericUpDownYear1.Increment = 1;     // خطوة الزيادة/النقصان سنة واحدة
            numericUpDownYear1.ThousandsSeparator = false; // حسب رغبتك
            int startYear1 = (int)numericUpDownYear1.Value;




        }
        private int currentPageIndex = 0;
        private List<DataTable> pages = new List<DataTable>();

        private void BeginPrint_Reset(object sender, PrintEventArgs e)
        {
            currentPageIndex = 0;
            currentRowIndex = 0;
        }
        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (currentPageIndex >= pages.Count)
            {
                e.HasMorePages = false;
                currentPageIndex = 0;
                currentRowIndex = 0;
                return;
            }

            DataTable dt = pages[currentPageIndex];

            string departmentName = dt.ExtendedProperties.Contains("Department") ? dt.ExtendedProperties["Department"]?.ToString() : string.Empty;
            string yearName = dt.ExtendedProperties.Contains("Year") ? dt.ExtendedProperties["Year"]?.ToString() : string.Empty;

            Font headerFont = new Font("Arial", 18, FontStyle.Bold);
            Font tableHeaderFont = new Font("Arial", 12, FontStyle.Bold);
            Font tableFont = new Font("Arial", 10);
            Brush brush = Brushes.Black;

            int xRight = e.MarginBounds.Right; // بداية من اليمين
            int y = 50;
            int defaultRowHeight = 28;

            // عنوان الصفحة
            e.Graphics.DrawString("كلية العلوم الصحية", headerFont, brush,
                e.MarginBounds.Left + e.MarginBounds.Width / 2, y,
                new StringFormat { Alignment = StringAlignment.Center });
            y += 30;

            e.Graphics.DrawString($"القسم: {departmentName}", headerFont, brush,
                e.MarginBounds.Left + e.MarginBounds.Width / 2, y,
                new StringFormat { Alignment = StringAlignment.Center });
            y += 25;

            e.Graphics.DrawString($"السنة الدراسية: {yearName}", headerFont, brush,
                e.MarginBounds.Left + e.MarginBounds.Width / 2, y,
                new StringFormat { Alignment = StringAlignment.Center });
            y += 30;

            e.Graphics.DrawString($"التاريخ: {DateTime.Now:yyyy/MM/dd}", tableHeaderFont, brush,
                e.MarginBounds.Left + e.MarginBounds.Width / 8, y,
                new StringFormat { Alignment = StringAlignment.Center });
            y += 40;

            // تحديد عرض الأعمدة
            int fixedColCount = 4; // رقم، اسم الطالب، الرقم الجامعي، النتيجة
            int[] colWidths = new int[dt.Columns.Count];

            colWidths[0] = 30;   // رقم
            colWidths[1] = 150;  // اسم الطالب
            colWidths[2] = 100;  // الرقم الجامعي
            colWidths[dt.Columns.Count - 1] = 70; // النتيجة

            int availableWidth = e.MarginBounds.Width - (colWidths[0] + colWidths[1] + colWidths[2] + colWidths[dt.Columns.Count - 1]);
            int subjectsCount = dt.Columns.Count - fixedColCount;
            int subjectColWidth = subjectsCount > 0 ? Math.Max(30, availableWidth / subjectsCount) : 0;

            for (int i = 3; i < dt.Columns.Count - 1; i++)
                colWidths[i] = subjectColWidth;

            // حساب ارتفاع رأس الجدول بسبب تدوير أسماء المواد
            int headerRowHeight = defaultRowHeight;
            for (int i = 3; i < dt.Columns.Count - 1; i++)
            {
                SizeF size = e.Graphics.MeasureString(dt.Columns[i].ColumnName, tableHeaderFont);
                headerRowHeight = Math.Max(headerRowHeight, (int)Math.Ceiling(size.Width) + 10);
            }

            // رسم رأس الجدول
            int colX = xRight;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                colX -= colWidths[i];

                // حماية من قيم سالبة أو صفرية
                int rectWidth = Math.Max(1, colWidths[i]);
                int rectHeight = Math.Max(1, headerRowHeight);

                Rectangle rect = new Rectangle(colX, y, rectWidth, rectHeight);
                e.Graphics.FillRectangle(Brushes.LightGray, rect);
                e.Graphics.DrawRectangle(Pens.Black, rect);

                StringFormat formatCenter = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                if (i >= 3 && i < dt.Columns.Count - 1)
                {
                    GraphicsState state = e.Graphics.Save();
                    e.Graphics.TranslateTransform(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
                    e.Graphics.RotateTransform(-90);
                    Rectangle textRect = new Rectangle(-rect.Height / 2, -rect.Width / 2, rect.Height, rect.Width);
                    e.Graphics.DrawString(dt.Columns[i].ColumnName, tableHeaderFont, brush, textRect, formatCenter);
                    e.Graphics.Restore(state);
                }
                else
                {
                    e.Graphics.DrawString(dt.Columns[i].ColumnName, tableHeaderFont, brush, rect, formatCenter);
                }
            }
            y += headerRowHeight;

            // صفوف الطلاب
            bool isAlternate = false;
            while (currentRowIndex < dt.Rows.Count)
            {
                int rowHeight = defaultRowHeight;

                if (y + rowHeight > e.MarginBounds.Bottom)
                {
                    // 🔹 رسم الترقيم قبل الانتقال للصفحة التالية
                    string pageNumber = $"صفحة {currentPageIndex + 1} من {pages.Count}";
                    Font footerFont = new Font("Arial", 10, FontStyle.Bold);
                    SizeF footerSize = e.Graphics.MeasureString(pageNumber, footerFont);
                    e.Graphics.DrawString(pageNumber, footerFont, Brushes.Black,
                        e.PageBounds.Width / 2 - footerSize.Width / 2, e.MarginBounds.Bottom + 30);

                    e.HasMorePages = true;
                    return;
                }

                colX = xRight;
                Brush rowBackBrush = isAlternate ? new SolidBrush(Color.FromArgb(235, 241, 255)) : Brushes.White;
                isAlternate = !isAlternate;

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    colX -= colWidths[i];
                    int rectWidth = Math.Max(1, colWidths[i]);
                    int rectHeight = Math.Max(1, rowHeight);
                    Rectangle rect = new Rectangle(colX, y, rectWidth, rectHeight);

                    e.Graphics.FillRectangle(rowBackBrush, rect);
                    e.Graphics.DrawRectangle(Pens.Black, rect);

                    StringFormat format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };

                    string cellText = dt.Rows[currentRowIndex][i]?.ToString() ?? "-";
                    e.Graphics.DrawString(cellText, tableFont, brush, rect, format);
                }

                y += rowHeight;
                currentRowIndex++;
            }

            // 🔹 رسم الترقيم أسفل الصفحة في المنتصف
            string finalPageNumber = $"صفحة {currentPageIndex + 1} من {pages.Count}";
            Font finalFooterFont = new Font("Arial", 10, FontStyle.Bold);
            SizeF finalFooterSize = e.Graphics.MeasureString(finalPageNumber, finalFooterFont);
            e.Graphics.DrawString(finalPageNumber, finalFooterFont, Brushes.Black,
                e.PageBounds.Width / 2 - finalFooterSize.Width / 2, e.MarginBounds.Bottom + 30);

            currentRowIndex = 0;
            currentPageIndex++;
            e.HasMorePages = currentPageIndex < pages.Count;
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

            int selectedYearNumber = Convert.ToInt32(comboBox_Year2.SelectedItem); // السنة الدراسية (1,2,3,4)
            int startAcademicYear = (int)numericUpDownYear.Value; // السنة الحالية
            int academicYearStart = startAcademicYear; // يمكن تعديل الحساب إذا احتجنا لصيغة محددة

            string query = @"
SELECT 
    c.course_name AS 'اسم المادة',
    c.course_code AS 'رمز المادة',
    c.year_number AS 'السنة الدراسية',
    cc.group_number AS 'رقم المجموعة',
    i.full_name AS 'اسم الأستاذ',
    s.full_name AS 'اسم الطالب',
    s.university_number AS 'الرقم الجامعي',
    d.dep_name AS 'القسم',
    g.total_grade AS 'الدرجة',
    g.success_status AS 'النتيجة'
FROM Grades g
INNER JOIN Students s ON g.student_id = s.student_id
INNER JOIN Registrations r ON r.student_id = s.student_id AND r.course_id = g.course_id
INNER JOIN Course_Classroom cc ON r.course_classroom_id = cc.id
INNER JOIN Courses c ON cc.course_id = c.course_id
INNER JOIN Departments d ON s.department_id = d.department_id
LEFT JOIN Course_Instructor ci ON c.course_id = ci.course_id
LEFT JOIN Instructors i ON ci.instructor_id = i.instructor_id
WHERE r.academic_year_start = @academic_year
  AND c.year_number = @year_number
ORDER BY d.dep_name, c.year_number, s.university_number, c.course_id;
";

            using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@academic_year", academicYearStart);
                cmd.Parameters.AddWithValue("@year_number", selectedYearNumber);

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("لا يوجد طلاب لهذه السنة الدراسية أو لا توجد درجات مسجلة.");
                    dataGridViewGrades.DataSource = null;
                    return;
                }

                dataGridViewGrades.DataSource = dt;
                dataGridViewGrades.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                reportData = dt;
            }
        }






        private void PreparePagesByDepartmentAndYear(DataTable data)
        {
            pages.Clear();

            var groupedByDept = data.AsEnumerable()
                .GroupBy(r => r["القسم"].ToString());

            foreach (var deptGroup in groupedByDept)
            {
                var groupedByYear = deptGroup.GroupBy(r => r["السنة الدراسية"].ToString());

                foreach (var yearGroup in groupedByYear)
                {
                    var courses = yearGroup
                        .Select(r => r["اسم المادة"].ToString())
                        .Distinct()
                        .OrderByDescending(c => c) // من اليمين إلى اليسار
                        .ToList();

                    // إنشاء الجدول
                    DataTable table = new DataTable();
                    table.Columns.Add("رقم", typeof(int));
                    table.Columns.Add("اسم الطالب", typeof(string));
                    table.Columns.Add("الرقم الجامعي", typeof(string));

                    foreach (var course in courses)
                        table.Columns.Add(course, typeof(string));

                    table.Columns.Add("النتيجة", typeof(string));

                    var students = yearGroup
                        .Select(r => new
                        {
                            Name = r["اسم الطالب"].ToString(),
                            UniNum = r["الرقم الجامعي"].ToString()
                        })
                        .Distinct()
                        .OrderBy(s => s.UniNum)
                        .ToList();

                    int counter = 1;
                    foreach (var student in students)
                    {
                        DataRow newRow = table.NewRow();
                        newRow["رقم"] = counter++;
                        newRow["اسم الطالب"] = student.Name;
                        newRow["الرقم الجامعي"] = student.UniNum;

                        bool isFail = false;
                        foreach (var course in courses)
                        {
                            var gradeStr = yearGroup
                                .FirstOrDefault(r =>
                                    r["اسم الطالب"].ToString() == student.Name &&
                                    r["اسم المادة"].ToString() == course)?["الدرجة"]?.ToString();

                            if (int.TryParse(gradeStr, out int grade))
                            {
                                newRow[course] = grade;
                                if (grade < 60) isFail = true; // أقل من 60 رسوب
                            }
                            else
                            {
                                newRow[course] = "-";
                                isFail = true;
                            }
                        }

                        newRow["النتيجة"] = isFail ? "راسب" : "ناجح";
                        table.Rows.Add(newRow);
                    }

                    // حفظ بيانات القسم والسنة
                    table.ExtendedProperties["Department"] = deptGroup.Key;
                    table.ExtendedProperties["Year"] = yearGroup.Key;

                    pages.Add(table);
                }
            }

            currentPageIndex = 0;
            currentRowIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dataGridViewGrades.DataSource;

            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات للطباعة.");
                return;
            }

            PreparePagesByDepartmentAndYear(dt); // تجهيز الصفحات
            currentPageIndex = 0;
            currentRowIndex = 0;

            PrintPreviewDialog preview = new PrintPreviewDialog();
            preview.Document = printDocument1;
            preview.ShowDialog();

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
    c.course_code AS رمز_المادة,
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
            string[] gradeHeaders = { " رمز المادة", "المادة", " عدد الوحدات", "عدد النقاط", "الدرجة", "نتيجة المادة", "ملاحظة" };

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
                string code = row["رمز_المادة"].ToString();
                string subject = row["المادة"].ToString();
                int units = row["الوحدات"] != DBNull.Value ? Convert.ToInt32(row["الوحدات"]) : 0;
                int grade = row["الدرجة"] != DBNull.Value ? Convert.ToInt32(row["الدرجة"]) : 0;


                int points = grade * units;
                string result = grade >= 60 ? "ناجح" : "راسب";
                string note = "";

                sumPoints += points;
                sumUnits += units;
                if (grade >= 60) completedUnits += units;

                string[] values = { code, subject, units.ToString(), points.ToString(), grade.ToString(), result, note };

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
                    int g = row["الدرجة"] != DBNull.Value ? Convert.ToInt32(row["الدرجة"]) : 0;
                    int u = row["الوحدات"] != DBNull.Value ? Convert.ToInt32(row["الوحدات"]) : 0;

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
            string[] gradeHeaders = { " رمز المادة", "المادة", " عدد الوحدات", "عدد النقاط", "الدرجة", "نتيجة المادة", "ملاحظة" };
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
                string code = row["رمز_المادة"].ToString();
                int units = row["الوحدات"] != DBNull.Value ? Convert.ToInt32(row["الوحدات"]) : 0;
                int grade = row["الدرجة"] != DBNull.Value ? Convert.ToInt32(row["الدرجة"]) : 0;

                int points = grade * units;
                string result = grade >= 60 ? "ناجح" : "راسب";
                string note = "";

                sumPoints += points;
                sumUnits += units;
                if (grade >= 60) completedUnits += units;

                string[] values = { code, subject, units.ToString(), points.ToString(), grade.ToString(), result, note };

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
                    int g = row["الدرجة"] != DBNull.Value ? Convert.ToInt32(row["الدرجة"]) : 0;
                    int u = row["الوحدات"] != DBNull.Value ? Convert.ToInt32(row["الوحدات"]) : 0;

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

            string[] signatures = { "قسم الدراسة والامتحانات", "المسجل العام" };
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
                int lineY = signY + gradeRowHeight + 20;
                e.Graphics.DrawLine(dottedPen, posX + 5, lineY, posX + signColWidth - 10, lineY);
            }

            // ===== إضافة ترقيم الصفحة =====
            string pageNumberText = $"صفحة {currentPageIndex + 1}";
            Rectangle pageNumberRect = new Rectangle(x, pageHeight - margin / 2, pageWidth, 20);
            e.Graphics.DrawString(pageNumberText, textFont, brush, pageNumberRect, centerFormat);

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
    c.course_code AS رمز_المادة,
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
            PrintPreviewDialog preview = new PrintPreviewDialog();
            preview.Document = printDocument3;
            preview.ShowDialog();
        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        //**************************************************************************************************************
        //طبته
        private void button6_Click(object sender, EventArgs e)
        {
            string selectedYear = comboBox_Year.SelectedItem?.ToString();
            if (!int.TryParse(selectedYear, out int yearNumber))
            {
                MessageBox.Show("يرجى اختيار السنة الدراسية صحيحة.");
                return;
            }

            if (comboBox_Department.SelectedValue == null)
            {
                MessageBox.Show("يرجى اختيار القسم.");
                return;
            }

            int departmentId = (int)comboBox_Department.SelectedValue;
            int academicYear = (int)numericUpDownYear1.Value;

            departmentName = comboBox_Department.Text;
            yearName = comboBox_Year.Text;

            // قراءة كل البيانات من قاعدة البيانات
            string query = @"
 SELECT 
     s.full_name AS [اسم الطالب],
     s.university_number AS [الرقم الجامعي],
     c.course_name AS [المادة],
     g.total_grade AS [الدرجة]
 FROM Grades g
 INNER JOIN Students s ON g.student_id = s.student_id
 INNER JOIN Courses c ON g.course_id = c.course_id
 INNER JOIN Course_Department cd ON cd.course_id = c.course_id
 INNER JOIN Registrations r ON r.student_id = s.student_id AND r.course_id = g.course_id
 WHERE c.year_number = @yearNumber
   AND cd.department_id = @deptId
   AND r.academic_year_start = @academicYear
 ORDER BY s.full_name, c.course_name";

            DataTable allData = new DataTable();
            using (SqlConnection conn = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;"))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@yearNumber", yearNumber);
                cmd.Parameters.AddWithValue("@deptId", departmentId);
                cmd.Parameters.AddWithValue("@academicYear", academicYear);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(allData);
            }

            if (allData.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد نتائج للعرض للسنة الدراسية والعام الدراسي والقسم المختارين.");
                return;
            }

            // إنشاء جدول للطباعة
            DataTable tableForPrinting = new DataTable();
            tableForPrinting.Columns.Add("اسم الطالب");
            tableForPrinting.Columns.Add("الرقم الجامعي");

            // جمع المواد
            var subjects = allData.AsEnumerable()
                .Select(r => r["المادة"].ToString())
                .Distinct()
                .ToList();

            foreach (var subject in subjects)
                tableForPrinting.Columns.Add(subject);

            // إضافة عمود النتيجة
            tableForPrinting.Columns.Add("النتيجة");

            // ترتيب الطلاب حسب الرقم الجامعي
            var students = allData.AsEnumerable()
                .GroupBy(r => r.Field<string>("الرقم الجامعي"));

            foreach (var studentGroup in students)
            {
                DataRow row = tableForPrinting.NewRow();
                var first = studentGroup.First();
                row["اسم الطالب"] = first["اسم الطالب"];
                row["الرقم الجامعي"] = first["الرقم الجامعي"];

                bool allPassed = true; // نفترض أنه ناجح

                foreach (var s in subjects)
                {
                    var gradeRow = studentGroup.FirstOrDefault(r => r.Field<string>("المادة") == s);
                    string gradeText = gradeRow != null ? gradeRow["الدرجة"].ToString() : string.Empty;

                    // ضع الدرجة في الجدول
                    row[s] = string.IsNullOrEmpty(gradeText) ? "-" : gradeText;

                    // تحقق النجاح
                    if (!int.TryParse(gradeText, out int grade) || grade < 60)
                    {
                        allPassed = false; // إذا في مادة أقل من 60 أو بدون درجة
                    }
                }

                // النتيجة النهائية
                row["النتيجة"] = allPassed ? "ناجح" : "راسب";

                tableForPrinting.Rows.Add(row);
            }
            // تحضير الطباعة
            subjectPages.Clear();
            subjectPages.Add(tableForPrinting);
            currentPrintIndex = 0;
            currentRowIndex = 0;
            currentPageNumber = 1;

            PrintPreviewDialog previewDialog = new PrintPreviewDialog();
            previewDialog.Document = printDocument12;
            previewDialog.WindowState = FormWindowState.Maximized;
            previewDialog.ShowDialog();



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

        private string departmentName;
        private string yearName;

        private int currentPageNumber = 1;   // رقم الصفحة العام

        private void printDocument12_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (currentPrintIndex >= subjectPages.Count)
            {
                e.HasMorePages = false;
                currentRowIndex = 0;
                currentPrintIndex = 0;
                currentPageNumber = 1;
                return;
            }

            DataTable dt = subjectPages[currentPrintIndex];

            Font headerFont = new Font("Arial", 16, FontStyle.Bold);
            Font tableHeaderFont = new Font("Arial", 9, FontStyle.Bold);
            Font tableFont = new Font("Arial", 8);
            Brush brush = Brushes.Black;

            int xRight = e.MarginBounds.Right;
            int y = 50;
            int rowHeight = 80;
            int rowHeight1 = 30;

            // ====== تنسيقات ======
            StringFormat centerAlign = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            StringFormat rightAlign = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };

            // ====== رأس الصفحة ======
            e.Graphics.DrawString("كلية العلوم الصحية", headerFont, brush,
                e.MarginBounds.Left + e.MarginBounds.Width / 2, y, centerAlign);
            y += 30;

            e.Graphics.DrawString($"القسم: {departmentName}", headerFont, brush,
                e.MarginBounds.Left + e.MarginBounds.Width / 2, y, centerAlign);
            y += 25;

            e.Graphics.DrawString($"السنة الدراسية: {yearName}", headerFont, brush,
                e.MarginBounds.Left + e.MarginBounds.Width / 2, y, centerAlign);
            y += 30;

            // التاريخ (يمين الصفحة)
            e.Graphics.DrawString($"التاريخ: {DateTime.Now:yyyy/MM/dd}", tableHeaderFont, brush,
                e.MarginBounds.Right, y, rightAlign);
            y += 40;

            // ====== حساب عرض الأعمدة ======
            int[] colWidths = new int[dt.Columns.Count];

            colWidths[0] = 160; // اسم الطالب
            colWidths[1] = 100; // الرقم الجامعي
            colWidths[dt.Columns.Count - 1] = 100; // النتيجة

            int remainingWidth = e.MarginBounds.Width - (colWidths[0] + colWidths[1] + colWidths[dt.Columns.Count - 1]);
            int subjectCount = dt.Columns.Count - 3;
            int subjectColWidth = subjectCount > 0 ? remainingWidth / subjectCount : 0;

            for (int i = 2; i < dt.Columns.Count - 1; i++)
                colWidths[i] = subjectColWidth;

            // ====== رسم رأس الجدول ======
            int colX = xRight;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                colX -= colWidths[i];
                Rectangle rect = new Rectangle(colX, y, colWidths[i], rowHeight);
                e.Graphics.FillRectangle(Brushes.LightGray, rect);
                e.Graphics.DrawRectangle(Pens.Black, rect);

                StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                // المواد عمودية فقط
                if (i >= 2 && i < dt.Columns.Count - 1)
                {
                    GraphicsState state = e.Graphics.Save();
                    e.Graphics.TranslateTransform(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
                    e.Graphics.RotateTransform(-90);
                    Rectangle textRect = new Rectangle(-rect.Height / 2, -rect.Width / 2, rect.Height, rect.Width);
                    e.Graphics.DrawString(dt.Columns[i].ColumnName, tableHeaderFont, brush, textRect, format);
                    e.Graphics.Restore(state);
                }
                else
                {
                    e.Graphics.DrawString(dt.Columns[i].ColumnName, tableHeaderFont, brush, rect, format);
                }
            }

            y += rowHeight;

            // ====== صفوف الطلاب ======
            bool isAlternate = false;
            while (currentRowIndex < dt.Rows.Count)
            {
                if (y + rowHeight1 > e.MarginBounds.Bottom)
                {
                    // ====== ترقيم الصفحة أسفل الصفحة ======
                    string pageNum = currentPageNumber.ToString();
                    Font footerFont = new Font("Arial", 10, FontStyle.Bold);
                    SizeF pageSize = e.Graphics.MeasureString(pageNum, footerFont);
                    e.Graphics.DrawString(pageNum, footerFont, Brushes.Black,
                        e.PageBounds.Width / 2 - pageSize.Width / 2, e.MarginBounds.Bottom + 30);

                    currentPageNumber++;
                    e.HasMorePages = true;
                    return;
                }

                colX = xRight;
                Brush rowBrush = isAlternate ? new SolidBrush(Color.FromArgb(235, 241, 255)) : Brushes.White;
                isAlternate = !isAlternate;

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    colX -= colWidths[i];
                    Rectangle rect = new Rectangle(colX, y, colWidths[i], rowHeight1);
                    e.Graphics.FillRectangle(rowBrush, rect);
                    e.Graphics.DrawRectangle(Pens.Black, rect);

                    StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    string cellText = dt.Rows[currentRowIndex][i]?.ToString() ?? "-";
                    e.Graphics.DrawString(cellText, tableFont, brush, rect, format);
                }

                y += rowHeight1;
                currentRowIndex++;
            }

            // ====== ترقيم الصفحة أسفل الصفحة ======
            string finalPageNum = currentPageNumber.ToString();
            Font finalFooterFont = new Font("Arial", 10, FontStyle.Bold);
            SizeF finalPageSize = e.Graphics.MeasureString(finalPageNum, finalFooterFont);
            e.Graphics.DrawString(finalPageNum, finalFooterFont, Brushes.Black,
                e.PageBounds.Width / 2 - finalPageSize.Width / 2, e.MarginBounds.Bottom + 30);

            currentPageNumber++;
            currentRowIndex = 0;
            currentPrintIndex++;
            e.HasMorePages = currentPrintIndex < subjectPages.Count;
        }




    }
}