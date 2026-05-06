using System;
using System.Windows.Forms;
using System.Xml.Linq;

namespace MotorcycleRental
{
    public partial class CategoryEditForm : Form
    {
        public bool IsEditMode { get; set; }
        public CategoryData CategoryData { get; set; } = new CategoryData();

        public CategoryEditForm()
        {
            InitializeComponent();
        }

        private void CategoryEditForm_Load(object sender, EventArgs e)
        {
            this.Text = IsEditMode ? "Редактирование категории" : "Добавление категории";
            if (IsEditMode)
            {
                txtName.Text = CategoryData.Name;
                numPrice.Value = CategoryData.BasePricePerHour;
                chkSeasonal.Checked = CategoryData.IsSeasonal;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название");
                return;
            }

            CategoryData.Name = txtName.Text.Trim();
            CategoryData.BasePricePerHour = numPrice.Value;
            CategoryData.IsSeasonal = chkSeasonal.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) => DialogResult = DialogResult.Cancel;
    }
}