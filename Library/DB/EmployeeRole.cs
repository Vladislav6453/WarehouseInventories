using System;
using System.Collections.Generic;

namespace Library.DB;

public partial class EmployeeRole
{
    public int Id { get; set; }

    public string Role { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
