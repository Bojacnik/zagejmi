
namespace Zagejmi.Server.Read.Services;

public class SidebarStateService
{
    public bool IsSidebarOpen { get; private set; } = true;

    public event Action? OnChange;

    public void ToggleSidebar()
    {
        IsSidebarOpen = !IsSidebarOpen;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
