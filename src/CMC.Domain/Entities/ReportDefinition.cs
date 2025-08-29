using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;
using System.Collections.Generic;

namespace CMC.Domain.Entities
{
	public class ReportDefinition : ISoftDeletable
	{
		public Guid Id { get; private set; }

		// Referenzen jetzt als Guid (kein Join)
		public Guid CustomerId { get; private set; }

		// Metadaten
		public string Name { get; private set; } = string.Empty;
		public string Kind { get; private set; } = string.Empty;
		public int WindowDays { get; private set; }
		public string Sections { get; private set; } = string.Empty;

		// Audit
		public DateTimeOffset CreatedAt { get; private set; }
		public DateTimeOffset UpdatedAt { get; private set; }

		// Soft delete
		public bool IsDeleted { get; set; }
		public DateTimeOffset? DeletedAt { get; set; }
		public string? DeletedBy { get; set; }

		private ReportDefinition() { }

		public ReportDefinition(
			Guid customerId,
			string name,
			string kind,
			int windowDays,
			string? sections = null,
			DateTimeOffset? createdAtUtc = null)
		{
			if (customerId == Guid.Empty) throw new ArgumentException("CustomerId required.", nameof(customerId));
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
			if (string.IsNullOrWhiteSpace(kind)) throw new ArgumentException("Kind required.", nameof(kind));
			if (windowDays < 0) throw new ArgumentOutOfRangeException(nameof(windowDays));

			Id = Guid.NewGuid();
			CustomerId = customerId;
			Name = name.Trim();
			Kind = kind.Trim();
			WindowDays = windowDays;
			Sections = (sections ?? string.Empty).Trim();

			CreatedAt = createdAtUtc?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
			UpdatedAt = CreatedAt;
		}

		public void Rename(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
			Name = name.Trim();
			Touch();
		}

		public void SetKind(string kind)
		{
			if (string.IsNullOrWhiteSpace(kind)) throw new ArgumentException("Kind required.", nameof(kind));
			Kind = kind.Trim();
			Touch();
		}

		public void SetWindowDays(int windowDays)
		{
			if (windowDays < 0) throw new ArgumentOutOfRangeException(nameof(windowDays));
			WindowDays = windowDays;
			Touch();
		}

		public void SetSections(string? sections)
		{
			Sections = (sections ?? string.Empty).Trim();
			Touch();
		}

		public void ReassignCustomer(Guid customerId)
		{
			if (customerId == Guid.Empty) throw new ArgumentException("CustomerId required.", nameof(customerId));
			CustomerId = customerId;
			Touch();
		}

		public void Delete(string? deletedBy = null)
		{
			if (!IsDeleted)
			{
				IsDeleted = true;
				DeletedAt = DateTimeOffset.UtcNow;
				DeletedBy = deletedBy;
			}
		}

		public void Restore()
		{
			if (IsDeleted)
			{
				IsDeleted = false;
				DeletedAt = null;
				DeletedBy = null;
				Touch();
			}
		}

		private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

		// === Zeitraum-Logik bleibt unverändert ===

		public (DateTimeOffset Start, DateTimeOffset End) CalculatePeriod(DateTimeOffset referenceUtc)
		{
			var refMidnightUtc = new DateTimeOffset(
				referenceUtc.ToUniversalTime().Date,
				TimeSpan.Zero
			);

			switch (Kind.Trim().ToLowerInvariant())
			{
				case "monthly":
					return PreviousCalendarMonth(refMidnightUtc);

				case "weekly":
					return PreviousIsoWeek(refMidnightUtc);

				case "daily":
				{
					var start = refMidnightUtc.AddDays(-1);
					var end = refMidnightUtc.AddTicks(-1);
					return (start, end);
				}

				default:
				{
					var days = WindowDays <= 0 ? 30 : WindowDays;
					var start = refMidnightUtc.AddDays(-days);
					var end = refMidnightUtc.AddTicks(-1);
					return (start, end);
				}
			}
		}

		private static (DateTimeOffset Start, DateTimeOffset End) PreviousCalendarMonth(DateTimeOffset refMidnightUtc)
		{
			var firstThis = new DateTimeOffset(new DateTime(refMidnightUtc.Year, refMidnightUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc));
			var firstPrev = firstThis.AddMonths(-1);
			var end = firstThis.AddTicks(-1);
			return (firstPrev, end);
		}

		private static (DateTimeOffset Start, DateTimeOffset End) PreviousIsoWeek(DateTimeOffset refMidnightUtc)
		{
			// ISO-Woche: Montag als Wochenstart
			int dow = (int)refMidnightUtc.DayOfWeek; // So=0 .. Sa=6
			int daysSinceMonday = (dow + 6) % 7;
			var mondayThisWeek = refMidnightUtc.AddDays(-daysSinceMonday);
			var start = mondayThisWeek.AddDays(-7);
			var end = mondayThisWeek.AddTicks(-1);
			return (start, end);
		}

		public bool IncludesSection(string key)
		{
			if (string.IsNullOrWhiteSpace(key)) return false;
			var map = GetSectionMap();
			return map.TryGetValue(Norm(key), out var enabled) && enabled;
		}

		public IReadOnlyCollection<string> GetSectionKeys()
		{
			var map = GetSectionMap();
			return map.Where(kv => kv.Value).Select(kv => kv.Key).ToArray();
		}

		public Dictionary<string, bool> GetSectionMap()
		{
			var dict = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

			if (string.IsNullOrWhiteSpace(Sections))
				return dict;

			var raw = Sections.Trim();

			if (TryParseJsonObject(raw, out var jsonMap))
				return jsonMap;

			if (TryParseJsonArray(raw, out jsonMap))
				return jsonMap;

			var lineMap = ParseKeyValueLines(raw);
			if (lineMap.Count > 0) return lineMap;

			foreach (var token in TokenizeLoose(raw))
				dict[Norm(token)] = true;

			return dict;
		}

		private static string Norm(string s) => s.Trim().ToLowerInvariant();

		private static bool Truthy(string? s)
		{
			if (string.IsNullOrWhiteSpace(s)) return false;
			switch (s.Trim().ToLowerInvariant())
			{
				case "true": case "yes": case "y": case "1": case "on": return true;
				default: return false;
			}
		}

		private static bool TryParseJsonObject(string raw, out Dictionary<string, bool> map)
		{
			map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
			try
			{
				var node = JsonNode.Parse(raw);
				if (node is JsonObject obj)
				{
					foreach (var kv in obj)
					{
						var k = Norm(kv.Key);
						var v = kv.Value;
						if (v is null) continue;

						bool enabled = v is JsonValue jv && jv.TryGetValue<bool>(out var b)
							? b
							: v.ToJsonString().Trim('"') is var s && Truthy(s);

						map[k] = enabled;
					}
					return true;
				}
			}
			catch { }
			return false;
		}

		private static bool TryParseJsonArray(string raw, out Dictionary<string, bool> map)
		{
			map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
			try
			{
				var node = JsonNode.Parse(raw);
				if (node is JsonArray arr)
				{
					foreach (var v in arr)
					{
						var s = v?.ToJsonString().Trim('"');
						if (!string.IsNullOrWhiteSpace(s))
							map[Norm(s!)] = true;
					}
					return true;
				}
			}
			catch { }
			return false;
		}

		private static Dictionary<string, bool> ParseKeyValueLines(string raw)
		{
			var dict = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
			var lines = raw.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);

			foreach (var line in lines)
			{
				var l = line.Trim();
				if (l.StartsWith("#") || l.StartsWith("//")) continue;

				int sep = l.IndexOf(':');
				if (sep < 0) sep = l.IndexOf('=');
				if (sep < 0) continue;

				var key = Norm(l[..sep]);
				var val = l[(sep + 1)..].Trim();
				if (string.IsNullOrWhiteSpace(key)) continue;

				dict[key] = Truthy(val);
			}
			return dict;
		}

		private static IEnumerable<string> TokenizeLoose(string raw)
		{
			char[] seps = [',', ';', '\t', ' '];
			return raw.Split(seps, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		}
	}
}
