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
    public partial class exams_edit_profile : UserControl
    {
        public exams_edit_profile()
        {
            InitializeComponent();
            LoadUserData();
        }

        private void LoadUserData()
        {
            string connStr = @"Server=.\SQLEXPRESS;Database=Cohs_DB;Trusted_Connection=True;";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT username, password FROM Users WHERE user_id = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", Session.userID);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    textBox1.Text = reader["username"].ToString();
                    textBox2.Text = reader["password"].ToString();
                }
            }
        }
        private void exams_edit_profile_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string newUsername = textBox1.Text.Trim();
            string newPassword = textBox2.Text.Trim();

            if (newUsername == "" || newPassword == "")
            {
                MessageBox.Show("يرجى إدخال اسم المستخدم وكلمة السر.");
                return;
            }

            string connStr = @"Server=.\SQLEXPRESS;Database=Cohs_DB;Trusted_Connection=True;";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "UPDATE Users SET username = @username, password = @password WHERE user_id = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", newUsername);
                cmd.Parameters.AddWithValue("@password", newPassword);
                cmd.Parameters.AddWithValue("@id", Session.userID);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                    MessageBox.Show("تم تحديث البيانات بنجاح.");
                else
                    MessageBox.Show("لم يتم التحديث، تحقق من البيانات.");
            }
        }
    
    }
}