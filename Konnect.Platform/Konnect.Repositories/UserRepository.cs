using Konnect.Infrastructure.Entities;
using Konnect.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Konnect.Repositories;

public sealed class UserRepository(KonnectDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid externalId, CancellationToken cancellationToken)
        => dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == externalId, cancellationToken);

    public async Task AddJobSeekerAsync(JobSeekerUser jobSeeker, CancellationToken cancellationToken)
    {
        dbContext.JobSeekers.Add(jobSeeker);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
