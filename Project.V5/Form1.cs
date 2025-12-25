using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Tyuiu.SivolapEM.Sprint7.Project.V5.Lib;

namespace Project.V5
{
    public partial class Form1 : Form
    {
        private MenuStrip menuStrip_SEM;
        private TabControl tabControlMain_SEM;
        private TabPage tabPageData_SEM, tabPageAnalytics_SEM;
        private Panel panelMenu_SEM;

        private DataGridView gridData_SEM;
        private Panel panelControls_SEM;
        private TextBox txtCode_SEM, txtName_SEM, txtQty_SEM, txtPrice_SEM, txtPurchase_SEM, txtSupplier_SEM, txtDesc_SEM;
        private DateTimePicker dtpDate_SEM;
        private ComboBox cmbCategoryInput_SEM;
        private Button btnAdd_SEM, btnEdit_SEM, btnDelete_SEM;

        private Panel panelFilter_SEM;
        private TextBox txtSearch_SEM;
        private ComboBox cmbCategoryFilter_SEM, cmbSupplierFilter_SEM;
        private NumericUpDown numPriceMin_SEM, numPriceMax_SEM, numQtyMin_SEM, numQtyMax_SEM;
        private Button btnResetFilter_SEM;

        private GroupBox grpStats_SEM;
        private Label lblStatCount_SEM, lblStatSum_SEM, lblStatProfit_SEM;
        private Label lblStatAvg_SEM, lblStatMin_SEM, lblStatMax_SEM;

        private Chart chartBar_SEM, chartPie_SEM;
        private TableLayoutPanel layoutCharts_SEM;

        private ContextMenuStrip contextMenuGrid_SEM;
        private DataService ds_SEM;
        private List<ItemModel> dataList_SEM;
        private List<ItemModel> currentViewList_SEM;
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private bool isAscending = true;

        public Form1()
        {
            this.Text = "Склад | Sprint 7 | Сиволап Е.М.";
            this.Size = new Size(1400, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F);

            ds_SEM = new DataService();
            dataList_SEM = new List<ItemModel>();
            currentViewList_SEM = new List<ItemModel>();

            InitializeUI();
        }

        private void InitializeUI()
        {
            this.BackColor = Color.White;

            ToolTip toolTip = new ToolTip();
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 500;
            toolTip.ShowAlways = true;

            menuStrip_SEM = new MenuStrip { BackColor = Color.WhiteSmoke, RenderMode = ToolStripRenderMode.System };

            var fileMenu = new ToolStripMenuItem("Файл");
            fileMenu.DropDownItems.Add("📂 Загрузить БД", null, (s, e) => LoadFile());
            fileMenu.DropDownItems.Add("💾 Сохранить БД", null, (s, e) => SaveFile());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Выход", null, (s, e) => Application.Exit());

            var toolsMenu = new ToolStripMenuItem("Инструменты");
            toolsMenu.DropDownItems.Add("Мастер закупок", null, (s, e) => OpenOrderWindow());
            toolsMenu.DropDownItems.Add("Открыть лог", null, (s, e) => { if (File.Exists("history.log")) Process.Start(new ProcessStartInfo("history.log") { UseShellExecute = true }); });

            var helpMenu = new ToolStripMenuItem("Справка");
            helpMenu.DropDownItems.Add("О программе", null, (s, e) => ShowAbout());

            menuStrip_SEM.Items.AddRange(new ToolStripItem[] { fileMenu, toolsMenu, helpMenu });


            panelMenu_SEM = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Color.FromArgb(33, 37, 41) };

            Label logo = new Label
            {
                Text = "📦 SKLAD\nSYSTEM",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 100,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var btnExit = CreateMenuBtn("🚪  Выход", 340, (s, e) => Application.Exit());
            toolTip.SetToolTip(btnExit, "Закрыть приложение");

            var btnAbout = CreateMenuBtn("ℹ️   О программе", 280, (s, e) => ShowAbout());
            toolTip.SetToolTip(btnAbout, "Информация о разработчике");

            var btnOrder = CreateMenuBtn("🛒  Мастер ЗАКУПОК", 220, (s, e) => OpenOrderWindow());
            btnOrder.ForeColor = Color.FromArgb(255, 193, 7);
            toolTip.SetToolTip(btnOrder, "Сформировать отчет по дефициту товара");

            var btnSave = CreateMenuBtn("💾  Сохранить БД", 160, (s, e) => SaveFile());
            toolTip.SetToolTip(btnSave, "Сохранить изменения в файл");

            var btnLoad = CreateMenuBtn("📂  Загрузить БД", 100, (s, e) => LoadFile());
            toolTip.SetToolTip(btnLoad, "Открыть файл базы данных");

            panelMenu_SEM.Controls.AddRange(new Control[] { btnExit, btnAbout, btnOrder, btnSave, btnLoad, logo });


            tabControlMain_SEM = new TabControl { Dock = DockStyle.Fill };
            tabPageData_SEM = new TabPage("📦 База товаров");
            tabPageAnalytics_SEM = new TabPage("📊 Дашборд");
            tabControlMain_SEM.Controls.Add(tabPageData_SEM);
            tabControlMain_SEM.Controls.Add(tabPageAnalytics_SEM);


            panelFilter_SEM = new Panel { Dock = DockStyle.Top, Height = 140, BackColor = Color.WhiteSmoke, Padding = new Padding(10) };


            Label lblSearch = new Label { Text = "Поиск:", Location = new Point(20, 15), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtSearch_SEM = new TextBox { Location = new Point(20, 35), Width = 200, Font = new Font("Segoe UI", 10) };
            txtSearch_SEM.TextChanged += (s, e) => ApplyFilters();
            toolTip.SetToolTip(txtSearch_SEM, "Поиск по Названию, Артикулу, Поставщику или Дате");

            Label lblCat = new Label { Text = "Категория:", Location = new Point(240, 15), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            cmbCategoryFilter_SEM = new ComboBox { Location = new Point(240, 35), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cmbCategoryFilter_SEM.Items.Add("Все");
            cmbCategoryFilter_SEM.SelectedIndex = 0;
            cmbCategoryFilter_SEM.SelectedIndexChanged += (s, e) => ApplyFilters();
            toolTip.SetToolTip(cmbCategoryFilter_SEM, "Фильтр по категории");

            Label lblSupp = new Label { Text = "Поставщик:", Location = new Point(410, 15), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            cmbSupplierFilter_SEM = new ComboBox { Location = new Point(410, 35), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cmbSupplierFilter_SEM.Items.Add("Все");
            cmbSupplierFilter_SEM.SelectedIndex = 0;
            cmbSupplierFilter_SEM.SelectedIndexChanged += (s, e) => ApplyFilters();
            toolTip.SetToolTip(cmbSupplierFilter_SEM, "Фильтр по поставщику");

            btnResetFilter_SEM = new Button
            {
                Text = "❌ Сброс",
                Location = new Point(580, 34),
                Width = 80,
                Height = 29,
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnResetFilter_SEM.FlatAppearance.BorderSize = 0;
            btnResetFilter_SEM.Click += (s, e) => ResetFilters();
            toolTip.SetToolTip(btnResetFilter_SEM, "Сбросить все фильтры");


            Label lblPrice = new Label { Text = "Цена (руб):", Location = new Point(20, 80), AutoSize = true, ForeColor = Color.DimGray };
            Label lblP1 = new Label { Text = "от", Location = new Point(20, 103), AutoSize = true, Font = new Font("Segoe UI", 8) };
            numPriceMin_SEM = CreateFilterNum(panelFilter_SEM, 40, 100, 80);
            toolTip.SetToolTip(numPriceMin_SEM, "Мин. цена");

            Label lblP2 = new Label { Text = "до", Location = new Point(130, 103), AutoSize = true, Font = new Font("Segoe UI", 8) };
            numPriceMax_SEM = CreateFilterNum(panelFilter_SEM, 150, 100, 80);
            toolTip.SetToolTip(numPriceMax_SEM, "Макс. цена (0 = без ограничений)");

            Label lblQty = new Label { Text = "Количество (шт):", Location = new Point(260, 80), AutoSize = true, ForeColor = Color.DimGray };
            Label lblQ1 = new Label { Text = "от", Location = new Point(260, 103), AutoSize = true, Font = new Font("Segoe UI", 8) };
            numQtyMin_SEM = CreateFilterNum(panelFilter_SEM, 280, 100, 80);
            toolTip.SetToolTip(numQtyMin_SEM, "Мин. остаток");

            Label lblQ2 = new Label { Text = "до", Location = new Point(370, 103), AutoSize = true, Font = new Font("Segoe UI", 8) };
            numQtyMax_SEM = CreateFilterNum(panelFilter_SEM, 390, 100, 80);
            toolTip.SetToolTip(numQtyMax_SEM, "Макс. остаток");


            grpStats_SEM = new GroupBox { Text = "Итоги выборки", Location = new Point(700, 5), Size = new Size(600, 125), Font = new Font("Segoe UI", 9) };

            lblStatCount_SEM = CreateStatLabel(grpStats_SEM, "Позиций: 0", 20, 25);
            lblStatSum_SEM = CreateStatLabel(grpStats_SEM, "Стоимость: 0 ₽", 20, 55);
            lblStatProfit_SEM = CreateStatLabel(grpStats_SEM, "Прибыль: 0 ₽", 20, 85);
            lblStatProfit_SEM.ForeColor = Color.SeaGreen;

            lblStatAvg_SEM = CreateStatLabel(grpStats_SEM, "Средняя цена: 0 ₽", 250, 25);
            lblStatMin_SEM = CreateStatLabel(grpStats_SEM, "Мин. цена: 0 ₽", 250, 55);
            lblStatMax_SEM = CreateStatLabel(grpStats_SEM, "Макс. цена: 0 ₽", 250, 85);

            panelFilter_SEM.Controls.AddRange(new Control[] {
                lblSearch, txtSearch_SEM, lblCat, cmbCategoryFilter_SEM, lblSupp, cmbSupplierFilter_SEM, btnResetFilter_SEM,
                lblPrice, lblP1, lblP2, lblQty, lblQ1, lblQ2, grpStats_SEM
            });


            panelControls_SEM = new Panel { Dock = DockStyle.Right, Width = 280, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };
            panelControls_SEM.Controls.Add(new Label { Text = "Карточка товара", Dock = DockStyle.Top, Font = new Font("Segoe UI", 12, FontStyle.Bold), Height = 30 });

            txtCode_SEM = CreateInput(panelControls_SEM, "Артикул:", 40);
            toolTip.SetToolTip(txtCode_SEM, "Уникальный код (обязательно)");

            txtName_SEM = CreateInput(panelControls_SEM, "Название:", 90);
            toolTip.SetToolTip(txtName_SEM, "Наименование товара");

            panelControls_SEM.Controls.Add(new Label { Text = "Категория:", Location = new Point(10, 140), AutoSize = true, ForeColor = Color.DimGray });
            cmbCategoryInput_SEM = new ComboBox { Location = new Point(10, 160), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cmbCategoryInput_SEM.Items.AddRange(new object[] { "Строительство", "Отделка", "Инструменты", "Электрика", "Сантехника", "Разное" });
            cmbCategoryInput_SEM.SelectedIndex = 0;
            panelControls_SEM.Controls.Add(cmbCategoryInput_SEM);

            txtQty_SEM = CreateInput(panelControls_SEM, "Количество:", 190);
            txtPrice_SEM = CreateInput(panelControls_SEM, "Цена продажи:", 240);
            txtPurchase_SEM = CreateInput(panelControls_SEM, "Цена закупки:", 290);
            txtSupplier_SEM = CreateInput(panelControls_SEM, "Поставщик:", 340);

            panelControls_SEM.Controls.Add(new Label { Text = "Дата поставки:", Location = new Point(10, 390), AutoSize = true, ForeColor = Color.DimGray });
            dtpDate_SEM = new DateTimePicker { Location = new Point(10, 410), Width = 250, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 10) };
            panelControls_SEM.Controls.Add(dtpDate_SEM);
            toolTip.SetToolTip(dtpDate_SEM, "Дата последнего поступления");

            panelControls_SEM.Controls.Add(new Label { Text = "Описание:", Location = new Point(10, 440), AutoSize = true, ForeColor = Color.DimGray });
            txtDesc_SEM = new TextBox { Location = new Point(10, 460), Width = 250, Height = 50, Multiline = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Segoe UI", 9) };
            panelControls_SEM.Controls.Add(txtDesc_SEM);

            btnAdd_SEM = CreateBtn(panelControls_SEM, "➕  ДОБАВИТЬ", Color.SeaGreen, 530, (s, e) => ActionAdd());
            toolTip.SetToolTip(btnAdd_SEM, "Создать новую запись");

            btnEdit_SEM = CreateBtn(panelControls_SEM, "✏️  СОХРАНИТЬ", Color.Orange, 580, (s, e) => ActionEdit());
            toolTip.SetToolTip(btnEdit_SEM, "Применить изменения");

            btnDelete_SEM = CreateBtn(panelControls_SEM, "🗑️  УДАЛИТЬ", Color.IndianRed, 630, (s, e) => ActionDelete());
            toolTip.SetToolTip(btnDelete_SEM, "Удалить запись");


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
            gridData_SEM.ColumnHeaderMouseClick += (s, e) => SortGrid(e.ColumnIndex);
            gridData_SEM.CellFormatting += GridData_SEM_CellFormatting;

            contextMenuGrid_SEM = new ContextMenuStrip();
            contextMenuGrid_SEM.Items.Add("Продать 1 шт.", null, (s, e) => ActionSellOne());
            contextMenuGrid_SEM.Items.Add("Удалить", null, (s, e) => ActionDelete());
            gridData_SEM.ContextMenuStrip = contextMenuGrid_SEM;

            tabPageData_SEM.Controls.Add(gridData_SEM);
            tabPageData_SEM.Controls.Add(panelControls_SEM);
            tabPageData_SEM.Controls.Add(panelFilter_SEM);


            layoutCharts_SEM = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(10) };
            layoutCharts_SEM.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            layoutCharts_SEM.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            chartBar_SEM = new Chart { Dock = DockStyle.Fill };
            chartBar_SEM.ChartAreas.Add(new ChartArea("A1"));
            chartBar_SEM.Legends.Add(new Legend("L1"));
            chartBar_SEM.Series.Add(new Series("Profit") { ChartType = SeriesChartType.Column });

            chartPie_SEM = new Chart { Dock = DockStyle.Fill };
            chartPie_SEM.ChartAreas.Add(new ChartArea("A2"));
            chartPie_SEM.Legends.Add(new Legend("LegendPie") { Docking = Docking.Bottom, Alignment = StringAlignment.Center });
            chartPie_SEM.Series.Add(new Series("S2") { ChartType = SeriesChartType.Doughnut });

            layoutCharts_SEM.Controls.Add(chartBar_SEM, 0, 0);
            layoutCharts_SEM.Controls.Add(chartPie_SEM, 1, 0);
            tabPageAnalytics_SEM.Controls.Add(layoutCharts_SEM);


            this.Controls.Add(tabControlMain_SEM);
            this.Controls.Add(panelMenu_SEM);
            this.Controls.Add(menuStrip_SEM);
            this.MainMenuStrip = menuStrip_SEM;

            openFileDialog = new OpenFileDialog { Filter = "CSV|*.csv" };
            saveFileDialog = new SaveFileDialog { Filter = "CSV|*.csv", FileName = "SkladData.csv" };
        }
        private NumericUpDown CreateFilterNum(Panel p, int x, int y, int width)
        {
            var num = new NumericUpDown { Location = new Point(x, y), Width = width, Maximum = 1000000, Font = new Font("Segoe UI", 9) };
            num.ValueChanged += (s, e) => ApplyFilters();
            p.Controls.Add(num);
            return num;
        }


        private void LoadFile()
        {
            try
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    dataList_SEM = ds_SEM.LoadData(openFileDialog.FileName);
                    if (dataList_SEM.Count == 0) return;

                    var cats = dataList_SEM.Select(x => x.Category).Distinct().OrderBy(x => x).ToArray();
                    cmbCategoryFilter_SEM.Items.Clear();
                    cmbCategoryFilter_SEM.Items.Add("Все");
                    cmbCategoryFilter_SEM.Items.AddRange(cats);
                    cmbCategoryFilter_SEM.SelectedIndex = 0;

                    if (cmbSupplierFilter_SEM != null)
                    {
                        var supps = dataList_SEM.Select(x => x.Supplier).Distinct().OrderBy(x => x).ToArray();
                        cmbSupplierFilter_SEM.Items.Clear();
                        cmbSupplierFilter_SEM.Items.Add("Все");
                        cmbSupplierFilter_SEM.Items.AddRange(supps);
                        cmbSupplierFilter_SEM.SelectedIndex = 0;
                    }

                    ApplyFilters();
                    ds_SEM.WriteLog("Загружена база: " + Path.GetFileName(openFileDialog.FileName));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void SaveFile()
        {
            try
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ds_SEM.SaveData(saveFileDialog.FileName, dataList_SEM);
                    ds_SEM.WriteLog("Сохранено: " + Path.GetFileName(saveFileDialog.FileName));
                    MessageBox.Show("Успешно!");
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }

        private void ApplyFilters()
        {
            if (cmbSupplierFilter_SEM == null || cmbCategoryFilter_SEM == null ||
                numPriceMin_SEM == null || numPriceMax_SEM == null ||
                numQtyMin_SEM == null || numQtyMax_SEM == null)
            {
                return;
            }

            string q = txtSearch_SEM.Text.Trim().ToLower();
            string cat = cmbCategoryFilter_SEM.SelectedItem?.ToString() ?? "Все";

            string supp = cmbSupplierFilter_SEM.SelectedItem?.ToString() ?? "Все";

            decimal minP = numPriceMin_SEM.Value;
            decimal maxP = numPriceMax_SEM.Value == 0 ? decimal.MaxValue : numPriceMax_SEM.Value;
            decimal minQ = numQtyMin_SEM.Value;
            decimal maxQ = numQtyMax_SEM.Value == 0 ? decimal.MaxValue : numQtyMax_SEM.Value;

            currentViewList_SEM = dataList_SEM.Where(x =>
                ((x.Name != null && x.Name.ToLower().Contains(q)) || (x.Code != null && x.Code.ToLower().Contains(q)) || (x.DeliveryDate != null && x.DeliveryDate.ToLower().Contains(q))) &&
                (cat == "Все" || (x.Category != null && x.Category == cat)) &&
                (supp == "Все" || (x.Supplier != null && x.Supplier == supp)) &&
                (x.Price >= minP && x.Price <= maxP) &&
                (x.Quantity >= minQ && x.Quantity <= maxQ)
            ).ToList();

            gridData_SEM.DataSource = null;
            gridData_SEM.DataSource = currentViewList_SEM;
            ConfigureGrid();
            UpdateStatsPanel();
            UpdateCharts();
        }

        private void ResetFilters()
        {
            txtSearch_SEM.Text = "";
            cmbCategoryFilter_SEM.SelectedIndex = 0;
            if (cmbSupplierFilter_SEM.Items.Count > 0) cmbSupplierFilter_SEM.SelectedIndex = 0;
            numPriceMin_SEM.Value = 0; numPriceMax_SEM.Value = 0;
            numQtyMin_SEM.Value = 0; numQtyMax_SEM.Value = 0;
        }

        private void UpdateStatsPanel()
        {
            lblStatCount_SEM.Text = $"Позиций: {currentViewList_SEM.Count}";
            lblStatSum_SEM.Text = $"Стоимость склада: {ds_SEM.GetTotalStockValue(currentViewList_SEM):C2}";
            lblStatProfit_SEM.Text = $"Потенц. прибыль: {ds_SEM.GetPotentialProfit(currentViewList_SEM):C2}";
            lblStatAvg_SEM.Text = $"Средняя цена: {ds_SEM.GetAveragePrice(currentViewList_SEM):C2}";
            lblStatMin_SEM.Text = $"Мин. цена: {ds_SEM.GetMinPrice(currentViewList_SEM):C2}";
            lblStatMax_SEM.Text = $"Макс. цена: {ds_SEM.GetMaxPrice(currentViewList_SEM):C2}";
        }

        private void ConfigureGrid()
        {
            if (gridData_SEM.Columns["Code"] != null) gridData_SEM.Columns["Code"].HeaderText = "Артикул";
            if (gridData_SEM.Columns["Name"] != null) gridData_SEM.Columns["Name"].HeaderText = "Товар";
            if (gridData_SEM.Columns["Category"] != null) gridData_SEM.Columns["Category"].HeaderText = "Категория";
            if (gridData_SEM.Columns["Quantity"] != null) gridData_SEM.Columns["Quantity"].HeaderText = "Остаток";
            if (gridData_SEM.Columns["Price"] != null) { gridData_SEM.Columns["Price"].HeaderText = "Цена прод."; gridData_SEM.Columns["Price"].DefaultCellStyle.Format = "C2"; }
            if (gridData_SEM.Columns["PurchasePrice"] != null) { gridData_SEM.Columns["PurchasePrice"].HeaderText = "Цена закуп."; gridData_SEM.Columns["PurchasePrice"].DefaultCellStyle.Format = "C2"; }
            if (gridData_SEM.Columns["Description"] != null) gridData_SEM.Columns["Description"].HeaderText = "Описание";
            if (gridData_SEM.Columns["Supplier"] != null) gridData_SEM.Columns["Supplier"].HeaderText = "Поставщик";
            if (gridData_SEM.Columns["DeliveryDate"] != null) gridData_SEM.Columns["DeliveryDate"].HeaderText = "Дата";
        }


        private void ActionAdd()
        {
            if (!ValidateInputs()) return;

            if (dataList_SEM.Any(x => x.Code.ToLower() == txtCode_SEM.Text.Trim().ToLower()))
            {
                MessageBox.Show($"Товар с артикулом '{txtCode_SEM.Text}' уже существует!\nИспользуйте другой код.",
                                "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var item = new ItemModel
            {
                Code = txtCode_SEM.Text.Trim(),
                Name = txtName_SEM.Text.Trim(),
                Category = cmbCategoryInput_SEM.Text,
                Quantity = int.Parse(txtQty_SEM.Text),
                Price = decimal.Parse(txtPrice_SEM.Text),
                PurchasePrice = decimal.Parse(txtPurchase_SEM.Text),
                Supplier = txtSupplier_SEM.Text,
                DeliveryDate = dtpDate_SEM.Value.ToString("dd.MM.yyyy"),
                Description = txtDesc_SEM.Text
            };

            ds_SEM.AddItem(dataList_SEM, item);
            ds_SEM.WriteLog($"Добавлен: {item.Name}");
            ApplyFilters();
            MessageBox.Show("Товар успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ActionEdit()
        {
            if (gridData_SEM.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите товар в таблице, который хотите изменить!", "Ничего не выбрано", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidateInputs()) return;

            var item = (ItemModel)gridData_SEM.SelectedRows[0].DataBoundItem;

            string newCode = txtCode_SEM.Text.Trim();

            if (item.Code != newCode && dataList_SEM.Any(x => x.Code.ToLower() == newCode.ToLower()))
            {
                MessageBox.Show($"Артикул '{newCode}' уже принадлежит другому товару!\nАртикул должен быть уникальным.",
                                "Ошибка дубликата", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<string> changes = new List<string>();
            string newDate = dtpDate_SEM.Value.ToString("dd.MM.yyyy");

            decimal newPrice = decimal.Parse(txtPrice_SEM.Text);
            int newQty = int.Parse(txtQty_SEM.Text);
            decimal newPurch = decimal.Parse(txtPurchase_SEM.Text);

            if (item.Price != newPrice) changes.Add($"Цена продажи: {item.Price} -> {newPrice}");
            if (item.Quantity != newQty) changes.Add($"Кол-во: {item.Quantity} -> {newQty}");
            if (item.PurchasePrice != newPurch) changes.Add($"Закупка: {item.PurchasePrice} -> {newPurch}");
            if (item.Code != newCode) changes.Add($"Артикул: {item.Code} -> {newCode}");
            if (item.DeliveryDate != newDate) changes.Add($"Дата: {item.DeliveryDate} -> {newDate}");

            item.Code = newCode;
            item.Name = txtName_SEM.Text.Trim();
            item.Category = cmbCategoryInput_SEM.Text;
            item.Quantity = newQty;
            item.Price = newPrice;
            item.PurchasePrice = newPurch;
            item.Supplier = txtSupplier_SEM.Text.Trim();
            item.DeliveryDate = newDate; 
            item.Description = txtDesc_SEM.Text;

            if (changes.Count > 0)
            {
                ds_SEM.WriteLog($"Изменен товар '{item.Name}': {string.Join("; ", changes)}");
            }
            else
            {
                ds_SEM.WriteLog($"Обновлена информация о товаре '{item.Name}' (без критических изменений)");
            }

            ApplyFilters();
            MessageBox.Show("Изменения успешно сохранены!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ActionDelete()
        {
            if (gridData_SEM.SelectedRows.Count > 0 && MessageBox.Show("Удалить?", "Вопрос", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ds_SEM.RemoveItem(dataList_SEM, (ItemModel)gridData_SEM.SelectedRows[0].DataBoundItem);
                ApplyFilters();
            }
        }

        private void ActionSellOne()
        {
            if (gridData_SEM.SelectedRows.Count > 0)
            {
                var item = (ItemModel)gridData_SEM.SelectedRows[0].DataBoundItem;
                if (item.Quantity > 0) { item.Quantity--; ApplyFilters(); }
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtCode_SEM.Text))
            {
                MessageBox.Show("Поле 'Артикул' не может быть пустым!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCode_SEM.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtName_SEM.Text))
            {
                MessageBox.Show("Поле 'Название' не может быть пустым!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName_SEM.Focus();
                return false;
            }

            if (!int.TryParse(txtQty_SEM.Text, out int qty))
            {
                MessageBox.Show("В поле 'Количество' должны быть только цифры!", "Ошибка формата", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtQty_SEM.Focus();
                return false;
            }
            if (qty < 0)
            {
                MessageBox.Show("Количество не может быть отрицательным!", "Логическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtQty_SEM.Focus();
                return false;
            }

            if (!decimal.TryParse(txtPrice_SEM.Text, out decimal price))
            {
                MessageBox.Show("В поле 'Цена продажи' введено некорректное число!", "Ошибка формата", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice_SEM.Focus();
                return false;
            }
            if (price < 0)
            {
                MessageBox.Show("Цена не может быть отрицательной!", "Логическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(txtPurchase_SEM.Text, out decimal purch))
            {
                MessageBox.Show("В поле 'Цена закупки' введено некорректное число!", "Ошибка формата", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPurchase_SEM.Focus();
                return false;
            }
            if (purch < 0)
            {
                MessageBox.Show("Закупка не может быть отрицательной!", "Логическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void GridSelectionChanged()
        {
            if (gridData_SEM.SelectedRows.Count > 0)
            {
                var item = (ItemModel)gridData_SEM.SelectedRows[0].DataBoundItem;
                txtCode_SEM.Text = item.Code; txtName_SEM.Text = item.Name; cmbCategoryInput_SEM.Text = item.Category;
                txtQty_SEM.Text = item.Quantity.ToString(); txtPrice_SEM.Text = item.Price.ToString(); txtPurchase_SEM.Text = item.PurchasePrice.ToString();
                txtSupplier_SEM.Text = item.Supplier; txtDesc_SEM.Text = item.Description;
                try { dtpDate_SEM.Value = DateTime.Parse(item.DeliveryDate); } catch { dtpDate_SEM.Value = DateTime.Now; }
            }
        }

        private void UpdateCharts()
        {
            chartBar_SEM.Series[0].Points.Clear();
            chartBar_SEM.Palette = ChartColorPalette.SeaGreen;
            chartBar_SEM.Titles.Clear(); chartBar_SEM.Titles.Add("Прибыльность (Топ-10)");
            chartBar_SEM.Series[0].Name = "Прибыль (руб.)";
            chartBar_SEM.Series[0].IsValueShownAsLabel = true; chartBar_SEM.Series[0].IsXValueIndexed = true;

            var axisX = chartBar_SEM.ChartAreas[0].AxisX;
            axisX.Interval = 1; axisX.LabelStyle.Interval = 1; axisX.LabelStyle.Angle = -45; axisX.LabelStyle.Font = new Font("Segoe UI", 9);

            var top = currentViewList_SEM.OrderByDescending(x => (x.Price - x.PurchasePrice) * x.Quantity).Take(10).ToList();
            foreach (var i in top) chartBar_SEM.Series[0].Points.AddXY(i.Name, (i.Price - i.PurchasePrice) * i.Quantity);

            chartPie_SEM.Series[0].Points.Clear();
            chartPie_SEM.Palette = ChartColorPalette.BrightPastel;
            chartPie_SEM.Titles.Clear(); chartPie_SEM.Titles.Add("Категории (в деньгах)");
            var cats = currentViewList_SEM.GroupBy(x => x.Category).Select(g => new { N = g.Key, V = g.Sum(x => x.Price * x.Quantity) });
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

        private void GridData_SEM_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (gridData_SEM.Columns[e.ColumnIndex].Name == "Quantity" && e.Value != null)
                if (int.TryParse(e.Value.ToString(), out int qty) && qty < 10) { gridData_SEM.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.MistyRose; }
        }

        private void SortGrid(int colIndex)
        {
            string colName = gridData_SEM.Columns[colIndex].DataPropertyName;
            currentViewList_SEM = isAscending
                ? currentViewList_SEM.OrderBy(x => x.GetType().GetProperty(colName).GetValue(x, null)).ToList()
                : currentViewList_SEM.OrderByDescending(x => x.GetType().GetProperty(colName).GetValue(x, null)).ToList();
            isAscending = !isAscending;
            gridData_SEM.DataSource = currentViewList_SEM;
        }

        private void OpenOrderWindow()
        {
            var lowStock = dataList_SEM.Where(x => x.Quantity < 10).ToList();

            if (lowStock.Count == 0)
            {
                MessageBox.Show("На складе достаточно товара. Дефицита нет.", "Все отлично");
                return;
            }

            Form orderForm = new Form
            {
                Text = "Мастер формирования заказа",
                Size = new Size(900, 550),
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false
            };

            DataGridView gridOrder = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 450,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White
            };

            gridOrder.Columns.Add("Code", "Артикул");
            gridOrder.Columns.Add("Name", "Наименование");
            gridOrder.Columns.Add("Supplier", "Поставщик");
            gridOrder.Columns.Add("CurQty", "Остаток");
            gridOrder.Columns.Add("Price", "Цена закупки");
            gridOrder.Columns.Add("ToOrder", "К ЗАКАЗУ (шт)");

            foreach (var item in lowStock)
            {
                gridOrder.Rows.Add(item.Code, item.Name, item.Supplier, item.Quantity, item.PurchasePrice, "50");
            }

            foreach (DataGridViewColumn col in gridOrder.Columns) col.ReadOnly = true;
            gridOrder.Columns[5].ReadOnly = false;
            gridOrder.Columns[5].DefaultCellStyle.BackColor = Color.LightYellow;
            gridOrder.Columns[5].DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            Button btnSaveReport = new Button
            {
                Text = "💾  Сформировать и сохранить отчет",
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat
            };

            btnSaveReport.Click += (s, e) => {
                string reportName = $"Order_{DateTime.Now:yyyy-MM-dd_HH-mm}.txt";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("                           ЗАЯВКА НА ПОСТАВКУ ТОВАРОВ");
                sb.AppendLine($"                           Дата формирования: {DateTime.Now}");
                sb.AppendLine(new string('=', 95));

                sb.AppendLine(String.Format("| {0,-10} | {1,-30} | {2,-20} | {3,8} | {4,12} |",
                    "АРТИКУЛ", "НАИМЕНОВАНИЕ", "ПОСТАВЩИК", "КОЛ-ВО", "СУММА"));
                sb.AppendLine(new string('-', 95));

                decimal totalBudget = 0;
                int positionCount = 0;

                foreach (DataGridViewRow row in gridOrder.Rows)
                {
                    string code = row.Cells[0].Value?.ToString() ?? "-";
                    string name = row.Cells[1].Value?.ToString() ?? "Товар";
                    if (name.Length > 28) name = name.Substring(0, 25) + "...";

                    string supplier = row.Cells[2].Value?.ToString() ?? "-";
                    if (supplier.Length > 18) supplier = supplier.Substring(0, 15) + "...";

                    decimal price = decimal.Parse(row.Cells[4].Value.ToString());

                    if (int.TryParse(row.Cells[5].Value.ToString(), out int qty) && qty > 0)
                    {
                        decimal rowSum = price * qty;
                        totalBudget += rowSum;
                        positionCount++;

                        sb.AppendLine(String.Format("| {0,-10} | {1,-30} | {2,-20} | {3,8} | {4,12:N2} |",
                            code, name, supplier, qty, rowSum));
                    }
                }

                sb.AppendLine(new string('=', 95));
                sb.AppendLine($"ИТОГО ПОЗИЦИЙ К ЗАКАЗУ: {positionCount}");
                sb.AppendLine($"ОБЩИЙ БЮДЖЕТ ЗАКУПКИ:   {totalBudget:N2} руб.");
                sb.AppendLine("\nПодпись менеджера: __________________");

                try
                {
                    File.WriteAllText(reportName, sb.ToString());
                    ds_SEM.WriteLog($"Сформирован отчет на закупку (Сумма: {totalBudget})");

                    Process.Start(new ProcessStartInfo(reportName) { UseShellExecute = true });
                    orderForm.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка сохранения отчета:\n" + ex.Message);
                }
            };

            orderForm.Controls.Add(btnSaveReport);
            orderForm.Controls.Add(gridOrder);
            orderForm.ShowDialog();
        }

        private void ShowAbout()
        {
            string info = "📦 АИС «Управление оптовой базой»\n" +
                          "Версия: 4.0 (Final Release)\n\n" +

                          "👤 РАЗРАБОТЧИК:\n" +
                          "Студент: Сиволап Е.М.\n" +
                          "Учебное задание: Спринт 7 | Вариант 5\n\n" +

                          "🏢 ПРЕДМЕТНАЯ ОБЛАСТЬ:\n" +
                          "Автоматизация складского учета и оптовой торговли. " +
                          "Система предназначена для управления товарными запасами, мониторинга цен, " +
                          "контроля остатков и анализа финансовой эффективности предприятия.\n\n" +

                          "⚙️ ФУНКЦИОНАЛЬНЫЕ ВОЗМОЖНОСТИ:\n" +
                          "• Полный цикл учета: Добавление, редактирование и списание товаров.\n" +
                          "• Работа с данными: Импорт/Экспорт базы данных в формате CSV.\n" +
                          "• Умный поиск и фильтрация: Выборка по категориям, поставщикам и диапазонам цен.\n" +
                          "• Бизнес-аналитика: Дашборд с графиками, расчет стоимости склада и потенциальной прибыли.\n" +
                          "• Автоматизация закупок: Генерация отчетов по дефицитным позициям.\n" +
                          "• Безопасность: Валидация вводимых данных и логирование всех операций.\n\n" +

                          "© ТИУ, 2025";

            MessageBox.Show(info, "О программе | Sklad System", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private Button CreateMenuBtn(string t, int top, EventHandler act)
        {
            var b = new Button { Text = "  " + t, Top = top, Left = 0, Width = 220, Height = 50, FlatStyle = FlatStyle.Flat, ForeColor = Color.Silver, BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(15, 0, 0, 0), Font = new Font("Segoe UI", 10), Cursor = Cursors.Hand };
            b.MouseEnter += (s, e) => { b.BackColor = Color.FromArgb(50, 55, 60); b.ForeColor = Color.White; }; b.MouseLeave += (s, e) => { b.BackColor = Color.Transparent; b.ForeColor = Color.Silver; };
            b.Click += act; return b;
        }
        private Button CreateBtn(Panel p, string t, Color c, int top, EventHandler act)
        {
            var b = new Button { Text = t, BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(10, top), Size = new Size(250, 45), Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            b.MouseEnter += (s, e) => b.BackColor = ControlPaint.Dark(c); b.MouseLeave += (s, e) => b.BackColor = c;
            b.Click += act; p.Controls.Add(b); return b;
        }
        private TextBox CreateInput(Panel p, string l, int t)
        {
            p.Controls.Add(new Label { Text = l, Location = new Point(10, t), AutoSize = true, ForeColor = Color.FromArgb(64, 64, 64) });
            var tb = new TextBox { Location = new Point(10, t + 22), Width = 250 }; p.Controls.Add(tb); return tb;
        }
        private NumericUpDown CreateFilterNum(Panel p, int x, int width)
        {
            var num = new NumericUpDown { Location = new Point(x, 80), Width = width, Maximum = 1000000 }; num.ValueChanged += (s, e) => ApplyFilters(); p.Controls.Add(num); return num;
        }
        private Label CreateStatLabel(GroupBox g, string txt, int x, int y)
        {
            var l = new Label { Text = txt, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) }; g.Controls.Add(l); return l;
        }
    }
}