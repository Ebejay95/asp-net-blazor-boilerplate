using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CMC.Web.Shared;

namespace CMC.Web.Services;

public sealed class EditDrawerRequest
{
    /// <summary>Titel im Drawer.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Das anzuzeigende/zu bearbeitende Modell (typisch DTO).</summary>
    public object Model { get; set; } = default!;

    /// <summary>Assembly, in der die Request-Typen liegen (für ctx.Build("Create"/"Update")).</summary>
    public Assembly ContractsAssembly { get; set; } = typeof(string).Assembly;

    /// <summary>Erstellmodus (steuert Felder wie Passwort etc.).</summary>
    public bool IsCreate { get; set; }

    /// <summary>Zusätzliche, nicht aus dem DTO abgeleitete Felder (Relationen, Passwort, ...).</summary>
    public List<ExtraField> ExtraFields { get; } = new();

    /// <summary>Speichern-Callback (wird von EditDrawer aufgerufen).</summary>
    public Func<RequestBuildContext, Task>? OnSave { get; set; }

    /// <summary>Löschen-Callback (wird von EditDrawer aufgerufen).</summary>
    public Func<RequestBuildContext, Task>? OnDelete { get; set; }
}

public sealed class EditDrawerService
{
    public event Action<EditDrawerRequest>? OpenRequested;
    public event Action? CloseRequested;

    public void Open(EditDrawerRequest request)
    {
        Console.WriteLine($"EditDrawerService.Open: {request.Title}");
        OpenRequested?.Invoke(request);
    }

    public void Close()
    {
        Console.WriteLine("EditDrawerService.Close");
        CloseRequested?.Invoke();
    }
}
