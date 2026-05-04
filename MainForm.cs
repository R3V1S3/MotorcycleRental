using System;
using System.Windows.Forms;
using System.Configuration;
using Npgsql;

namespace MotorcycleRental
{
    public partial class MainForm : Form
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["MotorcycleRentalDB"].ConnectionString;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            TestDatabaseConnection();
        }

        private void TestDatabaseConnection()
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand("CALL usp_UpdateSeasonalStatus();", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    tslblStatus.Text = "✅ БД подключена. Сезон обновлен.";
                    tslblStatus.ForeColor = System.Drawing.Color.DarkGreen;
                }
            }
            catch (Exception ex)
            {
                tslblStatus.Text = "❌ Ошибка подключения";
                tslblStatus.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show($"Ошибка подключения к БД:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // --- Все обработчики меню пока выводят заглушки ---

        private void tsmiClients_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Раздел 'Клиенты' будет реализован следующим шагом",
                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void tsmiVehicles_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Раздел 'Техника' будет реализован позже",
                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void tsmiCategories_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Раздел 'Категории' будет реализован позже",
                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void tsmiEmployees_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Раздел 'Сотрудники' будет реализован позже",
                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void tsmiRentals_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Раздел 'Аренда (1:М)' будет реализован позже",
                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void tsmiReportRevenue_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Отчёт 'Выручка' будет реализован позже",
                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void tsmiReportOverdue_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Отчёт 'Просрочки' будет реализован позже",
                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void tsmiReportEmployees_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Отчёт 'Сотрудники' будет реализован позже",
                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}