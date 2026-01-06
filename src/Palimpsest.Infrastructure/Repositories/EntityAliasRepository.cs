using Microsoft.EntityFrameworkCore;
using Palimpsest.Application.Interfaces.Repositories;
using Palimpsest.Domain.Entities;
using Palimpsest.Infrastructure.Data;

namespace Palimpsest.Infrastructure.Repositories;

/// <summary>
/// Repository for managing entity aliases with fuzzy matching support.
/// </summary>
public class EntityAliasRepository : IEntityAliasRepository
{
    private readonly PalimpsestDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityAliasRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public EntityAliasRepository(PalimpsestDbContext context)
    {
        _context = context;
    }

    public async Task<EntityAlias> CreateAsync(EntityAlias alias, CancellationToken cancellationToken = default)
    {
        _context.EntityAliases.Add(alias);
        await _context.SaveChangesAsync(cancellationToken);
        return alias;
    }

    public async Task<IEnumerable<EntityAlias>> GetByEntityIdAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        return await _context.EntityAliases
            .Where(a => a.EntityId == entityId)
            .OrderByDescending(a => a.Confidence)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<(Entity Entity, float Confidence)>> FindExactMatchAsync(
        string aliasNorm,
        Guid universeId,
        CancellationToken cancellationToken = default)
    {
        var results = await _context.EntityAliases
            .Include(a => a.Entity)
            .Where(a => a.Entity.UniverseId == universeId && a.AliasNorm == aliasNorm)
            .Select(a => new { a.Entity, a.Confidence })
            .Distinct()
            .ToListAsync(cancellationToken);

        return results.Select(r => (r.Entity, 1.0f)); // Exact match = confidence 1.0
    }

    public async Task<IEnumerable<(Entity Entity, float Confidence)>> FindSimilarMatchesAsync(
        string surfaceForm,
        Guid universeId,
        float similarityThreshold = 0.75f,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeAlias(surfaceForm);

        // Use PostgreSQL's similarity function (pg_trgm extension)
        // EF Core 8+ supports this with HasDbFunction or raw SQL
        var results = await _context.EntityAliases
            .FromSqlRaw(@"
                SELECT ea.*
                FROM entity_aliases ea
                INNER JOIN entities e ON ea.entity_id = e.entity_id
                WHERE e.universe_id = {0}
                  AND similarity(ea.alias_norm, {1}) >= {2}
                ORDER BY similarity(ea.alias_norm, {1}) DESC
                LIMIT {3}",
                universeId, normalized, similarityThreshold, maxResults)
            .Include(a => a.Entity)
            .ToListAsync(cancellationToken);

        // Calculate similarity scores
        var scoredResults = results.Select(alias =>
        {
            var similarity = CalculateSimilarity(normalized, alias.AliasNorm);
            return (alias.Entity, similarity * alias.Confidence);
        }).ToList();

        return scoredResults;
    }

    public async Task<bool> AliasExistsAsync(Guid entityId, string aliasNorm, CancellationToken cancellationToken = default)
    {
        return await _context.EntityAliases
            .AnyAsync(a => a.EntityId == entityId && a.AliasNorm == aliasNorm, cancellationToken);
    }

    /// <summary>
    /// Normalizes an alias for matching (lowercase, trim).
    /// </summary>
    private static string NormalizeAlias(string alias)
    {
        return alias.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Calculate similarity using Levenshtein distance as a fallback.
    /// In production, this would use pg_trgm's similarity function.
    /// </summary>
    private static float CalculateSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return 0f;

        if (s1 == s2)
            return 1f;

        // Simple Levenshtein-based similarity
        var distance = LevenshteinDistance(s1, s2);
        var maxLength = Math.Max(s1.Length, s2.Length);
        return 1f - (float)distance / maxLength;
    }

    /// <summary>
    /// Calculate Levenshtein distance between two strings.
    /// </summary>
    private static int LevenshteinDistance(string s1, string s2)
    {
        var d = new int[s1.Length + 1, s2.Length + 1];

        for (var i = 0; i <= s1.Length; i++)
            d[i, 0] = i;

        for (var j = 0; j <= s2.Length; j++)
            d[0, j] = j;

        for (var i = 1; i <= s1.Length; i++)
        {
            for (var j = 1; j <= s2.Length; j++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[s1.Length, s2.Length];
    }

    public async Task UpdateAsync(EntityAlias alias, CancellationToken cancellationToken = default)
    {
        _context.EntityAliases.Update(alias);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid aliasId, CancellationToken cancellationToken = default)
    {
        var alias = await _context.EntityAliases.FindAsync(new object[] { aliasId }, cancellationToken);
        if (alias != null)
        {
            _context.EntityAliases.Remove(alias);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
