using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Web.Shared;

public record ExtraField(
	string Name,
	string Label,
	Type Type,
	bool ReadOnly = false,
	string? Hint = null,
	DataType? DataType = null,
	object? Value = null
);
