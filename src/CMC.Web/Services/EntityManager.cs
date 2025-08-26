// src/CMC.Web/Services/EntityManager.cs
using System.Reflection;
using CMC.Web.Shared;

namespace CMC.Web.Services;

/// <summary>
///  entity manager that works with any entity type using reflection and conventions
/// </summary>
public class EntityManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Assembly _contractsAssembly;
    private readonly EditDrawerService _drawerService;
    private readonly DialogService _dialogService;

    public EntityManager(IServiceProvider serviceProvider, EditDrawerService drawerService, DialogService dialogService)
    {
        _serviceProvider = serviceProvider;
        _contractsAssembly = typeof(CMC.Contracts.Users.UserDto).Assembly;
        _drawerService = drawerService;
        _dialogService = dialogService;
    }

    /// <summary>
    /// Generic edit method that works for any entity type
    /// </summary>
    public void StartEdit<TDto>(TDto item, Func<Task> onSuccess, List<ExtraField>? extraFields = null) where TDto : class
    {
        var entityName = GetEntityDisplayName<TDto>();

        var req = new EditDrawerRequest
        {
            Title = $"{entityName} bearbeiten",
            Model = item,
            ContractsAssembly = _contractsAssembly,
            IsCreate = false,
            OnSave = async ctx =>
            {
                try
                {
                    var updateRequest = ctx.Build("Update");
                    await CallServiceMethod<TDto>("UpdateAsync", updateRequest);
                    await onSuccess();
                    _drawerService.Close();
                }
                catch (Exception ex)
                {
                    ShowError("Fehler beim Speichern", ex.Message);
                }
            },
            OnDelete = async ctx =>
            {
                await HandleDelete(item, onSuccess);
            }
        };

        // Add entity-specific extra fields
        if (extraFields != null)
            req.ExtraFields.AddRange(extraFields);
        else
            req.ExtraFields.AddRange(GenerateDefaultExtraFields<TDto>(item));

        _drawerService.Open(req);
    }

    /// <summary>
    /// Generic create method that works for any entity type
    /// </summary>
    public void StartCreate<TDto>(TDto emptyItem, Func<Task> onSuccess, List<ExtraField>? extraFields = null) where TDto : class
    {
        var entityName = GetEntityDisplayName<TDto>();

        var req = new EditDrawerRequest
        {
            Title = $"{entityName} anlegen",
            Model = emptyItem,
            ContractsAssembly = _contractsAssembly,
            IsCreate = true,
            OnSave = async ctx =>
            {
                try
                {
                    var createRequest = ctx.Build("Create");
                    await CallServiceMethod<TDto>("CreateAsync", createRequest);
                    await onSuccess();
                    _drawerService.Close();
                }
                catch (Exception ex)
                {
                    ShowError("Fehler beim Anlegen", ex.Message);
                }
            }
        };

        if (extraFields != null)
            req.ExtraFields.AddRange(extraFields);
        else
            req.ExtraFields.AddRange(GenerateDefaultExtraFields<TDto>(emptyItem, true));

        _drawerService.Open(req);
    }

    /// <summary>
    /// Handles delete with automatic relationship detection
    /// </summary>
    private async Task HandleDelete<TDto>(TDto item, Func<Task> onSuccess) where TDto : class
    {
        var itemId = GetEntityId(item);
        var itemName = GetEntityDisplayName(item);
        var entityTypeName = GetEntityDisplayName<TDto>();

        // Try to get dependent count
        var dependentCount = await GetDependentCount<TDto>(itemId);

        if (dependentCount == 0)
        {
            // Simple delete
            _dialogService.ConfirmDelete(itemName, async () =>
            {
                try
                {
                    await CallServiceMethod<TDto>("DeleteAsync", itemId);
                    await onSuccess();
                    _drawerService.Close();
                }
                catch (Exception ex)
                {
                    ShowError("Löschen fehlgeschlagen", ex.Message);
                }
            }, entityTypeName);
        }
        else
        {
            // Delete with dependencies - use enhanced choice dialog
            _dialogService.ConfirmDeleteWithRelations(itemName, dependentCount, async (strategy) =>
            {
                try
                {
                    switch (strategy)
                    {
                        case "cascade":
                            await CallServiceMethod<TDto>("DeleteCascadeAsync", itemId);
                            break;
                        case "detach":
                            await CallServiceMethod<TDto>("DetachAndDeleteAsync", itemId);
                            break;
                        default:
                            return; // Cancel
                    }
                    await onSuccess();
                    _drawerService.Close();
                }
                catch (Exception ex)
                {
                    ShowError("Löschen fehlgeschlagen", ex.Message);
                }
            }, entityTypeName);
        }
    }

    /// <summary>
    /// Automatically generates extra fields based on conventions
    /// </summary>
    private List<ExtraField> GenerateDefaultExtraFields<TDto>(TDto item, bool isCreate = false) where TDto : class
    {
        var extraFields = new List<ExtraField>();
        var itemType = typeof(TDto);

        // Add password field for user creation
        if (isCreate && itemType.Name.Contains("User"))
        {
            extraFields.Add(new ExtraField(
                Name: "Password",
                Label: "Passwort",
                Type: typeof(string),
                ReadOnly: false,
                Hint: "Mindestens 8 Zeichen",
                DataType: "password"
            ));
        }

        // Auto-detect foreign key relationships
        var foreignKeyProps = itemType.GetProperties()
            .Where(p => p.Name.EndsWith("Id") && p.PropertyType == typeof(Guid?))
            .Where(p => p.Name != "Id"); // Exclude primary key

        foreach (var fkProp in foreignKeyProps)
        {
            var relationName = fkProp.Name.Substring(0, fkProp.Name.Length - 2); // Remove "Id"
            var currentValue = fkProp.GetValue(item);

            extraFields.Add(CreateRelationField(relationName, currentValue));
        }

        return extraFields;
    }

    /// <summary>
    /// Creates a relation field automatically
    /// </summary>
    private ExtraField CreateRelationField(string relationName, object? currentValue)
    {
        return new ExtraField(
            Name: $"{relationName}Id",
            Label: GetDisplayName(relationName),
            Type: typeof(Guid?),
            ReadOnly: false,
            Hint: $"{GetDisplayName(relationName)} suchen oder neu anlegen.",
            DataType: "relation-single",
            Value: currentValue,
            Options: new(),
            OnCreateNew: () => CreateRelatedEntityInline(relationName),
            OnSearch: (term) => SearchRelatedEntities(relationName, term),
            DebounceMs: 300
        );
    }

    /// <summary>
    /// Search for related entities by convention
    /// </summary>
    private async Task<List<KeyValuePair<string, string>>> SearchRelatedEntities(string relationName, string term)
    {
        try
        {
            var serviceType = GetServiceType(relationName);
            if (serviceType == null) return new();

            var service = _serviceProvider.GetRequiredService(serviceType);
            var method = serviceType.GetMethod("GetAllAsync");
            if (method == null) return new();

            var result = method.Invoke(service, new object[] { CancellationToken.None });
            if (result is not Task task) return new();

            await task;

            var items = task.GetType().GetProperty("Result")?.GetValue(task) as System.Collections.IEnumerable;
            if (items == null) return new();

            var filteredItems = new List<KeyValuePair<string, string>>();

            foreach (var item in items)
            {
                var id = GetEntityId(item).ToString();
                var name = GetEntityDisplayName(item);

                if (string.IsNullOrWhiteSpace(term) ||
                    name.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    filteredItems.Add(new KeyValuePair<string, string>(name, id));
                }
            }

            return filteredItems;
        }
        catch
        {
            return new();
        }
    }

    /// <summary>
    /// Create related entity inline
    /// </summary>
    private async Task<KeyValuePair<string, string>?> CreateRelatedEntityInline(string relationName)
    {
        try
        {
            // This would need to be implemented based on your specific entities
            // For now, return null to indicate no inline creation
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets service type by convention
    /// </summary>
    private Type? GetServiceType(string entityName)
    {
        var serviceName = $"CMC.Application.Services.{entityName}Service";
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.FullName == serviceName);
    }

    /// <summary>
    /// Gets service type for a DTO
    /// </summary>
    private Type? GetServiceType<TDto>()
    {
        var dtoName = typeof(TDto).Name;
        if (dtoName.EndsWith("Dto"))
            dtoName = dtoName.Substring(0, dtoName.Length - 3);

        return GetServiceType(dtoName);
    }

    /// <summary>
    /// Calls service method dynamically
    /// </summary>
    private async Task CallServiceMethod<TDto>(string methodName, object parameter)
    {
        var serviceType = GetServiceType<TDto>();
        if (serviceType == null)
            throw new InvalidOperationException($"No service found for {typeof(TDto).Name}");

        var service = _serviceProvider.GetRequiredService(serviceType);
        var method = serviceType.GetMethod(methodName);
        if (method == null)
            throw new InvalidOperationException($"Method {methodName} not found on {serviceType.Name}");

        var result = method.Invoke(service, new[] { parameter });
        if (result is Task task)
            await task;
    }

    /// <summary>
    /// Gets dependent count for an entity
    /// </summary>
    private async Task<int> GetDependentCount<TDto>(Guid entityId)
    {
        try
        {
            var serviceType = GetServiceType<TDto>();
            if (serviceType == null) return 0;

            var service = _serviceProvider.GetRequiredService(serviceType);
            var method = serviceType.GetMethod("GetDependentCountAsync");
            if (method == null) return 0;

            var result = method.Invoke(service, new object[] { entityId });
            if (result is Task<int> task)
                return await task;
        }
        catch
        {
            // Ignore errors, return 0 as default
        }
        return 0;
    }

    /// <summary>
    /// Gets entity display name
    /// </summary>
    private string GetEntityDisplayName<TDto>()
    {
        var typeName = typeof(TDto).Name;
        if (typeName.EndsWith("Dto"))
            typeName = typeName.Substring(0, typeName.Length - 3);

        return typeName switch
        {
            "Customer" => "Kunde",
            "User" => "Benutzer",
            _ => typeName
        };
    }

    /// <summary>
    /// Gets display name for an entity instance
    /// </summary>
    private string GetEntityDisplayName(object entity)
    {
        var nameProps = new[] { "Name", "FirstName", "Email", "Title" };
        foreach (var propName in nameProps)
        {
            var prop = entity.GetType().GetProperty(propName);
            if (prop?.GetValue(entity) is string value && !string.IsNullOrEmpty(value))
            {
                if (propName == "FirstName")
                {
                    var lastNameProp = entity.GetType().GetProperty("LastName");
                    if (lastNameProp?.GetValue(entity) is string lastName && !string.IsNullOrEmpty(lastName))
                        return $"{value} {lastName}";
                }
                return value;
            }
        }

        var id = GetEntityId(entity);
        return $"{GetEntityDisplayName(entity.GetType())} {id}";
    }

    /// <summary>
    /// Gets display name for a string
    /// </summary>
    private string GetDisplayName(string name)
    {
        return name switch
        {
            "Customer" => "Firma",
            "User" => "Benutzer",
            _ => name
        };
    }

    /// <summary>
    /// Gets entity ID using reflection
    /// </summary>
    private Guid GetEntityId(object entity)
    {
        var idProp = entity.GetType().GetProperty("Id");
        if (idProp?.GetValue(entity) is Guid id)
            return id;
        throw new InvalidOperationException($"No Id property found on {entity.GetType().Name}");
    }

    /// <summary>
    /// Gets entity display name from type
    /// </summary>
    private string GetEntityDisplayName(Type type)
    {
        var typeName = type.Name;
        if (typeName.EndsWith("Dto"))
            typeName = typeName.Substring(0, typeName.Length - 3);

        return typeName switch
        {
            "Customer" => "Kunde",
            "User" => "Benutzer",
            _ => typeName
        };
    }

    /// <summary>
    /// Shows error dialog
    /// </summary>
    private void ShowError(string title, string message)
    {
        _dialogService.Open(new DialogRequest
        {
            Title = title,
            Message = message,
            ConfirmText = "OK",
            OnConfirm = () => Task.CompletedTask
        });
    }
}
