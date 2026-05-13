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
    public class InvoiceEditViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private string _number = "";
        private string _description = "";
        private DateTime _date = DateTime.Now;
        private InvoiceTypeDTO? _selectedType;
        private CounterpartyDTO? _selectedCounterparty;
        private ObservableCollection<InvoiceTypeDTO> _types = new();
        private ObservableCollection<CounterpartyDTO> _counterparties = new();
        private ObservableCollection<InvoiceItemDTO> _items = new();
        private ObservableCollection<ProductForInvoiceDTO> _availableProducts = new();
        private Action? _close;
        private int _currentEmployeeId = 1;

        public string Number
        {
            get => _number;
            set { _number = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        public InvoiceTypeDTO? SelectedType
        {
            get => _selectedType;
            set
            {
                _selectedType = value;
                OnPropertyChanged();
                _ = LoadCounterpartiesAsync();
            }
        }

        public CounterpartyDTO? SelectedCounterparty
        {
            get => _selectedCounterparty;
            set { _selectedCounterparty = value; OnPropertyChanged(); }
        }

        public ObservableCollection<InvoiceTypeDTO> Types
        {
            get => _types;
            set { _types = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CounterpartyDTO> Counterparties
        {
            get => _counterparties;
            set { _counterparties = value; OnPropertyChanged(); }
        }

        public ObservableCollection<InvoiceItemDTO> Items
        {
            get => _items;
            set { _items = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ProductForInvoiceDTO> AvailableProducts
        {
            get => _availableProducts;
            set { _availableProducts = value; OnPropertyChanged(); }
        }

        public decimal TotalAmount => Items.Sum(i => i.Total);

        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public InvoiceEditViewModel()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");

            AddItemCommand = new RelayCommand(_ => AddItem());
            RemoveItemCommand = new RelayCommand(RemoveItem);
            SaveCommand = new RelayCommand(_ => _ = SaveAsync());
            CancelCommand = new RelayCommand(_ => _close?.Invoke());

            _ = LoadTypesAsync();
            _ = LoadProductsAsync();
        }

        public void SetClose(Action close) => _close = close;

        private async System.Threading.Tasks.Task LoadTypesAsync()
        {
            try
            {
                var types = await _httpClient.GetFromJsonAsync<InvoiceTypeDTO[]>("Invoices/GetTypes");
                if (types != null)
                {
                    Types.Clear();
                    foreach (var type in types)
                        Types.Add(type);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка загрузки типов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadCounterpartiesAsync()
        {
            try
            {
                if (SelectedType == null) return;

                var url = SelectedType.Type == "Приходная"
                    ? "Invoices/GetSuppliers"
                    : "Invoices/GetCustomers";

                var counterparts = await _httpClient.GetFromJsonAsync<CounterpartyDTO[]>(url);
                if (counterparts != null)
                {
                    Counterparties.Clear();
                    foreach (var c in counterparts)
                        Counterparties.Add(c);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка загрузки контрагентов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadProductsAsync()
        {
            try
            {
                var products = await _httpClient.GetFromJsonAsync<ProductForInvoiceDTO[]>("Invoices/GetProducts");
                if (products != null)
                {
                    AvailableProducts.Clear();
                    foreach (var product in products)
                        AvailableProducts.Add(product);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка загрузки товаров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddItem()
        {
            Items.Add(new InvoiceItemDTO
            {
                ProductId = 0,
                ProductName = "",
                Quantity = 1,
                Price = 0
            });
        }

        private void RemoveItem(object? parameter)
        {
            if (parameter is InvoiceItemDTO item)
                Items.Remove(item);
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Number))
            {
                MessageBox.Show("❌ Введите номер накладной", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedType == null)
            {
                MessageBox.Show("❌ Выберите тип накладной", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedCounterparty == null)
            {
                var msg = SelectedType.Type == "Приходная" ? "поставщика" : "клиента";
                MessageBox.Show($"❌ Выберите {msg}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var invalidItems = Items.Where(i => i.ProductId == 0 || string.IsNullOrWhiteSpace(i.ProductName)).ToList();
            if (invalidItems.Any())
            {
                MessageBox.Show("❌ Заполните все товары", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Items.Any(i => i.Quantity <= 0))
            {
                MessageBox.Show("❌ Количество товара должно быть больше 0", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedType.Type == "Расходная")
            {
                foreach (var item in Items)
                {
                    var product = AvailableProducts.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null && product.Price != item.Price)
                    {
                        var result = MessageBox.Show($"Цена товара \"{item.ProductName}\" изменилась. Использовать актуальную цену {product.Price}₽ вместо {item.Price}₽?",
                            "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                            item.Price = product.Price;
                    }
                }
            }

            try
            {
                var request = new CreateInvoiceRequestDTO
                {
                    Number = Number,
                    Description = Description,
                    Date = Date,
                    TypeId = SelectedType.Id,
                    EmployeeId = _currentEmployeeId,
                    SupplierId = SelectedType.Type == "Приходная" ? SelectedCounterparty.Id : null,
                    CustomerId = SelectedType.Type == "Расходная" ? SelectedCounterparty.Id : null,
                    Items = Items.ToList()
                };

                var response = await _httpClient.PostAsJsonAsync("Invoices", request);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("✅ Накладная создана", "Успех",
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
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
