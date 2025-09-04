// CMC.Application.Services/SelfManagementService.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using CMC.Application.Ports;
using CMC.Contracts.Customers;
using CMC.Contracts.Users;
using CMC.Domain.Common;
using CMC.Domain.Entities;

namespace CMC.Application.Services;

public class SelfManagementService
{
  private readonly ICustomerRepository _customers;
  private readonly IUserRepository _users;
  private readonly IIndustryRepository _industries; // existiert bereits im Projekt
  private readonly IEmailService _email;            // optional (für Notifications)

  public SelfManagementService(ICustomerRepository customers, IUserRepository users, IIndustryRepository industries, IEmailService email)
  {
    _customers = customers;
    _users = users;
    _industries = industries;
    _email = email;
  }

  /// <summary>
  /// Erstellt ein Unternehmen und verknüpft den aktuellen Benutzer damit (nur wenn er noch keines hat).
  /// </summary>
  public async Task<CustomerDto> CreateCompanyForUserAsync(Guid currentUserId, CreateCustomerRequest request, CancellationToken ct = default)
  {
    var me = await _users.GetByIdAsync(currentUserId, ct) ?? throw new DomainException("User not found");
    if (me.CustomerId.HasValue)
      throw new DomainException("User is already linked to a company");

    // Name prüfen
    var exists = await _customers.ExistsAsync(request.Name, null, ct);
    if (exists) throw new DomainException("Customer with this name already exists");

    // Customer anlegen
    var customer = new Customer(request.Name, request.EmployeeCount, request.RevenuePerYear);

    // optionale Branchen prüfen & setzen
    var ids = (request.IndustryIds ?? Array.Empty<Guid>());
    if (ids.Length > 0)
    {
      var list = await _industries.GetByIdsAsync(ids, ct);
      if (list.Count != ids.Length)
        throw new DomainException("One or more specified industries do not exist");
      customer.SetIndustries(ids);
    }

    await _customers.AddAsync(customer, ct);

    // User verknüpfen
    me.AssignToCustomer(customer);
    await _users.UpdateAsync(me, ct);

    var persisted = await _customers.GetByIdWithUsersAsync(customer.Id, ct) ?? customer;
    // Reuse Mapping aus CustomerService
    return new CustomerDto(
      Id: persisted.Id,
      Name: persisted.Name,
      IndustryIds: persisted.CustomerIndustries?.Select(ci => ci.IndustryId).Distinct().ToArray() ?? Array.Empty<Guid>(),
      IndustryNames: persisted.CustomerIndustries?.Select(ci => ci.Industry?.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Cast<string>().Distinct().ToArray() ?? Array.Empty<string>(),
      EmployeeCount: persisted.EmployeeCount,
      RevenuePerYear: persisted.RevenuePerYear,
      IsActive: persisted.IsActive,
      CreatedAt: persisted.CreatedAt,
      UpdatedAt: persisted.UpdatedAt,
      UserCount: persisted.Users?.Count ?? 0
    );
  }

  /// <summary>Aktualisiert die eigenen User-Stammdaten (ohne CustomerId/EmailConfirmed).</summary>
  public async Task<UserDto> UpdateOwnProfileAsync(Guid currentUserId, UpdateOwnProfileRequest request, CancellationToken ct = default)
  {
    var me = await _users.GetByIdAsync(currentUserId, ct) ?? throw new DomainException("User not found");
    me.UpdatePersonalInfo(request.FirstName, request.LastName, request.Role, request.Department);
    await _users.UpdateAsync(me, ct);

    // Reuse: map über UserService-Logik? Hier minimal:
    return new UserDto(
      me.Id, me.Email, me.FirstName, me.LastName, me.Role, me.Department, me.IsEmailConfirmed,
      me.CreatedAt, me.LastLoginAt, me.CustomerId, null /* Name wird im Client nachgeladen */
    );
  }

  /// <summary>Aktualisiert Einstellungen der eigenen Firma.</summary>
  public async Task<CustomerDto> UpdateOwnCompanyAsync(Guid currentUserId, UpdateCustomerRequest request, CancellationToken ct = default)
  {
    var me = await _users.GetByIdAsync(currentUserId, ct) ?? throw new DomainException("User not found");
    if (!me.CustomerId.HasValue || me.CustomerId.Value != request.Id)
      throw new DomainException("You can only modify your own company");

    var customer = await _customers.GetByIdAsync(request.Id, ct) ?? throw new DomainException("Company not found");

    // Name-Unique prüfen (eigene Id ausschließen)
    if (await _customers.ExistsAsync(request.Name, request.Id, ct))
      throw new DomainException("Customer with this name already exists");

    customer.UpdateBusinessInfo(request.Name, request.EmployeeCount, request.RevenuePerYear);

    // Aktiv-Flag nicht von normalen Usern änderbar -> ignorieren (oder validieren)
    // Industries behandeln (NULL = keine Änderung, leere Liste = alle entfernen)
    if (request.IndustryIds != null)
    {
      var ids = request.IndustryIds.Where(x => x != Guid.Empty).Distinct().ToArray();
      if (ids.Length == 0)
      {
        customer.SetIndustries(Array.Empty<Guid>());
      }
      else
      {
        var list = await _industries.GetByIdsAsync(ids, ct);
        if (list.Count != ids.Length)
          throw new DomainException("One or more specified industries do not exist");
        customer.SetIndustries(ids);
      }
    }

    await _customers.UpdateAsync(customer, ct);

    var persisted = await _customers.GetByIdWithUsersAsync(customer.Id, ct) ?? customer;
    return new CustomerDto(
      persisted.Id, persisted.Name,
      persisted.CustomerIndustries?.Select(ci => ci.IndustryId).Distinct().ToArray() ?? Array.Empty<Guid>(),
      persisted.CustomerIndustries?.Select(ci => ci.Industry?.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Cast<string>().Distinct().ToArray() ?? Array.Empty<string>(),
      persisted.EmployeeCount, persisted.RevenuePerYear, persisted.IsActive, persisted.CreatedAt, persisted.UpdatedAt,
      persisted.Users?.Count ?? 0
    );
  }
}
