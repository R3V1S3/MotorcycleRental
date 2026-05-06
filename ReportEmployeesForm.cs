using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;
using System.Configuration;

namespace MotorcycleRental
{
    public partial class ReportEmployeesForm : Form
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MotorcycleRentalDB"].ConnectionString;

        public ReportEmployeesForm()
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
                    SELECT e.FullName AS ""Сотрудник"", 
                           COUNT(r.RentalID) AS ""Договоров"", 
                           SUM(r.TotalCost) AS ""Сумма"",
                           AVG(r.TotalCost) AS ""Средний чек""
                    FROM Employees e
                    JOIN Rentals r ON e.EmployeeID = r.EmployeeID
                    WHERE r.StartDate >= @start AND r.StartDate <= @end
                    GROUP BY e.FullName
                    ORDER BY ""Сумма"" DESC";

                using (var conn = new NpgsqlConnection(_connectionString))
                using (var adapter = new NpgsqlDataAdapter(query, conn))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@start", dtpStart.Value);
                    adapter.SelectCommand.Parameters.AddWithValue("@end", dtpEnd.Value);

                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvReport.DataSource = dt;

                    // Итого
                    int totalContracts = 0;
                    decimal totalSum = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        totalContracts += Convert.ToInt32(row["Договоров"]);
                        totalSum += Convert.ToDecimal(row["Сумма"]);
                    }
                    lblTotal.Text = $"ИТОГО: {totalContracts} договоров на сумму {totalSum:N2} руб.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}