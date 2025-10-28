using ClosedXML.Excel;
using HRApplication.Database;
using HRApplication.Models;
using HRApplication.Repositories;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


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
        private Button btnTestRepository;

        // Новые элементы для функции переворота слова
        private TextBox txtInputWord;
        private Button btnReverseWord;
        private Label lblReversedWord;
        private Panel reversalPanel;

        // Элементы фильтров
        private TextBox txtSearch;
        private ComboBox cmbSpecialization;
        private ComboBox cmbVacancy;
        private TrackBar trackBarMinAge;
        private TrackBar trackBarMaxAge;
        private Label lblMinAge;
        private Label lblMaxAge;
        private Label lblAgeRange;
        private ComboBox cmbSearchType;

        // Пагинация
        private int currentPage = 1;
        private int pageSize = 50;
        private Label lblPageInfo;
        private Button btnPrevPage, btnNextPage;
        private int totalItems = 0;
        private ComboBox cmbPageSize;

        // Высота элементов для динамического расчета
        private int headerHeight = 60;
        private int filtersHeight = 120;

        private Button btnExpandPanel;
        private bool isPanelExpanded = false;
        private Panel expandedPanel;
        private ComboBox cmbInputMode;
        private TextBox txtComplexInput;
        private Button btnProcessComplex;
        private Label lblValidationMessage;

       

        public ModernMainForm(DatabaseHelper dbHelper)
        {
            databaseHelper = dbHelper;

            // Сначала создаем панель переворота
            CreateWordReversalPanel();

            // Потом остальные компоненты
            InitializeCustomComponent();
            InitializeFilterTimer();
            LoadData();
            ApplyTheme();

            // Принудительное обновление позиции после загрузки
            this.Shown += (s, e) => {
                UpdateReversalPanelPosition();
                reversalPanel?.BringToFront();
            };
        }

        private void InitializeCustomComponent()
        {
            this.SuspendLayout();

            this.Text = "HR Management System";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = DesignColors.BackgroundColor;
            this.Font = new Font("Segoe UI", 9);

            CreateHeaderPanel();
            CreateFiltersPanel();
            CreateMainContentPanel();
            CreateCandidateCardPanel();

            // Добавляем основные элементы (панель переворота уже добавлена)
            this.Controls.Add(mainContentPanel);
            this.Controls.Add(filtersPanel);
            this.Controls.Add(candidateCardPanel);
            this.Controls.Add(headerPanel);

            this.ResumeLayout(false);

            // Обновляем layout
            UpdateLayout();
        }

        private void CreateWordReversalPanel()
        {
            // Создаем компактную кнопку для разворачивания
            btnExpandPanel = new Button
            {
                Text = "🔤", // Иконка текста
                Size = new Size(40, 40),
                Location = new Point(this.ClientSize.Width - 60, 20),
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExpandPanel.FlatAppearance.BorderSize = 0;
            btnExpandPanel.Click += BtnExpandPanel_Click;

            this.Controls.Add(btnExpandPanel);

            // Создаем развернутую панель (изначально скрыта)
            CreateExpandedPanel();
        }

        private void BtnExpandPanel_Click(object sender, EventArgs e)
        {
            ToggleExpandedPanel(!isPanelExpanded);
        }

        private void CreateExpandedPanel()
        {
            expandedPanel = new Panel
            {
                Size = new Size(400, 500),
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15),
                Visible = false,
                AutoScroll = true
            };

            // Заголовок с кнопкой закрытия
            var titlePanel = new Panel
            {
                Size = new Size(370, 30),
                Location = new Point(0, 0)
            };

            var lblTitle = new Label
            {
                Text = "Обработка текста",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = DesignColors.PrimaryColor,
                Size = new Size(200, 30),
                Location = new Point(0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var btnClosePanel = new Button
            {
                Text = "×",
                Size = new Size(30, 30),
                Location = new Point(340, 0),
                BackColor = Color.LightCoral,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClosePanel.FlatAppearance.BorderSize = 0;
            btnClosePanel.Click += (s, e) => ToggleExpandedPanel(false);

            titlePanel.Controls.AddRange(new Control[] { lblTitle, btnClosePanel });

            // Выбор режима ввода
            var lblInputMode = new Label
            {
                Text = "Режим обработки:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = DesignColors.TextColor,
                Size = new Size(150, 20),
                Location = new Point(0, 40)
            };

            cmbInputMode = new ComboBox
            {
                Size = new Size(200, 25),
                Location = new Point(0, 65),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbInputMode.Items.AddRange(new[] {
                "Одно слово",
                "ФИО (Иванов Иван Иванович)",
                "Список через запятую",
                "Сложные слова через дефис",
                "Многострочный ввод"  // Новый режим
            });
            cmbInputMode.SelectedIndex = 0;
            cmbInputMode.SelectedIndexChanged += CmbInputMode_SelectedIndexChanged;

            // Поле для сложного ввода
            var lblComplexInput = new Label
            {
                Text = "Введите текст:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = DesignColors.TextColor,
                Size = new Size(150, 20),
                Location = new Point(0, 100)
            };

            txtComplexInput = new TextBox
            {
                Size = new Size(370, 80),
                Location = new Point(0, 125),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            // Кнопка обработки
            btnProcessComplex = new Button
            {
                Text = "Обработать",
                Size = new Size(120, 30),
                Location = new Point(0, 215),
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnProcessComplex.FlatAppearance.BorderSize = 0;
            btnProcessComplex.Click += BtnProcessComplex_Click;

            // Сообщение валидации
            lblValidationMessage = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Red,
                Size = new Size(370, 40),
                Location = new Point(0, 250),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Результат
            lblReversedWord = new Label
            {
                Name = "lblReversedWord",
                Text = "Результат появится здесь...",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.DarkBlue,
                Size = new Size(370, 60),
                Location = new Point(0, 290),
                TextAlign = ContentAlignment.MiddleLeft,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(240, 240, 240),
                AutoSize = true,
                MaximumSize = new Size(370, 0)
            };

            expandedPanel.Controls.AddRange(new Control[] {
        titlePanel,
        lblInputMode,
        cmbInputMode,
        lblComplexInput,
        txtComplexInput,
        btnProcessComplex,
        lblValidationMessage,
        lblReversedWord
    });

            this.Controls.Add(expandedPanel);
            UpdateExpandedPanelPosition();
        }


        private void UpdateReversalPanelPosition()
        {
            if (reversalPanel != null)
            {
                reversalPanel.Location = new Point(this.ClientSize.Width - reversalPanel.Width - 20, 70);
                reversalPanel.BringToFront();
            }
        }

        private void ToggleExpandedPanel(bool show)
        {
            isPanelExpanded = show;
            expandedPanel.Visible = show;
            btnExpandPanel.Visible = !show;

            if (show)
            {
                UpdateExpandedPanelPosition();
                expandedPanel.BringToFront();
                txtComplexInput.Focus();
            }
        }

        private void UpdateExpandedPanelPosition()
        {
            if (expandedPanel != null)
            {
                expandedPanel.Location = new Point(
                    this.ClientSize.Width - expandedPanel.Width - 20,
                    70
                );
            }

            if (btnExpandPanel != null)
            {
                btnExpandPanel.Location = new Point(
                    this.ClientSize.Width - btnExpandPanel.Width - 20,
                    20
                );
            }
        }
        private void CmbInputMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateInputMode();
        }

        private void UpdateInputMode()
        {
            lblValidationMessage.Text = "";

            switch (cmbInputMode.SelectedIndex)
            {
                case 0: // Одно слово
                    txtComplexInput.Text = "Введите одно слово";
                    txtComplexInput.Height = 25; // Однострочное поле
                    txtComplexInput.Multiline = false;
                    break;
                case 1: // ФИО
                    txtComplexInput.Text = "Иванов Иван Иванович";
                    txtComplexInput.Height = 25;
                    txtComplexInput.Multiline = false;
                    break;
                case 2: // Список через запятую
                    txtComplexInput.Text = "слово1, слово2, слово3";
                    txtComplexInput.Height = 25;
                    txtComplexInput.Multiline = false;
                    break;
                case 3: // Сложные слова через дефис
                    txtComplexInput.Text = "северо-запад, интернет-магазин";
                    txtComplexInput.Height = 25;
                    txtComplexInput.Multiline = false;
                    break;
                case 4: // Многострочный ввод
                    txtComplexInput.Text = "Первое слово\nВторое слово\nТретье слово";
                    txtComplexInput.Height = 80; // Многострочное поле
                    txtComplexInput.Multiline = true;
                    break;
            }

            if (txtComplexInput.Text != "Введите одно слово")
            {
                txtComplexInput.ForeColor = Color.Black;
            }
            else
            {
                txtComplexInput.ForeColor = Color.Gray;
            }
        }

        private void BtnProcessComplex_Click(object sender, EventArgs e)
        {
            try
            {
                string input = txtComplexInput.Text.Trim();

                // Проверка на placeholder
                if (input == "Введите одно слово" || string.IsNullOrWhiteSpace(input))
                {
                    ShowValidationMessage("Введите текст для обработки");
                    return;
                }

                // Валидация ввода
                if (!ValidateInput(input))
                {
                    return;
                }

                string result = ProcessInput(input);
                lblReversedWord.Text = result;
                lblValidationMessage.Text = "✅ Обработка завершена успешно";
                lblValidationMessage.ForeColor = Color.Green;

            }
            catch (Exception ex)
            {
                ShowValidationMessage($"Ошибка: {ex.Message}");
            }
        }

        private bool ValidateInput(string input)
        {
            // Проверка на нижние подчеркивания
            if (input.Contains("_"))
            {
                ShowValidationMessage("❌ Ошибка: Использование нижнего подчеркивания запрещено!\n" +
                                    "Используйте пробелы, запятые или дефисы для разделения слов.");
                return false;
            }

            // Проверка на специальные символы (кроме разрешенных)
            var invalidChars = new[] { '@', '#', '$', '%', '&', '*', '=', '+', '\\', '/', '|' };
            foreach (char c in invalidChars)
            {
                if (input.Contains(c))
                {
                    ShowValidationMessage($"❌ Ошибка: Запрещенный символ '{c}'\n" +
                                        "Разрешенные разделители: пробел, запятая, дефис, точка, новая строка.");
                    return false;
                }
            }

            // Проверка для режима "Одно слово"
            if (cmbInputMode.SelectedIndex == 0)
            {
                if (input.Contains(' ') || input.Contains(',') || input.Contains('-') || input.Contains('\n'))
                {
                    ShowValidationMessage("❌ Ошибка: В режиме 'Одно слово' нельзя использовать пробелы, запятые, дефисы или новые строки");
                    return false;
                }
            }

            // Проверка для режима "Многострочный ввод" - минимальная проверка
            if (cmbInputMode.SelectedIndex == 4)
            {
                var lines = input.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0)
                {
                    ShowValidationMessage("❌ Ошибка: Введите хотя бы одну строку");
                    return false;
                }
            }

            lblValidationMessage.Text = "✅ Валидация пройдена";
            lblValidationMessage.ForeColor = Color.Green;
            return true;
        }

        private string ProcessInput(string input)
        {
            switch (cmbInputMode.SelectedIndex)
            {
                case 0: // Одно слово
                    return ReverseWord(input);

                case 1: // ФИО
                    return ProcessFIO(input);

                case 2: // Список через запятую
                    return ProcessCommaSeparated(input);

                case 3: // Сложные слова через дефис
                    return ProcessHyphenatedWords(input);

                case 4: // Многострочный ввод
                    return ProcessMultilineInput(input);

                default:
                    return ReverseWord(input);
            }
        }

        private string ProcessMultilineInput(string input)
        {
            var lines = input.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(x => x.Trim())
                             .Where(x => !string.IsNullOrWhiteSpace(x));

            var processedLines = lines.Select(line =>
            {
                // Обрабатываем каждую строку в зависимости от ее содержания
                if (line.Contains(',') && line.Contains('-'))
                {
                    // Если строка содержит и запятые и дефисы - обрабатываем как сложные слова
                    return ProcessHyphenatedWords(line);
                }
                else if (line.Contains(','))
                {
                    // Если строка содержит запятые - обрабатываем как список
                    return ProcessCommaSeparated(line);
                }
                else if (line.Contains('-'))
                {
                    // Если строка содержит дефисы - обрабатываем как сложные слова
                    return ProcessHyphenatedWords(line);
                }
                else if (line.Contains(' '))
                {
                    // Если строка содержит пробелы - разбиваем на слова и переворачиваем каждое
                    var words = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    return string.Join("\n", words.Select(ReverseWord));
                }
                else
                {
                    // Одиночное слово
                    return ReverseWord(line);
                }
            });

            return string.Join("\n", processedLines);
        }

        private string ProcessFIO(string fio)
        {
            var parts = fio.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
            {
                // Фамилия Имя Отчество -> переворачиваем каждую часть и выводим с новой строки
                return string.Join("\n", parts.Select(ReverseWord));
            }
            else
            {
                // Одиночное слово
                return ReverseWord(fio);
            }
        }


        private string ProcessCommaSeparated(string input)
        {
            var items = input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(x => x.Trim())
                             .Where(x => !string.IsNullOrWhiteSpace(x));

            var reversedItems = items.Select(item =>
            {
                if (item.Contains(' '))
                {
                    // Если элемент содержит пробелы, обрабатываем как несколько слов
                    var words = item.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(ReverseWord);
                    return string.Join("\n", words);
                }
                else
                {
                    return ReverseWord(item);
                }
            });

            return string.Join("\n", reversedItems);
        }



        private string ProcessHyphenatedWords(string input)
        {
            var words = input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(x => x.Trim())
                             .Where(x => !string.IsNullOrWhiteSpace(x));

            var processedWords = words.Select(word =>
            {
                if (word.Contains('-'))
                {
                    // Разделяем по дефисам и переворачиваем каждую часть
                    var parts = word.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(ReverseWord);
                    return string.Join("-", parts);
                }
                else
                {
                    return ReverseWord(word);
                }
            });

            return string.Join("\n", processedWords);
        }

        private void ShowValidationMessage(string message)
        {
            lblValidationMessage.Text = message;
            lblValidationMessage.ForeColor = Color.Red;
        }


        private void BtnToggleReversal_Click(object sender, EventArgs e)
        {
            if (sender is Button btnToggle)
            {
                bool isCollapsed = (bool)btnToggle.Tag;

                if (isCollapsed)
                {
                    // Разворачиваем панель
                    reversalPanel.Size = new Size(300, 120);
                    btnToggle.Text = "−";
                    btnToggle.Tag = false;

                    // Показываем все элементы кроме заголовка
                    foreach (Control control in reversalPanel.Controls)
                    {
                        if (control is Panel) continue; // Заголовок не скрываем
                        control.Visible = true;
                    }
                }
                else
                {
                    // Сворачиваем панель
                    reversalPanel.Size = new Size(300, 40);
                    btnToggle.Text = "+";
                    btnToggle.Tag = true;

                    // Скрываем все элементы кроме заголовка
                    foreach (Control control in reversalPanel.Controls)
                    {
                        if (control is Panel) continue; // Заголовок не скрываем
                        control.Visible = false;
                    }
                }

                // Обновляем позицию после изменения размера
                UpdateReversalPanelPosition();
            }
        }


        // --- ФУНКЦИЯ ПЕРЕВОРОТА СЛОВА ---
        private string ReverseWord(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Разворачиваем строку
            char[] arr = input.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        // --- ОБРАБОТЧИК КНОПКИ ПЕРЕВОРОТА ---
        private void btnReverseWord_Click(object sender, EventArgs e)
        {
            try
            {
                string inputWord = txtInputWord.Text.Trim();

                // Проверяем, не является ли ввод placeholder'ом
                if (string.IsNullOrWhiteSpace(inputWord) || inputWord == "Введите слово")
                {
                    MessageBox.Show("Введите слово для переворота.",
                        "Подсказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtInputWord.Focus();
                    return;
                }

                string reversed = ReverseWord(inputWord);
                lblReversedWord.Text = $"Результат: {reversed}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении переворота слова:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

            // Кнопка настроек
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

            // Кнопки экспорта в Excel
            btnExportCandidates = new Button
            {
                Text = "Excel кандидаты",
                Size = new Size(120, 25),
                Location = new Point(650, 17),
                BackColor = DesignColors.SuccessColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8),
                FlatStyle = FlatStyle.Flat
            };
            btnExportCandidates.FlatAppearance.BorderSize = 0;
            btnExportCandidates.Click += BtnExportCandidates_Click;

            btnExportVacancies = new Button
            {
                Text = "Excel вакансии",
                Size = new Size(120, 25),
                Location = new Point(780, 17),
                BackColor = DesignColors.SuccessColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8),
                FlatStyle = FlatStyle.Flat
            };
            btnExportVacancies.FlatAppearance.BorderSize = 0;
            btnExportVacancies.Click += BtnExportVacancies_Click;

            btnExportApplications = new Button
            {
                Text = "Excel отклики",
                Size = new Size(120, 25),
                Location = new Point(910, 17),
                BackColor = DesignColors.SuccessColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8),
                FlatStyle = FlatStyle.Flat
            };
            btnExportApplications.FlatAppearance.BorderSize = 0;
            btnExportApplications.Click += BtnExportApplications_Click;

            btnTestRepository = new Button
            {
                Text = "Тест репозитория",
                Size = new Size(120, 25),
                Location = new Point(1040, 17),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnTestRepository.FlatAppearance.BorderSize = 0;
            btnTestRepository.Click += BtnTestRepository_Click;

            headerPanel.Controls.AddRange(new Control[] {
                lblTitle, btnSettings, lblViewType, cmbViewType,
                btnToggleFilters, btnExportCandidates, btnExportVacancies,
                btnExportApplications, btnTestRepository
            });
            // Элементы пагинации
            var lblPageSize = new Label
            {
                Text = "На странице:",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.White,
                Size = new Size(100, 20),
                Location = new Point(1200, 20)
            };

            cmbPageSize = new ComboBox
            {
                Size = new Size(60, 25),
                Location = new Point(1300, 17),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8)
            };
            cmbPageSize.Items.AddRange(new object[] { 20, 50, 100, 200 });
            cmbPageSize.SelectedItem = 50;
            cmbPageSize.SelectedIndexChanged += CmbPageSize_SelectedIndexChanged;

            btnPrevPage = new Button
            {
                Text = "◀",
                Size = new Size(30, 25),
                Location = new Point(1400, 17),
                BackColor = DesignColors.SecondaryColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat
            };
            btnPrevPage.FlatAppearance.BorderSize = 0;
            btnPrevPage.Click += BtnPrevPage_Click;

            lblPageInfo = new Label
            {
                Text = "Страница 1",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                Size = new Size(100, 20),
                Location = new Point(1430, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            btnNextPage = new Button
            {
                Text = "▶",
                Size = new Size(30, 25),
                Location = new Point(1530, 17),
                BackColor = DesignColors.SecondaryColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat
            };
            btnNextPage.FlatAppearance.BorderSize = 0;
            btnNextPage.Click += BtnNextPage_Click;


            headerPanel.Controls.AddRange(new Control[] {
        lblPageSize, cmbPageSize, btnPrevPage, lblPageInfo, btnNextPage
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
                Padding = new Padding(10)
            };

            int yPos = 15;

            // Тип поиска
            var lblSearchType = new Label
            {
                Text = "Искать в:",
                Location = new Point(10, yPos),
                Size = new Size(70, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = DesignColors.TextColor
            };

            cmbSearchType = new ComboBox
            {
                Location = new Point(85, yPos - 3),
                Size = new Size(130, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbSearchType.Items.AddRange(new[] { "Навыках", "Имени", "Специализации", "Вакансиях", "Статусе отклика" });
            cmbSearchType.SelectedIndex = 0;
            cmbSearchType.SelectedIndexChanged += OnFilterChanged;

            // Поиск
            var lblSearch = new Label
            {
                Text = "Поиск:",
                Location = new Point(230, yPos),
                Size = new Size(50, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = DesignColors.TextColor
            };
            txtSearch = new TextBox
            {
                Location = new Point(285, yPos - 3),
                Size = new Size(180, 25),
                Font = new Font("Segoe UI", 9)
            };
            txtSearch.TextChanged += OnFilterChanged;

            // Специализация
            var lblSpecialization = new Label
            {
                Text = "Специализация:",
                Location = new Point(480, yPos),
                Size = new Size(90, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = DesignColors.TextColor
            };
            cmbSpecialization = new ComboBox
            {
                Location = new Point(575, yPos - 3),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbSpecialization.SelectedIndexChanged += OnFilterChanged;

            yPos += 40;

            // Вакансия
            var lblVacancy = new Label
            {
                Text = "Вакансия:",
                Location = new Point(10, yPos),
                Size = new Size(70, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = DesignColors.TextColor
            };
            cmbVacancy = new ComboBox
            {
                Location = new Point(85, yPos - 3),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbVacancy.SelectedIndexChanged += OnFilterChanged;

            // Возраст
            lblAgeRange = new Label
            {
                Text = "Возраст: 18 - 70",
                Location = new Point(250, yPos),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = DesignColors.TextColor
            };

            lblMinAge = new Label
            {
                Text = "18",
                Location = new Point(355, yPos),
                Size = new Size(30, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = DesignColors.TextColor
            };
            trackBarMinAge = new TrackBar
            {
                Location = new Point(385, yPos - 5),
                Size = new Size(150, 30),
                Minimum = 18,
                Maximum = 70,
                Value = 18
            };
            trackBarMinAge.Scroll += OnAgeFilterChanged;

            lblMaxAge = new Label
            {
                Text = "70",
                Location = new Point(545, yPos),
                Size = new Size(30, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = DesignColors.TextColor
            };
            trackBarMaxAge = new TrackBar
            {
                Location = new Point(575, yPos - 5),
                Size = new Size(150, 30),
                Minimum = 18,
                Maximum = 70,
                Value = 70
            };
            trackBarMaxAge.Scroll += OnAgeFilterChanged;

            filtersPanel.Controls.AddRange(new Control[] {
                lblSearchType, cmbSearchType, lblSearch, txtSearch,
                lblSpecialization, cmbSpecialization, lblVacancy, cmbVacancy,
                lblAgeRange, lblMinAge, trackBarMinAge, lblMaxAge, trackBarMaxAge
            });
        }

        private void CreateMainContentPanel()
        {
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
            candidateCardPanel = new Panel
            {
                Width = 350,
                BackColor = DesignColors.CardColor,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                Padding = new Padding(15)
            };
        }

        private void BtnPrevPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                RefreshDataView();
            }
        }

        private void BtnNextPage_Click(object sender, EventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (currentPage < totalPages)
            {
                currentPage++;
                RefreshDataView();
            }
        }

        private void CmbPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPageSize.SelectedItem != null)
            {
                pageSize = (int)cmbPageSize.SelectedItem;
                currentPage = 1;
                RefreshDataView();
            }
        }

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

            // Обновляем позиции новых элементов
            UpdateExpandedPanelPosition();
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
            UpdateLayout();
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
            currentPage = 1;
            RefreshDataView();
            HideCandidateCard();
        }

        private void DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (cmbViewType.SelectedIndex == 0 && dataGridView.SelectedRows.Count > 0)
            {
                // Получаем ID кандидата из выбранной строки
                var selectedRow = dataGridView.SelectedRows[0];
                if (selectedRow.DataBoundItem != null)
                {
                    // Получаем CandidateId из анонимного типа
                    var candidateIdProperty = selectedRow.DataBoundItem.GetType().GetProperty("CandidateId");
                    if (candidateIdProperty != null)
                    {
                        int candidateId = (int)candidateIdProperty.GetValue(selectedRow.DataBoundItem);
                        var selectedCandidate = allCandidates.FirstOrDefault(c => c.CandidateId == candidateId);
                        if (selectedCandidate != null)
                        {
                            ShowCandidateCard(selectedCandidate);
                            return;
                        }
                    }
                }
            }
            HideCandidateCard();
        }

        private void UpdatePaginationInfo()
        {
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages == 0) totalPages = 1; // Минимум 1 страница

            lblPageInfo.Text = $"Страница {currentPage} из {totalPages}";

            // Обновляем состояние кнопок
            btnPrevPage.Enabled = currentPage > 1;
            btnNextPage.Enabled = currentPage < totalPages;

            // Показываем информацию о количестве записей
            lblPageInfo.Text = $"Страница {currentPage} из {totalPages} ({totalItems} записей)";
        }

        // === ЭКСПОРТ В EXCEL ===
        private void BtnExportCandidates_Click(object sender, EventArgs e)
        {
            ExportToExcel(allCandidates, "candidates.xlsx", "Кандидаты");
        }

        private void BtnExportVacancies_Click(object sender, EventArgs e)
        {
            ExportToExcel(allVacancies, "vacancies.xlsx", "Вакансии");
        }

        private void BtnExportApplications_Click(object sender, EventArgs e)
        {
            ExportToExcel(allApplications, "applications.xlsx", "Отклики");
        }

        private void ExportToExcel<T>(List<T> data, string defaultFileName, string sheetName)
        {
            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Excel Files|*.xlsx";
                    saveFileDialog.FileName = defaultFileName;
                    saveFileDialog.Title = "Сохранить Excel файл";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add(sheetName);

                            // Создаем DataTable из данных
                            var dataTable = new DataTable();

                            // Получаем свойства типа T
                            var properties = typeof(T).GetProperties();

                            // Создаем колонки (исключаем FullName)
                            foreach (var prop in properties)
                            {
                                if (prop.Name == "FullName") continue;

                                string columnName = GetDisplayName(prop.Name);
                                dataTable.Columns.Add(columnName, typeof(string));
                            }

                            // Заполняем данные
                            foreach (var item in data)
                            {
                                var row = dataTable.NewRow();
                                foreach (var prop in properties)
                                {
                                    if (prop.Name == "FullName") continue;

                                    var value = prop.GetValue(item);
                                    row[GetDisplayName(prop.Name)] = value?.ToString() ?? "";
                                }
                                dataTable.Rows.Add(row);
                            }

                            // Добавляем данные в Excel
                            worksheet.Cell(1, 1).InsertTable(dataTable);

                            // Авто-размер колонок
                            worksheet.Columns().AdjustToContents();

                            // Сохраняем файл В ВЫБРАННУЮ ПОЛЬЗОВАТЕЛЕМ ПАПКУ
                            workbook.SaveAs(saveFileDialog.FileName);

                            MessageBox.Show($"Данные экспортированы в:\n{saveFileDialog.FileName}", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetDisplayName(string propertyName)
        {
            var displayNames = new Dictionary<string, string>
            {
                { "CandidateId", "ID" },
                { "VacancyId", "ID вакансии" },
                { "ApplicationId", "ID отклика" },
                { "FirstName", "Имя" },
                { "LastName", "Фамилия" },
                { "Age", "Возраст" },
                { "Specialization", "Специализация" },
                { "Skills", "Навыки" },
                { "Title", "Название" },
                { "Description", "Описание" },
                { "RequiredSkills", "Требуемые навыки" },
                { "Status", "Статус" },
                { "CreatedAt", "Дата создания" },
                { "AppliedAt", "Дата отклика" },
                { "VacancyTitle", "Вакансия" },
                { "CandidateName", "Кандидат" },
                { "MinAge", "Мин. возраст" },
                { "MaxAge", "Макс. возраст" }
            };

            return displayNames.ContainsKey(propertyName) ? displayNames[propertyName] : propertyName;
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
                if (filterTimer.Enabled)
                {
                    currentPage = 1;
                }
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

            // Применение фильтров (существующий код без изменений)
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                string searchText = txtSearch.Text.ToLower();
                string searchType = cmbSearchType.SelectedItem?.ToString() ?? "Навыках";

                if (searchType == "Навыках")
                {
                    filteredCandidates = filteredCandidates.Where(c =>
                        c.Skills != null && c.Skills.ToLower().Contains(searchText));
                }
                else if (searchType == "Имени")
                {
                    filteredCandidates = filteredCandidates.Where(c =>
                        (c.FirstName != null && c.FirstName.ToLower().Contains(searchText)) ||
                        (c.LastName != null && c.LastName.ToLower().Contains(searchText)));
                }
                else if (searchType == "Специализации")
                {
                    filteredCandidates = filteredCandidates.Where(c =>
                        c.Specialization != null && c.Specialization.ToLower().Contains(searchText));
                }
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

            // ПАГИНАЦИЯ
            var result = filteredCandidates.ToList();
            totalItems = result.Count;

            var pagedResult = result
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            UpdatePaginationInfo();

            var displayData = pagedResult.Select(c => new
            {
                c.CandidateId,
                c.FirstName,
                c.LastName,
                c.Age,
                c.Specialization,
                c.Skills,
                c.CreatedAt
            }).ToList();

            dataGridView.DataSource = displayData;
            FormatCandidatesGrid();
        }

        private void DisplayVacancies()
        {
            var filteredVacancies = allVacancies.AsEnumerable();

            // Поиск по вакансиям
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                string searchText = txtSearch.Text.ToLower();
                string searchType = cmbSearchType.SelectedItem?.ToString() ?? "Навыках";

                if (searchType == "Навыках")
                {
                    filteredVacancies = filteredVacancies.Where(v =>
                        v.RequiredSkills != null && v.RequiredSkills.ToLower().Contains(searchText));
                }
                else if (searchType == "Вакансиях")
                {
                    filteredVacancies = filteredVacancies.Where(v =>
                        v.Title != null && v.Title.ToLower().Contains(searchText));
                }
            }

            // ПАГИНАЦИЯ ДЛЯ ВАКАНСИЙ
            var result = filteredVacancies.ToList();
            totalItems = result.Count;

            var pagedResult = result
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            UpdatePaginationInfo();

            dataGridView.DataSource = pagedResult;
            FormatVacanciesGrid();
        }

        private void DisplayApplications()
        {
            var filteredApplications = allApplications.AsEnumerable();

            // Поиск по откликам
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                string searchText = txtSearch.Text.ToLower();
                string searchType = cmbSearchType.SelectedItem?.ToString() ?? "Навыках";

                if (searchType == "Статусе отклика")
                {
                    filteredApplications = filteredApplications.Where(a =>
                        a.Status != null && a.Status.ToLower().Contains(searchText));
                }
                else if (searchType == "Вакансиях")
                {
                    filteredApplications = filteredApplications.Where(a =>
                        a.VacancyTitle != null && a.VacancyTitle.ToLower().Contains(searchText));
                }
            }

            // ПАГИНАЦИЯ ДЛЯ ОТКЛИКОВ
            var result = filteredApplications.ToList();
            totalItems = result.Count;

            var pagedResult = result
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            UpdatePaginationInfo();

            dataGridView.DataSource = pagedResult;
            FormatApplicationsGrid();
        }

        private void FormatCandidatesGrid()
        {
            // Скрываем ненужные колонки
            var columnsToHide = new[] { "CandidateId", "CreatedAt" };
            foreach (string columnName in columnsToHide)
            {
                if (dataGridView.Columns.Contains(columnName))
                    dataGridView.Columns[columnName].Visible = false;
            }

            // Настраиваем заголовки
            if (dataGridView.Columns.Contains("FirstName"))
                dataGridView.Columns["FirstName"].HeaderText = "Имя";
            if (dataGridView.Columns.Contains("LastName"))
                dataGridView.Columns["LastName"].HeaderText = "Фамилия";
            if (dataGridView.Columns.Contains("Age"))
                dataGridView.Columns["Age"].HeaderText = "Возраст";
            if (dataGridView.Columns.Contains("Specialization"))
                dataGridView.Columns["Specialization"].HeaderText = "Специализация";
            if (dataGridView.Columns.Contains("Skills"))
                dataGridView.Columns["Skills"].HeaderText = "Навыки";
        }

        private void FormatVacanciesGrid()
        {
            var columnsToHide = new[] { "VacancyId", "CreatedAt", "Description", "RequiredSkills", "MinAge", "MaxAge" };
            foreach (string columnName in columnsToHide)
            {
                if (dataGridView.Columns.Contains(columnName))
                    dataGridView.Columns[columnName].Visible = false;
            }

            if (dataGridView.Columns.Contains("Title"))
                dataGridView.Columns["Title"].HeaderText = "Название вакансии";
        }

        private void FormatApplicationsGrid()
        {
            var columnsToHide = new[] { "ApplicationId", "CandidateId", "VacancyId" };
            foreach (string columnName in columnsToHide)
            {
                if (dataGridView.Columns.Contains(columnName))
                    dataGridView.Columns[columnName].Visible = false;
            }

            if (dataGridView.Columns.Contains("VacancyTitle"))
                dataGridView.Columns["VacancyTitle"].HeaderText = "Вакансия";
            if (dataGridView.Columns.Contains("Status"))
                dataGridView.Columns["Status"].HeaderText = "Статус";
            if (dataGridView.Columns.Contains("AppliedAt"))
                dataGridView.Columns["AppliedAt"].HeaderText = "Дата отклика";
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
                Text = $"{candidate.FirstName} {candidate.LastName}",
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


            var lblApplicationsTitle = new Label
            {
                Text = $"Отклики ({applications.Count}):",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = DesignColors.TextColor,
                Size = new Size(320, 20),
                Location = new Point(0, yPos)
            };
            yPos += 25;


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

            UpdateLayout();
        }

        private Color GetStatusColor(string status)
        {
            if (status == null) return DesignColors.TextColor;

            string statusLower = status.ToLower();

            if (statusLower == "accepted" || statusLower == "принят")
                return Color.Green;
            else if (statusLower == "rejected" || statusLower == "отклонен")
                return Color.Red;
            else if (statusLower == "interview" || statusLower == "собеседование")
                return Color.Orange;
            else
                return DesignColors.TextColor;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ModernMainForm
            // 
            this.ClientSize = new System.Drawing.Size(806, 381);
            this.Name = "ModernMainForm";
            this.ResumeLayout(false);

        }

        private void HideCandidateCard()
        {
            candidateCardPanel.Visible = false;
            candidateCardPanel.Controls.Clear();
            UpdateLayout();
        }

        private void ApplyTheme()
        {
            this.BackColor = DesignColors.BackgroundColor;

            if (headerPanel != null) headerPanel.BackColor = DesignColors.PrimaryColor;
            if (filtersPanel != null) filtersPanel.BackColor = DesignColors.CardColor;
            if (mainContentPanel != null) mainContentPanel.BackColor = DesignColors.BackgroundColor;
            if (candidateCardPanel != null) candidateCardPanel.BackColor = DesignColors.CardColor;
            if (reversalPanel != null) reversalPanel.BackColor = Color.FromArgb(240, 240, 240);

            if (dataGridView != null)
            {
                dataGridView.BackgroundColor = DesignColors.CardColor;
                dataGridView.DefaultCellStyle.BackColor = DesignColors.CardColor;
                dataGridView.DefaultCellStyle.ForeColor = DesignColors.TextColor;
                dataGridView.ColumnHeadersDefaultCellStyle.BackColor = DesignColors.PrimaryColor;
            }
        }

        private async void BtnTestRepository_Click(object sender, EventArgs e)
        {
            try
            {
                btnTestRepository.Enabled = false;
                btnTestRepository.Text = "Тестирование...";


                btnExportCandidates.Enabled = false;
                btnExportVacancies.Enabled = false;
                btnExportApplications.Enabled = false;

                if (databaseHelper != null && databaseHelper.IsConnected())
                {
                    string currentConnectionString = ModernConnectionForm.CurrentConnectionString;

                    var repository = new CandidateRepository(currentConnectionString);


                    var repositoryCandidates = await repository.GetAllCandidatesAsync();

                    var oldCandidates = databaseHelper.GetCandidates();

                    MessageBox.Show($"✅ Репозиторий работает!\n\n" +
                                  $"Старый DatabaseHelper: {oldCandidates.Count} кандидатов\n" +
                                  $"Новый репозиторий: {repositoryCandidates.Count} кандидатов\n\n" +
                                  $"Результаты совпадают: {oldCandidates.Count == repositoryCandidates.Count}",
                                  "Тест репозитория - УСПЕХ",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("❌ Сначала подключитесь к базе данных!", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка тестирования: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {

                btnTestRepository.Enabled = true;
                btnTestRepository.Text = "Тест репозитория";
                btnExportCandidates.Enabled = true;
                btnExportVacancies.Enabled = true;
                btnExportApplications.Enabled = true;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            databaseHelper?.Disconnect();
            base.OnFormClosing(e);
        }
    }
}