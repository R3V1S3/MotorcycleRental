using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MotorcycleRental // ✅ Исправлено
{
    public partial class ClientEditForm : Form
    {
        public bool IsEditMode { get; set; }
        public ClientData ClientData { get; set; } = new ClientData();

        public ClientEditForm()
        {
            InitializeComponent();
        }

        private void ClientEditForm_Load(object sender, EventArgs e)
        {
            this.Text = IsEditMode ? "Редактирование клиента" : "Добавление клиента";

            if (IsEditMode && ClientData != null)
            {
                txtFullName.Text = ClientData.FullName;
                txtPhone.Text = ClientData.Phone;
                txtPassportSeries.Text = ClientData.PassportSeries;
                txtPassportNumber.Text = ClientData.PassportNumber;
                txtAddress.Text = ClientData.Address;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Введите ФИО", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }
            if (!Regex.IsMatch(txtPhone.Text, @"^\+[0-9]{11}$"))
            {
                MessageBox.Show("Телефон должен быть в формате +79001234567", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return;
            }

            ClientData.FullName = txtFullName.Text.Trim();
            ClientData.Phone = txtPhone.Text.Trim();
            ClientData.PassportSeries = txtPassportSeries.Text.Trim();
            ClientData.PassportNumber = txtPassportNumber.Text.Trim();
            ClientData.Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) => DialogResult = DialogResult.Cancel;
    }
}