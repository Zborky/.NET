using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EshopCrud.Data;
using EshopCrud.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly string _uploadPath;

    public ProductsController(AppDbContext context)
    {
        _context = context;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        // Ensure the upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    // GET: api/products
    // Nacita produkty na stranke
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        try
        {
            // Načíta produkty z databázy a vracia ich s vybranými vlastnosťami
            var products = await _context.Products
                .Select(p => new 
                {
                    p.Id,
                    Name = p.Name ?? string.Empty,
                    Price = p.Price,
                    Description = p.Description ?? string.Empty,
                    Quantity = p.Quantity,
                    ImagePath = p.ImagePath ?? string.Empty
                })
                .ToListAsync();

            // Vracia zoznam produktov ako JSON odpoveď
            return Ok(products);
        }
        catch (Exception ex)
        {
            // Ak sa vyskytne chyba, vracia status 500 s chybovou správou
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error fetching products: {ex.Message}");
        }
    }

    // GET: api/products/5
    // Zobrazi produkt na stranke podľa jeho ID
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        try
        {
            // Načíta produkt z databázy podľa ID
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                // Ak produkt neexistuje, vracia NotFound
                return NotFound($"Product with ID {id} not found.");
            }
            // Vracia produkt ako JSON odpoveď
            return Ok(product);
        }
        catch (Exception ex)
        {
            // V prípade chyby vracia status 500 s chybovou správou
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error fetching product: {ex.Message}");
        }
    }

    // POST: api/products
    // Pridanie nového produktu
    [HttpPost]
    public async Task<ActionResult<Product>> PostProduct([FromForm] Product product, [FromForm] IFormFile? image)
    {
        // Skontroluje, či je model správne vyplnený
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Ak bol odoslaný obrázok, uloží ho na disk
            if (image != null && image.Length > 0)
            {
                product.ImagePath = await SaveFileAsync(image);
            }
            else
            {
                // Ak nie je obrázok, nastaví predvolený
                product.ImagePath = "/uploads/default.jpg";
            }

            // Pridá produkt do databázy a uloží zmeny
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Vracia novovytvorený produkt spolu s jeho ID
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            // V prípade chyby vracia status 500 s chybovou správou
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error adding product: {ex.Message}");
        }
    }

    // PUT: api/products/5
    // Aktualizácia existujúceho produktu
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProduct(int id, [FromForm] Product product, [FromForm] IFormFile? image)
    {
        // Skontroluje, či sa ID zhoduje s ID produktu
        if (id != product.Id)
        {
            return BadRequest("Product ID mismatch.");
        }

        // Skontroluje, či je model správne vyplnený
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Načíta existujúci produkt z databázy
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }

            // Aktualizuje vlastnosti produktu
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.Quantity = product.Quantity;

            // Ak bol odoslaný nový obrázok, vymaže starý a uloží nový
            if (image != null && image.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingProduct.ImagePath) && existingProduct.ImagePath != "/uploads/default.jpg")
                {
                    DeleteFile(existingProduct.ImagePath);
                }

                existingProduct.ImagePath = await SaveFileAsync(image);
            }

            // Označí produkt ako modifikovaný a uloží zmeny v databáze
            _context.Entry(existingProduct).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Vracia status 204 (No Content), čo znamená úspešnú aktualizáciu bez návratu obsahu
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Ak sa produkt nenájde počas aktualizácie, vracia NotFound
            if (!ProductExists(id))
            {
                return NotFound($"Product with ID {id} not found.");
            }
            else
            {
                throw;
            }
        }
        catch (Exception ex)
        {
            // V prípade chyby vracia status 500 s chybovou správou
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating product: {ex.Message}");
        }
    }

    // DELETE: api/products/5
    // Odstránenie produktu
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            // Načíta produkt z databázy
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }

            // Ak má produkt vlastný obrázok, vymaže ho
            if (!string.IsNullOrEmpty(product.ImagePath) && product.ImagePath != "/uploads/default.jpg")
            {
                DeleteFile(product.ImagePath);
            }

            // Odstráni produkt z databázy a uloží zmeny
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            // V prípade chyby vracia status 500 s chybovou správou
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting product: {ex.Message}");
        }
    }

    // Pomocná metóda na overenie, či produkt existuje
    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }

    // Uloží súbor (obrázok produktu) na disk a vráti jeho cestu
    private async Task<string> SaveFileAsync(IFormFile file)
    {
        if (file.Length > 0)
        {
            var filePath = Path.Combine(_uploadPath, file.FileName);

            // Uloží obrázok do súborového systému
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Vráti relatívnu cestu k obrázku
            return "/uploads/" + file.FileName;
        }

        // Ak je súbor prázdny, vyhodí výnimku
        throw new InvalidOperationException("File is empty.");
    }

    // Odstráni súbor z disku
    private void DeleteFile(string path)
    {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", path.TrimStart('/'));

        // Skontroluje, či súbor existuje a vymaže ho
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }
}
