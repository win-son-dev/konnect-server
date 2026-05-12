using Konnect.Infrastructure.Contracts.Companies;
using Konnect.Infrastructure.Entities;
using Konnect.Infrastructure.Repositories;
using Konnect.Services.Companies.Commands;
using NSubstitute;

namespace Konnect.Tests.Services.Companies.Commands;

/// <summary>
/// Pins the write-side orchestration of <see cref="CompanyCommandService"/>:
/// the recruiter can only edit their own company (target derived from their
/// domain row, never from input), the editable fields are exactly Name /
/// Description / WebsiteUrl, and every error path short-circuits before the
/// repository is asked to UPDATE.
/// </summary>
public class CompanyCommandServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ICompanyRepository _companyRepository = Substitute.For<ICompanyRepository>();
    private readonly CompanyCommandService _service;

    public CompanyCommandServiceTests()
    {
        _service = new CompanyCommandService(_userRepository, _companyRepository);
    }

    [Fact]
    public async Task Should_UpdateNameDescriptionAndWebsite_When_Happy()
    {
        var recruiterId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        _userRepository.GetByIdAsync(recruiterId, Arg.Any<CancellationToken>())
            .Returns(new RecruiterUser { Id = recruiterId, CompanyId = companyId });
        var company = new Company
        {
            Id = companyId,
            Slug = "acme",
            Name = "Old Name",
            Description = "Old description",
            WebsiteUrl = "https://old.test",
        };
        _companyRepository.GetByIdAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(company);

        var input = new UpdateCompanyInput("New Name", "New description", "https://new.test");

        var result = await _service.UpdateByRecruiterIdAsync(recruiterId, input, CancellationToken.None);

        Assert.Same(company, result);
        Assert.Equal("New Name", company.Name);
        Assert.Equal("New description", company.Description);
        Assert.Equal("https://new.test", company.WebsiteUrl);
        Assert.Equal("acme", company.Slug); // slug is not editable here
        await _companyRepository.Received(1).UpdateAsync(company, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ThrowArgumentNullException_When_InputIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.UpdateByRecruiterIdAsync(Guid.NewGuid(), null!, CancellationToken.None));

        await _companyRepository.DidNotReceive().UpdateAsync(
            Arg.Any<Company>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ThrowArgumentException_When_InputNameIsBlank()
    {
        var input = new UpdateCompanyInput("   ", null, null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateByRecruiterIdAsync(Guid.NewGuid(), input, CancellationToken.None));

        await _companyRepository.DidNotReceive().UpdateAsync(
            Arg.Any<Company>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ThrowInvalidOperation_When_RecruiterMissing()
    {
        _userRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var input = new UpdateCompanyInput("New", null, null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateByRecruiterIdAsync(Guid.NewGuid(), input, CancellationToken.None));

        await _companyRepository.DidNotReceive().UpdateAsync(
            Arg.Any<Company>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ThrowInvalidOperation_When_UserIsSeeker()
    {
        var id = Guid.NewGuid();
        _userRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(new JobSeekerUser { Id = id, Email = "seeker@example.test" });

        var input = new UpdateCompanyInput("New", null, null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateByRecruiterIdAsync(id, input, CancellationToken.None));
    }

    [Fact]
    public async Task Should_ThrowInvalidOperation_When_RecruiterReferencesMissingCompany()
    {
        var recruiterId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        _userRepository.GetByIdAsync(recruiterId, Arg.Any<CancellationToken>())
            .Returns(new RecruiterUser { Id = recruiterId, CompanyId = companyId });
        _companyRepository.GetByIdAsync(companyId, Arg.Any<CancellationToken>())
            .Returns((Company?)null);

        var input = new UpdateCompanyInput("New", null, null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateByRecruiterIdAsync(recruiterId, input, CancellationToken.None));
    }
}
