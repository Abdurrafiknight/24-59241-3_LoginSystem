using System;
using System.Windows.Forms;

namespace _24_59241_3_LoginSystem
{
    public partial class LoginForm : Form
    {
        private int failedAttempts = 0;
        private const int MaxAttempts = 3;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            ClearForm();
        }

        /// <summary>
        /// Clears all input fields and resets focus.
        /// </summary>
        public void ClearForm()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            failedAttempts = 0;
            btnLogin.Enabled = true;
            txtUsername.Focus();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            // Basic validation
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", 
                    "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate login against database
            UserInfo user = DatabaseHelper.ValidateLogin(username, password);

            if (user != null)
            {
                // Success: record login history
                int historyId = DatabaseHelper.RecordLogin(user.UserID);

                MessageBox.Show($"Welcome, {user.FullName}!", 
                    "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Open HomeForm and pass user info + history ID
                HomeForm homeForm = new HomeForm(user, historyId);
                homeForm.FormClosed += (s, args) => this.Show();
                homeForm.Show();
                this.Hide();
                ClearForm();
            }
            else
            {
                // Failure
                failedAttempts++;
                int remaining = MaxAttempts - failedAttempts;

                if (failedAttempts >= MaxAttempts)
                {
                    MessageBox.Show("Too many failed attempts. Login button has been disabled.", 
                        "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnLogin.Enabled = false;
                }
                else
                {
                    MessageBox.Show($"Invalid username or password. {remaining} attempt(s) remaining.", 
                        "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            RegistrationForm regForm = new RegistrationForm();
            regForm.ShowDialog();
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            DatabaseHelper.TestConnection();
        }
    }
}
