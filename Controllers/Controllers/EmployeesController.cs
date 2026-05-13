using Library.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Data.Controllers;

namespace Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _context.Employees
                .Include(e => e.Role)
                .Select(e => new EmployeeDTO
                {
                    Id = e.Id,
                    Login = e.Login,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    RoleId = e.RoleId,
                    RoleName = e.Role != null ? e.Role.Role : ""
                })
                .OrderBy(e => e.LastName)
                .ToListAsync();

            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
                return NotFound(new { message = "Сотрудник не найден" });

            return Ok(new EmployeeDTO
            {
                Id = employee.Id,
                Login = employee.Login,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                RoleId = employee.RoleId,
                RoleName = employee.Role?.Role ?? ""
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateEmployeeRequestDTO request)
        {
            var employee = await _context.Employees.FindAsync(request.Id);
            if (employee == null)
                return NotFound(new { message = "Сотрудник не найден" });

            // Проверка уникальности логина (если изменился)
            if (employee.Login != request.Login)
            {
                var loginExists = await _context.Employees.AnyAsync(e => e.Login == request.Login);
                if (loginExists)
                    return BadRequest(new { message = "Логин уже существует" });
            }

            employee.Login = request.Login;
            employee.FirstName = request.FirstName;
            employee.LastName = request.LastName;
            employee.RoleId = request.RoleId;

            // Если пароль передан и не пустой — обновляем
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                employee.Password = request.Password;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Сотрудник обновлён" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound(new { message = "Сотрудник не найден" });

            // Нельзя удалить администратора? (опционально)
            if (employee.RoleId == 1)
                return BadRequest(new { message = "Нельзя удалить администратора" });

            // Проверка: есть ли накладные у этого сотрудника?
            var hasInvoices = await _context.Invoices.AnyAsync(i => i.EmployeeId == id);
            if (hasInvoices)
                return BadRequest(new { message = "Нельзя удалить сотрудника, у которого есть накладные" });

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Сотрудник удалён" });
        }

        [HttpGet("GetRoles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.EmployeeRoles
                .Select(r => new EmployeeRoleDTO
                {
                    Id = r.Id,
                    Role = r.Role
                })
                .ToListAsync();

            return Ok(roles);
        }
    }
}
