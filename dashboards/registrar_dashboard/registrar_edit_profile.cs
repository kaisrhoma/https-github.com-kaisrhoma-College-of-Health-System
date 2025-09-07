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
    public partial class registrar_edit_profile : UserControl
    {
        public registrar_edit_profile()
        {
            InitializeComponent();
            LoadUserData();
            r();
        }
        private void r() {
            if (Session.Role == "Admin")
            {
                label13.Text = "Admin";
            }
            else if (Session.Role == "Registrar")
            {
                label13.Text = "مكتب المسجل العام";
            }
            else if (Session.Role == "Exams")
            {
                label13.Text = "الدراسة والامتحانات";
            }
         

        }
        private void LoadUserData()
        {
            string connStr = @"Server=.\SQLEXPRESS;Database=Cohs_DB;Trusted_Connection=True;";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT username FROM Users WHERE user_id = @id"; // فقط اسم المستخدم
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", Session.userID);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    textBox1.Text = reader["username"].ToString();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string oldPassword = textBox2.Text.Trim(); // كلمة المرور القديمة
            string newPassword = textBox3.Text.Trim(); // كلمة المرور الجديدة

            if (oldPassword == "" || newPassword == "")
            {
                MessageBox.Show("يرجى إدخال كلمة المرور القديمة والجديدة.");
                return;
            }

            string connStr = @"Server=.\SQLEXPRESS;Database=Cohs_DB;Trusted_Connection=True;";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction(); // بدء المعاملة

                try
                {
                    // أولاً التحقق من كلمة المرور القديمة
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE user_id = @id AND password = @oldPassword";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction);
                    checkCmd.Parameters.AddWithValue("@id", Session.userID);
                    checkCmd.Parameters.AddWithValue("@oldPassword", oldPassword);

                    int count = (int)checkCmd.ExecuteScalar();
                    if (count == 0)
                    {
                        MessageBox.Show("كلمة المرور القديمة غير صحيحة.");
                        transaction.Rollback(); // التراجع
                        return;
                    }

                    // تحديث كلمة المرور الجديدة
                    string updateQuery = "UPDATE Users SET password = @newPassword WHERE user_id = @id";
                    SqlCommand updateCmd = new SqlCommand(updateQuery, conn, transaction);
                    updateCmd.Parameters.AddWithValue("@newPassword", newPassword);
                    updateCmd.Parameters.AddWithValue("@id", Session.userID);

                    int rowsAffected = updateCmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        transaction.Commit(); // تأكيد التحديث
                        MessageBox.Show("تم تحديث كلمة المرور بنجاح.");
                    }
                    else
                    {
                        transaction.Rollback(); // التراجع إذا لم يتم التحديث
                        MessageBox.Show("لم يتم التحديث، تحقق من البيانات.");
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // التراجع في حالة الخطأ
                    MessageBox.Show("حدث خطأ أثناء تحديث كلمة المرور: " + ex.Message);
                }
            }
        }
    }
}
