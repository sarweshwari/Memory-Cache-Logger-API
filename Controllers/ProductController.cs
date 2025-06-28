using CacheLoggerAppAPI.Model;
using CacheLoggerAppAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CacheLoggerAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IMemoryCache cache;
        private readonly ILogger<ProductController> logger;
        private readonly ProductService service;

        public ProductController(IMemoryCache cache, ILogger<ProductController> logger, ProductService service)
        {
            this.cache = cache;
            this.logger = logger;
            this.service = service;
        }

        [HttpGet("{id}")]
        public ActionResult<Product> GetProduct(int id)
        {
            string cacheKey = $"product_{id}";
            if (!cache.TryGetValue(cacheKey, out Product? product))
            {
                logger.LogInformation("Cache miss for product {id}", id);
                product = service.GetProductById(id);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(60));

                cache.Set(cacheKey, product, cacheEntryOptions);

                logger.LogInformation("Product {id} added to cache", id);
            }
            else
            {
                logger.LogInformation("Cache hit for product {id}", id);
            }
            return Ok(product);
        }

        [HttpGet]
        public ActionResult<List<Product>> GetAllProducts()
        {
            string cacheKey = "all_products";

            if (!cache.TryGetValue(cacheKey, out List<Product>? products))
            {
                logger.LogInformation("Cache miss for all products");
                products = service.GetAllProducts();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

                cache.Set(cacheKey, products, cacheEntryOptions);

                logger.LogInformation("All products added to cache");
            }
            else
            {
                logger.LogInformation("Cache hit for all products");
            }

            return Ok(products);
        }

    }
}