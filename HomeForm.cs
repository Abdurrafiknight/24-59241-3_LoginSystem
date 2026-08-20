using System;
using System.Windows.Forms;

namespace _24_59241_3_LoginSystem
{
    public partial class HomeForm : Form
    {
        private UserInfo currentUser;
        private int loginHistoryId;

        public HomeForm(UserInfo user, int historyId)
        {
            InitializeComponent();
            currentUser = user;
            loginHistoryId = historyId;
        }

        private void HomeForm_Load(object sender, EventArgs e)
        {
            lblWelcome.Text = $"Welcome, {currentUser.FullName}!";
            LoadUsersGrid();
            LoadLoginHistory();
        }

        /// <summary>
        /// Loads all users into DataGridView. NEVER displays password hash.
        /// </summary>
        private void LoadUsersGrid()
        {
            dgvUsers.DataSource = DatabaseHelper.GetAllUsers();
            dgvUsers.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        /// <summary>
        /// Loads login history for the current user.
        /// </summary>
        private void LoadLoginHistory()
        {
            dgvLoginHistory.DataSource = DatabaseHelper.GetLoginHistory(currentUser.UserID);
            dgvLoginHistory.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            // Record logout time in LoginHistory
            DatabaseHelper.RecordLogout(loginHistoryId);

            MessageBox.Show("You have been logged out successfully.", 
                "Logout", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Close HomeForm - LoginForm will be shown via FormClosed event
            this.Close();
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            string oldPassword = txtOldPassword.Text;
            string newPassword = txtNewPassword.Text;
            string confirmNew = txtConfirmNewPassword.Text;

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmNew))
            {
                MessageBox.Show("All password fields are required.", 
                    "Change Password", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("New password must be at least 6 characters.", 
                    "Change Password", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPassword != confirmNew)
            {
                MessageBox.Show("New passwords do not match.", 
                    "Change Password", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool success = DatabaseHelper.ChangePassword(currentUser.UserID, oldPassword, newPassword);
            if (success)
            {
                MessageBox.Show("Password changed successfully!", 
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtOldPassword.Clear();
                txtNewPassword.Clear();
                txtConfirmNewPassword.Clear();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadUsersGrid();
            LoadLoginHistory();
        }
    }
}
