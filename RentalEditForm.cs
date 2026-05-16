using Npgsql;
using System;
using System.Configuration;
using System.Data;
using System.Net;
using System.Windows.Forms;

namespace MotorcycleRental
{
    public partial class RentalEditForm : Form
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["MotorcycleRentalDB"].ConnectionString;

        public bool IsEditMode { get; set; }
        public int RentalID { get; set; }

        private DataTable _itemsTable;

        public RentalEditForm()
        {
            InitializeComponent();
        }

        private void RentalEditForm_Load(object sender, EventArgs e)
        {
            LoadComboBoxes();

            if (IsEditMode)
            {
                LoadExistingRental();
            }
            else
            {
                dtpStartDate.Value = DateTime.Now.Date;
                dtpEndDate.Value = DateTime.Now.AddDays(1).Date;

                cmbStatus.SelectedItem = "Активен";
                CreateNewItemsTable();
            }
        }

        private void CreateNewItemsTable()
        {
            _itemsTable = new DataTable();
            _itemsTable.Columns.Add("VehicleID", typeof(int));
            _itemsTable.Columns.Add("Model", typeof(string));
            _itemsTable.Columns.Add("PricePerHour", typeof(decimal));
            _itemsTable.Columns.Add("Hours", typeof(int));
            _itemsTable.Columns.Add("Total", typeof(decimal), "PricePerHour * Hours");

            dgvItems.DataSource = _itemsTable;
        }

        private void LoadComboBoxes()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                string qClients = "SELECT ClientID, FullName FROM Clients ORDER BY FullName";
                using (var cmd = new NpgsqlCommand(qClients, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cmbClient.Items.Add(new ComboBoxItem(reader.GetInt32(0), reader.GetString(1)));
                    }
                }

                string qEmp = "SELECT EmployeeID, FullName FROM Employees ORDER BY FullName";
                using (var cmd = new NpgsqlCommand(qEmp, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cmbEmployee.Items.Add(new ComboBoxItem(reader.GetInt32(0), reader.GetString(1)));
                    }
                }

                string qVehicles = @"SELECT v.VehicleID, v.Model || ' (' || v.PlateNumber || ')' as Info, c.BasePricePerHour 
                                     FROM Vehicles v 
                                     JOIN VehicleCategories c ON v.CategoryID = c.CategoryID 
                                     WHERE v.Status = 'Свободна'";

                using (var cmd = new NpgsqlCommand(qVehicles, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cboFreeVehicles.Items.Add(new VehicleItem(reader.GetInt32(0), reader.GetString(1), reader.GetDecimal(2)));
                    }
                }
            }

            if (cmbClient.Items.Count > 0) cmbClient.SelectedIndex = 0;
            if (cmbEmployee.Items.Count > 0) cmbEmployee.SelectedIndex = 0;
        }

        private void LoadExistingRental()
        {
            string qHeader = "SELECT ClientID, EmployeeID, StartDate, PlannedEndDate, Status FROM Rentals WHERE RentalID = @id";

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(qHeader, conn))
                {
                    cmd.Parameters.AddWithValue("@id", RentalID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            SelectComboBoxItem(cmbClient, reader.GetInt32(0));
                            SelectComboBoxItem(cmbEmployee, reader.GetInt32(1));
                            dtpStartDate.Value = reader.GetDateTime(2);
                            dtpEndDate.Value = reader.GetDateTime(3);
                            cmbStatus.SelectedItem = reader.GetString(4);
                        }
                    }
                }

                string qItems = @"SELECT ri.VehicleID, v.Model, c.BasePricePerHour, ri.HoursCount 
                                  FROM RentalItems ri 
                                  JOIN Vehicles v ON ri.VehicleID = v.VehicleID 
                                  JOIN VehicleCategories c ON v.CategoryID = c.CategoryID 
                                  WHERE ri.RentalID = @id";

                _itemsTable = new DataTable();
                _itemsTable.Columns.Add("VehicleID", typeof(int));
                _itemsTable.Columns.Add("Model", typeof(string));
                _itemsTable.Columns.Add("PricePerHour", typeof(decimal));
                _itemsTable.Columns.Add("Hours", typeof(int));
                _itemsTable.Columns.Add("Total", typeof(decimal), "PricePerHour * Hours");

                using (var cmd = new NpgsqlCommand(qItems, conn))
                {
                    cmd.Parameters.AddWithValue("@id", RentalID);
                    using (var adapter = new NpgsqlDataAdapter(cmd))
                    {
                        adapter.Fill(_itemsTable);
                    }
                }
                dgvItems.DataSource = _itemsTable;
            }
        }

        private void SelectComboBoxItem(ComboBox cmb, int id)
        {
            foreach (ComboBoxItem item in cmb.Items)
            {
                if (item.Id == id)
                {
                    cmb.SelectedItem = item;
                    return;
                }
            }
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            if (cboFreeVehicles.SelectedItem == null)
            {
                MessageBox.Show("Выберите технику");
                return;
            }

            var vehicle = (VehicleItem)cboFreeVehicles.SelectedItem;
            int hours = (int)numHours.Value;

            if (hours <= 0)
            {
                MessageBox.Show("Часы должны быть больше 0");
                return;
            }

            DataRow row = _itemsTable.NewRow();
            row["VehicleID"] = vehicle.Id;
            row["Model"] = vehicle.Info;
            row["PricePerHour"] = vehicle.Price;
            row["Hours"] = hours;

            _itemsTable.Rows.Add(row);
        }

        private void btnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvItems.CurrentRow != null)
            {
                dgvItems.Rows.Remove(dgvItems.CurrentRow);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_itemsTable.Rows.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одну единицу техники", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var client = (ComboBoxItem)cmbClient.SelectedItem;
            var emp = (ComboBoxItem)cmbEmployee.SelectedItem;

            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        int newRentalId = 0;

                        string qInsRent = @"INSERT INTO Rentals (ClientID, EmployeeID, StartDate, PlannedEndDate, Status) 
                                            VALUES (@cid, @eid, @start, @end, @status) RETURNING RentalID";

                        string status = cmbStatus.SelectedItem?.ToString() ?? "Активен";

                        using (var cmd = new NpgsqlCommand(qInsRent, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@cid", client.Id);
                            cmd.Parameters.AddWithValue("@eid", emp.Id);
                            cmd.Parameters.AddWithValue("@start", dtpStartDate.Value);
                            cmd.Parameters.AddWithValue("@end", dtpEndDate.Value);
                            cmd.Parameters.AddWithValue("@status", status);

                            newRentalId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        string qInsItem = @"INSERT INTO RentalItems (RentalID, VehicleID, HoursCount, Cost) 
                                            VALUES (@rid, @vid, @hours, @cost)";

                        foreach (DataRow row in _itemsTable.Rows)
                        {
                            decimal cost = (decimal)row["Total"];
                            int vid = (int)row["VehicleID"];
                            int hours = (int)row["Hours"];

                            using (var cmd = new NpgsqlCommand(qInsItem, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@rid", newRentalId);
                                cmd.Parameters.AddWithValue("@vid", vid);
                                cmd.Parameters.AddWithValue("@hours", hours);
                                cmd.Parameters.AddWithValue("@cost", cost);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        if (IsEditMode)
                        {
                            string qDelOld = "DELETE FROM RentalItems WHERE RentalID = @rid";
                            using (var cmd = new NpgsqlCommand(qDelOld, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@rid", RentalID);
                                cmd.ExecuteNonQuery();
                            }

                            string qUpdRent = "UPDATE Rentals SET ClientID=@cid, EmployeeID=@eid, StartDate=@start, PlannedEndDate=@end, Status=@status WHERE RentalID=@rid";
                            using (var cmd = new NpgsqlCommand(qUpdRent, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@cid", client.Id);
                                cmd.Parameters.AddWithValue("@eid", emp.Id);
                                cmd.Parameters.AddWithValue("@start", dtpStartDate.Value);
                                cmd.Parameters.AddWithValue("@end", dtpEndDate.Value);
                                cmd.Parameters.AddWithValue("@status", cmbStatus.SelectedItem.ToString());
                                cmd.Parameters.AddWithValue("@rid", RentalID);
                                cmd.ExecuteNonQuery();
                            }

                            foreach (DataRow row in _itemsTable.Rows)
                            {
                                decimal cost = (decimal)row["Total"];
                                int vid = (int)row["VehicleID"];
                                int hours = (int)row["Hours"];

                                using (var cmd = new NpgsqlCommand(qInsItem, conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@rid", RentalID);
                                    cmd.Parameters.AddWithValue("@vid", vid);
                                    cmd.Parameters.AddWithValue("@hours", hours);
                                    cmd.Parameters.AddWithValue("@cost", cost);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        trans.Commit();
                    }
                }

                MessageBox.Show("Сохранено успешно!", "Успех");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e) => DialogResult = DialogResult.Cancel;

        public class ComboBoxItem
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public ComboBoxItem(int id, string text) { Id = id; Text = text; }
            public override string ToString() => Text;
        }

        public class VehicleItem
        {
            public int Id { get; set; }
            public string Info { get; set; }
            public decimal Price { get; set; }
            public VehicleItem(int id, string info, decimal price) { Id = id; Info = info; Price = price; }
            public override string ToString() => $"{Info} - {Price} руб/час";
        }
    }
}