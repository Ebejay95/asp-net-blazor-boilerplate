using System;

namespace CMC.Web.Util;

public interface IValueProvider
{
	bool TryGet(string name, out object? value);
}
