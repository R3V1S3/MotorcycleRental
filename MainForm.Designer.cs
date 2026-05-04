namespace MotorcycleRental
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tsmiClients = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiVehicles = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCategories = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiEmployees = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRentals = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiReports = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiReportRevenue = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiReportOverdue = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiReportEmployees = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiExit = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tslblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiClients,
            this.tsmiVehicles,
            this.tsmiCategories,
            this.tsmiEmployees,
            this.tsmiRentals,
            this.tsmiReports,
            this.tsmiExit});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tsmiClients
            // 
            this.tsmiClients.Name = "tsmiClients";
            this.tsmiClients.Size = new System.Drawing.Size(64, 20);
            this.tsmiClients.Text = "Клиенты";
            this.tsmiClients.Click += new System.EventHandler(this.tsmiClients_Click);
            // 
            // tsmiVehicles
            // 
            this.tsmiVehicles.Name = "tsmiVehicles";
            this.tsmiVehicles.Size = new System.Drawing.Size(59, 20);
            this.tsmiVehicles.Text = "Техника";
            this.tsmiVehicles.Click += new System.EventHandler(this.tsmiVehicles_Click);
            // 
            // tsmiCategories
            // 
            this.tsmiCategories.Name = "tsmiCategories";
            this.tsmiCategories.Size = new System.Drawing.Size(86, 20);
            this.tsmiCategories.Text = "Категории";
            this.tsmiCategories.Click += new System.EventHandler(this.tsmiCategories_Click);
            // 
            // tsmiEmployees
            // 
            this.tsmiEmployees.Name = "tsmiEmployees";
            this.tsmiEmployees.Size = new System.Drawing.Size(91, 20);
            this.tsmiEmployees.Text = "Сотрудники";
            this.tsmiEmployees.Click += new System.EventHandler(this.tsmiEmployees_Click);
            // 
            // tsmiRentals
            // 
            this.tsmiRentals.Name = "tsmiRentals";
            this.tsmiRentals.Size = new System.Drawing.Size(111, 20);
            this.tsmiRentals.Text = "Договоры (1:М)";
            this.tsmiRentals.Click += new System.EventHandler(this.tsmiRentals_Click);
            // 
            // tsmiReports
            // 
            this.tsmiReports.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiReportRevenue,
            this.tsmiReportOverdue,
            this.tsmiReportEmployees});
            this.tsmiReports.Name = "tsmiReports";
            this.tsmiReports.Size = new System.Drawing.Size(60, 20);
            this.tsmiReports.Text = "Отчёты";
            // 
            // tsmiReportRevenue
            // 
            this.tsmiReportRevenue.Name = "tsmiReportRevenue";
            this.tsmiReportRevenue.Size = new System.Drawing.Size(213, 22);
            this.tsmiReportRevenue.Text = "Выручка по категориям";
            this.tsmiReportRevenue.Click += new System.EventHandler(this.tsmiReportRevenue_Click);
            // 
            // tsmiReportOverdue
            // 
            this.tsmiReportOverdue.Name = "tsmiReportOverdue";
            this.tsmiReportOverdue.Size = new System.Drawing.Size(213, 22);
            this.tsmiReportOverdue.Text = "Просроченные аренды";
            this.tsmiReportOverdue.Click += new System.EventHandler(this.tsmiReportOverdue_Click);
            // 
            // tsmiReportEmployees
            // 
            this.tsmiReportEmployees.Name = "tsmiReportEmployees";
            this.tsmiReportEmployees.Size = new System.Drawing.Size(213, 22);
            this.tsmiReportEmployees.Text = "Эффективность сотрудников";
            this.tsmiReportEmployees.Click += new System.EventHandler(this.tsmiReportEmployees_Click);
            // 
            // tsmiExit
            // 
            this.tsmiExit.Name = "tsmiExit";
            this.tsmiExit.Size = new System.Drawing.Size(53, 20);
            this.tsmiExit.Text = "Выход";
            this.tsmiExit.Click += new System.EventHandler(this.tsmiExit_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tslblStatus
            // 
            this.tslblStatus.Name = "tslblStatus";
            this.tslblStatus.Size = new System.Drawing.Size(118, 17);
            this.tslblStatus.Text = "Статус подключения";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Пункт проката мототехники - Главное меню";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiClients;
        private System.Windows.Forms.ToolStripMenuItem tsmiVehicles;
        private System.Windows.Forms.ToolStripMenuItem tsmiCategories;
        private System.Windows.Forms.ToolStripMenuItem tsmiEmployees;
        private System.Windows.Forms.ToolStripMenuItem tsmiRentals;
        private System.Windows.Forms.ToolStripMenuItem tsmiReports;
        private System.Windows.Forms.ToolStripMenuItem tsmiReportRevenue;
        private System.Windows.Forms.ToolStripMenuItem tsmiReportOverdue;
        private System.Windows.Forms.ToolStripMenuItem tsmiReportEmployees;
        private System.Windows.Forms.ToolStripMenuItem tsmiExit;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tslblStatus;
    }
}