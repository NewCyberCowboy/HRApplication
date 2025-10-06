using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using HRApplication.Database;
using HRApplication.Models;

namespace HRApplication
{
    public partial class ModernMainForm : Form
    {
        private DatabaseHelper databaseHelper;
        private List<Candidate> allCandidates;
        private List<Vacancy> allVacancies;
        private List<JobApplication> allApplications;

        // Объявляем все панели как поля класса
        private Panel headerPanel;
        private Panel mainContentPanel;
        private DataGridView dataGridView;
        private ComboBox cmbViewType;
        private Button btnExportCandidates, btnExportVacancies, btnExportApplications;
        private Button btnSettings, btnToggleFilters;
        private Panel candidateCardPanel;
        private Panel filtersPanel;
        private Timer filterTimer;
        private bool filtersPinned = false;

        // Элементы фильтров
        private TextBox txtSearch;
        private ComboBox cmbSpecialization;
        private ComboBox cmbVacancy;
        private TrackBar trackBarMinAge;
        private TrackBar trackBarMaxAge;
        private Label lblMinAge;
        private Label lblMaxAge;
        private Label lblAgeRange;

        // Высота элементов для динамического расчета
        private int headerHeight = 60;
        private int filtersHeight = 90;

        public ModernMainForm(DatabaseHelper dbHelper)
        {
            databaseHelper = dbHelper;
            InitializeComponent();
            InitializeFilterTimer();
            LoadData();
            ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Основные настройки формы
            this.Text = "HR Management System";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = DesignColors.BackgroundColor;
            this.Font = new Font("Segoe UI", 9);

            CreateHeaderPanel();
            CreateFiltersPanel();
            CreateMainContentPanel();
            CreateCandidateCardPanel();

            // Правильно упорядочиваем элементы (важен порядок!)
            this.Controls.Add(mainContentPanel);
            this.Controls.Add(filtersPanel);
            this.Controls.Add(candidateCardPanel);
            this.Controls.Add(headerPanel);

            UpdateLayout(); // Первоначальное расположение

            this.ResumeLayout(false);
        }

        private void InitializeFilterTimer()
        {
            filterTimer = new Timer { Interval = 500 };
            filterTimer.Tick += FilterTimer_Tick;
        }

        private void CreateHeaderPanel()
        {
            // Верхняя панель
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = headerHeight,
                BackColor = DesignColors.PrimaryColor
            };

            // Заголовок
            var lblTitle = new Label
            {
                Text = "HR Management System",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(300, 40),
                Location = new Point(15, 10),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Кнопка настроек (шестеренка) - ПРАВЫЙ ВЕРХНИЙ УГОЛ
            btnSettings = new Button
            {
                Text = "⚙",
                Size = new Size(35, 35),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 255, 255, 255);
            btnSettings.Click += BtnSettings_Click;

            // Выбор типа данных
            var lblViewType = new Label
            {
                Text = "Просмотр:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(60, 20),
                Location = new Point(330, 20)
            };

            cmbViewType = new ComboBox
            {
                Size = new Size(120, 25),
                Location = new Point(400, 17),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbViewType.Items.AddRange(new[] { "Кандидаты", "Вакансии", "Отклики" });
            cmbViewType.SelectedIndex = 0;
            cmbViewType.SelectedIndexChanged += CmbViewType_SelectedIndexChanged;

            // Кнопка фильтров
            btnToggleFilters = new Button
            {
                Text = "Фильтры ▼",
                Size = new Size(90, 25),
                Location = new Point(540, 17),
                BackColor = DesignColors.SecondaryColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnToggleFilters.FlatAppearance.BorderSize = 0;
            btnToggleFilters.Click += BtnToggleFilters_Click;

            // Кнопки экспорта
            btnExportCandidates = new Button
            {
                Text = "Экспорт кандидатов",
                Size = new Size(120, 25),
                Location = new Point(670, 17),
                BackColor = DesignColors.SuccessColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8),
                FlatStyle = FlatStyle.Flat
            };
            btnExportCandidates.FlatAppearance.BorderSize = 0;
            btnExportCandidates.Click += BtnExportCandidates_Click;

            btnExportVacancies = new Button
            {
                Text = "Экспорт вакансий",
                Size = new Size(120, 25),
                Location = new Point(800, 17),
                BackColor = DesignColors.SuccessColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8),
                FlatStyle = FlatStyle.Flat
            };
            btnExportVacancies.FlatAppearance.BorderSize = 0;
            btnExportVacancies.Click += BtnExportVacancies_Click;

            btnExportApplications = new Button
            {
                Text = "Экспорт откликов",
                Size = new Size(120, 25),
                Location = new Point(930, 17),
                BackColor = DesignColors.SuccessColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8),
                FlatStyle = FlatStyle.Flat
            };
            btnExportApplications.FlatAppearance.BorderSize = 0;
            btnExportApplications.Click += BtnExportApplications_Click;

            headerPanel.Controls.AddRange(new Control[] {
                lblTitle, btnSettings, lblViewType, cmbViewType,
                btnToggleFilters, btnExportCandidates, btnExportVacancies, btnExportApplications
            });
        }

        private void CreateFiltersPanel()
        {
            filtersPanel = new Panel
            {
                Height = filtersHeight,
                BackColor = DesignColors.CardColor,
                Visible = false,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };

            // Поиск
            var lblSearch = new Label
            {
                Text = "Поиск:",
                Location = new Point(10, 15),
                Size = new Size(50, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = DesignColors.TextColor
            };
            txtSearch = new TextBox { Location = new Point(60, 12), Size = new Size(150, 25), Font = new Font("Segoe UI", 9) };
            txtSearch.TextChanged += OnFilterChanged;

            // Специализация
            var lblSpecialization = new Label
            {
                Text = "Специализация:",
                Location = new Point(220, 15),
                Size = new Size(80, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = DesignColors.TextColor
            };
            cmbSpecialization = new ComboBox
            {
                Location = new Point(305, 12),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbSpecialization.SelectedIndexChanged += OnFilterChanged;

            // Вакансия
            var lblVacancy = new Label
            {
                Text = "Вакансия:",
                Location = new Point(465, 15),
                Size = new Size(60, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = DesignColors.TextColor
            };
            cmbVacancy = new ComboBox
            {
                Location = new Point(530, 12),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbVacancy.SelectedIndexChanged += OnFilterChanged;

            // Возраст
            lblAgeRange = new Label
            {
                Text = "Возраст: 18 - 70",
                Location = new Point(10, 50),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = DesignColors.TextColor
            };

            lblMinAge = new Label
            {
                Text = "18",
                Location = new Point(120, 50),
                Size = new Size(30, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = DesignColors.TextColor
            };
            trackBarMinAge = new TrackBar
            {
                Location = new Point(150, 45),
                Size = new Size(200, 30),
                Minimum = 18,
                Maximum = 70,
                Value = 18
            };
            trackBarMinAge.Scroll += OnAgeFilterChanged;

            lblMaxAge = new Label
            {
                Text = "70",
                Location = new Point(360, 50),
                Size = new Size(30, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = DesignColors.TextColor
            };
            trackBarMaxAge = new TrackBar
            {
                Location = new Point(390, 45),
                Size = new Size(200, 30),
                Minimum = 18,
                Maximum = 70,
                Value = 70
            };
            trackBarMaxAge.Scroll += OnAgeFilterChanged;

            // Кнопка закрепления
            var btnPin = new Button
            {
                Text = "Закрепить",
                Location = new Point(600, 45),
                Size = new Size(80, 25),
                BackColor = DesignColors.SecondaryColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8),
                FlatStyle = FlatStyle.Flat
            };
            btnPin.FlatAppearance.BorderSize = 0;
            btnPin.Click += BtnPin_Click;

            filtersPanel.Controls.AddRange(new Control[] {
                lblSearch, txtSearch, lblSpecialization, cmbSpecialization,
                lblVacancy, cmbVacancy, lblAgeRange, lblMinAge, trackBarMinAge,
                lblMaxAge, trackBarMaxAge, btnPin
            });
        }

        private void CreateMainContentPanel()
        {
            // Основная панель контента
            mainContentPanel = new Panel
            {
                BackColor = DesignColors.BackgroundColor
            };

            // DataGridView для отображения данных
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = DesignColors.CardColor,
                BorderStyle = BorderStyle.Fixed3D,
                Font = new Font("Segoe UI", 9),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ScrollBars = ScrollBars.Both
            };
            dataGridView.SelectionChanged += DataGridView_SelectionChanged;

            // Стилизация DataGridView
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = DesignColors.PrimaryColor;
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.RowHeadersVisible = false;
            dataGridView.DefaultCellStyle.BackColor = DesignColors.CardColor;
            dataGridView.DefaultCellStyle.ForeColor = DesignColors.TextColor;
            dataGridView.DefaultCellStyle.SelectionBackColor = DesignColors.SecondaryColor;
            dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;

            mainContentPanel.Controls.Add(dataGridView);
        }

        private void CreateCandidateCardPanel()
        {
            // Панель карточки кандидата (изначально скрыта)
            candidateCardPanel = new Panel
            {
                Width = 350,
                BackColor = DesignColors.CardColor,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                Padding = new Padding(15)
            };
        }

        // === ДИНАМИЧЕСКОЕ РАСПОЛОЖЕНИЕ ===
        private void UpdateLayout()
        {
            int currentY = headerHeight;

            // Обновляем позицию кнопки настроек
            if (btnSettings != null)
            {
                btnSettings.Location = new Point(this.ClientSize.Width - 50, 12);
            }

            // Располагаем панель фильтров
            if (filtersPanel.Visible)
            {
                filtersPanel.Location = new Point(0, currentY);
                filtersPanel.Width = this.ClientSize.Width;
                filtersPanel.Height = filtersHeight;
                currentY += filtersHeight;
            }

            // Располагаем основную панель контента
            mainContentPanel.Location = new Point(0, currentY);
            mainContentPanel.Size = new Size(
                candidateCardPanel.Visible ? this.ClientSize.Width - candidateCardPanel.Width : this.ClientSize.Width,
                this.ClientSize.Height - currentY
            );

            // Располагаем панель карточки кандидата
            if (candidateCardPanel.Visible)
            {
                candidateCardPanel.Location = new Point(this.ClientSize.Width - candidateCardPanel.Width, currentY);
                candidateCardPanel.Height = this.ClientSize.Height - currentY;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateLayout();
        }

        // === ОБРАБОТЧИКИ СОБЫТИЙ ===

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            DesignColors.IsDarkTheme = !DesignColors.IsDarkTheme;
            ApplyTheme();

            string themeName = DesignColors.IsDarkTheme ? "Темная" : "Светлая";
            MessageBox.Show($"Тема изменена на: {themeName}", "Настройки",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnToggleFilters_Click(object sender, EventArgs e)
        {
            filtersPanel.Visible = !filtersPanel.Visible;
            btnToggleFilters.Text = filtersPanel.Visible ? "Фильтры ▲" : "Фильтры ▼";

            UpdateLayout(); // Обновляем расположение при изменении видимости фильтров

            if (filtersPanel.Visible && !filtersPinned)
            {
                var autoHideTimer = new Timer { Interval = 10000 };
                autoHideTimer.Tick += (s, args) =>
                {
                    if (!filtersPinned && filtersPanel.Visible)
                    {
                        filtersPanel.Visible = false;
                        btnToggleFilters.Text = "Фильтры ▼";
                        UpdateLayout();
                    }
                    autoHideTimer.Stop();
                    autoHideTimer.Dispose();
                };
                autoHideTimer.Start();
            }
        }

        private void BtnPin_Click(object sender, EventArgs e)
        {
            filtersPinned = !filtersPinned;
            ((Button)sender).Text = filtersPinned ? "Открепить" : "Закрепить";
        }

        private void OnFilterChanged(object sender, EventArgs e)
        {
            filterTimer.Stop();
            filterTimer.Start();
        }

        private void OnAgeFilterChanged(object sender, EventArgs e)
        {
            lblMinAge.Text = trackBarMinAge.Value.ToString();
            lblMaxAge.Text = trackBarMaxAge.Value.ToString();
            lblAgeRange.Text = $"Возраст: {trackBarMinAge.Value} - {trackBarMaxAge.Value}";
            OnFilterChanged(sender, e);
        }

        private void FilterTimer_Tick(object sender, EventArgs e)
        {
            filterTimer.Stop();
            RefreshDataView();
        }

        private void CmbViewType_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshDataView();
            HideCandidateCard();
        }

        private void DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (cmbViewType.SelectedIndex == 0 && dataGridView.SelectedRows.Count > 0)
            {
                var selectedCandidate = dataGridView.SelectedRows[0].DataBoundItem as Candidate;
                if (selectedCandidate != null)
                {
                    ShowCandidateCard(selectedCandidate);
                }
            }
            else
            {
                HideCandidateCard();
            }
        }

        private void BtnExportCandidates_Click(object sender, EventArgs e)
        {
            ExportToJson(allCandidates, "candidates.json");
        }

        private void BtnExportVacancies_Click(object sender, EventArgs e)
        {
            ExportToJson(allVacancies, "vacancies.json");
        }

        private void BtnExportApplications_Click(object sender, EventArgs e)
        {
            ExportToJson(allApplications, "applications.json");
        }

        // === ОСНОВНЫЕ МЕТОДЫ ===

        private void LoadData()
        {
            try
            {
                allCandidates = databaseHelper.GetCandidates();
                allVacancies = databaseHelper.GetVacancies();
                allApplications = databaseHelper.GetAllApplications();

                LoadSpecializations();
                LoadVacanciesFilter();
                RefreshDataView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSpecializations()
        {
            var specializations = allCandidates
                .Select(c => c.Specialization)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            cmbSpecialization.Items.Clear();
            cmbSpecialization.Items.Add("Все специализации");
            cmbSpecialization.Items.AddRange(specializations.ToArray());
            cmbSpecialization.SelectedIndex = 0;
        }

        private void LoadVacanciesFilter()
        {
            cmbVacancy.Items.Clear();
            cmbVacancy.Items.Add("Все вакансии");
            cmbVacancy.Items.AddRange(allVacancies.Select(v => v.Title).ToArray());
            cmbVacancy.SelectedIndex = 0;
        }

        private void RefreshDataView()
        {
            try
            {
                switch (cmbViewType.SelectedIndex)
                {
                    case 0: // Кандидаты
                        DisplayCandidates();
                        break;
                    case 1: // Вакансии
                        DisplayVacancies();
                        break;
                    case 2: // Отклики
                        DisplayApplications();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayCandidates()
        {
            var filteredCandidates = allCandidates.AsEnumerable();

            // Применение фильтров
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                string searchText = txtSearch.Text.ToLower();
                filteredCandidates = filteredCandidates.Where(c =>
                    (c.FullName != null && c.FullName.ToLower().Contains(searchText)) ||
                    (c.Skills != null && c.Skills.ToLower().Contains(searchText)));
            }

            if (cmbSpecialization.SelectedIndex > 0)
            {
                string selectedSpecialization = cmbSpecialization.SelectedItem.ToString();
                filteredCandidates = filteredCandidates.Where(c =>
                    c.Specialization == selectedSpecialization);
            }

            if (cmbVacancy.SelectedIndex > 0)
            {
                string selectedVacancyTitle = cmbVacancy.SelectedItem.ToString();
                var selectedVacancy = allVacancies.FirstOrDefault(v => v.Title == selectedVacancyTitle);
                if (selectedVacancy != null)
                {
                    var candidateIdsForVacancy = allApplications
                        .Where(a => a.VacancyId == selectedVacancy.VacancyId)
                        .Select(a => a.CandidateId)
                        .ToList();

                    filteredCandidates = filteredCandidates.Where(c =>
                        candidateIdsForVacancy.Contains(c.CandidateId));
                }
            }

            filteredCandidates = filteredCandidates.Where(c =>
                c.Age >= trackBarMinAge.Value && c.Age <= trackBarMaxAge.Value);

            dataGridView.DataSource = filteredCandidates.ToList();
            FormatCandidatesGrid();
        }

        private void DisplayVacancies()
        {
            dataGridView.DataSource = allVacancies;
            FormatVacanciesGrid();
        }

        private void DisplayApplications()
        {
            dataGridView.DataSource = allApplications;
            FormatApplicationsGrid();
        }

        private void FormatCandidatesGrid()
        {
            if (dataGridView.Columns.Contains("CandidateId"))
                dataGridView.Columns["CandidateId"].Visible = false;
            if (dataGridView.Columns.Contains("CreatedAt"))
                dataGridView.Columns["CreatedAt"].Visible = false;
            if (dataGridView.Columns.Contains("Skills"))
                dataGridView.Columns["Skills"].Visible = false;
        }

        private void FormatVacanciesGrid()
        {
            if (dataGridView.Columns.Contains("VacancyId"))
                dataGridView.Columns["VacancyId"].Visible = false;
            if (dataGridView.Columns.Contains("CreatedAt"))
                dataGridView.Columns["CreatedAt"].Visible = false;
            if (dataGridView.Columns.Contains("Description"))
                dataGridView.Columns["Description"].Visible = false;
            if (dataGridView.Columns.Contains("RequiredSkills"))
                dataGridView.Columns["RequiredSkills"].Visible = false;
        }

        private void FormatApplicationsGrid()
        {
            if (dataGridView.Columns.Contains("ApplicationId"))
                dataGridView.Columns["ApplicationId"].Visible = false;
            if (dataGridView.Columns.Contains("CandidateId"))
                dataGridView.Columns["CandidateId"].Visible = false;
            if (dataGridView.Columns.Contains("VacancyId"))
                dataGridView.Columns["VacancyId"].Visible = false;
        }

        private void ShowCandidateCard(Candidate candidate)
        {
            candidateCardPanel.Controls.Clear();
            candidateCardPanel.Visible = true;

            var applications = databaseHelper.GetApplicationsByCandidate(candidate.CandidateId);

            int yPos = 15;

            // Имя кандидата
            var lblName = new Label
            {
                Text = candidate.FullName,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = DesignColors.PrimaryColor,
                Size = new Size(320, 30),
                Location = new Point(0, yPos),
                TextAlign = ContentAlignment.MiddleLeft
            };
            yPos += 35;

            // Возраст
            var lblAge = new Label
            {
                Text = $"Возраст: {candidate.Age}",
                Font = new Font("Segoe UI", 10),
                ForeColor = DesignColors.TextColor,
                Size = new Size(320, 20),
                Location = new Point(0, yPos)
            };
            yPos += 25;

            // Специализация
            var lblSpecialization = new Label
            {
                Text = $"Специализация: {candidate.Specialization}",
                Font = new Font("Segoe UI", 10),
                ForeColor = DesignColors.TextColor,
                Size = new Size(320, 20),
                Location = new Point(0, yPos)
            };
            yPos += 30;

            // Навыки заголовок
            var lblSkillsTitle = new Label
            {
                Text = "Навыки:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = DesignColors.TextColor,
                Size = new Size(320, 20),
                Location = new Point(0, yPos)
            };
            yPos += 25;

            // Навыки текст
            var txtSkills = new TextBox
            {
                Text = candidate.Skills,
                Font = new Font("Segoe UI", 9),
                ForeColor = DesignColors.TextColor,
                Size = new Size(320, 80),
                Location = new Point(0, yPos),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = DesignColors.BackgroundColor
            };
            yPos += 90;

            // Отклики заголовок
            var lblApplicationsTitle = new Label
            {
                Text = $"Отклики ({applications.Count}):",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = DesignColors.TextColor,
                Size = new Size(320, 20),
                Location = new Point(0, yPos)
            };
            yPos += 25;

            // Список откликов
            var applicationsPanel = new Panel
            {
                Size = new Size(320, 150),
                Location = new Point(0, yPos),
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = DesignColors.BackgroundColor
            };

            int appYPos = 5;
            foreach (var app in applications)
            {
                var appLabel = new Label
                {
                    Text = $"• {app.VacancyTitle} ({app.Status})",
                    Font = new Font("Segoe UI", 9),
                    ForeColor = GetStatusColor(app.Status),
                    Size = new Size(300, 20),
                    Location = new Point(5, appYPos)
                };
                applicationsPanel.Controls.Add(appLabel);
                appYPos += 25;
            }
            yPos += 160;

            // Кнопка закрытия
            var btnClose = new Button
            {
                Text = "Закрыть",
                Size = new Size(100, 30),
                Location = new Point(220, yPos),
                BackColor = DesignColors.SecondaryColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, args) => HideCandidateCard();

            candidateCardPanel.Controls.AddRange(new Control[] {
                lblName, lblAge, lblSpecialization, lblSkillsTitle, txtSkills,
                lblApplicationsTitle, applicationsPanel, btnClose
            });

            UpdateLayout(); // Обновляем расположение при показе карточки
        }

        private Color GetStatusColor(string status)
        {
            if (status == null) return DesignColors.TextColor;

            string statusLower = status.ToLower();

            if (statusLower == "accepted")
                return Color.Green;
            else if (statusLower == "rejected")
                return Color.Red;
            else if (statusLower == "interview")
                return Color.Orange;
            else
                return DesignColors.TextColor;
        }

        private void HideCandidateCard()
        {
            candidateCardPanel.Visible = false;
            candidateCardPanel.Controls.Clear();
            UpdateLayout(); // Обновляем расположение при скрытии карточки
        }

        private void ApplyTheme()
        {
            this.BackColor = DesignColors.BackgroundColor;

            // Обновляем цвета основных панелей
            if (headerPanel != null) headerPanel.BackColor = DesignColors.PrimaryColor;
            if (filtersPanel != null) filtersPanel.BackColor = DesignColors.CardColor;
            if (mainContentPanel != null) mainContentPanel.BackColor = DesignColors.BackgroundColor;
            if (candidateCardPanel != null) candidateCardPanel.BackColor = DesignColors.CardColor;

            if (dataGridView != null)
            {
                dataGridView.BackgroundColor = DesignColors.CardColor;
                dataGridView.DefaultCellStyle.BackColor = DesignColors.CardColor;
                dataGridView.DefaultCellStyle.ForeColor = DesignColors.TextColor;
                dataGridView.ColumnHeadersDefaultCellStyle.BackColor = DesignColors.PrimaryColor;
            }
        }

        private void ExportToJson(object data, string fileName)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(fileName, json);
                MessageBox.Show($"Данные экспортированы в {fileName}", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            databaseHelper?.Disconnect();
            base.OnFormClosing(e);
        }
    }
}