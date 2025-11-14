using ReQuantum.Services;

namespace ReQuantum.Modules.Menu.Abstractions;

public interface IMenuItemProvider
{
    MenuItem MenuItem { get; }

    uint Order { get; }
}
