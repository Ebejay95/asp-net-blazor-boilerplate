using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CMC.Application.Ports;

namespace CMC.Infrastructure.Services;

/// <summary>
/// File-basierter E-Mail-Renderer mit Kultur-Fallback und Front-Matter.
/// Suchpfade:
///  - Konfig: configuration["EmailTemplates:Root"]
///  - AppContext.BaseDirectory (Publish/Container)
///  - Directory.GetCurrentDirectory()
///  - Projektwurzel (heuristisch aus /bin/... ermittelt)
/// Kandidaten: key.{culture}.html/txt -> key.{lang}.html/txt -> key.html/txt
/// Platzhalter: {{Name}} (aus anonyme/normale Models, verschachtelt)
/// Front-Matter:
/// ---
/// subject: Dein Betreff
/// ---
public sealed class FileEmailTemplateRenderer : IEmailTemplateRenderer
{
    private readonly ILogger<FileEmailTemplateRenderer> _log;
    private readonly string? _customRoot;

    private static readonly Regex FrontMatterRx = new(@"^\s*---\s*\r?\n(?<fm>[\s\S]*?)\r?\n---\s*\r?\n?", RegexOptions.Compiled);
    private static readonly Regex SubjectLineRx = new(@"^\s*subject\s*:\s*(?<s>.+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex PlaceholderRx = new(@"{{\s*([A-Za-z0-9_\.]+)\s*}}", RegexOptions.Compiled);

    public FileEmailTemplateRenderer(IConfiguration cfg, ILogger<FileEmailTemplateRenderer> log)
    {
        _log = log;
        _customRoot = cfg["EmailTemplates:Root"];
    }

    public async Task<(string Subject, string Html, string? Text)> RenderAsync<TModel>(
        string templateKey, TModel model, string? culture = null, CancellationToken ct = default)
    {
        culture ??= CultureInfo.CurrentUICulture.Name;
        var lang = new CultureInfo(culture).TwoLetterISOLanguageName;

        var htmlCandidates = Candidates(templateKey, "html", culture, lang);
        var txtCandidates  = Candidates(templateKey, "txt",  culture, lang);

        var htmlPath = FindFirstExisting(htmlCandidates);
        var txtPath  = FindFirstExisting(txtCandidates);

        if (htmlPath is null && txtPath is null)
            throw new FileNotFoundException($"No email template files found for key '{templateKey}'.");

        var subject = "(no subject)";
        string? html = null;
        string? text = null;

        if (htmlPath is not null)
        {
            var raw = await File.ReadAllTextAsync(htmlPath, Encoding.UTF8, ct);
            (subject, html) = ExtractSubjectAndBody(raw, subject);
        }

        if (txtPath is not null)
        {
            var raw = await File.ReadAllTextAsync(txtPath, Encoding.UTF8, ct);
            (subject, text) = ExtractSubjectAndBody(raw, subject);
        }

        var map = Flatten(model);
        map.TryAdd("Now", DateTimeOffset.Now.ToString("u"));
        map.TryAdd("NowUtc", DateTimeOffset.UtcNow.ToString("u"));

        subject = ReplacePlaceholders(subject, map);
        if (html is not null) html = ReplacePlaceholders(html, map);
        if (text is not null) text = ReplacePlaceholders(text, map);

        _log.LogDebug("Rendered template '{Key}' (html:{HasHtml}, text:{HasText})",
            templateKey, html is not null, text is not null);

        return (subject, html ?? string.Empty, text);
    }

    private static (string Subject, string Body) ExtractSubjectAndBody(string raw, string fallbackSubject)
    {
        var m = FrontMatterRx.Match(raw);
        if (m.Success)
        {
            var fm = m.Groups["fm"].Value;
            var sm = SubjectLineRx.Match(fm);
            var subj = sm.Success ? sm.Groups["s"].Value.Trim() : fallbackSubject;
            var body = raw[(m.Index + m.Length)..];
            return (subj, body);
        }

        if (raw.StartsWith("Subject:", StringComparison.OrdinalIgnoreCase))
        {
            var lines = raw.Replace("\r\n", "\n").Split('\n');
            var subj = lines[0]["Subject:".Length..].Trim();
            var body = string.Join('\n', lines.Skip(1)).TrimStart();
            return (string.IsNullOrWhiteSpace(subj) ? fallbackSubject : subj, body);
        }

        return (fallbackSubject, raw);
    }

    private static string ReplacePlaceholders(string input, Dictionary<string, string> map)
        => PlaceholderRx.Replace(input, m =>
        {
            var key = m.Groups[1].Value;
            if (TryGet(map, key, out var val)) return val ?? "";
            return "";
        });

    private static bool TryGet(Dictionary<string, string> map, string dotted, out string value)
    {
        if (map.TryGetValue(dotted, out value!)) return true;
        var leaf = dotted.Contains('.') ? dotted.Split('.').Last() : dotted;
        return map.TryGetValue(leaf, out value!);
    }

    private static Dictionary<string, string> Flatten<T>(T obj)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); // ⬅️ expliziter Typ
        if (obj is null) return dict;

        foreach (var (k, v) in Walk(obj, null)) dict[k] = v ?? "";
        return dict;

        static IEnumerable<(string Key, string? Val)> Walk(object o, string? prefix)
        {
            if (o is string s) { yield return (prefix ?? "Value", s); yield break; }
            foreach (var p in o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var name = string.IsNullOrWhiteSpace(prefix) ? p.Name : $"{prefix}.{p.Name}";
                var val = p.GetValue(o, null);
                if (val is null) { yield return (name, null); continue; }
                if (val is string sv) { yield return (name, sv); continue; }
                if (val is IFormattable f) { yield return (name, f.ToString(null, CultureInfo.InvariantCulture)); continue; }
                foreach (var inner in Walk(val, name)) yield return inner;
            }
        }
    }

    private static IEnumerable<string> Candidates(string key, string ext, string culture, string lang)
    {
        yield return $"{key}.{culture}.{ext}";
        yield return $"{key}.{lang}.{ext}";
        yield return $"{key}.{ext}";
    }

    private string? FindFirstExisting(IEnumerable<string> names)
    {
        foreach (var n in names)
        {
            foreach (var root in CandidateRoots())
            {
                var p1 = Path.Combine(root, "Services", "EmailTemplates", n);
                if (File.Exists(p1)) return p1;

                var p2 = Path.Combine(root, "EmailTemplates", n);
                if (File.Exists(p2)) return p2;
            }
        }
        return null;
    }

    private IEnumerable<string> CandidateRoots()
    {
        if (!string.IsNullOrWhiteSpace(_customRoot))
            yield return _customRoot!;

        var baseDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        yield return baseDir;

        yield return Directory.GetCurrentDirectory();

        // Kein yield im try/catch (C#-Regel) -> Pfad berechnen, dann außerhalb ausgeben
        string? projRoot = null;
        try
        {
            var di = new DirectoryInfo(baseDir);
            if (di.FullName.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            {
                var up = di.Parent?.Parent?.Parent; // bin/<cfg>/<tfm>
                if (up != null && up.Exists) projRoot = up.FullName;
            }
        }
        catch { /* ignore */ }

        if (!string.IsNullOrWhiteSpace(projRoot))
            yield return projRoot!;
    }
}
