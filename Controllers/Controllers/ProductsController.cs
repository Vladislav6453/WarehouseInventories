using Library.DB;
using Library.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Data.Controllers;

namespace Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description ?? "",
                    Price = p.Price,
                    Quantity = p.Quantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : ""
                }).ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
                return NotFound(new {message = "Товар не найден"});
            return Ok(new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description ?? "",
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name ?? ""
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateProductRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace((request.Name)))
                return BadRequest(new { message = "Название товара обязательно" });
            var product = await _context.Products.FindAsync(request.Id);
            if (product == null)
                return NotFound(new {message = "Товар не найден"});
            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.CategoryId = request.CategoryId;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Товар обновлен" });
        }

        [HttpDelete("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c=> new CategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToListAsync();
            return Ok(categories);
        }
    }
}
