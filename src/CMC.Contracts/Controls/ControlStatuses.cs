using System;
using System.Collections.Generic;

namespace CMC.Contracts.Controls
{
    /// <summary>
    /// Zentrale Options-Quelle für Control-Status (Label + Tag).
    /// Tags müssen zu CMC.Domain.Entities.Control.ValidStatuses passen.
    /// </summary>
    public static class ControlStatuses
    {
        public static readonly IReadOnlyList<(string Label, string Tag)> Statuses = new (string, string)[]
        {
            ("Proposed",    "proposed"),
            ("Planned",     "planned"),
            ("In Progress", "in_progress"),
            ("Blocked",     "blocked"),
            ("Active",      "active"),
            ("Retired",     "retired"),
        };
    }
}
