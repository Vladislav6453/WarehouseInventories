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
    public class MovementViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private ObservableCollection<MovementDTO> _allMovements = new();
        private ObservableCollection<MovementDTO> _movements = new();
        private ObservableCollection<MovementTypeDTO> _movementTypes = new();
        private string _searchQuery = "";
        private MovementTypeDTO? _selectedMovementType;
        private bool _isMenuOpen;

        public ObservableCollection<MovementDTO> Movements
        {
            get => _movements;
            set { _movements = value; OnPropertyChanged(); }
        }

        public ObservableCollection<MovementTypeDTO> MovementTypes
        {
            get => _movementTypes;
            set { _movementTypes = value; OnPropertyChanged(); }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                ApplyFilters(); // Применяем оба фильтра
            }
        }

        public MovementTypeDTO? SelectedMovementType
        {
            get => _selectedMovementType;
            set
            {
                _selectedMovementType = value;
                OnPropertyChanged();
                ApplyFilters(); // Применяем оба фильтра
            }
        }

        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            set { _isMenuOpen = value; OnPropertyChanged(); }
        }

        public ICommand ClearSearchCommand { get; }
        public ICommand LogoutCommand { get; }

        public MovementViewModel()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5059/api/");

            ClearSearchCommand = new RelayCommand(_ => SearchQuery = "");
            LogoutCommand = new RelayCommand(_ => Application.Current.Shutdown());

            _ = LoadMovementTypesAsync();
            _ = LoadMovementsAsync();
        }

        private async System.Threading.Tasks.Task LoadMovementTypesAsync()
        {
            try
            {
                var types = await _httpClient.GetFromJsonAsync<MovementTypeDTO[]>("Movements/GetMovementTypes");
                if (types != null)
                {
                    MovementTypes.Clear();
                    MovementTypes.Add(new MovementTypeDTO { Id = 0, Type = "Все типы" });
                    foreach (var type in types)
                        MovementTypes.Add(type);

                    SelectedMovementType = MovementTypes.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка загрузки типов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task LoadMovementsAsync()
        {
            try
            {
                _allMovements.Clear();
                var movements = await _httpClient.GetFromJsonAsync<MovementDTO[]>("Movements");
                if (movements != null)
                {
                    foreach (var movement in movements)
                        _allMovements.Add(movement);
                }
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка загрузки движений: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            var result = _allMovements.AsEnumerable();

            // Фильтр по типу (Приход/Расход)
            if (SelectedMovementType != null && SelectedMovementType.Id > 0)
            {
                result = result.Where(m => m.MovementTypeId == SelectedMovementType.Id);
            }

            // Поиск по тексту
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                result = result.Where(m =>
                    m.ProductName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    m.InvoiceNumber.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    m.EmployeeName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
            }

            Movements.Clear();
            foreach (var movement in result)
                Movements.Add(movement);
        }
    }
}
