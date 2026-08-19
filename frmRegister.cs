using System;
using System.Windows.Forms;

namespace Login_and_Register
{
    public partial class frmRegister : Form
    {
        public frmRegister()
        {
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirm = txtConPassword.Text;
            string email = txtEmail.Text.Trim();
            string fullName = txtFullName.Text.Trim();

            // Validation
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(confirm) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(fullName))
            {
                MessageBox.Show("All fields are required.", "Validation Error");
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Validation Error");
                return;
            }

            if (password != confirm)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error");
                txtPassword.Clear();
                txtConPassword.Clear();
                txtPassword.Focus();
                return;
            }

            if (!email.Contains("@") || !email.Contains("."))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error");
                return;
            }

            try
            {
                if (DatabaseHelper.UserExists(username))
                {
                    MessageBox.Show("Username already taken.", "Registration Failed");
                    txtUsername.Clear();
                    txtUsername.Focus();
                    return;
                }

                if (DatabaseHelper.RegisterUser(username, password, email, fullName))
                {
                    MessageBox.Show("Registration successful! You can now login.", "Success");
                    ClearForm();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration failed: {ex.Message}", "Error");
            }
        }

        private void ClearForm()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtConPassword.Clear();
            txtEmail.Clear();
            txtFullName.Clear();
            txtUsername.Focus();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void clickLogin_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkbxShowPas_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = checkbxShowPas.Checked ? '\0' : '•';
            txtConPassword.PasswordChar = checkbxShowPas.Checked ? '\0' : '•';
        }
    }
}