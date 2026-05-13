using Library.DB;
using Library.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Data.Controllers;

namespace Controllers.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InvoicesController : ControllerBase
{
    private readonly AppDbContext _context;
    public InvoicesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var invoices = await _context.Invoices
            .Include(i => i.InvoiceType)
            .Include(i => i.Employee)
            .Include(i => i.Supplier)
            .Include(i => i.Customer)
            .OrderByDescending(i => i.Date)
            .Select(i => new InvoiceDto
            {
                Id = i.Id,
                Number = i.Number,
                Description = i.Description,
                Date = i.Date,
                TypeId = i.TypeId,
                Type = i.InvoiceType != null ? i.InvoiceType.Type : "",
                EmployeeId = i.EmployeeId,
                EmployeeName = i.Employee != null ? $"{i.Employee.LastName} {i.Employee.FirstName}" : "",
                SupplierId = i.SupplierId,
                SupplierName = i.Supplier != null ? i.Supplier.Name : "",
                CustomerId = i.CustomerId,
                CustomerName = i.Customer != null ? i.Customer.Name : "",
                TotalAmount = i.TotalAmount
            })
            .ToListAsync();

        return Ok(invoices);
    }

    [HttpGet("GetTypes")]
    public async Task<IActionResult> GetTypes()
    {
        var types = await _context.InvoiceTypes
            .Select(t => new InvoiceTypeDTO
            {
                Id = t.Id,
                Type = t.Type
            })
            .ToListAsync();

        return Ok(types);
    }

    [HttpGet("GetSuppliers")]
    public async Task<IActionResult> GetSuppliers()
    {
        var suppliers = await _context.Suppliers
            .Select(s => new CounterpartyDTO
            {
                Id = s.Id,
                Name = s.Name,
                Type = "Supplier"
            })
            .ToListAsync();

        return Ok(suppliers);
    }

    [HttpGet("GetCustomers")]
    public async Task<IActionResult> GetCustomers()
    {
        var customers = await _context.Customers
            .Select(c => new CounterpartyDTO
            {
                Id = c.Id,
                Name = c.Name,
                Type = "Customer"
            })
            .ToListAsync();

        return Ok(customers);
    }

    [HttpGet("GetProducts")]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products
            .Select(p => new ProductForInvoiceDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequestDTO request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (await _context.Invoices.AnyAsync(i => i.Number == request.Number))
                return BadRequest(new { message = "Накладная с таким номером уже существует" });

            var invoice = new Invoice
            {
                Number = request.Number,
                Description = request.Description ?? "",
                Date = request.Date,
                TypeId = request.TypeId,
                EmployeeId = request.EmployeeId,
                SupplierId = request.SupplierId,
                CustomerId = request.CustomerId,
                TotalAmount = request.Items.Sum(i => i.Total)
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            int movementTypeId = request.TypeId == 1 ? 1 : 2;

            foreach (var item in request.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                    return BadRequest(new { message = $"Товар с ID {item.ProductId} не найден" });

                var movement = new StockMovement
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    MovementTypeId = movementTypeId,
                    Date = request.Date,
                    InvoiceId = invoice.Id
                };
                _context.StockMovements.Add(movement);

                if (request.TypeId == 1)
                    product.Quantity += item.Quantity;
                else
                    product.Quantity -= item.Quantity;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { id = invoice.Id, message = "Накладная создана" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(new { message = ex.Message });
        }
    }
}