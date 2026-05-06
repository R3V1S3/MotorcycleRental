using System;
using System.Windows.Forms;

namespace MotorcycleRental
{
    public partial class ReportsMenuForm : Form
    {
        public ReportsMenuForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void btnRevenue_Click(object sender, EventArgs e)
        {
            new ReportRevenueForm().ShowDialog();
        }

        private void btnEmployees_Click(object sender, EventArgs e)
        {
            new ReportEmployeesForm().ShowDialog();
        }

        private void btnClients_Click(object sender, EventArgs e)
        {
            new ReportClientsForm().ShowDialog();
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}