using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GGAWebApi.Entities;
using GGAWebApi.Models;
using System.Net;
using Microsoft.Graph;
using Microsoft.Identity.Web.Resource;
using Microsoft.AspNetCore.Authorization;

namespace GGAWebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    public class ProductsController : ControllerBase
    {
        private readonly GamesGlobalContext _context;
        public static IWebHostEnvironment _webHostEnvironment;
        private readonly GraphServiceClient _graphServiceClient;

        public ProductsController(GamesGlobalContext context, IWebHostEnvironment webHostEnvironment, GraphServiceClient graphServiceClient)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _graphServiceClient = graphServiceClient;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            // get loggedIn User
            var user = await _graphServiceClient.Me.Request().GetAsync();
            var username = user.UserPrincipalName;

            
            var products = await _context.Products.ToListAsync();
            if (products == null)
            {
                return NotFound();
            }
            return products.Where(p => p.Username == username).ToList();
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductUploadModel>> GetProduct(int id)
        {
            if (_context.Products == null)
            {
                  return NotFound();
            }
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }
            var model = new ProductUploadModel
            {
                Id = product.Id,
                ProductName = product.ProductName,
                ProductDescription = product.ProductDescription,
                ProductPrice = product.ProductPrice,
            };

            //add image to result
            string path = _webHostEnvironment.WebRootPath + "\\uploads\\";
            var filePath = path + product.ProductUrl;
            if(System.IO.File.Exists(filePath))
            {
                byte[] b = System.IO.File.ReadAllBytes(filePath);
                model.ProductImage = (IFormFile)File(b, "image/png");
            }

            return model;
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct([FromForm] ProductUploadModel product)
        {
            try
            {
                // get loggedIn User
                var user = await _graphServiceClient.Me.Request().GetAsync();
                var username = user.UserPrincipalName;
                //var username = "jaco@acoh.org.za";

                //upload ProductImage
                if (product.ProductImage.Length > 0)
                {
                    string path = _webHostEnvironment.WebRootPath + "\\uploads\\";
                    if(!System.IO.Directory.Exists(path)) { System.IO.Directory.CreateDirectory(path); }
                    using(FileStream fileStream = System.IO.File.Create(path + product.ProductImage.FileName))
                    {
                        product.ProductImage.CopyTo(fileStream);
                        fileStream.Flush();
                    }
                }

                //save to db
                _context.Products.Add(new Product
                {
                    ProductDescription = product.ProductDescription,
                    ProductName = product.ProductName,
                    ProductPrice = product.ProductPrice,
                    ProductUrl = product.ProductImage.FileName,
                    Username = username
                });
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetProduct", new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                throw;
            }                        
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
