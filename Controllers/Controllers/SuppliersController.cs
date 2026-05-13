using Library.DB;
using Library.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Data.Controllers;

namespace Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SuppliersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var suppliers = await _context.Suppliers
                .Select(s => new SupplierDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    Phone = s.Phone ?? "",
                    Email = s.Email ?? ""
                })
                .OrderBy(s => s.Name)
                .ToListAsync();

            return Ok(suppliers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound(new { message = "Поставщик не найден" });

            return Ok(new SupplierDTO
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Phone = supplier.Phone ?? "",
                Email = supplier.Email ?? ""
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSupplierRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Название поставщика обязательно" });

            var supplier = new Supplier
            {
                Name = request.Name,
                Phone = request.Phone ?? "",
                Email = request.Email ?? ""
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return Ok(new { id = supplier.Id, message = "Поставщик создан" });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateSupplierRequestDTO request)
        {
            var supplier = await _context.Suppliers.FindAsync(request.Id);
            if (supplier == null)
                return NotFound(new { message = "Поставщик не найден" });

            supplier.Name = request.Name;
            supplier.Phone = request.Phone ?? "";
            supplier.Email = request.Email ?? "";

            await _context.SaveChangesAsync();

            return Ok(new { message = "Поставщик обновлён" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound(new { message = "Поставщик не найден" });

            // Проверка: есть ли накладные у этого поставщика?
            var hasInvoices = await _context.Invoices.AnyAsync(i => i.SupplierId == id);
            if (hasInvoices)
                return BadRequest(new { message = "Нельзя удалить поставщика, у которого есть накладные" });

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Поставщик удалён" });
        }
    }
}
