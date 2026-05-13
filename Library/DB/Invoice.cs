using System;
using System.Collections.Generic;

namespace Library.DB;

public partial class Invoice
{
    public int Id { get; set; }

    public string Number { get; set; } = null!;

    public DateTime Date { get; set; }

    public int TypeId { get; set; }

    public string Description { get; set; } = null!;

    public int EmployeeId { get; set; }

    public int? SupplierId { get; set; }

    public int? CustomerId { get; set; }

    public decimal TotalAmount { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual Supplier? Supplier { get; set; }

    public virtual InvoiceType InvoiceType { get; set; } = null!;
}
