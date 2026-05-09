using System.Security.Cryptography;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Konnect.Tests.WebAPI.Authentication.Fixtures;

/// <summary>
/// Mints RSA-signed JWTs for integration tests. Owns a single RSA keypair
/// for the lifetime of the test fixture: the public key is exposed to the
/// JwtBearer scheme via <c>JwtBearerOptions.Configuration</c> override, and
/// the private key signs every test token. No real Auth0 tenant is touched
/// — the issuer string is fictitious and never resolves.
/// </summary>
public sealed class TestJwtTokenFactory : IDisposable
{
    public const string Issuer = "https://konnect-test.auth0.local/";

    private readonly RSA rsa;
    private readonly SigningCredentials signingCredentials;

    public TestJwtTokenFactory()
    {
        rsa = RSA.Create(2048);
        var signingKey = new RsaSecurityKey(rsa) { KeyId = "konnect-test-key" };
        SigningKey = signingKey;
        signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256);
    }

    public RsaSecurityKey SigningKey { get; }

    public string CreateToken(
        string audience,
        IEnumerable<KeyValuePair<string, object>> additionalClaims,
        DateTimeOffset? notBefore = null,
        DateTimeOffset? expires = null)
    {
        var now = DateTimeOffset.UtcNow;
        var claims = new Dictionary<string, object>(additionalClaims)
        {
            ["iss"] = Issuer,
            ["aud"] = audience,
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = audience,
            NotBefore = (notBefore ?? now.AddMinutes(-1)).UtcDateTime,
            Expires = (expires ?? now.AddMinutes(15)).UtcDateTime,
            SigningCredentials = signingCredentials,
            Claims = claims,
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    public void Dispose() => rsa.Dispose();
}
