using System;

namespace Zagejmi.Application.Profiles.Reviews.DeletePlayerReview;

/// <summary>
///     A customer removes their review of a player.
/// </summary>
/// <param name="UserId">The signed-in user.</param>
/// <param name="PlayerProfileId">The reviewed player.</param>
public sealed record DeletePlayerReviewCommand(Guid UserId, Guid PlayerProfileId);
