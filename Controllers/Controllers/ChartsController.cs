using Library.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Data.Controllers;

namespace Controllers.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChartsController : ControllerBase
{
   private readonly AppDbContext _context;
   public ChartsController(AppDbContext context)
   {
      _context = context;
   }
   
   [HttpGet("Movements")]
        public async Task<IActionResult> GetMovements([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var query = _context.StockMovements
                .Include(s => s.Product)
                .Include(s => s.MovementType)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(s => s.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(s => s.Date <= endDate.Value);

            var movements = await query
                .Select(s => new StockMovementDTO
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    ProductName = s.Product != null ? s.Product.Name : "",
                    Quantity = s.Quantity,
                    MovementTypeId = s.MovementTypeId,
                    MovementType = s.MovementType != null ? s.MovementType.Name : "",
                    Date = s.Date,
                    TotalAmount = s.Quantity * (s.Product != null ? s.Product.Price : 0)
                })
                .ToListAsync();

            return Ok(movements);
        }
        
        [HttpGet("Statistics")]
        public async Task<IActionResult> GetStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var query = _context.StockMovements
                .Include(s => s.MovementType)
                .Include(s => s.Product)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(s => s.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(s => s.Date <= endDate.Value);

            var totalMovements = await query.CountAsync();
            var totalIncome = await query
                .Where(s => s.MovementType != null && s.MovementType.Name == "Приход")
                .SumAsync(s => s.Quantity * (s.Product != null ? s.Product.Price : 0));
            var totalExpense = await query
                .Where(s => s.MovementType != null && s.MovementType.Name == "Расход")
                .SumAsync(s => s.Quantity * (s.Product != null ? s.Product.Price : 0));
            var currentStock = await _context.Products.SumAsync(p => p.Quantity);

            return Ok(new
            {
                TotalMovements = totalMovements,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                CurrentStock = currentStock
            });
        }
        
        [HttpGet("TopProducts")]
        public async Task<IActionResult> GetTopProducts([FromQuery] int count = 10)
        {
            var topProducts = await _context.StockMovements
                .Include(s => s.Product)
                .Where(s => s.MovementType != null && s.MovementType.Name == "Расход")
                .GroupBy(s => s.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(s => s.Quantity),
                    ProductName = g.First().Product != null ? g.First().Product.Name : ""
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(count)
                .ToListAsync();

            return Ok(topProducts);
        }
        
        [HttpGet("TopProductsByAmount")]
        public async Task<IActionResult> GetTopProductsByAmount([FromQuery] int count = 10)
        {
            var topProducts = await _context.StockMovements
                .Include(s => s.Product)
                .Where(s => s.MovementType != null && s.MovementType.Name == "Расход")
                .GroupBy(s => s.ProductId)
                .Select(g => new TopProductDTO
                {
                    ProductId = g.Key,
                    TotalQuantity = (int)g.Sum(s => s.Quantity * (s.Product != null ? s.Product.Price : 0)),
                    TotalAmount = g.Sum(s => s.Quantity * (s.Product != null ? s.Product.Price : 0)),
                    ProductName = g.First().Product != null ? g.First().Product.Name : ""
                })
                .OrderByDescending(x => x.TotalAmount)
                .Take(count)
                .ToListAsync();

            return Ok(topProducts);
        }
    
        
        [HttpGet("TopCustomers")]
        public async Task<IActionResult> GetTopCustomers([FromQuery] int count = 10)
        {
            var topCustomers = await _context.Invoices
                .Include(i => i.Customer)
                .Where(i => i.CustomerId != null)
                .GroupBy(i => i.CustomerId)
                .Select(g => new TopCustomerDTO
                {
                    Id = g.Key ?? 0,
                    Name = g.First().Customer != null ? g.First().Customer.Name : "",
                    TotalPurchases = g.Sum(i => i.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.TotalPurchases)
                .Take(count)
                .ToListAsync();

            return Ok(topCustomers);
        }
        
        [HttpGet("TopSuppliers")]
        public async Task<IActionResult> GetTopSuppliers([FromQuery] int count = 10)
        {
            var topSuppliers = await _context.Invoices
                .Include(i => i.Supplier)
                .Where(i => i.SupplierId != null)
                .GroupBy(i => i.SupplierId)
                .Select(g => new TopSupplierDTO
                {
                    Id = g.Key ?? 0,
                    Name = g.First().Supplier != null ? g.First().Supplier.Name : "",
                    TotalSupply = g.Sum(i => i.TotalAmount),
                    SupplyCount = g.Count()
                })
                .OrderByDescending(x => x.TotalSupply)
                .Take(count)
                .ToListAsync();

            return Ok(topSuppliers);
        }
        
        [HttpGet("ProductsStock")]
        public async Task<IActionResult> GetProductsStock()
        {
            var stocks = await _context.Products
                .Include(p => p.Category)
                .Select(p => new ProductStockDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    CategoryName = p.Category != null ? p.Category.Name : ""
                })
                .OrderByDescending(p => p.Quantity)
                .ToListAsync();
            return Ok(stocks);
        }
}