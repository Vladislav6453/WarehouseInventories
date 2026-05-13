using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Input;
using Library.DB;
using Library.DTO;
using WarehouseInventory;

namespace WarehouseInventory.ViewModels;

public class LoginViewModel : BaseViewModel
{
        private readonly HttpClient _httpClient;
        private string _login = "";
        private Action? _close;
        private object? _passwordBox;

        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }

        public event Action? OnLoginSuccess;

        public LoginViewModel()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");
            
            LoginCommand = new RelayCommand(_ => _ = LoginAsync());
            GoToRegisterCommand = new RelayCommand(_ => GoToRegister());
        }

        public void SetClose(Action close)
        {
            _close = close;
        }
        public void SetPasswordBinding(object passwordBox)
        {
            _passwordBox = passwordBox;
        }

        private string? GetPassword()
        {
            return (_passwordBox as System.Windows.Controls.PasswordBox)?.Password;
        }

        private async System.Threading.Tasks.Task LoginAsync()
        {
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

            try
            {
                var request = new LoginRequestDTO() { Login = Login, Password = password };
                var response = await _httpClient.PostAsJsonAsync("Auth/Login", request);

                if (response.IsSuccessStatusCode)
                {
                    var employee = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
                    
                    if (employee != null)
                    {
                        Application.Current.Properties["CurrentUser"] = employee;
                        OnLoginSuccess?.Invoke();
                    }
                    else
                    {
                        MessageBox.Show("Неверный логин или пароль", "Ошибка входа", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль", "Ошибка входа", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GoToRegister()
        {
            var registerWindow = new Windows.RegisterWindow();
            registerWindow.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            registerWindow.ShowDialog();
        }
}