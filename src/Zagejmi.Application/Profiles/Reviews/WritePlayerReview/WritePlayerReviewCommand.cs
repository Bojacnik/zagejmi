using System;

namespace Zagejmi.Application.Profiles.Reviews.WritePlayerReview;

/// <summary>
///     A customer rates a player (1–5 stars, optional text).
/// </summary>
/// <param name="UserId">The signed-in user; the review is written by their customer profile.</param>
/// <param name="PlayerProfileId">The reviewed player.</param>
/// <param name="Rating">1–5 stars.</param>
/// <param name="Comment">The optional text.</param>
public sealed record WritePlayerReviewCommand(Guid UserId, Guid PlayerProfileId, int Rating, string? Comment);
