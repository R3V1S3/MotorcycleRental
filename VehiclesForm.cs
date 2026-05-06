using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;
using System.Configuration;

namespace MotorcycleRental
{
    public partial class VehiclesForm : Form
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MotorcycleRentalDB"].ConnectionString;
        private DataTable _dataTable;

        public VehiclesForm()
        {
            InitializeComponent();
            LoadData();
            SetupDataGridView();
        }

        private void SetupDataGridView()
        {
            dgvVehicles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvVehicles.MultiSelect = false;
            dgvVehicles.ReadOnly = true;
        }

        private void LoadData()
        {
            try
            {
                string query = @"SELECT v.VehicleID AS ""ID"", v.Model AS ""Модель"", 
                                v.PlateNumber AS ""Госномер"", v.Power AS ""Мощность (л.с.)"",
                                vc.Name AS ""Категория"", v.Status AS ""Статус""
                                FROM Vehicles v
                                LEFT JOIN VehicleCategories vc ON v.CategoryID = vc.CategoryID
                                ORDER BY v.Model";

                using (var conn = new NpgsqlConnection(_connectionString))
                using (var adapter = new NpgsqlDataAdapter(query, conn))
                {
                    _dataTable = new DataTable();
                    adapter.Fill(_dataTable);
                }

                dgvVehicles.DataSource = _dataTable;
                lblCount.Text = $"Записей: {_dataTable.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new VehicleEditForm())
            {
                form.IsEditMode = false;
                if (form.ShowDialog() == DialogResult.OK)
                {
                    SaveVehicle(form.VehicleData);
                    LoadData();
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvVehicles.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись для редактирования", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = (DataRowView)dgvVehicles.CurrentRow.DataBoundItem;

            using (var form = new VehicleEditForm())
            {
                form.IsEditMode = true;
                form.VehicleData = new VehicleData
                {
                    VehicleID = Convert.ToInt32(row["ID"]),
                    Model = row["Модель"].ToString(),
                    PlateNumber = row["Госномер"].ToString(),
                    Power = Convert.ToInt32(row["Мощность (л.с.)"]),
                    Status = row["Статус"].ToString()
                };

                if (form.ShowDialog() == DialogResult.OK)
                {
                    SaveVehicle(form.VehicleData);
                    LoadData();
                }
            }
        }

        private void SaveVehicle(VehicleData data)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = data.VehicleID == 0
                        ? "INSERT INTO Vehicles (Model, PlateNumber, Power, CategoryID, Status) VALUES (@model, @plate, @power, @catid, @status)"
                        : "UPDATE Vehicles SET Model=@model, PlateNumber=@plate, Power=@power, CategoryID=@catid, Status=@status WHERE VehicleID=@id";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@model", data.Model);
                        cmd.Parameters.AddWithValue("@plate", data.PlateNumber);
                        cmd.Parameters.AddWithValue("@power", data.Power);
                        cmd.Parameters.AddWithValue("@catid", (object)data.CategoryID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@status", data.Status);
                        if (data.VehicleID != 0)
                            cmd.Parameters.AddWithValue("@id", data.VehicleID);

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
            if (dgvVehicles.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись для удаления", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = (DataRowView)dgvVehicles.CurrentRow.DataBoundItem;
            int id = Convert.ToInt32(row["ID"]);
            string status = row["Статус"].ToString();

            if (status == "В аренде")
            {
                MessageBox.Show("Нельзя удалить технику, которая находится в аренде",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Удалить технику?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("DELETE FROM Vehicles WHERE VehicleID=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadData();
                MessageBox.Show("Техника удалена", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (dgvVehicles.CurrentRow?.Index > 0)
            {
                dgvVehicles.CurrentCell = dgvVehicles.Rows[dgvVehicles.CurrentRow.Index - 1].Cells[0];
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (dgvVehicles.CurrentRow?.Index < dgvVehicles.Rows.Count - 1)
            {
                dgvVehicles.CurrentCell = dgvVehicles.Rows[dgvVehicles.CurrentRow.Index + 1].Cells[0];
            }
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }

    public class VehicleData
    {
        public int VehicleID { get; set; }
        public string Model { get; set; }
        public string PlateNumber { get; set; }
        public int Power { get; set; }
        public int? CategoryID { get; set; }
        public string Status { get; set; }
    }
}