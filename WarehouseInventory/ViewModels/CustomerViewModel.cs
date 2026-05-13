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
    public class CustomerViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private ObservableCollection<CustomerDTO> _customers = new();
        private string _searchQuery = "";
        private CustomerDTO? _selectedCustomer;
        private bool _isMenuOpen;

        public ObservableCollection<CustomerDTO> Customers
        {
            get => _customers;
            set { _customers = value; OnPropertyChanged(); }
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

        public CustomerDTO? SelectedCustomer
        {
            get => _selectedCustomer;
            set { _selectedCustomer = value; OnPropertyChanged(); }
        }

        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            set { _isMenuOpen = value; OnPropertyChanged(); }
        }

        public ICommand ToggleMenuCommand { get; }
        public ICommand AddCustomerCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand LogoutCommand { get; }

        public CustomerViewModel()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");

            ToggleMenuCommand = new RelayCommand(_ => IsMenuOpen = !IsMenuOpen);
            AddCustomerCommand = new RelayCommand(_ => OpenCustomerEditWindow(null));
            EditCustomerCommand = new RelayCommand(_ => OpenCustomerEditWindow(SelectedCustomer), _ => SelectedCustomer != null);
            DeleteCustomerCommand = new RelayCommand(_ => _ = DeleteCustomerAsync(), _ => SelectedCustomer != null);
            ClearSearchCommand = new RelayCommand(_ => SearchQuery = "");
            LogoutCommand = new RelayCommand(_ => Application.Current.Shutdown());

            _ = LoadCustomersAsync();
        }

        private async System.Threading.Tasks.Task LoadCustomersAsync()
        {
            try
            {
                var customers = await _httpClient.GetFromJsonAsync<CustomerDTO[]>("Customers");
                if (customers != null)
                {
                    Customers.Clear();
                    foreach (var customer in customers)
                        Customers.Add(customer);
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
                var all = await _httpClient.GetFromJsonAsync<CustomerDTO[]>("Customers");
                if (all != null)
                {
                    var filtered = string.IsNullOrWhiteSpace(SearchQuery)
                        ? all
                        : all.Where(c => c.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                        c.Phone.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                        c.Email.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                             .ToArray();

                    Customers.Clear();
                    foreach (var customer in filtered)
                        Customers.Add(customer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenCustomerEditWindow(CustomerDTO? customer)
        {
            var window = new CustomersEditWindow(customer);
            window.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            window.ShowDialog();
            _ = LoadCustomersAsync();
        }

        private async System.Threading.Tasks.Task DeleteCustomerAsync()
        {
            if (SelectedCustomer == null) return;

            var result = MessageBox.Show($"Удалить клиента \"{SelectedCustomer.Name}\"?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var response = await _httpClient.DeleteAsync($"Customers/{SelectedCustomer.Id}");

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("✅ Клиент удалён", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadCustomersAsync();
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
