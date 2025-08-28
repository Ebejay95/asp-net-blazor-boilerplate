using System;

namespace CMC.Domain.Entities;

public class LibraryScenario
{
	public Guid Id { get; private set; }
	public string Name { get; private set; } = string.Empty;
	public string Industry { get; private set; } = string.Empty;
	public int AnnualFrequency { get; private set; }
	public decimal ImpactPctRevenue { get; private set; }
	public bool Tags { get; private set; } = true;
	public DateTimeOffset CreatedAt { get; private set; }
	public DateTimeOffset UpdatedAt { get; private set; }

	private LibraryScenario() { }

	public LibraryScenario(string name, string industry, int annualFrequency, decimal impactPctRevenue, bool tags = true)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Name cannot be null or empty.", nameof(name));
		if (string.IsNullOrWhiteSpace(industry))
			throw new ArgumentException("Industry cannot be null or empty.", nameof(industry));
		if (annualFrequency < 0)
			throw new ArgumentOutOfRangeException(nameof(annualFrequency), "AnnualFrequency cannot be negative.");
		if (impactPctRevenue < 0m || impactPctRevenue > 1m)
			throw new ArgumentOutOfRangeException(nameof(impactPctRevenue), "ImpactPctRevenue must be between 0 and 1 (e.g., 0.15 for 15%).");

		Id = Guid.NewGuid();
		Name = name.Trim();
		Industry = industry.Trim();
		AnnualFrequency = annualFrequency;
		ImpactPctRevenue = impactPctRevenue;
		Tags = tags;
		CreatedAt = DateTime.UtcNow;
		UpdatedAt = DateTime.UtcNow;
	}
}
