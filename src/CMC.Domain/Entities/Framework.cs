using System;
using System.Collections.Generic;

namespace CMC.Domain.Entities;

public class Framework : IVersionedEntity, ISoftDeletable
{
	public Guid Id { get; private set; }
	public string Name { get; private set; } = string.Empty;
	public string Version { get; private set; } = string.Empty;

	public DateTimeOffset CreatedAt { get; private set; }
	public DateTimeOffset UpdatedAt { get; private set; }

	public bool IsDeleted { get; set; }
	public DateTimeOffset? DeletedAt { get; set; }
	public string? DeletedBy { get; set; }

	public virtual ICollection<LibraryControlFramework> ControlLinks { get; private set; } = new List<LibraryControlFramework>();
	public virtual ICollection<FrameworkIndustry> IndustryLinks { get; private set; } = new List<FrameworkIndustry>();

	private Framework() { }

	public Framework(string name, string? version = null, DateTime? createdAtUtc = null)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Name required.", nameof(name));

		Id = Guid.NewGuid();
		Name = name.Trim();
		Version = (version ?? string.Empty).Trim();

		CreatedAt = (createdAtUtc?.Kind == DateTimeKind.Utc ? createdAtUtc.Value : createdAtUtc?.ToUniversalTime()) ?? DateTime.UtcNow;
		UpdatedAt = CreatedAt;
	}

	public void Rename(string name)
	{
		if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
		Name = name.Trim();
		Touch();
	}

	public void SetVersion(string? version)
	{
		Version = (version ?? string.Empty).Trim();
		Touch();
	}

	private void Touch() => UpdatedAt = DateTime.UtcNow;
}
