using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace NexaLearn.Services;

public class CorreoService : ICorreoService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public CorreoService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public async Task EnviarAsync(string destino, string asunto, string contenido, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destino);
        ArgumentException.ThrowIfNullOrWhiteSpace(asunto);
        ArgumentException.ThrowIfNullOrWhiteSpace(contenido);
        cancellationToken.ThrowIfCancellationRequested();

        var smtp = _configuration.GetSection("Smtp");
        if (string.IsNullOrWhiteSpace(smtp["Host"]) || string.IsNullOrWhiteSpace(smtp["Remitente"]))
            throw new InvalidOperationException("Configura Smtp:Host y Smtp:Remitente para enviar correos.");

        var ruta = Path.Combine(_environment.ContentRootPath, "Templates", "Correo.html");
        var plantilla = await File.ReadAllTextAsync(ruta, cancellationToken);
        var html = string.Format(CultureInfo.InvariantCulture, plantilla,
            WebUtility.HtmlEncode(asunto), WebUtility.HtmlEncode(contenido).ReplaceLineEndings("<br />"));

        using var mensaje = new MailMessage
        {
            From = new MailAddress(smtp["Remitente"]!, "NexaLearn"),
            Subject = asunto,
            Body = html,
            IsBodyHtml = true,
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8
        };
        mensaje.To.Add(new MailAddress(destino));

        using var cliente = new SmtpClient(smtp["Host"], smtp.GetValue("Puerto", 587))
        {
            EnableSsl = smtp.GetValue("UsarSsl", true),
            UseDefaultCredentials = false
        };
        if (!string.IsNullOrWhiteSpace(smtp["Usuario"]))
            cliente.Credentials = new NetworkCredential(smtp["Usuario"], smtp["Clave"]);

        await cliente.SendMailAsync(mensaje, cancellationToken);
    }
}
