using Microsoft.AspNetCore.Mvc;
using EshopCrud.Services;
using System.Threading.Tasks;

public class ContactController : Controller
{
    private readonly EmailService _emailService;

    public ContactController(EmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(string name, string email, string subject, string message)
    {
        if (ModelState.IsValid)
        {
            // Skompletizuj obsah správy
            string fullMessage = $"Meno: {name}\nEmail: {email}\nSpráva:\n{message}";

            // Odoslanie emailu na tvoj email
            await _emailService.SendEmailAsync("testovaciemailzborky@gmail.com", subject, fullMessage);

            // Zobraz správu o úspešnom odoslaní
            return Json(new { success = true, message = "Správa bola úspešne odoslaná!" });
        }
        else
        {
            return Json(new { success = false, message = "Prosím, skontrolujte zadané údaje." });
        }
    }
}
