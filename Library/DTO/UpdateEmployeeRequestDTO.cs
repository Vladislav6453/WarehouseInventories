using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.DTO
{
    public class UpdateEmployeeRequestDTO
    {
        public int Id { get; set; }
        public string Login { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int RoleId { get; set; }
        public string? Password { get; set; }
    }
}
