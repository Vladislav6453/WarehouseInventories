using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.DTO
{
    public class CreateInvoiceRequestDTO
    {
        public string Number { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Date { get; set; }
        public int TypeId { get; set; }
        public int EmployeeId { get; set; }
        public int? SupplierId { get; set; }
        public int? CustomerId { get; set; }
        public List<InvoiceItemDTO> Items { get; set; } = new();
    }
}
