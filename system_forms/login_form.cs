using college_of_health_sciences.system_forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace college_of_health_sciences
{
    public partial class login_form : Form
    {

        public login_form()
        {
            InitializeComponent();
            Session.Role = "Admin";
        }

        private void login_form_Load(object sender, EventArgs e)
        {
    
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label4.Text = "Admin Login";
            textBox1.Focus();
            Session.Role = "Admin";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            label4.Text = "Registrar Login";
            textBox1.Focus();
            Session.Role = "Registrar";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            label4.Text = "Exams Login";
            textBox1.Focus();
            Session.Role = "Exams";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            label4.Text = "S Affairs Login";
            textBox1.Focus();
            Session.Role = "S Affairs";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            label4.Text = "Departments Login";
            textBox1.Focus();
            Session.Role = "Departments";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text.Trim();
            string password = textBox2.Text.Trim();

            if (Session.Role != "")
            {
                conn.DatabaseConnection db = new conn.DatabaseConnection();
                SqlConnection conn = db.OpenConnection();

                string query = "SELECT user_id FROM Users WHERE username = @username AND password = @password AND role = @role";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@role", Session.Role);


                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    Session.userID = Convert.ToInt32(reader["user_id"]);
                    Session.Username = username;


                    label5.Text = "تم تسجيل الدخول";

                    // افتح الفورم حسب الدور
                    Form nextForm = null;

                    switch (Session.Role.ToLower())
                    {
                        case "admin":
                            nextForm = new admin_form();
                            break;
                        case "registrar":
                            nextForm = new registerar_form();
                            break;
                        case "exams":
                            nextForm = new exams_form();
                            break;
                        //case "s affairs":
                        //    nextForm = new SAffairsForm();
                        //    break;
                        //case "departments":
                        //    nextForm = new DepartmentsForm();
                        //    break;
                        default:
                            MessageBox.Show("دور غير معروف.");
                            break;
                    }

                    if (nextForm != null)
                    {
                        this.Hide();
                        nextForm.Show();
                    }
                }
                else
                {
                    label5.Text = "اسم المستخدم أو كلمة المرور غير صحيحة.";
                }


                reader.Close();
                db.CloseConnection();

            }
            else
            {
                label5.Text = "الرجاء اختيار الدور قبل تسجيل الدخول.";
            }
            }

        private void login_form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // إذا المستخدم كتب في TextBox1 فقط → انقل التركيز إلى TextBox2
                if (textBox1.Text != "" && textBox2.Text == "")
                {
                    textBox2.Focus();
                    return;
                }

                // إذا كانت كلا الحقلين فيها بيانات → نفذ تسجيل الدخول
                if (textBox1.Text != "" && textBox2.Text != "")
                {
                    if (Session.Role != "")
                    {
                        string username = textBox1.Text.Trim();
                        string password = textBox2.Text.Trim();

                        conn.DatabaseConnection db = new conn.DatabaseConnection();
                        SqlConnection conn = db.OpenConnection();

                        string query = "SELECT user_id FROM Users WHERE username = @username AND password = @password AND role = @role";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@role", Session.Role);

                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            Session.userID = Convert.ToInt32(reader["user_id"]);
                            Session.Username = username;


                            label5.Text = "تم تسجيل الدخول";

                            // افتح الفورم حسب الدور
                            Form nextForm = null;

                            switch (Session.Role.ToLower())
                            {
                                case "admin":
                                    nextForm = new admin_form();
                                    break;
                                case "registrar":
                                    nextForm = new registerar_form();
                                    break;
                                case "exams":
                                    nextForm = new exams_form();
                                    break;
                                //case "s affairs":
                                //    nextForm = new SAffairsForm();
                                //    break;
                                //case "departments":
                                //    nextForm = new DepartmentsForm();
                                //    break;
                                default:
                                    MessageBox.Show("دور غير معروف.");
                                    break;
                            }

                            if (nextForm != null)
                            {
                                this.Hide();
                                nextForm.Show();
                            }
                        }
                        else
                        {
                            label5.Text = "اسم المستخدم أو كلمة المرور غير صحيحة.";
                        }

                        reader.Close();
                        db.CloseConnection();
                    }
                    else
                    {
                        label5.Text = "الرجاء اختيار الدور قبل تسجيل الدخول.";
                    }
                    }
                else
                {
                    label5.Text = "الرجاء إدخال اسم المستخدم وكلمة المرور.";
                }
            }
        


    }

        private void login_form_Shown(object sender, EventArgs e)
        {
            textBox1.Focus();
        }
    }
}
