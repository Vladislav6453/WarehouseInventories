using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Library.DTO;
using WarehouseInventory.Windows;

namespace WarehouseInventory.ViewModels
{
    public class MovementViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private ObservableCollection<MovementDTO> _allMovements = new();
        private ObservableCollection<MovementDTO> _movements = new();
        private ObservableCollection<MovementTypeDTO> _movementTypes = new();
        private string _searchQuery = "";
        private MovementTypeDTO? _selectedMovementType;
        private bool _isMenuOpen;
        private Window? _currentWindow;

        public ObservableCollection<MovementDTO> Movements
        {
            get => _movements;
            set { _movements = value; OnPropertyChanged(); }
        }

        public ObservableCollection<MovementTypeDTO> MovementTypes
        {
            get => _movementTypes;
            set { _movementTypes = value; OnPropertyChanged(); }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                ApplyFilters(); // Применяем оба фильтра
            }
        }

        public MovementTypeDTO? SelectedMovementType
        {
            get => _selectedMovementType;
            set
            {
                _selectedMovementType = value;
                OnPropertyChanged();
                ApplyFilters(); // Применяем оба фильтра
            }
        }

        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            set { _isMenuOpen = value; OnPropertyChanged(); }
        }

        public ICommand ClearSearchCommand { get; }
        public ICommand ToggleMenuCommand { get; }
        public ICommand NavigateProductsCommand { get; }
        public ICommand NavigateChartsCommand { get; }
        public ICommand NavigateSuppliersCommand { get; }
        public ICommand NavigateCustomersCommand { get; }
        public ICommand NavigateInvoicesCommand { get; }
        public ICommand NavigateEmployeesCommand { get; }
        public ICommand LogoutCommand { get; }

        public MovementViewModel()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");

            ToggleMenuCommand = new RelayCommand(_ => IsMenuOpen = !IsMenuOpen);
            ClearSearchCommand = new RelayCommand(_ => SearchQuery = "");
            LogoutCommand = new RelayCommand(_ => NavigateToLogout());
            NavigateProductsCommand = new RelayCommand(_ => NavigateToProduct());
            NavigateSuppliersCommand = new RelayCommand(_ => NavigateToSupplier());
            NavigateInvoicesCommand = new RelayCommand(_ => NavigateToInvoice());
            NavigateChartsCommand = new RelayCommand(_ => NavigateToChart());
            NavigateEmployeesCommand = new RelayCommand(_ => NavigateToEmployee());
            NavigateCustomersCommand = new RelayCommand(_ => NavigateToCustomer());

            _ = LoadMovementTypesAsync();
            _ = LoadMovementsAsync();
        }
        public void SetCurrentWindow(Window window)
        {
            _currentWindow = window;
        }

        private async System.Threading.Tasks.Task LoadMovementTypesAsync()
        {
            try
            {
                var types = await _httpClient.GetFromJsonAsync<MovementTypeDTO[]>("Movements/GetMovementTypes");
                if (types != null)
                {
                    MovementTypes.Clear();
                    MovementTypes.Add(new MovementTypeDTO { Id = 0, Type = "Все типы" });
                    foreach (var type in types)
                        MovementTypes.Add(type);

                    SelectedMovementType = MovementTypes.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка загрузки типов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadMovementsAsync()
        {
            try
            {
                _allMovements.Clear();
                var movements = await _httpClient.GetFromJsonAsync<MovementDTO[]>("Movements");
                if (movements != null)
                {
                    foreach (var movement in movements)
                        _allMovements.Add(movement);
                }
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка загрузки движений: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            var result = _allMovements.AsEnumerable();

            // Фильтр по типу (Приход/Расход)
            if (SelectedMovementType != null && SelectedMovementType.Id > 0)
            {
                result = result.Where(m => m.MovementTypeId == SelectedMovementType.Id);
            }

            // Поиск по тексту
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                result = result.Where(m =>
                    m.ProductName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    m.InvoiceNumber.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    m.EmployeeName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
            }

            Movements.Clear();
            foreach (var movement in result)
                Movements.Add(movement);
        }
        
        private void NavigateToProduct()
        {
            IsMenuOpen = false;
            var productsWindow = new MainWindow();
            productsWindow.Owner = _currentWindow;
            productsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            productsWindow.Show();
            _currentWindow?.Hide();
        }
        
        private void NavigateToSupplier()
        {
            IsMenuOpen = false;
            var suppliersWindow = new SuppliersWindow();
            suppliersWindow.Owner = _currentWindow;
            suppliersWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            suppliersWindow.Show();
            _currentWindow?.Hide();
        }
        
        private void NavigateToInvoice()
        {
            IsMenuOpen = false;
            var invoicesWindow = new InvoicesWindow();
            invoicesWindow.Owner = _currentWindow;
            invoicesWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            invoicesWindow.Show();
            _currentWindow?.Hide();
        }
        
        private void NavigateToChart()
        {
            IsMenuOpen = false;
            var chartsWindow = new ChartsWindow();
            chartsWindow.Owner = _currentWindow;
            chartsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            chartsWindow.Show();
            _currentWindow?.Hide();
        }
        
        private void NavigateToEmployee()
        {
            IsMenuOpen = false;
            var employeesWindow = new EmployeesWindow();
            employeesWindow.Owner = _currentWindow;
            employeesWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            employeesWindow.Show();
            _currentWindow?.Hide();
        }
        
        private void NavigateToLogout()
        {
            IsMenuOpen = false;
            var logoutWindow = new LoginWindow();
            logoutWindow.Owner = _currentWindow;
            logoutWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            logoutWindow.Show();
            _currentWindow?.Hide();
        }
        
        private void NavigateToCustomer()
        {
            IsMenuOpen = false;
            var customerWindow = new CustomersWindow();
            customerWindow.Owner = _currentWindow;
            customerWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            customerWindow.Show();
            _currentWindow?.Hide();
        }
    }
}
