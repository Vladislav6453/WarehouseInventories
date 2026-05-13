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

namespace WarehouseInventory.ViewModels
{
    public class EmployeeEditViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private Action? _close;
        private readonly int _employeeId;
        private readonly string _originalLogin;

        private string _lastName = "";
        private string _firstName = "";
        private string _login = "";
        private string _password = "";
        private EmployeeRoleDTO? _selectedRole;
        private ObservableCollection<EmployeeRoleDTO> _roles = new();

        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); }
        }

        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(); }
        }

        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public EmployeeRoleDTO? SelectedRole
        {
            get => _selectedRole;
            set { _selectedRole = value; OnPropertyChanged(); }
        }

        public ObservableCollection<EmployeeRoleDTO> Roles
        {
            get => _roles;
            set { _roles = value; OnPropertyChanged(); }
        }

        public string WindowTitle => "✏️ Редактирование сотрудника";
        public string PasswordHint => "🔒 Оставьте пустым, если не хотите менять пароль";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public EmployeeEditViewModel(EmployeeDTO employee)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");

            SaveCommand = new RelayCommand(_ => _ = SaveAsync());
            CancelCommand = new RelayCommand(_ => _close?.Invoke());

            _employeeId = employee.Id;
            _originalLogin = employee.Login;
            LastName = employee.LastName;
            FirstName = employee.FirstName;
            Login = employee.Login;

            _ = LoadRolesAsync(employee.RoleId);
        }

        public EmployeeEditViewModel() // Для красоты
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");

            SaveCommand = new RelayCommand(_ => _ = SaveAsync());
            CancelCommand = new RelayCommand(_ => _close?.Invoke());

        }

        public void SetClose(Action close) => _close = close;

        private async System.Threading.Tasks.Task LoadRolesAsync(int selectedRoleId)
        {
            try
            {
                var roles = await _httpClient.GetFromJsonAsync<EmployeeRoleDTO[]>("Employees/GetRoles");
                if (roles != null)
                {
                    Roles.Clear();
                    foreach (var role in roles)
                        Roles.Add(role);

                    SelectedRole = Roles.FirstOrDefault(r => r.Id == selectedRoleId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(LastName))
            {
                MessageBox.Show("Введите фамилию", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(FirstName))
            {
                MessageBox.Show("Введите имя", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Login))
            {
                MessageBox.Show("Введите логин", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedRole == null)
            {
                MessageBox.Show("Выберите роль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var request = new UpdateEmployeeRequestDTO
                {
                    Id = _employeeId,
                    Login = Login,
                    FirstName = FirstName,
                    LastName = LastName,
                    RoleId = SelectedRole.Id,
                    Password = string.IsNullOrWhiteSpace(Password) ? null : Password
                };

                var response = await _httpClient.PutAsJsonAsync("Employees", request);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("✅ Сотрудник обновлён", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    _close?.Invoke();
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
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
