

using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Input;
using Library.DTO;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace WarehouseInventory.ViewModels
{
    public class ChartsViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private DateTime _startDate = DateTime.Now.AddDays(-30);
        private DateTime _endDate = DateTime.Now;
        
        private int _totalMovements;
        private decimal _totalIncome;
        private decimal _totalExpense;
        private int _currentStock;
        
        private string _selectedPeriod = "Последние 30 дней";
        private string _topProductsMetric = "По количеству";
        private ObservableCollection<ISeries> _movementSeries = new();
        private ObservableCollection<ISeries> _movementTypeSeries = new();
        private ObservableCollection<ISeries> _topProductsSeries = new();
        private ObservableCollection<ISeries> _stockSeries = new();
        private ObservableCollection<ISeries> _topCustomersSeries = new();
        private ObservableCollection<ISeries> _topSuppliersSeries = new();
        private bool _isMenuOpen;
        private Window? _currentWindow;
        
        public ObservableCollection<ISeries> MovementSeries
        {
            get => _movementSeries;
            set { _movementSeries = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ISeries> MovementTypeSeries
        {
            get => _movementTypeSeries;
            set { _movementTypeSeries = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ISeries> TopProductsSeries
        {
            get => _topProductsSeries;
            set { _topProductsSeries = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ISeries> StockSeries
        {
            get => _stockSeries;
            set { _stockSeries = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ISeries> TopCustomersSeries
        {
            get => _topCustomersSeries;
            set { _topCustomersSeries = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ISeries> TopSuppliersSeries
        {
            get => _topSuppliersSeries;
            set { _topSuppliersSeries = value; OnPropertyChanged(); }
        }

        public int TotalMovements
        {
            get => _totalMovements;
            set { _totalMovements = value; OnPropertyChanged(); }
        }

        public decimal TotalIncome
        {
            get => _totalIncome;
            set { _totalIncome = value; OnPropertyChanged(); }
        }

        public decimal TotalExpense
        {
            get => _totalExpense;
            set { _totalExpense = value; OnPropertyChanged(); }
        }

        public int CurrentStock
        {
            get => _currentStock;
            set { _currentStock = value; OnPropertyChanged(); }
        }

        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                _selectedPeriod = value;
                OnPropertyChanged();
                UpdatePeriodBySelection();
            }
        }

        public string TopProductsMetric
        {
            get => _topProductsMetric;
            set
            {
                _topProductsMetric = value;
                OnPropertyChanged();
                _ = LoadTopProductsAsync();
            }
        }

        
        
        public ObservableCollection<Axis> DateAxes { get; } = new()
        {
            new Axis
            {
                Name = "Дата",
                NamePaint = new SolidColorPaint(SKColor.Parse("#4B5563")),
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#6B7280")),
                LabelsRotation = 45,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB"))
            }
        };

        public ObservableCollection<Axis> ProductAxes { get; } = new()
        {
            new Axis
            {
                Name = "Товары",
                NamePaint = new SolidColorPaint(SKColor.Parse("#4B5563")),
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#6B7280")),
                LabelsRotation = 45,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB"))
            }
        };

        public ObservableCollection<Axis> CustomerAxes { get; } = new()
        {
            new Axis
            {
                Name = "Клиенты",
                NamePaint = new SolidColorPaint(SKColor.Parse("#4B5563")),
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#6B7280")),
                LabelsRotation = 45,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB"))
            }
        };

        public ObservableCollection<Axis> SupplierAxes { get; } = new()
        {
            new Axis
            {
                Name = "Поставщики",
                NamePaint = new SolidColorPaint(SKColor.Parse("#4B5563")),
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#6B7280")),
                LabelsRotation = 45,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB"))
            }
        };

        public ObservableCollection<Axis> ValueAxes { get; } = new()
        {
            new Axis
            {
                Name = "Количество",
                NamePaint = new SolidColorPaint(SKColor.Parse("#4B5563")),
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#6B7280")),
                Labeler = (value) => value.ToString("N0"),
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#E5E7EB"))
            }
        };

        public ObservableCollection<string> Periods { get; } = new()
        {
            "Последние 7 дней",
            "Последние 30 дней",
            "Последние 90 дней",
            "Весь период"
        };

        public ObservableCollection<string> TopProductsMetrics { get; } = new()
        {
            "По количеству",
            "По сумме"
        };
        
        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            set { _isMenuOpen = value; OnPropertyChanged(); }
        }
        public ICommand ToggleMenuCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand NavigateProductCommand { get; }
        public ICommand SetWeekCommand { get; }
        public ICommand SetMonthCommand { get; }
        public ICommand SetQuarterCommand { get; }
        public ICommand SetYearCommand { get; }

        public ChartsViewModel()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");

            ToggleMenuCommand = new RelayCommand(_ => IsMenuOpen = !IsMenuOpen);
            RefreshCommand = new RelayCommand(_ => _ = LoadAllDataAsync());
            SetWeekCommand = new RelayCommand(_ => SetPeriod(7, "Последние 7 дней"));
            SetMonthCommand = new RelayCommand(_ => SetPeriod(30, "Последние 30 дней"));
            SetQuarterCommand = new RelayCommand(_ => SetPeriod(90, "Последние 90 дней"));
            SetYearCommand = new RelayCommand(_ => SetPeriod(365, "Последние 365 дней"));

            NavigateProductCommand = new RelayCommand(_ => NavigateProduct());
            _ = LoadAllDataAsync();
        }
        
        public void SetCurrentWindow(Window window)
        {
            _currentWindow = window;
        }
        
        private void NavigateProduct()
        {
            IsMenuOpen = false;
            var productsWindow = new MainWindow();
            productsWindow.Owner = _currentWindow;
            productsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            productsWindow.Show();
            _currentWindow?.Hide();
        }

        private void SetPeriod(int days, string periodName)
        {
            _startDate = DateTime.Now.AddDays(-days);
            _endDate = DateTime.Now;
            _selectedPeriod = periodName;
            OnPropertyChanged(nameof(SelectedPeriod));
            _ = LoadAllDataAsync();
        }

        private void UpdatePeriodBySelection()
        {
            switch (SelectedPeriod)
            {
                case "Последние 7 дней":
                    _startDate = DateTime.Now.AddDays(-7);
                    break;
                case "Последние 30 дней":
                    _startDate = DateTime.Now.AddDays(-30);
                    break;
                case "Последние 90 дней":
                    _startDate = DateTime.Now.AddDays(-90);
                    break;
                case "Весь период":
                    _startDate = DateTime.MinValue;
                    break;
                default:
                    _startDate = DateTime.Now.AddDays(-30);
                    break;
            }
            _endDate = DateTime.Now;
            _ = LoadAllDataAsync();
        }

        private async System.Threading.Tasks.Task LoadAllDataAsync()
        {
            await LoadStatisticsAsync();
            await LoadMovementSeriesAsync();
            await LoadMovementTypeSeriesAsync();
            await LoadTopProductsAsync();
            await LoadStockSeriesAsync();
            await LoadTopCustomersAsync();
            await LoadTopSuppliersAsync();
        }

        private async System.Threading.Tasks.Task LoadStatisticsAsync()
        {
            try
            {
                var url = $"Charts/Statistics?startDate={_startDate:yyyy-MM-dd}&endDate={_endDate:yyyy-MM-dd}";
                var stats = await _httpClient.GetFromJsonAsync<StatisticsDTO>(url);
                
                if (stats != null)
                {
                    TotalMovements = stats.TotalMovements;
                    TotalIncome = stats.TotalIncome;
                    TotalExpense = stats.TotalExpense;
                    CurrentStock = stats.CurrentStock;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка загрузки статистики: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadMovementSeriesAsync()
        {
            try
            {
                var url = $"Charts/Movements?startDate={_startDate:yyyy-MM-dd}&endDate={_endDate:yyyy-MM-dd}";
                var movements = await _httpClient.GetFromJsonAsync<MovementItemDTO[]>(url);
                
                if (movements != null && movements.Any())
                {
                    var byDate = movements
                        .GroupBy(m => m.Date.Date)
                        .Select(g => new 
                        { 
                            Date = g.Key, 
                            Income = g.Where(x => x.MovementType == "Приход").Sum(x => x.Quantity), 
                            Expense = g.Where(x => x.MovementType == "Расход").Sum(x => x.Quantity) 
                        })
                        .OrderBy(x => x.Date)
                        .ToList();

                    MovementSeries.Clear();
                    MovementSeries.Add(new ColumnSeries<int>
                    {
                        Name = "Приход",
                        Values = byDate.Select(x => x.Income).ToArray(),
                        Fill = new SolidColorPaint(SKColor.Parse("#10B981")),
                    });
                    MovementSeries.Add(new ColumnSeries<int>
                    {
                        Name = "Расход",
                        Values = byDate.Select(x => x.Expense).ToArray(),
                        Fill = new SolidColorPaint(SKColor.Parse("#EF4444")),
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка загрузки движений: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadMovementTypeSeriesAsync()
        {
            try
            {
                var url = $"Charts/Movements?startDate={_startDate:yyyy-MM-dd}&endDate={_endDate:yyyy-MM-dd}";
                var movements = await _httpClient.GetFromJsonAsync<MovementItemDTO[]>(url);
                
                if (movements != null)
                {
                    var incomeTotal = movements.Where(m => m.MovementType == "Приход").Sum(m => m.Quantity);
                    var expenseTotal = movements.Where(m => m.MovementType == "Расход").Sum(m => m.Quantity);

                    MovementTypeSeries.Clear();
                    MovementTypeSeries.Add(new PieSeries<int>
                    {
                        Name = "Приход",
                        Values = new[] { incomeTotal },
                        Fill = new SolidColorPaint(SKColor.Parse("#10B981")),
                    });
                    MovementTypeSeries.Add(new PieSeries<int>
                    {
                        Name = "Расход",
                        Values = new[] { expenseTotal },
                        Fill = new SolidColorPaint(SKColor.Parse("#EF4444")),
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка загрузки типов движений: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadTopProductsAsync()
        {
            try
            {
                var url = TopProductsMetric == "По количеству" 
                    ? "Charts/TopProducts?count=10" 
                    : "Charts/TopProductsByAmount?count=10";
                
                var topProducts = await _httpClient.GetFromJsonAsync<TopProductDTO[]>(url);
                
                if (topProducts != null && topProducts.Any())
                {
                    TopProductsSeries.Clear();
                    TopProductsSeries.Add(new ColumnSeries<int>
                    {
                        Name = TopProductsMetric == "По количеству" ? "Количество продаж (шт.)" : "Сумма продаж (₽)",
                        Values = topProducts.Select(x => x.TotalQuantity).ToArray(),
                        Fill = new SolidColorPaint(SKColor.Parse("#3B82F6")),
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка загрузки топ товаров: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadStockSeriesAsync()
        {
            try
            {
                var stocks = await _httpClient.GetFromJsonAsync<ProductStockDTO[]>("Charts/ProductsStock");
                
                if (stocks != null)
                {
                    var topStocks = stocks.OrderByDescending(x => x.Quantity).Take(15).ToList();
                    
                    StockSeries.Clear();
                    StockSeries.Add(new ColumnSeries<int>
                    {
                        Name = "Остаток на складе",
                        Values = topStocks.Select(x => x.Quantity).ToArray(),
                        Fill = new SolidColorPaint(SKColor.Parse("#8B5CF6")),
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка загрузки остатков: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadTopCustomersAsync()
        {
            try
            {
                var topCustomers = await _httpClient.GetFromJsonAsync<TopCustomerDTO[]>("Charts/TopCustomers?count=10");
                
                if (topCustomers != null && topCustomers.Any())
                {
                    TopCustomersSeries.Clear();
                    TopCustomersSeries.Add(new ColumnSeries<decimal>
                    {
                        Name = "Сумма покупок (₽)",
                        Values = topCustomers.Select(x => x.TotalPurchases).ToArray(),
                        Fill = new SolidColorPaint(SKColor.Parse("#F59E0B")),
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка загрузки топ клиентов: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadTopSuppliersAsync()
        {
            try
            {
                var topSuppliers = await _httpClient.GetFromJsonAsync<TopSupplierDTO[]>("Charts/TopSuppliers?count=10");
                
                if (topSuppliers != null && topSuppliers.Any())
                {
                    TopSuppliersSeries.Clear();
                    TopSuppliersSeries.Add(new ColumnSeries<decimal>
                    {
                        Name = "Сумма поставок (₽)",
                        Values = topSuppliers.Select(x => x.TotalSupply).ToArray(),
                        Fill = new SolidColorPaint(SKColor.Parse("#06B6D4")),
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка загрузки топ поставщиков: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}