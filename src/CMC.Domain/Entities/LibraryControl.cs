using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CMC.Domain.Entities;

public class LibraryControl
{
	public string Id { get; private set; } = string.Empty;
	public string Name { get; private set; } = string.Empty;
	public string Tag { get; private set; } = string.Empty;
	public decimal CapexEur { get; private set; }
	public decimal OpexYearEur { get; private set; }
	public int IntDays { get; private set; }
	public int ExtDays { get; private set; }
	public IReadOnlyList<string> Deps { get; private set; } = Array.Empty<string>();
	public int TtlDays { get; private set; }
	public string Industry { get; private set; } = string.Empty;

	public DateTimeOffset CreatedAt { get; private set; }
	public DateTimeOffset UpdatedAt { get; private set; }

	private static readonly Regex IdPattern = new(@"^C\d{3}$", RegexOptions.Compiled);

	private LibraryControl() { }

	public LibraryControl(
		string id,
		string name,
		string tag,
		decimal capexEur,
		decimal opexYearEur,
		int intDays,
		int extDays,
		IEnumerable<string>? deps,
		int ttlDays,
		string? industry = null,
		DateTime? createdAtUtc = null)
	{
		if (string.IsNullOrWhiteSpace(id))
			throw new ArgumentException("Id required.", nameof(id));
		if (!IdPattern.IsMatch(id.Trim()))
			throw new ArgumentException("Id must match pattern C### (e.g., C001).", nameof(id));

		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Name required.", nameof(name));
		if (string.IsNullOrWhiteSpace(tag))
			throw new ArgumentException("Tag required.", nameof(tag));
		if (capexEur < 0m)
			throw new ArgumentOutOfRangeException(nameof(capexEur), "Capex must be >= 0.");
		if (opexYearEur < 0m)
			throw new ArgumentOutOfRangeException(nameof(opexYearEur), "Opex per year must be >= 0.");
		if (intDays < 0)
			throw new ArgumentOutOfRangeException(nameof(intDays), "IntDays must be >= 0.");
		if (extDays < 0)
			throw new ArgumentOutOfRangeException(nameof(extDays), "ExtDays must be >= 0.");
		if (ttlDays <= 0)
			throw new ArgumentOutOfRangeException(nameof(ttlDays), "TtlDays must be > 0.");

		Id = id.Trim();
		Name = name.Trim();
		Tag = tag.Trim();
		CapexEur = capexEur;
		OpexYearEur = opexYearEur;
		IntDays = intDays;
		ExtDays = extDays;
		Deps = (deps ?? Enumerable.Empty<string>())
			.Where(s => !string.IsNullOrWhiteSpace(s))
			.Select(s => s.Trim().ToUpperInvariant())
			.Distinct()
			.ToArray();
		TtlDays = ttlDays;
		Industry = (industry ?? string.Empty).Trim();

		CreatedAt = (createdAtUtc?.Kind == DateTimeKind.Utc ? createdAtUtc.Value : createdAtUtc?.ToUniversalTime()) ?? DateTime.UtcNow;
		UpdatedAt = CreatedAt;
	}

	// Mini-API, um Änderungen sauber zu machen und UpdatedAt zu pflegen
	public void UpdateCostsAndEffort(decimal capexEur, decimal opexYearEur, int intDays, int extDays)
	{
		if (capexEur < 0m) throw new ArgumentOutOfRangeException(nameof(capexEur));
		if (opexYearEur < 0m) throw new ArgumentOutOfRangeException(nameof(opexYearEur));
		if (intDays < 0) throw new ArgumentOutOfRangeException(nameof(intDays));
		if (extDays < 0) throw new ArgumentOutOfRangeException(nameof(extDays));

		CapexEur = capexEur;
		OpexYearEur = opexYearEur;
		IntDays = intDays;
		ExtDays = extDays;
		Touch();
	}

	public void SetDeps(IEnumerable<string> deps)
	{
		Deps = (deps ?? Enumerable.Empty<string>())
			.Where(s => !string.IsNullOrWhiteSpace(s))
			.Select(s => s.Trim().ToUpperInvariant())
			.Distinct()
			.ToArray();
		Touch();
	}

	public void Rename(string name)
	{
		if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
		Name = name.Trim();
		Touch();
	}

	public void Retag(string tag)
	{
		if (string.IsNullOrWhiteSpace(tag)) throw new ArgumentException("Tag required.", nameof(tag));
		Tag = tag.Trim();
		Touch();
	}

	public void SetIndustry(string? industry)
	{
		Industry = (industry ?? string.Empty).Trim();
		Touch();
	}

	private void Touch() => UpdatedAt = DateTime.UtcNow;
}
