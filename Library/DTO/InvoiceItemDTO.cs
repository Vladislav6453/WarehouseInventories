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
        private ProductForInvoiceDTO? _selectedProduct;
        private int _quantity = 1;
        private decimal _price;
        private int _productId;
        private string _productName = "";
        private decimal _total;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateTotal()
        {
            Total = Quantity * Price;
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
                    ProductId = value.Id;
                    ProductName = value.Name;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProductName));
                OnPropertyChanged(nameof(ProductId));
                UpdateTotal();
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
                UpdateTotal();
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged();
                UpdateTotal();
            }
        }
        
        public int ProductId
        {
            get => _productId;
            set { _productId = value; OnPropertyChanged(); }
        }

        public string ProductName
        {
            get => _productName;
            set { _productName = value; OnPropertyChanged(); }
        }

        public decimal Total
        {
            get => _total;
            private set
            {
                _total = value;
                OnPropertyChanged();
                System.Diagnostics.Debug.WriteLine($"Total updated: {_total}");
            }
        }
        
    }
}
