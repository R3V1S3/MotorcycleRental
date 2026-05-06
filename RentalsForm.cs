using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;
using System.Configuration;

namespace MotorcycleRental
{
    public partial class RentalsForm : Form
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["MotorcycleRentalDB"].ConnectionString;

        public RentalsForm()
        {
            InitializeComponent();
            LoadData();
            SetupDataGridView();
        }

        private void LoadData()
        {
            try
            {
                // Используем представление или JOIN для получения полной информации
                string query = @"SELECT r.RentalID, c.FullName AS ""ClientName"", 
                                e.FullName AS ""EmployeeName"", r.StartDate, r.PlannedEndDate, 
                                r.Status, r.TotalCost 
                                FROM Rentals r
                                JOIN Clients c ON r.ClientID = c.ClientID
                                JOIN Employees e ON r.EmployeeID = e.EmployeeID
                                ORDER BY r.RentalID DESC";

                using (var conn = new NpgsqlConnection(_connectionString))
                using (var adapter = new NpgsqlDataAdapter(query, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvRentals.DataSource = dt;

                    // Скрытие ID если нужно, или форматирование
                    if (dgvRentals.Columns.Contains("RentalID"))
                        dgvRentals.Columns["RentalID"].HeaderText = "ID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupDataGridView()
        {
            dgvRentals.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRentals.MultiSelect = false;
            dgvRentals.ReadOnly = true;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string term = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(term))
            {
                LoadData();
                return;
            }

            try
            {
                string query = @"SELECT r.RentalID, c.FullName AS ""ClientName"", 
                                e.FullName AS ""EmployeeName"", r.StartDate, r.PlannedEndDate, 
                                r.Status, r.TotalCost 
                                FROM Rentals r
                                JOIN Clients c ON r.ClientID = c.ClientID
                                JOIN Employees e ON r.EmployeeID = e.EmployeeID
                                WHERE c.FullName ILIKE @term OR r.Status ILIKE @term
                                ORDER BY r.RentalID DESC";

                using (var conn = new NpgsqlConnection(_connectionString))
                using (var adapter = new NpgsqlDataAdapter(query, conn))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@term", $"%{term}%");
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvRentals.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new RentalEditForm())
            {
                form.IsEditMode = false;
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            // Исправленная проверка
            if (dgvRentals.CurrentRow == null)
            {
                MessageBox.Show("Выберите аренду", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int rentalId = Convert.ToInt32(dgvRentals.CurrentRow.Cells["RentalID"].Value);

            using (var form = new RentalEditForm())
            {
                form.IsEditMode = true;
                form.RentalID = rentalId;
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvRentals.SelectedRows.Count == 0) return;

            int rentalId = Convert.ToInt32(dgvRentals.SelectedRows[0].Cells["RentalID"].Value);

            if (MessageBox.Show("Удалить договор аренды? Это вернет технику в статус 'Свободна'.", "Подтверждение", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    // Удаляем договор. Благодаря ON DELETE CASCADE в RentalItems, позиции удалятся сами.
                    // Триггер trg_UpdateVehicleStatus автоматически вернет статусы техники.
                    string query = "DELETE FROM Rentals WHERE RentalID = @id";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", rentalId);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadData();
                MessageBox.Show("Договор удален.", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}