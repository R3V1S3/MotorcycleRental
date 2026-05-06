using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;
using System.Configuration;

namespace MotorcycleRental
{
    public partial class CategoriesForm : Form
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MotorcycleRentalDB"].ConnectionString;
        private DataTable _dataTable;

        public CategoriesForm()
        {
            InitializeComponent();
            LoadData();
            SetupDataGridView(); // Добавляем настройку DataGridView
        }

        private void SetupDataGridView()
        {
            dgvCategories.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCategories.MultiSelect = false;
            dgvCategories.ReadOnly = true;
        }

        private void LoadData()
        {
            try
            {
                string query = "SELECT CategoryID AS \"ID\", Name AS \"Название\", " +
                              "BasePricePerHour AS \"Цена за час\", " +
                              "CASE WHEN IsSeasonal THEN 'Да' ELSE 'Нет' END AS \"Сезонная\" " +
                              "FROM VehicleCategories ORDER BY Name";

                using (var conn = new NpgsqlConnection(_connectionString))
                using (var adapter = new NpgsqlDataAdapter(query, conn))
                {
                    _dataTable = new DataTable();
                    adapter.Fill(_dataTable);
                }

                dgvCategories.DataSource = _dataTable;
                lblCount.Text = $"Записей: {_dataTable.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ✅ Исправленный метод редактирования
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvCategories.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись для редактирования", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = (DataRowView)dgvCategories.CurrentRow.DataBoundItem;

            using (var form = new CategoryEditForm())
            {
                form.IsEditMode = true;
                form.CategoryData = new CategoryData
                {
                    CategoryID = Convert.ToInt32(row["ID"]),
                    Name = row["Название"].ToString(),
                    BasePricePerHour = Convert.ToDecimal(row["Цена за час"]),
                    IsSeasonal = row["Сезонная"].ToString() == "Да"
                };

                if (form.ShowDialog() == DialogResult.OK)
                {
                    SaveCategory(form.CategoryData);
                    LoadData();
                }
            }
        }

        // ✅ Методы навигации
        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (dgvCategories.CurrentRow?.Index > 0)
            {
                dgvCategories.CurrentCell = dgvCategories.Rows[dgvCategories.CurrentRow.Index - 1].Cells[0];
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (dgvCategories.CurrentRow?.Index < dgvCategories.Rows.Count - 1)
            {
                dgvCategories.CurrentCell = dgvCategories.Rows[dgvCategories.CurrentRow.Index + 1].Cells[0];
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new CategoryEditForm())
            {
                form.IsEditMode = false;
                if (form.ShowDialog() == DialogResult.OK)
                {
                    SaveCategory(form.CategoryData);
                    LoadData();
                }
            }
        }

        private void SaveCategory(CategoryData data)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = data.CategoryID == 0
                        ? "INSERT INTO VehicleCategories (Name, BasePricePerHour, IsSeasonal) VALUES (@name, @price, @seasonal)"
                        : "UPDATE VehicleCategories SET Name=@name, BasePricePerHour=@price, IsSeasonal=@seasonal WHERE CategoryID=@id";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", data.Name);
                        cmd.Parameters.AddWithValue("@price", data.BasePricePerHour);
                        cmd.Parameters.AddWithValue("@seasonal", data.IsSeasonal);
                        if (data.CategoryID != 0)
                            cmd.Parameters.AddWithValue("@id", data.CategoryID);

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
            if (dgvCategories.CurrentRow == null)
            {
                MessageBox.Show("Выберите запись для удаления");
                return;
            }

            var row = (DataRowView)dgvCategories.CurrentRow.DataBoundItem;
            int id = Convert.ToInt32(row["ID"]);

            if (MessageBox.Show("Удалить категорию?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("DELETE FROM VehicleCategories WHERE CategoryID=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadData();
                MessageBox.Show("Категория удалена", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\nВозможно, категория используется в технике.", "Ошибка");
            }
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }

    public class CategoryData
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public decimal BasePricePerHour { get; set; }
        public bool IsSeasonal { get; set; }
    }
}