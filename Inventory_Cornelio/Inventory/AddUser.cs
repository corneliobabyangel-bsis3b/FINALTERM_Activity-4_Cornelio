using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Inventory
{
    public partial class AddUser : Form
    {
        private int selectedUserID = 0;

        public AddUser()
        {
            InitializeComponent();

            cmbUserRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.SelectedIndex = 0;  

            btnAdd.Click += btnAdd_Click;
            btnDelete.Click += btnDelete_Click;
            this.Load += (s, e) => CenterHeader();
        }

        private void CenterHeader()
        {
            foreach (Label lbl in new[] { label1, label2 })
            {
                lbl.AutoSize = true;
                lbl.Width = lbl.PreferredSize.Width;
                lbl.Left = (ClientSize.Width - lbl.Width) / 2;
            }
        }

        public void LoadSelectedUser(DataGridViewRow row)
        {
            selectedUserID = Convert.ToInt32(row.Cells["UserID"].Value);

            txtFullName.Text = row.Cells["FullName"].Value.ToString();
            txtUsername.Text = row.Cells["Username"].Value.ToString();
            cmbUserRole.Text = row.Cells["UserRole"].Value.ToString().Trim();
            cmbStatus.Text = Convert.ToBoolean(row.Cells["IsActive"].Value) ? "Active" : "Inactive";
            txtPassword.Clear();

            label1.Text = "Edit User";
            label2.Text = "Leave password blank to keep the current one.";
            CenterHeader();
        }

        private bool ValidateUser(bool passwordRequired)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Please enter the full name.");
                txtFullName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Please enter a username.");
                txtUsername.Focus();
                return false;
            }

            if (passwordRequired && txtPassword.Text.Length == 0)
            {
                MessageBox.Show("Please enter a password.");
                txtPassword.Focus();
                return false;
            }

            if (txtPassword.Text.Length > 0 && txtPassword.Text.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters long.");
                txtPassword.Focus();
                return false;
            }

            if (cmbUserRole.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a user role.");
                cmbUserRole.Focus();
                return false;
            }

            if (cmbStatus.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a status.");
                cmbStatus.Focus();
                return false;
            }

            return true;
        }

        private bool UsernameExists(string username, int excludeUserID)
        {
            using (OleDbConnection con = Database.GetConnection())
            {
                con.Open();

                string query = "SELECT COUNT(*) FROM Users WHERE Username = ? AND UserID <> ?";

                using (OleDbCommand cmd = new OleDbCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@UserID", excludeUserID);

                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateUser(true))
                return;

            try
            {
                string username = txtUsername.Text.Trim();

                if (UsernameExists(username, 0))
                {
                    MessageBox.Show("Username already exists. Please choose another one.");
                    txtUsername.Focus();
                    return;
                }

                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string query = @"INSERT INTO Users (Username, PasswordHash, FullName, UserRole, IsActive) VALUES (?, ?, ?, ?, ?)";

                    using (OleDbCommand cmd = new OleDbCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@PasswordHash", PasswordHelper.HashPassword(txtPassword.Text));
                        cmd.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                        cmd.Parameters.AddWithValue("@UserRole", cmbUserRole.Text);
                        cmd.Parameters.AddWithValue("@IsActive", cmbStatus.Text == "Active");

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("User added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding user: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedUserID == 0)
            {
                MessageBox.Show("Please select a user row to update.");
                return;
            }

            if (!ValidateUser(false))
                return;

            if (selectedUserID == CurrentUser.UserID && cmbStatus.Text != "Active")
            {
                MessageBox.Show("You cannot deactivate the account you are logged in with.");
                return;
            }

            try
            {
                string username = txtUsername.Text.Trim();

                if (UsernameExists(username, selectedUserID))
                {
                    MessageBox.Show("Username already exists. Please choose another one.");
                    txtUsername.Focus();
                    return;
                }

                bool changePassword = txtPassword.Text.Length > 0;

                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string query = changePassword
                        ? "UPDATE Users SET Username = ?, FullName = ?, UserRole = ?, IsActive = ?, PasswordHash = ? WHERE UserID = ?"
                        : "UPDATE Users SET Username = ?, FullName = ?, UserRole = ?, IsActive = ? WHERE UserID = ?";

                    using (OleDbCommand cmd = new OleDbCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                        cmd.Parameters.AddWithValue("@UserRole", cmbUserRole.Text);
                        cmd.Parameters.AddWithValue("@IsActive", cmbStatus.Text == "Active");

                        if (changePassword)
                            cmd.Parameters.AddWithValue("@PasswordHash", PasswordHelper.HashPassword(txtPassword.Text));

                        cmd.Parameters.AddWithValue("@UserID", selectedUserID);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("User updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating user: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedUserID == 0)
            {
                MessageBox.Show("Please select a user row to delete.");
                return;
            }

            if (selectedUserID == CurrentUser.UserID)
            {
                MessageBox.Show("You cannot delete the account you are logged in with.");
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this user?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (OleDbConnection con = Database.GetConnection())
                    {
                        con.Open();

                        using (OleDbCommand cmd = new OleDbCommand("DELETE FROM Users WHERE UserID = ?", con))
                        {
                            cmd.Parameters.AddWithValue("@UserID", selectedUserID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("User deleted successfully!");

                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting user: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtFullName.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
            cmbUserRole.SelectedIndex = -1;
            cmbStatus.SelectedIndex = 0;
            selectedUserID = 0;
            txtFullName.Focus();
        }
    }
}
