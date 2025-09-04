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
}
