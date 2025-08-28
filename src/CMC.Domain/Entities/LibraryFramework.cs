using System;

namespace CMC.Domain.Entities;

public class LibraryFramework : IVersionedEntity, ISoftDeletable
{
	public Guid Id { get; private set; }                   // reine DB-Id (Guid)
	public string Name { get; private set; } = string.Empty;
	public string Version { get; private set; } = string.Empty; // frei als String
	public string Industry { get; private set; } = string.Empty;

	public DateTimeOffset CreatedAt { get; private set; }
	public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

	private LibraryFramework() { }

	public LibraryFramework(string name, string? version = null, string? industry = null, DateTime? createdAtUtc = null)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Name required.", nameof(name));

		Id = Guid.NewGuid(); // falls du DB-Generated nutzen willst, einfach entfernen
		Name = name.Trim();
		Version = (version ?? string.Empty).Trim();      // kein Zwangsformat
		Industry = (industry ?? string.Empty).Trim();

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

	public void SetIndustry(string? industry)
	{
		Industry = (industry ?? string.Empty).Trim();
		Touch();
	}

	private void Touch() => UpdatedAt = DateTime.UtcNow;
}
