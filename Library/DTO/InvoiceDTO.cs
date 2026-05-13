using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.DTO
{
    public class InvoiceDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Date { get; set; }
        public int TypeId { get; set; }
        public string Type { get; set; } = "";
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; } = "";
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public string Counterparty => SupplierId.HasValue ? SupplierName : CustomerName;
        public decimal TotalAmount { get; set; }
    }
}
