using Konnect.Infrastructure.Entities;
using Konnect.Infrastructure.Repositories;
using Konnect.Infrastructure.Services.Onboarding;
using Konnect.Services.Onboarding;
using NSubstitute;

namespace Konnect.Tests.Services.Onboarding;

/// <summary>
/// Pins the orchestration logic of <see cref="RecruiterOnboardingService"/>:
/// idempotency on existing recruiters, slug-conflict short-circuit, audience
/// mismatch detection, and the happy-path entity shape that gets handed to
/// the repository.
/// </summary>
public class RecruiterOnboardingServiceTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly ICompanyRepository companyRepository = Substitute.For<ICompanyRepository>();
    private readonly RecruiterOnboardingService service;

    public RecruiterOnboardingServiceTests()
    {
        service = new RecruiterOnboardingService(userRepository, companyRepository);
    }

    [Fact]
    public async Task Should_CreateNewCompanyAndRecruiter_When_NoExistingUser()
    {
        var externalId = Guid.NewGuid();
        userRepository.GetByIdAsync(externalId, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        companyRepository.SlugExistsAsync("acme", Arg.Any<CancellationToken>())
            .Returns(false);

        var input = NewRecruiterInput("acme");

        var result = await service.OnboardAsync(externalId, "alice@acme.test", input, CancellationToken.None);

        var created = Assert.IsType<RecruiterOnboardingResult.Created>(result);
        Assert.Equal(externalId, created.RecruiterId);
        Assert.Equal("acme", created.Company.Slug);
        Assert.Equal("Acme Corp", created.Company.Name);
        Assert.False(created.Company.Verified);

        await companyRepository.Received(1).AddWithFirstRecruiterAsync(
            Arg.Is<Company>(company => company.Slug == "acme" && company.Id != Guid.Empty),
            Arg.Is<RecruiterUser>(recruiter =>
                recruiter.Id == externalId
                && recruiter.Email == "alice@acme.test"
                && recruiter.FirstName == "Alice"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnExisting_When_RecruiterAlreadyExists()
    {
        var externalId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var existingRecruiter = new RecruiterUser
        {
            Id = externalId,
            Email = "alice@acme.test",
            CompanyId = companyId,
            FirstName = "Alice",
            LastName = "Anderson",
            JobTitle = "Recruiter",
        };
        var existingCompany = new Company
        {
            Id = companyId,
            Name = "Acme Corp",
            Slug = "acme",
        };
        userRepository.GetByIdAsync(externalId, Arg.Any<CancellationToken>())
            .Returns(existingRecruiter);
        companyRepository.GetByIdAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(existingCompany);

        var input = NewRecruiterInput("different-slug");

        var result = await service.OnboardAsync(externalId, "alice@acme.test", input, CancellationToken.None);

        var existing = Assert.IsType<RecruiterOnboardingResult.Existing>(result);
        Assert.Equal(externalId, existing.RecruiterId);
        Assert.Equal("acme", existing.Company.Slug);

        await companyRepository.DidNotReceive().AddWithFirstRecruiterAsync(
            Arg.Any<Company>(),
            Arg.Any<RecruiterUser>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnSlugConflict_When_SlugAlreadyTaken()
    {
        var externalId = Guid.NewGuid();
        userRepository.GetByIdAsync(externalId, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        companyRepository.SlugExistsAsync("acme", Arg.Any<CancellationToken>())
            .Returns(true);

        var input = NewRecruiterInput("acme");

        var result = await service.OnboardAsync(externalId, "bob@example.test", input, CancellationToken.None);

        var conflict = Assert.IsType<RecruiterOnboardingResult.SlugConflict>(result);
        Assert.Equal("acme", conflict.Slug);

        await companyRepository.DidNotReceive().AddWithFirstRecruiterAsync(
            Arg.Any<Company>(),
            Arg.Any<RecruiterUser>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ThrowInvalidOperation_When_UserExistsAsJobSeeker()
    {
        var externalId = Guid.NewGuid();
        userRepository.GetByIdAsync(externalId, Arg.Any<CancellationToken>())
            .Returns(new JobSeekerUser { Id = externalId, Email = "alice@example.test" });

        var input = NewRecruiterInput("acme");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.OnboardAsync(externalId, "alice@example.test", input, CancellationToken.None));
    }

    [Fact]
    public async Task Should_ThrowArgumentException_When_CompanyNameIsBlank()
    {
        var input = new OnboardRecruiterInput(
            CompanyName: "   ",
            CompanySlug: "acme",
            CompanyDescription: null,
            CompanyWebsiteUrl: null,
            FirstName: "Alice",
            LastName: "Anderson",
            JobTitle: "Recruiter");

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.OnboardAsync(Guid.NewGuid(), "alice@acme.test", input, CancellationToken.None));
    }

    private static OnboardRecruiterInput NewRecruiterInput(string slug) => new(
        CompanyName: "Acme Corp",
        CompanySlug: slug,
        CompanyDescription: "We make anvils.",
        CompanyWebsiteUrl: "https://acme.test",
        FirstName: "Alice",
        LastName: "Anderson",
        JobTitle: "Senior Recruiter");
}
