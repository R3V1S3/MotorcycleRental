using System;
using System.Windows.Forms;

namespace MotorcycleRental
{
    public partial class EmployeeEditForm : Form
    {
        public bool IsEditMode { get; set; }
        public EmployeeData EmployeeData { get; set; } = new EmployeeData();

        public EmployeeEditForm()
        {
            InitializeComponent();
        }

        private void EmployeeEditForm_Load(object sender, EventArgs e)
        {
            this.Text = IsEditMode ? "Редактирование сотрудника" : "Добавление сотрудника";
            if (IsEditMode)
            {
                txtFullName.Text = EmployeeData.FullName;
                txtLogin.Text = EmployeeData.Login;
                cmbRole.SelectedItem = EmployeeData.Role;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                MessageBox.Show("Заполните все обязательные поля");
                return;
            }

            EmployeeData.FullName = txtFullName.Text.Trim();
            EmployeeData.Login = txtLogin.Text.Trim();
            EmployeeData.Role = cmbRole.SelectedItem?.ToString() ?? "Менеджер";

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) => DialogResult = DialogResult.Cancel;
    }
}