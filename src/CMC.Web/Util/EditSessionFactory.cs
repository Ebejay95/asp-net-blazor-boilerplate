using System.Reflection;

namespace CMC.Web.Services;

public sealed class EditSessionFactory
{
    public EditSession Create(object model, Assembly asm, string action = "Update")
        => new EditSession(model, asm, action);
}
