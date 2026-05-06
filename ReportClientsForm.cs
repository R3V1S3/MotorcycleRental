using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;
using System.Configuration;

namespace MotorcycleRental
{
    public partial class ReportClientsForm : Form
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MotorcycleRentalDB"].ConnectionString;

        public ReportClientsForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterParent;
            dtpStart.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dtpEnd.Value = DateTime.Now;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                string query = @"
                    SELECT c.FullName AS ""Клиент"", 
                           COUNT(r.RentalID) AS ""Всего аренд"", 
                           SUM(r.TotalCost) AS ""Потрачено"",
                           MAX(r.StartDate) AS ""Последняя аренда""
                    FROM Clients c
                    JOIN Rentals r ON c.ClientID = r.ClientID
                    WHERE r.StartDate >= @start AND r.StartDate <= @end
                    GROUP BY c.FullName
                    ORDER BY ""Потрачено"" DESC";

                using (var conn = new NpgsqlConnection(_connectionString))
                using (var adapter = new NpgsqlDataAdapter(query, conn))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@start", dtpStart.Value);
                    adapter.SelectCommand.Parameters.AddWithValue("@end", dtpEnd.Value);

                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvReport.DataSource = dt;

                    // Итого
                    decimal totalSpent = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        totalSpent += Convert.ToDecimal(row["Потрачено"]);
                    }
                    lblTotal.Text = $"ОБЩАЯ СУММА всех клиентов: {totalSpent:N2} руб.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}