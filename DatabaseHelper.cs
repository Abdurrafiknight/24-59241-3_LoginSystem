using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace _24_59241_3_LoginSystem
{
    /// <summary>
    /// Centralized database helper class.
    /// All database operations are handled here; forms never touch SqlConnection directly.
    /// </summary>
    public static class DatabaseHelper
    {
        // Read connection string from App.config
        private static readonly string ConnectionString = 
            ConfigurationManager.ConnectionStrings["LoginDB"].ConnectionString;

        /// <summary>
        /// Returns a new SqlConnection instance.
        /// </summary>
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        /// <summary>
        /// Tests the database connection and shows a friendly message on failure.
        /// </summary>
        public static bool TestConnection()
        {
            try
            {
                using (SqlConnection con = GetConnection())
                {
                    con.Open();
                    MessageBox.Show("Database connection successful!", "Connection Test", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to database.\n\nError: {ex.Message}", 
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Computes SHA-256 hash of the input string.
        /// </summary>
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Checks if a username already exists in the database.
        /// </summary>
        public static bool UsernameExists(string username)
        {
            using (SqlConnection con = GetConnection())
            {
                string query = "SELECT COUNT(*) FROM dbo.Users WHERE Username = @Username";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    con.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        /// <summary>
        /// Registers a new user with hashed password.
        /// </summary>
        public static bool RegisterUser(string username, string password, string email, string fullName)
        {
            try
            {
                using (SqlConnection con = GetConnection())
                {
                    string query = @"INSERT INTO dbo.Users (Username, PasswordHash, Email, FullName) 
                                      VALUES (@Username, @PasswordHash, @Email, @FullName)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@PasswordHash", HashPassword(password));
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@FullName", fullName);

                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 2627) // UNIQUE constraint violation
            {
                MessageBox.Show("Username already taken. Please choose a different username.", 
                    "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration failed.\n\nError: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Validates login credentials and returns user info if successful.
        /// </summary>
        public static UserInfo ValidateLogin(string username, string password)
        {
            using (SqlConnection con = GetConnection())
            {
                string query = "SELECT UserID, Username, PasswordHash, FullName FROM dbo.Users WHERE Username = @Username";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedHash = reader["PasswordHash"].ToString();
                            string inputHash = HashPassword(password);

                            if (storedHash == inputHash)
                            {
                                return new UserInfo
                                {
                                    UserID = Convert.ToInt32(reader["UserID"]),
                                    Username = reader["Username"].ToString(),
                                    FullName = reader["FullName"] != DBNull.Value ? reader["FullName"].ToString() : ""
                                };
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Records a login event in LoginHistory table.
        /// Returns the HistoryID for later logout update.
        /// </summary>
        public static int RecordLogin(int userId)
        {
            using (SqlConnection con = GetConnection())
            {
                string query = "INSERT INTO dbo.LoginHistory (UserID) OUTPUT INSERTED.HistoryID VALUES (@UserID)";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    con.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Updates the LogoutTime for a given login history record.
        /// </summary>
        public static void RecordLogout(int historyId)
        {
            using (SqlConnection con = GetConnection())
            {
                string query = "UPDATE dbo.LoginHistory SET LogoutTime = GETDATE() WHERE HistoryID = @HistoryID";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@HistoryID", historyId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Retrieves all users for display in DataGridView.
        /// NEVER includes the password hash column.
        /// </summary>
        public static DataTable GetAllUsers()
        {
            using (SqlConnection con = GetConnection())
            {
                string query = "SELECT UserID, Username, Email, FullName, CreatedAt FROM dbo.Users ORDER BY CreatedAt DESC";
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, con))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        /// <summary>
        /// Retrieves login history for a specific user.
        /// </summary>
        public static DataTable GetLoginHistory(int userId)
        {
            using (SqlConnection con = GetConnection())
            {
                string query = @"SELECT HistoryID, LoginTime, LogoutTime 
                                  FROM dbo.LoginHistory 
                                  WHERE UserID = @UserID 
                                  ORDER BY LoginTime DESC";
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, con))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@UserID", userId);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        /// <summary>
        /// Updates user password after verifying old password.
        /// </summary>
        public static bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            using (SqlConnection con = GetConnection())
            {
                string verifyQuery = "SELECT PasswordHash FROM dbo.Users WHERE UserID = @UserID";
                using (SqlCommand verifyCmd = new SqlCommand(verifyQuery, con))
                {
                    verifyCmd.Parameters.AddWithValue("@UserID", userId);
                    con.Open();
                    string storedHash = verifyCmd.ExecuteScalar()?.ToString();

                    if (storedHash != HashPassword(oldPassword))
                    {
                        MessageBox.Show("Old password is incorrect.", 
                            "Change Password", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
            }

            using (SqlConnection con = GetConnection())
            {
                string updateQuery = "UPDATE dbo.Users SET PasswordHash = @NewHash WHERE UserID = @UserID";
                using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@NewHash", HashPassword(newPassword));
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }

    /// <summary>
    /// Simple data class to hold user information after successful login.
    /// </summary>
    public class UserInfo
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
    }
}
