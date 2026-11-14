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
    public partial class DashboardForm : Form
    {
        private bool _navigating = false;

        public DashboardForm()
        {
            InitializeComponent();

            btnLogout.Click += btnLogout_Click;
            btnUsers.Click += btnUsers_Click;
            this.FormClosed += DashboardForm_FormClosed;

            lblUser.Text = CurrentUser.DisplayName;
            if (!string.IsNullOrWhiteSpace(CurrentUser.Role))
                lblRole.Text = CurrentUser.Role.Trim();
            lblWelcome.Text = "Welcome, " + CurrentUser.DisplayName + "!";
            lblWelcome.Width = TextRenderer.MeasureText(lblWelcome.Text, lblWelcome.Font).Width + 10;

            LoadDashboardStats();
        }

        internal void LoadDashboardStats()
        {
            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string query = @"SELECT COUNT(*) AS TotalProducts, SUM(IIF(Quantity > ReorderLevel, 1, 0)) AS InStock, SUM(IIF(Quantity > 0 AND Quantity <= ReorderLevel, 1,0)) AS LowStock, SUM(IIF(Quantity = 0, 1, 0)) AS OutOfStock FROM Products";

                    using (OleDbCommand cmd = new OleDbCommand(query, con))
                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtTotalProd.Text = reader["totalProducts"].ToString();
                            txtLowStocks.Text = reader["LowStock"].ToString();
                            txtOutofStock.Text = reader["OutOfStock"].ToString();
                        }
                    }

                    string categoryQuery = @"SELECT COUNT(*) FROM (SELECT DISTINCT Category FROM Products WHERE Category IS NOT NULL)";

                    using (OleDbCommand cmd = new OleDbCommand(categoryQuery, con))
                    {
                        label14.Text = cmd.ExecuteScalar().ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Statistic error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to log out?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Restart();
            }
        }


        private void btnUsers_Click(object sender, EventArgs e)
        {
            _navigating = true;
            new UserManagementForm().Show();
            this.Close();
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            _navigating = true;
            new ProductForm().Show();
            this.Close();
        }

        private void DashboardForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!_navigating)
                Application.Exit();
        }
    }
}