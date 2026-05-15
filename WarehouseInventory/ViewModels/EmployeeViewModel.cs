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
    public class EmployeeViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private ObservableCollection<EmployeeDTO> _employees = new();
        private string _searchQuery = "";
        private EmployeeDTO? _selectedEmployee;
        private bool _isMenuOpen;
        private Window? _currentWindow;
        
        private EmployeeDTO? CurrentUser => Session.CurrentUser;
        private bool IsAdmin => CurrentUser?.RoleName == "Админ";
        private bool IsManager => CurrentUser?.RoleName == "Менеджер";
        private bool IsEmployee => CurrentUser?.RoleName == "Сотрудник";
        private bool CanEditOrDelete => IsAdmin;

        public ObservableCollection<EmployeeDTO> Employees
        {
            get => _employees;
            set { _employees = value; OnPropertyChanged(); }
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

        public EmployeeDTO? SelectedEmployee
        {
            get => _selectedEmployee;
            set { _selectedEmployee = value; OnPropertyChanged(); }
        }

        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            set { _isMenuOpen = value; OnPropertyChanged(); }
        }

        public ICommand EditEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand NavigateProductsCommand { get; }
        public ICommand NavigateChartsCommand { get; }
        public ICommand NavigateSuppliersCommand { get; }
        public ICommand NavigateCustomersCommand { get; }
        public ICommand ToggleMenuCommand { get; }
        public ICommand NavigateInvoicesCommand { get; }
        public ICommand NavigateMovementsCommand { get; }
        public ICommand LogoutCommand { get; }

        public EmployeeViewModel()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");

            EditEmployeeCommand = new RelayCommand(_ => OpenEmployeeEditWindow(SelectedEmployee), _ => CanEditOrDelete && SelectedEmployee != null);
            DeleteEmployeeCommand = new RelayCommand(_ => _ = DeleteEmployeeAsync(), _ => CanEditOrDelete && SelectedEmployee != null);
            
            ToggleMenuCommand = new RelayCommand(_ => IsMenuOpen = !IsMenuOpen);
            ClearSearchCommand = new RelayCommand(_ => SearchQuery = "");
            LogoutCommand = new RelayCommand(_ => NavigateToLogout());
            NavigateProductsCommand = new RelayCommand(_ => NavigateToProduct());
            NavigateSuppliersCommand = new RelayCommand(_ => NavigateToSupplier());
            NavigateInvoicesCommand = new RelayCommand(_ => NavigateToInvoice());
            NavigateChartsCommand = new RelayCommand(_ => NavigateToChart());
            NavigateMovementsCommand = new RelayCommand(_ => NavigateToMovement());
            NavigateCustomersCommand = new RelayCommand(_ => NavigateToCustomer());

            _ = LoadEmployeesAsync();
            _ = LoadRolesAsync();
        }
        
        public void SetCurrentWindow(Window window)
        {
            _currentWindow = window;
        }

        private async System.Threading.Tasks.Task LoadEmployeesAsync()
        {
            try
            {
                var employees = await _httpClient.GetFromJsonAsync<EmployeeDTO[]>("Employees");
                if (employees != null)
                {
                    Employees.Clear();
                    foreach (var employee in employees)
                        Employees.Add(employee);
                }
            }
            catch (Exception ex)
            {
                /*MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);*/
            }
        }

        private async System.Threading.Tasks.Task LoadRolesAsync()
        {
            try
            {
                var roles = await _httpClient.GetFromJsonAsync<EmployeeRoleDTO[]>("Employees/GetRoles");
                if (roles != null && Application.Current.Properties["EmployeeRoles"] == null)
                {
                    Application.Current.Properties["EmployeeRoles"] = roles;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task SearchAsync()
        {
            try
            {
                var all = await _httpClient.GetFromJsonAsync<EmployeeDTO[]>("Employees");
                if (all != null)
                {
                    var filtered = string.IsNullOrWhiteSpace(SearchQuery)
                        ? all
                        : all.Where(e => e.LastName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                        e.FirstName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                        e.Login.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                             .ToArray();

                    Employees.Clear();
                    foreach (var employee in filtered)
                        Employees.Add(employee);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenEmployeeEditWindow(EmployeeDTO? employee)
        {
            if (!IsAdmin)
            {
                MessageBox.Show("У вас нет прав на редактирование сотрудников.\n\nТолько администратор может изменять данные сотрудников.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (employee == null) return;

            var window = new EmployeesEditWindow(employee);
            window.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            window.ShowDialog();
            _ = LoadEmployeesAsync();
        }

        private async System.Threading.Tasks.Task DeleteEmployeeAsync()
        {
            if (!IsAdmin)
            {
                MessageBox.Show("У вас нет прав на удаление сотрудников.\n\nТолько администратор может удалять сотрудников.",
                    "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (SelectedEmployee == null) return;
            
            if (CurrentUser != null && SelectedEmployee.Id == CurrentUser.Id)
            {
                MessageBox.Show("Вы не можете удалить самого себя.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить сотрудника \"{SelectedEmployee.FullName}\"?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var response = await _httpClient.DeleteAsync($"Employees/{SelectedEmployee.Id}");

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("✅ Сотрудник удалён", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadEmployeesAsync();
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
        
        private void NavigateToMovement()
        {
            IsMenuOpen = false;
            var movementsWindow = new MovementsWindow();
            movementsWindow.Owner = _currentWindow;
            movementsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            movementsWindow.Show();
            _currentWindow?.Hide();
        }
        private void NavigateToLogout()
        {
            /*IsMenuOpen = false;
            var logoutWindow = new LoginWindow();
            logoutWindow.Owner = _currentWindow;
            logoutWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            logoutWindow.Show();
            _currentWindow?.Hide();*/
            
            IsMenuOpen = false;
            Session.CurrentUser = null;  // ← ОЧИЩАЕМ СЕССИЮ!
    
            var loginWindow = new LoginWindow();
            loginWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            loginWindow.Show();
            _currentWindow?.Close();  // ← Close вместо Hide
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
