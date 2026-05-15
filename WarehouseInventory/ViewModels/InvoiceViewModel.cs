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
    public class InvoiceViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private ObservableCollection<InvoiceDto> _invoices = new();
        private string _searchQuery = "";
        private bool _isMenuOpen;
        private Window? _currentWindow;

        public ObservableCollection<InvoiceDto> Invoices
        {
            get => _invoices;
            set { _invoices = value; OnPropertyChanged(); }
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

        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            set { _isMenuOpen = value; OnPropertyChanged(); }
        }

        public ICommand ToggleMenuCommand { get; }
        public ICommand OpenCreateInvoiceCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand NavigateProductsCommand { get; }
        public ICommand NavigateMovementsCommand { get; }
        public ICommand NavigateChartsCommand { get; }
        public ICommand NavigateSuppliersCommand { get; }
        public ICommand NavigateCustomersCommand { get; }
        public ICommand NavigateInvoicesCommand { get; }
        public ICommand NavigateEmployeesCommand { get; }

        public InvoiceViewModel()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");

            ToggleMenuCommand = new RelayCommand(_ => IsMenuOpen = !IsMenuOpen);
            OpenCreateInvoiceCommand = new RelayCommand(_ => OpenCreateInvoice());
            ClearSearchCommand = new RelayCommand(_ => SearchQuery = "");
            LogoutCommand = new RelayCommand(_ => NavigateToLogout());
            NavigateProductsCommand = new RelayCommand(_ => NavigateToProduct());
            NavigateMovementsCommand = new RelayCommand(_ => NavigateToMovement());
            NavigateSuppliersCommand = new RelayCommand(_ => NavigateToSupplier());
            NavigateInvoicesCommand = new RelayCommand(_ => NavigateToInvoice());
            NavigateChartsCommand = new RelayCommand(_ => NavigateToChart());
            NavigateEmployeesCommand = new RelayCommand(_ => NavigateToEmployee());
            NavigateCustomersCommand = new RelayCommand(_ => NavigateToCustomer());
            _ = LoadInvoicesAsync();
        }
        
        public void SetCurrentWindow(Window window)
        {
            _currentWindow = window;
        }

        private async System.Threading.Tasks.Task LoadInvoicesAsync()
        {
            try
            {
                var invoices = await _httpClient.GetFromJsonAsync<InvoiceDto[]>("Invoices");
                if (invoices != null)
                {
                    Invoices.Clear();
                    foreach (var invoice in invoices)
                        Invoices.Add(invoice);
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
                var all = await _httpClient.GetFromJsonAsync<InvoiceDto[]>("Invoices");
                if (all != null)
                {
                    var filtered = string.IsNullOrWhiteSpace(SearchQuery)
                        ? all
                        : all.Where(i => i.Number.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                        i.Counterparty.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                             .ToArray();

                    Invoices.Clear();
                    foreach (var invoice in filtered)
                        Invoices.Add(invoice);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenCreateInvoice()
        {
            var window = new InvoicesEditWindow();
            window.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            window.ShowDialog();
            _ = LoadInvoicesAsync();
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
        
        private void NavigateToMovement()
        {
            IsMenuOpen = false;
            var movementsWindow = new MovementsWindow();
            movementsWindow.Owner = _currentWindow;
            movementsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            movementsWindow.Show();
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
