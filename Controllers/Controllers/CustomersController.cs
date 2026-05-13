using Library.DB;
using Library.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Data.Controllers;

namespace Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _context.Customers
                .Select(c => new CustomerDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Phone = c.Phone ?? "",
                    Email = c.Email ?? "",
                    Address = c.Address ?? ""
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound(new { message = "Клиент не найден" });

            return Ok(new CustomerDTO
            {
                Id = customer.Id,
                Name = customer.Name,
                Phone = customer.Phone ?? "",
                Email = customer.Email ?? "",
                Address = customer.Address ?? ""
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Название клиента обязательно" });

            var customer = new Customer
            {
                Name = request.Name,
                Phone = request.Phone ?? "",
                Email = request.Email ?? "",
                Address = request.Address ?? ""
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return Ok(new { id = customer.Id, message = "Клиент создан" });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCustomerRequestDTO request)
        {
            var customer = await _context.Customers.FindAsync(request.Id);
            if (customer == null)
                return NotFound(new { message = "Клиент не найден" });

            customer.Name = request.Name;
            customer.Phone = request.Phone ?? "";
            customer.Email = request.Email ?? "";
            customer.Address = request.Address ?? "";

            await _context.SaveChangesAsync();

            return Ok(new { message = "Клиент обновлён" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound(new { message = "Клиент не найден" });

            var hasInvoices = await _context.Invoices.AnyAsync(i => i.CustomerId == id);
            if (hasInvoices)
                return BadRequest(new { message = "Нельзя удалить клиента, у которого есть накладные" });

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Клиент удалён" });
        }
    }
}
