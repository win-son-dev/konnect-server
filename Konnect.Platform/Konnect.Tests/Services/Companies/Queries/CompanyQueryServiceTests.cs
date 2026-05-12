using Konnect.Infrastructure.Entities;
using Konnect.Infrastructure.Repositories;
using Konnect.Services.Companies.Queries;
using NSubstitute;

namespace Konnect.Tests.Services.Companies.Queries;

/// <summary>
/// Pins the read-side orchestration of <see cref="CompanyQueryService"/>:
/// public slug lookup is a thin pass-through to the repo (with input
/// validation), and the recruiter-scoped lookup must derive the company id
/// from the recruiter's domain row, never from the caller. Each error path
/// is exercised so a refactor cannot silently downgrade the recruiter-scope
/// guarantees.
/// </summary>
public class CompanyQueryServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ICompanyRepository _companyRepository = Substitute.For<ICompanyRepository>();
    private readonly CompanyQueryService _service;

    public CompanyQueryServiceTests()
    {
        _service = new CompanyQueryService(_userRepository, _companyRepository);
    }

    [Fact]
    public async Task Should_DelegateToRepository_When_GetBySlug()
    {
        var company = new Company { Id = Guid.NewGuid(), Slug = "acme", Name = "Acme" };
        _companyRepository.GetBySlugAsync("acme", Arg.Any<CancellationToken>())
            .Returns(company);

        var result = await _service.GetBySlugAsync("acme", CancellationToken.None);

        Assert.Same(company, result);
    }

    [Fact]
    public async Task Should_ReturnNull_When_GetBySlugMissing()
    {
        _companyRepository.GetBySlugAsync("ghost", Arg.Any<CancellationToken>())
            .Returns((Company?)null);

        var result = await _service.GetBySlugAsync("ghost", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ThrowArgumentException_When_GetBySlugIsBlank()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetBySlugAsync("   ", CancellationToken.None));
    }

    [Fact]
    public async Task Should_ReturnRecruitersCompany_When_GetByRecruiterIdHappy()
    {
        var recruiterId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        _userRepository.GetByIdAsync(recruiterId, Arg.Any<CancellationToken>())
            .Returns(new RecruiterUser { Id = recruiterId, CompanyId = companyId });
        var company = new Company { Id = companyId, Slug = "acme" };
        _companyRepository.GetByIdAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(company);

        var result = await _service.GetByRecruiterIdAsync(recruiterId, CancellationToken.None);

        Assert.Same(company, result);
    }

    [Fact]
    public async Task Should_ThrowInvalidOperation_When_GetByRecruiterIdMissingUser()
    {
        _userRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetByRecruiterIdAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Should_ThrowInvalidOperation_When_GetByRecruiterIdAndUserIsSeeker()
    {
        var id = Guid.NewGuid();
        _userRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(new JobSeekerUser { Id = id, Email = "seeker@example.test" });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetByRecruiterIdAsync(id, CancellationToken.None));
    }

    [Fact]
    public async Task Should_ThrowInvalidOperation_When_GetByRecruiterIdReferencesMissingCompany()
    {
        var recruiterId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        _userRepository.GetByIdAsync(recruiterId, Arg.Any<CancellationToken>())
            .Returns(new RecruiterUser { Id = recruiterId, CompanyId = companyId });
        _companyRepository.GetByIdAsync(companyId, Arg.Any<CancellationToken>())
            .Returns((Company?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetByRecruiterIdAsync(recruiterId, CancellationToken.None));
    }
}
