using System;

namespace Zagejmi.Read.Api.Services;

/// <summary>
///     Service responsible for managing the state of the sidebar in the application. It keeps track of whether the sidebar
///     is open or closed, as well as the currently selected page. The service provides methods to toggle the sidebar's
///     visibility and to set the selected page based on user interactions or changes in the application's URI. It also
///     exposes an event that can be subscribed to by components that need to react to changes in the sidebar state,
///     allowing for a responsive and dynamic user interface. The SidebarStateService is a central point for managing the
///     sidebar's state, ensuring that all components that depend on this state can stay in sync and update accordingly
///     when changes occur. This service is essential for providing a consistent user experience across the application,
///     allowing users to easily navigate between different pages while maintaining the state of the sidebar. By
///     centralizing the management of the sidebar's state, the SidebarStateService helps to reduce code duplication and
///     improve maintainability, as all logic related to the sidebar's state is contained within a single service that can
///     be easily accessed and updated by any component that needs to interact with the sidebar. Overall, the
///     SidebarStateService plays a crucial role in ensuring that the sidebar's state is managed effectively and that the
///     user interface remains responsive and consistent across the application, providing a seamless navigation experience
///     for users as they interact with different pages and features within the application.
/// </summary>
public class SidebarStateService
{
    /// <summary>
    ///     Gets a value indicating whether the sidebar is currently open or closed. The IsSidebarOpen property is used to
    ///     determine the visibility of the sidebar in the user interface, allowing components to react accordingly based on
    ///     its value. When IsSidebarOpen is true, the sidebar should be displayed, and when it is false, the sidebar should be
    ///     hidden. This property is essential for managing the state of the sidebar and ensuring that the user interface
    ///     remains consistent with the user's interactions and navigation within the application. By tracking whether the
    ///     sidebar is open or closed, components can provide a responsive and dynamic user experience, allowing users to
    ///     easily access navigation options while maintaining a clean and organized interface when the sidebar is not needed.
    /// </summary>
    public bool IsSidebarOpen { get; private set; } = true;

    /// <summary>
    ///     Gets the name of the currently selected page in the application. The SelectedPage property is used to track which
    ///     page
    ///     the user is currently viewing, allowing components to update their state and display content accordingly. The value
    ///     of SelectedPage can be set based on user interactions, such as clicking on navigation links or based on changes in
    ///     the application's URI, ensuring that the user interface remains consistent with the user's navigation actions. By
    ///     maintaining the state of the currently selected page, components can provide a more personalized and relevant user
    ///     experience, displaying content and options that are specific to the page the user is currently on. The SelectedPage
    ///     property is essential for managing the navigation state of the application and ensuring that users can easily
    ///     understand where they are within the application, allowing for a seamless and intuitive navigation experience as
    ///     they interact with different pages and features within the application.
    /// </summary>
    public string SelectedPage { get; private set; } = "Home";

    /// <summary>
    ///     An event that is triggered whenever there is a change in the sidebar's state, such as toggling the sidebar's
    ///     visibility
    ///     or changing the selected page. Components that subscribe to the OnChange event can react to these changes by
    ///     updating their own state or re-rendering as necessary, ensuring that the user interface remains consistent with the
    ///     current state of the sidebar. The OnChange event is essential for enabling a responsive and dynamic user
    ///     experience, allowing components to stay in sync with the sidebar's state and providing a seamless navigation
    ///     experience for users as they interact with different pages and features within the application. By subscribing to
    ///     the OnChange event, components can ensure that they are always up-to-date with the latest state of the sidebar,
    ///     allowing for a more cohesive and intuitive user interface that responds effectively to user interactions and
    ///     navigation actions within the application.
    /// </summary>
    public event Action? OnChange;

    /// <summary>
    ///     Toggles the visibility of the sidebar by changing the value of the IsSidebarOpen property. When this method is
    ///     called,
    ///     it will switch the state of the sidebar from open to closed or vice versa, allowing users to show or hide the
    ///     sidebar as needed. After toggling the sidebar's visibility, the method also triggers the OnChange event to notify
    ///     any subscribed components that the state of the sidebar has changed, allowing them to update their own state or
    ///     re-render as necessary to reflect the new state of the sidebar. This method is essential for providing users with
    ///     control over the visibility of the sidebar, allowing them to customize their user interface and access navigation
    ///     options when needed while maintaining a clean and organized interface when the sidebar is not needed. By toggling
    ///     the sidebar's visibility, users can easily access navigation options and other features within the sidebar while
    ///     keeping the main content area focused and uncluttered when the sidebar is not needed, providing a more personalized
    ///     and efficient user experience as they interact with different pages and features within the application.
    /// </summary>
    public void ToggleSidebar()
    {
        this.IsSidebarOpen = !this.IsSidebarOpen;
        this.NotifyStateChanged();
    }

    /// <summary>
    ///     Sets the currently selected page in the application by updating the value of the SelectedPage property. When this
    ///     method is called with a new page name, it will update the SelectedPage property to reflect the new page, allowing
    ///     components to update their state and display content accordingly based on the currently selected page. If the new
    ///     page is different from the current value of SelectedPage, the method will also trigger the OnChange event to notify
    ///     any subscribed components that the selected page has changed, allowing them to update their own state or re-render
    ///     as necessary to reflect the new selected page. This method is essential for managing the navigation state of the
    ///     application and ensuring that users can easily understand where they are within the application, allowing for a
    ///     seamless and intuitive navigation experience as they interact with different pages and features within the
    ///     application. By setting the selected page, users can navigate through the application effectively, with components
    ///     responding appropriately to display relevant content and options based on the current page they are viewing.
    /// </summary>
    /// <param name="page">
    ///     The name of the page to set as the currently selected page. This parameter should be a string that corresponds to
    ///     the name of a page within the application, such as "Home", "Profile", "People", "Wallets", "Login", or "Register".
    ///     The method will update the SelectedPage property to this new value, allowing components to react accordingly based
    ///     on the currently selected page. If the new page is different from the current value of SelectedPage, the method
    ///     will also trigger the OnChange event to notify any subscribed components that the selected page has changed,
    ///     allowing them to update their own state or re-render as necessary to reflect the new selected page. This parameter
    ///     is essential for managing the navigation state of the application and ensuring that users can easily understand
    ///     where they are within the application, allowing for a seamless and intuitive navigation experience as they interact
    ///     with different pages and features within the application.
    /// </param>
    public void SetSelectedPage(string page)
    {
        if (this.SelectedPage == page)
        {
            return;
        }

        this.SelectedPage = page;
        this.NotifyStateChanged();
    }

    /// <summary>
    ///     Sets the currently selected page based on the provided URI. This method parses the URI to determine which page
    ///     should
    ///     be set as the currently selected page, allowing components to update their state and display content accordingly
    ///     based on the user's navigation actions. The method checks the relative path of the URI to determine which page to
    ///     set as the selected page, such as "People", "Wallets", "Profile", "Login", or "Register". If the URI does not match
    ///     any of the known page paths, it defaults to setting the selected page to "Home". Additionally, if the URI
    ///     corresponds to the "Login" or "Register" pages, the method will also check if the sidebar is currently open and
    ///     close it if necessary, ensuring that the user interface remains consistent with the user's navigation actions.
    ///     After updating the selected page and potentially toggling the sidebar's visibility, the method will trigger the
    ///     OnChange event to notify any subscribed components that the state of the sidebar has changed, allowing them to
    ///     update their own state or re-render as necessary to reflect the new selected page and sidebar state. This method is
    ///     essential for managing the navigation state of the application and ensuring that users can easily understand where
    ///     they are within the application, allowing for a seamless and intuitive navigation experience as they interact with
    ///     different pages and features within the application. By setting the selected page based on the URI, users can
    ///     navigate through the application effectively, with components responding appropriately to display relevant content
    ///     and options based on the current page they are viewing, while also maintaining a consistent user interface that
    ///     adapts to their navigation actions.
    /// </summary>
    /// <param name="uri">
    ///     Uri to parse for determining the selected page. This parameter should be a string representing the URI of the
    ///     current page, such as "https://example.com/profile" or "https://example.com/login". The method will parse the URI
    ///     to extract the relative path and determine which page to set as the currently selected page based on the path
    ///     segments. If the URI corresponds to known page paths like "People", "Wallets", "Profile", "Login", or "Register",
    ///     the method will set the SelectedPage property accordingly. If the URI does not match any of the known page paths,
    ///     it will default to setting the selected page to "Home". Additionally, if the URI corresponds to the "Login" or
    ///     "Register" pages, the method will also check if the sidebar is currently open and close it if necessary, ensuring
    ///     that the user interface remains consistent with the user's navigation actions. After updating the selected page and
    ///     potentially toggling the sidebar's visibility, the method will trigger the OnChange event to notify any subscribed
    ///     components that the state of the sidebar has changed, allowing them to update their own state or render as
    ///     necessary to reflect the new selected page and sidebar state. This parameter is essential for managing the
    ///     navigation state of the application and ensuring that users can easily understand where they are within the
    ///     application, allowing for a seamless and intuitive navigation experience as they interact with different pages and
    ///     features within the application. By setting the selected page based on the URI, users can navigate through the
    ///     application effectively, with components responding appropriately to display relevant content and options based on
    ///     the current page they are viewing, while also maintaining a consistent user interface that adapts to their
    ///     navigation actions.
    /// </param>
    public void SetSelectedPageFromUri(string uri)
    {
        string relativePath = new Uri(uri).AbsolutePath.Trim('/');
        string newPage;
        bool needsNotify = false;

        if (relativePath.StartsWith("people", StringComparison.Ordinal))
        {
            newPage = "People";
        }
        else if (relativePath.StartsWith("wallets", StringComparison.Ordinal))
        {
            newPage = "Wallets";
        }
        else if (relativePath.StartsWith("profile", StringComparison.Ordinal))
        {
            newPage = "Profile";
        }
        else if (relativePath.StartsWith("login", StringComparison.Ordinal))
        {
            newPage = "Login";
            if (this.IsSidebarOpen)
            {
                this.IsSidebarOpen = false;
                needsNotify = true;
            }
        }
        else if (relativePath.StartsWith("register", StringComparison.Ordinal))
        {
            newPage = "Register";
            if (this.IsSidebarOpen)
            {
                this.IsSidebarOpen = false;
                needsNotify = true;
            }
        }
        else
        {
            newPage = "Home";
        }

        if (this.SelectedPage != newPage)
        {
            this.SelectedPage = newPage;
            needsNotify = true;
        }

        if (needsNotify)
        {
            this.NotifyStateChanged();
        }
    }

    private void NotifyStateChanged()
    {
        this.OnChange?.Invoke();
    }
}