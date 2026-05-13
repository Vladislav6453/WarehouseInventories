using System;
using System.Collections.Generic;

namespace Library.DB;

public partial class MovementType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
