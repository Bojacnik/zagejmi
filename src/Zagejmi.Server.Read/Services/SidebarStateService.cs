namespace Zagejmi.Server.Read.Services;

public class SidebarStateService
{
    public bool IsSidebarOpen { get; private set; } = true;
    
    public string SelectedPage { get; private set; } = "Home";

    public event Action? OnChange;

    public void ToggleSidebar()
    {
        IsSidebarOpen = !IsSidebarOpen;
        NotifyStateChanged();
    }
    
    public void SetSelectedPage(string page)
    {
        if (SelectedPage != page)
        {
            SelectedPage = page;
            NotifyStateChanged();
        }
    }

    public void SetSelectedPageFromUri(string uri)
    {
        var relativePath = new Uri(uri).AbsolutePath.Trim('/');
        string newPage;
        var needsNotify = false;

        if (relativePath.StartsWith("people"))
        {
            newPage = "People";
        }
        else if (relativePath.StartsWith("wallets"))
        {
            newPage = "Wallets";
        }
        else if (relativePath.StartsWith("profile"))
        {
            newPage = "Profile";
        }
        else if (relativePath.StartsWith("login"))
        {
            newPage = "Login";
            if (IsSidebarOpen)
            {
                IsSidebarOpen = false;
                needsNotify = true;
            }
        }
        else if (relativePath.StartsWith("register"))
        {
            newPage = "Register";
            if (IsSidebarOpen)
            {
                IsSidebarOpen = false;
                needsNotify = true;
            }
        }
        else
        {
            newPage = "Home";
        }
        
        if (SelectedPage != newPage)
        {
            SelectedPage = newPage;
            needsNotify = true;
        }

        if (needsNotify)
        {
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
