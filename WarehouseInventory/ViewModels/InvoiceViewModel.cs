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
    public class InvoiceViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private ObservableCollection<InvoiceDto> _invoices = new();
        private string _searchQuery = "";
        private bool _isMenuOpen;

        public ObservableCollection<InvoiceDto> Invoices
        {
            get => _invoices;
            set { _invoices = value; OnPropertyChanged(); }
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

        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            set { _isMenuOpen = value; OnPropertyChanged(); }
        }

        public ICommand ToggleMenuCommand { get; }
        public ICommand OpenCreateInvoiceCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand NavigateToMainCommand { get; }
        public ICommand LogoutCommand { get; }

        public InvoiceViewModel()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");

            ToggleMenuCommand = new RelayCommand(_ => IsMenuOpen = !IsMenuOpen);
            OpenCreateInvoiceCommand = new RelayCommand(_ => OpenCreateInvoice());
            ClearSearchCommand = new RelayCommand(_ => SearchQuery = "");
            NavigateToMainCommand = new RelayCommand(_ => NavigateToMain());
            LogoutCommand = new RelayCommand(_ => Application.Current.Shutdown());

            _ = LoadInvoicesAsync();
        }

        private async System.Threading.Tasks.Task LoadInvoicesAsync()
        {
            try
            {
                var invoices = await _httpClient.GetFromJsonAsync<InvoiceDto[]>("Invoices");
                if (invoices != null)
                {
                    Invoices.Clear();
                    foreach (var invoice in invoices)
                        Invoices.Add(invoice);
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
                var all = await _httpClient.GetFromJsonAsync<InvoiceDto[]>("Invoices");
                if (all != null)
                {
                    var filtered = string.IsNullOrWhiteSpace(SearchQuery)
                        ? all
                        : all.Where(i => i.Number.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                        i.Counterparty.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                             .ToArray();

                    Invoices.Clear();
                    foreach (var invoice in filtered)
                        Invoices.Add(invoice);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenCreateInvoice()
        {
            var window = new InvoicesEditWindow();
            window.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            window.ShowDialog();
            _ = LoadInvoicesAsync();
        }

        private void NavigateToMain()
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w is InvoicesWindow)?.Close();
        }
    }
}
