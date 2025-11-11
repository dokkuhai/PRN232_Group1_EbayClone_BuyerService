using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using EbayCloneBuyerService_CoreAPI.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EbayCloneBuyerService_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductServices _product;
        public ProductController(IProductServices product)
        {
            _product = product;
        }
        [HttpGet]
        public IActionResult GetAllProducts()
        {
            var products = _product.GetAllAsync();
            return Ok(products);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _product.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
    }
}
