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
    public class SupplierEditViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private Action? _close;
        private int? _editingId;
        private bool _isEditMode;

        private string _name = "";
        private string _phone = "";
        private string _email = "";

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

        public string WindowTitle => _isEditMode ? "✏️ Редактирование поставщика" : "➕ Добавление поставщика";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // Конструктор для РЕДАКТИРОВАНИЯ
        public SupplierEditViewModel(SupplierDTO supplier)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");

            SaveCommand = new RelayCommand(_ => _ = SaveAsync());
            CancelCommand = new RelayCommand(_ => _close?.Invoke());

            _isEditMode = true;
            _editingId = supplier.Id;
            Name = supplier.Name;
            Phone = supplier.Phone;
            Email = supplier.Email;
        }

        // Конструктор для ДОБАВЛЕНИЯ
        public SupplierEditViewModel()
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
                MessageBox.Show("Введите название поставщика", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Phone))
            {
                MessageBox.Show("Введите телефон", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("Введите email", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_isEditMode && _editingId.HasValue)
                {
                    // РЕДАКТИРОВАНИЕ
                    var request = new UpdateSupplierRequestDTO
                    {
                        Id = _editingId.Value,
                        Name = Name,
                        Phone = Phone,
                        Email = Email
                    };

                    var response = await _httpClient.PutAsJsonAsync("Suppliers", request);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("✅ Поставщик обновлён", "Успех",
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
                else
                {
                    // ДОБАВЛЕНИЕ
                    var request = new CreateSupplierRequestDTO
                    {
                        Name = Name,
                        Phone = Phone,
                        Email = Email
                    };

                    var response = await _httpClient.PostAsJsonAsync("Suppliers", request);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("✅ Поставщик добавлен", "Успех",
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
