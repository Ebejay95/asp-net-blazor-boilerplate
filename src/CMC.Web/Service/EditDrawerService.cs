using System.ComponentModel.DataAnnotations;
using System.Reflection;
using CMC.Web.Shared;
using CMC.Web.Util;

namespace CMC.Web.Services;

public sealed class EditDrawerRequest
{
	public required string Title { get; init; }
	public required object Model { get; init; }
	public required Assembly ContractsAssembly { get; init; }

	// Aufrufe bekommen den Builder-Kontext
	public Func<RequestBuildContext, Task>? OnSave { get; init; }
	public Func<RequestBuildContext, Task>? OnDelete { get; init; }
}


public class EditDrawerService {
  public event Action<EditDrawerRequest>
    ? OnOpen;
  public event Action? OnClose;

  public void Open(EditDrawerRequest req) => OnOpen
    ?.Invoke(req);
  public void Close() => OnClose
    ?.Invoke();
}
