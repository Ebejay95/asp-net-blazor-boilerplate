namespace CMC.Contracts.Common
{
    /// <summary>
    /// Versteckt eine Property im Editor (unabhängig von Display/Scaffold).
    /// Für Tabellen/Lesen kann sie weiterhin genutzt werden.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class EditorHiddenAttribute : System.Attribute { }

    /// <summary>
    /// Blendet eine Property im Editor aus, WENN eine angegebene andere Property existiert.
    /// Beispiel: [EditorHideIfExists(nameof(CustomerId))] auf CustomerName.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public sealed class EditorHideIfExistsAttribute : System.Attribute
    {
        public string OtherProperty { get; }
        public EditorHideIfExistsAttribute(string otherProperty)
            => OtherProperty = otherProperty ?? string.Empty;
    }

    /// <summary>
    /// Layoutvorgaben für Editor-Form: Spaltenbreite je Breakpoint und Reihenfolge.
    /// Werte sind Tailwind-Grid-Spalten (1..12); null = Default.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class EditorLayoutAttribute : System.Attribute
    {
        public int? ColXs { get; init; } = 12;
        public int? ColSm { get; init; }
        public int? ColMd { get; init; }
        public int? ColLg { get; init; }
        public int? Order { get; init; }

        public EditorLayoutAttribute() { }

        public EditorLayoutAttribute(int colXs, int? colSm = null, int? colMd = null, int? colLg = null, int? order = null)
        {
            ColXs = colXs; ColSm = colSm; ColMd = colMd; ColLg = colLg; Order = order;
        }
    }
}
