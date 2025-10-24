// CMC.Infrastructure/Services/BasicEmailTemplateRenderer.cs
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CMC.Application.Ports.Mail;
using Microsoft.Extensions.Options;

namespace CMC.Infrastructure.Services;

public sealed class BasicEmailTemplateRenderer : IEmailTemplateRenderer
{
    private readonly string? _baseUrl;

    public BasicEmailTemplateRenderer(IOptions<GraphMailOptions> mailOptions)
    {
        _baseUrl = mailOptions?.Value?.PublicBaseUrl;
    }

    public Task<(string Subject, string Html, string? Text)> RenderEmailAsync(
        string subject,
        string text,
        IReadOnlyList<EmailButton> buttons,
        string? baseUrl = null)
    {
        static bool IsAbsolute(string? href) =>
            !string.IsNullOrWhiteSpace(href) && Uri.TryCreate(href, UriKind.Absolute, out _);

        string PrefixBaseUrl(string? href)
        {
            if (string.IsNullOrWhiteSpace(href)) return string.Empty;
            if (string.IsNullOrWhiteSpace(baseUrl)) return href!;
            return $"{baseUrl!.TrimEnd('/')}/{href!.TrimStart('/')}";
        }

        string HtmlEncode(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

        // --- Buttons direkt typisiert rendern ---
        var buttonsInner = new StringBuilder();
        if (buttons is not null)
        {
            foreach (var b in buttons)
            {
                var label = string.IsNullOrWhiteSpace(b.Label) ? "Link" : b.Label;
                var href  = PrefixBaseUrl(string.IsNullOrWhiteSpace(b.Href) ? "#" : b.Href);

                buttonsInner
                    .Append("<table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">")
                    .Append("<tbody><tr>")
                    .Append("<td style=\"font-family: Helvetica, sans-serif; font-size: 16px; vertical-align: top; border-radius: 4px; text-align: center; background-color: #00609c;\" valign=\"top\" align=\"center\" bgcolor=\"#00609c\">")
                    .Append($"<a href=\"{HtmlEncode(href)}\" target=\"_blank\" style=\"border: solid 2px #00609c; border-radius: 4px; box-sizing: border-box; cursor: pointer; display: inline-block; font-size: 16px; font-weight: bold; margin: 0; padding: 12px 24px; text-decoration: none; text-transform: capitalize; background-color: #00609c; border-color: #00609c; color: #ffffff;\">{HtmlEncode(label)}</a> ")
                    .Append("</td>")
                    .Append("</tr></tbody></table>");
            }
        }

        // --- dein HTML, Button-Block dynamisch ---
        var html = new StringBuilder()
            .Append("<!doctype html><html lang=\"en\"><head>")
            .Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">")
            .Append("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">")
            .Append("<title>Simple Transactional Email</title>")
            .Append("<style media=\"all\" type=\"text/css\">@media all {.btn-primary table td:hover {background-color: #004672ff !important;}.btn-primary a:hover {background-color: #004672ff !important;border-color: #004672ff !important;}}@media only screen and (max-width: 640px) {.main p,.main td,.main span {font-size: 16px !important;}.wrapper {padding: 8px !important;}.content {padding: 0 !important;}.container {padding: 0 !important;padding-top: 8px !important;width: 100% !important;}.main {border-left-width: 0 !important;border-radius: 0 !important;border-right-width: 0 !important;}.btn table {max-width: 100% !important;width: 100% !important;}.btn a {font-size: 16px !important;max-width: 100% !important;width: 100% !important;}}@media all {.ExternalClass {width: 100%;}.ExternalClass,.ExternalClass p,.ExternalClass span,.ExternalClass font,.ExternalClass td,.ExternalClass div {line-height: 100%;}.apple-link a {color: inherit !important;font-family: inherit !important;font-size: inherit !important;font-weight: inherit !important;line-height: inherit !important;text-decoration: none !important;}#MessageViewBody a {color: inherit;text-decoration: none;font-size: inherit;font-family: inherit;font-weight: inherit;line-height: inherit;}}</style>")
            .Append("</head><body style=\"font-family: Helvetica, sans-serif; -webkit-font-smoothing: antialiased; font-size: 16px; line-height: 1.3; -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%; background-color: #f4f5f6; margin: 0; padding: 0;\">")
            .Append("<table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"body\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #f4f5f6; width: 100%;\" width=\"100%\" bgcolor=\"#f4f5f6\"><tr>")
            .Append("<td style=\"font-family: Helvetica, sans-serif; font-size: 16px; vertical-align: top;\" valign=\"top\">&nbsp;</td>")
            .Append("<td class=\"container\" style=\"font-family: Helvetica, sans-serif; font-size: 16px; vertical-align: top; max-width: 600px; padding: 0; padding-top: 24px; width: 600px; margin: 0 auto;\" width=\"600\" valign=\"top\">")
            .Append("<div class=\"content\" style=\"box-sizing: border-box; display: block; margin: 0 auto; max-width: 600px; padding: 0;\">")
            .Append("<span class=\"preheader\" style=\"color: transparent; display: none; height: 0; max-height: 0; max-width: 0; opacity: 0; overflow: hidden; mso-hide: all; visibility: hidden; width: 0;\">This is preheader text. Some clients will show this text as a preview.</span>")
            .Append("<table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"main\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; background: #ffffff; border: 1px solid #eaebed; border-radius: 16px; width: 100%;\" width=\"100%\"><tr>")
            .Append("<td class=\"wrapper\" style=\"font-family: Helvetica, sans-serif; font-size: 16px; vertical-align: top; box-sizing: border-box; padding: 24px;\" valign=\"top\">")
            .Append("<span style=\"font-family: Helvetica, sans-serif; font-size: 20px; font-weight: bold; margin: 0; margin-bottom: 16px;\"><img width=\"150px\" src=\"https://examplecompany.de/hubfs/Logo_250px.png\"/></span>")
            .Append("<p style=\"font-family: Helvetica, sans-serif; font-size: 20px; font-weight: bold; margin: 0; margin-bottom: 16px;\">" + WebUtility.HtmlEncode(subject) + "</p>")
            .Append("<p style=\"font-family: Helvetica, sans-serif; font-size: 16px; font-weight: normal; margin: 0; margin-bottom: 16px;\">" + WebUtility.HtmlEncode(text) + "</p>")
            .Append("<table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"btn btn-primary\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; box-sizing: border-box; width: 100%; min-width: 100%;\" width=\"100%\"><tbody><tr>")
            .Append("<td align=\"left\" style=\"font-family: Helvetica, sans-serif; font-size: 16px; vertical-align: top; padding-bottom: 16px;\" valign=\"top\">")
            .Append(buttonsInner.ToString())
            .Append("</td></tr></tbody></table>")
            .Append("</td></tr></table>")
            .Append("<div class=\"footer\" style=\"clear: both; padding-top: 24px; text-align: center; width: 100%;\">")
            .Append("<table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\" width=\"100%\"><tr>")
            .Append("<td class=\"content-block\" style=\"font-family: Helvetica, sans-serif; vertical-align: top; color: #9a9ea6; font-size: 16px; text-align: center;\" valign=\"top\" align=\"center\">")
            .Append("<span class=\"apple-link\" style=\"color: #9a9ea6; font-size: 16px; text-align: center;\">examplecompany : Alexander Welsch</span><br>Auf dem Daubmann 6, 75045 Walzbachtal")
            .Append("</td></tr><tr>")
            .Append("<td class=\"content-block powered-by\" style=\"font-family: Helvetica, sans-serif; vertical-align: top; color: #9a9ea6; font-size: 16px; text-align: center;\" valign=\"top\" align=\"center\">")
            .Append("Automatische E-Mail von cmc.examplecompany.de")
            .Append("</td></tr></table>")
            .Append("</div></div>")
            .Append("</td><td style=\"font-family: Helvetica, sans-serif; font-size: 16px; vertical-align: top;\" valign=\"top\">&nbsp;</td>")
            .Append("</tr></table></body></html>")
            .ToString();

        return Task.FromResult((subject, html, text));
    }
}
