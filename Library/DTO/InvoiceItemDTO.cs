using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.DTO
{
    public class InvoiceItemDTO 
    {
        public ProductForInvoiceDTO? SelectedProduct { get; set; }
        public int Quantity { get; set; } = 1;

        // Цена может редактироваться вручную (на случай скидки)
        public decimal Price { get; set; }

        // Вычисляемое поле
        public decimal Total => Quantity * Price;

        // Удобные свойства для отправки на сервер
        public int ProductId => SelectedProduct?.Id ?? 0;
        public string ProductName => SelectedProduct?.Name ?? "";
    }
}
