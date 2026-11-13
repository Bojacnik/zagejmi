using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Zagejmi.Application.Abstractions;
using Zagejmi.Application.Profiles;
using Zagejmi.Shared;

namespace Zagejmi.Api.Services;

/// <summary>
///     Loads one page of one gender group (or of everyone for <see langword="null" />).
/// </summary>
/// <typeparam name="T">The result item.</typeparam>
/// <param name="group">The gender group, or <see langword="null" /> for everyone.</param>
/// <param name="page">The 1-based page.</param>
/// <param name="pageSize">The page size.</param>
/// <returns>The page and the total count, or a failure.</returns>
public delegate Task<Result<PagedResult<T>>> FetchPage<T>(GenderGroup? group, int page, int pageSize);

/// <summary>
///     Results of a Hráči or Služby search in one of two layouts:
///     <list type="bullet">
///         <item>
///             <description>
///                 Overview (MARKET-002 default): one short preview per gender group (women, men, others), each with
///                 its total count and a link to the full list.
///             </description>
///         </item>
///         <item>
///             <description>
///                 List: one group (or everyone) with full pages and "Načíst další", plus the other groups' counts.
///             </description>
///         </item>
///     </list>
/// </summary>
/// <typeparam name="T">The result item.</typeparam>
public sealed class GenderSections<T>
{
    /// <summary>
    ///     How many items one page of the full list has.
    /// </summary>
    public const int PageSize = 48;

    /// <summary>
    ///     How many items a preview loads: two rows of the widest grid (5 columns). Narrower grids hide the extra
    ///     cards; phones show them all in a swipeable strip.
    /// </summary>
    public const int PreviewSize = 10;

    private static readonly GenderGroup[] SplitOrder = [GenderGroup.Women, GenderGroup.Men, GenderGroup.Others];

    /// <summary>
    ///     Gets a value indicating whether this is the overview of short previews.
    /// </summary>
    public bool IsPreview { get; private set; }

    /// <summary>
    ///     Gets the sections in display order.
    /// </summary>
    public IReadOnlyList<Section> Sections { get; private set; } = [];

    /// <summary>
    ///     Gets a value indicating whether no section has any item.
    /// </summary>
    public bool IsEmpty => this.Sections.All(s => s.Items.Count == 0);

    /// <summary>
    ///     Gets how many items match in each gender group (for the tab counts), whichever layout is loaded.
    /// </summary>
    public IReadOnlyDictionary<GenderGroup, int> Counts { get; private set; } = new Dictionary<GenderGroup, int>();

    /// <summary>
    ///     Gets how many items match in all groups together.
    /// </summary>
    public int TotalCount => this.Counts.Values.Sum();

    /// <summary>
    ///     Loads the overview: a preview of every gender group.
    /// </summary>
    /// <param name="fetch">Loads one page.</param>
    /// <returns>An error message, or <see langword="null" /> on success.</returns>
    public Task<string?> ReloadOverviewAsync(FetchPage<T> fetch)
    {
        return this.ReloadAsync(SplitOrder.Select(g => (GenderGroup?)g).ToArray(), preview: true, fetch);
    }

    /// <summary>
    ///     Loads the full list of one gender group, or of everyone.
    /// </summary>
    /// <param name="group">The gender group, or <see langword="null" /> for everyone.</param>
    /// <param name="fetch">Loads one page.</param>
    /// <returns>An error message, or <see langword="null" /> on success.</returns>
    public async Task<string?> ReloadListAsync(GenderGroup? group, FetchPage<T> fetch)
    {
        if (await this.ReloadAsync([group], preview: false, fetch) is { } error)
        {
            return error;
        }

        // The tabs show every group's count; a one-item page is the cheapest way to get the others.
        Dictionary<GenderGroup, int> counts = [];
        foreach (GenderGroup other in SplitOrder)
        {
            if (other == group)
            {
                counts[other] = this.Sections[0].TotalCount;
                continue;
            }

            Result<PagedResult<T>> result = await fetch(other, 1, 1);
            if (result.IsFailure)
            {
                return result.Error!.Message;
            }

            counts[other] = result.Value.TotalCount;
        }

        this.Counts = counts;
        return null;
    }

    private async Task<string?> ReloadAsync(GenderGroup?[] groups, bool preview, FetchPage<T> fetch)
    {
        List<Section> sections = [];
        foreach (GenderGroup? group in groups)
        {
            Section section = new(group, preview ? PreviewSize : PageSize, preview);
            if (await section.LoadNextAsync(fetch) is { } error)
            {
                this.Sections = [];
                return error;
            }

            sections.Add(section);
        }

        this.IsPreview = preview;
        this.Sections = sections;
        this.Counts = sections
            .Where(s => s.Group is not null)
            .ToDictionary(s => s.Group!.Value, s => s.TotalCount);
        return null;
    }

    /// <summary>
    ///     One gender group (or everyone) with its loaded items.
    /// </summary>
    public sealed class Section
    {
        private readonly int pageSize;
        private readonly bool preview;
        private int loadedPages;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Section" /> class.
        /// </summary>
        /// <param name="group">The gender group, or <see langword="null" /> for everyone.</param>
        /// <param name="pageSize">How many items to load at a time.</param>
        /// <param name="preview">Whether this is a preview that never loads more.</param>
        public Section(GenderGroup? group, int pageSize, bool preview)
        {
            this.Group = group;
            this.pageSize = pageSize;
            this.preview = preview;
        }

        /// <summary>
        ///     Gets the gender group, or <see langword="null" /> for everyone.
        /// </summary>
        public GenderGroup? Group { get; }

        /// <summary>
        ///     Gets the loaded items.
        /// </summary>
        public List<T> Items { get; } = [];

        /// <summary>
        ///     Gets how many items match in total.
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether "Načíst další" can load more (never in a preview).
        /// </summary>
        public bool HasMore => !this.preview && this.Items.Count < this.TotalCount;

        /// <summary>
        ///     Gets a value indicating whether the next page is loading.
        /// </summary>
        public bool IsLoading { get; private set; }

        /// <summary>
        ///     Loads and appends the next page.
        /// </summary>
        /// <param name="fetch">Loads one page.</param>
        /// <returns>An error message, or <see langword="null" /> on success.</returns>
        public async Task<string?> LoadNextAsync(FetchPage<T> fetch)
        {
            this.IsLoading = true;
            try
            {
                Result<PagedResult<T>> result = await fetch(this.Group, this.loadedPages + 1, this.pageSize);
                if (result.IsFailure)
                {
                    return result.Error!.Message;
                }

                this.loadedPages++;
                this.Items.AddRange(result.Value.Items);
                this.TotalCount = result.Value.TotalCount;
                return null;
            }
            finally
            {
                this.IsLoading = false;
            }
        }
    }
}
