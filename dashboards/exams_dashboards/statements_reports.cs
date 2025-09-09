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
        private readonly string connectionString = @"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;";
        private PrintDocument printDocument1 = new PrintDocument();
        private DataTable reportData;
        private PrintDocument printDocument2 = new PrintDocument();
        private PrintDocument printDocument3 = new PrintDocument();
        private PrintDocument printDocument0 = new PrintDocument();

        private List<string> pageSummaries = new List<string>();
        private string studentName = "", universityNumber = "";
        // بيانات الطالب
        // حساب المعدلات
        private double totalWeightedGrades = 0;
        private int totalUnits = 0;
        private double cumulativeAverage = 0;
        private string studentDepartment;
        private string academicYear;

     




        //3
        private List<DataTable> subjectPages = new List<DataTable>();
        private int currentPrintIndex = 0;
        private int currentRowIndex = 0;

        private PrintDocument printDocument12 = new PrintDocument();

        private string d;
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
            printDocument0.PrintPage += printDocument0_PrintPage;

            printDocument1.BeginPrint += BeginPrint_Reset;
            printDocument2.BeginPrint += BeginPrint_Reset;
            printDocument3.BeginPrint += BeginPrint_Reset;
            printDocument12.BeginPrint += BeginPrint_Reset;
            printDocument0.BeginPrint += BeginPrint_Reset;
            //3
            n();

            comboBox_Year.SelectedIndex = 0;
            FillDepartmentComboBox();
            printDocument12.PrintPage += printDocument12_PrintPage;
            numericUpDownYear1.Minimum = 1990;    // أقل سنة مسموح بها
            numericUpDownYear1.Maximum = 2100;    // أعلى سنة مسموح بها
            numericUpDownYear1.Value = DateTime.Now.Year;      // القيمة الافتراضية (مثلاً)
            numericUpDownYear1.Increment = 1;     // خطوة الزيادة/النقصان سنة واحدة
            numericUpDownYear1.ThousandsSeparator = false; // حسب رغبتك
            int startYear1 = (int)numericUpDownYear1.Value;

            //************
            nv();
            LoadDepartments();


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
    cd.course_dep_code AS 'رمز المادة',
    c.year_number AS 'السنة الدراسية',
    ISNULL(CAST(cc.group_number AS NVARCHAR(50)), N'غير محدد') AS 'رقم المجموعة',
    i.full_name AS 'اسم الأستاذ',
    s.full_name AS 'اسم الطالب',
    s.university_number AS 'الرقم الجامعي',
    d.dep_name AS 'القسم',
    g.total_grade AS 'الدرجة',
    g.success_status AS 'النتيجة',
    r.status AS 'حالة التسجيل'
FROM Grades g
INNER JOIN Students s 
    ON g.student_id = s.student_id
INNER JOIN Registrations r 
    ON r.student_id = s.student_id 
   AND r.course_id = g.course_id
INNER JOIN Courses c 
    ON r.course_id = c.course_id
LEFT JOIN Course_Classroom cc 
    ON r.course_classroom_id = cc.id
INNER JOIN Course_Department cd 
    ON cd.course_id = c.course_id 
   AND cd.department_id = s.department_id
INNER JOIN Departments d 
    ON s.department_id = d.department_id
LEFT JOIN Course_Instructor ci 
    ON c.course_id = ci.course_id
LEFT JOIN Instructors i 
    ON ci.instructor_id = i.instructor_id
WHERE r.academic_year_start = @academic_year
  AND c.year_number = @year_number
  AND r.status = N'مسجل'
ORDER BY d.dep_name, c.year_number, s.university_number, c.course_id;

";

            using (SqlConnection conn = new SqlConnection(connectionString))
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


        //*****************************************************************************************



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
        private void printDocument2_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (currentPageIndex >= pages.Count)
            {
                e.HasMorePages = false;
                return;
            }

            DataTable dt = pages[currentPageIndex];
            Font headerFont = new Font("Arial", 16, FontStyle.Bold);
            Font textFont = new Font("Arial", 10, FontStyle.Regular);
            Font boldFont = new Font("Arial", 10, FontStyle.Bold);
            Brush brush = Brushes.Black;
            int margin = 40;
            int y = margin;
            int pageWidth = e.PageBounds.Width - 2 * margin;
            int x = margin;

            StringFormat leftFormat = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
            StringFormat rightFormat = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near };
            StringFormat centerFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

         
            Image logo = Properties.Resources.garyan_univirsty_logo;

            e.Graphics.DrawImage(logo, x + pageWidth / 2 - 60, y, 120, 120);
            y += 130;

            e.Graphics.DrawString("كشف درجات التخرج", headerFont, brush, new Rectangle(x, y, pageWidth, 25), centerFormat);
            y += 35;

            // الهامش الأيمن
            e.Graphics.DrawString("وزارة التعليم - ليبيا\nجامعة غريان\nالمسجل العام", boldFont, brush,
                new Rectangle(x + pageWidth - 200, margin, 200, 60), rightFormat);

            // الهامش الأيسر
            e.Graphics.DrawString("Ministry of Education - Libya\nUniversity of Gharian\nRegistrar\nDate: ............", boldFont, brush,
                new Rectangle(x, margin, 300, 60), leftFormat);

            y += 10;

            // ------------------- معلومات الطالب (تظهر فقط في الصفحة الأولى) -------------------
            if (currentPageIndex == 0)
            {
                StringFormat arabicFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.DirectionRightToLeft
                };

                string studentInfo = $"تشهد كلية العلوم الصحية غريان بأن الطالب/ة {studentName} (رقم القيد: {universityNumber}) " +
                                     $"قد درس/ت في كلية العلوم الصحية قسم {studentDepartment} بصفة قيد نظامي وقد أنهى متطلبات الدراسة بنجاح " +
                                     $"وتحصل على درجة الإجازة المتخصصة (البكالوريوس) في {studentDepartment} خلال العام الجامعي {academicYear} " +
                                     $"بتقدير عام {GetGradeLetter(cumulativeAverage)} و بنسبة: {cumulativeAverage:F2}%.";

                Rectangle infoRect = new Rectangle(x, y, pageWidth, 60);
                e.Graphics.DrawString(studentInfo, boldFont, brush, infoRect, arabicFormat);
                y += 70;
            }

            // ------------------- جدول المواد -------------------
            string[] gradeHeaders = { "رمز المادة", "المادة", "عدد الوحدات", "عدد النقاط", "الدرجة", "نتيجة المادة", "ملاحظة" };
            int gradeColCount = gradeHeaders.Length;
            int gradeColWidth = pageWidth / gradeColCount;
            int gradeRowHeight = 25;
            int tableX = x;
            int tableWidth = gradeColWidth * gradeColCount;

            // ------------------- صف السنة الدراسية -------------------
            int startYear = DateTime.Now.Year;
            Rectangle yearRect = new Rectangle(tableX, y, tableWidth, gradeRowHeight);
            e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), yearRect);
            e.Graphics.DrawRectangle(Pens.Black, yearRect);
            string yearText = $"السنة الدراسية: {GetYearText(Convert.ToInt32(dt.Rows[0]["السنة"]))} - العام الجامعي: {startYear + Convert.ToInt32(dt.Rows[0]["السنة"]) - 1}-{startYear + Convert.ToInt32(dt.Rows[0]["السنة"])}";
            e.Graphics.DrawString(yearText, boldFont, brush, yearRect, centerFormat);
            y += gradeRowHeight;

            // ------------------- رؤوس الجدول -------------------
            for (int i = gradeColCount - 1; i >= 0; i--)
            {
                Rectangle rect = new Rectangle(tableX + (gradeColCount - 1 - i) * gradeColWidth, y, gradeColWidth, gradeRowHeight);
                e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), rect);
                e.Graphics.DrawRectangle(Pens.Black, rect);
                e.Graphics.DrawString(gradeHeaders[i], boldFont, brush, rect, centerFormat);
            }
            y += gradeRowHeight;

            // ------------------- بيانات المواد -------------------
            double sumPoints = 0;
            int sumUnits = 0;

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

                string[] values = { code, subject, units.ToString(), points.ToString(), grade.ToString(), result, note };

                for (int i = gradeColCount - 1; i >= 0; i--)
                {
                    Rectangle rect = new Rectangle(tableX + (gradeColCount - 1 - i) * gradeColWidth, y, gradeColWidth, gradeRowHeight);
                    e.Graphics.DrawRectangle(Pens.Black, rect);
                    e.Graphics.DrawString(values[i], textFont, brush, rect, centerFormat);
                }
                y += gradeRowHeight;
            }

            // ------------------- صف المجموع -------------------
            int mergedColsWidth = gradeColWidth * 2;
            int startX = tableX + tableWidth - mergedColsWidth;

            Rectangle groupRectMerged = new Rectangle(startX, y, mergedColsWidth, gradeRowHeight);
            e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), groupRectMerged);
            e.Graphics.DrawRectangle(Pens.Black, groupRectMerged);
            e.Graphics.DrawString("المجموع", boldFont, brush, groupRectMerged, centerFormat);

            Rectangle unitsRect = new Rectangle(startX - gradeColWidth, y, gradeColWidth, gradeRowHeight);
            e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), unitsRect);
            e.Graphics.DrawRectangle(Pens.Black, unitsRect);
            e.Graphics.DrawString(sumUnits.ToString(), boldFont, brush, unitsRect, centerFormat);

            Rectangle pointsRect = new Rectangle(startX - 2 * gradeColWidth, y, gradeColWidth, gradeRowHeight);
            e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), pointsRect);
            e.Graphics.DrawRectangle(Pens.Black, pointsRect);
            e.Graphics.DrawString(sumPoints.ToString(), boldFont, brush, pointsRect, centerFormat);

            for (int i = 0; i < gradeColCount - 4; i++)
            {
                Rectangle rect = new Rectangle(tableX + i * gradeColWidth, y, gradeColWidth, gradeRowHeight);
                e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), rect);
                e.Graphics.DrawRectangle(Pens.Black, rect);
                e.Graphics.DrawString("", boldFont, brush, rect, centerFormat);
            }
            y += gradeRowHeight;

            // ------------------- صف المعدل السنوي -------------------
            double annualAverage = sumUnits > 0 ? sumPoints / (double)sumUnits : 0;

            Rectangle avgMergedRect = new Rectangle(tableX + tableWidth - mergedColsWidth, y, mergedColsWidth, gradeRowHeight);
            e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), avgMergedRect);
            e.Graphics.DrawRectangle(Pens.Black, avgMergedRect);
            e.Graphics.DrawString("المعدل السنوي", boldFont, brush, avgMergedRect, centerFormat);

            Rectangle avgRemainingRect = new Rectangle(tableX, y, tableWidth - mergedColsWidth, gradeRowHeight);
            e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), avgRemainingRect);
            e.Graphics.DrawRectangle(Pens.Black, avgRemainingRect);
            e.Graphics.DrawString($"{annualAverage:F2} - {GetGradeLetter(annualAverage)}", boldFont, brush, avgRemainingRect, centerFormat);
            y += gradeRowHeight;

            // ------------------- صف المعدل التراكمي -------------------
            if (currentPageIndex == pages.Count - 1)
            {
                double cumulativePoints = 0;
                int cumulativeUnits = 0;

                foreach (DataTable dtPage in pages)
                {
                    foreach (DataRow r in dtPage.Rows)
                    {
                        int units = r["الوحدات"] != DBNull.Value ? Convert.ToInt32(r["الوحدات"]) : 0;
                        int grade = r["الدرجة"] != DBNull.Value ? Convert.ToInt32(r["الدرجة"]) : 0;
                        cumulativePoints += units * grade;
                        cumulativeUnits += units;
                    }
                }

                double cumulativeAverage = cumulativeUnits > 0 ? cumulativePoints / (double)cumulativeUnits : 0;

                Rectangle cumMergedRect = new Rectangle(tableX + tableWidth - mergedColsWidth, y, mergedColsWidth, gradeRowHeight);
                e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), cumMergedRect);
                e.Graphics.DrawRectangle(Pens.Black, cumMergedRect);
                e.Graphics.DrawString("المعدل التراكمي", boldFont, brush, cumMergedRect, centerFormat);

                Rectangle cumRemainingRect = new Rectangle(tableX, y, tableWidth - mergedColsWidth, gradeRowHeight);
                e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), cumRemainingRect);
                e.Graphics.DrawRectangle(Pens.Black, cumRemainingRect);
                e.Graphics.DrawString($"{cumulativeAverage:F2} - {GetGradeLetter(cumulativeAverage)}", boldFont, brush, cumRemainingRect, centerFormat);
                y += gradeRowHeight;
            }

            // ------------------- ترقيم الصفحات -------------------
            Rectangle footerRect = new Rectangle(x, e.PageBounds.Height - 40, pageWidth, 20);
            e.Graphics.DrawString($"الصفحة {currentPageIndex + 1} من {pages.Count}", textFont, brush, footerRect, centerFormat);

            currentPageIndex++;
            e.HasMorePages = currentPageIndex < pages.Count;
        }



        // ---------------------- تحويل السنة إلى نص ----------------------
        private string GetYearText(int year)
        {
            switch (year)
            {
                case 1: return "أولى";
                case 2: return "ثانية";
                case 3: return "ثالثة";
                case 4: return "رابعة";
                default: return year.ToString();
            }
        }


        // ---------------------- تحويل المعدل إلى تقدير ----------------------
        private string GetGradeLetter(double avg)
        {
            if (avg >= 85 && avg <=100) return "ممتاز";
            if (avg >= 75 && avg<= 84) return "جيد جدًا";
            if (avg >= 65 && avg <74) return "جيد";
            if (avg >= 50 && avg<= 64) return "مقبول";

            return "راسب";
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
    s.student_id,
    s.full_name           AS اسم_الطالب,
    s.university_number   AS الرقم_جامعي,
    s.department_id       AS القسم_ID,  -- جلب الـ department_id من جدول الطلاب
    c.year_number         AS السنة,
    cd.course_dep_code    AS رمز_المادة,
    c.course_name         AS المادة,
    c.units               AS الوحدات,
    d.dep_name            AS القسم,
    g.total_grade         AS الدرجة,
    r.academic_year_start AS العام_الجامعي
FROM Registrations r
INNER JOIN Students s ON r.student_id = s.student_id
INNER JOIN Courses c ON r.course_id = c.course_id
INNER JOIN Course_Department cd ON cd.course_id = c.course_id
INNER JOIN Departments d ON cd.department_id = d.department_id
LEFT JOIN Grades g ON g.student_id = s.student_id AND g.course_id = c.course_id
WHERE s.university_number = @university_number
  AND r.status = N'مسجل'
ORDER BY c.year_number, c.course_name;
";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@university_number", uniNumber);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                reportData = new DataTable();
                da.Fill(reportData);

                if (reportData.Rows.Count == 0)
                {
                    MessageBox.Show("لا توجد بيانات لهذا الرقم الجامعي.");
                    return;
                }

                // جلب معلومات الطالب الأساسية
                studentName = reportData.Rows[0]["اسم_الطالب"].ToString();
                universityNumber = reportData.Rows[0]["الرقم_جامعي"].ToString();

                // جلب القسم الحقيقي من جدول Departments باستخدام القسم_ID
                int deptId = Convert.ToInt32(reportData.Rows[0]["القسم_ID"]);
                using (SqlCommand deptCmd = new SqlCommand("SELECT dep_name FROM Departments WHERE department_id = @id", conn))
                {
                    deptCmd.Parameters.AddWithValue("@id", deptId);
                    conn.Open();
                    object depNameObj = deptCmd.ExecuteScalar();
                    conn.Close();

                    studentDepartment = depNameObj != null ? depNameObj.ToString() : "غير محدد";
                }

                // الحصول على آخر سنة أكاديمية مسجلة
                DataRow lastRow = reportData.AsEnumerable()
                                            .OrderByDescending(r => r.Field<int>("السنة"))
                                            .ThenByDescending(r => r.Field<int>("العام_الجامعي"))
                                            .FirstOrDefault();

                if (lastRow != null)
                    academicYear = lastRow["العام_الجامعي"].ToString();

                dataGridViewReport.DataSource = reportData;
                dataGridViewReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // حساب المعدلات
                CalculateAndDisplayAverages(reportData);
            }
        }

        //--------------------------------------------------------------------------------------------------------------------2





        private void CalculateAndDisplayAverages(DataTable dt)
        {
            var groupedByYear = dt.AsEnumerable().GroupBy(r => r.Field<int>("السنة"));
            totalWeightedGrades = 0;
            totalUnits = 0;

            foreach (var yearGroup in groupedByYear)
            {
                double sumWeightedGrades = 0;
                int sumUnits = 0;

                foreach (var row in yearGroup)
                {
                    if (row["الدرجة"] != DBNull.Value && row["الوحدات"] != DBNull.Value)
                    {
                        int grade = Convert.ToInt32(row["الدرجة"]);
                        int units = Convert.ToInt32(row["الوحدات"]);
                        sumWeightedGrades += grade * units;
                        sumUnits += units;
                    }
                }

                totalWeightedGrades += sumWeightedGrades;
                totalUnits += sumUnits;
            }

            cumulativeAverage = totalUnits == 0 ? 0 : totalWeightedGrades / totalUnits;
        }

        // ---------------------- إعداد الصفحات للطباعة ----------------------
        private void PrepareStudentReportPages(DataTable dt)
        {
            pages.Clear();
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

            currentPageIndex = 0;
        }
        // ---------------------- زر عرض المعاينة للطباعة ----------------------
        private void button4_Click(object sender, EventArgs e)
        {
            if (reportData == null || reportData.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات للطباعة.");
                return;
            }

            PrepareStudentReportPages(reportData);

            PrintPreviewDialog preview = new PrintPreviewDialog();
            preview.Document = printDocument2;
            preview.ShowDialog();
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
            y += 50;
            e.Graphics.DrawString($"القسم: {d}", headerFont, brush,
         e.MarginBounds.Left + e.MarginBounds.Width / 2, y, centerFormat);
            y += 10;
            e.Graphics.DrawString("كشف درجات", subHeaderFont, brush, new Rectangle(x, y, pageWidth, 30), centerFormat);
            y += 25;



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
            if (checkBox1.Checked)
            {
                string query = @"
SELECT 
    s.full_name       AS اسم_الطالب,
    s.university_number AS الرقم_الجامعي,
    c.year_number     AS السنة,
    c.course_code     AS رمز_المادة,
    c.course_name     AS المادة,
    c.units           AS الوحدات,
    d.dep_name        AS القسم,
    g.total_grade     AS الدرجة
FROM Grades g
INNER JOIN Students s 
    ON g.student_id = s.student_id
INNER JOIN Courses c 
    ON g.course_id = c.course_id
INNER JOIN Departments d 
    ON s.department_id = d.department_id
WHERE s.university_number = @university_number
ORDER BY c.year_number, c.course_name;";

                using (SqlConnection conn = new SqlConnection(connectionString))
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
            else
            {
                string query = @"
SELECT 
    s.full_name       AS اسم_الطالب,
    s.university_number AS الرقم_الجامعي,
    c.year_number     AS السنة,
    cd.course_dep_code AS رمز_المادة,
    c.course_name     AS المادة,
    c.units           AS الوحدات,
    d.dep_name        AS القسم,
    g.total_grade     AS الدرجة
FROM Registrations r
INNER JOIN Students s 
    ON r.student_id = s.student_id
INNER JOIN Courses c 
    ON r.course_id = c.course_id
INNER JOIN Course_Department cd 
    ON cd.course_id = c.course_id
INNER JOIN Departments d 
    ON s.department_id = d.department_id   -- هنا التعديل: استخدم department_id من Students
LEFT JOIN Grades g 
    ON g.student_id = s.student_id 
   AND g.course_id = c.course_id
WHERE s.university_number = @university_number
  AND r.status = N'مسجل'
ORDER BY c.year_number, c.course_name;
";



                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@university_number", uniNumber);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    reportData = new DataTable();
                    da.Fill(reportData);
                    // 🔹 تحقق من وجود بيانات أولًا
                    if (reportData.Rows.Count == 0)
                    {
                        MessageBox.Show("لا توجد بيانات لهذا الرقم الجامعي.");
                        return;
                    }
                    d = reportData.Rows[0]["القسم"].ToString();


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

            using (SqlConnection conn = new SqlConnection(connectionString))
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

        //**************************************************************************************************************4
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

                    comboBox3.DisplayMember = "dep_name";
                    comboBox3.ValueMember = "department_id";
                    comboBox3.DataSource = dt;

                    dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء تحميل الأقسام: " + ex.Message);
            }
        }
        private void nv()
        {
            if (comboBox3.Text == "عام")
            {
                comboBox2.Items.Clear();
                comboBox2.Items.Add("1");
                comboBox2.SelectedIndex = 0;

            

            }
            else
            {
                comboBox2.Items.Clear();
                comboBox2.Items.Add("2");
                comboBox2.Items.Add("3");
                comboBox2.Items.Add("4");
                comboBox2.SelectedIndex = 0;


            }


        }
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            nv();
            if (comboBox3.SelectedValue != null && comboBox2.SelectedItem != null)
            {
                int deptId = (int)comboBox3.SelectedValue;
                int year = Convert.ToInt32(comboBox2.SelectedItem);
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

                    comboBox1.DisplayMember = "course_name";
                    comboBox1.ValueMember = "course_id";

                    if (dt.Rows.Count > 0)
                    {
                        comboBox1.DataSource = dt;
                    }
                    else
                    {
                        comboBox1.DataSource = null;
                        comboBox1.Items.Clear();
                        comboBox1.Items.Add("لا توجد مواد");
                        comboBox1.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء تحميل المواد: " + ex.Message);
            }
        }



        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedValue != null && comboBox2.SelectedItem != null &&
                int.TryParse(comboBox3.SelectedValue.ToString(), out int deptId) &&
                int.TryParse(comboBox2.SelectedItem.ToString(), out int year))
            {
                LoadCourses(deptId, year); // تحميل المواد حسب القسم والسنة
            }
        }
        private void LoadStudentsForCourse(int courseId, int year, int deptId)
        {
            try
            {
                int currentYear = DateTime.Now.Year; // السنة الأكاديمية الحالية

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
SELECT 
    s.full_name AS [اسم الطالب],
    s.university_number AS [الرقم الجامعي],
    d.dep_name AS [القسم],
    c.course_name AS [المادة],
    '' AS [درجات الأعمال],
    '' AS [درجة الامتحان العملي]
FROM Registrations r
INNER JOIN (
    SELECT student_id, course_id, MAX(registration_id) AS max_reg_id
    FROM Registrations
    GROUP BY student_id, course_id
) rmax ON r.registration_id = rmax.max_reg_id
INNER JOIN Students s ON r.student_id = s.student_id
INNER JOIN Status st ON s.status_id = st.status_id
INNER JOIN Courses c ON r.course_id = c.course_id
INNER JOIN Course_Department cd ON c.course_id = cd.course_id
INNER JOIN Departments d ON cd.department_id = d.department_id
LEFT JOIN Grades g ON r.student_id = g.student_id AND r.course_id = g.course_id
WHERE c.course_id = @courseId
  AND c.year_number = @year
  AND cd.department_id = @deptId
  AND st.description = 'مستمر'
  AND r.status = 'مسجل'
  AND g.total_grade IS NULL
  AND g.success_status IS NULL;


;";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);

                    // تمرير المتغيرات كـ Parameters
                    da.SelectCommand.Parameters.AddWithValue("@courseId", courseId);
                    da.SelectCommand.Parameters.AddWithValue("@year", year);
                    da.SelectCommand.Parameters.AddWithValue("@deptId", deptId);
                    da.SelectCommand.Parameters.AddWithValue("@CurrentYear", currentYear);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dataGridView2.DataSource = dt;
                    reportData = dt.Copy(); // نسخ بيانات الطلاب للطباعة
                    PrepareStudentReportPages2(reportData); // إعداد الصفحات للطباعة

                    dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء تحميل الطلاب: " + ex.Message);
            }
        }


        // --- هذه الدالة تحوّل reportData إلى pages للطباعة ---
        private void PrepareStudentReportPages2(DataTable reportData)
        {
            pages = new List<DataTable>(); // pages هي متغير List<DataTable>
            int rowsPerPage = 31; // عدد الصفوف لكل صفحة (يمكن تغييره حسب الحاجة)
            int totalRows = reportData.Rows.Count;
            int pageCount = (int)Math.Ceiling((double)totalRows / rowsPerPage);

            for (int i = 0; i < pageCount; i++)
            {
                DataTable dtPage = reportData.Clone(); // نسخ الهيكل فقط
                int startRow = i * rowsPerPage;
                int endRow = Math.Min(startRow + rowsPerPage, totalRows);

                for (int j = startRow; j < endRow; j++)
                {
                    dtPage.ImportRow(reportData.Rows[j]);
                }

                pages.Add(dtPage);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (reportData == null || reportData.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات للطباعة.");
                return;
            }

            currentPageIndex = 0;

            // تأكد من أن pages تم إعدادها مسبقًا
            if (pages == null || pages.Count == 0)
            {
                PrepareStudentReportPages2(reportData);
            }

            PrintPreviewDialog preview = new PrintPreviewDialog();
            preview.Document = printDocument0;
            preview.ShowDialog();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (comboBox1.SelectedValue != null && comboBox2.SelectedItem != null && comboBox3.SelectedValue != null)
            {
                int courseId = (int)comboBox1.SelectedValue;
                int year = Convert.ToInt32(comboBox2.SelectedItem);
                int deptId = (int)comboBox3.SelectedValue;

                LoadStudentsForCourse(courseId, year, deptId);
            }
            else
            {
              
            }
        }
        //-----------------------------------------------
        private void n()
        {
            if (comboBox_Department.Text == "عام")
            {
                comboBox_Year.Items.Clear();

                comboBox_Year.Items.AddRange(new object[] {1});
                comboBox_Year.SelectedIndex = 0;

            }
            else
            {
                comboBox_Year.Items.Clear();
                comboBox_Year.Items.AddRange(new object[] {2, 3, 4 });
                comboBox_Year.SelectedIndex = 0;

            }


        }
        private void comboBox_Department_SelectedIndexChanged(object sender, EventArgs e)
        {
     n();

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void printDocument0_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (pages == null || pages.Count == 0 || currentPageIndex >= pages.Count)
            {
                e.HasMorePages = false;
                return;
            }

            DataTable dt = pages[currentPageIndex];
            DataRow firstRow = dt.Rows[0];

            Font titleFont = new Font("Arial", 14, FontStyle.Bold);
            Font subTitleFont = new Font("Arial", 12, FontStyle.Bold);
            Font headerFont = new Font("Arial", 11, FontStyle.Bold);
            Font textFont = new Font("Arial", 10);
            Brush brush = Brushes.Black;

            int y = 40;
            int pageWidth = e.PageBounds.Width;

            StringFormat centerFormat = new StringFormat { Alignment = StringAlignment.Center };

            // --- رأس الصفحة ---
            e.Graphics.DrawString("دولة ليبيا", titleFont, brush, pageWidth / 2, y, centerFormat); y += 25;
            e.Graphics.DrawString("وزارة التعليم", titleFont, brush, pageWidth / 2, y, centerFormat); y += 25;
            e.Graphics.DrawString("جامعة غريان", titleFont, brush, pageWidth / 2, y, centerFormat); y += 25;
            e.Graphics.DrawString("كلية العلوم الصحية", titleFont, brush, pageWidth / 2, y, centerFormat); y += 30;

            // --- اسم القسم واسم المادة ---ش
            string departmentName = firstRow.Table.Columns.Contains("القسم") ? firstRow["القسم"].ToString() : "غير محدد";
            string courseName = firstRow.Table.Columns.Contains("المادة") ? firstRow["المادة"].ToString() : "غير محدد";

            e.Graphics.DrawString("القسم: " + departmentName, subTitleFont, brush, pageWidth / 2, y, centerFormat); y += 25;
            e.Graphics.DrawString("المادة: " + courseName, subTitleFont, brush, pageWidth / 2, y, centerFormat); y += 30;

            // --- جدول الطلاب RTL مع عمود الترقيم ---
            string[] headers = { "رقم", "اسم الطالب", "الرقم الجامعي", "درجات الأعمال", "درجة الامتحان العملي" };
            int[] columnWidths = { 50, 200, 120, 120, 120 };
            int rowHeight = 25;
            int totalWidth = columnWidths.Sum();
            int tableY = y;
            int tableStartX = (pageWidth - totalWidth) / 2; // منتصف الصفحة

            // رسم رؤوس الأعمدة من اليمين لليسار
            int tableX = tableStartX + totalWidth;
            for (int i = 0; i < headers.Length; i++)
            {
                tableX -= columnWidths[i];
                Rectangle rect = new Rectangle(tableX, tableY, columnWidths[i], rowHeight);
                e.Graphics.FillRectangle(Brushes.LightGray, rect);
                e.Graphics.DrawRectangle(Pens.Black, rect);
                e.Graphics.DrawString(headers[i], headerFont, Brushes.Black,
                    rect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
            tableY += rowHeight;

            // بيانات الطلاب مع الترقيم وتظليل الصفوف
            bool shade = false;
            int pageHeightLimit = e.MarginBounds.Bottom - 50;
            int rowNumber = 1;

            foreach (DataRow row in dt.Rows)
            {
                if (tableY + rowHeight > pageHeightLimit)
                {
                    e.HasMorePages = true;
                    currentPageIndex++;
                    return;
                }

                tableX = tableStartX + totalWidth;

                if (shade)
                    e.Graphics.FillRectangle(Brushes.AliceBlue, new Rectangle(tableStartX, tableY, totalWidth, rowHeight));
                shade = !shade;

                string[] values = {
            rowNumber.ToString(), // عمود الترقيم
            row["اسم الطالب"].ToString(),
            row["الرقم الجامعي"].ToString(),
            row["درجات الأعمال"].ToString(),
            row["درجة الامتحان العملي"].ToString()
        };

                for (int i = 0; i < values.Length; i++)
                {
                    tableX -= columnWidths[i];
                    Rectangle rect = new Rectangle(tableX, tableY, columnWidths[i], rowHeight);
                    e.Graphics.DrawRectangle(Pens.Black, rect);
                    e.Graphics.DrawString(values[i], textFont, Brushes.Black,
                        rect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }

                tableY += rowHeight;
                rowNumber++;
            }

            // ترقيم الصفحة أسفل الصفحة
            string pageNumberText = $"الصفحة {currentPageIndex + 1}";
            e.Graphics.DrawString(pageNumberText, new Font("Arial", 10), brush,
                new RectangleF(0, e.PageBounds.Bottom - 30, pageWidth, 20),
                new StringFormat { Alignment = StringAlignment.Center });

            currentPageIndex++;
            e.HasMorePages = currentPageIndex < pages.Count;
        }



    }



}
