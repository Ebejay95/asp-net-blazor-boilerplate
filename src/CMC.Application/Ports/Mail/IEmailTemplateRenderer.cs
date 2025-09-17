namespace CMC.Application.Ports;

public interface IEmailTemplateRenderer
{
    // Liefert (Subject, HtmlBody, TextBody?) aus einem TemplateKey + Modell.
    Task<(string Subject, string Html, string? Text)>
        RenderAsync<TModel>(string templateKey, TModel model, string? culture = null, CancellationToken ct = default);
}
