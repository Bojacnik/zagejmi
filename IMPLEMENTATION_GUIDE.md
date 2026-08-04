# Associate Profile Page - Implementation Guide

## 🎯 What Was Created

### Components
1. **ViewAssociateProfile.razor** (`/associates/{ProfileId:guid}`)
   - Main profile page with banner, profile card, and services grid
   - Fully responsive layout (mobile, tablet, desktop)
   - Sticky profile sidebar
   - Loading & error states
   - Service grid display (2 columns)

2. **ServiceCard.razor** (Reusable component)
   - Beautiful service display card
   - Shows rating, price, availability
   - Promo badges support
   - Chat & Order buttons
   - Hover effects

### Data Models
1. **ServiceDto** - Represents a single service
2. **AssociateProfileDto** - Complete associate profile with services

### UI Updates
1. **People.razor** - Now links to individual associate profiles

---

## 🚀 Quick Start - How to Use

### To View a Profile (Frontend)
1. Navigate to `/people` to see associates directory
2. Click any associate card
3. Browser navigates to `/associates/{profileId}`
4. Profile page loads with banner, stats, and services

### Route Structure
```
URL: /people
     → Click associate → /associates/{guid}
```

---

## 🔧 Backend Integration TODO

### 1. Create API Endpoint
```csharp
[HttpGet("api/v1/read/associates/{profileId:guid}")]
public async Task<ActionResult<AssociateProfileDto>> GetAssociateProfile(Guid profileId)
{
    // Fetch from database/read model
    // Include services collection
    // Return AssociateProfileDto
}
```

### 2. Create Read-Side Projections
- **AssociateProfileProjection**: Denormalized view with services
- **ServiceProjection**: Service details with ratings

### 3. Seed Mock Data (for testing)
```csharp
// Create mock AssociateProfileDto with sample services
var profile = new AssociateProfileDto
{
    ProfileId = Guid.NewGuid(),
    DisplayName = "Associate Name",
    Reputation = 4.8m,
    Level = 5,
    CompletedSessions = 47,
    Services = new List<ServiceDto>
    {
        new ServiceDto
        {
            ServiceId = Guid.NewGuid(),
            Name = "Game Coaching",
            Rating = 4.9m,
            Served = 188,
            Price = 4.99m,
            PriceSuffix = "/game",
            Category = "Gaming",
            IsAvailable = true,
            ResponseTime = "5 mins"
        }
    }
};
```

---

## 📱 Design Features

### Visual Hierarchy
```
┌─────────────────────────────────────────────┐
│  [← Back]             [Banner Image]        │
├──────┬──────────────────────────────────────┤
│      │                                      │
│ Prof │        Services Grid (2 cols)       │
│ Card │     ┌──────────┐  ┌──────────┐      │
│ Stic │     │ Service  │  │ Service  │      │
│ ky   │     │  Card    │  │  Card    │      │
│      │     └──────────┘  └──────────┘      │
│      │     ┌──────────┐  ┌──────────┐      │
│      │     │ Service  │  │ Service  │      │
│      │     │  Card    │  │  Card    │      │
│      │     └──────────┘  └──────────┘      │
└──────┴──────────────────────────────────────┘
```

### Profile Card Contains
- ⭐ Avatar (with online indicator)
- 📝 Display name & badges
- 📊 Stats (Reputation, Level, Completed)
- 💬 Action buttons (Chat, Add Friend)
- 📅 Member since date

### Service Card Contains
- 🖼️ Service image (with hover scale)
- ⭐ Rating & served count
- 💰 Price with suffix
- 🎯 Category badge
- ⚡ Promo tag (if available)
- ⏱️ Response time
- 🔘 Chat & Order buttons

---

## 🎨 Styling References

### Color Scheme (Already in Tailwind config)
- `bg-primary-950` - Darkest background
- `bg-primary-900` - Dark background
- `bg-accent-600` - Primary action button
- `bg-accent-700` - Hover state

### Typography
- Page header: `text-4xl font-bold`
- Section header: `text-3xl font-bold`
- Card titles: `text-lg font-bold`
- Descriptions: `text-sm text-text-muted`

### Spacing
- Page padding: `px-4 sm:px-6 lg:px-8 py-8`
- Card padding: `p-4` or `p-6`
- Gap between cards: `gap-6`

---

## 🧪 Testing Checklist

### Frontend
- [ ] Page loads without errors
- [ ] Responsive layout on mobile/tablet/desktop
- [ ] Sticky sidebar works on scroll
- [ ] Error state displays when profile not found
- [ ] Loading spinner shows while fetching
- [ ] No services message displays when empty
- [ ] Back button navigates to /people

### Backend
- [ ] API returns valid AssociateProfileDto
- [ ] Services array is populated
- [ ] Online status indicator works
- [ ] Stats display correctly
- [ ] Images load (fallback to placeholder)

### Integration
- [ ] Click from People page → Profile page ✓
- [ ] Deep linking to `/associates/{id}` works ✓
- [ ] Profile loading with real API data

---

## 📝 Code Documentation

All files include comprehensive documentation:
- ✅ Class-level summaries
- ✅ Property descriptions
- ✅ Method documentation
- ✅ Inline comments for complex UI sections

### Key Methods

**ViewAssociateProfile.razor:**
- `LoadProfileAsync()` - Fetches profile from API
- `BuildInitials()` - Creates initials from name
- `ExtractErrorMessage()` - Parses API errors
- `CreateReadApiClient()` - HTTP client setup

**ServiceCard.razor:**
- Parameter: `Service` (ServiceDto) - Data to display

---

## 🔄 Data Flow

```
User clicks associate in People.razor
        ↓
Navigates to /associates/{profileId}
        ↓
ViewAssociateProfile.OnInitializedAsync() called
        ↓
LoadProfileAsync() makes GET to API
        ↓
API returns AssociateProfileDto with Services
        ↓
Page renders with profile card + service cards
        ↓
User can view all services, stats, and interact
```

---

## 📦 Dependencies

### Already Installed
- ✅ Blazor (Server-side rendering)
- ✅ Tailwind CSS (Styling)
- ✅ HttpClientFactory (API calls)
- ✅ Configuration (Settings)

### No New NuGet Packages Required

---

## 🎯 Next Features (Future)

1. **Messaging System**
   - Implement "💬 Send Message" button
   - Create chat interface

2. **Service Ordering**
   - Implement "Order" button on service cards
   - Create checkout/confirmation flow

3. **Friend System**
   - Implement "Add Friend" button
   - Show friend status

4. **Reviews & Ratings**
   - Display service reviews
   - Leave review functionality

5. **Search & Filter**
   - Search by service name/category
   - Filter by rating, price, availability

---

## 🐛 Known Limitations

1. **Mock Data**: People.razor still uses mock PersonModel data
   - **Fix**: Replace with API call to get real associates

2. **Action Buttons**: Chat, Order, Add Friend are placeholders
   - **Fix**: Implement actual functionality

3. **No Pagination**: Services don't paginate
   - **Fix**: Add pagination for many services

4. **No Sorting**: Services not sortable
   - **Fix**: Add sort options (rating, price, etc.)

---

## 📞 Support

For questions about the design:
- See `ASSOCIATE_PROFILE_DESIGN.md` for full design document
- Check component comments for implementation details
- Review component parameter descriptions

---

**Status**: ✅ Design complete and ready for backend integration

