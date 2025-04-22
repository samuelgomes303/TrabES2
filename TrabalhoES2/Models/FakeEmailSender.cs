namespace TrabalhoES2.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

public class FakeEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Console.WriteLine($"[FakeEmailSender] To: {email}, Subject: {subject}, Message: {htmlMessage}");
        return Task.CompletedTask;
    }
}
