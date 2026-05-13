using System;
using System.Collections.Generic;
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
    public class CustomerEditViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private Action? _close;
        private int? _editingId;
        private bool _isEditMode;

        private string _name = "";
        private string _phone = "";
        private string _email = "";
        private string _address = "";

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(); }
        }

        public string WindowTitle => _isEditMode ? "✏️ Редактирование клиента" : "➕ Добавление клиента";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // Конструктор для РЕДАКТИРОВАНИЯ
        public CustomerEditViewModel(CustomerDTO customer)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");

            SaveCommand = new RelayCommand(_ => _ = SaveAsync());
            CancelCommand = new RelayCommand(_ => _close?.Invoke());

            _isEditMode = true;
            _editingId = customer.Id;
            Name = customer.Name;
            Phone = customer.Phone;
            Email = customer.Email;
            Address = customer.Address;
        }

        // Конструктор для ДОБАВЛЕНИЯ
        public CustomerEditViewModel()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");

            SaveCommand = new RelayCommand(_ => _ = SaveAsync());
            CancelCommand = new RelayCommand(_ => _close?.Invoke());

            _isEditMode = false;
        }

        public void SetClose(Action close) => _close = close;

        private async System.Threading.Tasks.Task SaveAsync()
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("❌ Введите название клиента", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Phone))
            {
                MessageBox.Show("❌ Введите телефон", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("❌ Введите email", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_isEditMode && _editingId.HasValue)
                {
                    // РЕДАКТИРОВАНИЕ
                    var request = new UpdateCustomerRequestDTO
                    {
                        Id = _editingId.Value,
                        Name = Name,
                        Phone = Phone,
                        Email = Email,
                        Address = Address
                    };

                    var response = await _httpClient.PutAsJsonAsync("Customers", request);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("✅ Клиент обновлён", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        _close?.Invoke();
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"❌ Ошибка: {error}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // ДОБАВЛЕНИЕ
                    var request = new CreateCustomerRequestDTO
                    {
                        Name = Name,
                        Phone = Phone,
                        Email = Email,
                        Address = Address
                    };

                    var response = await _httpClient.PostAsJsonAsync("Customers", request);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("✅ Клиент добавлен", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        _close?.Invoke();
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"❌ Ошибка: {error}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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
