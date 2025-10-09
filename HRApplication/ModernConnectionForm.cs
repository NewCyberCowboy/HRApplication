using System;
using System.Drawing;
using System.Windows.Forms;
using HRApplication.Database;

namespace HRApplication
{
    public partial class ModernConnectionForm : Form
    {
        private DatabaseHelper databaseHelper;
        private Panel mainPanel;
        private TextBox txtHost, txtPort, txtDatabase, txtUsername, txtPassword;
        private Button btnConnect, btnCancel;
        private Label lblTitle;
        public static string CurrentConnectionString { get; private set; }
        public ModernConnectionForm()
        {
            InitializeComponent();
            databaseHelper = new DatabaseHelper();
            SetDefaultValues();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Настройка формы - УВЕЛИЧИЛИ ВЫСОТУ
            this.Text = "HR System - Подключение";
            this.ClientSize = new Size(450, 520); // Увеличили высоту с 450 до 520
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = DesignColors.BackgroundColor;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Основная панель - УВЕЛИЧИЛИ ВЫСОТУ
            mainPanel = new Panel
            {
                Size = new Size(400, 470), // Увеличили высоту с 400 до 470
                Location = new Point(25, 25),
                BackColor = DesignColors.CardColor,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Заголовок
            lblTitle = new Label
            {
                Text = "Подключение к базе данных",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = DesignColors.PrimaryColor,
                Size = new Size(350, 40),
                Location = new Point(25, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Поля ввода
            CreateInputFields();

            // Кнопки - ПОДНЯЛИ ВЫШЕ И ДОБАВИЛИ ПРОСТРАНСТВО
            btnConnect = new Button
            {
                Text = "ПОДКЛЮЧИТЬСЯ",
                Size = new Size(350, 40),
                Location = new Point(25, 380), // Подняли выше (было 320)
                BackColor = DesignColors.PrimaryColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnConnect.FlatAppearance.BorderSize = 0;
            btnConnect.Click += BtnConnect_Click;

            btnCancel = new Button
            {
                Text = "ОТМЕНА",
                Size = new Size(350, 40),
                Location = new Point(25, 430), // Подняли выше (было 370) и добавили пространство
                BackColor = DesignColors.SecondaryColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => Application.Exit();

            // Добавление элементов
            mainPanel.Controls.AddRange(new Control[] {
                lblTitle, txtHost, txtPort, txtDatabase, txtUsername, txtPassword,
                btnConnect, btnCancel
            });
            this.Controls.Add(mainPanel);

            this.ResumeLayout(false);
        }

        private void CreateInputFields()
        {
            int yPos = 90;

            // Хост
            var lblHost = new Label
            {
                Text = "Хост:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = DesignColors.TextColor,
                Size = new Size(100, 20),
                Location = new Point(25, yPos)
            };

            txtHost = new TextBox
            {
                Size = new Size(350, 30),
                Location = new Point(25, yPos + 20),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            yPos += 60;

            // Порт
            var lblPort = new Label
            {
                Text = "Порт:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = DesignColors.TextColor,
                Size = new Size(100, 20),
                Location = new Point(25, yPos)
            };

            txtPort = new TextBox
            {
                Size = new Size(350, 30),
                Location = new Point(25, yPos + 20),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            yPos += 60;

            // База данных
            var lblDatabase = new Label
            {
                Text = "База данных:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = DesignColors.TextColor,
                Size = new Size(100, 20),
                Location = new Point(25, yPos)
            };

            txtDatabase = new TextBox
            {
                Size = new Size(350, 30),
                Location = new Point(25, yPos + 20),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            yPos += 60;

            // Пользователь
            var lblUsername = new Label
            {
                Text = "Пользователь:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = DesignColors.TextColor,
                Size = new Size(100, 20),
                Location = new Point(25, yPos)
            };

            txtUsername = new TextBox
            {
                Size = new Size(350, 30),
                Location = new Point(25, yPos + 20),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            yPos += 60;

            // Пароль - ПОДНЯЛИ ВЫШЕ (сразу после пользователя)
            var lblPassword = new Label
            {
                Text = "Пароль:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = DesignColors.TextColor,
                Size = new Size(100, 20),
                Location = new Point(25, yPos)
            };

            txtPassword = new TextBox
            {
                Size = new Size(350, 30),
                Location = new Point(25, yPos + 20),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '●'
            };

            // Добавляем лейблы
            mainPanel.Controls.AddRange(new Control[] { lblHost, lblPort, lblDatabase, lblUsername, lblPassword });
        }

        private void SetDefaultValues()
        {
            txtHost.Text = "localhost";
            txtPort.Text = "5432";
            txtDatabase.Text = "hr_management";
            txtUsername.Text = "postgres";
            txtPassword.Text = "";
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                AttemptConnection();
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtHost.Text))
            {
                MessageBox.Show("Введите хост сервера", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHost.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPort.Text) || !int.TryParse(txtPort.Text, out _))
            {
                MessageBox.Show("Введите корректный порт", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPort.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDatabase.Text))
            {
                MessageBox.Show("Введите название базы данных", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDatabase.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Введите имя пользователя", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return false;
            }

            return true;
        }

        private void AttemptConnection()
        {
            btnConnect.Enabled = false;
            btnConnect.Text = "ПОДКЛЮЧЕНИЕ...";

            CurrentConnectionString = $"Host={txtHost.Text.Trim()};Port={txtPort.Text.Trim()};" +
                             $"Database={txtDatabase.Text.Trim()};Username={txtUsername.Text.Trim()};" +
                             $"Password={txtPassword.Text};Timeout=30";

            bool connected = databaseHelper.Connect(
                txtHost.Text.Trim(),
                txtPort.Text.Trim(),
                txtDatabase.Text.Trim(),
                txtUsername.Text.Trim(),
                txtPassword.Text
            );

            if (connected)
            {
                this.Hide();
                var mainForm = new ModernMainForm(databaseHelper);
                mainForm.Closed += (s, e) => this.Close();
                mainForm.Show();
            }
            else
            {
                btnConnect.Enabled = true;
                btnConnect.Text = "ПОДКЛЮЧИТЬСЯ";
            }
        }
    }
}