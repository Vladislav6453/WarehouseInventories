using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Input;
using Library.DB;
using Library.DTO;

namespace WarehouseInventory.ViewModels;

public class RegisterViewModel : BaseViewModel
{
        private readonly HttpClient _httpClient;
        private Action? _close;
        private string _lastName = "";
        private string _firstName = "";
        private string _login = "";
        private object? _passwordBox;
        private object? _confirmPasswordBox;
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

        public ICommand RegisterCommand { get; }
        public ICommand GoBackCommand { get; }

        public event Action? OnRegisterSuccess;
        public event Action? OnGoBack;

        public RegisterViewModel()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");
            
            RegisterCommand = new RelayCommand(_ => _ = RegisterAsync());
            GoBackCommand = new RelayCommand(_ => _close?.Invoke());
            
            _ = LoadRolesAsync();
        }

        public void SetClose(Action close)
        {
            _close = close;
        }
        public void SetPasswordBinding(object passwordBox, object confirmPasswordBox)
        {
            _passwordBox = passwordBox;
            _confirmPasswordBox = confirmPasswordBox;
        }

        private string? GetPassword() => (_passwordBox as System.Windows.Controls.PasswordBox)?.Password;
        private string? GetConfirmPassword() => (_confirmPasswordBox as System.Windows.Controls.PasswordBox)?.Password;

        private async System.Threading.Tasks.Task LoadRolesAsync()
        {
            try
            {
                var roles = await _httpClient.GetFromJsonAsync<EmployeeRoleDTO[]>("Auth/GetRoles");
                if (roles != null)
                {
                    Roles.Clear();
                    foreach (var role in roles)
                        Roles.Add(role);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task RegisterAsync()
        {
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

            var password = GetPassword();
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите пароль", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = GetConfirmPassword();
            if (password != confirm)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 4)
            {
                MessageBox.Show("Пароль должен быть не менее 4 символов", "Ошибка", 
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
                var request = new RegisterRequestDTO
                {
                    Login = Login,
                    Password = password,
                    FirstName = FirstName,
                    LastName = LastName,
                    RoleId = SelectedRole.Id
                };

                var response = await _httpClient.PostAsJsonAsync("Auth/Register", request);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("✅ Регистрация успешна!\n\nТеперь вы можете войти в систему.", 
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    OnRegisterSuccess?.Invoke();
                    _close?.Invoke();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка регистрации: {error}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GoBack()
        {
            OnGoBack?.Invoke();
        }
}