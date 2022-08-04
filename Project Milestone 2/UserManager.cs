using System.Data.SqlClient;
using System.Windows.Forms;
using System;
using System.Data;

namespace Project_Milestone_2
{
    public enum UserDetail
    {
        Email,
        Password,
        Name,
        Surname
    }
    internal class UserManager
    {
        public string userName;
        public string userSurname;
        public string userEmail;
        readonly SqlConnection sqlConnection;
        public UserManager(SqlConnection sqlConnection)
        {
            this.sqlConnection = sqlConnection;
        }

        // Checks if given email and password match and returns true or false.
        public bool Login(string email, string password)
        {
            bool ableToLogin = false;
            string cmdString = $"SELECT * FROM Users WHERE Email LIKE '{email}' AND Password LIKE '{password}'";
            SqlCommand sqlCommand = new SqlCommand
            {
                Connection = sqlConnection,
                CommandText = cmdString
            };
      
            sqlCommand.ExecuteNonQuery();

            using (SqlDataReader dataReader = sqlCommand.ExecuteReader())
            {
                if (dataReader.HasRows)
                {
                    ableToLogin = true;
                    userEmail = email;
                    while (dataReader.Read())
                    {
                        userName = (string)dataReader["Name"];
                        userSurname = (string)dataReader["Surname"];
                    }
                }
            }   
            return ableToLogin;
        }

        // This method increases the quantity of an item by the given amount(Can be negative).
        public bool ChangeDetail(UserDetail userDetail, String newValue)
        {
            bool success = false;
            string cmdString = $"UPDATE Users SET \"{userDetail.ToString()}\" = @value WHERE Email = @email";
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = sqlConnection;
            sqlCommand.CommandText = cmdString;
            sqlCommand.Parameters.AddWithValue("@email", userEmail);
            sqlCommand.Parameters.AddWithValue("@value", newValue);
         
            int rows = sqlCommand.ExecuteNonQuery();
            if (rows > 0)
            {
                success = true;
            }     
            return success;
        }

        // Register a user        
        public bool Register(string email, string password, string name, string surname)
        {
            bool exists = true;
            string cmdString = $"SELECT * FROM Users WHERE Email = \"{email}\"";
            SqlCommand sqlCommand = new SqlCommand
            {
                Connection = sqlConnection,
                CommandText = cmdString
            };
          
                int rows = sqlCommand.ExecuteNonQuery();
            if (rows == 0)
            {
                exists = false;
            }
            if (exists)
            {
                return false;
            }
            else
            {
                bool success = false;
                cmdString = "INSERT INTO Users VALUES (@email, @password)";
                sqlCommand.CommandText = cmdString;
                sqlCommand.Parameters.AddWithValue("@email", email);
                sqlCommand.Parameters.AddWithValue("@password", password);
                sqlCommand.Parameters.AddWithValue("@name", name);
                sqlCommand.Parameters.AddWithValue("@surname", surname);
             
                rows = sqlCommand.ExecuteNonQuery();
                if (rows == 1)
                {
                    success = true;
                    userEmail = email;
                    using (SqlDataReader dataReader = sqlCommand.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            userName = (string)dataReader["Name"];
                            userSurname = (string)dataReader["Surname"];
                        }
                    }
                }

                return success;
            }
        }
        public DataTable ShowAll()
        {
            string cmdString = "SELECT * FROM Users";
            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmdString, sqlConnection);

            DataSet ds = new DataSet();
            dataAdapter.Fill(ds);

            return ds.Tables[0];
        }
    }
}