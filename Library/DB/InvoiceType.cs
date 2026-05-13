using System;
using System.Collections.Generic;

namespace Library.DB;

public partial class InvoiceType
{
    public int Id { get; set; }

    public string Type { get; set; } = null!;

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
