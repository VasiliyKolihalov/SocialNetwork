using MailKit.Net.Smtp;
using MimeKit;

namespace SocialNetwork.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private const string Host = "smtp.gmail.com";
    private const int Port = 587;
    
    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public void SendEmail(string email, string subject, string message)
    {
        var emailMessage = new MimeMessage();

        emailMessage.From.Add(new MailboxAddress(_configuration["CompanyData:Name"], _configuration["CompanyData:Email"]));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = message
        };

        using (var client = new SmtpClient())
        {
            client.Connect(Host, Port, false);
            client.Authenticate(_configuration["CompanyData:Email"], _configuration["CompanyData:EmailPassword"]);
            client.Send(emailMessage);

            client.Disconnect(true);
        }
    }
}