using System.Data.SqlClient;

namespace college_of_health_sciences
{
    public class conn
    {
        public class DatabaseConnection
        {
            private readonly string connectionString = @"Server=.\SQLEXPRESS;Database=Cohs_DB;Integrated Security=True;";
            private SqlConnection connection;

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
