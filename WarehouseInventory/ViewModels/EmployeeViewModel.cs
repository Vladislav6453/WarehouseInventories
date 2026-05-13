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
        public ICommand LogoutCommand { get; }

        public EmployeeViewModel()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");

            EditEmployeeCommand = new RelayCommand(_ => OpenEmployeeEditWindow(SelectedEmployee), _ => SelectedEmployee != null);
            DeleteEmployeeCommand = new RelayCommand(_ => _ = DeleteEmployeeAsync(), _ => SelectedEmployee != null);
            ClearSearchCommand = new RelayCommand(_ => SearchQuery = "");
            LogoutCommand = new RelayCommand(_ => Application.Current.Shutdown());

            _ = LoadEmployeesAsync();
            _ = LoadRolesAsync();
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
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (employee == null) return;

            var window = new EmployeesEditWindow(employee);
            window.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            window.ShowDialog();
            _ = LoadEmployeesAsync();
        }

        private async System.Threading.Tasks.Task DeleteEmployeeAsync()
        {
            if (SelectedEmployee == null) return;

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
    }
}
