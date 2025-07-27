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
ORDER BY c.course_id, cc.group_number, s.university_number;";


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

                // اربط دالة الطباعة بالـ PrintDocument
                printDocument1.PrintPage -= printDocument1_PrintPage; // لتجنب التكرار عند الطباعة أكثر من مرة
                printDocument1.PrintPage += printDocument1_PrintPage;



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
            //// الحصول على البيانات من DataGridView
            //DataTable dt = (DataTable)dataGridViewGrades.DataSource;
            //if (dt == null || dt.Rows.Count == 0)
            //{
            //    MessageBox.Show("لا توجد بيانات للطباعة.");
            //    return;
            //}

            //// تجهيز الصفحات حسب كل مادة
            //PreparePagesByCourse(dt);
            //PreparePagesByCourse((DataTable)dataGridViewGrades.DataSource);
            //printDocument1.Print();


            //// إعادة تعيين الصفحة الحالية
            //currentPageIndex = 0;

            //// تأكد من عدم تكرار ربط الحدث
            //printDocument1.PrintPage -= printDocument1_PrintPage;
            //printDocument1.PrintPage += printDocument1_PrintPage;

            //// معاينة قبل الطباعة
            //PrintPreviewDialog previewDialog = new PrintPreviewDialog();
            //previewDialog.Document = printDocument1;
            //previewDialog.ShowDialog();
            // الحصول على البيانات من DataGridView


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




            //printDocument1.PrintPage -= printDocument1_PrintPage;
            printDocument1.PrintPage += printDocument1_PrintPage;



            //PrintPreviewDialog previewDialog = new PrintPreviewDialog();

            //previewDialog.Document = printDocument1;

            //previewDialog.ShowDialog();
            currentPageIndex = 0;
            printDocument1.Print();






        }
        //--------------------------------------------------------------------------------------------------------------------2

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
        c.year_number AS السنة,
        c.course_name AS المادة,
        g.final_grade AS الدرجة,
        c.units AS الوحدات
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
            // حساب معدل كل سنة والمعدل التراكمي
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
                    int grade = row.Field<int>("الدرجة");
                    int units = row.Field<int>("الوحدات");
                    sumWeightedGrades += grade * units;
                    sumUnits += units;
                }

                double yearAverage = sumUnits == 0 ? 0 : sumWeightedGrades / sumUnits;
                averagesText += $"معدل السنة {year}: {yearAverage:F2}\n";

                totalWeightedGrades += sumWeightedGrades;
                totalUnits += sumUnits;
            }

            double cumulativeAverage = totalUnits == 0 ? 0 : totalWeightedGrades / totalUnits;
            averagesText += $"المعدل التراكمي: {cumulativeAverage:F2}";

            // عرض المعدلات في مربع نص أو label
            label9.Text = averagesText;
        }
    }
}
