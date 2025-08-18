using System.Reflection;
using CMC.Web.Util;

namespace CMC.Web.Shared;

public sealed class RequestBuildContext
{
	public RequestBuildContext(object model, Assembly contractsAssembly, IValueProvider provider)
	{
		Model = model;
		ContractsAssembly = contractsAssembly;
		Provider = provider;
	}

	public object Model { get; }
	public Assembly ContractsAssembly { get; }
	public IValueProvider Provider { get; }

	public object Build(string action)
		=> RequestFactory.MapByProvider(Model, Provider, action, ContractsAssembly);
}
