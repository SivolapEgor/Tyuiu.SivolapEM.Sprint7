using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Tyuiu.SivolapEM.Sprint7.Project.V5.Lib;

namespace Project.V5
{
    public partial class Form1 : Form
    {
        // --- КОМПОНЕНТЫ UI ---
        private TabControl tabControlMain_SEM;
        private TabPage tabPageData_SEM, tabPageAnalytics_SEM;

        // Меню
        private Panel panelMenu_SEM;

        // Вкладка 1: Таблица и Управление
        private DataGridView gridData_SEM;
        private Panel panelControls_SEM;
        private TextBox txtCode_SEM, txtName_SEM, txtQty_SEM, txtPrice_SEM, txtSupplier_SEM, txtDate_SEM, txtDesc_SEM;
        private ComboBox cmbCategoryInput_SEM;
        private Button btnAdd_SEM, btnEdit_SEM, btnDelete_SEM;

        // Фильтры и Поиск
        private Panel panelFilter_SEM;
        private TextBox txtSearch_SEM;
        private ComboBox cmbCategoryFilter_SEM;

        // --- НОВОЕ: Блок детальной статистики ---
        private GroupBox grpStats_SEM;
        private Label lblStatCount_SEM, lblStatSum_SEM, lblStatAvg_SEM, lblStatMin_SEM, lblStatMax_SEM;

        // Вкладка 2: Графики
        private Chart chartBar_SEM, chartPie_SEM;
        private TableLayoutPanel layoutCharts_SEM;

        // Служебные
        private ContextMenuStrip contextMenuGrid_SEM; // ПКМ меню
        private DataService ds_SEM;
        private List<ItemModel> dataList_SEM;
        private List<ItemModel> currentViewList_SEM; // Список, который сейчас на экране (с учетом фильтра)
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;

        // Переменная для сортировки
        private bool isAscending = true;

        public Form1()
        {
            this.Text = "Склад | Sprint 7 | Сиволап Е.М.";
            this.Size = new Size(1350, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F);

            ds_SEM = new DataService();
            dataList_SEM = new List<ItemModel>();
            currentViewList_SEM = new List<ItemModel>();

            InitializeUI();
        }

        private void InitializeUI()
        {
            // 1. ЛЕВОЕ МЕНЮ
            panelMenu_SEM = new Panel { Dock = DockStyle.Left, Width = 200, BackColor = Color.FromArgb(33, 37, 41) };
            Label logo = new Label { Text = "SKLAD\nSYSTEM", ForeColor = Color.White, Font = new Font("Segoe UI", 18, FontStyle.Bold), Dock = DockStyle.Top, Height = 100, TextAlign = ContentAlignment.MiddleCenter };

            panelMenu_SEM.Controls.Add(CreateMenuBtn("Выход", 280, (s, e) => Application.Exit()));
            panelMenu_SEM.Controls.Add(CreateMenuBtn("О программе", 220, (s, e) =>
            {
                string info = "Автоматизированная информационная система «Оптовая база»\n" +
                              "Версия: 3.0 (Release)\n\n" +

                              "Разработчик: студент Сиволап Е.М.\n" +
                              "Предметная область: Складской учет и оптовая торговля\n" +
                              "Учебное задание: Спринт 7 | Вариант 5\n\n" +

                              "Назначение программы:\n" +
                              "Приложение предназначено для автоматизации процессов учета товаров, " +
                              "анализа остатков и формирования отчетности. Позволяет сократить время " +
                              "на обработку накладных и визуализировать финансовые показатели.\n\n" +

                              "Основные возможности:\n" +
                              "• Учет поступления и списания товаров (CSV БД);\n" +
                              "• Многофакторный поиск (Артикул, Поставщик, Дата);\n" +
                              "• Динамическая статистика и аналитические графики;\n" +
                              "• Контроль дефицита товара (цветовая индикация);\n" +
                              "• Сортировка и фильтрация данных.\n\n" +

                              "© ТИУ, 2025";

                MessageBox.Show(info, "Справка | Оптовая база", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));
            panelMenu_SEM.Controls.Add(CreateMenuBtn("Сохранить БД", 160, (s, e) => SaveFile()));
            panelMenu_SEM.Controls.Add(CreateMenuBtn("Загрузить БД", 100, (s, e) => LoadFile()));
            panelMenu_SEM.Controls.Add(logo);

            // 2. ВКЛАДКИ
            tabControlMain_SEM = new TabControl { Dock = DockStyle.Fill };
            tabPageData_SEM = new TabPage("База товаров");
            tabPageAnalytics_SEM = new TabPage("Статистика");
            tabControlMain_SEM.Controls.Add(tabPageData_SEM);
            tabControlMain_SEM.Controls.Add(tabPageAnalytics_SEM);

            // --- ВКЛАДКА 1: ДАННЫЕ ---

            // Панель фильтров (Верх)
            panelFilter_SEM = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.WhiteSmoke };

            // 1. Поиск (Заголовок)
            Label lblSearch = new Label
            {
                Text = "Поиск",
                Location = new Point(10, 8),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };

            // 2. Поле ввода
            txtSearch_SEM = new TextBox { Location = new Point(10, 32), Width = 250 };
            txtSearch_SEM.TextChanged += (s, e) => ApplyFilters();

            // 3. Мелкий текст под полем ввода
            Label lblSearchHint = new Label
            {
                Text = "Артикул / Наименование / Поставщик / Дата",
                Location = new Point(10, 58),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8),
                AutoSize = true
            };

            // 4. Фильтр по категории
            Label lblCatFilter = new Label
            {
                Text = "Фильтр по категории:",
                Location = new Point(280, 8),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };

            cmbCategoryFilter_SEM = new ComboBox { Location = new Point(280, 32), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategoryFilter_SEM.Items.Add("Все");
            cmbCategoryFilter_SEM.SelectedIndex = 0;
            cmbCategoryFilter_SEM.SelectedIndexChanged += (s, e) => ApplyFilters();

            // ГРУППА СТАТИСТИКИ
            grpStats_SEM = new GroupBox { Text = "Оперативная статистика", Location = new Point(500, 5), Size = new Size(600, 85) };
            lblStatCount_SEM = CreateStatLabel(grpStats_SEM, "Позиций: 0", 20, 25);
            lblStatSum_SEM = CreateStatLabel(grpStats_SEM, "Общая стоимость: 0 ₽", 150, 25);
            lblStatAvg_SEM = CreateStatLabel(grpStats_SEM, "Средняя цена: 0 ₽", 350, 25);
            lblStatMin_SEM = CreateStatLabel(grpStats_SEM, "Мин. цена: 0 ₽", 20, 55);
            lblStatMax_SEM = CreateStatLabel(grpStats_SEM, "Макс. цена: 0 ₽", 150, 55);

            panelFilter_SEM.Controls.Add(grpStats_SEM);
            panelFilter_SEM.Controls.Add(cmbCategoryFilter_SEM);
            panelFilter_SEM.Controls.Add(lblCatFilter);
            panelFilter_SEM.Controls.Add(lblSearchHint);
            panelFilter_SEM.Controls.Add(txtSearch_SEM);
            panelFilter_SEM.Controls.Add(lblSearch);

            // Панель редактирования
            panelControls_SEM = new Panel { Dock = DockStyle.Right, Width = 260, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            panelControls_SEM.Controls.Add(new Label { Text = "Карточка товара", Location = new Point(10, 10), Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true });

            txtCode_SEM = CreateInput(panelControls_SEM, "Артикул:", 50);
            txtName_SEM = CreateInput(panelControls_SEM, "Название:", 100);

            panelControls_SEM.Controls.Add(new Label { Text = "Категория:", Location = new Point(10, 150), AutoSize = true });
            cmbCategoryInput_SEM = new ComboBox { Location = new Point(10, 170), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategoryInput_SEM.Items.AddRange(new object[] { "Строительство", "Отделка", "Инструменты", "Электрика", "Сантехника", "Разное" });
            cmbCategoryInput_SEM.SelectedIndex = 0;
            panelControls_SEM.Controls.Add(cmbCategoryInput_SEM);

            txtQty_SEM = CreateInput(panelControls_SEM, "Количество:", 200);
            txtPrice_SEM = CreateInput(panelControls_SEM, "Цена:", 250);
            txtSupplier_SEM = CreateInput(panelControls_SEM, "Поставщик:", 300);
            txtDate_SEM = CreateInput(panelControls_SEM, "Дата поставки:", 350);

            panelControls_SEM.Controls.Add(new Label { Text = "Описание:", Location = new Point(10, 400), AutoSize = true, ForeColor = Color.DimGray });
            txtDesc_SEM = new TextBox { Location = new Point(10, 420), Width = 230, Height = 50, Multiline = true, ScrollBars = ScrollBars.Vertical };
            panelControls_SEM.Controls.Add(txtDesc_SEM);

            btnAdd_SEM = CreateBtn(panelControls_SEM, "ДОБАВИТЬ", Color.SeaGreen, 490, (s, e) => ActionAdd());
            btnEdit_SEM = CreateBtn(panelControls_SEM, "СОХРАНИТЬ", Color.Orange, 540, (s, e) => ActionEdit());
            btnDelete_SEM = CreateBtn(panelControls_SEM, "УДАЛИТЬ", Color.IndianRed, 590, (s, e) => ActionDelete());

            // Таблица
            gridData_SEM = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None
            };
            gridData_SEM.SelectionChanged += (s, e) => GridSelectionChanged();

            gridData_SEM.ColumnHeaderMouseClick += GridData_SEM_ColumnHeaderMouseClick;

            gridData_SEM.CellFormatting += GridData_SEM_CellFormatting;

            contextMenuGrid_SEM = new ContextMenuStrip();
            contextMenuGrid_SEM.Items.Add("Продать 1 шт.", null, (s, e) => ActionSellOne());
            contextMenuGrid_SEM.Items.Add("Удалить", null, (s, e) => ActionDelete());
            gridData_SEM.ContextMenuStrip = contextMenuGrid_SEM;

            tabPageData_SEM.Controls.Add(gridData_SEM);
            tabPageData_SEM.Controls.Add(panelControls_SEM);
            tabPageData_SEM.Controls.Add(panelFilter_SEM);


            // --- ВКЛАДКА 2: ГРАФИКИ ---
            layoutCharts_SEM = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            layoutCharts_SEM.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            layoutCharts_SEM.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            // ГРАФИК 1 (Столбцы)
            chartBar_SEM = new Chart { Dock = DockStyle.Fill };
            chartBar_SEM.ChartAreas.Add(new ChartArea("A1"));
            chartBar_SEM.Legends.Add(new Legend("L1"));
            chartBar_SEM.Series.Add(new Series("S1") { ChartType = SeriesChartType.Column });

            // ГРАФИК 2 (Круг)
            chartPie_SEM = new Chart { Dock = DockStyle.Fill };
            chartPie_SEM.ChartAreas.Add(new ChartArea("A2"));
            var legendPie = new Legend("LegendPie")
            {
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center
            };
            chartPie_SEM.Legends.Add(legendPie);
            chartPie_SEM.Series.Add(new Series("S2") { ChartType = SeriesChartType.Doughnut });

            layoutCharts_SEM.Controls.Add(chartBar_SEM, 0, 0);
            layoutCharts_SEM.Controls.Add(chartPie_SEM, 1, 0);
            tabPageAnalytics_SEM.Controls.Add(layoutCharts_SEM);

            this.Controls.Add(tabControlMain_SEM);
            this.Controls.Add(panelMenu_SEM);

            openFileDialog = new OpenFileDialog { Filter = "CSV|*.csv" };
            saveFileDialog = new SaveFileDialog { Filter = "CSV|*.csv", FileName = "SkladData.csv" };
        }


        private void LoadFile()
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                dataList_SEM = ds_SEM.LoadData(openFileDialog.FileName);

                var cats = dataList_SEM.Select(x => x.Category).Distinct().OrderBy(x => x).ToArray();
                cmbCategoryFilter_SEM.Items.Clear();
                cmbCategoryFilter_SEM.Items.Add("Все");
                cmbCategoryFilter_SEM.Items.AddRange(cats);
                cmbCategoryFilter_SEM.SelectedIndex = 0;

                ApplyFilters();
                UpdateCharts();
            }
        }

        private void SaveFile()
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                ds_SEM.SaveData(saveFileDialog.FileName, dataList_SEM);
                MessageBox.Show("Успешно сохранено!", "Система");
            }
        }

        private void ApplyFilters()
        {
            string q = txtSearch_SEM.Text.Trim().ToLower();
            string cat = cmbCategoryFilter_SEM.SelectedItem?.ToString() ?? "Все";

            // 2. Фильтрация
            currentViewList_SEM = dataList_SEM.Where(x =>
                (
                    (x.Name != null && x.Name.ToLower().Contains(q)) ||
                    (x.Code != null && x.Code.ToLower().Contains(q)) ||
                    (x.Supplier != null && x.Supplier.ToLower().Contains(q)) ||
                    (x.DeliveryDate != null && x.DeliveryDate.ToLower().Contains(q))
                )
                &&
                (cat == "Все" || (x.Category != null && x.Category == cat))
            ).ToList();

            gridData_SEM.DataSource = null;
            gridData_SEM.DataSource = currentViewList_SEM;

            ConfigureGrid();
            UpdateStatsPanel();
        }

        private void UpdateStatsPanel()
        {
            lblStatCount_SEM.Text = $"Позиций: {currentViewList_SEM.Count}";
            lblStatSum_SEM.Text = $"Общая стоимость: {ds_SEM.GetTotalStockValue(currentViewList_SEM):C2}";
            lblStatAvg_SEM.Text = $"Средняя цена: {ds_SEM.GetAveragePrice(currentViewList_SEM):C2}";
            lblStatMin_SEM.Text = $"Мин. цена: {ds_SEM.GetMinPrice(currentViewList_SEM):C2}";
            lblStatMax_SEM.Text = $"Макс. цена: {ds_SEM.GetMaxPrice(currentViewList_SEM):C2}";
        }

        private void ConfigureGrid()
        {
            if (gridData_SEM.Columns["Code"] != null) gridData_SEM.Columns["Code"].HeaderText = "Артикул";
            if (gridData_SEM.Columns["Name"] != null) gridData_SEM.Columns["Name"].HeaderText = "Наименование";
            if (gridData_SEM.Columns["Category"] != null) gridData_SEM.Columns["Category"].HeaderText = "Категория";
            if (gridData_SEM.Columns["Quantity"] != null) gridData_SEM.Columns["Quantity"].HeaderText = "Остаток";
            if (gridData_SEM.Columns["Price"] != null) { gridData_SEM.Columns["Price"].HeaderText = "Цена"; gridData_SEM.Columns["Price"].DefaultCellStyle.Format = "C2"; }
            if (gridData_SEM.Columns["Description"] != null) gridData_SEM.Columns["Description"].HeaderText = "Описание";
            if (gridData_SEM.Columns["Supplier"] != null) gridData_SEM.Columns["Supplier"].HeaderText = "Поставщик";
            if (gridData_SEM.Columns["DeliveryDate"] != null) gridData_SEM.Columns["DeliveryDate"].HeaderText = "Дата";
        }


        private void ActionAdd()
        {
            try
            {
                var item = new ItemModel
                {
                    Code = txtCode_SEM.Text,
                    Name = txtName_SEM.Text,
                    Category = cmbCategoryInput_SEM.Text,
                    Quantity = int.Parse(txtQty_SEM.Text),
                    Price = decimal.Parse(txtPrice_SEM.Text),
                    Supplier = txtSupplier_SEM.Text,
                    DeliveryDate = txtDate_SEM.Text,
                    Description = txtDesc_SEM.Text
                };
                dataList_SEM.Add(item);
                RefreshAll();
            }
            catch { MessageBox.Show("Проверьте формат чисел!"); }
        }

        private void ActionEdit()
        {
            if (gridData_SEM.SelectedRows.Count > 0)
            {
                var item = (ItemModel)gridData_SEM.SelectedRows[0].DataBoundItem;
                item.Code = txtCode_SEM.Text;
                item.Name = txtName_SEM.Text;
                item.Category = cmbCategoryInput_SEM.Text;
                int.TryParse(txtQty_SEM.Text, out int q); item.Quantity = q;
                decimal.TryParse(txtPrice_SEM.Text, out decimal p); item.Price = p;
                item.Supplier = txtSupplier_SEM.Text;
                item.DeliveryDate = txtDate_SEM.Text;
                item.Description = txtDesc_SEM.Text;
                RefreshAll();
            }
        }

        private void ActionDelete()
        {
            if (gridData_SEM.SelectedRows.Count > 0)
            {
                var item = (ItemModel)gridData_SEM.SelectedRows[0].DataBoundItem;
                if (MessageBox.Show($"Удалить {item.Name}?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    dataList_SEM.Remove(item);
                    RefreshAll();
                }
            }
        }

        private void ActionSellOne()
        {
            if (gridData_SEM.SelectedRows.Count > 0)
            {
                var item = (ItemModel)gridData_SEM.SelectedRows[0].DataBoundItem;
                if (item.Quantity > 0)
                {
                    item.Quantity--;
                    RefreshAll();
                }
                else
                {
                    MessageBox.Show("Товара нет на складе!");
                }
            }
        }

        private void RefreshAll()
        {
            ApplyFilters();
            UpdateCharts();
        }


        private void GridSelectionChanged()
        {
            if (gridData_SEM.SelectedRows.Count > 0)
            {
                var item = (ItemModel)gridData_SEM.SelectedRows[0].DataBoundItem;
                txtCode_SEM.Text = item.Code;
                txtName_SEM.Text = item.Name;
                cmbCategoryInput_SEM.Text = item.Category;
                txtQty_SEM.Text = item.Quantity.ToString();
                txtPrice_SEM.Text = item.Price.ToString();
                txtSupplier_SEM.Text = item.Supplier;
                txtDate_SEM.Text = item.DeliveryDate;
                txtDesc_SEM.Text = item.Description;
            }
        }

        private void GridData_SEM_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (gridData_SEM.Columns[e.ColumnIndex].Name == "Quantity")
            {
                if (e.Value != null && int.TryParse(e.Value.ToString(), out int qty))
                {
                    if (qty < 10)
                    {
                        gridData_SEM.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.MistyRose;
                        gridData_SEM.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.Red;
                    }
                }
            }
        }

        private void GridData_SEM_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string colName = gridData_SEM.Columns[e.ColumnIndex].DataPropertyName;

            if (isAscending)
                currentViewList_SEM = currentViewList_SEM.OrderBy(x => x.GetType().GetProperty(colName).GetValue(x, null)).ToList();
            else
                currentViewList_SEM = currentViewList_SEM.OrderByDescending(x => x.GetType().GetProperty(colName).GetValue(x, null)).ToList();

            isAscending = !isAscending;
            gridData_SEM.DataSource = currentViewList_SEM;
        }

        private void UpdateCharts()
        {
            // --- ГРАФИК 1: ГИСТОГРАММА ---
            chartBar_SEM.Series[0].Points.Clear();
            chartBar_SEM.Palette = ChartColorPalette.SeaGreen;
            chartBar_SEM.Titles.Clear();
            chartBar_SEM.Titles.Add("Топ дорогих позиций");

            chartBar_SEM.Series[0].Name = "Стоимость запасов (руб.)";

            chartBar_SEM.Series[0].IsValueShownAsLabel = true;
            chartBar_SEM.Series[0].IsXValueIndexed = true;

            // Настройка осей
            var axisX = chartBar_SEM.ChartAreas[0].AxisX;
            axisX.Interval = 1;
            axisX.LabelStyle.Interval = 1;
            axisX.LabelStyle.Angle = -45;
            axisX.LabelStyle.Font = new Font("Segoe UI", 9);

            var top = currentViewList_SEM.OrderByDescending(x => x.Price * x.Quantity).Take(10).ToList();
            foreach (var i in top)
            {
                chartBar_SEM.Series[0].Points.AddXY(i.Name, i.Price * i.Quantity);
            }

            // --- ГРАФИК 2: КРУГОВАЯ ---
            chartPie_SEM.Series[0].Points.Clear();
            chartPie_SEM.Palette = ChartColorPalette.BrightPastel;
            chartPie_SEM.Titles.Clear();
            chartPie_SEM.Titles.Add("Доли категорий (в деньгах)");

            var cats = currentViewList_SEM.GroupBy(x => x.Category)
                                          .Select(g => new { N = g.Key, V = g.Sum(x => x.Price * x.Quantity) });

            foreach (var c in cats)
            {
                if (c.V > 0)
                {
                    int idx = chartPie_SEM.Series[0].Points.AddXY(c.N, c.V);

                    chartPie_SEM.Series[0].Points[idx].Label = "#PERCENT";
                    chartPie_SEM.Series[0].Points[idx].LegendText = c.N;
                }
            }
        }

        private Button CreateMenuBtn(string t, int top, EventHandler act)
        {
            var b = new Button { Text = t, Top = top, Left = 0, Width = 200, Height = 50, FlatStyle = FlatStyle.Flat, ForeColor = Color.LightGray, BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(20, 0, 0, 0) };
            b.Click += act; return b;
        }
        private TextBox CreateInput(Panel p, string l, int t)
        {
            p.Controls.Add(new Label { Text = l, Location = new Point(10, t), AutoSize = true, ForeColor = Color.DimGray });
            var tb = new TextBox { Location = new Point(10, t + 20), Width = 230 }; p.Controls.Add(tb); return tb;
        }
        private Button CreateBtn(Panel p, string t, Color c, int top, EventHandler act)
        {
            var b = new Button { Text = t, BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(10, top), Size = new Size(230, 40) };
            b.Click += act; p.Controls.Add(b); return b;
        }
        private Label CreateStatLabel(GroupBox g, string txt, int x, int y)
        {
            var l = new Label { Text = txt, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            g.Controls.Add(l); return l;
        }
    }
}