using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;
using System.Configuration;

namespace MotorcycleRental
{
    public partial class ReportRevenueForm : Form
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MotorcycleRentalDB"].ConnectionString;

        public ReportRevenueForm()
        {
            InitializeComponent();
            // Установим даты по умолчанию (текущий месяц)
            dtpStart.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dtpEnd.Value = DateTime.Now;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                string query = @"
                    SELECT vc.Name AS ""Категория"", 
                           COUNT(ri.ItemID) AS ""Кол-во аренд"", 
                           SUM(ri.Cost) AS ""Выручка""
                    FROM VehicleCategories vc
                    JOIN Vehicles v ON vc.CategoryID = v.CategoryID
                    JOIN RentalItems ri ON v.VehicleID = ri.VehicleID
                    JOIN Rentals r ON ri.RentalID = r.RentalID
                    WHERE r.StartDate >= @start AND r.StartDate <= @end
                    GROUP BY vc.Name
                    ORDER BY ""Выручка"" DESC";

                using (var conn = new NpgsqlConnection(_connectionString))
                using (var adapter = new NpgsqlDataAdapter(query, conn))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@start", dtpStart.Value);
                    adapter.SelectCommand.Parameters.AddWithValue("@end", dtpEnd.Value);

                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvReport.DataSource = dt;

                    // Расчет итогов
                    decimal totalRevenue = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        totalRevenue += Convert.ToDecimal(row["Выручка"]);
                    }
                    lblTotal.Text = $"ИТОГО выручка за период: {totalRevenue:N2} руб.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}