// Helpers for Components/Layout/MainLayout.razor.
//
// The page itself never scrolls; only the <main> content area does. Browsers reset the window's scroll position on
// navigation but not an inner scroller's, so the layout calls this after each navigation.

export function scrollToTop(element) {
    element?.scrollTo({ top: 0, left: 0, behavior: "instant" });
}
