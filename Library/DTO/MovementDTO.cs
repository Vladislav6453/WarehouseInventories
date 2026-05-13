using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.DTO
{
    public class MovementDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public int MovementTypeId { get; set; }
        public string MovementType { get; set; } = "";
        public DateTime Date { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = "";
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public decimal TotalAmount { get; set; }
    }
}
