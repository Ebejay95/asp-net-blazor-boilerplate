using System;
using System.Linq;
using System.Reflection;

namespace CMC.Web.Util;

public static class RequestConventions
{
	public static Type? FindRequestType(object model, string action, Assembly contractsAssembly)
	{
		var modelName = model.GetType().Name; // z.B. UserDto
		// Optional: falls du mit EditFooModel arbeiten würdest:
		if (modelName.StartsWith("Edit", StringComparison.OrdinalIgnoreCase))
			modelName = modelName[4..];
		if (modelName.EndsWith("Model", StringComparison.OrdinalIgnoreCase))
			modelName = modelName[..^5];

		// Übliche DTOs enden auf Dto → für Request-Namen entfernen
		if (modelName.EndsWith("Dto", StringComparison.OrdinalIgnoreCase))
			modelName = modelName[..^3];

		var reqName = $"{action}{modelName}Request";
		return contractsAssembly.GetTypes().FirstOrDefault(x => x.Name.Equals(reqName, StringComparison.Ordinal));
	}
}
