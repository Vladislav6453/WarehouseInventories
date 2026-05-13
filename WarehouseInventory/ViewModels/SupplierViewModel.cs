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
    public class SupplierViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private ObservableCollection<SupplierDTO> _suppliers = new();
        private string _searchQuery = "";
        private SupplierDTO? _selectedSupplier;
        private bool _isMenuOpen;

        public ObservableCollection<SupplierDTO> Suppliers
        {
            get => _suppliers;
            set { _suppliers = value; OnPropertyChanged(); }
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

        public SupplierDTO? SelectedSupplier
        {
            get => _selectedSupplier;
            set { _selectedSupplier = value; OnPropertyChanged(); }
        }

        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            set { _isMenuOpen = value; OnPropertyChanged(); }
        }

        public ICommand AddSupplierCommand { get; }
        public ICommand EditSupplierCommand { get; }
        public ICommand DeleteSupplierCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand LogoutCommand { get; }

        public SupplierViewModel()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");

            AddSupplierCommand = new RelayCommand(_ => OpenSupplierEditWindow(null));
            EditSupplierCommand = new RelayCommand(_ => OpenSupplierEditWindow(SelectedSupplier), _ => SelectedSupplier != null);
            DeleteSupplierCommand = new RelayCommand(_ => _ = DeleteSupplierAsync(), _ => SelectedSupplier != null);
            ClearSearchCommand = new RelayCommand(_ => SearchQuery = "");
            LogoutCommand = new RelayCommand(_ => Application.Current.Shutdown());

            _ = LoadSuppliersAsync();
        }

        private async System.Threading.Tasks.Task LoadSuppliersAsync()
        {
            try
            {
                var suppliers = await _httpClient.GetFromJsonAsync<SupplierDTO[]>("Suppliers");
                if (suppliers != null)
                {
                    Suppliers.Clear();
                    foreach (var supplier in suppliers)
                        Suppliers.Add(supplier);
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
                var all = await _httpClient.GetFromJsonAsync<SupplierDTO[]>("Suppliers");
                if (all != null)
                {
                    var filtered = string.IsNullOrWhiteSpace(SearchQuery)
                        ? all
                        : all.Where(s => s.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                        s.Phone.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                        s.Email.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                             .ToArray();

                    Suppliers.Clear();
                    foreach (var supplier in filtered)
                        Suppliers.Add(supplier);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenSupplierEditWindow(SupplierDTO? supplier)
        {
            var window = new SuppliersEditWindow(supplier);
            window.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            window.ShowDialog();
            _ = LoadSuppliersAsync();
        }

        private async System.Threading.Tasks.Task DeleteSupplierAsync()
        {
            if (SelectedSupplier == null) return;

            var result = MessageBox.Show($"Удалить поставщика \"{SelectedSupplier.Name}\"?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var response = await _httpClient.DeleteAsync($"Suppliers/{SelectedSupplier.Id}");

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("✅ Поставщик удалён", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadSuppliersAsync();
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
