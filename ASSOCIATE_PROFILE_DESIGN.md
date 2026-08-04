# Associate Profile Page Design

## Overview

This document describes the **Associate Profile Page** feature—a comprehensive profile viewing system that allows customers to browse associate profiles, view their services, and interact with them.

## Inspiration

The design is inspired by modern service marketplaces (similar to Twitch service listings), featuring:
- **Full-width banner** at the top with fallback gradient
- **Left sidebar** with sticky profile card containing:
  - Profile avatar with online status indicator
  - Associate name and profile type badge
  - Reputation rating, level, and completed sessions statistics
  - Action buttons (Send Message, Add Friend)
  - Member since information
- **Right content area** displaying a grid of services

## Architecture

### New Files Created

#### 1. **DTOs** (`src/Zagejmi.Contracts/Dtos/`)
- **`ServiceDto.cs`**: Represents a single service offering
  - Includes rating (0-5 scale), served count, pricing, promo tags
  - Supports optional category, image URL, response time
  - Properties for availability and promotional badges
  
- **`AssociateProfileDto.cs`**: Complete public profile view for Associates
  - Contains profile metadata (ID, user ID, display name, avatar)
  - Banner image URL support
  - Optional bio/description
  - Reputation, level, and completed sessions stats
  - Online/offline status indicator
  - Collection of services offered
  - Creation timestamp

#### 2. **Components** (`src/Zagejmi.Read.Api/Components/`)

**`ServiceCard.razor`** - Reusable Service Display Component
- Shows service image with hover scaling effect
- Displays rating (⭐) and served count
- Category badge
- Service description (2-line truncation)
- Price display with suffix options (e.g., "/game", "/15min", "/month")
- Promotional badge (if available)
- Promo badge (⚡ with text)
- Average response time indicator
- Availability status with overlay when unavailable
- Dual action buttons (Chat, Order)
- Disabled state when service is unavailable

**`Pages/ViewAssociateProfile.razor`** - Main Profile Page
- **Route**: `/associates/{ProfileId:guid}`
- **Features**:
  - Full-width banner section with gradient fallback
  - Back button to return to People directory
  - 3-column responsive layout (1 column on mobile, 2 on tablet, 3 on desktop)
  - Sticky profile card on the left
  - Services grid on the right (2 columns on small screens)
  - Loading state with spinner
  - Error boundary with user-friendly error messages
  - Empty state message when no services are available
  - Comprehensive documentation

#### 3. **Updated Pages**

**`Pages/People.razor`** - Enhanced Associates Directory
- Now displays as a proper grid with header section
- Each profile card links to `/associates/{profileId}`
- PersonModel extended with `ProfileId` for navigation
- TODO comment for future API integration
- Better spacing and typography

### Data Models

#### ServiceDto Properties
```csharp
Guid ServiceId                 // Unique identifier
Guid ProfileId                 // Associate offering the service
string Name                    // Service title
string Description             // Service details
string Category                // Service type/category
decimal Rating                 // Rating (0-5 scale)
int Served                      // Times completed/purchased
decimal Price                   // Base price
string? PriceSuffix             // "/game", "/15min", "/month", etc.
string? PromoTag                // "Buy 10 Get 1 Free", etc.
string? ImageUrl                // Service image URL
bool IsAvailable                // Current availability status
string? ResponseTime            // "5 mins", "15 mins", etc.
```

#### AssociateProfileDto Properties
```csharp
Guid ProfileId                  // Profile aggregate ID
Guid UserId                     // Owning user
string DisplayName              // Associate's name
string? ProfileImageUrl         // Avatar image URL
string? BannerImageUrl          // Cover/banner image URL
string? Bio                     // Optional biography
decimal Reputation              // Rating (0-5 scale)
int Level                       // Experience level
int CompletedSessions           // Total completions
IReadOnlyList<ServiceDto> Services  // Offered services
DateTime CreatedAt              // Profile creation date
bool IsOnline                   // Current online status
```

## UI/UX Features

### Responsive Design
- **Mobile** (< 640px): Single column, full-width layout
- **Tablet** (640px - 1024px): Stacked layout
- **Desktop** (> 1024px): 3-column grid with sticky sidebar

### Visual Elements
- **Color Scheme**: Uses existing `primary-900/950`, `accent-600/700` colors
- **Borders**: Semi-transparent accent borders with `border-accent-700/50`
- **Shadows**: Layered shadows with `shadow-2xl` and `shadow-accent-700/30`
- **Animations**:
  - Hover scale on service cards
  - Hover shadow effects
  - Loading spinner animation
  - Smooth transitions on buttons

### Status Indicators
- **Online Badge**: Green circle (🟢) in top-right of avatar
- **Availability**: Service cards show overlay when unavailable
- **Promo Tags**: Purple accent badge with ⚡ emoji
- **Category Tags**: Semi-transparent dark badges

### Interactive Elements
- **Service Cards**: Full clickable area with hover effects
- **Action Buttons**: Chat and Order buttons (currently placeholder implementations)
- **Back Button**: Easy navigation back to People list
- **Add Friend / Send Message**: Placeholder buttons for future implementation

## Future Integration Points

### Backend API Endpoints Required
1. **GET `/api/v1/read/associates/{profileId}`** - Fetch single associate profile
   - Response: `AssociateProfileDto`
   - Should include all services for the associate

2. **GET `/api/v1/read/associates`** - List all associates (for People page)
   - Response: `IReadOnlyList<AssociateProfileDto>`
   - Consider pagination for large datasets

### Commands/Actions to Implement
- Service ordering flow (order button)
- Direct messaging (chat button)
- Friend/follow system (add friend button)
- Service availability management (for associates)

### Data Projection Models
The read side will need projections for:
- Associate profiles with services (denormalized)
- Service offerings with ratings/stats
- Online status tracking

## Styling Notes

### Dark Theme Colors Used
- **Primary Background**: `bg-primary-950` (darkest)
- **Secondary Background**: `bg-primary-900` (light dark)
- **Accent Colors**: `bg-accent-600` (primary accent), `bg-accent-700` (hover)
- **Text**: `text-text-default`, `text-text-muted`
- **Borders**: `border-accent-800/40` to `border-accent-700/50`

### Consistency with Existing Design
- Matches the dark anime-inspired aesthetic of the Bamboobite reference
- Uses same Tailwind spacing (p-4, p-6, etc.)
- Rounded corners: `rounded-lg`, `rounded-xl`, `rounded-2xl`
- Typography: `font-bold`, `font-semibold`, `text-sm`, `text-lg`

## Code Quality

### Documentation
- ✅ All classes documented with `<summary>` tags
- ✅ All public properties documented with inline comments
- ✅ Method documentation for key operations
- ✅ Inline comments explaining complex UI sections

### Best Practices
- ✅ Null-coalescing and null-safe operators
- ✅ Proper use of `@if` guards for loading/error states
- ✅ Accessibility considerations (alt text for images)
- ✅ Performance: Lazy loading indicator, sticky sidebar
- ✅ Error handling with user-friendly messages

## Testing Considerations

### Unit Tests for Components
- ServiceCard rendering with various states (available/unavailable)
- Navigation parameter handling
- Error state rendering
- Loading state animation

### Integration Tests
- API call to fetch associate profile
- Handling missing profile (404)
- Handling API errors (500)
- Service data population

### UI/UX Tests
- Responsive layout on different screen sizes
- Hover effects on service cards
- Sticky sidebar behavior on scroll
- Deep linking to `/associates/{id}`

## Next Steps

1. **Backend Integration**
   - Create read-side projections for AssociateProfileDto
   - Implement API endpoint `/api/v1/read/associates/{profileId}`
   - Add service data to associate profiles

2. **Feature Implementation**
   - Messaging system integration (Chat button)
   - Friend/follow functionality (Add Friend button)
   - Service ordering flow (Order button)

3. **Service Management**
   - Create UI for associates to manage their services
   - Service creation, editing, availability toggling
   - Promo/discount management

4. **Analytics**
   - Track profile views
   - Service popularity metrics
   - Associate statistics tracking

5. **Search & Filtering** (Future Enhancement)
   - Search by service category
   - Filter by rating/reputation
   - Location-based filtering

## File Structure Summary
```
src/
├── Zagejmi.Contracts/Dtos/
│   ├── ServiceDto.cs                          [NEW]
│   └── AssociateProfileDto.cs                 [NEW]
├── Zagejmi.Read.Api/Components/
│   ├── ServiceCard.razor                      [NEW]
│   └── Pages/
│       ├── ViewAssociateProfile.razor         [NEW]
│       └── People.razor                       [UPDATED]
```

## Summary

This design creates a professional, modern associate profile viewing experience that:
- ✅ Matches the Bamboobite-inspired dark aesthetic
- ✅ Provides clear service browsing capabilities
- ✅ Shows associate reputation and statistics
- ✅ Supports future feature integration
- ✅ Is fully responsive across devices
- ✅ Includes comprehensive documentation
- ✅ Follows project conventions and styling

The implementation is ready for backend API integration and further feature development.

