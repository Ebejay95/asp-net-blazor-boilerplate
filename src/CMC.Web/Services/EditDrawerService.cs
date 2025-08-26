using System.Collections.Concurrent;
using System.Reflection; // <- This was missing
using CMC.Web.Shared;

namespace CMC.Web.Services;
public sealed class EditDrawerRequest
{
	public string Title { get; init; } = "";
	public object Model { get; init; } = default!;
	public Assembly ContractsAssembly { get; init; } = default!;
	public bool IsCreate { get; init; }
	public Func<EditContextAdapter, Task> OnSave { get; init; } = _ => Task.CompletedTask;
	public Func<EditContextAdapter, Task>? OnDelete { get; init; }

	// NEU: Für relation-auto
	public Type? EfParentType { get; init; }          // z.B. typeof(User)
	public Func<object, object>? GetParentKey { get; init; } // z.B. m => ((UserDto)m).Id

	public List<ExtraField> ExtraFields { get; } = new();
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
