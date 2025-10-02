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
    public partial class MainForm : Form
    {
        private DatabaseHelper databaseHelper;
        private Timer filterTimer;
        private bool filtersPinned = false;

        private List<Candidate> allCandidates;
        private List<Vacancy> allVacancies;
        private List<JobApplication> allApplications;

        // UI Controls
        private DataGridView dataGridView;
        private Panel filtersPanel;
        private Panel candidateDetailsPanel;
        private Button btnToggleFilters;
        private Button btnExportCandidates;
        private Button btnExportVacancies;
        private Button btnExportApplications;
        private ComboBox cmbViewType;
        private TextBox txtSearch;
        private ComboBox cmbSpecialization;
        private ComboBox cmbVacancy;
        private TrackBar trackBarMinAge;
        private TrackBar trackBarMaxAge;
        private Label lblMinAge;
        private Label lblMaxAge;
        private Label lblAgeRange;

        public MainForm(DatabaseHelper dbHelper)
        {
            databaseHelper = dbHelper;
            InitializeMainForm();
            InitializeFilterTimer();
            LoadData();
        }

        private void InitializeMainForm()
        {
            this.Text = "HR Management System";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1200, 700);

            CreateMainControls();
        }

        private void CreateMainControls()
        {
            // Панель инструментов
            var toolPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.LightGray
            };

            // Кнопка переключения фильтров
            btnToggleFilters = new Button
            {
                Text = "Фильтры ▲",
                Location = new Point(10, 10),
                Size = new Size(80, 30)
            };
            btnToggleFilters.Click += BtnToggleFilters_Click;

            // Выбор типа данных
            cmbViewType = new ComboBox
            {
                Location = new Point(100, 10),
                Size = new Size(120, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbViewType.Items.AddRange(new[] { "Кандидаты", "Вакансии", "Отклики" });
            cmbViewType.SelectedIndex = 0;
            cmbViewType.SelectedIndexChanged += CmbViewType_SelectedIndexChanged;

            // Кнопки экспорта
            btnExportCandidates = new Button
            {
                Text = "Экспорт кандидатов",
                Location = new Point(230, 10),
                Size = new Size(130, 30)
            };
            btnExportCandidates.Click += BtnExportCandidates_Click;

            btnExportVacancies = new Button
            {
                Text = "Экспорт вакансий",
                Location = new Point(370, 10),
                Size = new Size(130, 30)
            };
            btnExportVacancies.Click += BtnExportVacancies_Click;

            btnExportApplications = new Button
            {
                Text = "Экспорт откликов",
                Location = new Point(510, 10),
                Size = new Size(130, 30)
            };
            btnExportApplications.Click += BtnExportApplications_Click;

            toolPanel.Controls.AddRange(new Control[] {
                btnToggleFilters, cmbViewType,
                btnExportCandidates, btnExportVacancies, btnExportApplications
            });

            // Панель фильтров
            CreateFiltersPanel();

            // Основная таблица данных
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dataGridView.SelectionChanged += DataGridView_SelectionChanged;

            // Панель деталей кандидата
            CreateCandidateDetailsPanel();

            // Добавление на форму
            this.Controls.AddRange(new Control[] {
                dataGridView, candidateDetailsPanel, filtersPanel, toolPanel
            });
        }

        private void CreateFiltersPanel()
        {
            filtersPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.WhiteSmoke,
                Visible = false,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Поиск
            var lblSearch = new Label { Text = "Поиск:", Location = new Point(10, 15), Size = new Size(50, 20) };
            txtSearch = new TextBox { Location = new Point(60, 12), Size = new Size(150, 20) };
            txtSearch.TextChanged += OnFilterChanged;

            // Специализация
            var lblSpecialization = new Label { Text = "Специализация:", Location = new Point(220, 15), Size = new Size(80, 20) };
            cmbSpecialization = new ComboBox { Location = new Point(305, 12), Size = new Size(150, 20), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSpecialization.SelectedIndexChanged += OnFilterChanged;

            // Вакансия
            var lblVacancy = new Label { Text = "Вакансия:", Location = new Point(465, 15), Size = new Size(60, 20) };
            cmbVacancy = new ComboBox { Location = new Point(530, 12), Size = new Size(150, 20), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbVacancy.SelectedIndexChanged += OnFilterChanged;

            // Возраст
            lblAgeRange = new Label { Text = "Возраст: 18 - 70", Location = new Point(10, 50), Size = new Size(100, 20) };

            lblMinAge = new Label { Text = "18", Location = new Point(120, 50), Size = new Size(30, 20) };
            trackBarMinAge = new TrackBar
            {
                Location = new Point(150, 45),
                Size = new Size(200, 30),
                Minimum = 18,
                Maximum = 70,
                Value = 18
            };
            trackBarMinAge.Scroll += OnAgeFilterChanged;

            lblMaxAge = new Label { Text = "70", Location = new Point(360, 50), Size = new Size(30, 20) };
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
                Size = new Size(80, 30)
            };
            btnPin.Click += BtnPin_Click;

            filtersPanel.Controls.AddRange(new Control[] {
                lblSearch, txtSearch, lblSpecialization, cmbSpecialization,
                lblVacancy, cmbVacancy, lblAgeRange, lblMinAge, trackBarMinAge,
                lblMaxAge, trackBarMaxAge, btnPin
            });
        }

        private void CreateCandidateDetailsPanel()
        {
            candidateDetailsPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 0,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private void InitializeFilterTimer()
        {
            filterTimer = new Timer { Interval = 500 };
            filterTimer.Tick += FilterTimer_Tick;
        }

        private void LoadData()
        {
            allCandidates = databaseHelper.GetCandidates();
            allVacancies = databaseHelper.GetVacancies();
            allApplications = databaseHelper.GetAllApplications();

            LoadSpecializations();
            LoadVacanciesFilter();
            RefreshDataView();
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

        // Обработчики событий
        private void BtnToggleFilters_Click(object sender, EventArgs e)
        {
            if (filtersPanel.Visible)
            {
                filtersPanel.Visible = false;
                btnToggleFilters.Text = "Фильтры ▼";
            }
            else
            {
                filtersPanel.Visible = true;
                btnToggleFilters.Text = "Фильтры ▲";

                if (!filtersPinned)
                {
                    var autoHideTimer = new Timer { Interval = 10000 };
                    autoHideTimer.Tick += (s, args) =>
                    {
                        if (!filtersPinned)
                        {
                            filtersPanel.Visible = false;
                            btnToggleFilters.Text = "Фильтры ▼";
                        }
                        autoHideTimer.Stop();
                        autoHideTimer.Dispose();
                    };
                    autoHideTimer.Start();
                }
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
        }

        private void DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (cmbViewType.SelectedIndex == 0 && dataGridView.SelectedRows.Count > 0)
            {
                var candidate = dataGridView.SelectedRows[0].DataBoundItem as Candidate;
                if (candidate != null)
                {
                    ShowCandidateDetails(candidate);
                }
            }
        }

        // ИСПРАВЛЕННЫЙ МЕТОД - убраны все проблемы с преобразованием типов
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

        // ПОЛНОСТЬЮ ПЕРЕПИСАННЫЙ МЕТОД - исправлены все проблемы
        private void DisplayCandidates()
        {
            try
            {
                var filteredCandidates = allCandidates.AsEnumerable();

                // Фильтр по поиску
                if (!string.IsNullOrEmpty(txtSearch.Text))
                {
                    string searchText = txtSearch.Text.ToLower();
                    filteredCandidates = filteredCandidates.Where(c =>
                        (c.FullName != null && c.FullName.ToLower().Contains(searchText)) ||
                        (c.Skills != null && c.Skills.ToLower().Contains(searchText)));
                }

                // Фильтр по специализации
                if (cmbSpecialization.SelectedIndex > 0)
                {
                    string selectedSpecialization = cmbSpecialization.SelectedItem.ToString();
                    filteredCandidates = filteredCandidates.Where(c =>
                        c.Specialization == selectedSpecialization);
                }

                // Фильтр по вакансии
                if (cmbVacancy.SelectedIndex > 0)
                {
                    string selectedVacancyTitle = cmbVacancy.SelectedItem.ToString();
                    var selectedVacancy = allVacancies.FirstOrDefault(v => v.Title == selectedVacancyTitle);
                    if (selectedVacancy != null)
                    {
                        // Получаем ID кандидатов, откликнувшихся на эту вакансию
                        var candidateIdsForVacancy = allApplications
                            .Where(a => a.VacancyId == selectedVacancy.VacancyId)
                            .Select(a => a.CandidateId)
                            .ToList();

                        // Фильтруем кандидатов по ID
                        filteredCandidates = filteredCandidates.Where(c =>
                            candidateIdsForVacancy.Contains(c.CandidateId));
                    }
                }

                // Фильтр по возрасту
                filteredCandidates = filteredCandidates.Where(c =>
                    c.Age >= trackBarMinAge.Value && c.Age <= trackBarMaxAge.Value);

                dataGridView.DataSource = filteredCandidates.ToList();

                // Скрываем технические колонки
                if (dataGridView.Columns.Contains("CandidateId"))
                    dataGridView.Columns["CandidateId"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отображении кандидатов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayVacancies()
        {
            try
            {
                dataGridView.DataSource = allVacancies;
                if (dataGridView.Columns.Contains("VacancyId"))
                    dataGridView.Columns["VacancyId"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отображении вакансий: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayApplications()
        {
            try
            {
                dataGridView.DataSource = allApplications;
                if (dataGridView.Columns.Contains("ApplicationId"))
                    dataGridView.Columns["ApplicationId"].Visible = false;
                if (dataGridView.Columns.Contains("CandidateId"))
                    dataGridView.Columns["CandidateId"].Visible = false;
                if (dataGridView.Columns.Contains("VacancyId"))
                    dataGridView.Columns["VacancyId"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отображении откликов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowCandidateDetails(Candidate candidate)
        {
            if (candidate == null) return;

            candidateDetailsPanel.Width = 300;
            candidateDetailsPanel.Controls.Clear();

            var applications = databaseHelper.GetApplicationsByCandidate(candidate.CandidateId);

            var details = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10)
            };

            string candidateInfo = $@"{candidate.FullName}

Возраст: {candidate.Age}
Специализация: {candidate.Specialization}

Навыки:
{candidate.Skills ?? "Не указаны"}

Отклики ({applications.Count}):";

            foreach (var app in applications)
            {
                candidateInfo += $"\n• {app.VacancyTitle} ({app.Status})";
            }

            details.Text = candidateInfo;
            candidateDetailsPanel.Controls.Add(details);
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