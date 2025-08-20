using System;
using System.Linq;
using System.Reflection;

namespace CMC.Web.Util;

public static class RequestConventions
{
	public static Type? FindRequestType(object model, string action, Assembly contractsAssembly)
	{
		var modelName = model.GetType().Name;
		if (modelName.StartsWith("Edit", StringComparison.OrdinalIgnoreCase))
			modelName = modelName[4..];
		if (modelName.EndsWith("Model", StringComparison.OrdinalIgnoreCase))
			modelName = modelName[..^5];
		if (modelName.EndsWith("Dto", StringComparison.OrdinalIgnoreCase))
			modelName = modelName[..^3];

		var reqName = $"{action}{modelName}Request";
		return contractsAssembly.GetTypes().FirstOrDefault(x => x.Name.Equals(reqName, StringComparison.Ordinal));
	}
}
