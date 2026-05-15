using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Library.DTO
{
    public class InvoiceItemDTO : INotifyPropertyChanged
    {
        // public ProductForInvoiceDTO? SelectedProduct { get; set; }
        // public int Quantity { get; set; } = 1;
        //
        // public decimal Price { get; set; }
        //
        // public decimal Total => Quantity * Price;
        //
        // public int ProductId => SelectedProduct?.Id ?? 0;
        // public string ProductName => SelectedProduct?.Name ?? "";
        
        private ProductForInvoiceDTO? _selectedProduct;
        private int _quantity = 1;
        private decimal _price;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ProductForInvoiceDTO? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                if (value != null)
                {
                    Price = value.Price;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProductName));
                OnPropertyChanged(nameof(ProductId));
                OnPropertyChanged(nameof(Total));
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        public decimal Total => Quantity * Price;

        public int ProductId => SelectedProduct?.Id ?? 0;
        public string ProductName => SelectedProduct?.Name ?? "";
    }
}
