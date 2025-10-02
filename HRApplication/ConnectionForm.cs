using System;
using System.Drawing;
using System.Windows.Forms;

namespace HRApplication
{
    public partial class ConnectionForm : Form
    {
        private Database.DatabaseHelper databaseHelper;

        private Label lblHost, lblPort, lblDatabase, lblUsername, lblPassword;
        private TextBox txtHost, txtPort, txtDatabase, txtUsername, txtPassword;
        private Button btnConnect, btnCancel;

        public ConnectionForm()
        {
            InitializeComponent();
            databaseHelper = new Database.DatabaseHelper();

            // Установка значений по умолчанию
            txtHost.Text = "localhost";
            txtPort.Text = "5432";
            txtDatabase.Text = "hr_management";
            txtUsername.Text = "postgres";
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Host
            lblHost = new Label { Text = "Host:", Location = new Point(20, 20), Size = new Size(80, 20) };
            txtHost = new TextBox { Location = new Point(100, 20), Size = new Size(200, 20) };

            // Port
            lblPort = new Label { Text = "Port:", Location = new Point(20, 50), Size = new Size(80, 20) };
            txtPort = new TextBox { Location = new Point(100, 50), Size = new Size(200, 20) };

            // Database
            lblDatabase = new Label { Text = "Database:", Location = new Point(20, 80), Size = new Size(80, 20) };
            txtDatabase = new TextBox { Location = new Point(100, 80), Size = new Size(200, 20) };

            // Username
            lblUsername = new Label { Text = "Username:", Location = new Point(20, 110), Size = new Size(80, 20) };
            txtUsername = new TextBox { Location = new Point(100, 110), Size = new Size(200, 20) };

            // Password
            lblPassword = new Label { Text = "Password:", Location = new Point(20, 140), Size = new Size(80, 20) };
            txtPassword = new TextBox { Location = new Point(100, 140), Size = new Size(200, 20) };
            txtPassword.PasswordChar = '*';

            // Buttons
            btnConnect = new Button { Text = "Подключиться", Location = new Point(100, 180), Size = new Size(95, 30) };
            btnConnect.Click += BtnConnect_Click;

            btnCancel = new Button { Text = "Отмена", Location = new Point(205, 180), Size = new Size(95, 30) };
            btnCancel.Click += (s, e) => System.Windows.Forms.Application.Exit();

            // Form
            this.Text = "Подключение к базе данных";
            this.ClientSize = new Size(320, 230);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Добавление контролов
            this.Controls.AddRange(new Control[] {
                lblHost, txtHost, lblPort, txtPort, lblDatabase, txtDatabase,
                lblUsername, txtUsername, lblPassword, txtPassword,
                btnConnect, btnCancel
            });

            this.ResumeLayout(false);
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHost.Text) ||
                string.IsNullOrWhiteSpace(txtPort.Text) ||
                string.IsNullOrWhiteSpace(txtDatabase.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Заполните все обязательные поля", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnConnect.Enabled = false;
            btnConnect.Text = "Подключение...";

            // Попытка подключения
            bool connected = databaseHelper.Connect(
                txtHost.Text,
                txtPort.Text,
                txtDatabase.Text,
                txtUsername.Text,
                txtPassword.Text
            );

            if (connected)
            {
                this.Hide();
                var mainForm = new MainForm(databaseHelper);
                mainForm.Closed += (s, args) => this.Close();
                mainForm.Show();
            }
            else
            {
                btnConnect.Enabled = true;
                btnConnect.Text = "Подключиться";
            }
        }
    }
}