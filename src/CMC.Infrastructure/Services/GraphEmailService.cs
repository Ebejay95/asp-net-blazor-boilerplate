using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using CMC.Application.Ports;
using CMC.Application.Ports.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;

namespace CMC.Infrastructure.Services;

public sealed class GraphEmailService : IEmailService
{
    private readonly GraphMailOptions _options;
    private readonly IEmailTemplateRenderer _renderer;
    private readonly ILogger<GraphEmailService> _logger;
    private readonly GraphServiceClient _graph;

    public GraphEmailService(
        IOptions<GraphMailOptions> options,
        IEmailTemplateRenderer renderer,
        ILogger<GraphEmailService> logger)
    {
        _options  = options.Value ?? throw new ArgumentNullException(nameof(options));
        _renderer = renderer;
        _logger   = logger;

        if (string.IsNullOrWhiteSpace(_options.TenantId))     throw new InvalidOperationException("GRAPH_TENANT_ID fehlt.");
        if (string.IsNullOrWhiteSpace(_options.ClientId))     throw new InvalidOperationException("GRAPH_CLIENT_ID fehlt.");
        if (string.IsNullOrWhiteSpace(_options.ClientSecret)) throw new InvalidOperationException("GRAPH_CLIENT_SECRET fehlt.");
        if (string.IsNullOrWhiteSpace(_options.FromUser))     throw new InvalidOperationException("GRAPH_FROM_USER fehlt.");

        // Application Flow (Client Credentials) – nutzt .default Scope (App Permissions)
        var credential = new ClientSecretCredential(_options.TenantId, _options.ClientId, _options.ClientSecret);
        _graph = new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });
    }

    public async Task SendEmailAsync(string email, string subject, string text, string links)
    {
            //var baseUrl = (_options.PublicBaseUrl ?? "").TrimEnd('/');
            //var link = string.IsNullOrWhiteSpace(baseUrl)
            //    ? $"/reset-password?token={Uri.EscapeDataString(resetToken)}"
            //    : $"{baseUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";

        // liefert (Subject, Html, Text)
        var (renderedSubject, renderedHtml, renderedText) = await _renderer.RenderEmailAsync(subject, text, links);

        // Verwende gerenderte Werte; fallback auf Input falls null
        await SendAsync(
            email,
            renderedSubject ?? subject,
            renderedHtml ?? string.Empty,
            renderedText ?? text
        );
    }


    private async Task SendAsync(string to, string subject, string htmlBody, string? textBody)
    {
        // Graph unterstützt eine Body-Variante pro Nachricht (HTML **oder** Text).
        // Wir senden HTML. (Wenn du Plaintext brauchst, setze ContentType=Text
        // oder rendere eine sehr schlichte HTML-Variante.)
        var message = new Message
        {
            Subject = subject,
            Body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = htmlBody ?? string.Empty
            },
            ToRecipients = new List<Recipient>
            {
                new Recipient
                {
                    EmailAddress = new EmailAddress { Address = to }
                }
            }
        };


        // Optional: Anzeigename/ReplyTo „simulieren“ – wirkt je nach Tenant-Policy/Client
        if (!string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            message.ReplyTo = new List<Recipient> {
                new Recipient {
                    EmailAddress = new EmailAddress {
                        Address = _options.FromUser, // oder gewünschte Reply-Adresse
                        Name    = _options.FromName
                    }
                }
            };
            message.Sender = message.From;
        }

        try
        {
            var body = new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
            {
                Message = message,
                SaveToSentItems = true
            };

            await _graph.Users[_options.FromUser].SendMail.PostAsync(body);

            _logger.LogInformation("📧 Mail via Graph an {To} gesendet (FromUser {FromUser})", to, _options.FromUser);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex,
                "❌ Graph SendMail ApiException: Status={Status}, Message={Message}",
                ex.ResponseStatusCode, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Graph SendMail Fehler an {To}", to);
            throw;
        }
    }
}
