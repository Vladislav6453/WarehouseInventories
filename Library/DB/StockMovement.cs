using System;
using System.Collections.Generic;

namespace Library.DB;

public partial class StockMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public int MovementTypeId { get; set; }

    public DateTime Date { get; set; }

    public int InvoiceId { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;

    public virtual MovementType MovementType { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
