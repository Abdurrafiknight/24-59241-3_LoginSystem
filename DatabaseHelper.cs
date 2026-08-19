using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Login_and_Register
{
    public static class DatabaseHelper
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["LoginDB"].ConnectionString;

        public static bool TestConnection()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot connect to database: {ex.Message}", "Connection Error");
                return false;
            }
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        public static bool UserExists(string username)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Users WHERE Username = @username";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public static bool RegisterUser(string username, string password, string email, string fullName)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO Users (Username, PasswordHash, Email, FullName) 
                                    VALUES (@username, @passwordHash, @email, @fullName)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@passwordHash", HashPassword(password));
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@fullName", fullName);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                throw new Exception("Username already taken.");
            }
        }

        public static bool ValidateLogin(string username, string password, out string fullName)
        {
            fullName = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT PasswordHash, FullName FROM Users WHERE Username = @username";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedHash = reader["PasswordHash"].ToString();
                            fullName = reader["FullName"].ToString();
                            return HashPassword(password) == storedHash;
                        }
                        return false;
                    }
                }
            }
        }

        public static DataTable GetAllUsers()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT UserID, Username, Email, CreatedAt FROM Users ORDER BY CreatedAt DESC";
                using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                {
                    da.Fill(dt);
                }
            }
            return dt;
        }
    }
}