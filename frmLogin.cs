using System;
using System.Windows.Forms;

namespace Login_and_Register
{
    public partial class frmLogin : Form
    {
        private int failedAttempts = 0;

        public frmLogin()
        {
            InitializeComponent();
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            if (!DatabaseHelper.TestConnection())
            {
                btnLogin.Enabled = false;
                MessageBox.Show("Database connection failed. Please check your connection string.", "Error");
            }
            ClearLoginForm();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (failedAttempts >= 3)
            {
                MessageBox.Show("Too many failed attempts. Login disabled.", "Account Locked");
                return;
            }

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Validation Error");
                return;
            }

            try
            {
                if (DatabaseHelper.ValidateLogin(username, password, out string fullName))
                {
                    failedAttempts = 0;
                    this.Hide();
                    frmDashboard dashboard = new frmDashboard(fullName);
                    dashboard.ShowDialog();
                    this.Show();
                    ClearLoginForm();
                }
                else
                {
                    failedAttempts++;
                    int remaining = 3 - failedAttempts;
                    MessageBox.Show($"Invalid username or password. {remaining} attempts remaining.", "Login Failed");

                    if (failedAttempts >= 3)
                    {
                        btnLogin.Enabled = false;
                        MessageBox.Show("Login disabled due to 3 failed attempts.", "Account Locked");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error");
            }
        }

        private void ClearLoginForm()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtUsername.Focus();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearLoginForm();
        }

        private void clickRegister_Click(object sender, EventArgs e)
        {
            frmRegister register = new frmRegister();
            register.ShowDialog();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void checkbxShowPas_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = checkbxShowPas.Checked ? '\0' : '•';
        }
    }
}