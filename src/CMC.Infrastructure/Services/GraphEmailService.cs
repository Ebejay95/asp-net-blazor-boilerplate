// CMC.Infrastructure/Services/GraphEmailService.cs
using System;
using System.Collections.Generic;
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

        var credential = new ClientSecretCredential(_options.TenantId, _options.ClientId, _options.ClientSecret);
        _graph = new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });
    }

    public async Task SendEmailAsync(
        string email,
        string subject,
        string text,
        IReadOnlyList<EmailButton> buttons,
        string? baseUrl = null)
    {
        var baseUri = new Uri(Nav.BaseUri).GetLeftPart(UriPartial.Authority);
        var (renderedSubject, renderedHtml, renderedText) =
            await _renderer.RenderEmailAsync(
                subject,
                text,
                buttons,
                baseUrl ?? baseUri
            );

        await SendAsync(
            email,
            renderedSubject ?? subject,
            renderedHtml ?? string.Empty,
            renderedText ?? text
        );
    }

    private async Task SendAsync(string to, string subject, string htmlBody, string? textBody)
    {
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
                new Recipient { EmailAddress = new EmailAddress { Address = to } }
            }
        };

        if (!string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            message.ReplyTo = new List<Recipient> {
                new Recipient {
                    EmailAddress = new EmailAddress {
                        Address = _options.FromUser,
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
            _logger.LogError(ex, "❌ Graph SendMail ApiException: Status={Status}, Message={Message}", ex.ResponseStatusCode, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Graph SendMail Fehler an {To}", to);
            throw;
        }
    }
}
