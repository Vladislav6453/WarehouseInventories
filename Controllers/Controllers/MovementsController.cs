using Library.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Data.Controllers;

namespace Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovementsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MovementsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? movementTypeId = null)
        {
            var query = _context.StockMovements
                .Include(m => m.Product)
                .Include(m => m.MovementType)
                .Include(m => m.Invoice)
                .ThenInclude(i => i.Employee)
                .OrderByDescending(m => m.Date)
                .AsQueryable();

            if (movementTypeId.HasValue && movementTypeId.Value > 0)
            {
                query = query.Where(m => m.MovementTypeId == movementTypeId.Value);
            }

            var movements = await query
                .Select(m => new MovementDTO
                {
                    Id = m.Id,
                    ProductId = m.ProductId,
                    ProductName = m.Product != null ? m.Product.Name : "",
                    Quantity = m.Quantity,
                    MovementTypeId = m.MovementTypeId,
                    MovementType = m.MovementType != null ? m.MovementType.Name : "",
                    Date = m.Date,
                    InvoiceId = m.InvoiceId,
                    InvoiceNumber = m.Invoice != null ? m.Invoice.Number : "",
                    EmployeeId = m.Invoice != null && m.Invoice.Employee != null ? m.Invoice.Employee.Id : 0,
                    EmployeeName = m.Invoice != null && m.Invoice.Employee != null
                        ? $"{m.Invoice.Employee.LastName} {m.Invoice.Employee.FirstName}"
                        : "",
                    TotalAmount = m.Quantity * (m.Product != null ? m.Product.Price : 0)
                })
                .ToListAsync();

            return Ok(movements);
        }

        [HttpGet("GetMovementTypes")]
        public async Task<IActionResult> GetMovementTypes()
        {
            var types = await _context.MovementTypes
                .Select(t => new MovementTypeDTO
                {
                    Id = t.Id,
                    Type = t.Name
                })
                .ToListAsync();

            return Ok(types);
        }

        [HttpGet("ByProduct/{productId}")]
        public async Task<IActionResult> GetByProductId(int productId)
        {
            var movements = await _context.StockMovements
                .Include(m => m.Product)
                .Include(m => m.MovementType)
                .Include(m => m.Invoice)
                .ThenInclude(i => i.Employee)
                .Where(m => m.ProductId == productId)
                .OrderByDescending(m => m.Date)
                .Select(m => new MovementDTO
                {
                    Id = m.Id,
                    ProductId = m.ProductId,
                    ProductName = m.Product != null ? m.Product.Name : "",
                    Quantity = m.Quantity,
                    MovementTypeId = m.MovementTypeId,
                    MovementType = m.MovementType != null ? m.MovementType.Name : "",
                    Date = m.Date,
                    InvoiceId = m.InvoiceId,
                    InvoiceNumber = m.Invoice != null ? m.Invoice.Number : "",
                    EmployeeName = m.Invoice != null && m.Invoice.Employee != null
                        ? $"{m.Invoice.Employee.LastName} {m.Invoice.Employee.FirstName}"
                        : "",
                    TotalAmount = m.Quantity * (m.Product != null ? m.Product.Price : 0)
                })
                .ToListAsync();

            return Ok(movements);
        }
    }
}
