using System.Reflection;
using CMC.Web.Util;

namespace CMC.Web.Shared;

public sealed class RequestBuildContext
{
	public object Model { get; }
	public Assembly ContractsAssembly { get; }
	public IValueProvider Provider { get; }
	public string Action { get; }

	public RequestBuildContext(object model, Assembly asm, IValueProvider provider, string action = "Update")
	{
		Model = model;
		ContractsAssembly = asm;
		Provider = provider;
		Action = action;
	}

	public object Build(string? action = null)
	{
		var act = action ?? Action;
		return RequestFactory.MapByProvider(Model, Provider, act, ContractsAssembly);
	}
}
