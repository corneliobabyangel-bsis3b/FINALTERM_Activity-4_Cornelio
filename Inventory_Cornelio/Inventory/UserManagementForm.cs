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
    public partial class UserManagementForm : Form
    {
        private bool _navigating = false;   
        private const string SearchPlaceholder = "🔍Search ";  
        private DataTable _usersTable;  

        public UserManagementForm()
        {
            InitializeComponent();

            lblUser.Text = CurrentUser.DisplayName;

            if (!string.IsNullOrWhiteSpace(CurrentUser.Role))
                lblRole.Text = CurrentUser.Role.Trim();

            button2.Click += btnDashboard_Click;
            btnProducts.Click += btnProducts_Click;
            btnLogout.Click += btnLogout_Click;
            this.FormClosed += UserManagementForm_FormClosed;

            txtSearch.Text = SearchPlaceholder;
            txtSearch.ForeColor = Color.Gray;
            txtSearch.Enter += txtSearch_Enter;
            txtSearch.Leave += txtSearch_Leave;
           txtSearch.TextChanged += txtSearch_TextChanged;
                      
            dgvUserList.ReadOnly = true;
            dgvUserList.AllowUserToAddRows = false;
            dgvUserList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUserList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvUserList.CellClick += dgvUserList_CellClick;

            btnAddUser.Click += btnAddUser_Click;
            btnRefresh.Click += btnRefresh_Click;
            this.Load += UserManagementForm_Load;
        }

        private void LoadUserStats()
        {
            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string query = @"SELECT COUNT(*) AS TotalUsers, SUM(IIF(IsActive = True, 1, 0)) AS ActiveUsers, SUM(IIF(IsActive = False, 1, 0)) AS InactiveUsers FROM Users";

                    using (OleDbCommand cmd = new OleDbCommand(query, con))
                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblTotalUser.Text = CountText(reader["TotalUsers"]);
                            lblActiveUser.Text = CountText(reader["ActiveUsers"]);
                            lblInactiveUser.Text = CountText(reader["InactiveUsers"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Statistic error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string CountText(object value)
        {
            return (value == null || value == DBNull.Value) ? "0" : value.ToString();
        }

        private void LoadUsers()
        {
            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string query = @"SELECT UserID, Username, FullName, UserRole, IsActive FROM Users ORDER BY UserID DESC";

                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();

                        adapter.Fill(dt);

                        _usersTable = dt;
                        dgvUserList.DataSource = _usersTable.DefaultView;

                        dgvUserList.Columns["FullName"].HeaderText = "Full Name";
                        dgvUserList.Columns["UserRole"].HeaderText = "Role";
                        dgvUserList.Columns["IsActive"].HeaderText = "Active";

                        ApplyFilter();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load users: " + ex.Message);
            }
        }

        private void ApplyFilter()
        {
            if (_usersTable == null) return;

            List<string> filters = new List<string>();

            string text = (txtSearch.Text == SearchPlaceholder) ? "" : txtSearch.Text.Trim();
            if (text.Length > 0)
            {
                string safe = EscapeLike(text);
                filters.Add("(Username LIKE '%" + safe + "%' OR FullName LIKE '%" + safe + "%')");
            }
                       
            _usersTable.DefaultView.RowFilter = string.Join(" AND ", filters.ToArray());
        }

        private static string EscapeLike(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c == '*' || c == '%' || c == '[' || c == ']')
                    sb.Append('[').Append(c).Append(']');
                else if (c == '\'')
                    sb.Append("''");
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        private void UserManagementForm_Load(object sender, EventArgs e)
        {
            LoadUsers();
            LoadUserStats();
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            using (AddUser add = new AddUser())
            {
                add.ShowDialog(this);
            }

            LoadUsers();
            LoadUserStats();
        }

        private void dgvUserList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvUserList.Rows[e.RowIndex];

                using (AddUser edit = new AddUser())
                {
                    edit.LoadSelectedUser(row);
                    edit.ShowDialog(this);
                }

                LoadUsers();
                LoadUserStats(); 
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Text = SearchPlaceholder;
            txtSearch.ForeColor = Color.Black;

            LoadUsers();
            LoadUserStats();
        }

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == SearchPlaceholder)
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = SystemColors.WindowText;
            }
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (txtSearch.Text.Trim().Length == 0)
            {
                txtSearch.Text = SearchPlaceholder;
                txtSearch.ForeColor = Color.Gray;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void cmbRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void cmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            _navigating = true;
            new DashboardForm().Show();
            this.Close();
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            _navigating = true;
            new ProductForm().Show();
            this.Close();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to log out?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Restart();
            }
        }

        private void UserManagementForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!_navigating)
                Application.Exit();
        }
    }
}
