namespace Zagejmi.Write.Domain.Profile;

/// <summary>
///     Represents the different types of users or personas that can interact with the system. Each person type has
///     distinct characteristics,
///     permissions, and access levels within the application. The person types defined in this enumeration include:
///     - Anonymous: Represents users who have not registered or logged in to the system, typically with limited access to
///     features and functionalities.
///     - Customer: Represents registered users who have their own profiles and access to a wider range of features,
///     including personalized interactions and potential purchasing capabilities.
///     - Associate: Represents users with specific roles or relationships to the system, such as partners, affiliates, or
///     collaborators, who may have access to certain features based on their role and permissions.
///     - Admin: Represents users with elevated privileges and access to manage and oversee the system, including
///     administrative tasks, user account management, and system configuration. Administrators play a crucial role in
///     maintaining the security, integrity, and performance of the application, and they may have access to sensitive
///     information and critical functionalities that are not available to other user types.
/// </summary>
public enum PersonType
{
    /// <summary>
    ///     Represents an anonymous user who has not registered or logged in to the system. This type of user typically has
    ///     limited access to features and functionalities, as they are not associated with a specific account or profile.
    ///     Anonymous users may be able to browse content or perform certain actions, but they do not have personalized
    ///     settings or saved data within the application.
    /// </summary>
    Anonymous,

    /// <summary>
    ///     Represents a customer who has registered and logged in to the system. Customers have their own profiles, which may
    ///     include personal information, preferences, and activity history. They have access to a wider range of features and
    ///     functionalities compared to anonymous users, and they can interact with the application in a more personalized way.
    ///     Customers may also have the ability to make purchases, save content, and receive personalized recommendations based
    ///     on their preferences and activity within the application.
    /// </summary>
    Customer,

    /// <summary>
    ///     Represents an associate who has a specific role or relationship with the system, such as a partner, affiliate, or
    ///     collaborator. Associates may have access to certain features or functionalities that are not available to customers
    ///     or anonymous users, depending on their role and permissions within the application. They may also have their own
    ///     profiles, which can include information relevant to their association with the system, such as partnership details,
    ///     affiliate links, or collaboration history. Associates may interact with the application in a way that supports
    ///     their specific role, such as managing partnerships, tracking affiliate performance, or collaborating on projects
    ///     within the application.
    /// </summary>
    Associate,

    /// <summary>
    ///     Represents an administrator who has elevated privileges and access to manage and oversee the system. Administrators
    ///     typically have the ability to perform administrative tasks, such as managing user accounts, configuring system
    ///     settings, and overseeing the overall operation of the application. They may have access to sensitive information
    ///     and critical functionalities that are not available to other user types, and they play a crucial role in
    ///     maintaining the security, integrity, and performance of the system. Administrators may also be responsible for
    ///     monitoring user activity, handling support requests, and ensuring compliance with policies and regulations within
    ///     the application.
    /// </summary>
    Admin
}