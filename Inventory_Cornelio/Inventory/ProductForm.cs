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
    public partial class ProductForm : Form
    {
        private bool _navigating = false;
        private const string SearchPlaceholder = "🔍Search ";   
        private DataTable _productsTable;   

        public ProductForm()
        {
            InitializeComponent();

            lblUser.Text = CurrentUser.DisplayName;
            if (!string.IsNullOrWhiteSpace(CurrentUser.Role))
                lblRole.Text = CurrentUser.Role.Trim();

            btnDashboard.Click += btnDashboard_Click;
            btnAdd.Click += btnAdd_Click;
            this.FormClosed += ProductForm_FormClosed;
            btnLogout.Click += btnLogout_Click;

            txtSearch.Text = SearchPlaceholder;
            txtSearch.ForeColor = Color.Gray;
            txtSearch.Enter += txtSearch_Enter;
            txtSearch.Leave += txtSearch_Leave;

            dgvProducts.ReadOnly = true;
            dgvProducts.AllowUserToAddRows = false; 
            dgvProducts.CellClick += dgvProducts_CellClick;
        }

        private void LoadProductStats()
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
                            lblTotalCount.Text = reader["totalProducts"].ToString();
                            lblInStockCount.Text = reader["InStock"].ToString();
                            lblLowStockCount.Text = reader["LowStock"].ToString();
                            lblOutOfStockCount.Text = reader["OutOfStock"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Statistic error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string query = @"SELECT * FROM Products ORDER BY ProductID DESC";

                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();

                        adapter.Fill(dt);

                        _productsTable = dt;
                        dgvProducts.DataSource = _productsTable.DefaultView;
                        ApplyFilter();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load products:" + ex.Message);
            }
        }

        private void ApplyFilter()
        {
            if (_productsTable == null) return;

            List<string> filters = new List<string>();

            string text = (txtSearch.Text == SearchPlaceholder) ? "" : txtSearch.Text.Trim();
            if (text.Length > 0)
            {
                string safe = EscapeLike(text);
                filters.Add("(ProductName LIKE '%" + safe + "%' OR Description LIKE '%" + safe + "%')");
            }

            _productsTable.DefaultView.RowFilter = string.Join(" AND ", filters.ToArray());
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (AddProduct add = new AddProduct())
            {
                add.ShowDialog(this);
            }

            LoadProducts();
            LoadProductStats();
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            _navigating = true;
            new DashboardForm().Show();
            this.Close();
        }

        private void ProductForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!_navigating)
                Application.Exit();
        }

        private void ProductForm_Load(object sender, EventArgs e)
        {
            LoadProducts();
            LoadProductStats();
        }

        private void dgvProducts_CellClick (object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex >= 0)
            {
                DataGridViewRow row =  dgvProducts.Rows[e.RowIndex];

                using (AddProduct edit = new AddProduct())
                {
                    edit.LoadSelectedProduct(row);
                    edit.ShowDialog(this);
                }

                LoadProducts();
                LoadProductStats();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Text = SearchPlaceholder;
            txtSearch.ForeColor = Color.Black;

            LoadProducts();
            LoadProductStats();
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

        private void cmb1Category_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel7_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel16_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel14_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel15_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
