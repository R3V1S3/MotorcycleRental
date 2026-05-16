using MotorcycleRental;
using Npgsql;
using System;
using System.Configuration;
using System.Windows.Forms;

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

        private void btnClients_Click(object sender, EventArgs e)
        {
            new ClientsForm().ShowDialog();
        }

        private void btnVehicles_Click(object sender, EventArgs e)
        {
            new VehiclesForm().ShowDialog();
        }

        private void btnCategories_Click(object sender, EventArgs e)
        {
            new CategoriesForm().ShowDialog();
        }

        private void btnEmployees_Click(object sender, EventArgs e)
        {
            new EmployeesForm().ShowDialog();
        }

        private void btnRentals_Click(object sender, EventArgs e)
        {
            new RentalsForm().ShowDialog();
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            using (var form = new ReportsMenuForm())
            {
                form.ShowDialog();
            }
        }
    }
}