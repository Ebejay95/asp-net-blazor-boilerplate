using CMC.Domain.Entities;
using CMC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMC.Infrastructure.Services;

public enum ControlAttachStrategy { ClonePerScenario, AttachFirst }

public sealed class LibraryProvisioningService
{
    private readonly AppDbContext _db;
    private readonly ILogger<LibraryProvisioningService> _log;

    public LibraryProvisioningService(AppDbContext db, ILogger<LibraryProvisioningService> log)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _log = log;
    }

    public sealed record ProvisionResult(
        int ScenariosCreated,
        int ScenariosRestored,
        int ControlsCreated,
        int ControlsRestored,
        int ToDosCreated);

    /// <summary>
    /// Vollständige, idempotente Provisionierung aus Library-Objekten (mit DB-Maps).
    /// </summary>
    public async Task<ProvisionResult> ProvisionAsync(
        Guid customerId,
        IEnumerable<Guid> libraryScenarioIds,
        IEnumerable<Guid> libraryControlIds,
        ControlAttachStrategy attachStrategy = ControlAttachStrategy.ClonePerScenario,
        bool createToDos = true,
        CancellationToken ct = default)
    {
        if (customerId == Guid.Empty) throw new ArgumentException(nameof(customerId));

        var libScenarioSet = (libraryScenarioIds ?? Enumerable.Empty<Guid>()).Where(x => x != Guid.Empty).ToHashSet();
        var libControlSet  = (libraryControlIds  ?? Enumerable.Empty<Guid>()).Where(x => x != Guid.Empty).ToHashSet();

        int scenCreated = 0, scenRestored = 0, ctrlCreated = 0, ctrlRestored = 0, todosCreated = 0;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        // === 1) Library laden ===
        var libScenarios = await _db.LibraryScenarios
            .Include(ls => ls.TagLinks)
            .Where(ls => libScenarioSet.Contains(ls.Id) && !ls.IsDeleted)
            .ToListAsync(ct);

        var libControls = await _db.LibraryControls
            .Include(lc => lc.ScenarioLinks)
            .Where(lc => libControlSet.Contains(lc.Id) && !lc.IsDeleted)
            .ToListAsync(ct);

        // === 2) Szenarien provisionieren (idempotent via Map + Unique-Index) ===
        var mapsScenario = await _db.ProvisionedScenarioMaps
            .Where(m => m.CustomerId == customerId && libScenarioSet.Contains(m.LibraryScenarioId))
            .ToDictionaryAsync(m => m.LibraryScenarioId, m => m, ct);

        var libScenarioIdToScenarioId = new Dictionary<Guid, Guid>();

        foreach (var ls in libScenarios)
        {
            if (mapsScenario.TryGetValue(ls.Id, out var map))
            {
                var s = await _db.Scenarios.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == map.ScenarioId, ct);
                if (s is not null)
                {
                    if (s.IsDeleted)
                    {
                        s.Restore();
                        _db.Update(s);
                        scenRestored++;
                    }
                    libScenarioIdToScenarioId[ls.Id] = s.Id;
                    continue;
                }
                // Map verweist ins Leere -> neu erstellen
            }

            var created = Scenario.FromLibrary(customerId, ls);
            await _db.Scenarios.AddAsync(created, ct);
            await _db.SaveChangesAsync(ct); // Id benötigt für Map

            var newMap = new ProvisionedScenarioMap(customerId, ls.Id, created.Id);
            await _db.ProvisionedScenarioMaps.AddAsync(newMap, ct);
            await _db.SaveChangesAsync(ct);

            libScenarioIdToScenarioId[ls.Id] = created.Id;
            scenCreated++;
        }

        // === 3) Controls provisionieren (idempotent via Map + Unique-Index) ===
        var toDoSeedControls = new List<Control>();

        // Vorhandene Control-Maps für schnelleren Zugriff
        var mapsControl = await _db.ProvisionedControlMaps
            .Where(m => m.CustomerId == customerId && libControlSet.Contains(m.LibraryControlId))
            .ToListAsync(ct);

        foreach (var lc in libControls)
        {
            // alle Library-Szenarien, die dieses Control betreffen:
            var linkedLibScenarioIds = lc.ScenarioLinks.Select(sl => sl.LibraryScenarioId).Distinct().ToArray();

            // Kandidaten an kundenspezifischen Szenario-Ids ableiten (nur, wenn zuvor provisioniert)
            var scenarioIds = linkedLibScenarioIds
                .Select(libId => libScenarioIdToScenarioId.TryGetValue(libId, out var sid) ? sid : Guid.Empty)
                .Where(x => x != Guid.Empty)
                .ToArray();

            if (scenarioIds.Length == 0)
            {
                // Kein Szenario verlinkt oder nicht provisioniert -> attachPolicy:
                if (attachStrategy == ControlAttachStrategy.AttachFirst && libScenarioIdToScenarioId.Values.FirstOrDefault() is Guid attachTo && attachTo != Guid.Empty)
                {
                    scenarioIds = new[] { attachTo };
                }
                else if (attachStrategy == ControlAttachStrategy.ClonePerScenario)
                {
                    // Keine Scenarios -> Control ohne Scenario (kein Map-Eintrag möglich);
                    // Idempotenz wird hier über den Unique-Index an Controls sichergestellt.
                    var existing = await _db.Controls
                        .IgnoreQueryFilters()
                        .Where(c => c.CustomerId == customerId && c.LibraryControlId == lc.Id)
                        .Where(c => !_db.ControlScenarios.Any(cs => cs.ControlId == c.Id))
                        .FirstOrDefaultAsync(ct);

                    if (existing is not null)
                    {
                        if (existing.IsDeleted)
                        {
                            existing.Restore();
                            _db.Update(existing);
                            ctrlRestored++;
                        }
                    }
                    else
                    {
                        var c = Control.FromLibrary(
                            customerId: customerId,
                            lib: lc,
                            implemented: false,
                            coverage: 0m,
                            maturity: 0,
                            evidenceWeight: 0m,
                            freshness: 0m,
                            costTotalEur: lc.OpexYearEur + lc.CapexEur);

                        await _db.Controls.AddAsync(c, ct);
                        toDoSeedControls.Add(c);
                        ctrlCreated++;
                    }
                    continue;
                }
            }

            // Clone-per-Scenario: für jede kundenspezifische Scenario eine Control-Instanz
            foreach (var sid in scenarioIds)
            {
                // Map vorhanden?
                var existingMap = mapsControl.FirstOrDefault(m =>
                    m.CustomerId == customerId &&
                    m.LibraryControlId == lc.Id &&
                    m.ScenarioId == sid);

                if (existingMap is not null)
                {
                    var existing = await _db.Controls.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == existingMap.ControlId, ct);
                    if (existing is not null && existing.IsDeleted)
                    {
                        existing.Restore();
                        _db.Update(existing);
                        ctrlRestored++;
                    }
                    continue;
                }

                // Prüfe aktive Control via Unique-Index-Kombination
                var active = await _db.Controls
                    .Where(c => c.CustomerId == customerId &&
                                c.LibraryControlId == lc.Id &&
                                !c.IsDeleted)
                    .Where(c => _db.ControlScenarios.Any(cs => cs.ControlId == c.Id && cs.ScenarioId == sid))
                    .FirstOrDefaultAsync(ct);

                Control ctrl;
                if (active is not null)
                {
                    ctrl = active;
                }
                else
                {
                    ctrl = Control.FromLibrary(
                        customerId: customerId,
                        lib: lc,
                        implemented: false,
                        coverage: 0m,
                        maturity: 0,
                        evidenceWeight: 0m,
                        freshness: 0m,
                        costTotalEur: lc.OpexYearEur + lc.CapexEur);
                    ctrl.AttachScenario(sid);
                    await _db.Controls.AddAsync(ctrl, ct);
                    toDoSeedControls.Add(ctrl);
                    ctrlCreated++;
                }

                // Map anlegen
                var newMap = new ProvisionedControlMap(customerId, lc.Id, sid, ctrl.Id);
                await _db.ProvisionedControlMaps.AddAsync(newMap, ct);
                await _db.SaveChangesAsync(ct);
            }
        }

        // === 4) ToDos aus den neu erzeugten Controls erstellen ===
        if (createToDos && toDoSeedControls.Count > 0)
        {
            var todos = ToDoFactory.FromControls(toDoSeedControls, startDateUtc: DateTimeOffset.UtcNow);
            await _db.ToDos.AddRangeAsync(todos, ct);
            todosCreated = todos.Count;
        }

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new ProvisionResult(scenCreated, scenRestored, ctrlCreated, ctrlRestored, todosCreated);
    }
}
