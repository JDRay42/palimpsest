using Palimpsest.Domain.Entities;
using Palimpsest.Domain.Enums;

namespace Palimpsest.Application.Interfaces.Repositories;

/// <summary>
/// Repository for managing questionable items (conflicts, identity ambiguities, etc.).
/// </summary>
public interface IQuestionableItemRepository
{
    /// <summary>
    /// Creates a new questionable item.
    /// </summary>
    Task<QuestionableItem> CreateAsync(QuestionableItem item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a questionable item by ID.
    /// </summary>
    Task<QuestionableItem?> GetByIdAsync(Guid itemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all questionable items for a universe, optionally filtered by status.
    /// </summary>
    Task<IEnumerable<QuestionableItem>> GetByUniverseIdAsync(
        Guid universeId,
        QuestionableItemStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing questionable item.
    /// </summary>
    Task UpdateAsync(QuestionableItem item, CancellationToken cancellationToken = default);
}
