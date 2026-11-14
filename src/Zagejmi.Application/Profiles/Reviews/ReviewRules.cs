using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles.Reviews;

/// <summary>
///     Validates what a customer enters into a recommendation or a service review.
/// </summary>
public static class ReviewRules
{
    /// <summary>
    ///     Checks the rating and normalizes the text (trimmed; empty means no text).
    /// </summary>
    /// <param name="rating">The stars.</param>
    /// <param name="comment">The optional text.</param>
    /// <returns>The normalized text, or a validation failure.</returns>
    public static Result<string?> Validate(int rating, string? comment)
    {
        if (rating is < PlayerReview.MinRating or > PlayerReview.MaxRating)
        {
            return Result<string?>.Failure(ErrorDetail.Validation(
                "Review.Rating",
                $"Hodnocení musí být {PlayerReview.MinRating} až {PlayerReview.MaxRating} hvězdiček."));
        }

        string? text = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        if (text is { Length: > PlayerReview.CommentMaxLength })
        {
            return Result<string?>.Failure(ErrorDetail.Validation(
                "Review.Comment",
                $"Text recenze může mít nejvýše {PlayerReview.CommentMaxLength} znaků."));
        }

        return Result<string?>.Success(text);
    }
}
