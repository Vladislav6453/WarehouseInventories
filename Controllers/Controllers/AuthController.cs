using Library.DB;
using Library.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Data.Controllers;

namespace Controllers.Controllers 
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }
        
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            var employee = await _context.Employees
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Login == request.Login);
            
            if (employee == null || employee.Password != request.Password)
                return Unauthorized(new { message = "Неверный логин или пароль" });
            
            return Ok(new LoginResponseDTO
            {
                Id = employee.Id,
                Login = employee.Login,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                RoleId = employee.RoleId,
                RoleName = employee.Role.Role ?? ""
            });
        }
        
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            var exists = await _context.Employees.AnyAsync(e => e.Login == request.Login);
            if (exists)
                return BadRequest("Логин уже существует");
            
            var employee = new Employee
            {
                Login = request.Login,
                Password = request.Password, 
                FirstName = request.FirstName,
                LastName = request.LastName,
                RoleId = request.RoleId
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Регистрация успешна" });
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
                .ToArrayAsync();

            return Ok(roles);
        }
        
        [HttpGet("HasAnyUsers")]
        public async Task<IActionResult> HasAnyUsers()
        {
            var hasUsers = await _context.Employees.AnyAsync();
            return Ok(new{HasUsers = hasUsers});
        }
    }
}