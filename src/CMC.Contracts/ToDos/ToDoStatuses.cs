using System;
using System.Collections.Generic;

namespace CMC.Contracts.ToDos
{
    /// <summary>
    /// UI-Optionsquelle für ToDo-Status (Tags), gemappt auf Domain-Enum via ToDoStatusExtensions.
    /// </summary>
    public static class ToDoStatuses
    {
        public static readonly IReadOnlyList<(string Label, string Tag)> Statuses = new (string, string)[]
        {
            ("To Do",       "todo"),
            ("In Progress", "in_progress"),
            ("Done",        "done"),
            ("Blocked",     "blocked"),
            ("Canceled",    "canceled"),
        };
    }
}
