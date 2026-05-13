

using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Input;
using Library.DTO;

namespace WarehouseInventory.ViewModels
{
    public class ProductEditViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private string _name = "";
        private string _description = "";
        private string _price = "";
        private CategoryDTO? _selectedCategory;
        private ObservableCollection<CategoryDTO> _categories = new();
        private Action? _close;
        private int? _editingId;
        private bool _isEditMode;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public string Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        public CategoryDTO? SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CategoryDTO> Categories
        {
            get => _categories;
            set { _categories = value; OnPropertyChanged(); }
        }

        public string WindowTitle => _isEditMode ? "✏️ Редактирование товара" : "➕ Добавление товара";
        public string QuantityInfo => "ℹ️ Количество товара изменяется только через накладные";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ProductEditViewModel(ProductDTO product)
        {

            /*_httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");
            
            SaveCommand = new RelayCommand(_ => _ = SaveAsync());
            CancelCommand = new RelayCommand(_ => _close?.Invoke());
            _productId = product?.Id ?? 0;
            Name = product?.Name ?? "";
            Description = product?.Description ?? "";
            Price = product?.Price.ToString("0.00") ?? "0.00";
            if (product != null)
            {
                _selectedCategory = new CategoryDTO
                {
                    Id = product.CategoryId,
                    Name = product.CategoryName ?? ""
                };
            }
            
            _ = LoadCategoriesAsync();*/

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");

            SaveCommand = new RelayCommand(_ => _ = SaveAsync());
            CancelCommand = new RelayCommand(_ => _close?.Invoke());

            _isEditMode = true;
            _editingId = product.Id;
            Name = product.Name;
            Description = product.Description;
            Price = product.Price.ToString("0.00");
            _selectedCategory = new CategoryDTO { Id = product.CategoryId, Name = product.CategoryName ?? ""};

            _ = LoadCategoriesAsync();
        }

        public ProductEditViewModel()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");

            SaveCommand = new RelayCommand(_ => _ = SaveAsync());
            CancelCommand = new RelayCommand(_ => _close?.Invoke());

            _isEditMode = false;

            _ = LoadCategoriesAsync();
        }

        public void SetClose(Action close) => _close = close;

        private async System.Threading.Tasks.Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _httpClient.GetFromJsonAsync<CategoryDTO[]>("Products/GetCategories");
                if (categories != null)
                {
                    Categories.Clear();
                    foreach (var category in categories)
                        Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("❌ Введите название товара", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(Price, out var priceValue))
            {
                MessageBox.Show("❌ Введите корректную цену", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedCategory == null)
            {
                MessageBox.Show("❌ Выберите категорию", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_isEditMode && _editingId.HasValue)
                {
                    // РЕДАКТИРОВАНИЕ (количество не трогаем!)
                    var request = new UpdateProductRequestDTO
                    {
                        Id = _editingId.Value,
                        Name = Name,
                        Description = Description,
                        Price = priceValue,
                        CategoryId = SelectedCategory.Id
                    };

                    var response = await _httpClient.PutAsJsonAsync("Products", request);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("✅ Товар обновлён", "Успех",
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
                    // ДОБАВЛЕНИЕ (количество = 0)
                    var request = new UpdateProductRequestDTO
                    {
                        Name = Name,
                        Description = Description,
                        Price = priceValue,
                        CategoryId = SelectedCategory.Id
                    };

                    var response = await _httpClient.PostAsJsonAsync("Products", request);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("✅ Товар добавлен в справочник", "Успех",
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
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
