using System.Collections.Concurrent;
using System.Reflection; // <- This was missing
using CMC.Web.Shared;

namespace CMC.Web.Services;
public sealed class EditDrawerRequest
{
    public required string Title { get; init; }
    public required object Model { get; init; }
    public required Assembly ContractsAssembly { get; init; }
    public bool IsCreate { get; init; } = false;
    public string Action { get; init; } = "Update";

    public Func<RequestBuildContext, Task>? OnSave { get; init; }
    public Func<RequestBuildContext, Task>? OnDelete { get; init; }

    public Func<RequestBuildContext, Task>? OnBeforeSave { get; set; }
    public Func<RequestBuildContext, Task>? OnAfterSave  { get; set; }

    public List<ExtraField> ExtraFields { get; } = new();

    // NEU: generische Relationen
    public List<RelationDescriptor> Relations { get; } = new();
}
public sealed class EditDrawerService
{
	public event Action? StackChanged;

	public event Action<EditDrawerRequest>? OpenRequested;
	public event Action? CloseRequested;
	private readonly List<Frame> _stack = new();
	private record Frame(EditDrawerRequest Request, TaskCompletionSource<object?>? Result);

	public IReadOnlyList<EditDrawerRequest> Stack => _stack.Select(f => f.Request).ToList();

		// Open anpassen
	public void Open(EditDrawerRequest request)
	{
		Push(new Frame(request, null));
		OpenRequested?.Invoke(request);
	}


	public Task<T?> OpenForResult<T>(EditDrawerRequest request)
	{
		var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
		Push(new Frame(request, tcs));
		return tcs.Task.ContinueWith(t => (T?)t.Result);
	}

	// Close anpassen
	public void Close(object? result = null)
	{
		if (_stack.Count == 0) return;
		var top = _stack[^1];
		_stack.RemoveAt(_stack.Count - 1);
		top.Result?.TrySetResult(result);
		StackChanged?.Invoke();
		CloseRequested?.Invoke();
	}

	private void Push(Frame frame)
	{
		_stack.Add(frame);
		StackChanged?.Invoke();
	}
}
