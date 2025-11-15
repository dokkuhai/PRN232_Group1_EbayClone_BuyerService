using EbayCloneBuyerService_CoreAPI.Repositories.Interface;
using EbayCloneBuyerService_CoreAPI.Services.Interface;
//using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace EbayCloneBuyerService_CoreAPI.Controllers
{
    [Route("api/")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductServices _product;
        public ProductController(IProductServices product)
        {
            _product = product;
        }
        [HttpGet("Product")]
        [EnableQuery]
        public IActionResult GetAllProducts()
        {
            var products = _product.GetAllAsync();
            return Ok(products);
        }
        [HttpGet("Product/{id}")]
        [EnableQuery]
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
