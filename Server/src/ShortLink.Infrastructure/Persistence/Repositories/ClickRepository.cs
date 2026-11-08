using ShortLink.Application.Clicks;
using ShortLink.Domain.Clicks;

namespace ShortLink.Infrastructure.Persistence.Repositories;

internal sealed class ClickRepository(AppDbContext dbContext) : IClickRepository
{
    public void Add(Click click)
    {
        dbContext.Clicks.Add(click);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
