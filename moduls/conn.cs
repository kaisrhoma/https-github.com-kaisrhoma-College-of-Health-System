using System;
using System.Data.SqlClient;

namespace college_of_health_sciences
{
    internal class conn
    {
        public class DatabaseConnection
        {
            private SqlConnection connection;

            // تأكد من اسم السيرفر والقاعدة صحيحين
            private string connectionString = @"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;";

            public SqlConnection OpenConnection()
            {
                if (connection == null)
                    connection = new SqlConnection(connectionString);

                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                return connection;
            }

            public void CloseConnection()
            {
                if (connection != null && connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }
    }
}




