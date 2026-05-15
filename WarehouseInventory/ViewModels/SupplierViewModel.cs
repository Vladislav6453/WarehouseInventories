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
    public class SupplierViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private ObservableCollection<SupplierDTO> _suppliers = new();
        private string _searchQuery = "";
        private SupplierDTO? _selectedSupplier;
        private bool _isMenuOpen;
        private Window? _currentWindow;

        public ObservableCollection<SupplierDTO> Suppliers
        {
            get => _suppliers;
            set { _suppliers = value; OnPropertyChanged(); }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                _ = SearchAsync();
            }
        }

        public SupplierDTO? SelectedSupplier
        {
            get => _selectedSupplier;
            set { _selectedSupplier = value; OnPropertyChanged(); }
        }

        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            set { _isMenuOpen = value; OnPropertyChanged(); }
        }

        public ICommand ToggleMenuCommand { get; }
        public ICommand AddSupplierCommand { get; }
        public ICommand EditSupplierCommand { get; }
        public ICommand DeleteSupplierCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand NavigateToProductCommand { get; }
        public ICommand NavigateToChartCommand { get; }
        public ICommand NavigateToInvoiceCommand { get; }
        public ICommand NavigateToCustomerCommand { get; }
        public ICommand NavigateToMovementCommand { get; }
        public ICommand NavigateToEmployeeCommand { get; }
        public ICommand LogoutCommand { get; }

        public SupplierViewModel()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");

            ToggleMenuCommand = new RelayCommand(_ => IsMenuOpen = !IsMenuOpen);
            AddSupplierCommand = new RelayCommand(_ => OpenSupplierEditWindow(null));
            EditSupplierCommand = new RelayCommand(_ => OpenSupplierEditWindow(SelectedSupplier), _ => SelectedSupplier != null);
            DeleteSupplierCommand = new RelayCommand(_ => _ = DeleteSupplierAsync(), _ => SelectedSupplier != null);
            ClearSearchCommand = new RelayCommand(_ => SearchQuery = "");
            LogoutCommand = new RelayCommand(_ => NavigateToLogout());
            NavigateToProductCommand = new RelayCommand(_ => NavigateToProduct());
            NavigateToChartCommand = new RelayCommand(_ => NavigateToChart());
            NavigateToInvoiceCommand = new RelayCommand(_ => NavigateToInvoice());
            NavigateToCustomerCommand = new RelayCommand(_ => NavigateToCustomer());
            NavigateToMovementCommand = new RelayCommand(_ => NavigateToMovement());
            NavigateToEmployeeCommand = new RelayCommand(_ => NavigateToEmployee());

            _ = LoadSuppliersAsync();
        }
        
        public void SetCurrentWindow(Window window)
        {
            _currentWindow = window;
        }

        private async System.Threading.Tasks.Task LoadSuppliersAsync()
        {
            try
            {
                var suppliers = await _httpClient.GetFromJsonAsync<SupplierDTO[]>("Suppliers");
                if (suppliers != null)
                {
                    Suppliers.Clear();
                    foreach (var supplier in suppliers)
                        Suppliers.Add(supplier);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task SearchAsync()
        {
            try
            {
                var all = await _httpClient.GetFromJsonAsync<SupplierDTO[]>("Suppliers");
                if (all != null)
                {
                    var filtered = string.IsNullOrWhiteSpace(SearchQuery)
                        ? all
                        : all.Where(s => s.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                        s.Phone.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                        s.Email.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                             .ToArray();

                    Suppliers.Clear();
                    foreach (var supplier in filtered)
                        Suppliers.Add(supplier);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenSupplierEditWindow(SupplierDTO? supplier)
        {
            var window = new SuppliersEditWindow(supplier);
            window.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            window.ShowDialog();
            _ = LoadSuppliersAsync();
        }

        private async System.Threading.Tasks.Task DeleteSupplierAsync()
        {
            if (SelectedSupplier == null) return;

            var result = MessageBox.Show($"Удалить поставщика \"{SelectedSupplier.Name}\"?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var response = await _httpClient.DeleteAsync($"Suppliers/{SelectedSupplier.Id}");

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("✅ Поставщик удалён", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadSuppliersAsync();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка: {error}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
        
        private void NavigateToChart()
        {
            IsMenuOpen = false;
            var chartsWindow = new ChartsWindow();
            chartsWindow.Owner = _currentWindow;
            chartsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            chartsWindow.Show();
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
        
        private void NavigateToCustomer()
        {
            IsMenuOpen = false;
            var customersWindow = new CustomersWindow();
            customersWindow.Owner = _currentWindow;
            customersWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            customersWindow.Show();
            _currentWindow?.Hide();
        }
        
        private void NavigateToMovement()
        {
            IsMenuOpen = false;
            var movementsWindow = new MovementsWindow();
            movementsWindow.Owner = _currentWindow;
            movementsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            movementsWindow.Show();
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
    }
}
