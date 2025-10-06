using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace HRApplication
{
    public partial class ModernConnectionForm : Form
    {
        private Database.DatabaseHelper databaseHelper;
        private Panel mainPanel;
        private TextBox txtHost, txtPort, txtDatabase, txtUsername, txtPassword;
        private Button btnConnect, btnCancel;
        private Label lblTitle;
        private Timer animationTimer;
        private int animationStep = 0;

        public ModernConnectionForm()
        {
            InitializeComponent();
            databaseHelper = new Database.DatabaseHelper();
            SetDefaultValues();
            StartAnimation();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Настройка формы
            this.Text = "HR System - Подключение";
            this.ClientSize = new Size(450, 500);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = DesignColors.BackgroundColor;

            // Основная панель с тенью
            mainPanel = new Panel
            {
                Size = new Size(400, 450),
                Location = new Point(25, 25),
                BackColor = DesignColors.CardColor,
                BorderRadius = 20
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

            // Кнопки
            btnConnect = new ModernButton
            {
                Text = "ПОДКЛЮЧИТЬСЯ",
                Size = new Size(350, 45),
                Location = new Point(25, 320),
                BackColor = DesignColors.PrimaryColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                CornerRadius = 10
            };
            btnConnect.Click += BtnConnect_Click;

            btnCancel = new ModernButton
            {
                Text = "ОТМЕНА",
                Size = new Size(350, 45),
                Location = new Point(25, 375),
                BackColor = Color.Transparent,
                ForeColor = DesignColors.LightTextColor,
                Font = new Font("Segoe UI", 10),
                BorderColor = DesignColors.LightTextColor,
                BorderSize = 1,
                CornerRadius = 10
            };
            btnCancel.Click += (s, e) => Application.Exit();

            // Добавление элементов
            mainPanel.Controls.AddRange(new Control[] { lblTitle, txtHost, txtPort, txtDatabase, txtUsername, txtPassword, btnConnect, btnCancel });
            this.Controls.Add(mainPanel);

            // Таймер анимации
            animationTimer = new Timer { Interval = 20 };

            this.ResumeLayout(false);
        }

        private void CreateInputFields()
        {
            int yPos = 90;
            int fieldHeight = 45;

            txtHost = CreateStyledTextBox("Хост сервера", yPos);
            yPos += fieldHeight + 10;

            txtPort = CreateStyledTextBox("Порт", yPos);
            yPos += fieldHeight + 10;

            txtDatabase = CreateStyledTextBox("База данных", yPos);
            yPos += fieldHeight + 10;

            txtUsername = CreateStyledTextBox("Пользователь", yPos);
            yPos += fieldHeight + 10;

            txtPassword = CreateStyledTextBox("Пароль", yPos);
            txtPassword.PasswordChar = '●';
        }

        private TextBox CreateStyledTextBox(string placeholder, int yPos)
        {
            return new ModernTextBox
            {
                PlaceholderText = placeholder,
                Size = new Size(350, 45),
                Location = new Point(25, yPos),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                BorderColor = DesignColors.LightTextColor,
                BorderSize = 1,
                CornerRadius = 8
            };
        }

        private void SetDefaultValues()
        {
            txtHost.Text = "localhost";
            txtPort.Text = "5432";
            txtDatabase.Text = "hr_management";
            txtUsername.Text = "postgres";
        }

        private void StartAnimation()
        {
            this.Opacity = 0;
            animationTimer.Tick += (s, e) =>
            {
                animationStep++;
                this.Opacity = Math.Min(animationStep * 0.05, 1.0);

                if (animationStep >= 20)
                {
                    animationTimer.Stop();
                    animationStep = 0;
                }
            };
            animationTimer.Start();
        }

        private async void BtnConnect_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                await AnimateButtonClick(btnConnect);
                AttemptConnection();
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtHost.Text))
            {
                ShowValidationError(txtHost, "Введите хост сервера");
                return false;
            }
            // Добавьте другие проверки по необходимости
            return true;
        }

        private void ShowValidationError(Control control, string message)
        {
            if (control is ModernTextBox modernTb)
            {
                modernTb.BorderColor = DesignColors.DangerColor;
            }
            MessageBox.Show(message, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private async System.Threading.Tasks.Task AnimateButtonClick(Button button)
        {
            var originalColor = button.BackColor;
            button.BackColor = DesignColors.SecondaryColor;
            button.Enabled = false;
            btnConnect.Text = "ПОДКЛЮЧЕНИЕ...";

            await System.Threading.Tasks.Task.Delay(300);

            button.BackColor = originalColor;
        }

        private void AttemptConnection()
        {
            bool connected = databaseHelper.Connect(
                txtHost.Text.Trim(),
                txtPort.Text.Trim(),
                txtDatabase.Text.Trim(),
                txtUsername.Text.Trim(),
                txtPassword.Text
            );

            if (connected)
            {
                AnimateSuccessTransition();
            }
            else
            {
                ResetConnectionButton();
            }
        }

        private void AnimateSuccessTransition()
        {
            animationTimer.Tick += (s, e) =>
            {
                animationStep++;
                this.Opacity = Math.Max(1.0 - animationStep * 0.05, 0.0);

                if (animationStep >= 20)
                {
                    animationTimer.Stop();
                    this.Hide();
                    var mainForm = new ModernMainForm(databaseHelper);
                    mainForm.Closed += (ss, ee) => this.Close();
                    mainForm.Show();
                }
            };
            animationTimer.Start();
        }

        private void ResetConnectionButton()
        {
            btnConnect.Enabled = true;
            btnConnect.Text = "ПОДКЛЮЧИТЬСЯ";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Рисуем тень вокруг формы
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                DesignColors.PrimaryColor, 2, ButtonBorderStyle.Solid,
                DesignColors.PrimaryColor, 2, ButtonBorderStyle.Solid,
                DesignColors.PrimaryColor, 2, ButtonBorderStyle.Solid,
                DesignColors.PrimaryColor, 2, ButtonBorderStyle.Solid);
        }
    }
}