using System;
using System.Collections.Generic;

namespace CMC.Web.Services;

/// <summary>
/// Zusätzliche, „manuelle“ Felder für EFEdit/FormRenderer (z. B. Passwort, Confirm, Selects).
/// </summary>
public sealed class ExtraField
{
    // Core
    public string Name { get; init; }
    public Type   Type { get; init; }

    // UI
    public string? Label { get; init; }
    public string? Hint  { get; init; }
    public bool    ReadOnly { get; init; }
    public bool    Required { get; init; }  // zur Anzeige/Markierung im UI

    /// <summary>
    /// Spezielle Feld-Typen:
    ///  - "password": TextField mit type="password"
    ///  - "relation-auto": RelationSelect/CheckboxList basierend auf EfParentType+Name
    ///  - null/leer: Standard-Mapping anhand Type/Options
    /// </summary>
    public string? DataType { get; init; }

    /// <summary>Nur für Select-Felder: [Label, Value]</summary>
    public List<KeyValuePair<string, string>>? Options { get; init; }

    /// <summary>Startwert / gebundener Wert (z. B. bei Edit oder Defaults)</summary>
    public object? Value { get; set; }

    /// <summary>Minimaler Ctor (kompatibel zu bestehendem Code).</summary>
    public ExtraField(string name, Type type)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type  ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>
    /// Komfort-Ctor, damit Aufrufe mit benannten Argumenten wie
    /// <c>new ExtraField(Name: "Password", Label: "...", Type: typeof(string), ...)</c>
    /// ohne Änderungen weiter funktionieren.
    /// </summary>
    public ExtraField(
        string Name,
        string? Label,
        Type Type,
        bool ReadOnly = false,
        string? Hint = null,
        string? DataType = null,
        List<KeyValuePair<string, string>>? Options = null,
        bool Required = false,
        object? Value = null)
    {
        this.Name = Name ?? throw new ArgumentNullException(nameof(Name));
        this.Type = Type ?? throw new ArgumentNullException(nameof(Type));
        this.Label = Label;
        this.ReadOnly = ReadOnly;
        this.Hint = Hint;
        this.DataType = DataType;
        this.Options = Options;
        this.Required = Required;
        this.Value = Value;
    }
}
