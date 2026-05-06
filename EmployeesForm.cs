using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;
using System.Configuration;

namespace MotorcycleRental
{
    public partial class EmployeesForm : Form
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MotorcycleRentalDB"].ConnectionString;
        private DataTable _dataTable;

        public EmployeesForm()
        {
            InitializeComponent();
            LoadData();
            SetupDataGridView();
        }

        private void SetupDataGridView()
        {
            dgvEmployees.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvEmployees.MultiSelect = false;
            dgvEmployees.ReadOnly = true;
        }

        private void LoadData()
        {
            try
            {
                string query = "SELECT EmployeeID AS \"ID\", FullName AS \"ФИО\", " +
                              "Login AS \"Логин\", Role AS \"Роль\" " +
                              "FROM Employees ORDER BY FullName";

                using (var conn = new NpgsqlConnection(_connectionString))
                using (var adapter = new NpgsqlDataAdapter(query, conn))
                {
                    _dataTable = new DataTable();
                    adapter.Fill(_dataTable);
                }

                dgvEmployees.DataSource = _dataTable;
                lblCount.Text = $"Записей: {_dataTable.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new EmployeeEditForm())
            {
                form.IsEditMode = false;
                if (form.ShowDialog() == DialogResult.OK)
                {
                    SaveEmployee(form.EmployeeData);
                    LoadData();
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись для редактирования", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = (DataRowView)dgvEmployees.CurrentRow.DataBoundItem;

            using (var form = new EmployeeEditForm())
            {
                form.IsEditMode = true;
                form.EmployeeData = new EmployeeData
                {
                    EmployeeID = Convert.ToInt32(row["ID"]),
                    FullName = row["ФИО"].ToString(),
                    Login = row["Логин"].ToString(),
                    Role = row["Роль"].ToString()
                };

                if (form.ShowDialog() == DialogResult.OK)
                {
                    SaveEmployee(form.EmployeeData);
                    LoadData();
                }
            }
        }

        private void SaveEmployee(EmployeeData data)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = data.EmployeeID == 0
                        ? "INSERT INTO Employees (FullName, Login, Role) VALUES (@name, @login, @role)"
                        : "UPDATE Employees SET FullName=@name, Login=@login, Role=@role WHERE EmployeeID=@id";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", data.FullName);
                        cmd.Parameters.AddWithValue("@login", data.Login);
                        cmd.Parameters.AddWithValue("@role", data.Role);
                        if (data.EmployeeID != 0)
                            cmd.Parameters.AddWithValue("@id", data.EmployeeID);

                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Сохранено успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись для удаления", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = (DataRowView)dgvEmployees.CurrentRow.DataBoundItem;
            int id = Convert.ToInt32(row["ID"]);

            if (MessageBox.Show("Удалить сотрудника?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("DELETE FROM Employees WHERE EmployeeID=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadData();
                MessageBox.Show("Сотрудник удален", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.CurrentRow?.Index > 0)
            {
                dgvEmployees.CurrentCell = dgvEmployees.Rows[dgvEmployees.CurrentRow.Index - 1].Cells[0];
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.CurrentRow?.Index < dgvEmployees.Rows.Count - 1)
            {
                dgvEmployees.CurrentCell = dgvEmployees.Rows[dgvEmployees.CurrentRow.Index + 1].Cells[0];
            }
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }

    public class EmployeeData
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }
    }
}