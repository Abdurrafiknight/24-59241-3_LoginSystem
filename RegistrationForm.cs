using System;
using System.Windows.Forms;

namespace _24_59241_3_LoginSystem
{
    public partial class RegistrationForm : Form
    {
        public RegistrationForm()
        {
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;
            string email = txtEmail.Text.Trim();
            string fullName = txtFullName.Text.Trim();

            // Validation 1: No empty fields
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(confirmPassword) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(fullName))
            {
                MessageBox.Show("All fields are required.", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validation 2: Password length
            if (password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validation 3: Passwords match
            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validation 4: Email contains @
            if (!email.Contains("@"))
            {
                MessageBox.Show("Please enter a valid email address.", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if username already exists using ExecuteScalar
            if (DatabaseHelper.UsernameExists(username))
            {
                MessageBox.Show("Username already taken. Please choose a different username.", 
                    "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Insert with parameterized query and hashed password
            bool success = DatabaseHelper.RegisterUser(username, password, email, fullName);

            if (success)
            {
                MessageBox.Show("Registration successful! You can now log in.", 
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                this.Close();
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ClearForm()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();
            txtEmail.Clear();
            txtFullName.Clear();
        }
    }
}
