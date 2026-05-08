using Konnect.Infrastructure.Entities;
using Konnect.Infrastructure.Repositories;
using Konnect.Infrastructure.Services.Onboarding;
using Konnect.Services.Onboarding;
using NSubstitute;

namespace Konnect.Tests.Services.Onboarding;

/// <summary>
/// Pins the orchestration logic of <see cref="JobSeekerOnboardingService"/>:
/// idempotency on existing seekers, audience mismatch detection, and the
/// happy-path entity shape that gets handed to the repository.
/// </summary>
public class JobSeekerOnboardingServiceTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly JobSeekerOnboardingService service;

    public JobSeekerOnboardingServiceTests()
    {
        service = new JobSeekerOnboardingService(userRepository);
    }

    [Fact]
    public async Task Should_CreateNewSeeker_When_NoExistingUser()
    {
        var externalId = Guid.NewGuid();
        userRepository.GetByIdAsync(externalId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var input = new OnboardJobSeekerInput(
            Headline: "Senior Engineer",
            Location: "Sydney",
            OpenToWork: true);

        var result = await service.OnboardAsync(externalId, "alice@example.test", input, CancellationToken.None);

        var created = Assert.IsType<JobSeekerOnboardingResult.Created>(result);
        Assert.Equal(externalId, created.JobSeeker.Id);
        Assert.Equal("Senior Engineer", created.JobSeeker.Headline);
        Assert.Equal("Sydney", created.JobSeeker.Location);
        Assert.True(created.JobSeeker.OpenToWork);

        await userRepository.Received(1).AddJobSeekerAsync(
            Arg.Is<JobSeekerUser>(seeker => seeker.Id == externalId && seeker.Email == "alice@example.test"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_DefaultEmptyStringsForOptionalFields()
    {
        var externalId = Guid.NewGuid();
        userRepository.GetByIdAsync(externalId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var input = new OnboardJobSeekerInput(Headline: null, Location: null, OpenToWork: false);

        var result = await service.OnboardAsync(externalId, "alice@example.test", input, CancellationToken.None);

        var created = Assert.IsType<JobSeekerOnboardingResult.Created>(result);
        Assert.Equal(string.Empty, created.JobSeeker.Headline);
        Assert.Equal(string.Empty, created.JobSeeker.Location);
        Assert.False(created.JobSeeker.OpenToWork);
    }

    [Fact]
    public async Task Should_ReturnExisting_When_SeekerAlreadyExists()
    {
        var externalId = Guid.NewGuid();
        var existingSeeker = new JobSeekerUser
        {
            Id = externalId,
            Email = "alice@example.test",
            Headline = "Existing Headline",
            Location = "Existing Location",
            OpenToWork = true,
        };
        userRepository.GetByIdAsync(externalId, Arg.Any<CancellationToken>())
            .Returns(existingSeeker);

        var input = new OnboardJobSeekerInput("Different", "Different", false);

        var result = await service.OnboardAsync(externalId, "alice@example.test", input, CancellationToken.None);

        var existing = Assert.IsType<JobSeekerOnboardingResult.Existing>(result);
        Assert.Equal("Existing Headline", existing.JobSeeker.Headline);

        await userRepository.DidNotReceive().AddJobSeekerAsync(
            Arg.Any<JobSeekerUser>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ThrowInvalidOperation_When_UserExistsAsRecruiter()
    {
        var externalId = Guid.NewGuid();
        userRepository.GetByIdAsync(externalId, Arg.Any<CancellationToken>())
            .Returns(new RecruiterUser
            {
                Id = externalId,
                Email = "alice@acme.test",
                CompanyId = Guid.NewGuid(),
                FirstName = "Alice",
                LastName = "Anderson",
                JobTitle = "Recruiter",
            });

        var input = new OnboardJobSeekerInput(null, null, false);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.OnboardAsync(externalId, "alice@acme.test", input, CancellationToken.None));
    }
}
