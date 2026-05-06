using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;
using System.Configuration;

namespace MotorcycleRental
{
    public partial class VehicleEditForm : Form
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MotorcycleRentalDB"].ConnectionString;

        public bool IsEditMode { get; set; }
        public VehicleData VehicleData { get; set; } = new VehicleData();

        public VehicleEditForm()
        {
            InitializeComponent();
        }

        private void VehicleEditForm_Load(object sender, EventArgs e)
        {
            this.Text = IsEditMode ? "Редактирование техники" : "Добавление техники";
            LoadCategories();

            if (IsEditMode)
            {
                txtModel.Text = VehicleData.Model;
                txtPlate.Text = VehicleData.PlateNumber;
                numPower.Value = VehicleData.Power;
                cmbStatus.SelectedItem = VehicleData.Status;
                SelectCategory(VehicleData.CategoryID);
            }
            else
            {
                cmbStatus.SelectedItem = "Свободна";
            }
        }

        private void LoadCategories()
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT CategoryID, Name FROM VehicleCategories ORDER BY Name", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbCategory.Items.Add(new ComboBoxItem(reader.GetInt32(0), reader.GetString(1)));
                        }
                    }
                }
                if (cmbCategory.Items.Count > 0) cmbCategory.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
            }
        }

        private void SelectCategory(int? categoryId)
        {
            if (!categoryId.HasValue) return;
            foreach (ComboBoxItem item in cmbCategory.Items)
            {
                if (item.Id == categoryId.Value)
                {
                    cmbCategory.SelectedItem = item;
                    return;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtModel.Text) || string.IsNullOrWhiteSpace(txtPlate.Text))
            {
                MessageBox.Show("Заполните обязательные поля");
                return;
            }

            var catItem = cmbCategory.SelectedItem as ComboBoxItem;

            VehicleData.Model = txtModel.Text.Trim();
            VehicleData.PlateNumber = txtPlate.Text.Trim().ToUpper();
            VehicleData.Power = (int)numPower.Value;
            VehicleData.CategoryID = catItem?.Id;
            VehicleData.Status = cmbStatus.SelectedItem?.ToString() ?? "Свободна";

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) => DialogResult = DialogResult.Cancel;
    }

    public class ComboBoxItem
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public ComboBoxItem(int id, string text) { Id = id; Text = text; }
        public override string ToString() => Text;
    }
}