using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EshopCrud.Data;
using EshopCrud.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EshopCrud.Services;

namespace EshopCrud.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        // Konštruktor pre injekciu závislostí AppDbContext a EmailService
        public OrderController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Metóda na vytvorenie objednávky
        [HttpPost]
        [HttpPost]
public async Task<ActionResult<Order>> CreateOrder([FromBody] OrderRequest orderRequest)
{
    // Overenie, či objednávka alebo jej položky nie sú null alebo prázdne
    if (orderRequest == null || orderRequest.Products == null || !orderRequest.Products.Any())
    {
        return BadRequest("Order request or order items cannot be null or empty.");
    }

    // Vytvorenie objektu objednávky a výpočet celkovej ceny
    var order = new Order
    {
        CustomerName = orderRequest.CustomerName,
        CustomerEmail = orderRequest.CustomerEmail,
        CustomerPhone = orderRequest.CustomerPhone,
        Total = orderRequest.Products.Sum(p => (decimal)p.Price * p.Quantity),
        Street = orderRequest.Street,
        City = orderRequest.City,
        PostalCode = orderRequest.PostalCode,
        Country = orderRequest.Country,
        OrderDate = DateTime.Now,

        OrderItems = new List<OrderItem>()
    };

    // Pre každý produkt v objednávke nájdi produkt v databáze
    foreach (var item in orderRequest.Products)
    {
        var product = await _context.Products.FindAsync(item.Id);
        if (product == null)
        {
            return NotFound($"Product with ID {item.Id} not found.");
        }

        // Vytvorenie položky objednávky a pridanie do zoznamu položiek
        var orderItem = new OrderItem
        {
            Order = order,
            ProductId = item.Id,
            Quantity = item.Quantity,
            Price = (decimal)item.Price,
            Product = product
        };

        order.OrderItems.Add(orderItem);

        // Odpočítaj množstvo produktu
        product.Quantity -= item.Quantity;
    }

    // Pridanie objednávky do databázy a uloženie zmien
    _context.Orders.Add(order);
    await _context.SaveChangesAsync();

    // Generovanie PDF pre objednávku
    var pdfPath = Path.Combine("C:/Users/Jakub/Desktop/Programovanie/C#/EshopFinalAndrej/pdf", $"Detaily_objednavky.pdf");
    var pdfGenerator = new PdfGenerator();
    pdfGenerator.GeneratePdf(pdfPath, order);

    // Po úspešnom uložení objednávky sa odošle e-mail s potvrdením
    var subject = "Potvrdenie objednavky";
    var message = $"Vazeny {order.CustomerName},\n\n" +
      $"Ďakujeme za vašu objednávku. Vaše objednávkové ID je {order.Id}.\n\n" +
      $"Detaily objednávky:\n" +
      $"{string.Join("\n", order.OrderItems.Select(item => $"Produkt: {item.Product?.Name ?? "Neznámy"}, Množstvo: {item.Quantity}, Cena: {item.Price:C}"))}\n\n" +
      $"Celková cena: {order.Total:C}\n\n" +
      $"Adresa na doručenie:\n" +
      $"{order.Street}\n{order.City}, {order.PostalCode}\n{order.Country}\n\n" +
      $"S pozdravom,\nVaše EqBarbers";

    // Pokus o odoslanie e-mailu, chyby sa zachytia a zalogujú
    try
    {
        await _emailService.SendEmailAsync(order.CustomerEmail, subject, message, pdfPath);
    }
    catch (Exception ex)
    {
        // Logovanie chyby pri odosielaní e-mailu
        Console.WriteLine($"Failed to send email: {ex.Message}");
    }

    // Vráti sa odpoveď so stavom Created a obsahuje ID objednávky
    return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new { orderId = order.Id });
}

        // Metóda na získanie objednávky podľa ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            // Nájde objednávku vrátane jej položiek a príslušných produktov
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            // Ak objednávka neexistuje, vráti sa 404 Not Found
            if (order == null)
            {
                return NotFound();
            }

            // Vráti objednávku s kódom 200 OK
            return Ok(order);
        }

        // Metóda na získanie zoznamu všetkých objednávok
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            // Získa všetky objednávky vrátane ich položiek a produktov
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();

            // Vráti zoznam objednávok
            return Ok(orders);
        }
    }

    // Trieda pre požiadavku na vytvorenie objednávky
    public class OrderRequest
    {
        public required string CustomerName { get; set; }
        public required string CustomerEmail { get; set; }
        public required string Street { get; set; }
        public required string City { get; set; }
        public required string PostalCode { get; set; }
        public required string Country { get; set; }

        public required int CustomerPhone { get; set; }
        public required List<OrderItemRequest> Products { get; set; }
        public string? message { get; set; } 
    }

    // Trieda pre položku objednávky v požiadavke
    public class OrderItemRequest
    {
        public int Id { get; set; } // ID produktu
        public int Quantity { get; set; } // Množstvo objednaného produktu
        public decimal Price { get; set; } // Cena za kus produktu
    }
}
