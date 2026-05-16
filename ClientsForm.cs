using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;
using System.Configuration;

namespace MotorcycleRental
{
    public partial class ClientsForm : Form
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["MotorcycleRentalDB"].ConnectionString;

        private DataTable _dataTable;

        public ClientsForm()
        {
            InitializeComponent();
            LoadData();
            SetupDataGridView();
        }

        private void LoadData()
        {
            try
            {
                string query = "SELECT ClientID AS \"ID\", FullName AS \"ФИО\", Phone AS \"Телефон\", " +
                              "PassportSeries AS \"Серия паспорта\", PassportNumber AS \"Номер паспорта\", " +
                              "Address AS \"Адрес\" FROM Clients ORDER BY FullName";

                using (var conn = new NpgsqlConnection(_connectionString))
                using (var adapter = new NpgsqlDataAdapter(query, conn))
                {
                    _dataTable = new DataTable();
                    adapter.Fill(_dataTable);
                }

                dgvClients.DataSource = _dataTable;
                lblRecordCount.Text = $"Записей: {_dataTable.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupDataGridView()
        {
            dgvClients.AutoGenerateColumns = true;
            dgvClients.ReadOnly = true;
            dgvClients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClients.MultiSelect = false;
            dgvClients.AllowUserToAddRows = false;
            dgvClients.AllowUserToDeleteRows = false;
            dgvClients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        }

        private void dgvClients_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0) return;

            string columnName = dgvClients.Columns[e.ColumnIndex].DataPropertyName;
            if (string.IsNullOrEmpty(columnName)) return;

            string dbColumnName = GetSortDbFieldName(columnName);
            if (string.IsNullOrEmpty(dbColumnName)) return;

            string direction = "ASC";
            if (dgvClients.Tag?.ToString() == dbColumnName + "_ASC")
            {
                direction = "DESC";
                dgvClients.Tag = dbColumnName + "_DESC";
            }
            else
            {
                dgvClients.Tag = dbColumnName + "_ASC";
            }

            SortData(dbColumnName, direction);
        }

        private void SortData(string columnName, string direction)
        {
            try
            {
                string query = $"SELECT ClientID AS \"ID\", FullName AS \"ФИО\", Phone AS \"Телефон\", " +
                              "PassportSeries AS \"Серия паспорта\", PassportNumber AS \"Номер паспорта\", " +
                              $"Address AS \"Адрес\" FROM Clients ORDER BY {columnName} {direction}";

                using (var conn = new NpgsqlConnection(_connectionString))
                using (var adapter = new NpgsqlDataAdapter(query, conn))
                {
                    DataTable tempTable = new DataTable();
                    adapter.Fill(tempTable);
                    dgvClients.DataSource = tempTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сортировки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim();
            string searchField = cmbSearchField.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(searchTerm))
            {
                LoadData();
                return;
            }

            try
            {
                string dbFieldName = GetDbFieldName(searchField);
                string query = $"SELECT ClientID AS \"ID\", FullName AS \"ФИО\", Phone AS \"Телефон\", " +
                              "PassportSeries AS \"Серия паспорта\", PassportNumber AS \"Номер паспорта\", " +
                              $"Address AS \"Адрес\" FROM Clients " +
                              $"WHERE {dbFieldName} ILIKE @searchTerm ORDER BY FullName";

                using (var conn = new NpgsqlConnection(_connectionString))
                using (var adapter = new NpgsqlDataAdapter(query, conn))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@searchTerm", $"%{searchTerm}%");

                    DataTable resultTable = new DataTable();
                    adapter.Fill(resultTable);

                    dgvClients.DataSource = resultTable;
                    lblRecordCount.Text = $"Найдено: {resultTable.Rows.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetDbFieldName(string displayName)
        {
            switch (displayName)
            {
                case "ФИО": return "FullName";
                case "Телефон": return "Phone";
                case "Серия паспорта": return "PassportSeries";
                case "Номер паспорта": return "PassportNumber";
                case "Адрес": return "Address";
                default: return "FullName";
            }
        }

        private string GetSortDbFieldName(string displayName)
        {
            switch (displayName)
            {
                case "ID": return "ClientID";
                case "ФИО": return "FullName";
                case "Телефон": return "Phone";
                case "Серия паспорта": return "PassportSeries";
                case "Номер паспорта": return "PassportNumber";
                case "Адрес": return "Address";
                default: return null;
            }
        }

        private void btnResetSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            cmbSearchField.SelectedIndex = 0;
            LoadData();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new ClientEditForm())
            {
                form.IsEditMode = false;
                if (form.ShowDialog() == DialogResult.OK)
                {
                    SaveClient(form.ClientData);
                    LoadData();
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvClients.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для редактирования", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = (DataRowView)dgvClients.SelectedRows[0].DataBoundItem;

            using (var form = new ClientEditForm())
            {
                form.IsEditMode = true;
                form.ClientData = new ClientData
                {
                    ClientID = Convert.ToInt32(row["ID"]),
                    FullName = row["ФИО"].ToString(),
                    Phone = row["Телефон"].ToString(),
                    PassportSeries = row["Серия паспорта"].ToString(),
                    PassportNumber = row["Номер паспорта"].ToString(),
                    Address = row["Адрес"]?.ToString()
                };

                if (form.ShowDialog() == DialogResult.OK)
                {
                    SaveClient(form.ClientData);
                    LoadData();
                }
            }
        }

        private void SaveClient(ClientData data)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = data.ClientID == 0
                        ? "INSERT INTO Clients (FullName, Phone, PassportSeries, PassportNumber, Address) VALUES (@FullName, @Phone, @PassportSeries, @PassportNumber, @Address)"
                        : "UPDATE Clients SET FullName=@FullName, Phone=@Phone, PassportSeries=@PassportSeries, PassportNumber=@PassportNumber, Address=@Address WHERE ClientID=@ClientID";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FullName", data.FullName);
                        cmd.Parameters.AddWithValue("@Phone", data.Phone);
                        cmd.Parameters.AddWithValue("@PassportSeries", data.PassportSeries);
                        cmd.Parameters.AddWithValue("@PassportNumber", data.PassportNumber);
                        cmd.Parameters.AddWithValue("@Address", (object)data.Address ?? DBNull.Value);

                        if (data.ClientID != 0)
                            cmd.Parameters.AddWithValue("@ClientID", data.ClientID);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvClients.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись для удаления", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = (DataRowView)dgvClients.SelectedRows[0].DataBoundItem;
            int clientId = Convert.ToInt32(row["ID"]);
            string clientName = row["ФИО"].ToString();

            if (MessageBox.Show($"Удалить клиента \"{clientName}\"?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Rentals WHERE ClientID=@ClientID AND Status='Активен'";
                    using (var checkCmd = new NpgsqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@ClientID", clientId);
                        int activeRentals = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (activeRentals > 0)
                        {
                            MessageBox.Show("Невозможно удалить клиента с активными арендами", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    string deleteQuery = "DELETE FROM Clients WHERE ClientID=@ClientID";
                    using (var cmd = new NpgsqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientID", clientId);
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadData();
                MessageBox.Show("Клиент успешно удалён", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (dgvClients.CurrentRow?.Index > 0)
                dgvClients.CurrentCell = dgvClients.Rows[dgvClients.CurrentRow.Index - 1].Cells[0];
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (dgvClients.CurrentRow?.Index < dgvClients.Rows.Count - 1)
                dgvClients.CurrentCell = dgvClients.Rows[dgvClients.CurrentRow.Index + 1].Cells[0];
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }

    public class ClientData
    {
        public int ClientID { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string PassportSeries { get; set; }
        public string PassportNumber { get; set; }
        public string Address { get; set; }
    }
}