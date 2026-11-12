using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ShortLink.Application.Links;
using ShortLink.Domain.Links;

namespace ShortLink.Infrastructure.Persistence.Repositories;

internal sealed class LinkRepository(AppDbContext dbContext) : ILinkRepository
{
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    public Task<Link?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => dbContext.Links.FirstOrDefaultAsync(link => link.Id == id, cancellationToken);

    public Task<Link?> GetBySlugAsync(string slug, CancellationToken cancellationToken) => dbContext.Links.AsNoTracking().FirstOrDefaultAsync(link => link.Slug == slug, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken) => dbContext.Links.AnyAsync(link => link.Slug == slug, cancellationToken);

    public async Task<IReadOnlyList<Link>> ListAsync(Guid ownerId, CancellationToken cancellationToken) => await dbContext.Links
        .AsNoTracking()
        .Where(link => link.OwnerId == ownerId)
        .OrderByDescending(link => link.CreatedAt)
        .ToListAsync(cancellationToken);

    public void Add(Link link) => dbContext.Links.Add(link);

    public void Remove(Link link) => dbContext.Links.Remove(link);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: UniqueIndexViolation or UniqueConstraintViolation } && ex.Entries.Select(entry => entry.Entity).OfType<Link>().FirstOrDefault() is { } link) 
        {
            throw new SlugAlreadyInUseException(link.Slug);
        }
    }
}