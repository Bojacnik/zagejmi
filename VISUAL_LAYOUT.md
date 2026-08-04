# Associate Profile Page - Visual Layout Reference

## Page Structure Overview

```
╔════════════════════════════════════════════════════════════════════════════╗
║                         Associate Profile Page                              ║
╚════════════════════════════════════════════════════════════════════════════╝

┌────────────────────────────────────────────────────────────────────────────┐
│ [← Back]                                                                    │
│                                                                             │
│                          ╭ BANNER IMAGE ╮                                 │
│                          │  (or gradient)                                  │
│                          ╰───────────────╯                                 │
│                                                                             │
└────────────────────────────────────────────────────────────────────────────┘

┌───────────────────────────────────────────────────────────────────────────┐
│  ┌────────────────┐  ┌──────────────────────────────────────────────────┐ │
│  │ PROFILE CARD   │  │                                                  │ │
│  │ (Sticky)       │  │         SERVICES SECTION                        │ │
│  │                │  │                                                  │ │
│  │ ╭────────────╮ │  │  "Services"                                    │ │
│  │ │  Avatar    │ │  │  "Explore the services offered"                │ │
│  │ │ (with dot) │ │  │                                                 │ │
│  │ ╰────────────╯ │  │                                                 │ │
│  │                │  │  ┌────────────────┐  ┌────────────────┐       │ │
│  │ Associate Name │  │  │ Service Card   │  │ Service Card   │       │ │
│  │ ⭐ Associate  │  │  │  + Hover       │  │  + Hover       │       │ │
│  │ 🟢 Online     │  │  │                │  │                │       │ │
│  │                │  │  │ ┌────────────┐│  │ ┌────────────┐│       │ │
│  │ Bio text       │  │  │ │   Image    ││  │ │   Image    ││       │ │
│  │ (optional)     │  │  │ │  (⚡Promo) ││  │ │  (⚡Promo) ││       │ │
│  │                │  │  │ ├────────────┤│  │ ├────────────┤│       │ │
│  │ ┌────┬────┬──┐ │  │  │ │ ⭐ 4.9     ││  │ │ ⭐ 5.0     ││       │ │
│  │ │Rep │Lvl │Co│ │  │  │ │ · 188     ││  │ │ · 142     ││       │ │
│  │ │4.8 │ 5  │47│ │  │  │ │           ││  │ │           ││       │ │
│  │ └────┴────┴──┘ │  │  │ │ Gaming... ││  │ │ Streaming ││       │ │
│  │                │  │  │ │           ││  │ │           ││       │ │
│  │ ┌────────────┐ │  │  │ │ $4.99     ││  │ │ $2.99     ││       │ │
│  │ │   Share    │ │  │  │ │ /game     ││  │ │ /game     ││       │ │
│  │ ├────────────┤ │  │  │ │           ││  │ │           ││       │ │
│  │ │💬 Message  │ │  │  │ │[Chat][Ord]││  │ │[Chat][Ord]││       │ │
│  │ │🔗 Friend   │ │  │  │ └────────────┘  │ └────────────┘│       │ │
│  │ └────────────┘ │  │  │                                 │       │ │
│  │                │  │  │ ┌────────────────┐  ┌────────────────┐   │ │
│  │ Member since   │  │  │ │ Service Card   │  │ Service Card   │   │ │
│  │ January 2024   │  │  │ │                │  │                │   │ │
│  │                │  │  │ │ [Service 3]    │  │ [Service 4]    │   │ │
│  └────────────────┘  │  │ └────────────────┘  └────────────────┘   │ │
│                      │  │                                          │ │
│                      │  └──────────────────────────────────────────┘ │
└───────────────────────────────────────────────────────────────────────┘
```

---

## Component Breakdowns

### Profile Card (Left Sidebar - Sticky)

```
┌─────────────────────────────┐
│   ┏━━━━━━━━━━━━━━━━━━━┓     │
│   ┃  Gradient BG      ┃     │
│   ┃                   ┃     │
│   ┃   ╭───────────╮   ┃     │
│   ┃   │  Avatar   │🟢 ┃     │
│   ┃   │(24x24px)  │   ┃     │
│   ┃   ╰───────────╯   ┃     │
│   ┃                   ┃     │
│   ┗━━━━━━━━━━━━━━━━━━━┛     │
│                             │
│  Associate Name             │
│  ★ Associate   🟢 Online    │
│                             │
│  This is where the bio      │
│  text goes if provided      │
│                             │
├─────────────────────────────┤
│  ┌───────┬───────┬────────┐ │
│  │ 4.8   │   5   │   47   │ │
│  │ Rep   │ Lvl   │ Done   │ │
│  └───────┴───────┴────────┘ │
├─────────────────────────────┤
│ [Share] [Message] [Friend] │
├─────────────────────────────┤
│   Member since Jan 2024     │
└─────────────────────────────┘
```

### Service Card Component

```
┌──────────────────────────┐
│                          │
│  ┌──────────────────────┐│
│  │  Service Image       ││
│  │  (aspect-video)      ││
│  │                      ││
│  │  ⚡ Promo Tag      ││
│  │                      ││
│  └──────────────────────┘│
│                          │
│  Gaming                  │
│                          │
│  Game Coaching Service.. │
│  (line clamp 2)          │
│                          │
│  ⭐ 4.9 · 188 served    │
│                          │
│  Description text that   │
│  appears here and gets   │
│  clamped after 2 lines   │
│                          │
├──────────────────────────┤
│  $4.99 /game             │
│  ⏱️ Avg: 5 mins          │
├──────────────────────────┤
│ [  Chat  ] [ Order ]    │
└──────────────────────────┘
```

---

## Responsive Breakpoints

### 📱 Mobile (< 640px)
```
┌──────────────────┐
│ [← Banner]       │
├──────────────────┤
│ Profile Card     │
│ (full width)     │
├──────────────────┤
│ Services         │
│ (1 column grid)  │
│                  │
│ ┌──────────────┐ │
│ │ Service Card │ │
│ └──────────────┘ │
│ ┌──────────────┐ │
│ │ Service Card │ │
│ └──────────────┘ │
│ ┌──────────────┐ │
│ │ Service Card │ │
│ └──────────────┘ │
└──────────────────┘
```

### 📱 Tablet (640px - 1024px)
```
┌────────────────────────────────┐
│      [← Banner]                │
├────────────────────────────────┤
│                                │
│ Profile   │  Services          │
│ Card      │  (1-2 cols)       │
│ (sticky)  │                    │
│           │ ┌──────┐ ┌──────┐│
│           │ │Card1 │ │Card2 ││
│           │ └──────┘ └──────┘│
│           │ ┌──────┐ ┌──────┐│
│           │ │Card3 │ │Card4 ││
│           │ └──────┘ └──────┘│
│           │                    │
└────────────────────────────────┘
```

### 🖥️ Desktop (> 1024px)
```
┌──────────────────────────────────────────┐
│           [← Banner Full Width]          │
├──────────────────────────────────────────┤
│                                          │
│ Prof │ Services (2 columns)             │
│ Card │                                   │
│      │ ┌─────────────┐  ┌─────────────┐│
│      │ │   Service   │  │   Service   ││
│ (Stk │ │   Card 1    │  │   Card 2    ││
│ y)   │ └─────────────┘  └─────────────┘│
│      │                                   │
│      │ ┌─────────────┐  ┌─────────────┐│
│      │ │   Service   │  │   Service   ││
│      │ │   Card 3    │  │   Card 4    ││
│      │ └─────────────┘  └─────────────┘│
│      │                                   │
└──────────────────────────────────────────┘
```

---

## Color Reference

### Status Indicators
- 🟢 **Online**: `bg-green-500` (bright green)
- ⭐ **Rating**: `text-amber-400` (golden)
- ⚡ **Promo**: `bg-accent-600` (purple accent)
- ❌ **Unavailable**: Overlay `bg-black/60`

### UI Elements
- **Main Buttons**: `bg-accent-600` with `hover:bg-accent-700`
- **Secondary Buttons**: `bg-primary-800` with `hover:bg-primary-700`
- **Card Background**: `bg-primary-950/60` or `bg-primary-900/60`
- **Borders**: `border-accent-700/50` (semi-transparent)

### Text
- **Headers**: `text-text-default` (white/light)
- **Body**: `text-text-muted` (gray)
- **Accents**: `text-accent-300` (light accent)

---

## Animation Effects

### Hover States
```
Service Card:
  Default  → Scale at 1.0
  Hover    → Scale to 1.05 + shadow-accent-700/30

Image in Service:
  Default  → scale-100
  Hover    → scale-105 (image zooms in)

Buttons:
  Default  → bg-primary-800
  Hover    → bg-primary-700 (darker)
```

### Loading State
```
┌──────────────────────────────┐
│                              │
│         ◑ (spinning)         │
│                              │
│  Loading associate profile.. │
│                              │
└──────────────────────────────┘
```

### Error State
```
┌──────────────────────────────┐
│   Unable to Load Profile     │
│                              │
│  Network connection error    │
│  or profile not found        │
│                              │
│  [Back to Associates]        │
└──────────────────────────────┘
```

---

## Typography Hierarchy

```
Page Header (Main):
╔════════════════════╗
║ Associate Profile  ║  ← 4xl, bold
╚════════════════════╝

Section Header:
┌────────────────────┐
│ Services            │  ← 3xl, bold
│ Explore services   │  ← text-muted, smaller
└────────────────────┘

Card Header:
┌────────────────────┐
│ Game Coaching      │  ← lg, bold
│ $4.99 /game        │  ← sm, muted
└────────────────────┘

Body Text:
Lorem ipsum dolor sit   ← sm, muted
amet consectetur       ← line-clamp-2
```

---

## Spacing System

### Typical Distances
- **Page padding**: `p-4 sm:p-6 lg:p-8` (responsive)
- **Section gap**: `gap-8` (large sections)
- **Component gap**: `gap-2` or `gap-4` (internal)
- **Grid gap**: `gap-6` (service cards)
- **Sticky offset**: `top-8` (sidebar)

### Example Layout Spacing
```
Page content
  ↓
max-w-7xl mx-auto  ← centered, max 80rem
  ↓
px-4 sm:px-6 lg:px-8  ← responsive horizontal padding
  ↓
py-8              ← vertical padding
  ↓
grid lg:grid-cols-3 gap-8  ← responsive columns with gap
```

---

## Empty States

### No Services Available
```
┌─────────────────────────────────┐
│                                 │
│        🔍 No services           │
│                                 │
│  This associate hasn't added    │
│  any services yet               │
│                                 │
└─────────────────────────────────┘
```

### Profile Not Found
```
┌─────────────────────────────────┐
│                                 │
│  Unable to Load Profile         │
│                                 │
│  The profile you're looking     │
│  for doesn't exist              │
│                                 │
│  [Back to Associates]           │
│                                 │
└─────────────────────────────────┘
```

---

## Interaction Flow

```
User navigates to /people
  ↓
Sees grid of associates
  ↓
Clicks on card
  ↓
NavLink routes to /associates/{id}
  ↓
ViewAssociateProfile loads
  ↓
API call: GET /api/v1/read/associates/{id}
  ↓
Response received: AssociateProfileDto
  ↓
Page renders with:
  - Profile info (card)
  - Service list (grid)
  ↓
User can:
  - Scroll (profile card is sticky)
  - Read service details
  - Click Chat/Order buttons
  - Click Back button to return
```

---

## Key Features Summary

✅ **Responsive**: Mobile, tablet, desktop layouts  
✅ **Dark Theme**: Anime-inspired aesthetic  
✅ **Sticky Sidebar**: Profile card stays visible  
✅ **Service Cards**: Beautiful, detailed display  
✅ **Loading State**: Spinner while fetching  
✅ **Error Handling**: User-friendly messages  
✅ **Empty State**: Clear message when no services  
✅ **Hover Effects**: Interactive visual feedback  
✅ **Status Indicators**: Online, rating, availability  
✅ **Full Documentation**: Comments & guides  

---

**Ready for Backend Integration! 🚀**

